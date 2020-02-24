using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformPlayer : MonoBehaviour {
    Rigidbody2D rb;
    BoxCollider2D bc;
    public float speed = 120;
    float speedMax;
    public float jumpForce = 200;
    float jumpForceMax;
    public float springForce = 350;
    public float deceleration = 60;
    public bool canJump;
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        speedMax = speed;
        jumpForceMax = jumpForce;
    }

    // Update is called once per frame
    void Update() {

    }
    private void FixedUpdate() {
        rb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * speed, 0), ForceMode2D.Impulse);
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -speed, speed), Mathf.Clamp(rb.velocity.y, -jumpForce, jumpForce));
        //if (!canJump) {
            rb.velocity = new Vector2(rb.velocity.x / (Time.deltaTime * deceleration), rb.velocity.y);
        //}
        if (Input.GetButton("Action 1") && canJump && Input.GetAxisRaw("Vertical") >= 0) {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground" || (collision.gameObject.tag == "Platform" && transform.position.y > collision.gameObject.transform.position.y)) {
            canJump = true;
            jumpForce = jumpForceMax;
        }
    }
    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.tag == "Platform") {
            if (Input.GetButtonDown("Action 1") && Input.GetAxisRaw("Vertical") < 0) {
                canJump = false;
                bc = collision.gameObject.GetComponent<BoxCollider2D>();
                bc.enabled = false;
                Invoke("EnablePlatform", 0.25f);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Spring") {
            jumpForce = springForce;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Death") {
            GameController.Instance.DamagePlayer();
            GameController.Instance.RespawnPlayer();
        }
    }
    void EnablePlatform() {
        bc.enabled = true;
    }
}
