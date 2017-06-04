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

namespace OutLookAR
{
    [RequireComponent(typeof(CaptureManager))]
    public class ARCameraManager : SingletonMonoBehaviour<ARCameraManager>
    {

        [SerializeField]
        float _diagonalAngle;
        [SerializeField]
        int _width;
        [SerializeField]
        int _height;

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
        public float DiagonalAngle { get { return _diagonalAngle; } }
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
        public float Diagonal { get { return Mathf.Sqrt(Mathf.Pow((float)Width, 2) + Mathf.Pow((float)Height, 2)); } }
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
        /// Init the specified width and height.
		/// カメラの画像サイズを初期化
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public void Init(int width, int height)
        {
            _width = width; _height = height;
        }
        /// <summary>
        /// カメラパラメータと特徴点より方向ベクトルを作成.
        /// </summary>
        /// <returns>The point.</returns>
        /// <param name="src">Source.</param>
        /// <param name="camera">Camera.</param>
        public Vector3 ToVector(Point src)
        {
            if (Width <= 0 || Height <= 0)
                throw new OutLookARException("カメラが初期化されていません.");
            if (src.x > Width || src.x < 0 || src.y > Height || src.y < 0)
            {
                throw new OutLookARException(string.Format("ポイントが画面外です。 X : {0}, Y : {1}", src.x, src.y));
            }
            Vector3 dst;
            Point pointP = new Point(src.x - Width / 2, Height / 2 - src.y);
            float PointL = (float)Math.Sqrt(Math.Pow(pointP.x, 2) + Math.Pow(pointP.y, 2) + Math.Pow(FocalLength, 2));
            dst.x = (float)pointP.x / PointL;
            dst.y = (float)pointP.y / PointL;
            dst.z = FocalLength / PointL;
            return dst;
        }
        /// <summary>
        /// カメラと特徴点より方向ベクトルを作成.
        /// </summary>
        /// <returns>The point.</returns>
        /// <param name="src">Source.</param>
        /// <param name="camera">Camera.</param>
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
        public Point ToPoint(Vector3 src, Quaternion posture)
        {
            return ToPoint(Quaternion.Inverse(posture) * src);
        }
        /// <summary>
        /// To the point.
        /// 方向ベクトルより特徴点を作成.
        /// </summary>
        /// <returns>The point.</returns>
        /// <param name="src">Source.</param>
        public Point ToPoint(Vector3 src)
        {
            if (Width <= 0 || Height <= 0)
                throw new OutLookARException("カメラが初期化されていません.");
            float ratio = FocalLength / src.z;
            Point dst = new Point((double)src.x / ratio, (double)src.y / ratio);
            dst.x += Width / 2;
            dst.y += Height / 2;
            if (dst.x > Width || dst.x < 0 || dst.y > Height || dst.y < 0)
            {
                throw new OutLookARException(string.Format("ポイントが画面外です。 X : {0}, Y : {1}", dst.x, dst.y));
            }
            return dst;
        }

        public void UpdateCameraPosture(Quaternion posture)
        {
            CameraObject.transform.localRotation = posture;
        }
        public void UpdateTexture(Texture2D texture)
        {
            ARF.aspectRatio = ((float)texture.width) / texture.height;
            ScreenObject.sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), new Vector2(texture.width, texture.height), 1.0f);
        }
    }
}