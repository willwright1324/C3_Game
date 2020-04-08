using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterMachineGun : MonoBehaviour {
    GameObject bullet;
    GameObject player;
    Vector2 playerPos;
    public int health = 5;
    public int ammoCapacity = 10;
    public int currentAmmo;
    public float reloadWait = 0.7f;
    public float fireRate = 0.1f;
    public float waitLength;
    public float rotateSpeed = 7f;
    public bool reloading;
    public bool onScreen;
    public bool seesPlayer;

    // Start is called before the first frame update
    void Start() {
        bullet = Resources.Load("Shooter/MachineGunBullet") as GameObject;
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0 || !onScreen)
            return;

        if (seesPlayer) {
            transform.up = Vector3.Slerp(transform.up, playerPos - (Vector2)transform.position, Time.deltaTime * rotateSpeed);
            if (!reloading) {
                if (waitLength > 0) {
                    waitLength -= Time.deltaTime;
                }
                else {
                    waitLength = fireRate;
                    if (currentAmmo > 0) {
                        currentAmmo--;
                        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.machineGunShoot);
                        Instantiate(bullet, transform.position + transform.up * 14, transform.rotation);
                    }
                    else {
                        currentAmmo = ammoCapacity;
                        Invoke("Reload", reloadWait);
                        reloading = true;
                    }
                }
            }
        }
    }
    private void FixedUpdate() {
        if (!onScreen)
            return;

        Debug.DrawRay((Vector2)transform.position, (Vector2)(player.transform.position - transform.position));
        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position);
        if (hit.collider.tag == "Player") {
            playerPos = hit.transform.position;
            seesPlayer = true;
        }
        else
            seesPlayer = false;
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Trigger") {
            onScreen = true;
        }
        if (collision.name.Contains("PlayerBullet")) {
            Damage();
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.tag == "Trigger") {
            onScreen = false;
        }
    }
    void Damage() {
        if (health > 0)
            health--;
        else {
            player.GetComponent<ShooterPlayer>().RemoveEnemy(gameObject);
            ShooterController.Instance.RemoveEnemy();
            Destroy(gameObject);
        }
    }
    void Reload() {
        reloading = false;
    }
}
