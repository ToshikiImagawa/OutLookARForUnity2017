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
    public class MenusUI : MonoBehaviour
    {
        [SerializeField]
        GameObject ButtonObject;
        [SerializeField]
        GameObject BaseObject;
        [SerializeField]
        GameObject TrackingManager;
        [SerializeField]
        Slider DiaganalAngle;
        [SerializeField]
        InputField[] LensDistortion;
        [SerializeField]
        GameObject[] SceneObjects;

        public void Open()
        {
            ButtonObject.SetActive(false);
            BaseObject.SetActive(true);
            foreach (GameObject scene in SceneObjects)
            {
                scene.SetActive(true);
            }
            switch (Application.loadedLevelName)
            {
                case "OutlookARForUnitySample":
                    TrackingManager.SetActive(false);
                    break;
                case "CameraTest":
                    SceneObjects[0].SetActive(false);
                    TrackingManager.SetActive(false);
                    break;
                case "PointFeatureTest":
                    SceneObjects[1].SetActive(false);
                    TrackingManager.SetActive(false);
                    break;
                case "MatchingTest":
                    SceneObjects[2].SetActive(false);
                    TrackingManager.SetActive(false);
                    break;
                case "TrackingTest":
                    SceneObjects[3].SetActive(false);
                    TrackingManager.SetActive(true);
                    getValues();
                    break;
                case "DetailedTrackingTest":
                    SceneObjects[4].SetActive(false);
                    TrackingManager.SetActive(true);
                    getValues();
                    break;
                case "RadioControlLite":
                    SceneObjects[5].SetActive(false);
                    TrackingManager.SetActive(true);
                    getValues();
                    break;
                case "RadioControl":
                    SceneObjects[6].SetActive(false);
                    TrackingManager.SetActive(true);
                    getValues();
                    break;
                case "Fireworks":
                    SceneObjects[7].SetActive(false);
                    TrackingManager.SetActive(true);
                    getValues();
                    break;
                case "FireworksLite":
                    SceneObjects[8].SetActive(false);
                    TrackingManager.SetActive(true);
                    getValues();
                    break;
            }

        }
        public void Close()
        {
            BaseObject.SetActive(false);
            ButtonObject.SetActive(true);
        }

        void getValues()
        {
            DiaganalAngle.value = PlayerPrefs.GetFloat("DiagonalAngle", 77f);
            LensDistortion[0].text = PlayerPrefs.GetFloat("LensDistortionX", 0.87f).ToString();
            LensDistortion[1].text = PlayerPrefs.GetFloat("LensDistortionY", 0.87f).ToString();
            ARCameraManager.Instance.DiagonalAngle = PlayerPrefs.GetFloat("DiagonalAngle", 77f);
            ARCameraManager.Instance.setLensDistortionX(PlayerPrefs.GetFloat("LensDistortionX", 0.87f));
            ARCameraManager.Instance.setLensDistortionY(PlayerPrefs.GetFloat("LensDistortionY", 0.87f));
        }

        public void OnChangeDiagonalAngle(Single val)
        {
            ARCameraManager.Instance.DiagonalAngle = val;
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
        
        public void OnScene(string name)
        {
            Application.LoadLevel(name);
        }
    }
}