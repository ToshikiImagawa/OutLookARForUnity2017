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
namespace OutLookAR.Test
{
    public class PointFeatureTest : MonoBehaviour
    {

        string text = "";
        // Use this for initialization
        void Start()
        {
            CaptureManager.Instance.onUpdateMat.AddListener(c => UpdateScreen(c));
        }

        // Update is called once per frame
        void UpdateScreen(Mat mat)
        {
            FeatureDetector detector = FeatureDetector.create(FeatureDetector.ORB);
            DescriptorExtractor extractor = DescriptorExtractor.create(DescriptorExtractor.ORB);

            MatOfKeyPoint keypoints = new MatOfKeyPoint();
            Mat descriptors = new Mat();

            detector.detect(mat, keypoints);
            //extractor.compute(mat, keypoints, descriptors);

            var Points = keypoints.toArray();
            foreach (KeyPoint kp in Points)
            {
                int a = (int)kp.pt.x / mat.width() * 250;
                int b = (int)kp.pt.y / mat.height() * 250;

                Scalar color = new Scalar(255, b, a, 100);
                switch (Random.Range(0, 3))
                {
                    case 0:
                        color = new Scalar(255, a, b, 100);
                        break;
                    case 1:
                        color = new Scalar(a, 255, b, 100);
                        break;
                    case 2:
                        color = new Scalar(a, b, 255, 100);
                        break;
                }
                Imgproc.circle(mat, kp.pt, 4, color, -1);
            }
            text = string.Format("PointFeature Count : {0}.", Points.Length);

            Texture2D tex = new Texture2D(mat.width(), mat.height());
            OpenCVForUnity.Utils.matToTexture2D(mat, tex);
            ARCameraManager.Instance.UpdateTexture(tex);
        }

        void OnGUI()
        {
            GUI.Label(new UnityEngine.Rect(Screen.width - 100, Screen.height - 50, 100, 50), text);
        }
    }
}