using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenCVForUnity;

namespace OutLookAR
{
    public static class Extention
    {
        static FeatureDetector ORBDetector = FeatureDetector.create(FeatureDetector.ORB);
        static DescriptorExtractor ORBExtractor = DescriptorExtractor.create(DescriptorExtractor.ORB);

        /// <summary>
        /// Partitions the given list around a pivot element such that all elements on left of pivot are <= pivot
        /// and the ones at thr right are > pivot. This method can be used for sorting, N-order statistics such as
        /// as median finding algorithms.
        /// Pivot is selected ranodmly if random number generator is supplied else its selected as last element in the list.
        /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 171
        /// </summary>
        private static int Partition<T>(this IList<T> list, int start, int end, System.Random rnd = null) where T : IComparable<T>
        {
            if (rnd != null)
                list.Swap(end, rnd.Next(start, end));

            var pivot = list[end];
            var lastLow = start - 1;
            for (var i = start; i < end; i++)
            {
                if (list[i].CompareTo(pivot) <= 0)
                    list.Swap(i, ++lastLow);
            }
            list.Swap(end, ++lastLow);
            return lastLow;
        }

        /// <summary>
        /// Returns Nth smallest element from the list. Here n starts from 0 so that n=0 returns minimum, n=1 returns 2nd smallest element etc.
        /// Note: specified list would be mutated in the process.
        /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 216
        /// </summary>
        public static T NthOrderStatistic<T>(this IList<T> list, int n, System.Random rnd = null) where T : IComparable<T>
        {
            return NthOrderStatistic(list, n, 0, list.Count - 1, rnd);
        }
        private static T NthOrderStatistic<T>(this IList<T> list, int n, int start, int end, System.Random rnd) where T : IComparable<T>
        {
            while (true)
            {
                var pivotIndex = list.Partition(start, end, rnd);
                if (pivotIndex == n)
                    return list[pivotIndex];

                if (n < pivotIndex)
                    end = pivotIndex - 1;
                else
                    start = pivotIndex + 1;
            }
        }
        /// <summary>
        /// Swap the specified list, i and j.
        /// </summary>
        /// <param name="list">List.</param>
        /// <param name="i">The index.</param>
        /// <param name="j">J.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            if (i == j)   //This check is not required but Partition function may make many calls so its for perf reason
                return;
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        /// <summary>
        /// Note: specified list would be mutated in the process.
        /// </summary>
        public static T Median<T>(this IList<T> list) where T : IComparable<T>
        {
            return list.NthOrderStatistic((list.Count - 1) / 2);
        }
        /// <summary>
        /// Median the specified sequence and getValue.
        /// </summary>
        /// <param name="sequence">Sequence.</param>
        /// <param name="getValue">Get value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static double Median<T>(this IEnumerable<T> sequence, Func<T, double> getValue)
        {
            var list = sequence.Select(getValue).ToList();
            var mid = (list.Count - 1) / 2;
            return list.NthOrderStatistic(mid);
        }
        /// <summary>
        /// Dispose the specified list.
        /// </summary>
        /// <param name="list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void Dispose<T>(this List<T> list) where T : IDisposable
        {
            foreach (T item in list)
            {
                item.Dispose();
            }
            list.Clear();
        }
        /// <summary>
        /// Degree the specified radian.
        /// </summary>
        /// <param name="radian">Radian.</param>
        public static float Degree(this float radian)
        {
            return radian * 180f / Mathf.PI;
        }
        /// <summary>
        /// Degree the specified radian.
        /// </summary>
        /// <param name="radian">Radian.</param>
        public static double Degree(this double radian)
        {
            return radian * 180 / Math.PI;
        }
        /// <summary>
        /// Radian the specified degree.
        /// </summary>
        /// <param name="degree">Degree.</param>
        public static float Radian(this float degree)
        {
            return degree / 180f * Mathf.PI;
        }
        /// <summary>
        /// Radian the specified degree.
        /// </summary>
        /// <param name="degree">Degree.</param>
        public static double Radian(this double degree)
        {
            return degree / 180 * Math.PI;
        }

        /// <summary>
        /// ORB the point feature.
        /// ORBで特徴点取得
        /// </summary>
        /// <param name="srcMat">Source mat.</param>
        /// <param name="dstKeypoints">Dst keypoints.</param>
        /// <param name="dstDescriptors">Dst descriptors.</param>
        public static void ORBPointFeature(this Mat srcMat, MatOfKeyPoint dstKeyPoints, Mat dstDescriptors)
        {
            ORBDetector.detect(srcMat, dstKeyPoints);
            ORBExtractor.compute(srcMat, dstKeyPoints, dstDescriptors);
        }

        /// <summary>
        /// Lows the pass filter.
        /// ローパスフィルターで誤差をカットしたマッチングの作成
        /// </summary>
        /// <returns>The pass filter.</returns>
        /// <param name="queryKeypoints">Query keypoints.</param>
        /// <param name="trainKeypoints">Train keypoints.</param>
        /// <param name="matches">Matches.</param>
        /// <param name = "Tolerance"></param>
        public static IList<DMatch> LowPassFilter(this IList<DMatch> matches, double Tolerance = 0)
        {
            var matchL = new List<DMatch>();
            var lengthL = new List<double>();
            {
                foreach (DMatch match in matches)
                    lengthL.Add(match.distance);

                IList<double> lengths = lengthL.ToArray();
                if (lengthL.Count > 0)
                {
                    double cutoff = lengthL.Median();
                    int i = 0;
                    foreach (DMatch match in matches)
                    {
                        if (lengths[i] < cutoff + cutoff * Tolerance)
                        {
                            matchL.Add(match);
                        }
                        i++;
                    }
                }
                IList<DMatch> buff = matchL.ToArray();
                matchL.Clear();
                lengthL.Clear();
                return buff;
            }
        }

		/// <summary>
		/// Resize the specified baseMat and rows.
		/// Matの変形
		/// </summary>
		/// <param name="baseMat">Base mat.</param>
		/// <param name="rows">Rows.</param>
        public static Mat resize(this Mat baseMat, int rows)
        {
            return baseMat.resize(rows, baseMat.cols());
        }
		/// <summary>
		/// Resize the specified baseMat, rows and cols.
        /// Matの変形
		/// </summary>
		/// <param name="baseMat">Base mat.</param>
		/// <param name="rows">Rows.</param>
		/// <param name="cols">Cols.</param>
        public static Mat resize(this Mat baseMat, int rows, int cols)
        {
            double[] zero = { 0 };

            if (rows <= 0 || cols <= 0)
                throw new OutLookARException(string.Format("Mat resize : 引数は0以上に設定してください.  rows : {0} ,cols : {1}", rows, cols));
            Mat reMat = new Mat(rows, cols, baseMat.type());
            for (int r = 0; r < reMat.rows(); r++)
            {
                for (int c = 0; c < reMat.cols(); c++)
                {
                    reMat.put(r, c, (baseMat.cols() < c && baseMat.rows() < r) ? baseMat.get(r, c) : zero);
                }
            }
            return reMat;
        }
		/// <summary>
		/// Add the specified baseMat and addMat.
        /// Matの追加
		/// </summary>
		/// <param name="baseMat">Base mat.</param>
		/// <param name="addMat">Add mat.</param>
        public static void Add(this Mat baseMat, Mat addMat)
        {
            if (!baseMat.empty() && baseMat.checkVector(addMat.channels(), addMat.depth()) < 0)
                throw new CvException("Incompatible Mat");
            if (baseMat.cols() != addMat.cols())
                throw new OutLookARException("Mat cols is not equal.");

            if (baseMat.cols() == 0)
            {
                baseMat.Dispose();
                baseMat = addMat.clone();
                return;
            }
            Mat newMat = baseMat.resize(baseMat.rows() + addMat.rows());

            for (int r = baseMat.rows(); r < baseMat.rows() + addMat.rows(); r++)
            {
                for (int c = 0; c < baseMat.cols(); c++)
                {
                    newMat.put(r, c, addMat.get(r - baseMat.rows(), c));
                }
            }
            baseMat.Dispose();
            baseMat = newMat;
        }
		/// <summary>
		/// Removes at baseMat and row.
        /// Matの削除
		/// </summary>
		/// <param name="baseMat">Base mat.</param>
		/// <param name="row">Row.</param>
        public static void RemoveAt(this Mat baseMat, int row)
        {
            for (int r = row; r < baseMat.rows() - 1; r++)
            {
                for (int c = 0; c < baseMat.cols(); c++)
                {
                    baseMat.put(r, c, baseMat.get(r + 1, c));
                }
            }
            baseMat.resize(baseMat.rows() - 1);
        }
		/// <summary>
		/// Tos the array mat.
        /// ArrayMatに変換
		/// </summary>
		/// <returns>The array mat.</returns>
		/// <param name="baseMat">Base mat.</param>
        public static ArrayMat ToArrayMat(this Mat baseMat)
        {
            double[] dm = new double[baseMat.rows() * baseMat.cols()];
            for (int r = 0; r < baseMat.rows(); r++)
            {
                for (int c = 0; c < baseMat.cols(); c++)
                {
                    dm[r * baseMat.cols() + c] = baseMat.get(r, c)[0];
                }
            }
            return new ArrayMat(baseMat.rows(), baseMat.cols(), dm);
        }
		/// <summary>
		/// Tos the mat.
        /// Matに変換
		/// </summary>
		/// <returns>The mat.</returns>
		/// <param name="baseArrayMat">Base array mat.</param>
        public static Mat ToMat(this ArrayMat baseArrayMat)
        {
            double[,] dm = baseArrayMat.ToArray;
            Mat m = new Mat(baseArrayMat.Rows, baseArrayMat.Cols, CvType.CV_64F);
            for (int r = 0; r < baseArrayMat.Rows; r++)
            {
                for (int c = 0; c < baseArrayMat.Cols; c++)
                {
                    m.put(r, c, dm[r, c]);
                }
            }
            return m;
        }
        
        public static string ToStringMat(this Mat baseMat)
        {
            string text = "{\n";
            for (int r = 0; r < baseMat.rows(); r++)
            {
                text += "[";
                for (int c = 0; c < baseMat.cols(); c++)
                {
                    text += baseMat.get(r, c)[0];
                    text += " ,";
                }
                text += "]\n";
            }
            text += "}";
            return text;
        }
    }
}