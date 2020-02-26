using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingPlayer : MonoBehaviour {
    GameObject cam;
    GameObject car;
    Rigidbody2D rb;
    public float speed = 2.5f;
    public float normalSpeed;
    public float currentSpeed;
    public float DECELERATION = 0.2f;
    public float currentDeceleration;
    public float turnSpeed = 40f;
    public float driftTurnSpeed;
    public float normalTurnSpeed;
    public float currentTurnSpeed;
    public float TURN_DECELERATION = 60f;
    public float driftMultiplier = 0.5f;
    public float currentMultiplier;
    // Start is called before the first frame update
    void Start() {
        cam = Camera.main.gameObject;
        car = GameObject.Find("Player/Car");
        rb = GetComponent<Rigidbody2D>();
        normalTurnSpeed = turnSpeed;
        driftTurnSpeed = turnSpeed * 1.5f;

        normalSpeed = speed;
    }
    // Update is called once per frame
    void Update() {

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

            car.transform.localRotation = Quaternion.Slerp(car.transform.localRotation, Quaternion.identity, Time.deltaTime * 10f);

            if (turnSpeed > normalTurnSpeed)
                turnSpeed -= TURN_DECELERATION * Time.deltaTime * 5;
            else
                turnSpeed = normalTurnSpeed;
            currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -turnSpeed, turnSpeed);
        }
        currentSpeed = Mathf.Clamp(currentSpeed, -speed / 2, speed);
    }
    private void FixedUpdate() {
        if (currentSpeed > speed / 2)
            rb.AddTorque(-currentTurnSpeed * currentSpeed);
        else
            rb.AddTorque(-currentTurnSpeed);

        rb.MovePosition(transform.position + transform.up * currentSpeed);
    }
}
