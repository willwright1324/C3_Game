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
    Vector2 moveDirection;
    Vector2 dashDirection;

    GameObject audioListener;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        audioListener = GameObject.Find("AudioListener");
    }
    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0)
            return;

        audioListener.transform.position = transform.position;
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetButtonDown("Action 1")) {
            if (!dashCooldown) {
                AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerDash);
                dashDirection = moveDirection;
                Invoke("DoneDash", dashLength);
                isDashing = true;
            }
        }
    }
    private void FixedUpdate() {
        if (!isDashing)
            rb.MovePosition((Vector2)transform.position + moveDirection * speed);
        else
            rb.MovePosition((Vector2)transform.position + dashDirection * dashSpeed);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Coin")
            MazeController.Instance.MoveRespawn();
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Damage") {
            MazeController.Instance.Respawn();
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
