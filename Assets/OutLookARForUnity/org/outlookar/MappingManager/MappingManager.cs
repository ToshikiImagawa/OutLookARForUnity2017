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
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SpicyPixel.Threading.Tasks;

namespace OutLookAR
{
    public class MappingManager : SingletonConcurrentBehaviour<MappingManager>
    {
        [SerializeField]
        Vector3 RotationError;
        int MaxMapSize = 100;
        [SerializeField]
        int _maxMapSize = 100;

        ArrayMat _varianceCovarianceMatrix;
        ArrayMat _stateMatrix;
        List<bool> InitList = new List<bool>();

        Mat _stateDescriptors;

        List<MMatch> MMatchList;

        bool InitFlag = false;

        public delegate void UpdateMap(Quaternion rotation, IList<Vector3> keyVectors, Mat Descriptors);
        public event UpdateMap OnUpdate;

        public Quaternion StateRotation { get { if (_stateMatrix.Rows < 4 || _stateMatrix.Cols != 1) throw new OutLookARException("StateMatrixが不正です."); return new Quaternion((float)_stateMatrix.At(0, 0), (float)_stateMatrix.At(1, 0), (float)_stateMatrix.At(2, 0), (float)_stateMatrix.At(3, 0)); } }

        void MappingTask(Quaternion u, float error, IDictionary<int, Vector3> z)
        {
            ArrayMat uStateMatrix = g(_stateMatrix, u);

            ArrayMat uVarianceCovarianceMatrix = _varianceCovarianceMatrix;
            uVarianceCovarianceMatrix.At(0, 0, error);
            uVarianceCovarianceMatrix.At(1, 1, error);
            uVarianceCovarianceMatrix.At(2, 2, error);
            uVarianceCovarianceMatrix.At(3, 3, error);

            var NoStateIDs = from i in MMatchList where i.StateID < 0 orderby i.MapID select i.MapID;
            int NoStateCount = 0;
            foreach (KeyValuePair<int, Vector3> j in z)
            {
                if (j.Key < 0)
                {
                    int MatID;
                    if (NoStateIDs.Count() >= NoStateCount)
                    {
                        var MaxStateIDs = from i in MMatchList
                                          where i.Error > 0
                                          orderby i.OutOfCount descending
                                          select new { i.MapID, i.Error } into countObje
                                          orderby countObje.Error descending
                                          select countObje.MapID;
                        MatID = MaxStateIDs.ElementAt(0);
                    }
                    else
                    {
                        MatID = NoStateIDs.ElementAt(NoStateCount);
                        NoStateCount++;
                    }
                    Vector3 newVector = u * j.Value;
                    uStateMatrix.At(MatID * 3 + 4, 0, newVector.x);
                    uStateMatrix.At(MatID * 3 + 4, 0, newVector.y);
                    uStateMatrix.At(MatID * 3 + 4, 0, newVector.z);
                }
                else
                {

                }
            }
        }

        void EKFTask(Quaternion u, float error, IDictionary<int, Vector3> z)
        {
            ArrayMat uStateMatrix = g(_stateMatrix, u);

            ArrayMat uVarianceCovarianceMatrix = _varianceCovarianceMatrix;
            uVarianceCovarianceMatrix.At(0, 0, error);
            uVarianceCovarianceMatrix.At(1, 1, error);
            uVarianceCovarianceMatrix.At(2, 2, error);
            uVarianceCovarianceMatrix.At(3, 3, error);

            foreach (KeyValuePair<int, Vector3> zit in z)
            {
                int ObservingKey = zit.Key;
                Vector3 ObservingValue = zit.Value;

                if (InitList[ObservingKey])
                {
                    Vector3 newVector = u * ObservingValue;
                    uStateMatrix.At(4 + 3 * ObservingKey, 0, (double)newVector.x);
                    uStateMatrix.At(5 + 3 * ObservingKey, 0, (double)newVector.y);
                    uStateMatrix.At(6 + 3 * ObservingKey, 0, (double)newVector.z);
                    uVarianceCovarianceMatrix.At(4 + 3 * ObservingKey, 4 + 3 * ObservingKey, (double)Mathf.Infinity);
                    uVarianceCovarianceMatrix.At(5 + 3 * ObservingKey, 5 + 3 * ObservingKey, (double)Mathf.Infinity);
                    uVarianceCovarianceMatrix.At(6 + 3 * ObservingKey, 6 + 3 * ObservingKey, (double)Mathf.Infinity);
                    InitList[ObservingKey] = false;
                }
                Vector3 j = new Vector3((float)uStateMatrix.At(4 + 3 * ObservingKey, 0), (float)uStateMatrix.At(5 + 3 * ObservingKey, 0), (float)uStateMatrix.At(6 + 3 * ObservingKey, 0));
                Vector3 hObservingValue = new Quaternion(-u.x, -u.y, -u.z, u.w) * j;
                ArrayMat hit = H(j, u) * FxJ(ObservingKey, MaxMapSize);
                ArrayMat kit = uVarianceCovarianceMatrix * hit.t() * (hit * uVarianceCovarianceMatrix * hit.t() + Q).inv();
                Vector3 Difference = ObservingValue - hObservingValue;
                ArrayMat DifferenceMat = new ArrayMat(3, 1);
                DifferenceMat.At(0, 0, Difference.x);
                DifferenceMat.At(1, 0, Difference.y);
                DifferenceMat.At(2, 0, Difference.z);
                uStateMatrix = uStateMatrix + kit * DifferenceMat;
            }
        }
        ArrayMat g(ArrayMat stateMatrix, Quaternion q)
        {
            if (_stateMatrix.Rows < 4 || _stateMatrix.Cols != 1)
                throw new OutLookARException("ArrayMat mu is not rows >= 4 & cols() = 1 .");
            stateMatrix.At(0, 0, q.x);
            stateMatrix.At(1, 0, q.y);
            stateMatrix.At(2, 0, q.z);
            stateMatrix.At(3, 0, q.w);
            return stateMatrix;
        }
        ArrayMat q;
        ArrayMat Q
        {
            get
            {
                if (q.Cols == 0 || q.Rows == 0)
                {
                    q = new ArrayMat(3, 3);
                    q.At(0, 0, (double)Mathf.Pow(RotationError.x, 2));
                    q.At(1, 1, (double)Mathf.Pow(RotationError.y, 2));
                    q.At(2, 2, (double)Mathf.Pow(RotationError.z, 2));
                }
                return q;
            }
        }
        ArrayMat r = new ArrayMat(4, 4);
        ArrayMat R(float error)
        {
            double aError = Math.Abs((double)error);
            r.At(0, 0, aError);
            r.At(1, 1, aError);
            r.At(2, 2, aError);
            r.At(3, 3, aError);
            return r;
        }
        ArrayMat Fx(int n)
        {
            if (n < 0)
            {
                throw new OutLookARException("引数が不正です。引数は0以上の整数である必要が有ります。");
            }
            return ArrayMat.eye(4, 4 + 3 * n);
        }
        ArrayMat FxT(int n)
        {
            if (n < 0)
            {
                throw new OutLookARException("引数が不正です。引数は0以上の整数である必要が有ります。");
            }
            return ArrayMat.eye(4 + 3 * n, 4);
        }
        ArrayMat FxJ(int j, int n)
        {
            if (j < 0 & n >= j)
            {
                throw new OutLookARException("引数が不正です。引数は0以上の整数である必要が有ります。またn>j。");
            }
            ArrayMat mj = new ArrayMat(7, 4 + 3 * n);
            mj.At(0, 0, 1);
            mj.At(1, 1, 1);
            mj.At(2, 2, 1);
            mj.At(3, 3, 1);

            mj.At(4, 3 * j + 1, 1);
            mj.At(5, 3 * j + 2, 1);
            mj.At(6, 3 * j + 3, 1);
            return mj;
        }

        ArrayMat H(Vector3 v, Quaternion q)
        {
            ArrayMat h = new ArrayMat(3, 7);

            //{{2 (qy Y + qz Z)}, {2 (qy X - 2 qx Y + qw Z)}, {2 qz X - 2 qw Y - 4 qx Z}}
            h.At(0, 0, (double)(2 * (q.y * v.y - q.z * v.z)));
            h.At(1, 0, (double)(2 * (q.y * v.x - 2 * q.x * v.y + q.w * v.z)));
            h.At(2, 0, (double)(2 * (q.z * v.x - q.w * v.y - 2 * q.x * v.z)));

            //{{-4 qy X + 2 qx Y - 2 qw Z}, {2 (qx X + qz Z)}, {-2 qw X + 2 qz Y - 4 qy Z}}
            h.At(0, 1, (double)(2 * (-2 * q.y * v.x + q.x * v.y - q.w * v.z)));
            h.At(1, 1, (double)(2 * (q.x * v.x + q.z * v.z)));
            h.At(2, 1, (double)(2 * (-q.w * v.x + q.z * v.y - 2 * q.y * v.z)));

            //{{2 (-2 qz X + qw Y + qx Z)}, {-2 qw X - 4 qz Y + 2 qy Z}, {2 (qx X + qy Y)}}
            h.At(0, 2, (double)(2 * (-2 * q.z * v.x + q.w * v.y + q.x * v.z)));
            h.At(1, 2, (double)(2 * (-q.w * v.x - 2 * q.z * v.y + q.y * v.z)));
            h.At(2, 2, (double)(2 * (q.x * v.x + q.y * v.y)));

            //{{2 qz Y - 2 qy Z}, {-2 qz X + 2 qx Z}, {-2 (qy X + qx Y)}}
            h.At(0, 3, (double)(2 * (q.z * v.y - q.y * v.z)));
            h.At(1, 3, (double)(2 * (-q.z * v.x + q.x * v.z)));
            h.At(2, 3, (double)(-2 * (q.y * v.x + q.x * v.y)));

            //{{1 - 2 qy^2 - 2 qz^2}, {2 qx qy - 2 qz qw}, {2 qx qz - 2 qy qw}}
            h.At(0, 4, (double)(1 - 2 * Mathf.Pow(q.y, 2f) - 2 * Mathf.Pow(q.z, 2f)));
            h.At(1, 4, (double)(2 * (q.x * q.y - q.z * q.w)));
            h.At(2, 4, (double)(2 * (q.x * q.z - q.y * q.w)));

            //{{2 (qx qy + qz qw)}, {1 - 2 qx^2 - 2 qz^2}, {2 qy qz - 2 qx qw}}
            h.At(0, 5, (double)(2 * (q.x * q.y - q.z * q.w)));
            h.At(1, 5, (double)(1 - 2 * Mathf.Pow(q.x, 2f) - 2 * Mathf.Pow(q.z, 2f)));
            h.At(2, 5, (double)(2 * (q.y * q.z - q.x * q.w)));

            //{{2 qx qz - 2 qy qw}, {2 (qy qz + qx qw)}, {1 - 2 qx^2 - 2 qy^2}}
            h.At(0, 6, (double)(2 * (q.x * q.z - q.y * q.w)));
            h.At(1, 6, (double)(2 * (q.y * q.z + q.x * q.w)));
            h.At(2, 6, (double)(1 - 2 * Mathf.Pow(q.x, 2f) - 2 * Mathf.Pow(q.y, 2f)));

            return h;
        }

        // Use this for initialization
        void Start()
        {
            Thread t = new Thread(Init);
            t.Start();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Init()
        {
            MaxMapSize = _maxMapSize;
            MMatchList.Clear();
            for (int i = 0; i < MaxMapSize; i++)
            {
                MMatchList.Add(new MMatch(i));
                InitList.Add(true);
            }
            _varianceCovarianceMatrix = new ArrayMat(4 + 3 * MaxMapSize, 4 + 3 * MaxMapSize);
            _stateMatrix = new ArrayMat(4 + 3 * MaxMapSize, 1);
            InitFlag = true;
        }

        public struct MMatch
        {
            public int MapID;
            public int StateID;
            public float Error;
            public int OutOfCount;
            public MMatch(int id)
            {
                MapID = id;
                StateID = -1;
                Error = Mathf.Infinity;
                OutOfCount = -1;
            }
        }
    }
}