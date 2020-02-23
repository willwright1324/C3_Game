using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePlayer : MonoBehaviour {
    Rigidbody2D rb;
    public float speed = 1.5f;
    public int dashSpeed = 5;
    public float dashLength = 0.15f;
    public bool dashCooldown = false;
    bool isDashing;
    Vector3 dashDirection;
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0)
            return;
        if (Input.GetButtonDown("Action 1")) {
            if (!dashCooldown) {
                dashDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
                Invoke("DoneDash", dashLength);
                isDashing = true;
            }
        }
    }
    private void FixedUpdate() {
        if (!isDashing)
            rb.MovePosition(new Vector2(transform.position.x + Input.GetAxis("Horizontal") * speed, transform.position.y + Input.GetAxis("Vertical") * speed));
        else
            rb.MovePosition(transform.position + (dashDirection * dashSpeed));
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Coin") {
            Destroy(collision.gameObject);
            MazeController.Instance.CollectCoin();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.name == "Crusher") {
            MazeController.Instance.Respawn();
        }
    }
    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.tag == "Door") {
            if (Input.GetButtonDown("Action 1")) {
                MazeController.Instance.OpenDoor();
            }
        }
    }
    void DoneDash() {
        isDashing = false;
        Invoke("DashCooldown", 0.25f);
        dashCooldown = true;
    }
    void DashCooldown() {
        dashCooldown = false;

        /* Insert where needed
        if (!dashCooldown) {
            Invoke("DashCooldown", 0.5f);
            dashCooldown = true;
        }
        */
    }
}
