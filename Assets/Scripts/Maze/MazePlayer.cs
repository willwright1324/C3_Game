using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePlayer : MonoBehaviour {
    Rigidbody2D rb;
    public float speed = 1.5f;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0)
            return;
    }
    private void FixedUpdate() {
        rb.MovePosition(new Vector2(transform.position.x + Input.GetAxis("Horizontal") * speed, transform.position.y + Input.GetAxis("Vertical") * speed));
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Coin") {
            Destroy(collision.gameObject);
            MazeController.Instance.CollectCoin();
        }
    }
    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.tag == "Door") {
            if (Input.GetButtonDown("Action 1")) {
                MazeController.Instance.OpenDoor();
            }
        }
    }
}
