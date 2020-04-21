using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBullet : MonoBehaviour {
    Rigidbody2D rb;
    public float speed = 1.7f;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate() {
        rb.MovePosition(transform.position + transform.up * speed);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == "BoundedCamera" || 
            collision.tag == "Trigger" || 
            collision.name == "ReticleTrigger" ||
            collision.tag == "Damage" ||
            collision.name.Contains("PlayerBullet"))
            return;

        Destroy(gameObject);
    }
}
