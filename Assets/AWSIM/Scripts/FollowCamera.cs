using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AWSIM
{
    /// <summary>
    /// Camera script that follows the target.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [Tooltip("Transform of object to follow")]
        public Transform target;

        [Tooltip("The distance between the camera and the target object")]
        public float Distance = 10.0f;

        [Tooltip("Lateral offset of the camera position")]
        public float Offset = 0.0f;

        [Tooltip("Height of the camera above the target object")]
        public float Height = 5.0f;

        [Tooltip("Camera height multiplier")]
        public float HeightMultiplier = 0.5f;

        private float heightDamping = 2.0f;

        void LateUpdate()
        {
#if UNITY_EDITOR
            if (Time.deltaTime == 0.0f)
                return;
#endif
            if (target == null)
                return;

            // calculate height for camera
            float newHeight = target.position.y + Height;
            float currentCameraHeight = transform.position.y;
            currentCameraHeight = Mathf.Lerp(currentCameraHeight, newHeight, heightDamping * Time.deltaTime);

            // calculate rotation for camera
            float currentRotationAngle = target.eulerAngles.y;
            Quaternion currentCameraRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // set camera position and orientation
            Vector3 pos = target.position;
            pos -= (currentCameraRotation * Vector3.forward * Distance + currentCameraRotation * Vector3.right * Offset);
            pos.y = currentCameraHeight;

            transform.position = pos;
            transform.LookAt(target.position + Vector3.up * Height * HeightMultiplier);
        }
    }
}
