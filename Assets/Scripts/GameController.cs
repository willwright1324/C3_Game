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

    // General
    GameObject colorCube;
    public GameObject[] cubes;
    Text cubeSelect;
    public int currentCube;
    public int selectState; // 0 = Cubes | 1 = Levels
    public int[] levelUnlocks = new int[8];
    public int[] levelSelects = new int[8];
    public string[] cubeNames = { "Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "Ball Bounce", "Puzzle" };
    public float selectCubeCooldown;
    public bool selectStateCooldown;
    // Camera
    GameObject cam;
    GameObject camOrbit;
    public int camLookSpeed = 10;
    public int camMoveSpeed = 500;
    public int camDistance = 80;
    public int camRotateSpeed = 10;
    public bool camIsLooking;
    public bool camIsMoving;
    IEnumerator lookAtCubeCoroutine;
    IEnumerator rotateCamOrbitCoroutine;

    // Start is called before the first frame update
    void Start() {
        colorCube = GameObject.Find("ColorCube");
        cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in cubes)
            cube.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        cubeSelect = GameObject.Find("CubeSelect").GetComponent<Text>();
        cubeSelect.text = cubeNames[0];

        cam = GameObject.FindWithTag("MainCamera");
        camOrbit = GameObject.Find("CameraOrbit");
        cam.transform.rotation = Quaternion.LookRotation(cubes[0].transform.position - cam.transform.position);
    }
    // Update is called once per frame
    void Update()
    {
        // Switch Cube/Level
        if (Input.GetAxisRaw("Horizontal") != 0) {
            if (SelectCubeCooldown()) {
                if (selectState == 0)
                    SelectCube(Input.GetAxisRaw("Horizontal"));
                if (selectState == 1 && !camIsLooking)
                    SelectLevel(Input.GetAxisRaw("Horizontal"));
            }
        }
        else
            selectCubeCooldown = 0;

        // Select Cube/Level
        if (Input.GetAxisRaw("Action 1") != 0) {
            if (!selectStateCooldown) {
                Invoke("SelectStateCooldown", 0.5f);
                selectStateCooldown = true;
                if (selectState == 0 && !camIsLooking && !camIsMoving) {
                    selectState = 1;
                    StartCoroutine(MoveToCube(currentCube));
                }
            }
        }

        if (Input.GetAxisRaw("Action 2") != 0) {
            if (!selectStateCooldown) {
                Invoke("SelectStateCooldown", 0.5f);
                selectStateCooldown = true;
                if (selectState == 1 && !camIsLooking && !camIsMoving) {
                    selectState = 0;
                    StartCoroutine(LookAtCenter());
                }
            }
        }

    }
    // Switch through cubes
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
    // Switch through levels
    void SelectLevel(float direction) {
        if (direction < 0)
            levelSelects[currentCube] = levelSelects[currentCube] - 1 < 0 ? 3 : --levelSelects[currentCube];
        else
            levelSelects[currentCube] = levelSelects[currentCube] + 1 > 3 ? 0 : ++levelSelects[currentCube];
        
        if (rotateCamOrbitCoroutine != null)
            StopCoroutine(rotateCamOrbitCoroutine);
        rotateCamOrbitCoroutine = RotateCamOrbit(levelSelects[currentCube]);
        StartCoroutine(rotateCamOrbitCoroutine);
    }
    // Camera looks at selected cube
    IEnumerator LookAtCube(int whichCube) {
        camIsLooking = true;
        Quaternion camRotation = Quaternion.LookRotation(cubes[whichCube].transform.position - cam.transform.position);

        while (Quaternion.Angle(cam.transform.rotation, camRotation) > 0.1f) {
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, camRotation, Time.deltaTime * camLookSpeed);
            if (Quaternion.Angle(cam.transform.rotation, camRotation) <= 0.1f) {
                cam.transform.rotation = camRotation;
                camIsLooking = false;
                yield break;
            }
            yield return null;
        }
    }
    // Camera moves to selected cube
    IEnumerator MoveToCube(int whichCube) {
        camIsLooking = true;
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
    // Camera looks at level side
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
    // Orients camera to cube
    IEnumerator FixCamRotation() {
        cam.transform.SetParent(camOrbit.transform);
        Quaternion camRotation = Quaternion.Euler(Vector3.zero);

        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.deltaTime * camRotateSpeed);
            if (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) <= 0.1f) {
                camOrbit.transform.localRotation = camRotation;
                camIsLooking = false;
                yield break;
            }
            yield return null;
        }
    }
    // Camera Orbit points back to center
    IEnumerator LookAtCenter() {
        camIsLooking = true;
        camOrbit.transform.SetParent(null);
        Quaternion camRotation = Quaternion.LookRotation(Vector3.zero - camOrbit.transform.position);

        while (Quaternion.Angle(camOrbit.transform.rotation, camRotation) > 0.1f)
        {
            camOrbit.transform.rotation = Quaternion.Slerp(camOrbit.transform.rotation, camRotation, Time.deltaTime * camRotateSpeed);
            if (Quaternion.Angle(camOrbit.transform.rotation, camRotation) <= 0.1f)
            {
                camOrbit.transform.rotation = camRotation;
                StartCoroutine(MoveToCenter());
                yield break;
            }
            yield return null;
        }
    }
    // Camera moves back to center
    IEnumerator MoveToCenter() {
        cam.transform.SetParent(null);

        while (Vector3.Distance(cam.transform.position, Vector3.zero) > 5)
        {
            print(Vector3.Distance(cam.transform.position, Vector3.zero));
            cam.transform.position -= cam.transform.forward * Time.deltaTime * camMoveSpeed;
            if (Vector3.Distance(cam.transform.position, Vector3.zero) <= 5)
            {
                cam.transform.position = Vector3.zero;
                camIsLooking = false;
                yield break;
            }
            yield return null;
        }
    }
    // Prevents fast select when held down
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
    // Prevents selection spam
    void SelectStateCooldown() {
        selectStateCooldown = false;
    }
}
