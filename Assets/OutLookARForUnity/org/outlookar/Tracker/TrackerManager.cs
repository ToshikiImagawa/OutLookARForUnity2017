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
using System.Collections.Generic;

namespace OutLookAR
{
    public class TrackerManager : SingletonMonoBehaviour<TrackerManager>
    {
        [SerializeField]
        float MatchFilter = 20f;
        [SerializeField]
        float LMedSFilter = 10f;
        [SerializeField]
        int SufficientCount = 4;
        [SerializeField]
        float FinalizedPercentage = 0.5f;
        public TrackingEvent _trackingEvent = new TrackingEvent();
        public MatchingEvent _matchingEvent = new MatchingEvent();

        List<List<Vector3>> ProvisioningLandmarks = new List<List<Vector3>>();
        List<List<Mat>> ProvisioningLandmarkDescriptors = new List<List<Mat>>();
        List<Vector3> FinalizingLandmarks = new List<Vector3>();
        Mat FinalizingLandmarkDescriptors;

        Quaternion Attitude = Quaternion.identity;

        FeatureDetector detector = FeatureDetector.create(FeatureDetector.ORB);
        DescriptorExtractor extractor = DescriptorExtractor.create(DescriptorExtractor.ORB);
        DescriptorMatcher matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);

        int OptimisationLandmark(IList<Vector3> Landmarks, Quaternion attitudeEstimate, out Vector3 dst)
        {
            List<double> PointLandmarks_X = new List<double>();
            List<double> PointLandmarks_Y = new List<double>();
            foreach (Vector3 Landmark in Landmarks)
            {
                try
                {
                    Point buff = ARCameraManager.Instance.toPoint(Landmark, attitudeEstimate);
                    PointLandmarks_X.Add(buff.x);
                    PointLandmarks_Y.Add(buff.y);
                }
                catch (OutLookARException e)
                {
                    if (e.Message == "カメラが初期化されていません.")
                    {
                        PointLandmarks_X.Clear();
                        PointLandmarks_Y.Clear();
                        throw new OutLookARException("ARCameraManager : カメラが初期化されていません.");
                    }
                }
            }
            int count = PointLandmarks_X.Count;
            if (count > 0)
            {
                try
                {
                    dst = attitudeEstimate * ARCameraManager.Instance.ToVector(new Point(PointLandmarks_X.Median(), PointLandmarks_Y.Median()));
                }
                catch (OutLookARException e)
                {
                    if (e.Message == "カメラが初期化されていません.")
                    {
                        PointLandmarks_X.Clear();
                        PointLandmarks_Y.Clear();
                        throw new OutLookARException("ARCameraManager : カメラが初期化されていません.");
                    }
                    dst = Vector3.zero;
                    return 0;
                }
            }
            else
            {
                dst = Vector3.zero;
            }
            PointLandmarks_X.Clear();
            PointLandmarks_Y.Clear();
            return count;
        }

        List<Vector3> _optimisationLandmarks = new List<Vector3>();
        List<Vector3> OptimisationLandmarks(Quaternion attitudeEstimate, out IList<int> FinalizeIndex)
        {
            if (_optimisationLandmarks.Count > 0)
                _optimisationLandmarks.Clear();
            if (ProvisioningLandmarks.Count <= 0)
            {
                throw new OutLookARException("ProvisioningLandmarks is empty.");
            }
            List<int> _FinalizeIndex = new List<int>();
            for (int i = 0; i < ProvisioningLandmarks.Count; i++)
            {
                Vector3 optimisationLandmark;
                int filter = OptimisationLandmark(ProvisioningLandmarks[i], attitudeEstimate, out optimisationLandmark);
                if (filter > 0)
                {
                    _optimisationLandmarks.Add(optimisationLandmark);
                    if (filter > SufficientCount)
                    {
                        _FinalizeIndex.Add(i);
                    }
                }
            }
            FinalizeIndex = _FinalizeIndex.ToArray();
            return _optimisationLandmarks;
        }

        void OptimisationDescriptor(IList<Mat> Descriptors, out Mat dst)
        {
            List<List<double>> DescriptorValues = new List<List<double>>();
            foreach (Mat Descriptor in Descriptors)
            {
                for (int i = 0; i < Descriptor.cols(); i++)
                {
                    if (DescriptorValues.Count < Descriptor.cols())
                    {
                        List<double> Value = new List<double>();
                        Value.Add(Descriptor.get(0, i)[0]);
                        DescriptorValues.Add(Value);
                    }
                    else
                    {
                        DescriptorValues[i].Add(Descriptor.get(0, i)[0]);
                    }
                }
            }
            var buff = new Mat(1, DescriptorValues.Count, Descriptors[0].type());
            for (int i = 0; i < DescriptorValues.Count; i++)
            {
                buff.put(0, i, DescriptorValues[i].Median());
            }
            DescriptorValues.Clear();
            dst = buff;
        }
        Mat _optimisationDescriptors;
        Mat OptimisationDescriptors
        {
            get
            {
                if (_optimisationDescriptors != null)
                    _optimisationDescriptors.Dispose();
                if (ProvisioningLandmarkDescriptors.Count <= 0)
                {
                    throw new OutLookARException("ProvisioningLandmarkDescriptors is empty.");
                }
                for (int r = 0; r < ProvisioningLandmarkDescriptors.Count; r++)
                {
                    Mat optimisationDescriptor;
                    OptimisationDescriptor(ProvisioningLandmarkDescriptors[r], out optimisationDescriptor);
                    if (r == 0)
                    {
                        _optimisationDescriptors = optimisationDescriptor;
                    }
                    else
                    {
                        _optimisationDescriptors.push_back(optimisationDescriptor);
                    }
                }
                return _optimisationDescriptors;
            }
        }

        void Optimisation(Quaternion attitudeEstimate, out List<Vector3> OptimisationLandmarks, out Mat OptimisationDescriptors)
        {
            if (_optimisationLandmarks.Count > 0)
                _optimisationLandmarks.Clear();
            if (_optimisationDescriptors != null)
                _optimisationDescriptors.Dispose();
            if (ProvisioningLandmarks.Count <= 0)
            {
                throw new OutLookARException("ProvisioningLandmarks is empty.");
            }
            if (ProvisioningLandmarkDescriptors.Count <= 0)
            {
                throw new OutLookARException("ProvisioningLandmarkDescriptors is empty.");
            }
            if (ProvisioningLandmarkDescriptors.Count != ProvisioningLandmarks.Count)
            {
                throw new OutLookARException("ProvisioningLandmarks and ProvisioningLandmarkDescriptors is not match the number.");
            }
            for (int i = 0; i < ProvisioningLandmarks.Count; i++)
            {
                Vector3 optimisationLandmark;
                int filter = OptimisationLandmark(ProvisioningLandmarks[i], attitudeEstimate, out optimisationLandmark);
                Mat optimisationDescriptor;
                OptimisationDescriptor(ProvisioningLandmarkDescriptors[i], out optimisationDescriptor);
                if (filter > 0)
                {
                    _optimisationLandmarks.Add(optimisationLandmark);
                    if (_optimisationDescriptors.IsDisposed)
                    {
                        _optimisationDescriptors = optimisationDescriptor;
                    }
                    else
                    {
                        _optimisationDescriptors.push_back(optimisationDescriptor);
                    }

                    if (filter > SufficientCount)
                    {
                        FinalizingLandmarks.Add(optimisationLandmark);
                        if (FinalizingLandmarkDescriptors != null)
                        {
                            FinalizingLandmarkDescriptors.push_back(optimisationDescriptor);
                        }
                        else
                        {
                            FinalizingLandmarkDescriptors = optimisationDescriptor;
                        }
                    }
                }
            }
            OptimisationLandmarks = _optimisationLandmarks;
            OptimisationDescriptors = _optimisationDescriptors;
        }

        public void Init()
        {
            foreach (var val in ProvisioningLandmarks)
            {
                val.Clear();
            }
            ProvisioningLandmarks.Clear();
            foreach (var val in ProvisioningLandmarkDescriptors)
            {
                val.Dispose();
            }
            ProvisioningLandmarkDescriptors.Clear();
            FinalizingLandmarks.Clear();
            if (FinalizingLandmarkDescriptors != null)
                FinalizingLandmarkDescriptors = null;
                
            Attitude = Quaternion.identity;
        }
        public void Tracking(Mat mat)
        {
            using (MatOfKeyPoint keypoints = new MatOfKeyPoint())
            using (Mat descriptors = new Mat())
            {
                detector.detect(mat, keypoints);
                extractor.compute(mat, keypoints, descriptors);

                var trainPoints = keypoints.toArray();

                List<List<Vector3>> newLandmarks = new List<List<Vector3>>();
                List<List<Mat>> newDescriptors = new List<List<Mat>>();
                for (int i = 0; i < trainPoints.Length; i++)
                {
                    var keyVectorL = new List<Vector3>();
                    keyVectorL.Add(ARCameraManager.Instance.ToVector(trainPoints[i]));
                    var DescriptorL = new List<Mat>();
                    DescriptorL.Add(descriptors.clone().row(i));
                    newLandmarks.Add(keyVectorL);
                    newDescriptors.Add(DescriptorL);
                }

                List<Vector3> FromVectorL = new List<Vector3>();
                List<Vector3> ToVectorL = new List<Vector3>();
                List<int> FinalizingL = new List<int>();
                bool finLMedS = false;

                if (FinalizingLandmarks.Count > 0)
                {
                    using (MatOfDMatch matchesFinal = new MatOfDMatch())
                    using (MatOfDMatch crossMatchesFinal = new MatOfDMatch())
                    {
                        matcher.match(FinalizingLandmarkDescriptors, descriptors, matchesFinal);
                        matcher.match(descriptors, FinalizingLandmarkDescriptors, crossMatchesFinal);
                        var matchLFinal = matchesFinal.toArray();
                        var crossMatchLFinal = crossMatchesFinal.toArray();
                        int i = 0;
                        foreach (DMatch match in matchLFinal)
                        {
                            bool flag = false;
                            foreach (DMatch crossMatch in crossMatchLFinal)
                            {
                                if (match.trainIdx == crossMatch.queryIdx && match.queryIdx == crossMatch.trainIdx)
                                {
                                    flag = true;
                                }
                            }
                            if (match.distance > MatchFilter)
                            {
                                flag = false;
                            }
                            if (flag)
                            {
                                FromVectorL.Add(newLandmarks[match.trainIdx][0]);
                                ToVectorL.Add(FinalizingLandmarks[match.queryIdx]);
                                FinalizingL.Add(match.trainIdx);
                                newLandmarks[match.trainIdx][0] = FinalizingLandmarks[match.queryIdx];
                                newDescriptors[match.trainIdx][0] = FinalizingLandmarkDescriptors.row(match.queryIdx);
                            }
                            i++;
                        }
                        Quaternion newAttitude;
                        float error = ARCameraManager.Instance.LMedS(FromVectorL, ToVectorL, out newAttitude);
                        if (error > 0 && LMedSFilter > error)
                        {
                            Attitude = newAttitude;
                            _trackingEvent.Invoke(Attitude);
                            _matchingEvent.Invoke(FromVectorL.Count);
                            //ARCameraManager.Instance.UpdateCameraPosture(Attitude);
                            Debug.Log(string.Format("Attitude = {0}\nError = {1}\nFinalizMatch = {2}\nAccuracy = {3}", Attitude, error, FinalizingL.Count, 100 * FinalizingL.Count / FromVectorL.Count));
                            finLMedS = true;
                        }
                    }
                }

                if (ProvisioningLandmarks.Count > 0)
                {
                    using (MatOfDMatch matches = new MatOfDMatch())
                    using (MatOfDMatch crossMatches = new MatOfDMatch())
                    {
                        Mat optimisationDescriptors = OptimisationDescriptors;
                        matcher.match(optimisationDescriptors, descriptors, matches);
                        matcher.match(descriptors, optimisationDescriptors, crossMatches);
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
                            if (match.distance > MatchFilter)
                            {
                                flag = false;
                            }
                            if (flag)
                            {
                                if (FinalizingL.IndexOf(match.trainIdx) < 0)
                                {
                                    var trainVectors = newLandmarks[match.trainIdx];
                                    var queryVectors = ProvisioningLandmarks[match.queryIdx];
                                    Vector3 queryVector;
                                    int filter = OptimisationLandmark(queryVectors, Attitude, out queryVector);
                                    if (filter > 0)
                                    {
                                        if ((filter > SufficientCount) && (matchL.Length * FinalizedPercentage < FinalizingL.Count || matchL.Length * FinalizedPercentage > FinalizingLandmarks.Count))
                                        {
                                            FinalizingLandmarks.Add(queryVector);
                                            if (FinalizingLandmarkDescriptors != null)
                                            {
                                                FinalizingLandmarkDescriptors.push_back(optimisationDescriptors.row(match.queryIdx));
                                            }
                                            else
                                            {
                                                FinalizingLandmarkDescriptors = optimisationDescriptors.row(match.queryIdx);
                                            }

                                            Debug.Log(string.Format("Finalizing :Landmark = {0}\nDescriptors = {1}\nCount ALL = {2}", queryVector, optimisationDescriptors.row(match.queryIdx).ToStringMat(), FinalizingLandmarks.Count));
                                        }
                                        else
                                        {
                                            FromVectorL.Add(trainVectors[0]);
                                            ToVectorL.Add(queryVector);
                                            newLandmarks[match.trainIdx].AddRange(queryVectors.ToArray());
                                            newDescriptors[match.trainIdx].AddRange(ProvisioningLandmarkDescriptors[match.queryIdx].ToArray());
                                        }
                                    }
                                }
                            }
                            i++;
                        }
                    }
                }

                if (FromVectorL.Count == ToVectorL.Count && ToVectorL.Count > 0)
                {
                    Quaternion newAttitude;
                    float error = ARCameraManager.Instance.LMedS(FromVectorL, ToVectorL, out newAttitude);
                    if ((error > 0 && LMedSFilter > error) && (!finLMedS))
                    {
                        Attitude = newAttitude;
                        _trackingEvent.Invoke(Attitude);
                        //ARCameraManager.Instance.UpdateCameraPosture(Attitude);
                        Debug.Log(string.Format("Attitude = {0}\nError = {1}\nFinalizMatch = {2}\nAccuracy = {3}", Attitude, error, FinalizingL.Count, 100 * FinalizingL.Count / FromVectorL.Count));
                    }
                    for (int i = 0; i < newLandmarks.Count; i++)
                    {
                        if (FinalizingL.IndexOf(i) < 0)
                        {
                            newLandmarks[i][0] = Attitude * newLandmarks[i][0];
                        }
                    }
                }
                _matchingEvent.Invoke(FromVectorL.Count);
                FromVectorL.Clear();
                ToVectorL.Clear();
                ProvisioningLandmarks.Clear();
                ProvisioningLandmarks = newLandmarks;
                ProvisioningLandmarkDescriptors = newDescriptors;
            }
        }
        [System.Serializable]
        public class TrackingEvent : UnityEvent<Quaternion> { }
        [System.Serializable]
        public class MatchingEvent : UnityEvent<int> { }
    }
}