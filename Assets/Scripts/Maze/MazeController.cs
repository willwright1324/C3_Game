using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeController : MonoBehaviour {
    GameObject[] coins;
    GameObject player;
    GameObject door;
    GameObject respawn;
    GameObject light;
    Text coinScore;
    public int coinAmount;

    public static MazeController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        GameController.Instance.InitHealth();
        coins = GameObject.FindGameObjectsWithTag("Coin");
        player = GameObject.FindWithTag("Player");
        door = GameObject.FindWithTag("Door");
        respawn = GameObject.FindWithTag("Respawn");
        light = GameObject.Find("Spot Light");
        coinScore = GameObject.Find("CoinScore").GetComponent<Text>();
        coinAmount = coins.Length;
        coinScore.text = "Coins: 0 / " + coinAmount;
        InvokeRepeating("MoveRespawn", 0, 5f);
    }
    void MoveRespawn() {
        respawn.transform.position = player.transform.position;
    }
    public void Respawn() {
        GameController.Instance.DamagePlayer();
        light.transform.SetParent(null);
        player.SetActive(false);
        player.transform.position = respawn.transform.position;
        light.transform.position = new Vector3(respawn.transform.position.x, respawn.transform.position.y, light.transform.position.z);
        Invoke("EnablePlayer", 1f);
    }
    void EnablePlayer() {
        player.SetActive(true);
        light.transform.SetParent(player.transform);
    }
    public void CollectCoin() {
        coinAmount--;
        coinScore.text = "Coins: " + (coins.Length - coinAmount) + " / " + coins.Length;
    }
    public void OpenDoor() {
        if (coinAmount == 0) {
            Destroy(door);
        }
    }
}
