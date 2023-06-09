using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Terresquall.FruitSlicer
{
    public class SkinSelector : MonoBehaviour
    {
        public int skinIndex;

        FruitNinjaGameManager fruitNinjaGameManager;
        // Start is called before the first frame update
        void Start()
        {
            fruitNinjaGameManager = FindObjectOfType<FruitNinjaGameManager>();
        }
        public void ChangeSkin()
        {
            fruitNinjaGameManager.skinIndex = skinIndex;
            PlayerPrefs.SetInt("CurrentSkinIndex", skinIndex);
            PlayerPrefs.Save();
        }
    }
}
