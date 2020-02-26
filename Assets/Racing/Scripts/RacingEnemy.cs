using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingEnemy : MonoBehaviour {
    GameObject enemyPath;
    public Transform[] paths;
    public int pathIndex = 1;
    public float speed = 200f;
    public float turnSpeed;
    // Start is called before the first frame update
    void Start() {
        turnSpeed = speed * 2;
        enemyPath = GameObject.Find("EnemyPath");
        paths = enemyPath.GetComponentsInChildren<Transform>();
        Transform[] tempPaths = enemyPath.GetComponentsInChildren<Transform>();
        paths = new Transform[tempPaths.Length - 1];

        for (int i = 0; i < paths.Length; i++) {
            for (int j = 0; j < tempPaths.Length; j++) {
                string name = tempPaths[j].name;
                if (name.Contains("" + i)) {
                    paths[i] = tempPaths[j];
                    break;
                }
            }
        }

        StartCoroutine(Drive());
    }

    // Update is called once per frame
    void Update() {
        
    }
    IEnumerator Drive() {
        while (Vector3.Distance(transform.position, paths[pathIndex].transform.position) > 1) {
            transform.position += transform.up * Time.deltaTime * speed;

            Vector3 target = paths[pathIndex].position - transform.position;
            float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg - 90;
            Quaternion enemyRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, enemyRotation, Time.deltaTime * turnSpeed);
            yield return null;
        }
        pathIndex = pathIndex + 1 >= paths.Length ? 0 : pathIndex + 1;
        StartCoroutine(Drive());
    }
}
