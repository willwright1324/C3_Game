using System.Collections;
using UnityEngine;

public class GravityPlayer : MonoBehaviour {
    public Rigidbody2D rb;
    public ConstantForce2D cf;
    public float gravity = 1000;
    public bool canFlip = true;
    public bool flipped;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        cf = GetComponent<ConstantForce2D>();
    }
    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0)
            return;

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
        if (gameObject.transform.position.x < GravityController.Instance.camScroll.transform.position.x - GravityController.Instance.camDistance)
            rb.AddForce(Vector3.right * 110);
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, 0, cf.force.x), rb.velocity.y);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground")
            canFlip = true;
        if (collision.gameObject.tag == "Death")
            GameController.Instance.ResetLevel();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.name == "WinTrigger") {
            GravityController.Instance.DoWinTrigger(gameObject);
            GravityController.Instance.DoWinTrigger(GravityController.Instance.camScroll);
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.name == "Bounds")
            GameController.Instance.ResetLevel();
    }
}
