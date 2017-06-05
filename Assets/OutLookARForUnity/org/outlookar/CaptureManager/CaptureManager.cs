/**
 * 
 *  You can not modify and use this source freely
 *  only for the development of application related OutLookAR.
 * 
 * (c) OutLookAR All rights reserved.
 * by Toshiki Imagawa
**/
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using OpenCVForUnity;
namespace OutLookAR
{
    public class CaptureManager : SingletonMonoBehaviour<CaptureManager>
    {
#if UNITY_EDITOR
        [SerializeField]
        bool DenugMode = false;
#endif
        [SerializeField]
        bool AutoStart;

        /// <summary>
        /// The web cam texture.
        /// </summary>
        WebCamTexture webCamTexture;

        /// <summary>
        /// The web cam device.
        /// </summary>
        WebCamDevice _webCamDevice;
        WebCamDevice webCamDevice
        {
            get
            {
                if (_webCamDevice.name == null)
                    _webCamDevice = WebCamTexture.devices[0];
                return _webCamDevice;
            }
        }
        int DeviceID = 0;

        /// <summary>
        /// The colors.
        /// </summary>
        Color32[] colors;

        /// <summary>
        /// The rgba mat.
        /// </summary>
        Mat rgbaMat;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The init done.
        /// </summary>
        bool initDone = false;

        public OnUpdateTexture onUpdateTexture = new OnUpdateTexture();
        public OnUpdateMat onUpdateMat = new OnUpdateMat();

        void Change()
        {
            if (!initDone)
                return;

#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
			if (webCamTexture.width > 16 && webCamTexture.height > 16) {
#else
            if (webCamTexture.didUpdateThisFrame)
            {
#endif
                rgbaMat.Dispose();
                rgbaMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);
                OpenCVForUnity.Utils.webCamTextureToMat(webCamTexture, rgbaMat, colors);
                if (webCamTexture.videoVerticallyMirrored)
                {
                    if (webCamDevice.isFrontFacing)
                    {
                        if (webCamTexture.videoRotationAngle == 0)
                        {
                            Core.flip(rgbaMat, rgbaMat, 1);
                        }
                        else if (webCamTexture.videoRotationAngle == 90)
                        {
                            Core.flip(rgbaMat, rgbaMat, 0);
                        }
                        else if (webCamTexture.videoRotationAngle == 270)
                        {
                            Core.flip(rgbaMat, rgbaMat, 1);
                        }
                    }
                    else
                    {
                        if (webCamTexture.videoRotationAngle == 90)
                        {

                        }
                        else if (webCamTexture.videoRotationAngle == 270)
                        {
                            Core.flip(rgbaMat, rgbaMat, -1);
                        }
                    }
                }
                else
                {
                    if (webCamDevice.isFrontFacing)
                    {
                        if (webCamTexture.videoRotationAngle == 0)
                        {
                            Core.flip(rgbaMat, rgbaMat, 1);
                        }
                        else if (webCamTexture.videoRotationAngle == 90)
                        {
                            Core.flip(rgbaMat, rgbaMat, 0);
                        }
                        else if (webCamTexture.videoRotationAngle == 270)
                        {
                            Core.flip(rgbaMat, rgbaMat, 1);
                        }
                    }
                    else
                    {
                        if (webCamTexture.videoRotationAngle == 90)
                        {

                        }
                        else if (webCamTexture.videoRotationAngle == 270)
                        {
                            Core.flip(rgbaMat, rgbaMat, -1);
                        }
                    }
                }
                onUpdateMat.Invoke(rgbaMat);
                DestroyImmediate(texture);
                texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
                OpenCVForUnity.Utils.matToTexture2D(rgbaMat, texture, colors);
                onUpdateTexture.Invoke(texture);
            }
        }

        public void DevicesChange()
        {
            DeviceID++;
            if (DeviceID >= WebCamTexture.devices.Length || DeviceID < 0)
            {
                DeviceID = 0;
            }
            _webCamDevice = WebCamTexture.devices[DeviceID];
            Play();
        }
        /// <summary>
        /// Init this instance.
        /// 初期化
        /// </summary>
        IEnumerator init()
        {
            Stop();
            webCamTexture = new WebCamTexture(webCamDevice.name);
            // Starts the camera
            webCamTexture.Play();

            while (true)
            {
                //If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
				if (webCamTexture.width > 16 && webCamTexture.height > 16) {
#else
                if (webCamTexture.didUpdateThisFrame)
                {
#endif
                    colors = new Color32[webCamTexture.width * webCamTexture.height];

                    rgbaMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);

                    texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);

                    initDone = true;

                    ARCameraManager.Instance.Init(webCamTexture.width, webCamTexture.height);

                    break;
                }
                else
                {
                    yield return 0;
                }
            }
#if UNITY_EDITOR
            if (DenugMode)
                Debug.Log("CaptureManager : Device name is " + webCamDevice.name);
#endif
        }

        /// <summary>
        /// Stop the capture.
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                initDone = false;
                if (rgbaMat != null)
                    rgbaMat.Dispose();
            }
        }
        /// <summary>
        /// Play the capture.
        /// 再生
        /// </summary>
        public void Play()
        {
            StartCoroutine(init());
        }
        public void Play(int id)
        {
            DeviceID = id;
            if (DeviceID >= WebCamTexture.devices.Length || DeviceID < 0)
            {
                DeviceID = 0;
            }
            _webCamDevice = WebCamTexture.devices[DeviceID];
            Play();
        }

        // Use this for initialization
        protected virtual void Start()
        {
            if (AutoStart)
            {
                Play(1);
            }
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            Change();
        }

        void OnDisable()
        {
            Stop();
            webCamTexture.Stop();
        }
        [System.Serializable]
        public class OnUpdateTexture : UnityEvent<Texture2D> { }
        [System.Serializable]
        public class OnUpdateMat : UnityEvent<Mat> { }
    }
}