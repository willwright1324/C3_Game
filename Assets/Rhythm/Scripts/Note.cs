using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour {
    public float speed = 50f;
    // Update is called once per frame
    void Update() {
        transform.position -= transform.right * Time.deltaTime * speed;
    }
}
