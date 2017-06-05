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
using System.Collections;

namespace OutLookAR.Test
{
    public class Fireworks : MonoBehaviour
    {
        [SerializeField]
        GameObject CameraObject;
        [SerializeField]
        GameObject FireworksObject;
        [SerializeField]
        GameObject DustStormMobile;
        [SerializeField]
        float length = 60f;
        [SerializeField]
        int MaxInstance = 20;

        int instanceCount = 0;

        Camera _camera;
        Camera Cam { get { return _camera ?? (_camera = CameraObject.GetComponent<Camera>()); } }

        void Update()
        {
            //タッチがあるかどうか？
            if (Input.touchCount > 0)
            {
                var touch = Input.touches[0];
                if (touch.phase == TouchPhase.Ended)
                {
                    var x = touch.position.x / Cam.pixelWidth;
                    var y = 1f - (touch.position.y / Cam.pixelHeight);
                    Create(x, y);
                }
            }
#if UNITY_EDITOR

            if (Input.GetMouseButtonUp(0))
            {
                var position = Input.mousePosition;
                var x = position.x / Cam.pixelWidth;
                var y = 1f - (position.y / Cam.pixelHeight);
                Create(x, y);
            }
#endif
        }

        void Create(float x, float y)
        {
            if (instanceCount < MaxInstance)
            {
                Debug.Log("Create Fireworks");
                DustStormMobile.SetActive(false);
                StartCoroutine(CreateFireworks(CameraObject.transform.rotation * ARCameraManager.Instance.ToVector(new Point(x * ARCameraManager.Instance.Width, y * ARCameraManager.Instance.Height)) * length));
            }
            else
            {
                DustStormMobile.SetActive(true);
            }
        }
        IEnumerator CreateFireworks(Vector3 pos)
        {
            GameObject instans = Instantiate(FireworksObject);
            instans.transform.position = pos;
            instanceCount++;
            yield return new WaitForSeconds(20);
            DestroyImmediate(instans);
            instanceCount--;
        }
    }
}