﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState { LEVEL_SELECT, GAME, PAUSED }
public enum SelectState { CUBES, LEVELS, HOW_TO, BOSS }
public class GameController : MonoBehaviour {
    /* 
     * Racing:      0
     * Gravity:     1
     * Action:      2
     */

    public GameState gameState;
    public SelectState selectState;
    public GameObject pauseUI;
    public GameObject startUI;
    Text countdownText;
    public int currentCube;
    public int[,] levelHowToBoss = new int[3, 2];
    public int[] levelUnlocks = new int[3];
    public int[] levelSelects = new int[3];
    public bool[] cubeCompletes = new bool[3];
    public string[] cubeNames = { "Racing", "Gravity", "Action" };
    /*
    public int[,] levelHowToBoss = new int[8, 2];
    public int[] levelUnlocks = new int[8];
    public int[] levelSelects = new int[8];
    public string[] cubeNames = {"Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "BallBounce", "Puzzle"};
    */
    public bool completedLevel;
    public float startTimer;
    public AudioClip startMusic;
    IEnumerator startGameCoroutine;

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
        //levelUnlocks = new int[] { 0, 0, 0, 0, 1, 0, 0, 0};
        pauseUI = GameObject.Find("PauseUI");
        pauseUI.SetActive(false);
        startUI = GameObject.Find("StartUI");
        countdownText = GameObject.Find("Countdown").GetComponent<Text>();
        startUI.SetActive(false);

        int scene = SceneManager.GetActiveScene().buildIndex;
        int cube = scene / 4;
        int level = scene % 4;
        if (scene == 0)
            return;
        if (cube > -1 && cube < 3) {
            switch (level) {
                case 1:
                    selectState = SelectState.HOW_TO;
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
            levelHowToBoss = new int[3, 2];
            levelSelects = new int[3];
            currentCube = 0;
            pauseUI.SetActive(false);
            SceneManager.LoadScene(0);
        }
        if (Input.GetButtonDown("Cancel")) {
            if (gameState == GameState.GAME) {
                if (selectState == SelectState.HOW_TO) {
                    gameState = GameState.LEVEL_SELECT;
                    SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
                    AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.selectBack);
                }
                else {
                    pauseUI.SetActive(true);
                    gameState = GameState.PAUSED;
                    AudioController.Instance.audioSound.Pause();
                    Time.timeScale = 0f;
                }
            }
            else {
                if (gameState == GameState.PAUSED) {
                    if (startTimer > 0) {
                        if (startGameCoroutine != null)
                            StopCoroutine(startGameCoroutine);
                        startGameCoroutine = StartGame(startMusic, startTimer);
                        StartCoroutine(startGameCoroutine);
                    }
                    else
                        Time.timeScale = 1f;
                    pauseUI.SetActive(false);
                    gameState = GameState.GAME;
                }
            }
        }
        if (Input.GetButtonDown("Action 2")) {
            if (gameState == GameState.PAUSED) {
                if (startGameCoroutine != null) {
                    StopCoroutine(startGameCoroutine);
                    startTimer = 0;
                    startMusic = null;
                    AudioController.Instance.audioSound.Stop();
                    countdownText.text = "3";
                    startUI.SetActive(false);
                }
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
                    if (levelUnlocks[currentCube] == 0) {
                        levelSelects[currentCube] = 1;
                        levelUnlocks[currentCube] = 1;
                    }
                    SceneManager.LoadScene(2 + (currentCube * 4));
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            if (gameState == GameState.PAUSED || gameState == GameState.LEVEL_SELECT)
                Application.Quit();
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
        startMusic = ac;
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.countdown);
        Time.timeScale = 0;

        if (startGameCoroutine != null)
            StopCoroutine(startGameCoroutine);
        startGameCoroutine = StartGame(ac, 0);
        StartCoroutine(startGameCoroutine);
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
        if (levelUnlocks[currentCube] == levelSelects[currentCube] && levelUnlocks[currentCube] < 3)
            levelUnlocks[currentCube]++;
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.winTune);
        AudioController.Instance.PlayMusic(AudioController.Instance.menuMusic);
        SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
    }
    // Resets level
    public void ResetLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    IEnumerator StartGame(AudioClip ac, float time) {
        AudioController.Instance.audioSound.UnPause();
        startUI.SetActive(true);
        startTimer = time;

        while (startTimer < 4) {
            if (gameState != GameState.PAUSED) {
                startTimer += Time.unscaledDeltaTime;
                if (startTimer >= 1) {
                    if (startTimer < 3)
                        countdownText.text = (3 - (int)startTimer) + "";
                    else
                        countdownText.text = "Go!";
                }
            }
            yield return null;
        }
        startTimer = 0;
        Time.timeScale = 1;
        countdownText.text = "3";
        startUI.SetActive(false);
        AudioController.Instance.PlayMusic(ac);
    }
}
