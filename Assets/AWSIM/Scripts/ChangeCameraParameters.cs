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
            Vector3 old_pos = thisTransform.position;
            Vector3 new_pos;
            /*CAMERA1_POSITION_X*/ new_pos.x = old_pos.x;
            /*CAMERA1_POSITION_Y*/ new_pos.y = old_pos.y;
            /*CAMERA1_POSITION_Z*/ new_pos.z = old_pos.z;
            Vector3 old_angle = thisTransform.eulerAngles;
            Vector3 new_angle;
            /*CAMERA1_ANGLE_X*/ new_angle.x = old_angle.x;
            /*CAMERA1_ANGLE_Y*/ new_angle.y = old_angle.y;
            /*CAMERA1_ANGLE_Z*/ new_angle.z = old_angle.z;
            thisTransform.position = new_pos;
            thisTransform.eulerAngles = new_angle;

            Camera cam = cameraToCapture1.GetComponent<Camera>();
            float old_focalLength = cam.focalLength;
            float new_focalLength;
            /*CAMERA1_FOCALLENGTH*/ new_focalLength = old_focalLength;
            cam.focalLength = new_focalLength;
        }

        // Change parameters of CameraToCapture2
        {
            GameObject cameraToCapture2=GameObject.Find("CameraToCapture2");
            Transform thisTransform = cameraToCapture2.transform;
            Vector3 old_pos = thisTransform.position;
            Vector3 new_pos;
            /*CAMERA2_POSITION_X*/ new_pos.x = old_pos.x;
            /*CAMERA2_POSITION_Y*/ new_pos.y = old_pos.y;
            /*CAMERA2_POSITION_Z*/ new_pos.z = old_pos.z;
            Vector3 old_angle = thisTransform.eulerAngles;
            Vector3 new_angle;
            /*CAMERA2_ANGLE_X*/ new_angle.x = old_angle.x;
            /*CAMERA2_ANGLE_Y*/ new_angle.y = old_angle.y;
            /*CAMERA2_ANGLE_Z*/ new_angle.z = old_angle.z;
            thisTransform.position = new_pos;
            thisTransform.eulerAngles = new_angle;

            Camera cam = cameraToCapture2.GetComponent<Camera>();
            float old_focalLength = cam.focalLength;
            float new_focalLength;
            /*CAMERA2_FOCALLENGTH*/ new_focalLength = old_focalLength;
            cam.focalLength = new_focalLength;
        }

        // Change parameters of CameraToCapture3
        {
            GameObject cameraToCapture3=GameObject.Find("CameraToCapture3");
            Transform thisTransform = cameraToCapture3.transform;
            Vector3 old_pos = thisTransform.position;
            Vector3 new_pos;
            /*CAMERA3_POSITION_X*/ new_pos.x = old_pos.x;
            /*CAMERA3_POSITION_Y*/ new_pos.y = old_pos.y;
            /*CAMERA3_POSITION_Z*/ new_pos.z = old_pos.z;
            Vector3 old_angle = thisTransform.eulerAngles;
            Vector3 new_angle;
            /*CAMERA3_ANGLE_X*/ new_angle.x = old_angle.x;
            /*CAMERA3_ANGLE_Y*/ new_angle.y = old_angle.y;
            /*CAMERA3_ANGLE_Z*/ new_angle.z = old_angle.z;
            thisTransform.position = new_pos;
            thisTransform.eulerAngles = new_angle;

            Camera cam = cameraToCapture3.GetComponent<Camera>();
            float old_focalLength = cam.focalLength;
            float new_focalLength;
            /*CAMERA3_FOCALLENGTH*/ new_focalLength = old_focalLength;
            cam.focalLength = new_focalLength;
        }
    }

}