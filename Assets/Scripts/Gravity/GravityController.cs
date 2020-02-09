using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour {
    public GameObject player;
    GameObject cam;

    // Singleton
    private static GravityController instance = null;
    public static GravityController Instance { get { return instance; } }
    private void Awake() { instance = this; }
    // Start is called before the first frame update
    void Start() {
        player  = GameObject.FindWithTag("Player");
        cam = GameObject.FindWithTag("MainCamera");
    }
    // Update is called once per frame
    void Update() {
        //cam.transform.Translate(Vector3.right * Time.deltaTime * scrollSpeed / 10);
    }
    private void LateUpdate() {
        cam.transform.position = new Vector3(player.transform.position.x + 50, cam.transform.position.y, cam.transform.position.z);
    }
}
