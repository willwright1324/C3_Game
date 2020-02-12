using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeController : MonoBehaviour {
    GameObject[] coins;
    GameObject door;
    Text coinScore;
    public int coinAmount;

    public static MazeController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        coins = GameObject.FindGameObjectsWithTag("Coin");
        door = GameObject.FindWithTag("Door");
        coinScore = GameObject.Find("CoinScore").GetComponent<Text>();
        coinAmount = coins.Length;
        coinScore.text = "Coins: 0 / " + coinAmount;
    }
    // Update is called once per frame
    void Update() {
        
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
