using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeController : MonoBehaviour {
    GameObject player;
    GameObject respawn;
    GameObject spotlight;

    public static MazeController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        GameController.Instance.InitPlayer();
        GameController.Instance.InitCoins();
        GameController.Instance.InitHealth();
        player = GameObject.FindWithTag("Player");
        respawn = GameObject.FindWithTag("Respawn");
        spotlight = GameObject.Find("Spot Light");
        InvokeRepeating("MoveRespawn", 0, 5f);
        GameController.Instance.DoStartGame(AudioController.Instance.mazeMusic);
    }
    void MoveRespawn() {
        respawn.transform.position = player.transform.position;
    }
    public void Respawn() {
        GameController.Instance.DamagePlayer();
        spotlight.transform.SetParent(null);
        player.SetActive(false);
        player.transform.position = respawn.transform.position;
        Invoke("EnablePlayer", 1f);
    }
    void EnablePlayer() {
        player.SetActive(true);
        spotlight.transform.position = new Vector3(respawn.transform.position.x, respawn.transform.position.y, spotlight.transform.position.z);
        spotlight.transform.SetParent(player.transform);
    }
}
