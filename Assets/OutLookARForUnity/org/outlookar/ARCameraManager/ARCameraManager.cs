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
using UnityEngine.UI;
using OpenCVForUnity;
using System;
using System.Collections.Generic;

namespace OutLookAR
{
    [RequireComponent(typeof(CaptureManager))]
    public class ARCameraManager : SingletonMonoBehaviour<ARCameraManager>
    {
        [SerializeField]
        Vector2 _lensDistortion;
        [SerializeField]
        float _diagonalAngle;
        [SerializeField]
        int _width;
        [SerializeField]
        int _height;
        float _diagonal;

        [SerializeField]
        Camera CameraObject;
        [SerializeField]
        Image ScreenObject;

        AspectRatioFitter arf;
        AspectRatioFitter ARF
        {
            get
            {
                if (arf == null)
                {
                    arf = ScreenObject.GetComponent<AspectRatioFitter>();
                    if (arf == null)
                    {
                        arf = ScreenObject.gameObject.AddComponent<AspectRatioFitter>();
                    }
                }
                return arf;
            }
        }
        /// <summary>
        /// Gets the diagonal angle.
        /// 対角画角
        /// </summary>
        /// <value>The diagonal angle.</value>
        public float DiagonalAngle { get { return _diagonalAngle; } set { _diagonalAngle = value; } }
        /// <summary>
        /// Gets the width.
        /// 幅
        /// </summary>
        /// <value>The width.</value>
        public int Width { get { return _width; } }
        /// <summary>
        /// Gets the height.
        /// 高さ
        /// </summary>
        /// <value>The height.</value>
        public int Height { get { return _height; } }
        /// <summary>
        /// Gets the diagonal.
        /// 対角長
        /// </summary>
        /// <value>The diagonal.</value>
        public float Diagonal { get { return _diagonal; } }
        /// <summary>
        /// Gets the length of the focal.
        /// 焦点距離
        /// </summary>
        /// <value>The length of the focal.</value>
        public float FocalLength
        {
            get { return Diagonal / 2f / Mathf.Atan((DiagonalAngle / 2f).Radian()); }
        }
        /// <summary>
        /// Gets the lens distortion.
        /// レンズ歪み
        /// </summary>
        /// <value>The lens distortion.</value>
        public Vector2 LensDistortion { get { return _lensDistortion; } }
        
        public void setLensDistortionX(float value){
            _lensDistortion = new Vector2(value,LensDistortion.y);
        }
        public void setLensDistortionY(float value){
            _lensDistortion = new Vector2(LensDistortion.x,value);
        }
        /// <summary>
        /// Init the specified width and height.
        /// カメラの画像サイズを初期化
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public void Init(int width, int height)
        {
            _width = width; _height = height;
            _diagonal = Mathf.Sqrt(Mathf.Pow((float)Width, 2) + Mathf.Pow((float)Height, 2));
        }
        /// <summary>
        /// Tos the vector.
        /// カメラパラメータと特徴点より方向ベクトルを作成.
        /// </summary>
        /// <returns>The vector.</returns>
        /// <param name="src">Source.</param>
        public Vector3 ToVector(Point src)
        {
            if (Width <= 0 || Height <= 0)
                throw new OutLookARException("カメラが初期化されていません.");
            if (src.x > Width || src.x < 0 || src.y > Height || src.y < 0)
            {
                throw new OutLookARException(string.Format("ポイントが画面外です。 X : {0}, Y : {1}", src.x, src.y));
            }
            Vector3 dst = new Vector3(((float)src.x - Width / 2) / (FocalLength * LensDistortion.x), (Height / 2 - (float)src.y) / (FocalLength * LensDistortion.y), 1f);
            return dst.normalized;
        }
        /// <summary>
        /// Tos the vector.
        /// カメラパラメータと特徴点より方向ベクトルを作成.
        /// </summary>
        /// <returns>The vector.</returns>
        /// <param name="src">Source.</param>
        public Vector3 ToVector(KeyPoint src)
        {
            return ToVector(src.pt);
        }
        /// <summary>
        /// To the point.
        /// 方向ベクトルより特徴点を作成.
        /// </summary>
        /// <returns>The point.</returns>
        /// <param name="src">Source.</param>
        /// <param name="camera">Camera.</param>
        public Point toPoint(Vector3 src, Quaternion posture)
        {
            return toPoint(Quaternion.Inverse(posture) * src);
        }
        /// <summary>
        /// To the point.
        /// 方向ベクトルより特徴点を作成.
        /// </summary>
        /// <returns>The point.</returns>
        /// <param name="src">Source.</param>
        public Point toPoint(Vector3 src)
        {
            if (Width <= 0 || Height <= 0)
                throw new OutLookARException("カメラが初期化されていません.");
            Point dst = new Point((double)(src.x * FocalLength * LensDistortion.x / src.z + Width / 2), (double)(-src.y * FocalLength * LensDistortion.y / src.z + Height / 2));
            if (dst.x > Width || dst.x < 0 || dst.y > Height || dst.y < 0)
            {
                throw new OutLookARException(string.Format("ポイントが画面外です。 X : {0}, Y : {1}", dst.x, dst.y));
            }
            return dst;
        }
        /// <summary>
        /// Updates the camera posture.
        /// カメラの姿勢を更新
        /// </summary>
        /// <param name="posture">Posture.</param>
        public void UpdateCameraPosture(Quaternion posture)
        {
            CameraObject.transform.rotation = posture;
            CameraObject.fieldOfView = DiagonalAngle * Height / Diagonal;
        }
        /// <summary>
        /// Updates the screen texture.
        /// スクリーンの画像を更新
        /// </summary>
        /// <param name="texture">Texture.</param>
        public void UpdateScreenTexture(Texture2D texture)
        {
            ARF.aspectRatio = ((float)texture.width) / texture.height;
            DestroyImmediate(ScreenObject.sprite);
            ScreenObject.sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), new Vector2(texture.width, texture.height), 1.0f);
        }
        /// <summary>
        /// LMedS.
        /// 中央値推定による姿勢推定
        /// </summary>
        /// <returns>The LMedS error.</returns>
        /// <param name="fromVectorL">From vector list.</param>
        /// <param name="toVectorL">To vector list.</param>
        /// <param name="attitude">Attitude.</param>
        /// <param name="confidence">Confidence.</param>
        /// <param name="outlier">Outlier.</param>
        public float LMedS(IList<Vector3> fromVectorL, IList<Vector3> toVectorL, out Quaternion attitude, float confidence = 0.99f, float outlier = 0.1f)
        {
            int VectorCount = fromVectorL.Count;
            if (VectorCount != toVectorL.Count)
            {
                throw new OutLookARException(string.Format("LMedS : fromVectorL.Count({0}) != toVectorL.Count({1}) ", VectorCount, toVectorL.Count));
            }
            if (VectorCount > 0)
            {
                System.Random cRand = new System.Random();
                attitude = Quaternion.identity;
                float MinError = 1000000f;
                double nBuff = Math.Log(1 - confidence, (1 - Math.Pow(1 - outlier, 2)));
                int n = Math.Abs(nBuff - (int)nBuff) > 0 ? (int)nBuff + 1 : (int)nBuff;
                if (fromVectorL.Count * (fromVectorL.Count - 1) / 2 < n)
                {
                    return -1f;
                }
                int i = 0;
                while (i < n)
                {
                    int pt1ID = cRand.Next(fromVectorL.Count);
                    int pt2ID = cRand.Next(toVectorL.Count);
                    if (pt1ID != pt2ID)
                    {
                        Vector3[] fromVectors = { fromVectorL[pt1ID], fromVectorL[pt2ID] };
                        Vector3[] toVectors = { toVectorL[pt1ID], toVectorL[pt2ID] };
                        Quaternion q = Utils.FromToLookRotation(fromVectors, toVectors);
                        List<float> errorL = new List<float>();
                        for (int j = 0; j < fromVectorL.Count; j++)
                        {
                            if (j != pt1ID && j != pt2ID)
                            {
                                Vector3 Estimation = q * fromVectorL[j];
                                errorL.Add(Mathf.Sqrt((Mathf.Pow(Estimation.x - toVectorL[j].x, 2f) + Mathf.Pow(Estimation.y - toVectorL[j].y, 2f) + Mathf.Pow(Estimation.z - toVectorL[j].z, 2))));
                            }
                        }
                        if (errorL.Median() < MinError)
                        {
                            MinError = errorL.Median();
                            attitude = q;
                        }
                        errorL.Clear();
                        i++;
                    }
                }
                return MinError;
            }
            attitude = Quaternion.identity;
            return -1f;
        }
    }
}