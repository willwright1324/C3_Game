﻿using UnityEngine;

public class GravityPlayer : MonoBehaviour {
    public Rigidbody2D rb;
    public ConstantForce2D cf;
    public float gravity = 1000;
    public bool canFlip = true;
    public bool flipped;
    public Vector3 vel;
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        cf = GetComponent<ConstantForce2D>();
    }
    // Update is called once per frame
    void Update() {
        vel = rb.velocity;
        if(Input.GetButtonDown("Action 1") && canFlip) {
            flipped = !flipped;
            canFlip = false;
            GravityController.Instance.DoFlipArrow(flipped);
        }
    }
    private void FixedUpdate() {
        if (!flipped) {
            rb.AddForce(Vector3.down * gravity);
        }
        else {
           rb.AddForce(Vector3.up * gravity);
        }
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, 0, cf.force.x), rb.velocity.y);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground")
            canFlip = true;
        if (collision.gameObject.tag == "Spike")
            GameController.Instance.ResetLevel();
        
    }
    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.name == "OutOfBounds")
            GameController.Instance.ResetLevel();
    }
}
