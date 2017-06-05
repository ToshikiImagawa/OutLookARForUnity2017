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
    public class GearRotation : UIBehaviour
    {
#if UNITY_EDITOR
        [SerializeField]
        bool _debug = false;
#endif
        [SerializeField]
        float _spead = 5f;
        [SerializeField]
        bool _reverse = false;
        float startTime;
        protected override void Start()
        {
            startTime = Time.time;
        }

        void Update()
        {
            Rotation();
        }

        void Rotation()
        {
            float distance = (Time.time - startTime) % _spead;
            var pos = (!_reverse) ? 360f * distance / _spead : 360f * (1f - distance / _spead);
#if UNITY_EDITOR
            if (_debug)
                Debug.Log(pos);
#endif
            var localRotation = gameObject.transform.localEulerAngles;
            localRotation.Set(localRotation.x, localRotation.y, pos);
            gameObject.transform.localEulerAngles = localRotation;
        }
    }
}