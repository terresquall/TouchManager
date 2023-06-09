using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Terresquall.FruitSlicer {
    [RequireComponent(typeof(Camera))]
    public class FruitNinjaGameManager : MonoBehaviour {

        public enum Scene
        {
            Menu,
            Game
        }
        public Scene fnScene;

        [Header("Game Stuff")]
        [SerializeField] GameObject skinSelector;
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject scoreHolder;
        [SerializeField] GameObject pauseScreen;
        [SerializeField] GameObject pauseIcon;

        public GameObject[] spawnedPrefabs;
        public float spawnInterval = 1.5f, intervalVariance = 1f;
        public Rect spawnArea;

        float currentSpawnCooldown;
        const float SPAWN_AREA_HEIGHT = 1f;

        public GameObject[] skins;
        public int skinIndex;

        //game stuff
        public int score;
        
        public TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI pauseScoreText;

        void Start()
        {
            skinIndex = PlayerPrefs.GetInt("CurrentSkinIndex", 0);
        }
        private void FixedUpdate()
        {
            if (fnScene == Scene.Game)
            {
                SpawnFruit();
            }
        }
        void SpawnFruit()
        {
            if (currentSpawnCooldown > 0)
                currentSpawnCooldown -= Time.deltaTime;
            else
            {
                currentSpawnCooldown += spawnInterval + Random.Range(0, intervalVariance);
                Vector2 spawnPos = GetRandomSpawnPosition();
                Instantiate(
                    spawnedPrefabs[Random.Range(0, spawnedPrefabs.Length)],
                    (Vector2)transform.position + spawnPos,
                    Quaternion.Euler(0, 0, 45f * spawnPos.x / (spawnArea.size.x / 2f))
                );
            }
        }

        // Generates a random spawn position centred around the origin.
        public Vector2 GetRandomSpawnPosition() {
            float hw = spawnArea.size.x * 0.5f, hh = spawnArea.size.y * 0.5f;
            return spawnArea.position + new Vector2(
                Random.Range(-hw, hw),
                Random.Range(-hh, hh)
            );
        }

        void OnDrawGizmosSelected() {
            if(spawnArea.size.sqrMagnitude > 0) {
                // Draw the lines of the bounds.
                Gizmos.color = Color.cyan;

                Vector2 pos = (Vector2)transform.position + spawnArea.position - spawnArea.size * 0.5f,
                        size = spawnArea.size;

                // Get the 4 points in the bounds.
                Vector3 a = new Vector3(pos.x, pos.y),
                        b = new Vector3(pos.x, pos.y + size.y),
                        c = new Vector2(pos.x + size.x, pos.y + size.y),
                        d = new Vector3(pos.x + size.x, pos.y);
                
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(c, d);
                Gizmos.DrawLine(d, a);
            }
        }

        void Reset() {
            Camera camera = GetComponent<Camera>();
            spawnArea.y = -camera.orthographicSize - SPAWN_AREA_HEIGHT * 0.5f;
            spawnArea.size = new Vector2(camera.orthographicSize * 2f * camera.aspect, SPAWN_AREA_HEIGHT);
        }

        public int Score
        {
            get { return score; }
            set
            {
                if (score != value)
                {
                    score = value;
                    scoreText.text = score.ToString();
                }
            }
        }

        //menu stuff
        public void PlayGame()
        {
            SceneManager.LoadScene("Game");
        }
        public void BackToMain()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Menu");
        }
        public void PauseGame()
        {
            scoreHolder.SetActive(false);
            pauseIcon.SetActive(false);
            pauseScreen.SetActive(true);
            pauseScoreText.text = Score.ToString();
            Time.timeScale = 0f;
        }
        public void ResumeGame()
        {           
            pauseScreen.SetActive(false);
            scoreHolder.SetActive(true);
            pauseIcon.SetActive(true);
            Time.timeScale = 1f;
        }

        public void MenuToggle()
        {
            skinSelector.SetActive(false);
            mainMenu.SetActive(true);
        }
        public void SkinSelectToggle()
        {
            mainMenu.SetActive(false);
            skinSelector.SetActive(true);           
        }
    }
}