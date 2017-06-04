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

        // Use this for initialization
        void Start()
        {
            CaptureManager.Instance.onUpdateTexture.AddListener(c => UpdateScreen(c));
        }

        // Update is called once per frame
        void UpdateScreen(Texture2D tex)
        {
            ARCameraManager.Instance.UpdateTexture(tex);
        }
    }
}