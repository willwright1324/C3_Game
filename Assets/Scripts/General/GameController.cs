using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState { LEVEL_SELECT, GAME, PAUSED }
public enum SelectState { CUBES, LEVELS, HOW_TO }
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
    public int[] levelUnlocks = new int[8];
    public int[] levelSelects = new int[8];
    public string[] cubeNames = {"Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "Ball Bounce", "Puzzle"};
    
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
        pauseUI = GameObject.Find("PauseUI");
        pauseUI.SetActive(false);
    }
    private void Update() {
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
                SceneManager.LoadScene(0);
            }
        }
    }
    // Player completes a level
    public void CompleteLevel() {
        gameState = GameState.LEVEL_SELECT;
        SceneManager.LoadScene(0);
    }
    // Reset the level
    public void ResetLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
