using UnityEngine;

public class GravityPlayer : MonoBehaviour
{
    public Rigidbody2D rb;
    public float gravity = 1000;
    public bool canFlip = true;
    public bool flipped;
    
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void Update() {
        if(Input.GetButtonDown("Action 1") && canFlip) {
            flipped = !flipped;
            canFlip = false;
        }
    }
    private void FixedUpdate() {
        if (!flipped) {
            rb.AddForce(Vector3.down * gravity);
        }
        else {
           rb.AddForce(Vector3.up * gravity);
        }
        //rb.AddForce(Vector3.right * scrollSpeed);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground")
            canFlip = true;
        if (collision.gameObject.tag == "Spike")
            GameController.Instance.Reset();
        
    }
}
