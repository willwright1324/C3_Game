using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPlayer : MonoBehaviour {
    GameObject camOrbit;
    GameObject cam;
    GameObject playerTarget;
    GameObject arm;
    GameObject respawn;
    Rigidbody rb;
    public float speed = 1f;
    float baseSpeed;
    public float speedMax = 12f;
    public float deceleration = 1.2f;
    public float jumpHeight = 10f;
    public float jumpForce = 60f;
    public float gravityForce = -60f;
    public float forwardSpeed = 15f;
    public float rotateSpeed = 50f;
    Vector3 playerPos;
    Vector3 targetPos;
    Vector3 gravity;
    public bool canJump = true;

    IEnumerator moveCamCoroutine;
    // Start is called before the first frame update
    void Start() {
        GameController.Instance.InitPlayer();
        camOrbit = GameObject.Find("CameraOrbit");
        cam = Camera.main.gameObject;
        playerTarget = GameObject.Find("PlayerTarget");
        arm = GameObject.Find("PlayerModel/ArmR");
        respawn = GameObject.FindWithTag("Respawn");
        rb = GetComponent<Rigidbody>();
        baseSpeed = speed;
    }

    // Update is called once per frame
    void Update() {
        if (transform.position.y < camOrbit.transform.position.y) {
            GameController.Instance.DamagePlayer();
            GameController.Instance.RespawnPlayer();
        }

        if (Input.GetButtonDown("Action 1") && canJump) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerJump);
            canJump = false;
            //StartCoroutine(Jump());
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        if (Input.GetButtonDown("Action 2")) {
            //StartCoroutine(PunchUp());
        }

        //transform.LookAt(new Vector3(camOrbit.transform.position.x, transform.position.y, camOrbit.transform.position.z), Vector3.up);
        if (BossController.Instance.playerMove) {
            camOrbit.transform.rotation *= Quaternion.Euler(0, -Input.GetAxisRaw("Horizontal") * Time.deltaTime * rotateSpeed, 0);
            playerTarget.transform.position += playerTarget.transform.forward * -Input.GetAxisRaw("Vertical") * Time.deltaTime * forwardSpeed;
            Vector3 targetLocalPos = playerTarget.transform.localPosition;
            playerTarget.transform.localPosition = new Vector3(targetLocalPos.x, targetLocalPos.y, Mathf.Clamp(targetLocalPos.z, 5, 17));
        }
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(transform.position - cam.transform.position), Time.deltaTime * rotateSpeed / 10);
        /*rb.MovePosition(Vector3.MoveTowards(transform.position, new Vector3(playerTarget.transform.position.x,
                                                                                 transform.position.y, playerTarget.transform.position.z), 
                                                                                 Time.deltaTime * speed));*/
        //camOrbit.transform.LookAt(new Vector3(transform.position.x, camOrbit.transform.position.y, transform.position.z), Vector3.up);
        //speed = baseSpeed * targetLocalPos.z;
        respawn.transform.position = targetPos = playerTarget.transform.position;
        playerPos = transform.position;
        playerPos.y = targetPos.y = 0;
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPos - playerPos), Time.deltaTime * rotateSpeed);
        //transform.LookAt(targetPos);
    }
    private void FixedUpdate() {
        if (!BossController.Instance.playerMove)
            return;
        if (Vector3.Distance(playerPos, targetPos) > 1f) {
            //rb.velocity += transform.forward * speed;
            rb.velocity += (targetPos - playerPos).normalized * speed;
            //rb.AddForce((targetPos - playerPos), ForceMode.Impulse);
        }
        else {
            rb.velocity = new Vector3(rb.velocity.x / deceleration, rb.velocity.y, rb.velocity.z / deceleration);
        }
        //rb.velocity = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0, rb.velocity.z), speedMax);
        //else {
            //rb.velocity = new Vector3(rb.velocity.x / (Time.deltaTime * deceleration), rb.velocity.y, rb.velocity.x / (Time.deltaTime * deceleration));
        //}
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -speedMax, speedMax), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -speedMax, speedMax));
        /*
        Vector3 hor = (transform.right * Input.GetAxisRaw("Horizontal")).normalized;
        Vector3 vert = (transform.forward * Input.GetAxisRaw("Vertical")).normalized;
        if (Vector3.Distance(transform.position, playerTarget.transform.position) > 0.5f)
            rb.AddForce((playerTarget.transform.position - transform.position).normalized * speed);
        else {
            rb.velocity = new Vector3(rb.velocity.x / (Time.deltaTime * deceleration), rb.velocity.y, rb.velocity.x / (Time.deltaTime * deceleration));
        }
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -speedMax, speedMax), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -speedMax, speedMax));
        //rb.AddTorque(transform.up * -Input.GetAxisRaw("Horizontal") * speed);
        //rb.AddForce((hor + vert) * speed);

        //rb.AddForce(gravity);*/
    }
    
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Trigger") {
            gravity = other.gameObject.transform.up * gravityForce;
            //transform.rotation = camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, other.gameObject.transform.localRotation, Time.smoothDeltaTime * camSpeed);
        }
        if (other.gameObject.tag == "Enemy") {
            BossController.Instance.DamageBoss();
            rb.AddForce(transform.up * jumpForce / 2, ForceMode.Impulse);
        }
        if (other.gameObject.tag == "Damage") {
            GameController.Instance.DamagePlayer();
        }
    }
    private void OnTriggerStay(Collider other) {

    }
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Ground") {
            canJump = true;
            BossController.Instance.playerMove = true;
        }
        if (collision.gameObject.tag == "Cube") {
            rb.AddForce((transform.position - collision.contacts[0].point).normalized * 100, ForceMode.Impulse);
        }
        if (collision.gameObject.tag == "Enemy") {
            if (BossController.Instance.bossDamage)
                GameController.Instance.DamagePlayer();
        }
    }
    IEnumerator Jump() {
        float height = transform.position.y + jumpHeight;

        while (transform.position.y < height) {
            rb.MovePosition(transform.position + transform.up * Time.deltaTime * jumpForce * Mathf.Max((height - transform.position.y), 1));
            yield return null;
        }
        StartCoroutine(Fall(height));
    }
    IEnumerator Fall(float height) {
        while (!canJump) {
            rb.MovePosition(transform.position - transform.up * Time.deltaTime * jumpForce * Mathf.Max((height - transform.position.y), 1));
            yield return null;
        }
    }
    IEnumerator PunchUp() {
        while (arm.transform.localRotation != Quaternion.Euler(100, 0, 0)) {
            arm.transform.localRotation = Quaternion.RotateTowards(arm.transform.localRotation, Quaternion.Euler(100, 0, 0), Time.deltaTime * 100f);
            yield return null;
        }
        //arm.transform.localRotation = Quaternion.Euler(100, 0, 0);
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(PunchDown());
    }
    IEnumerator PunchDown() {
        while (arm.transform.localRotation != Quaternion.Euler(0, 0, 0)) {
            arm.transform.localRotation = Quaternion.RotateTowards(arm.transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 100f);
            yield return null;
        }
        //arm.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
    IEnumerator MoveCam(Quaternion whichRotation) {
        while (Quaternion.Angle(camOrbit.transform.localRotation, whichRotation) > 0.1f) {
            transform.rotation = camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, whichRotation, Time.smoothDeltaTime * rotateSpeed);
            yield return null;
        }
    }
}
