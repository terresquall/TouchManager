using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Terresquall.FruitSlicer
{
    public class SkinSelector : MonoBehaviour
    {
        public int skinIndex;

        TouchManager touchManager;
        // Start is called before the first frame update
        void Start()
        {
            touchManager = FindObjectOfType<TouchManager>();
        }
        public void ChangeSkin()
        {
            touchManager.skinIndex = skinIndex;
            PlayerPrefs.SetInt("CurrentSkinIndex", skinIndex);
            PlayerPrefs.Save();
        }
    }
}
