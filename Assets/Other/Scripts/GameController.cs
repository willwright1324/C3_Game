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
    public int currentCube;
    public int[,] levelHowToBoss = new int[8, 2];
    public int[] levelUnlocks = new int[8];
    public int[] levelSelects = new int[8];
    public string[] cubeNames = {"Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "BallBounce", "Puzzle"};

    Transform[] playerHealth;
    public int healthCount;

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
        levelUnlocks = new int[]{ 0, 0, 0, 0, 1, 0, 0, 0};
        pauseUI = GameObject.Find("PauseUI");
        pauseUI.SetActive(false);
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
                SceneManager.LoadScene(0);
            }
            else {
                if (gameState == GameState.GAME && selectState == SelectState.HOW_TO) {
                    levelHowToBoss[currentCube, 0] = 1;
                    gameState = GameState.LEVEL_SELECT;
                    SceneManager.LoadScene(0);
                }
            }
        }
    }
    // Initializes health when needed
    public void InitHealth() {
        playerHealth = GameObject.Find("PlayerHealth").GetComponentsInChildren<Transform>();
        healthCount = playerHealth.Length - 1;
    }
    // Player takes damage
    public void DamagePlayer() {
        healthCount--;
        if (healthCount <= 0)
            ResetLevel();
        else
            Destroy(playerHealth[healthCount + 1].gameObject);
    }
    // Player completes a level
    public void CompleteLevel() {
        gameState = GameState.LEVEL_SELECT;
        if (selectState == SelectState.BOSS) {
            selectState = SelectState.LEVELS;
        }
        SceneManager.LoadScene(0);
    }
    // Reset the level
    public void ResetLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
