/**
 * 
 *  You can not modify and use this source freely
 *  only for the development of application related OutLookAR.
 * 
 * (c) OutLookAR All rights reserved.
 * by Toshiki Imagawa
**/
using UnityEngine;
namespace OutLookAR.Test
{
    public class CameraTest : MonoBehaviour
    {
        // Update is called once per frame
        public void UpdateScreen(Texture2D tex)
        {
            ARCameraManager.Instance.UpdateScreenTexture(tex);
        }
    }
}