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
    public class RotationText : UIBehaviour
    {
        [SerializeField]
        Text[] texts;

        public void ChangeText(Quaternion val){
            texts[0].text = (Mathf.Round(val.eulerAngles.x*10)/10).ToString() + "°";
            texts[1].text = (Mathf.Round(val.eulerAngles.y*10)/10).ToString() + "°";
            texts[2].text = (Mathf.Round(val.eulerAngles.z*10)/10).ToString() + "°";
        }
    }
}