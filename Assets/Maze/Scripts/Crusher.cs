using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crusher : MonoBehaviour {
    public float crushSpeed = 30f;
    public float retractSpeed = 30f;
    public float crushPause = 0.2f;
    public float retractPause = 0.2f;
    public float length = 20f;
    float size;
    Vector3 pos;

    // Start is called before the first frame update
    void Start() {
        size = transform.lossyScale.y;
        pos = transform.position;
        Invoke("DoCrush", retractPause);
    }
    void DoCrush() { StartCoroutine(Crush()); }
    void DoRetract() { StartCoroutine(Retract()); }

    IEnumerator Crush() {
        while (transform.localScale.y < length) {           
            transform.localScale += new Vector3(0, Time.deltaTime * crushSpeed, 0);
            transform.position += new Vector3(0, Time.deltaTime * crushSpeed, 0);
            yield return null;
        }
        transform.localScale = new Vector3(transform.localScale.x, length, transform.localScale.z);
        Invoke("DoRetract", crushPause);
    }
    IEnumerator Retract() {
        while (transform.localScale.y > size) {
            transform.localScale -= new Vector3(0, Time.deltaTime * retractSpeed, 0);
            transform.position -= new Vector3(0, Time.deltaTime * retractSpeed, 0);
            yield return null;
        }
        transform.position = pos;
        transform.localScale = new Vector3(transform.localScale.x, size, transform.localScale.z);
        Invoke("DoCrush", retractPause);
    }
}
