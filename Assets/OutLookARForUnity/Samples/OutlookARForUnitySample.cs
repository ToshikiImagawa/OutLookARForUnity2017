/**
* 
*  You can not modify and use this source freely
*  only for the development of application related OutLookAR.
* 
* (c) OutLookAR All rights reserved.
* by Toshiki Imagawa
**/
using System;
using UnityEngine;
using UnityEngine.UI;

namespace OutLookAR.Test
{
    public class OutlookARForUnitySample : MonoBehaviour
    {
        [SerializeField]
        GameObject BaseUI;
        [SerializeField]
        GameObject MenuUI;

        [SerializeField]
        GameObject TrackingManagerUI;
        [SerializeField]
        Text SliderText;
        [SerializeField]
        GameObject[] Scenes;


        public void OnClicks(string name)
        {
            Application.LoadLevel(name);
        }
        void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (Application.loadedLevelName == "OutlookARForUnitySample")
                OnOpen();
            else
                OnClose();
            if (Application.loadedLevelName == "TrackingTest" || Application.loadedLevelName == "RadioControl")
            {
                ARCameraManager.Instance.setLensDistortionX(PlayerPrefs.GetFloat("LensDistortionX", 0.87f));
                ARCameraManager.Instance.setLensDistortionY(PlayerPrefs.GetFloat("LensDistortionY", 0.87f));
                ARCameraManager.Instance.DiagonalAngle = PlayerPrefs.GetFloat("DiagonalAngle", 77f);
                SliderText.text = PlayerPrefs.GetFloat("DiagonalAngle", 77f).ToString();
            }
        }

        public void OnClose()
        {
            BaseUI.SetActive(true);
            MenuUI.SetActive(false);
        }
        public void OnOpen()
        {
            BaseUI.SetActive(false);
            MenuUI.SetActive(true);
            foreach (GameObject scene in Scenes)
            {
                scene.SetActive(true);
            }
            switch (Application.loadedLevelName)
            {
                case "OutlookARForUnitySample":
                    TrackingManagerUI.SetActive(false);
                    break;
                case "CameraTest":
                    Scenes[0].SetActive(false);
                    TrackingManagerUI.SetActive(false);
                    break;
                case "PointFeatureTest":
                    Scenes[1].SetActive(false);
                    TrackingManagerUI.SetActive(false);
                    break;
                case "MatchingTest":
                    Scenes[2].SetActive(false);
                    TrackingManagerUI.SetActive(false);
                    break;
                case "TrackingTest":
                    Scenes[3].SetActive(false);
                    TrackingManagerUI.SetActive(true);
                    break;
                case "DetailedTrackingTest":
                    Scenes[4].SetActive(false);
                    TrackingManagerUI.SetActive(true);
                    break;
                case "RadioControlLite":
                    Scenes[5].SetActive(false);
                    TrackingManagerUI.SetActive(true);
                    break;
                case "RadioControl":
                    Scenes[6].SetActive(false);
                    TrackingManagerUI.SetActive(true);
                    break;
                case "Fireworks":
                    Scenes[7].SetActive(false);
                    TrackingManagerUI.SetActive(true);
                    break;
                case "FireworksLite":
                    Scenes[8].SetActive(false);
                    TrackingManagerUI.SetActive(true);
                    break;
            }
        }
        public void OnChangeDiagonalAngle(Single val)
        {
            ARCameraManager.Instance.DiagonalAngle = val;
            SliderText.text = val.ToString();
            PlayerPrefs.SetFloat("DiagonalAngle", val);
        }
        public void OnChangeLensDistortionX(String val)
        {
            ARCameraManager.Instance.setLensDistortionX(float.Parse(val));
            PlayerPrefs.SetFloat("LensDistortionX", float.Parse(val));
        }
        public void OnChangeLensDistortionY(String val)
        {
            ARCameraManager.Instance.setLensDistortionY(float.Parse(val));
            PlayerPrefs.SetFloat("LensDistortionY", float.Parse(val));
        }
    }
}