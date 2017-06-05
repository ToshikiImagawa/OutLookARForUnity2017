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
    public class TitleText : UIBehaviour
    {
        [SerializeField]
        Text tex;
        
        protected override void Start (){
            base.Start();
            tex.text = Application.loadedLevelName;
        }
        
    }
}