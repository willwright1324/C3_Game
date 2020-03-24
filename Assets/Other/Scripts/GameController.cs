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

    GameObject[] coins;
    GameObject player;
    GameObject door;
    GameObject respawn;
    Rigidbody2D rb;
    Transform[] playerHealth;
    Text coinScore;
    public int healthCount;
    public int coinAmount;

    public AudioSource audioSound;
    public AudioSource audioMusic;
    public AudioClip currentMusic;

    public AudioClip selectMove;
    public AudioClip selectConfirm;
    public AudioClip selectBack;
    public AudioClip cameraMove;
    public AudioClip countdown;

    public AudioClip playerCollect;
    public AudioClip playerHit;
    public AudioClip playerDeath;

    public AudioClip menuMusic;
    public AudioClip levelMusic;
    public AudioClip bossMusic;


    // Singleton
    private static GameController instance = null;
    public static GameController Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameController>();
                if (instance == null) {
                    GameObject gc = new GameObject();
                    gc.name = "GameController";
                    instance = gc.AddComponent<GameController>();
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
    private void Start() {
        UnityEngine.Cursor.visible = false;
        /*audioSound = GetComponent<AudioSource>();
        audioMusic = GameObject.Find("Music").GetComponent<AudioSource>();
        levelUnlocks = new int[] { 0, 0, 0, 0, 1, 0, 0, 0};
        pauseUI = GameObject.Find("PauseUI");
        pauseUI.SetActive(false);
        startUI = GameObject.Find("StartUI");
        startUI.SetActive(false);
        PlayMusic(menuMusic);*/
    }
    private void Update() {

        if (Input.GetKeyDown(KeyCode.R)) {
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
                PlayMusic(menuMusic);
                SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
            }
            else {
                if (gameState == GameState.GAME && selectState == SelectState.HOW_TO) {
                    levelHowToBoss[currentCube, 0] = 1;
                    gameState = GameState.LEVEL_SELECT;
                    PlayMusic(menuMusic);
                    SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
                }
            }
        }
    }
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
    public void DoStartGame() {
        PlayMusic(levelMusic);
        audioSound.PlayOneShot(countdown);
        Time.timeScale = 0;
        StartCoroutine(StartGame());
    }
    // Gives player full health
    public void ResetHealth() {
        healthCount = playerHealth.Length - 1;
        foreach (Transform h in playerHealth) {
            h.gameObject.SetActive(true);
        }
    }
    // Player takes damage
    public void DamagePlayer() {
        //audioSound.PlayOneShot(playerHit);
        healthCount--;
        if (healthCount <= 0)
            ResetLevel();
        else
            playerHealth[healthCount + 1].gameObject.SetActive(false);
    }
    // Player respawns
    public void RespawnPlayer() {
        audioSound.PlayOneShot(playerDeath);
        player.SetActive(false);
        player.transform.position = respawn.transform.position;
        Invoke("EnablePlayer", 1f);
    }
    void EnablePlayer() {
        player.SetActive(true);
    }
    // Player collects coin
    public void CollectCoin() {
        audioSound.PlayOneShot(playerCollect);
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
        gameState = GameState.LEVEL_SELECT;
        if (selectState == SelectState.BOSS) {
            selectState = SelectState.LEVELS;
        }
        PlayMusic(menuMusic);
        SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
    }
    // Resets level
    public void ResetLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void PlaySoundOnce(AudioClip ac) {
        if (!audioSound.isPlaying)
            audioSound.PlayOneShot(ac);
    }
    public void PlayMusic(AudioClip ac) {
        audioMusic.Stop();
        audioMusic.clip = ac;
        audioMusic.Play(0);
    }
    IEnumerator StartGame() {
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
    }
}
