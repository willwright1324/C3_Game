using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour {
    public GameObject player;
    GameObject cam;
    public GameObject camScroll;
    GameObject arrow;
    public GameObject powerCube;
    public Rigidbody2D rb;
    public ConstantForce2D cf;
    public int camDistance = 50;
    bool getComponents;
    IEnumerator flipArrowCoroutine;

    public static GravityController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        player  = GameObject.FindWithTag("Player");
        cam = GameObject.FindWithTag("MainCamera");
        camScroll = GameObject.Find("CamScroll");
        arrow = GameObject.Find("Arrow");
        powerCube = GameObject.FindWithTag("PowerCube");

        rb = camScroll.GetComponent<Rigidbody2D>();
        cf = camScroll.GetComponent<ConstantForce2D>();

        camScroll.transform.position = new Vector3(player.transform.position.x + camDistance, cam.transform.position.y, cam.transform.position.z);
    }
    // Update is called once per frame
    void Update() {
        if (!getComponents) {
            rb = camScroll.GetComponent<Rigidbody2D>();
            cf = camScroll.GetComponent<ConstantForce2D>();
            getComponents = true;
        }
    }
    private void FixedUpdate() {
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, 0, cf.force.x), rb.velocity.y);
    }
    public void DoWinTrigger(GameObject obj) {
        //cf.enabled = false;
        StartCoroutine(WinTrigger(obj));
    }
    IEnumerator WinTrigger(GameObject obj) {
        ConstantForce2D cf = obj.GetComponent<ConstantForce2D>();
        cf.enabled = false;
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        Transform t = obj.GetComponent<Transform>();

        while (t.position.x < powerCube.transform.position.x) {
            rb.velocity = new Vector2(rb.velocity.x - Time.deltaTime * 100, rb.velocity.y);
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, 30, rb.velocity.x), rb.velocity.y);
            yield return null;
        }
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
    public void DoFlipArrow(bool flipped) {
        if (flipArrowCoroutine != null)
            StopCoroutine(flipArrowCoroutine);
        flipArrowCoroutine = FlipArrow(flipped);
        StartCoroutine(flipArrowCoroutine);
    }
    IEnumerator FlipArrow(bool flipped) {
        Quaternion arrowRotation = Quaternion.Euler(Vector3.back * 90);
        if (flipped)
            arrowRotation = Quaternion.Euler(Vector3.forward * 90);

        while (Quaternion.Angle(arrow.transform.rotation, arrowRotation) > 0.1f) {
            arrow.transform.rotation = Quaternion.Lerp(arrow.transform.rotation, arrowRotation, Time.deltaTime * 20);
            if (Quaternion.Angle(arrow.transform.rotation, arrowRotation) <= 0.1f) {
                arrow.transform.rotation = arrowRotation;
                yield break;
            }
            yield return null;
        }
    }
}
