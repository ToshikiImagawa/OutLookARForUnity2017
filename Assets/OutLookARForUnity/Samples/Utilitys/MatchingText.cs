/**
* 
*  You can not modify and use this source freely
*  only for the development of application related OutLookAR.
* 
* (c) OutLookAR All rights reserved.
* by Toshiki Imagawa
**/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OutLookAR.Test
{
    public class MatchingText : UIBehaviour
    {

        [SerializeField]
        Text text;
        [SerializeField]
        GameObject RollObject;
        [SerializeField]
        GameObject ValueRollObject;
        
        [SerializeField]
        float _spead = 5f;
        [SerializeField]
        bool _reverse = false;
        float startTime;
        
        int count = 0;

        public void ChangeText(int val){
            text.text = val.ToString();
            ValueRotation(val);
        }
        
        protected override void Start()
        {
            base.Start();
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
            var localRotation = RollObject.transform.localEulerAngles;
            localRotation.Set(localRotation.x, localRotation.y, pos);
            RollObject.transform.localEulerAngles = localRotation;
        }
        void ValueRotation(int val)
        {
            var pos = 360f*val/200f;
            var localRotation = ValueRollObject.transform.localEulerAngles;
            localRotation.Set(localRotation.x, localRotation.y, pos);
            ValueRollObject.transform.localEulerAngles = localRotation;
        }
    }
}