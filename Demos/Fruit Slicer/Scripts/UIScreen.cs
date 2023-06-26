using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Terresquall.FruitSlicer {
    public class UIScreen : MonoBehaviour
    {

        public bool pausesGame = false, fadeIn = false;
        public float sceneTransitionDuration = 1f;

        Image fadeOverlay;
        Canvas canvas;

        void Awake() {
            canvas = GetComponentInChildren<Canvas>();

            // Create the GameObject and Image component.
            GameObject go = new GameObject("Fader");
            RectTransform r = go.AddComponent<RectTransform>();
            fadeOverlay = go.AddComponent<Image>();

            // Make the Image cover the screen.
            r.SetParent(canvas.transform);
            r.anchorMax = new Vector2(1,1);
            r.anchorMin = new Vector2(0,0);
            r.offsetMin = new Vector2(0,0);
            r.offsetMax = new Vector2(0,0);

            fadeOverlay.gameObject.SetActive(false);
        }

        public void FadeIn(float duration)
        {
            StartCoroutine(FadeCoroutine(true, duration));
        }

        public void FadeOut(float duration)
        {
            StartCoroutine(FadeCoroutine(false, duration));
        }

        IEnumerator FadeCoroutine(bool fadeIn, float duration)
        {

            if(!canvas) yield break;

            fadeOverlay.gameObject.SetActive(true);

            // Fades the image in / out.
            WaitForEndOfFrame w = new WaitForEndOfFrame();
            float dur = Mathf.Max(0,duration);

            if(fadeIn)
            {
                fadeOverlay.color = new Color(0,0,0,0);
                while(dur > 0)
                {
                    yield return w;
                    dur -= Time.deltaTime;
                    fadeOverlay.color = new Color(0,0,0,dur/duration);
                }
            }
            else
            {
                fadeOverlay.color = new Color(0,0,0,1);
                while(dur > 0)
                {
                    yield return w;
                    dur -= Time.deltaTime;
                    fadeOverlay.color = new Color(0,0,0,1f - dur/duration);
                }
            }

            fadeOverlay.gameObject.SetActive(false);

            if(pausesGame)
            {
                Time.timeScale = 0f;
            }
        }


        public void Close()
        {
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
        }

        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        void OnEnable()
        {
            if(fadeIn) FadeIn(sceneTransitionDuration);
            else
            {
                if(pausesGame)
                {
                    Time.timeScale = 0f;
                }
            }
        }

        void OnDisable()
        {
            if(pausesGame)
            {
                Time.timeScale = 1f;
            }
        }
    }
}