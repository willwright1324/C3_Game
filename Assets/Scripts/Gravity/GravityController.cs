using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour {
    public GameObject player;
    GameObject cam;
    GameObject camScroll;
    GameObject arrow;
    public Rigidbody2D rb;
    public ConstantForce2D cf;
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
        rb = camScroll.GetComponent<Rigidbody2D>();
        cf = camScroll.GetComponent<ConstantForce2D>();

        camScroll.transform.position = new Vector3(player.transform.position.x + 50, cam.transform.position.y, cam.transform.position.z);
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
