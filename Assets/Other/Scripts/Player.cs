﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Coin") {
            Destroy(collision.gameObject);
            GameController.Instance.CollectCoin();
        }
        if (collision.gameObject.tag == "PowerCube") {
            GameController.Instance.CompleteLevel();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Door") {
            GameController.Instance.OpenDoor();
        }
    }
}
