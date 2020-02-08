using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    /* 
     * Racing:      0
     * Shooter:     1
     * Rhythm:      2
     * Platformer:  3
     * Gravity:     4
     * Maze:        5
     * Ball Bounce: 6
     * Puzzle:      7
     */
    GameObject cam, camOrbit, colorcube;
    public string[] cubeNames = { "Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "Ball Bounce", "Puzzle" };
    public int[] levelUnlocks = new int[32];
    public int currentLevel;
    public int currentCube;
    public int selectState;
    public GameObject[] cubes;
    public int camLookSpeed = 10;
    public int camMoveSpeed = 500;
    public int camDistance = 80;
    public int camRotateSpeed = 10;
    Text cubeSelect;
    IEnumerator lookAtCubeCoroutine;
    IEnumerator rotateCamOrbitCoroutine;
    public float selectCubeCooldown;
    public bool selectStateCooldown;
    public bool camIsMoving;

    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < levelUnlocks.Length; i += 4)
            levelUnlocks[i] = 1;
        cam = GameObject.FindWithTag("MainCamera");
        camOrbit = GameObject.Find("CameraOrbit");
        colorcube = GameObject.Find("ColorCube");
        cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in cubes)
            cube.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        cubeSelect = GameObject.Find("CubeSelect").GetComponent<Text>();

        cam.transform.rotation = Quaternion.LookRotation(cubes[0].transform.position - cam.transform.position);
        cubeSelect.text = cubeNames[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0) {
            if (SelectCubeCooldown()) {
                if (selectState == 0)
                    SelectCube(Input.GetAxisRaw("Horizontal"));
                if (selectState == 1)
                    SelectLevel(Input.GetAxisRaw("Horizontal"));
            }
        }
        else
            selectCubeCooldown = 0;

        if (Input.GetAxisRaw("Action 1") != 0) {
            if (!selectStateCooldown) {
                Invoke("SelectStateCooldown", 0.5f);
                selectStateCooldown = true;
                if (selectState == 0 && camIsMoving == false) {
                    selectState = 1;
                    StartCoroutine(MoveToCube(currentCube));
                }
            }
        }
    }
    void SelectCube(float direction) {
        if (direction < 0)
            currentCube = currentCube - 1 < 0 ? 7 : --currentCube;
        else
            currentCube = currentCube + 1 > 7 ? 0 : ++currentCube;

        cubeSelect.text = cubeNames[currentCube];

        if (lookAtCubeCoroutine != null)
            StopCoroutine(lookAtCubeCoroutine);
        lookAtCubeCoroutine = LookAtCube(currentCube);
        StartCoroutine(lookAtCubeCoroutine);
    }
    void SelectLevel(float direction) {
        if (direction < 0)
            currentLevel = currentLevel - 1 < 0 ? 3 : --currentLevel;
        else
            currentLevel = currentLevel + 1 > 3 ? 0 : ++currentLevel;
        
        if (rotateCamOrbitCoroutine != null)
            StopCoroutine(rotateCamOrbitCoroutine);
        rotateCamOrbitCoroutine = RotateCamOrbit(currentLevel);
        StartCoroutine(rotateCamOrbitCoroutine);
    }
    IEnumerator LookAtCube(int whichCube) {
        camIsMoving = true;
        Quaternion camRotation = Quaternion.LookRotation(cubes[whichCube].transform.position - cam.transform.position);

        while (Quaternion.Angle(cam.transform.rotation, camRotation) > 0.1f) {
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, camRotation, Time.deltaTime * camLookSpeed);
            if (Quaternion.Angle(cam.transform.rotation, camRotation) <= 0.1f) {
                cam.transform.rotation = camRotation;
                camIsMoving = false;
                yield break;
            }
            yield return null;
        }
    }
    IEnumerator MoveToCube(int whichCube) {
        camIsMoving = true;
        camOrbit.transform.rotation = cam.transform.rotation * Quaternion.Euler(Vector3.up * 180);
        camOrbit.transform.position = cubes[whichCube].transform.position;
        camOrbit.transform.SetParent(cubes[whichCube].transform);

        while (Vector3.Distance(cam.transform.position, camOrbit.transform.position) > camDistance) {
            cam.transform.position += cam.transform.forward * Time.deltaTime * camMoveSpeed;
            if (Vector3.Distance(cam.transform.position, camOrbit.transform.position) <= camDistance) {
                StartCoroutine(FixCamRotation());
                yield break;
            }
            yield return null;
        }
    }
    IEnumerator RotateCamOrbit(int whichLevel) {
        camIsMoving = true;
        Quaternion camRotation = Quaternion.Euler(0, whichLevel * -90, 0);

        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {           
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.deltaTime * camRotateSpeed);
            if (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) <= 0.1f) {
                camOrbit.transform.localRotation = camRotation;
                camIsMoving = false;
                yield break;
            }
            yield return null;
        }

    }
    IEnumerator FixCamRotation() {
        cam.transform.SetParent(camOrbit.transform);
        Quaternion camRotation = Quaternion.Euler(Vector3.zero);

        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.deltaTime * camRotateSpeed);
            if (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) <= 0.1f) {
                camOrbit.transform.localRotation = camRotation;
                camIsMoving = false;
                yield break;
            }
            yield return null;
        }
    }
    bool SelectCubeCooldown() {
        if (selectCubeCooldown <= 0) {
            selectCubeCooldown = 0.5f;
            return true;
        }
        else {
            selectCubeCooldown -= Time.deltaTime;
            return false;
        }
    }
    void SelectStateCooldown() {
        selectStateCooldown = false;
    }
}
