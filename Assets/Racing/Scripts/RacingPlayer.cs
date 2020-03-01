﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingPlayer : MonoBehaviour {
    GameObject cam;
    GameObject car;
    GameObject respawn;
    GameObject wheelL;
    GameObject wheelR;
    Rigidbody2D rb;
    GameObject[,] tracks;
    int[,] trackCoords = null;
    public int lastX = -1;
    public int lastY = -1;
    public int onTrack;
    public bool moveRespawn = true;
    public bool bumped;
    public float speed = 2.5f;
    public float normalSpeed;
    public float currentSpeed;
    public float DECELERATION = 0.2f;
    public float currentDeceleration;
    public float turnSpeed = 50f;
    public float driftTurnSpeed;
    public float normalTurnSpeed;
    public float currentTurnSpeed;
    public float TURN_DECELERATION = 100f;
    public float driftMultiplier = 0.5f;
    public float currentMultiplier;

    public float camY;
    float camOffset;
    public float camYMax = 15;
    // Start is called before the first frame update
    void Start() {
        tracks = RacingController.Instance.tracks;
        trackCoords = RacingController.Instance.trackCoords;
        cam = Camera.main.gameObject;
        car = GameObject.Find("Player/Car");
        respawn = GameObject.FindWithTag("Respawn");
        wheelL = GameObject.Find("Player/Car/WheelL");
        wheelR = GameObject.Find("Player/Car/WheelR");

        rb = GetComponent<Rigidbody2D>();
        camOffset = cam.transform.localPosition.y;
        normalTurnSpeed = turnSpeed;
        driftTurnSpeed = turnSpeed * 1.2f;

        normalSpeed = speed;
    }
    // Update is called once per frame
    void Update() {
        if (onTrack <= 0 && lastX != -1)
            Respawn();            

        if (!bumped) {
            // Speed Control
            if (Input.GetAxisRaw("Vertical") != 0 && !Input.GetButton("Action 2")) {
                currentSpeed += Input.GetAxisRaw("Vertical") * Time.deltaTime * speed;
            }
            else {
                if (Input.GetButton("Action 2"))
                    currentDeceleration = speed * 1.1f;
                else
                    currentDeceleration = DECELERATION;

                if (currentSpeed > 0.1f)
                    currentSpeed -= currentDeceleration * Time.deltaTime;
                else {
                    if (currentSpeed < -0.1f)
                        currentSpeed += currentDeceleration * Time.deltaTime * 4;
                    else {
                        currentSpeed = 0;
                    }
                }
            }
        }

        // Steering Control
        if (Input.GetAxisRaw("Horizontal") != 0) {
            currentTurnSpeed += Input.GetAxisRaw("Horizontal") * Time.deltaTime * turnSpeed * 4 * currentMultiplier;
        }
        else {
            if (currentTurnSpeed > 1)
                currentTurnSpeed -= TURN_DECELERATION * Time.deltaTime * currentMultiplier;
            else {
                if (currentTurnSpeed < -1)
                    currentTurnSpeed += TURN_DECELERATION * Time.deltaTime * currentMultiplier;
                else {
                    currentTurnSpeed = 0;
                }
            }
        }

        // Drift Control
        if (currentSpeed > 0 && Input.GetButton("Action 1")) {
            turnSpeed = driftTurnSpeed;
            currentMultiplier = driftMultiplier;

            car.transform.localRotation = Quaternion.Slerp(car.transform.localRotation, Quaternion.Euler(0, 0, -currentTurnSpeed), Time.deltaTime * 10f);
            currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -driftTurnSpeed, driftTurnSpeed);

            if (speed > 0)
                speed -= DECELERATION * Time.deltaTime;
            else
                speed = 0;
        }
        else {
            speed = normalSpeed;
            currentMultiplier = 1;

            car.transform.localRotation = Quaternion.Slerp(car.transform.localRotation, Quaternion.identity, Time.deltaTime * 5f);

            if (turnSpeed > normalTurnSpeed)
                turnSpeed -= TURN_DECELERATION * Time.deltaTime * 5;
            else
                turnSpeed = normalTurnSpeed;
            currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -turnSpeed, turnSpeed);
        }
        if (!RacingController.Instance.raceOver)
            currentSpeed = Mathf.Clamp(currentSpeed, -speed / 2, speed);
        else
            currentSpeed = Mathf.Clamp(currentSpeed, -speed / 2, speed / 2);

        if (turnSpeed <= normalTurnSpeed)
            wheelR.transform.localRotation = wheelL.transform.localRotation = Quaternion.Euler(0, 0, -currentTurnSpeed / 2);
        else
            wheelR.transform.localRotation = wheelL.transform.localRotation = Quaternion.Euler(0, 0, currentTurnSpeed / 2);
    }
    private void FixedUpdate() {
        if (!bumped) {
            if (currentSpeed > speed / 2)
                rb.AddTorque(-currentTurnSpeed * currentSpeed);
            else {
                if (currentSpeed != 0) {
                    if (currentSpeed > 0)
                        rb.AddTorque(-currentTurnSpeed * 1.5f);
                    if (currentSpeed < 0)
                        rb.AddTorque(currentTurnSpeed * 1.5f);
                }
            }
            rb.MovePosition(transform.position + transform.up * currentSpeed);
        }
        else {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector2.zero, Time.fixedDeltaTime * 5);
            if (rb.velocity.sqrMagnitude < 1)
                bumped =  false;
        }

    }
    private void LateUpdate() {
        camY = currentSpeed * 2;
        camY = Mathf.Clamp(camY, -camYMax / 2, camYMax);
        cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, new Vector3(0, camY + camOffset, -100), Time.deltaTime * 10);

        if (Mathf.Abs(currentSpeed) > 0)
            cam.transform.localRotation = Quaternion.RotateTowards(cam.transform.localRotation, Quaternion.Euler(new Vector3(0, 0, -currentTurnSpeed / 10)), Time.deltaTime * 20);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Damage") {
            float bumpForce = 50 * Mathf.Abs(currentSpeed);
            if (collision.gameObject.tag == "Enemy")
                bumpForce = 150;
            rb.AddForce(((Vector2)transform.position - (Vector2)collision.gameObject.transform.position).normalized * bumpForce, ForceMode2D.Impulse);
            currentSpeed = 0;
            bumped = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Track") {
            onTrack++;
            if (lastX != -1 && moveRespawn) {
                respawn.transform.position = tracks[lastX, lastY].transform.position + Vector3.back * 2;
                respawn.transform.rotation = tracks[lastX, lastY].transform.rotation;
                if (tracks[lastX, lastY].name.Contains("Corner")) {
                    if (tracks[lastX, lastY].transform.lossyScale.x < 0)
                        respawn.transform.rotation *= Quaternion.Euler(0, 0, 90);
                    else
                        respawn.transform.rotation *= Quaternion.Euler(0, 0, -90);
                }
            }
            Track t = collision.gameObject.GetComponent<Track>();
            lastX = t.coordX;
            lastY = t.coordY;
            trackCoords[t.coordX, t.coordY] = 1;
        }
        if (collision.gameObject.name == "RaceStart(Clone)") {
            RacingController.Instance.CheckLap();
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Track") {
            onTrack--;
        }
    }
    void Respawn() {
        moveRespawn = false;
        currentTurnSpeed = currentSpeed = 0;
        rb.angularVelocity = 0;
        cam.transform.localPosition = new Vector3(0, camOffset, -100);
        cam.transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.position = respawn.transform.position;
        transform.rotation = respawn.transform.rotation;
        Invoke("RespawnBuffer", 5);
    }
    void RespawnBuffer() {
        moveRespawn = true;
    }
}
