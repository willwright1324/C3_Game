using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState { LEVEL_SELECT, GAME, PAUSED }
public enum SelectState { CUBES, LEVELS, HOW_TO, BOSS }
public class GameController : MonoBehaviour {
    /* 
     * Racing:      0
     * Shooter:     1
     * Rhythm:      2
     * Platformer:  3
     * Gravity:     4
     * Maze:        5
     * Ball Bounce: 6
     * Puzzle:      7
     */

    public GameState gameState;
    public SelectState selectState;
    public GameObject pauseUI;
    public GameObject startUI;
    public int currentCube;
    public int[,] levelHowToBoss = new int[8, 2];
    public int[] levelUnlocks = new int[8];
    public int[] levelSelects = new int[8];
    public string[] cubeNames = {"Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "BallBounce", "Puzzle"};
    public bool completedLevel;

    GameObject[] coins;
    GameObject player;
    GameObject door;
    GameObject respawn;
    Rigidbody2D rb;
    Transform[] playerHealth;
    Text coinScore;
    public int healthCount;
    public int coinAmount;

    // Singleton
    private static GameController instance = null;
    public static GameController Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameController>();
                if (instance == null) {
                    GameObject gc = Resources.Load("General/GameController") as GameObject;
                    gc = Instantiate(gc);
                    instance = gc.GetComponent<GameController>();
                    DontDestroyOnLoad(gc);
                }
            }
            return instance;
        }
    }
    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }
    }
    private void OnEnable() {
        UnityEngine.Cursor.visible = false;
        levelUnlocks = new int[] { 0, 0, 0, 0, 1, 0, 0, 0};
        pauseUI = GameObject.Find("PauseUI");
        pauseUI.SetActive(false);
        startUI = GameObject.Find("StartUI");
        startUI.SetActive(false);

        int scene = SceneManager.GetActiveScene().buildIndex;
        int cube = scene / 6;
        int level = scene % 6;
        if (scene == 0)
            return;
        if (cube > -1 && cube < 8) {
            switch (level) {
                case 1:
                    selectState = SelectState.HOW_TO;
                    break;
                case 6:
                    selectState = SelectState.BOSS;
                    break;
                default:
                    selectState = SelectState.LEVELS;
                    break;
            }
            gameState = GameState.GAME;
        }
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            AudioController.Instance.audioMusic.Stop();
            gameState = GameState.LEVEL_SELECT;
            selectState = SelectState.CUBES;
            levelHowToBoss = new int[8, 2];
            levelSelects = new int[8];
            currentCube = 0;
            pauseUI.SetActive(false);
            SceneManager.LoadScene(0);
        }
        if (Input.GetButtonDown("Cancel")) {
            if (gameState == GameState.GAME) {
                pauseUI.SetActive(true);
                gameState = GameState.PAUSED;
                Time.timeScale = 0f;
            }
            else {
                if (gameState == GameState.PAUSED) {
                    Time.timeScale = 1f;
                    pauseUI.SetActive(false);
                    gameState = GameState.GAME;
                }
            }
        }
        if (Input.GetButtonDown("Action 2")) {
            if (gameState == GameState.PAUSED) {
                Time.timeScale = 1f;
                pauseUI.SetActive(false);
                gameState = GameState.LEVEL_SELECT;
                if (selectState == SelectState.BOSS) {
                    selectState = SelectState.LEVELS;
                }
                AudioController.Instance.PlayMusic(AudioController.Instance.menuMusic);
                SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
            }
            else {
                if (gameState == GameState.GAME && selectState == SelectState.HOW_TO) {
                    levelHowToBoss[currentCube, 0] = 1;
                    gameState = GameState.GAME;
                    selectState = SelectState.LEVELS;
                    SceneManager.LoadScene(2 + (currentCube * 6) + levelSelects[currentCube]);
                }
            }
        }
    }
    public void Init() {}
    // Initializes player and respawn when needed
    public void InitPlayer() {
        player = GameObject.FindWithTag("Player");
        respawn = GameObject.FindWithTag("Respawn");
        respawn.transform.position = player.transform.position;
    }
    // Initializes health when needed
    public void InitHealth() {
        playerHealth = GameObject.Find("PlayerHealth").GetComponentsInChildren<Transform>();
        healthCount = playerHealth.Length - 1;
    }
    // Initializes coins and door when needed
    public void InitCoins() {
        coins = GameObject.FindGameObjectsWithTag("Coin");
        door = GameObject.FindWithTag("Door");
        coinScore = GameObject.Find("CoinScore").GetComponent<Text>();
        coinAmount = coins.Length;
        coinScore.text = "Coins: 0 / " + coinAmount;
    }
    // Countdown for game
    public void DoStartGame(AudioClip ac) {
        if (AudioController.Instance.audioMusic.isPlaying)
            AudioController.Instance.audioMusic.Stop();
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.countdown);
        Time.timeScale = 0;
        StartCoroutine(StartGame(ac));
    }
    // Gives player full health
    public void ResetHealth() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.healthReset);
        healthCount = playerHealth.Length - 1;
        foreach (Transform h in playerHealth) {
            h.gameObject.SetActive(true);
        }
    }
    // Player takes damage
    public void DamagePlayer() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerDamage);
        healthCount--;
        if (healthCount <= 0)
            ResetLevel();
        else
            playerHealth[healthCount + 1].gameObject.SetActive(false);
    }
    // Player respawns
    public void RespawnPlayer() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerDeath);
        player.SetActive(false);
        player.transform.position = respawn.transform.position;
        Invoke("EnablePlayer", 1f);
    }
    void EnablePlayer() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.respawn);
        player.SetActive(true);
    }
    // Player collects coin
    public void CollectCoin() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerCollect);
        coinAmount--;
        coinScore.text = "Coins: " + (coins.Length - coinAmount) + " / " + coins.Length;
    }
    // Player opens door
    public void OpenDoor() {
        if (coinAmount == 0) {
            Destroy(door);
        }
    }
    // Player completes level
    public void CompleteLevel() {
        completedLevel = true;
        gameState = GameState.LEVEL_SELECT;
        if (selectState == SelectState.BOSS) {
            selectState = SelectState.LEVELS;
        }
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.winTune);
        AudioController.Instance.PlayMusic(AudioController.Instance.menuMusic);
        SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
    }
    // Resets level
    public void ResetLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    IEnumerator StartGame(AudioClip ac) {
        startUI.SetActive(true);
        Text countdownText = GameObject.Find("Countdown").GetComponent<Text>();
        float timer = 0;
        while (timer < 4) {
            timer += Time.unscaledDeltaTime;
            if (timer >= 1) {
                if (timer < 3)
                    countdownText.text = (3 - (int)timer) + "";
                else
                    countdownText.text = "Go!";
            }
            yield return null;
        }
        Time.timeScale = 1;
        countdownText.text = "3";
        startUI.SetActive(false);
        AudioController.Instance.PlayMusic(ac);
    }
}
