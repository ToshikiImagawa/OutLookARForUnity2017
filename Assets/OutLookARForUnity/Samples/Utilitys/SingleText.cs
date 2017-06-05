/**
* 
*  You can not modify and use this source freely
*  only for the development of application related OutLookAR.
* 
* (c) OutLookAR All rights reserved.
* by Toshiki Imagawa
**/
using UnityEngine.UI;
using System;
using UnityEngine;

namespace OutLookAR.Test
{
    public class SingleText : Text
    {
        public void ChangeText(Single valueText)
        {
            Debug.Log(valueText.ToString());
            text = valueText.ToString();
        }
    }
}