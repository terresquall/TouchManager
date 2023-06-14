using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

namespace Terresquall.FruitSlicer {
    //[RequireComponent(typeof(Camera))]
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
        [SerializeField] GameObject loseScreen;
        [SerializeField] GameObject gameOverText;
        bool gameOver = false;

        public GameObject[] spawnedPrefabs;
        public float spawnInterval = 1.5f, intervalVariance = 1f;
        public Rect spawnArea;

        float currentSpawnCooldown;
        const float SPAWN_AREA_HEIGHT = 1f;

        

        //game stuff
        public int score;
        public int penalty;
        [SerializeField] GameObject[] crossFills;
        
        public TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI pauseScoreText;
        [SerializeField] TextMeshProUGUI endScoreText;
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
            if (fnScene == GameState.Game && !gameOver)
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
                Vector3 spawnPos = GetRandomSpawnPosition();

                GameObject spawnedFruit = Instantiate(spawnedPrefabs[Random.Range(0, spawnedPrefabs.Length)], 
                    spawnPos, Quaternion.Euler(0, 0, 45f * spawnPos.x / (spawnArea.size.x / 2f))
                );

                // Add a random force to the spawned fruit
                Rigidbody fruitRB = spawnedFruit.GetComponent<Rigidbody>();
                float randomForce = Random.Range(30, 30);
                fruitRB.AddForce(Vector3.up * randomForce, ForceMode.Impulse);
            }
        }

        public Vector3 GetRandomSpawnPosition()
        {
            float hw = spawnArea.size.x * 0.5f;
            float hh = spawnArea.size.y * 0.5f;
            Vector3 center = spawnArea.position;

            Vector3 spawnPos = new Vector3(
                Random.Range(-hw, hw),
                center.y, // Keep the Y position constant
                Random.Range(-hh, hh)
            );

            return center + spawnPos;
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
        public int Penalty
        {
            get { return penalty; }
            set
            {
                if (penalty != value)
                {
                    penalty = value;
                    UpdateCrosses();
                }
            }
        }
        public void UpdateCrosses()
        {
            if(Penalty == 0)
            {
                crossFills[0].SetActive(false);
                crossFills[1].SetActive(false);
                crossFills[2].SetActive(false);
            }
            else if(Penalty == 1)
            {
                crossFills[0].SetActive(true);
            }
            else if(Penalty == 2)
            {
                crossFills[0].SetActive(true);
                crossFills[1].SetActive(true);
            }
            else if (Penalty == 3)
            {
                crossFills[0].SetActive(true);
                crossFills[1].SetActive(true);
                crossFills[2].SetActive(true);
                StartCoroutine(LoseGame());
            }
        }

        //functions
        public void PlayGame()
        {
            
            gameOver = false;
            ChangeState(GameState.Game);
            loseScreen.SetActive(false);
            scoreHolder.SetActive(true);
            pauseIcon.SetActive(true);
            Score = 0;
            Penalty = 0;
            UpdateCrosses();
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
            gameOver = false;
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

        float timeElapsed = 0;
        IEnumerator LoseGame()
        {
            gameOver = true;
            scoreHolder.SetActive(false);
            pauseIcon.SetActive(false);
            pauseScreen.SetActive(false);

            GameObject[] _fruits = GameObject.FindGameObjectsWithTag("Fruit");
            for (int i = 0; i < _fruits.Length; i++)
            {
                Destroy(_fruits[i]);
            }

            gameOverText.SetActive(true);

            yield return new WaitForSeconds(1.4f);

            gameOverText.SetActive(false);
            loseScreen.SetActive(true);

            while (timeElapsed < 1f)
            {
                timeElapsed += Time.deltaTime;

                float t = Mathf.Clamp01(timeElapsed / 1f);
                float increasedValue = Mathf.Lerp(0, Score, t);

                int roundedValue = Mathf.RoundToInt(increasedValue);
                endScoreText.text = roundedValue.ToString();

                yield return null;
            }

            endScoreText.text = Score.ToString();
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
            fnScene = _gameState;
        }
    }
}