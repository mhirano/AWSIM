// Copyright 2022 Robotec.ai.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RGLUnityPlugin
{
    /// <summary>
    /// Encapsulates all non-ROS components of a RGL-based Lidar.
    /// </summary>
    public class LidarSensor : MonoBehaviour
    {
        /// <summary>
        /// Sensor processing and callbacks are automatically called in this hz.
        /// </summary>
        [FormerlySerializedAs("OutputHz")]
        [Range(0, 50)] public int AutomaticCaptureHz = 10;

        /// <summary>
        /// Delegate used in callbacks.
        /// </summary>
        public delegate void OnNewDataDelegate();

        /// <summary>
        /// Called when new data is generated via automatic capture.
        /// </summary>
        public OnNewDataDelegate onNewData;

        /// <summary>
        /// Called when lidar model configuration has changed.
        /// </summary>
        public OnNewDataDelegate onLidarModelChange;

        /// <summary>
        /// Allows to select one of built-in LiDAR models.
        /// Defaults to a range meter to ensure the choice is conscious.
        /// </summary>
        public LidarModel modelPreset = LidarModel.RangeMeter;

        /// <summary>
        /// Allows to quickly enable/disable distance gaussian noise.
        /// </summary>
        public bool applyDistanceGaussianNoise = true;

        /// <summary>
        /// Allows to quickly enable/disable angular gaussian noise.
        /// </summary>
        public bool applyAngularGaussianNoise = true;

        /// <summary>
        /// Allows to quickly enable/disable velocity distortion
        /// </summary>
        public bool applyVelocityDistortion = false;

        /// <summary>
        /// Encapsulates description of a point cloud generated by a LiDAR and allows for fine-tuning.
        /// </summary>
        // It is safer to refer to concrete property instead of using dictionary here because of static initialization order.
        public LidarConfiguration configuration = LidarConfigurationLibrary.RangeMeter;

        private RGLNodeSequence rglGraphLidar;
        private RGLNodeSequence rglSubgraphCompact;
        private RGLNodeSequence rglSubgraphToLidarFrame;
        private SceneManager sceneManager;

        private const string lidarRaysNodeId = "LIDAR_RAYS";
        private const string lidarRangeNodeId = "LIDAR_RANGE";
        private const string lidarRingsNodeId = "LIDAR_RINGS";
        private const string lidarTimeOffsetsNodeId = "LIDAR_OFFSETS";
        private const string lidarPoseNodeId = "LIDAR_POSE";
        private const string noiseLidarRayNodeId = "NOISE_LIDAR_RAY";
        private const string lidarRaytraceNodeId = "LIDAR_RAYTRACE";
        private const string noiseHitpointNodeId = "NOISE_HITPOINT";
        private const string noiseDistanceNodeId = "NOISE_DISTANCE";
        private const string pointsCompactNodeId = "POINTS_COMPACT";
        private const string toLidarFrameNodeId = "TO_LIDAR_FRAME";

        private LidarModel? validatedPreset;
        private float timer;

        private Matrix4x4 lastTransform;
        private Matrix4x4 currentTransform;

        private int fixedUpdatesInCurrentFrame = 0;
        private int lastUpdateFrame = -1;

        private static List<LidarSensor> activeSensors = new List<LidarSensor>();

        public void Awake()
        {
            rglGraphLidar = new RGLNodeSequence()
                .AddNodeRaysFromMat3x4f(lidarRaysNodeId, new Matrix4x4[1] {Matrix4x4.identity})
                .AddNodeRaysSetRange(lidarRangeNodeId, new Vector2[1] {new Vector2(0.0f, Mathf.Infinity)})
                .AddNodeRaysSetRingIds(lidarRingsNodeId, new int[1] {0})
                .AddNodeRaysSetTimeOffsets(lidarTimeOffsetsNodeId, new float[1] {0})
                .AddNodeRaysTransform(lidarPoseNodeId, Matrix4x4.identity)
                .AddNodeGaussianNoiseAngularRay(noiseLidarRayNodeId, 0, 0)
                .AddNodeRaytrace(lidarRaytraceNodeId)
                .AddNodeGaussianNoiseAngularHitpoint(noiseHitpointNodeId, 0, 0)
                .AddNodeGaussianNoiseDistance(noiseDistanceNodeId, 0, 0, 0);

            rglSubgraphCompact = new RGLNodeSequence()
                .AddNodePointsCompact(pointsCompactNodeId);

            rglSubgraphToLidarFrame = new RGLNodeSequence()
                .AddNodePointsTransform(toLidarFrameNodeId, Matrix4x4.identity);

            RGLNodeSequence.Connect(rglGraphLidar, rglSubgraphCompact);
            RGLNodeSequence.Connect(rglSubgraphCompact, rglSubgraphToLidarFrame);
        }

        public void Start()
        {
            sceneManager = FindObjectOfType<SceneManager>();
            if (sceneManager == null)
            {
                // TODO(prybicki): this is too tedious, implement automatic instantiation of RGL Scene Manager
                Debug.LogError($"RGL Scene Manager is not present on the scene. Destroying {name}.");
                Destroy(this);
                return;
            }
            OnValidate();

            // Apply initial transform of the sensor.
            lastTransform = gameObject.transform.localToWorldMatrix;
        }

        public void OnValidate()
        {
            // This tricky code ensures that configuring from a preset dropdown
            // in Unity Inspector works well in prefab edit mode and regular edit mode. 
            bool presetChanged = validatedPreset != modelPreset;
            bool firstValidation = validatedPreset == null;
            if (!firstValidation && presetChanged)
            {
                configuration = LidarConfigurationLibrary.ByModel[modelPreset];
            }
            ApplyConfiguration(configuration);
            validatedPreset = modelPreset;
        }

        private void ApplyConfiguration(LidarConfiguration newConfig)
        {
            if (rglGraphLidar == null)
            {
                return;
            }

            onLidarModelChange?.Invoke();

            rglGraphLidar.UpdateNodeRaysFromMat3x4f(lidarRaysNodeId, newConfig.GetRayPoses())
                         .UpdateNodeRaysSetRange(lidarRangeNodeId, newConfig.GetRayRanges())
                         .UpdateNodeRaysSetRingIds(lidarRingsNodeId, newConfig.laserArray.GetLaserRingIds())
                         .UpdateNodeRaysTimeOffsets(lidarTimeOffsetsNodeId, newConfig.GetRayTimeOffsets())
                         .UpdateNodeGaussianNoiseAngularRay(noiseLidarRayNodeId,
                             newConfig.noiseParams.angularNoiseMean * Mathf.Deg2Rad,
                             newConfig.noiseParams.angularNoiseStDev * Mathf.Deg2Rad)
                         .UpdateNodeGaussianNoiseAngularHitpoint(noiseHitpointNodeId,
                             newConfig.noiseParams.angularNoiseMean * Mathf.Deg2Rad,
                             newConfig.noiseParams.angularNoiseStDev * Mathf.Deg2Rad)
                         .UpdateNodeGaussianNoiseDistance(noiseDistanceNodeId, newConfig.noiseParams.distanceNoiseMean,
                             newConfig.noiseParams.distanceNoiseStDevBase, newConfig.noiseParams.distanceNoiseStDevRisePerMeter);

            rglGraphLidar.SetActive(noiseDistanceNodeId, applyDistanceGaussianNoise);
            var angularNoiseType = newConfig.noiseParams.angularNoiseType;
            rglGraphLidar.SetActive(noiseLidarRayNodeId, applyAngularGaussianNoise && angularNoiseType == AngularNoiseType.RayBased);
            rglGraphLidar.SetActive(noiseHitpointNodeId, applyAngularGaussianNoise && angularNoiseType == AngularNoiseType.HitpointBased);

            // If distortion is disabled, update raytrace node with no velocities provided (it disables distortion in native RGL library)
            if (!applyVelocityDistortion)
            {
                rglGraphLidar.UpdateNodeRaytrace(lidarRaytraceNodeId);
            }
        }

        public void OnEnable()
        {
            activeSensors.Add(this);
        }

        public void OnDisable()
        {
            activeSensors.Remove(this);
        }

        public void FixedUpdate()
        {
            // One LidarSensor triggers FixedUpdateLogic for all of active LidarSensors on the scene
            // This is an optimization to take full advantage of asynchronous RGL graph execution
            // First, all RGL graphs are run which enqueue the most priority graph branches (e.g., visualization for Unity) properly
            // Then, `onNewData` delegate is called to notify other components about new data available
            // This way, the most important (Unity blocking) computations for all of the sensors are performed first
            // Non-blocking operations (e.g., ROS2 publishing) are performed next
            if (activeSensors[0] != this)
            {
                return;
            }

            var triggeredSensorsIndexes = new List<int>();
            for (var i = 0; i < activeSensors.Count; i++)
            {
                if (activeSensors[i].FixedUpdateLogic())
                {
                    triggeredSensorsIndexes.Add(i);
                }
            }

            foreach (var idx in triggeredSensorsIndexes)
            {
                activeSensors[idx].NotifyNewData();
            }
        }

        /// <summary>
        /// Performs fixed update logic.
        /// Returns true if sensor was triggered (raytracing was performed)
        /// </summary>
        private bool FixedUpdateLogic()
        {
            if (lastUpdateFrame != Time.frameCount)
            {
                fixedUpdatesInCurrentFrame = 0;
                lastUpdateFrame = Time.frameCount;
            }
            fixedUpdatesInCurrentFrame += 1;

            if (AutomaticCaptureHz == 0.0f)
            {
                return false;
            }

            timer += Time.deltaTime;

            // Update last known transform of lidar.
            UpdateTransforms();

            var interval = 1.0f / AutomaticCaptureHz;
            if (timer + 0.00001f < interval)
                return false;

            timer = 0;

            Capture();
            return true;
        }

        private void NotifyNewData()
        {
            onNewData?.Invoke();
        }

        /// <summary>
        /// Connect to point cloud in world coordinate frame.
        /// </summary>
        public void ConnectToWorldFrame(RGLNodeSequence nodeSequence, bool compacted = true)
        {
            if (compacted)
            {
                RGLNodeSequence.Connect(rglSubgraphCompact, nodeSequence);
            }
            else
            {
                RGLNodeSequence.Connect(rglGraphLidar, nodeSequence);
            }
        }

        /// <summary>
        /// Connect to compacted point cloud in lidar coordinate frame.
        /// </summary>
        public void ConnectToLidarFrame(RGLNodeSequence nodeSequence)
        {
            RGLNodeSequence.Connect(rglSubgraphToLidarFrame, nodeSequence);
        }

        public void Capture()
        {
            sceneManager.DoUpdate(fixedUpdatesInCurrentFrame);

            // Set lidar pose
            Matrix4x4 lidarPose = gameObject.transform.localToWorldMatrix * configuration.GetLidarOriginTransfrom();
            rglGraphLidar.UpdateNodeRaysTransform(lidarPoseNodeId, lidarPose);
            rglSubgraphToLidarFrame.UpdateNodePointsTransform(toLidarFrameNodeId, lidarPose.inverse);

            // Set lidar velocity
            if (applyVelocityDistortion)
            {
                SetVelocityToRaytrace();
            }

            rglGraphLidar.Run();
        }

        private void UpdateTransforms()
        {
            lastTransform = currentTransform;
            currentTransform = gameObject.transform.localToWorldMatrix;
        }

        private void SetVelocityToRaytrace()
        {
            // Calculate delta transform of lidar.
            // Velocities must be in sensor-local coordinate frame.
            // Sensor linear velocity in m/s.
            Vector3 globalLinearVelocity = (currentTransform.GetColumn(3) - lastTransform.GetColumn(3)) / Time.deltaTime;
            Vector3 localLinearVelocity = gameObject.transform.InverseTransformDirection(globalLinearVelocity);

            Vector3 deltaRotation = Quaternion.LookRotation(currentTransform.GetColumn(2), currentTransform.GetColumn(1)).eulerAngles
                                  - Quaternion.LookRotation(lastTransform.GetColumn(2), lastTransform.GetColumn(1)).eulerAngles;
            // Fix delta rotation when switching between 0 and 360.
            deltaRotation = new Vector3(Mathf.DeltaAngle(0, deltaRotation.x), Mathf.DeltaAngle(0, deltaRotation.y), Mathf.DeltaAngle(0, deltaRotation.z));
            // Sensor angular velocity in rad/s.
            Vector3 localAngularVelocity = (deltaRotation * Mathf.Deg2Rad) / Time.deltaTime;

            rglGraphLidar.UpdateNodeRaytrace(lidarRaytraceNodeId, localLinearVelocity, localAngularVelocity, true);
        }
    }
}
