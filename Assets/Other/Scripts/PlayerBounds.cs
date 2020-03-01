using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBounds : MonoBehaviour {
    PlatformerCamera bc;
    // Start is called before the first frame update
    void Start() {
        bc = Camera.main.GetComponent<PlatformerCamera>();
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            //follow = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            bc.followSide = Mathf.Sign(collision.gameObject.transform.position.x - transform.position.x);
            //follow = true;
        }
    }
}
