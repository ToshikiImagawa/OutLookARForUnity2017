/**
* 
*  You can not modify and use this source freely
*  only for the development of application related OutLookAR.
* 
* (c) OutLookAR All rights reserved.
* by Toshiki Imagawa
**/
using UnityEngine;
using UnityEngine.Events;
using OpenCVForUnity;
using System.Collections.Generic;

namespace OutLookAR.Test
{
    public class TrackingTest : MonoBehaviour
    {
        [SerializeField]
        bool debugMode = false;
        [SerializeField]
        float MatchFilter = 20f;
        [SerializeField]
        float LMedSFilter = 10f;
        public TrackingTestEvent _trackingTestEvent = new TrackingTestEvent();
        public MatchTestEvent _matchTestEvent = new MatchTestEvent();
        string text;
        float startime = 0;
        Texture2D tex;

        Quaternion Attitude = Quaternion.identity;
        List<List<Vector3>> Landmarks = new List<List<Vector3>>();
        Mat MapDescriptors = new Mat();

        FeatureDetector detector = FeatureDetector.create(FeatureDetector.ORB);
        DescriptorExtractor extractor = DescriptorExtractor.create(DescriptorExtractor.ORB);
        DescriptorMatcher matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);

        public void Init()
        {
            Attitude = Quaternion.identity;
            foreach (var val in Landmarks)
            {
                val.Clear();
            }
            Landmarks.Clear();
            MapDescriptors.Dispose();
            MapDescriptors = new Mat();
        }
        public void UpdateAttitude(Mat mat)
        {
            int LandmarksCount = 0;
            int MatchsCount = 0;
            using (MatOfKeyPoint keypoints = new MatOfKeyPoint())
            using (Mat descriptors = new Mat())
            {
                detector.detect(mat, keypoints);
                extractor.compute(mat, keypoints, descriptors);

                var trainPoints = keypoints.toArray();

                List<List<Vector3>> newLandmarks = new List<List<Vector3>>();
                foreach (var keyPoint in trainPoints)
                {
                    var keyVectorL = new List<Vector3>();
                    keyVectorL.Add(ARCameraManager.Instance.ToVector(keyPoint));
                    newLandmarks.Add(keyVectorL);
                    LandmarksCount++;
                }
                if (Landmarks.Count > 0)
                {
                    List<Vector3> FromVectorL = new List<Vector3>();
                    List<Vector3> ToVectorL = new List<Vector3>();
                    using (MatOfDMatch matches = new MatOfDMatch())
                    using (MatOfDMatch crossMatches = new MatOfDMatch())
                    {
                        matcher.match(MapDescriptors, descriptors, matches);
                        matcher.match(descriptors, MapDescriptors, crossMatches);
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
                                    MatchsCount++;
                                }
                            }
                            if (match.distance > MatchFilter)
                            {
                                flag = false;
                            }
                            if (flag)
                            {
                                var trainVectors = newLandmarks[match.trainIdx];
                                var queryVectors = Landmarks[match.queryIdx];
                                FromVectorL.Add(trainVectors[0]);
                                //ToVectorL.Add(queryVectors.ToArray().Median()); START
                                double[] queryPointsX = new double[queryVectors.Count];
                                double[] queryPointsY = new double[queryVectors.Count];
                                for (int j = 0; j < queryVectors.Count; j++)
                                {
                                    var queryPoint = ARCameraManager.Instance.toPoint(queryVectors[j], Attitude);
                                    queryPointsX[j] = queryPoint.x;
                                    queryPointsY[j] = queryPoint.y;
                                }
                                ToVectorL.Add(Attitude * ARCameraManager.Instance.ToVector(new Point(queryPointsX.Median(), queryPointsY.Median())));
                                //ToVectorL.Add(queryVectors.ToArray().Median()); END
                                newLandmarks[match.trainIdx].AddRange(queryVectors.ToArray());
                            }
                            i++;
                        }
                        Quaternion newAttitude;
                        float error = ARCameraManager.Instance.LMedS(FromVectorL, ToVectorL, out newAttitude);
                        _matchTestEvent.Invoke(FromVectorL.Count);
                        FromVectorL.Clear();
                        ToVectorL.Clear();
                        if (error > 0 && LMedSFilter > error)
                        {
                            Attitude = newAttitude;
                            _trackingTestEvent.Invoke(Attitude);
                            //ARCameraManager.Instance.UpdateCameraPosture(Attitude);
                            if (debugMode) Debug.Log(string.Format("Attitude = {0}\nError = {1}", Attitude, error));
                        }
                        foreach (var newLandmark in newLandmarks)
                        {
                            newLandmark[0] = Attitude * newLandmark[0];
                        }
                    }
                }
                MapDescriptors.Dispose();
                Landmarks.Clear();
                Landmarks = newLandmarks;
                MapDescriptors = descriptors.clone();
            }
            float now = Time.time;
            if (debugMode)
                Debug.Log(string.Format("time : {0} Landmarks : {1}, Matchs : {2}.", 1 / (Time.time - startime), LandmarksCount, MatchsCount));
            startime = Time.time;
        }
        public void UpdateScreen(Texture2D tex)
        {
            ARCameraManager.Instance.UpdateScreenTexture(tex);
        }

        [System.Serializable]
        public class TrackingTestEvent : UnityEvent<Quaternion> { }
        [System.Serializable]
        public class MatchTestEvent : UnityEvent<int> { }
    }
}