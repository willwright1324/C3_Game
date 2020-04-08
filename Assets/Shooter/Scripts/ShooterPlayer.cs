using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterPlayer : MonoBehaviour {
    Rigidbody2D rb;
    GameObject reticle;
    public GameObject currentEnemy;
    public List<GameObject> enemies = new List<GameObject>();
    public float speed = 1.5f;
    public float rotateSpeed = 600f;
    public bool canShoot = true;
    public bool enemyLocked;
    GameObject bullet;

    IEnumerator rotateCoroutine;
    float currentAngle;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        reticle = GameObject.Find("Reticle");
        bullet = Resources.Load("Shooter/PlayerBullet") as GameObject;
    }
    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0)
            return;

        if (currentEnemy != null) {
            reticle.SetActive(true);
            reticle.transform.position = currentEnemy.transform.position - Vector3.forward;
            reticle.transform.rotation = currentEnemy.transform.rotation;
        }
        else {
            reticle.SetActive(false);
        }

        float horInput = Input.GetAxisRaw("Horizontal");
        float vertInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Action 2")) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerShoot);
            Instantiate(bullet, transform.position + transform.up * 8, transform.rotation);
        }

        if (!Input.GetButton("Action 1")) {
            if (enemyLocked) {
                reticle.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Shooter/reticle0");
            }
            enemyLocked = false;

            if (horInput != 0 && vertInput == 0) {
                if (horInput > 0)
                    DoRotate(270);
                else
                    DoRotate(90);
            }
            if (vertInput != 0 && horInput == 0) {
                if (vertInput > 0)
                    DoRotate(0);
                else
                    DoRotate(180);
            }
            if (horInput != 0 && vertInput != 0) {
                if (horInput > 0 && vertInput > 0)
                    DoRotate(315);
                if (horInput < 0 && vertInput > 0)
                    DoRotate(45);
                if (horInput < 0 && vertInput < 0)
                    DoRotate(135);
                if (horInput > 0 && vertInput < 0)
                    DoRotate(225);
            }
        }
        else {
            if (currentEnemy != null) {
                currentAngle = -1;
                if (rotateCoroutine != null)
                    StopCoroutine(rotateCoroutine);
                transform.up = Vector3.Slerp(transform.up, (Vector2)currentEnemy.transform.position - (Vector2)transform.position, Time.deltaTime * rotateSpeed / 25);
                if (!enemyLocked) {
                    AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.lockAim);
                    reticle.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Shooter/reticle1");
                }
                enemyLocked = true;
            }
        }
    }
    private void FixedUpdate() {
        rb.MovePosition((Vector2)transform.position + new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * speed);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Enemy") {
            if (!enemies.Contains(collision.gameObject))
                enemies.Add(collision.gameObject);
            if (!enemyLocked || currentEnemy == null) {
                CheckEnemies();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.tag == "Enemy") {
            enemies.Remove(collision.gameObject);
        }
    }
    void CheckEnemies() {
        if (enemies.Count == 0)
            return;

        currentEnemy = enemies[0];
        foreach (GameObject e in enemies) {
            if (Vector3.Distance(transform.position, e.transform.position) < Vector3.Distance(transform.position, currentEnemy.transform.position)) {
                currentEnemy = e;
            }
        }
    }
    public void RemoveEnemy(GameObject e) {
        enemies.Remove(e);
        CheckEnemies();
    }
    void DoRotate(float angle) {
        if (angle == currentAngle)
            return;

        canShoot = false;
        currentAngle = angle;

        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);
        rotateCoroutine = Rotate(rotation);
        StartCoroutine(rotateCoroutine);
    }
    IEnumerator Rotate(Quaternion rotation) {
        while (Quaternion.Angle(transform.rotation, rotation) > 0.1f) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * rotateSpeed);
            yield return null;
        }
        transform.rotation = rotation;
        canShoot = true;
    }
}
