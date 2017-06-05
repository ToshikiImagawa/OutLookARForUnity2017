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

namespace OutLookAR.Test
{
    public class MenuUI : UIBehaviour
    {
        [SerializeField]
        MatchingText Matching;
        [SerializeField]
        RotationText Rotation;
        [SerializeField]
        BarControllers LeftObject;
        [SerializeField]
        BarControllers RightObject;

        public void OnRotation(Quaternion val)
        {
            if (Rotation != null) Rotation.ChangeText(val);
            if (LeftObject != null) LeftObject.AddRotation(val);
            if (RightObject != null) RightObject.AddRotation(val);
        }

        public void OnMatching(int val)
        {
            if (Matching != null) Matching.ChangeText(val);
        }
        
        public void OnChangeCamera(){
            CaptureManager.Instance.DevicesChange();
            TrackerManager.Instance.Init();
        }
    }
}