/**
* 
*  You can not modify and use this source freely
*  only for the development of application related OutLookAR.
* 
* (c) OutLookAR All rights reserved.
* by Toshiki Imagawa
**/
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace OutLookAR.Test
{
    public class BarControllers : UIBehaviour
    {
        [SerializeField]
        float spring = 5f;
        [SerializeField]
        float resistance = 5f;
        Vector3 acceleration;
        Vector3 velocity;
        Vector3 pos;
        Quaternion buffRotation;

        float startTime;
        float moveTime;
        public void AddRotation(Quaternion val)
        {
            var euler = val.eulerAngles;
            var eulerBuff = buffRotation.eulerAngles;
            euler.x = (euler.x < 180) ? euler.x : euler.x - 360f;
            euler.y = (euler.y < 180) ? euler.y : euler.y - 360f;
            euler.z = (euler.z < 180) ? euler.z : euler.z - 360f;
            eulerBuff.x = (eulerBuff.x < 180) ? eulerBuff.x : eulerBuff.x - 360f;
            eulerBuff.y = (eulerBuff.y < 180) ? eulerBuff.y : eulerBuff.y - 360f;
            eulerBuff.z = (eulerBuff.z < 180) ? eulerBuff.z : eulerBuff.z - 360f;
            var distance = eulerBuff - euler;
            var timeDistance = Time.time - startTime;
            Debug.Log(distance);
            acceleration += distance / Mathf.Pow(timeDistance, 2f);
            buffRotation = val;
            startTime = Time.time;
        }
        protected override void Start()
        {
            base.Start();
            pos = Vector3.zero;
            acceleration = Vector3.zero;
            velocity = Vector3.zero;
            buffRotation = Quaternion.identity;
            startTime = Time.time;
            moveTime = Time.time;
        }

        void Update()
        {
            var timeDistance = Time.time - moveTime;
            acceleration -= spring * pos;
            acceleration -= resistance * acceleration * timeDistance;
            pos += acceleration * Mathf.Pow(timeDistance, 2f);
            moveTime = Time.time;
            gameObject.transform.localEulerAngles = pos;
        }

    }
}