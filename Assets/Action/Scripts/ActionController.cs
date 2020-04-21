using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionController : MonoBehaviour {
    GameObject player;
    GameObject respawn;

    public static ActionController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        GameController.Instance.InitPlayer();
        GameController.Instance.InitCoins();
        GameController.Instance.InitHealth();
        player = GameObject.FindWithTag("Player");
        respawn = GameObject.FindWithTag("Respawn");
        MoveRespawn();
        GameController.Instance.DoStartGame(AudioController.Instance.mazeMusic);
    }
    public void MoveRespawn() {
        respawn.transform.position = player.transform.position;
    }
    public void Respawn() {
        GameController.Instance.DamagePlayer();
        player.SetActive(false);
        player.transform.position = respawn.transform.position;
        Invoke("EnablePlayer", 1f);
    }
    void EnablePlayer() {
        player.SetActive(true);
    }
}
