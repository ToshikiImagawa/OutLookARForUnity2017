/**
* 
*  You can not modify and use this source freely
*  only for the development of application related OutLookAR.
* 
* (c) OutLookAR All rights reserved.
* by Toshiki Imagawa
**/
using UnityEngine;
using OpenCVForUnity;
using System.Collections.Generic;

namespace OutLookAR.Test
{
    public class DetailedTrackingTest : MonoBehaviour
    {

        public void UpdateAttitude(Mat mat)
        {
            TrackerManager.Instance.Tracking(mat);
        }
        public void UpdateScreen(Texture2D tex)
        {
            ARCameraManager.Instance.UpdateScreenTexture(tex);
        }
    }
}