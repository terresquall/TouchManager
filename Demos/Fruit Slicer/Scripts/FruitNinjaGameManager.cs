using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Terresquall.FruitSlicer {
    //[RequireComponent(typeof(Camera))]
    public class FruitNinjaGameManager : MonoBehaviour {

        public enum GameState
        {
            Menu,
            Game
        }
        public GameState scene;
        AudioSource audio;
        [SerializeField] AudioClip[] audioClips;

        [Header("Game Stuff")]
        [SerializeField] UIScreen[] screens; // 0 is main menu, 1 is pause, 2 is lose, 3 is game, 4 is options
        [SerializeField] TextMeshProUGUI[] scoreTexts;
        bool gameOver = false;

        public GameObject[] spawnedPrefabs;

        public float spawnInterval = 1.5f, intervalVariance = 1f;
        public Rect spawnArea;

        public float currentSpawnCooldown;
        const float SPAWN_AREA_HEIGHT = 1f;

        //game stuff
        public int score;
        public int penalty;
        [SerializeField] GameObject[] crossFills;

        [Header("UI")]
        [SerializeField] Transform playButton;
        [SerializeField] GameObject startFruit;
        [SerializeField] Vector3 startFruitPos;
        public TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI pauseScoreText;
        [SerializeField] TextMeshProUGUI endScoreText;
        [SerializeField] TextMeshProUGUI highScoreText;

        public GameObject[] trailSkinPrefabs;
        int currentSkinIndex;

        [Header("Combos")]
        public float comboTimer = 1f;
        float comboCountDown;
        public int comboCounter;

        void Start()
        {
            // Set the skin to the last saved one.
            SetTrailSkin(PlayerPrefs.GetInt("CurrentSkinIndex", 0));

            // Start the audio clip.
            audio = GetComponent<AudioSource>();
            audio.clip = audioClips[0];
            audio.loop = true;
            audio.Play();
            scene = GameState.Menu;
        }

        public void SetTrailSkin(int index) {
            TouchManager.SetTrail(trailSkinPrefabs[currentSkinIndex = index]);
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlayerPrefs.DeleteKey("HighScore");
            }
        }
        public float spawnFruitVariance = 10;
        public float difficultyTimer;
        float difficultyTimerCountdown;
        private void FixedUpdate()
        {
            if (scene == GameState.Game && !gameOver)
            {
                SpawnFruit();
                spawnFruitVariance -= Time.deltaTime;

                difficultyTimerCountdown -= Time.deltaTime;
                if (difficultyTimerCountdown <= 0)
                {
                    DecreaseInterval();
                }
            }

            comboCountDown -= Time.deltaTime;
            if (comboCountDown <= 0)
            {
                comboCountDown = comboTimer;
                comboCounter = 0;
            }

            
        }
        void SpawnFruit()
        {

            if (currentSpawnCooldown > 0)
                currentSpawnCooldown -= Time.deltaTime;
            else
            {
                if (spawnFruitVariance > 0)
                {
                    currentSpawnCooldown += spawnInterval + Random.Range(0, intervalVariance);
                    Vector3 spawnPos = GetRandomSpawnPosition();

                    GameObject spawnedFruit = Instantiate(spawnedPrefabs[Random.Range(0, spawnedPrefabs.Length - 2)],
                        spawnPos, Quaternion.Euler(0, 0, 45f * spawnPos.x / (spawnArea.size.x / 2f))
                    );

                    // Add a random force to the spawned fruit
                    Rigidbody fruitRB = spawnedFruit.GetComponent<Rigidbody>();
                    fruitRB.AddForce(Vector3.up * 30, ForceMode.Impulse);
                }
                else
                {
                    StartCoroutine(SlowTime());
                    for (int i = 0; i < Random.Range(5, 8); i++)
                    {
                        Vector3 spawnPos = GetRandomSpawnPosition();

                        GameObject spawnedFruit = Instantiate(spawnedPrefabs[Random.Range(0, spawnedPrefabs.Length)],
                            spawnPos, Quaternion.Euler(0, 0, 45f * spawnPos.x / (spawnArea.size.x / 2f))
                        );

                        // Add a random force to the spawned fruit
                        Rigidbody fruitRB = spawnedFruit.GetComponent<Rigidbody>();
                        fruitRB.AddForce(Vector3.up * 25, ForceMode.Impulse);
                    }

                    spawnFruitVariance = Random.Range(8, 12);

                }

            }
        }
        IEnumerator SlowTime()
        {
            Time.timeScale = 0.7f;
            yield return new WaitForSeconds(1f);
            Time.timeScale = 1f;
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
            else if (Penalty >= 3)
            {
                crossFills[0].SetActive(true);
                crossFills[1].SetActive(true);
                crossFills[2].SetActive(true);
                StartCoroutine(LoseGame());
            }
        }
        public void StartGame()
        {
            //ChangeState(GameState.Menu);
            
            StartCoroutine(PlayGame());
        }

        //functions
        public IEnumerator PlayGame()
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < screens.Length; i++)
            {
                screens[i].Close();
            }
            screens[3].Open();
            gameOver = false;

            Score = 0;
            Penalty = 0;
            comboCounter = 0;
            UpdateCrosses();
            highScoreText.text = "HighScore: " + PlayerPrefs.GetInt("HighScore", 0).ToString();

            audio.Stop();
            audio.clip = audioClips[2];
            audio.loop = false;
            audio.Play();
            
            yield return new WaitForSeconds(3f);
            ChangeState(GameState.Game);
            difficultyTimer = 20f;
            spawnFruitVariance = 10f; 
        }
        void SaveHighScore()
        {
            if(Score > PlayerPrefs.GetInt("HighScore", 0))
            {
                PlayerPrefs.SetInt("HighScore", Score);
                PlayerPrefs.Save();
            }
        }

        public void ResetGame()
        {
            SaveHighScore();
            gameOver = false;
            ChangeState(GameState.Menu);
            scene = GameState.Menu;
        }

        public void PauseGame()
        {
            for (int i = 0; i < screens.Length; i++)
            {
                screens[i].Close();
            }
            screens[1].Open();
            pauseScoreText.text = Score.ToString();           
        }

        float timeElapsed = 0;
        public IEnumerator LoseGame()
        {
            Time.timeScale = 1f;
            gameOver = true;
            audio.Stop();
            audio.clip = audioClips[3];
            audio.loop = false;
            audio.Play();
            yield return new WaitForSeconds(1.1f);

            GameObject[] _fruits = GameObject.FindGameObjectsWithTag("Fruit");
            for (int i = 0; i < _fruits.Length; i++)
            {
                Destroy(_fruits[i]);
            }

            screens[2].Open();
            timeElapsed = 0;
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

        void ChangeState(GameState _gameState)
        {
            switch (_gameState)
            {
                case GameState.Menu:
                    for(int i = 0; i < screens.Length; i++)
                    {
                        screens[i].Close();
                    }
                    screens[0].Open();
                    Instantiate(startFruit, playButton);
                    Time.timeScale = 1f;

                    audio.Stop();
                    audio.clip = audioClips[0];
                    audio.loop = true;
                    audio.Play();

                    GameObject[] _fruits = GameObject.FindGameObjectsWithTag("Fruit");
                    for (int i = 0; i < _fruits.Length; i++)
                    {
                        Destroy(_fruits[i]);
                    }

                    break;

                case GameState.Game:
                    Time.timeScale = 1f;
                    
                    audio.Stop();
                    audio.clip = audioClips[1];
                    audio.loop = true;
                    audio.Play();

                    break;
            }
            scene = _gameState;
        }
        void DecreaseInterval()
        {
            difficultyTimerCountdown = difficultyTimer;
            spawnInterval -= 0.2f;
            intervalVariance -= 0.1f;
            Debug.Log("spawnInterval is" + spawnInterval);
            Debug.Log("internalVariance is " + intervalVariance);
        }
        public void AddCombo()
        {
            comboCountDown = comboTimer;
            comboCounter++;
        }
    }
}