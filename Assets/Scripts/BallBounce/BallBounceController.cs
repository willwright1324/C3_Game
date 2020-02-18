﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BallBounceController : MonoBehaviour {
    public int level = 1;
    public float blockSpace = 2;
    GameObject ball;
    GameObject player;
    GameObject bounds;
    GameObject temps;
    GameObject blocks;
    GameObject sBlocks;
    public Transform[] playerHealth;
    Rigidbody2D rb;
    public bool paddleL;
    public bool paddleR;
    bool getComponents;
    public int healthCount;
    public int blockCount;
    public float ballAccel = 50;
    public float ballSpeedCap;
    public float ballSpeedMin = 50;
    public float ballSpeedMax = 400;

    public static BallBounceController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        ball = GameObject.Find("Ball");
        player = GameObject.FindWithTag("Player");
        bounds = GameObject.Find("Bounds");
        playerHealth = GameObject.Find("PlayerHealth").GetComponentsInChildren<Transform>();
        healthCount = playerHealth.Length - 1;
        rb = ball.GetComponent<Rigidbody2D>();

        Bounds blockSize = (Resources.Load("BallBounce/Block") as GameObject).GetComponent<Renderer>().bounds;

        StreamReader sr = new StreamReader("Assets/Resources/BallBounce/ballbounce_level" + level + ".txt");

        temps = new GameObject("Temps");
        blocks = new GameObject("Blocks");
        sBlocks = new GameObject("ShadowBlocks");
        int lineNum = -1;

        while (!sr.EndOfStream) {
            lineNum++;
            string line = sr.ReadLine();
            string[] types = line.Split(' ');
            int length = types.Length;

            for (int i = 0; i < length; i++) {
                GameObject b = new GameObject("Temp");
                b.transform.SetParent(temps.transform);

                if (types[i].Equals("1")) {
                    b = Instantiate(Resources.Load("BallBounce/Block") as GameObject);
                    b.transform.SetParent(blocks.transform);
                }
                if (types[i].Equals("2")) {
                    b = Instantiate(Resources.Load("BallBounce/ShadowBlock") as GameObject);
                    b.transform.SetParent(sBlocks.transform);
                }
                b.transform.position = new Vector3(-((length * (blockSize.size.x + blockSpace)) / 2) + (blockSize.size.x / 2) + (i * (blockSize.size.x + blockSpace)) + 1,
                                                       ((length * (blockSize.size.y + blockSpace)) / 2) - (blockSize.size.y / 2) - (lineNum * (blockSize.size.y + blockSpace)) - 1,
                                                       player.transform.position.z);

            }
        }
        blockCount = blocks.GetComponentsInChildren<Transform>().Length - 1;
        Destroy(temps);
    }
    // Update is called once per frame
    void Update() {
        if (!getComponents) {
            rb = ball.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(ballSpeedMin, 0);
            getComponents = true;
        }

        ballSpeedCap = Mathf.Clamp(ballSpeedCap, ballSpeedMin, ballSpeedMax);
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, ballSpeedCap);
    }
    void ChangeBallSpeed(int whichSpeed) {
        switch (whichSpeed) {
            case 0:
                ballSpeedCap += 100;
                break;
            case 1:
                ballSpeedCap -= 50;
                break;
        }
    }
    public void HitBall(GameObject paddle) {
        if (paddle.name.Contains("Up")) {
            if (paddle.name.Contains("PaddleL")) {
                if (paddleL) {
                    ChangeBallSpeed(0);
                    rb.velocity = player.transform.parent.up.normalized * rb.velocity.magnitude * 1.2f;
                }
                else {
                    ChangeBallSpeed(1);
                }
            }
            else {
                if (paddleR) {
                    ChangeBallSpeed(0);
                    rb.velocity = player.transform.parent.up.normalized * rb.velocity.magnitude * 1.2f;
                }
                else {
                    ChangeBallSpeed(1);
                }
            }
        }
        else {
            ChangeBallSpeed(1);
        }
    }
    public void WrapBall() {
        Vector3 pos = ball.transform.position;
        Vector2 boundSize = bounds.transform.lossyScale;
        float x = pos.x;
        float y = pos.y;

        if (pos.x < -boundSize.x)
            x = boundSize.x;
        if (pos.x > boundSize.x)
            x = -boundSize.x;
        if (pos.y < -boundSize.y)
            y = boundSize.y;
        if (pos.y > boundSize.y)
            y = -boundSize.y;

        ball.transform.position = new Vector3(x, y, pos.z);
    }
    public void DestroyBlock() {
        blockCount--;
        if (blockCount <= 0) {
            Destroy(ball);
            Destroy(sBlocks);
            GameObject pc = GameObject.FindWithTag("PowerCube");
            pc.transform.position = new Vector3(0, 0, 100);
            StartCoroutine(Win());
        }
    }
    public void DamagePlayer() {
        healthCount--;
        if (healthCount <= 0)
            GameController.Instance.ResetLevel();
        else
            Destroy(playerHealth[healthCount + 1].gameObject);
    }
    IEnumerator Win() {
        GameObject pc = GameObject.FindWithTag("PowerCube");
        while (Vector3.Distance(pc.transform.position, player.transform.position) > 1) {
            pc.transform.position = Vector2.MoveTowards(pc.transform.position, player.transform.position, Time.deltaTime * 30);
            yield return null;
        }
    }
}
