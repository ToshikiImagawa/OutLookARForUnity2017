/**
 * 
 *  You can not modify and use this source freely
 *  only for the development of application related OutLookAR.
 * 
 * (c) OutLookAR All rights reserved.
 * by Toshiki Imagawa
**/
using System;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;

namespace OutLookAR
{
    public static class Utils
    {
        static DescriptorMatcher matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);

        /// <summary>
        /// Crosses the matcher.
        /// </summary>
        /// <returns>The matcher.</returns>
        /// <param name="queryDescriptors">Query descriptors.</param>
        /// <param name="trainDescriptors">Train descriptors.</param>
        public static IList<DMatch> CrossMatcher(Mat queryDescriptors, Mat trainDescriptors)
        {
            MatOfDMatch matchQT = new MatOfDMatch();
            MatOfDMatch matchTQ = new MatOfDMatch();
            List<DMatch> bmatch = new List<DMatch>();
            DMatch[] dmatch;
            if (trainDescriptors.cols() <= 0)
                throw new OutLookARException("CrossMatcherの引数trainDescriptorsがありません。");
            matcher.match(queryDescriptors, trainDescriptors, matchQT);
            if (matchQT.rows() <= 0)
                Debug.Log("matchQTはmatchしませんでした.");
            if (queryDescriptors.cols() <= 0)
                throw new OutLookARException("CrossMatcherの引数queryDescriptorsがありません。");
            matcher.match(trainDescriptors, queryDescriptors, matchTQ);
            if (matchTQ.rows() <= 0)
                Debug.Log("matchTQはmatchしませんでした.");
            for (int i = 0; i < matchQT.rows(); i++)
            {
                DMatch forward = matchQT.toList()[i];
                DMatch backward = matchTQ.toList()[forward.trainIdx];
                if (backward.trainIdx == forward.queryIdx)
                    bmatch.Add(forward);
            }
            dmatch = bmatch.ToArray();
            bmatch.Clear();
            return dmatch;
        }

        public static int ORBMatcher(Mat queryMat, Mat trainMat, MatOfKeyPoint queryKeypoints, MatOfKeyPoint trainKeypoints, out IList<DMatch> matches)
        {
            using (Mat queryDescriptors = new Mat())
            using (Mat trainDescriptors = new Mat())
            {
                queryMat.ORBPointFeature(queryKeypoints, queryDescriptors);
                trainMat.ORBPointFeature(trainKeypoints, trainDescriptors);
                if (queryDescriptors.type() == CvType.CV_8U && trainDescriptors.type() == CvType.CV_8U)
                {
                    matches = Utils.CrossMatcher(queryDescriptors, trainDescriptors);
                    if (matches.Count > 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    matches = null;
                    return -1;
                }
            }
        }
/*        public static void ORBPointFeature(Mat src, out IList<Vector3> KeyVector, Mat Descriptors)
        {
            using (MatOfKeyPoint points = new MatOfKeyPoint())
            {
                src.ORBPointFeature(points, Descriptors);
                KeyPoint[] pointL = points.toArray();
                KeyVector = new Vector3[pointL.Length];
                for (int i = 0; i < pointL.Length; i++)
                {
                    KeyVector[i] = ARManager.Instance.ToPoint(pointL[i]);
                }
            }
        }*/

        /// <summary>
        /// Vectors the length.
        /// ベクトルの長さを計算する
        /// </summary>
        /// <returns>The length.</returns>
        /// <param name="V">V.</param>
        public static float VectorLength(Vector2 V)
        {
            return (float)Math.Sqrt(V.x * V.x + V.y * V.y);
        }
        /// <summary>
        /// Dots the product.
        /// ベクトル内積
        /// </summary>
        /// <returns>The product.</returns>
        /// <param name="vl">Vl.</param>
        /// <param name="vr">Vr.</param>
        public static float DotProduct(Vector2 vl, Vector2 vr)
        {
            return vl.x * vr.x + vl.y * vr.y;
        }
        /// <summary>
        /// Crosses the product.
        /// ベクトル外積
        /// </summary>
        /// <returns>The product.</returns>
        /// <param name="vl">Vl.</param>
        /// <param name="vr">Vr.</param>
        public static float CrossProduct(Vector2 vl, Vector2 vr)
        {
            return vl.x * vr.y - vl.y * vr.x;
        }

        /// <summary>
        /// Froms to look rotation.
		/// 2組のベクトルから回転を求める
        /// </summary>
        /// <returns>The to look rotation.</returns>
        /// <param name="fromDirections">From directions.</param>
        /// <param name="toDirections">To directions.</param>
        public static Quaternion FromToLookRotation(IList<Vector3> fromDirections, IList<Vector3> toDirections)
        {
            if (fromDirections.Count != 2 || toDirections.Count != 2)
            {
                throw new ApplicationException("FromToLookRotationの引数が2つずつではありません。");
            }
            var fromDirectionM = fromDirections[0].normalized + (fromDirections[1].normalized - fromDirections[0].normalized) / 2;
            var toDirectionM = toDirections[0].normalized + (toDirections[1].normalized - toDirections[0].normalized) / 2;

            var axis = Vector3.Cross(toDirections[0].normalized - fromDirections[0].normalized, toDirections[1].normalized - fromDirections[1].normalized);
            var orthogonal = Vector3.Project(fromDirectionM.normalized, axis.normalized);
            var fromDirectionMO = fromDirectionM.normalized - orthogonal;
            var toDirectionMO = toDirectionM.normalized - orthogonal;

            axis = Vector3.Cross(fromDirectionMO.normalized, toDirectionMO.normalized);
            float theta = Vector3.Angle(fromDirectionMO.normalized, toDirectionMO.normalized);

            var q = Quaternion.AngleAxis(theta, axis.normalized);
            return q;
        }
        
        public static Mat Mul(Mat z, Mat w)
        {
            if (z.cols() != w.rows())
            {
                throw new OpenCVForUnity.CvException("Mat type must bu equal to the number of w.rows and z.cols.");
            }
            var Z = z.ToArrayMat();
            var W = w.ToArrayMat();
            var V = Z * W;
            Mat v = V.ToMat();
            return v;
        }
        public static System.Collections.IEnumerator cvmMul(Mat z, Mat w, Action<Mat> callback)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            if (z.cols() != w.rows())
            {
                throw new OpenCVForUnity.CvException("Mat type must bu equal to the number of w.rows and z.cols.");
            }
            Mat v = new Mat(z.rows(), w.cols(), w.type());

            for (int i = 0; i < z.rows(); i++)
            {
                for (int j = 0; j < w.cols(); j++)
                {
                    List<double> buffVL = new List<double>();
                    for (int c = 0; c < z.cols(); c++)
                    {
                        double[] buffZ = z.get(i, c);
                        double[] buffW = w.get(c, j);
                        for (int b = 0; b < buffZ.Length; b++)
                        {
                            if (c == 0)
                            {
                                buffVL.Add(0);
                            }
                            buffVL[b] += buffZ[b] * buffW[b];
                            sw.Stop();
                            if (sw.Elapsed.Milliseconds > 100)
                            {
                                sw.Reset();
                                Debug.Log(string.Format("cvmMul : {0}/{1} ... {2} % : {3}/{4} ,{5}/{6}", i, z.rows(), i * 100 / z.rows(), j, w.cols(), b, buffZ.Length));
                                yield return null;
                            }
                            sw.Start();
                        }
                        buffZ = null;
                        buffW = null;
                    }
                    v.put(i, j, buffVL.ToArray());
                }
            }
            GC.Collect();
            yield return null;
            callback(v);
        }
		/// <summary>
		/// Cvms the mul.
		/// 非同期による行列の掛け算
		/// </summary>
		/// <returns>The mul.</returns>
		/// <param name="z">The z coordinate.</param>
		/// <param name="w">The width.</param>
		/// <param name="callback">Callback.</param>
        public static System.Collections.IEnumerator cvmMul(ArrayMat z, ArrayMat w, Action<ArrayMat> callback)
        {
            if (z.Cols != w.Rows)
            {
                throw new OutLookARException("Mat type must bu equal to the number of w.rows and z.cols.");
            }
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            ArrayMat v = ArrayMat.zeros(z.Rows, w.Cols);
            for (int r = 0; r < v.Rows; r++)
            {
                for (int c = 0; c < v.Cols; c++)
                {
                    double pt = 0;
                    for (int b = 0; b < z.Cols; b++)
                    {
                        pt += z.At(r, b) * w.At(b, c);
                        if (sw.Elapsed.Milliseconds > 100)
                        {
                            sw.Reset();
                            Debug.Log(string.Format("cvmMul :... {0}/{1} ... {2} %", r, v.Rows, r * 100 / v.Rows));
                            yield return null;
                        }
                        sw.Start();
                    }
                    v.At(r, c, pt);
                }
            }
            callback(v);
        }
		/// <summary>
		/// Safes the dispose.
		/// 確実にDisposeする
		/// </summary>
		/// <param name="disposable">Disposable.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void SafeDispose<T>(ref T disposable) where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }
		/// <summary>
		/// Arraies the dispose.
		/// 配列のDispose
		/// </summary>
		/// <param name="array">Array.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ArrayDispose<T>(ref T[] array) where T : class, IDisposable
        {
            if (array != null)
            {
                for (int i = 0; i < array.Length; ++i) SafeDispose(ref array[i]);
                array = null;
            }
        }
    }
}