using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCameraParameters: MonoBehaviour {

    void Start()
    {
        // Change parameters of CameraToCapture1
        {
            GameObject cameraToCapture1=GameObject.Find("CameraToCapture1");
            Transform thisTransform = cameraToCapture1.transform;
            Vector3 pos = thisTransform.position;
            /*CAMERA1_POSITION_X*/ pos.x = pos.x;
            /*CAMERA1_POSITION_Y*/ pos.y = pos.y;
            /*CAMERA1_POSITION_Z*/ pos.z = pos.z;
            Vector3 angle = thisTransform.eulerAngles;
            /*CAMERA1_ANGLE_X*/ angle.x = angle.x;
            /*CAMERA1_ANGLE_Y*/ angle.y = angle.y;
            /*CAMERA1_ANGLE_Z*/ angle.z = angle.z;
            thisTransform.position = pos;
            thisTransform.eulerAngles = angle;

            Camera cam = cameraToCapture1.GetComponent<Camera>();
            /*CAMERA1_FOCALLENGTH*/ cam.focalLength = 50;
        }

        // Change parameters of CameraToCapture2
        {
            GameObject cameraToCapture2=GameObject.Find("CameraToCapture2");
            Transform thisTransform = cameraToCapture2.transform;
            Vector3 pos = thisTransform.position;
            /*CAMERA2_POSITION_X*/ pos.x = pos.x;
            /*CAMERA2_POSITION_Y*/ pos.y = pos.y;
            /*CAMERA2_POSITION_Z*/ pos.z = pos.z;
            Vector3 angle = thisTransform.eulerAngles;
            /*CAMERA2_ANGLE_X*/ angle.x = angle.x;
            /*CAMERA2_ANGLE_Y*/ angle.y = angle.y;
            /*CAMERA2_ANGLE_Z*/ angle.z = angle.z;
            thisTransform.position = pos;
            thisTransform.eulerAngles = angle;

            Camera cam = cameraToCapture2.GetComponent<Camera>();
            /*CAMERA2_FOCALLENGTH*/ cam.focalLength = 50;
        }

        // Change parameters of CameraToCapture3
        {
            GameObject cameraToCapture3=GameObject.Find("CameraToCapture3");
            Transform thisTransform = cameraToCapture3.transform;
            Vector3 pos = thisTransform.position;
            /*CAMERA3_POSITION_X*/ pos.x = pos.x;
            /*CAMERA3_POSITION_Y*/ pos.y = pos.y;
            /*CAMERA3_POSITION_Z*/ pos.z = pos.z;
            Vector3 angle = thisTransform.eulerAngles;
            /*CAMERA3_ANGLE_X*/ angle.x = angle.x;
            /*CAMERA3_ANGLE_Y*/ angle.y = angle.y;
            /*CAMERA3_ANGLE_Z*/ angle.z = angle.z;
            thisTransform.position = pos;
            thisTransform.eulerAngles = angle;

            Camera cam = cameraToCapture3.GetComponent<Camera>();
            /*CAMERA3_FOCALLENGTH*/cam.focalLength = 50;
        }
    }

}