using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterPlayer1 : MonoBehaviour {
    Rigidbody2D rb;
    public float speed = 80f;
    public float rotateSpeed = 600f;
    float horMove;
    float vertMove;
    public bool canShoot = true;
    Vector2 moveDirection;
    GameObject bullet;

    IEnumerator rotateCoroutine;
    float currentAngle;
    public float horInput;
    public float vertInput;
    public int nextInput;
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        bullet = Resources.Load("Shooter/PlayerBullet") as GameObject;
    }
    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0)
            return;

        horInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Action 2")) {
            Instantiate(bullet, transform.position + transform.up * 8, transform.rotation);
        }
        moveDirection = Vector2.zero;
        if (horInput != 0) {
            if (vertInput == 0)
                nextInput = 1;
        }
        if (vertInput != 0) {
            if (horInput == 0)
                nextInput = 0;
        }
        if (!Input.GetButton("Action 1")) {
            if (horInput != 0 && vertInput != 0) {
                if (nextInput == 0) {
                    moveDirection = new Vector2(horInput, 0);
                    CheckHorAngle();
                }
                else {
                    moveDirection = new Vector2(0, vertInput);
                    CheckVertAngle();
                }
            }
            else {
                if (horInput != 0) {
                    moveDirection = new Vector2(horInput, 0);
                    CheckHorAngle();
                }
                else {
                    moveDirection = new Vector2(0, vertInput);
                    CheckVertAngle();
                }
            }
        }
        else {
            if (horInput != 0 && vertInput != 0) {
                if (nextInput == 0) {
                    moveDirection = new Vector2(0, vertInput);
                    CheckHorAngle();
                }
                else {
                    moveDirection = new Vector2(horInput, 0);
                    CheckVertAngle();
                }
            }
            else {
                if (horInput != 0) {
                    CheckHorAngle();
                }
                else {
                    CheckVertAngle();
                }
            }
        }
        rb.MovePosition((Vector2)transform.position + moveDirection.normalized * Time.deltaTime * speed);
    }
    void CheckHorAngle() {
        if (horInput != 0) {
            if (horInput > 0)
                DoRotate(270);
            else
                DoRotate(90);
        }
    }
    void CheckVertAngle() {
        if (vertInput != 0) {
            if (vertInput > 0)
                DoRotate(0);
            else
                DoRotate(180);
        }
    }
    void DoRotate(float angle) {
        if (angle == currentAngle)
            return;

        canShoot = false;
        currentAngle = angle;

        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);
        rotateCoroutine = Rotate(rotation);
        StartCoroutine(rotateCoroutine);
    }
    IEnumerator Rotate(Quaternion rotation) {
        while (Quaternion.Angle(transform.rotation, rotation) > 0.1f) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * rotateSpeed);
            yield return null;
        }
        transform.rotation = rotation;
        canShoot = true;
    }
}
