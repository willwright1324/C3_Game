using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingCamera : MonoBehaviour {
    GameObject player;
    public float speed = 20f;
    // Start is called before the first frame update
    void Start() {
        player = GameObject.FindWithTag("Player");
    }
    private void LateUpdate() {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z), Time.deltaTime * speed);
    }
}
