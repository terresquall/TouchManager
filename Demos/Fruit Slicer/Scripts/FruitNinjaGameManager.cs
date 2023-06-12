﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Terresquall.FruitSlicer {
    [RequireComponent(typeof(Camera))]
    public class FruitNinjaGameManager : MonoBehaviour {

        public enum GameState
        {
            Menu,
            Game
        }
        public GameState fnScene;

        [Header("Game Stuff")]
        [SerializeField] GameObject skinSelector;
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject scoreHolder;
        [SerializeField] GameObject pauseScreen;
        [SerializeField] GameObject pauseIcon;
        [SerializeField] GameObject gameStuff;
        [SerializeField] GameObject menuStuff;

        public GameObject[] spawnedPrefabs;
        public float spawnInterval = 1.5f, intervalVariance = 1f;
        public Rect spawnArea;

        float currentSpawnCooldown;
        const float SPAWN_AREA_HEIGHT = 1f;

        

        //game stuff
        public int score;
        
        public TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI pauseScoreText;
        [SerializeField] TextMeshProUGUI highScoreText;

        void Start()
        {
            fnScene = GameState.Menu;
        }
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlayerPrefs.DeleteKey("HighScore");
            }
        }
        private void FixedUpdate()
        {
            if (fnScene == GameState.Game)
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

        //functions
        public void PlayGame()
        {
            ChangeState(GameState.Game);
            Score = 0;
            ResumeGame();
            highScoreText.text = "HighScore: " + PlayerPrefs.GetInt("HighScore", 0).ToString();
        }
        void SaveHighScore()
        {
            if(Score > PlayerPrefs.GetInt("HighScore", 0))
            {
                PlayerPrefs.SetInt("HighScore", Score);
                PlayerPrefs.Save();
            }
        }
        public void BackToMain()
        {
            SaveHighScore();
            ChangeState(GameState.Menu);
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
        void ChangeState(GameState _gameState)
        {
            switch (_gameState)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    GameObject[] _fruits = GameObject.FindGameObjectsWithTag("Fruit");
                    for (int i = 0; i < _fruits.Length; i++)
                    {
                        Destroy(_fruits[i]);
                    }
                    gameStuff.SetActive(false);
                    menuStuff.SetActive(true);
                    break;

                case GameState.Game:
                    Time.timeScale = 1f;                
                    menuStuff.SetActive(false);
                    gameStuff.SetActive(true);
                    break;
            }
        }
    }
}