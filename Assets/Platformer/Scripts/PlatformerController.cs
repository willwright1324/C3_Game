using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformerController : MonoBehaviour {

    public static PlatformerController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        GameController.Instance.InitPlayer();
        GameController.Instance.InitCoins();
        GameController.Instance.InitHealth();
        GameController.Instance.DoStartGame();
    }
}
