using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour {
    GameObject player;
    public float offsetX;
    public float offsetY;
    public float offsetZ;

    // Start is called before the first frame update
    void Start() {
        player = GameObject.FindWithTag("Player");      
    }
    void LateUpdate() {
        transform.position = new Vector3(player.transform.position.x + offsetX, player.transform.position.y + offsetY, transform.position.z + offsetZ);
    }
}
