using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerEnemy : MonoBehaviour {
    Vector3 startPos;
    Vector3 endPos;
    public float speed = 15;
    public float length = 40;
    public float pauseTime = 0.5f;
    bool moved;
    // Start is called before the first frame update
    void Start() {
        startPos = transform.position;
        endPos = startPos + Vector3.right * length;
        StartCoroutine(Move(endPos));
    }
    void DoMove() {
        moved = !moved;
        if (moved)
            StartCoroutine(Move(startPos));
        else
            StartCoroutine(Move(endPos));
    }
    IEnumerator Move(Vector3 whichPos) {
        while (Vector3.Distance(transform.position, whichPos) > 0.1f) {
            transform.position = Vector3.MoveTowards(transform.position, whichPos, Time.deltaTime * speed);
            yield return null;
        }
        Invoke("DoMove", pauseTime);
    }
}
