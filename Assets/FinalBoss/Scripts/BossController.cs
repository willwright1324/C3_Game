using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour {
    enum BossState { ATTACK, CENTER, CUBE}
    public bool playerMove = true;
    public bool bossDamage = true;
    GameObject player;
    GameObject camOrbit;
    GameObject cam;
    GameObject ground;
    GameObject boss;
    GameObject bossOrbit;
    GameObject colorCube;
    GameObject spin;
    GameObject planet;
    GameObject cubeShadow;
    Text bossHealth;
    public GameObject lastAttack;
    public GameObject[] attacks;
    public List<GameObject> attackList;
    public List<GameObject> attackPicks;
    public int currentSide;
    public float flipTime = 0.5f;
    public float flipSpeed = 500f;
    public Transform[] sidePositions = new Transform[6];
    Vector3 spinDirection;
    public float bossSpeed = 30f;
    IEnumerator chargePlayerCoroutine;

    public static BossController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        GameController.Instance.InitHealth();
        player = GameObject.FindWithTag("Player");
        camOrbit = GameObject.Find("CameraOrbit");
        cam = Camera.main.gameObject;
        ground = GameObject.FindWithTag("Ground");
        attacks = new GameObject[4];
        boss = GameObject.FindWithTag("Enemy");
        bossOrbit = GameObject.Find("BossOrbit");
        colorCube = GameObject.Find("ColorCube");
        spin = GameObject.Find("Spin");
        planet = GameObject.Find("Planet");

        bossHealth = GameObject.Find("BossHealth").GetComponent<Text>();

        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        for (int i = 0; i < 6; i++) {
            foreach (GameObject cube in cubes) {
                CubeInfo ci = cube.GetComponent<CubeInfo>();
                if (ci.side1 == i)
                    attackList.Add(cube);
            }
        }

        Transform[] tempPositions = GameObject.Find("Sides").GetComponentsInChildren<Transform>();
        for (int i = 0; i < sidePositions.Length; i++) {
            for (int j = 0; j < tempPositions.Length; j++) {
                if (tempPositions[j].name.Contains(i + "")) {
                    sidePositions[i] = tempPositions[j];
                }
            }
        }

        Invoke("NextAttack", 3f);
    }

    // Update is called once per frame
    void Update() {
        planet.transform.Rotate(Time.deltaTime * -0.4f, Time.deltaTime * 0.6f, Time.deltaTime * -0.2f);
    }
    void DoMoveToCube() {
        StartCoroutine(MoveToCube());
    }
    public void DamageBoss() {
        if (chargePlayerCoroutine != null) {
            StopCoroutine(chargePlayerCoroutine);
            chargePlayerCoroutine = null;
            boss.GetComponent<Renderer>().material = Resources.Load<Material>("FinalBoss/ObstacleBlack");
            Destroy(cubeShadow);
            bossHealth.text = "Boss Health: " + (8 - attackPicks.Count);
            //StartCoroutine(MoveToCube());
            if (attackPicks.Count >= 8) {
                Destroy(boss);
                GameController.Instance.Invoke("CompleteLevel", 2f);
            }
            else
                Invoke("NextAttack", 2f);
        }
    }
    void NextAttack() {
        bossDamage = true;

        Vector3 upSide = sidePositions[currentSide].position;
        Vector3 nextSide = Vector3.zero;
        spinDirection = Vector3.zero;
        GameObject attack = attackList[(Mathf.RoundToInt(Random.Range(0, 8)))];

        if (attackPicks.Count == 0)
            attack = attackList[(Mathf.RoundToInt(Random.Range(0, 4)))];
        else {
            if (attackPicks.Count < 8) {
                while (attackPicks.Contains(attack)) {
                    attack = attackList[(Mathf.RoundToInt(Random.Range(0, 8)))];
                }
                CubeInfo ci = attack.GetComponent<CubeInfo>();
                int[] sides = new int[3];

                sides[0] = ci.side1;
                sides[1] = ci.side2;
                sides[2] = ci.side3;

                for (int i = 0; i < sides.Length; i++) {
                    if (currentSide != sides[i]) {
                        if (currentSide <= 2 && currentSide + 3 == sides[i])
                            continue;
                        else {
                            if (currentSide > 2 && currentSide - 3 == sides[i])
                                continue;
                        }
                        currentSide = sides[i];
                        break;
                    }
                }

                nextSide = sidePositions[currentSide].position;
                if (upSide.x == nextSide.x)
                    spinDirection = new Vector3(Mathf.Sign(upSide.z - nextSide.z), 0, 0);
                else
                    spinDirection = new Vector3(0, 0, Mathf.Sign(nextSide.x - upSide.x));

                spin.transform.localRotation = Quaternion.identity;
                colorCube.transform.SetParent(spin.transform);
            }
        }
        lastAttack = attack;
        attackPicks.Add(lastAttack);

        if (spinDirection != Vector3.zero)
            StartCoroutine(SlamUp());
        else
            StartCoroutine(MoveToCube());
    }
    IEnumerator SlamUp() {
        int oppositeSide = currentSide + 3;
        if (oppositeSide > 5)
            oppositeSide = oppositeSide - 6;
        Vector3 newPos = sidePositions[oppositeSide].position + Vector3.up * 40;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        boss.transform.position = newPos;

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(SlamDown(sidePositions[oppositeSide].position));

    }
    IEnumerator SlamDown(Vector3 oppositeSide) {
        Vector3 slamPos = oppositeSide + Vector3.up * 10;

        while (Vector3.Distance(boss.transform.position, slamPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, slamPos, Time.deltaTime * bossSpeed * 2);
            yield return null;
        }
        boss.transform.position = slamPos;
        StartCoroutine(FlipCube(spinDirection));
    }
    IEnumerator FlipCube(Vector3 flipDirection) {
        playerMove = false;
        ground.SetActive(false);

        float flipCount = 0;
        while (flipCount < flipTime) {
            spin.transform.localRotation *= Quaternion.Euler(flipDirection * Time.deltaTime * flipSpeed);
            flipCount += Time.deltaTime;
            yield return null;
        }

        Vector3 upDirection = flipDirection * 90;
        while (Quaternion.Angle(Quaternion.Euler(spin.transform.localEulerAngles), Quaternion.Euler(upDirection)) > 0.1f) {
            spin.transform.localRotation = Quaternion.Slerp(spin.transform.localRotation, Quaternion.Euler(upDirection), Time.deltaTime * flipSpeed);
            yield return null;
        }
        spin.transform.localRotation = Quaternion.Euler(upDirection);
        colorCube.transform.SetParent(null);
        spin.transform.localRotation = Quaternion.identity;
        ground.SetActive(true);

        StartCoroutine(MoveToCube());
    }
    IEnumerator MoveToCube() {
        Vector3 attackPos = lastAttack.transform.position;
        attackPos.y = boss.transform.position.y;
        boss.transform.LookAt(attackPos);
        Vector3 newPos = lastAttack.transform.position + Vector3.up * 20;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(1);

        StartCoroutine(PowerUp());
    }
    IEnumerator PowerUp() {
        Vector3 newPos = lastAttack.transform.position;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed * 2);
            yield return null;
        }

        cubeShadow = Instantiate(Resources.Load("FinalBoss/CubeShadow") as GameObject, newPos, Quaternion.identity);
        boss.GetComponent<Renderer>().material = lastAttack.GetComponent<Renderer>().material;
        Vector3 playerPos = player.transform.position;
        playerPos.y = boss.transform.position.y;
        boss.transform.LookAt(playerPos);
        bossDamage = false;
        yield return new WaitForSeconds(1);
        //boss.transform.position = colorCube.transform.position + Vector3.forward * 30;
        StartCoroutine(MoveToTop());
    }
    IEnumerator MoveToTop() {
        Vector3 newPos = lastAttack.transform.position + Vector3.up * 15;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(2);

        StartCoroutine(Shoot());
    }
    IEnumerator Shoot() {
        Vector3 playerPos = player.transform.position;
        playerPos.y = boss.transform.position.y;
        boss.transform.LookAt(playerPos);

        float spinTime = 0;
        float shootTime = 0;
        GameObject bullet = Resources.Load("FinalBoss/Bullet") as GameObject;
        while (spinTime < 5) {
            spinTime += Time.deltaTime;
            shootTime += Time.deltaTime;
            if (shootTime > 0.1) {
                GameObject b = Instantiate(bullet, boss.transform.position + Vector3.up * -3, Quaternion.identity);
                b.GetComponent<ConstantForce>().force = boss.transform.forward * 50;
                Destroy(b, 10f);
                shootTime = 0;
            }
            boss.transform.rotation *= Quaternion.Euler(boss.transform.up * Time.deltaTime * 200);
            yield return null;
        }
        StartCoroutine(StartCharge());
    }
    IEnumerator StartCharge() {
        yield return new WaitForSeconds(1);
        Vector3 playerPos = player.transform.position;
        playerPos.y = boss.transform.position.y;
        boss.transform.LookAt(playerPos);

        Vector3 newPos = boss.transform.position + boss.transform.forward * -5;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(2);

        chargePlayerCoroutine = ChargePlayer();
        StartCoroutine(chargePlayerCoroutine);
    }
    IEnumerator ChargePlayer() {
        Vector3 newPos = boss.transform.position + boss.transform.forward * 50;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed * 1);
            yield return null;
        }
        StartCoroutine(MoveBack());
    }
    IEnumerator MoveBack() {
        Vector3 bossPos = boss.transform.position;
        bossPos.y = bossOrbit.transform.position.y;
        bossOrbit.transform.LookAt(bossPos);
        boss.transform.parent = bossOrbit.transform;
        Vector3 attackPos = lastAttack.transform.position;
        Vector3 orbitPos = bossOrbit.transform.position;
        attackPos.y = orbitPos.y;

        Quaternion orbitRotation = Quaternion.LookRotation(attackPos - orbitPos);

        while (Quaternion.Angle(bossOrbit.transform.rotation, orbitRotation) > 0.1f) {
            bossOrbit.transform.rotation = Quaternion.Slerp(bossOrbit.transform.rotation, orbitRotation, Time.deltaTime * 2f);
            yield return null;
        }
        bossOrbit.transform.rotation = orbitRotation;
        boss.transform.parent = null;
        StartCoroutine(MoveToTop());
    }

}
