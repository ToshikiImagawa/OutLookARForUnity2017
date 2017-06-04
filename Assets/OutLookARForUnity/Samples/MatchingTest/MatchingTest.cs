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
    public class MatchingTest : MonoBehaviour
    {
        [SerializeField]
        float filter = 20f;
        string text;

        List<List<Point>> buffPointL = new List<List<Point>>();
        List<Scalar> buffColorL = new List<Scalar>();
        Mat buffDescriptors = new Mat();
        FeatureDetector detector = FeatureDetector.create(FeatureDetector.ORB);
        DescriptorExtractor extractor = DescriptorExtractor.create(DescriptorExtractor.ORB);
        DescriptorMatcher matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);
        // Use this for initialization
        void Start()
        {
            CaptureManager.Instance.onUpdateMat.AddListener(c => UpdateScreen(c));
        }

        // Update is called once per frame
        void UpdateScreen(Mat mat)
        {

            MatOfKeyPoint keypoints = new MatOfKeyPoint();
            Mat descriptors = new Mat();

            detector.detect(mat, keypoints);
            extractor.compute(mat, keypoints, descriptors);

            int matchCount = 0;
            var trainPoints = keypoints.toArray();

            var newBuffPointL = new List<List<Point>>();
            var newBuffColorL = new List<Scalar>();
            foreach (var keyPoint in trainPoints)
            {
                var points = new List<Point>();
                points.Add(keyPoint.pt);
                newBuffPointL.Add(points);

                Scalar color = new Scalar(255, 225, 225, 100);
                var x = Random.Range(0, 256);
                switch (Random.Range(0, 6))
                {
                    case 0:
                        color = new Scalar(255, 0, x, 100);
                        break;
                    case 1:
                        color = new Scalar(0, 255, x, 100);
                        break;
                    case 2:
                        color = new Scalar(0, x, 255, 100);
                        break;

                    case 3:
                        color = new Scalar(225, x, 0, 100);
                        break;
                    case 4:
                        color = new Scalar(x,0, 255, 100);
                        break;
                    case 5:
                        color = new Scalar(x, 255, 0, 100);
                        break;
                }
                newBuffColorL.Add(color);
            }
            if (buffPointL.Count > 0)
            {
                MatOfDMatch matches = new MatOfDMatch();
                MatOfDMatch crossMatches = new MatOfDMatch();
                matcher.match(buffDescriptors, descriptors, matches);
                matcher.match(descriptors, buffDescriptors, crossMatches);
                var matchL = matches.toArray();
                var crossMatchL = crossMatches.toArray();
                int i = 0;
                foreach (DMatch match in matchL)
                {
                    bool flag = false;
                    foreach (DMatch crossMatch in crossMatchL)
                    {
                        if (match.trainIdx == crossMatch.queryIdx && match.queryIdx == crossMatch.trainIdx)
                        {
                            flag = true;
                        }
                    }
                    if (match.distance > filter)
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        var trainPoint = trainPoints[match.trainIdx];
                        var queryPoints = buffPointL[match.queryIdx];
                        int a = (int)trainPoint.pt.x / mat.width() * 250;
                        int b = (int)trainPoint.pt.y / mat.height() * 250;

                        Scalar color = buffColorL[match.queryIdx];
                        Imgproc.circle(mat, trainPoint.pt, 4, color, -1);
                        Point startPoint = trainPoint.pt;
                        foreach (Point queryPoint in queryPoints)
                        {
                            Imgproc.line(mat, startPoint, queryPoint, color, 2);
                            Imgproc.circle(mat, queryPoint, 4, color, -1);
                            startPoint = queryPoint;
                            newBuffPointL[match.trainIdx].Add(queryPoint);
                        }
                        newBuffColorL[match.trainIdx]=buffColorL[match.queryIdx];
                        matchCount++;
                    }
                    i++;
                }
            }
            buffPointL = newBuffPointL;
            buffColorL = newBuffColorL;
            buffDescriptors = descriptors;
            text = string.Format("Matching Count : {0}.", matchCount);

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