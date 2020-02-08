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
    Text cubeSelectText;
    Text controlsText;
    public int currentCube;
    public int selectState; // 0 = Cubes | 1 = Levels | 2 = How To
    public int[] levelUnlocks = new int[8];
    public int[] levelSelects = new int[8];
    public string[] cubeNames = {"Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "Ball Bounce", "Puzzle"};
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
    public bool camIsRotating;
    IEnumerator lookAtCubeCoroutine;
    IEnumerator rotateCamOrbitCoroutine;

    // Start is called before the first frame update
    void Start() {
        colorCube = GameObject.Find("ColorCube");
        cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in cubes)
            cube.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

        cubeSelectText = GameObject.Find("CubeSelect").GetComponent<Text>();
        cubeSelectText.text = "<- " + cubeNames[0] + " ->";
        controlsText = GameObject.Find("Controls").GetComponent<Text>();
        controlsText.text = "Z: Select \nX: Back \nLeft/Right: Change Selection";

        cam = GameObject.FindWithTag("MainCamera");
        camOrbit = GameObject.Find("CameraOrbit");
        cam.transform.rotation = Quaternion.LookRotation(cubes[0].transform.position - cam.transform.position);
    }
    // Update is called once per frame
    void Update()
    {
        // Switch Cube/Level
        if (Input.GetAxisRaw("Horizontal") != 0 && selectState != 2) {
            if (SelectCubeCooldown() && !camIsMoving) {
                if (selectState == 0)
                    SelectCube(Input.GetAxisRaw("Horizontal"));
                if (selectState == 1)
                    SelectLevel(Input.GetAxisRaw("Horizontal"));
            }
        }
        else
            selectCubeCooldown = 0;

        // Switch to How To and back
        if (Input.GetAxisRaw("Vertical") != 0) {
            if (!selectStateCooldown && !camIsLooking && !camIsMoving && !camIsRotating && (selectState == 1 || selectState == 2)) {
                Invoke("SelectStateCooldown", 0.5f);
                selectStateCooldown = true;
                if (Input.GetAxisRaw("Vertical") == -1 && selectState == 1) {
                    selectState = 2;
                    cubeSelectText.text = "How To Play";
                    StartCoroutine(HowToCamOrbit(currentCube));
                }
                if (Input.GetAxisRaw("Vertical") == 1 && selectState == 2) {
                    CheckIfLocked();
                    StartCoroutine(FixCamRotation(levelSelects[currentCube]));
                }
            }
        }

        // Select Cube/Level
        if (Input.GetAxisRaw("Action 1") != 0) {
            if (selectState == 0 && !camIsLooking && !camIsMoving && !camIsRotating) {
                selectState = 1;                 
                StartCoroutine(MoveToCube(currentCube));
            }
        }

        // Go Back
        if (Input.GetAxisRaw("Action 2") != 0) {
            if (selectState == 1 && !camIsLooking && !camIsMoving && !camIsRotating) {
                selectState = 0;
                StartCoroutine(LookAtCenter());
            }
        }

    }
    // Switch through cubes
    void SelectCube(float direction) {
        if (direction < 0)
            currentCube = currentCube - 1 < 0 ? 7 : --currentCube;
        else
            currentCube = currentCube + 1 > 7 ? 0 : ++currentCube;

        cubeSelectText.text = "<- " + cubeNames[currentCube] + " ->";

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

        CheckIfLocked();

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
        camIsMoving = true;
        camOrbit.transform.rotation = cam.transform.rotation * Quaternion.Euler(Vector3.up * 180);
        camOrbit.transform.position = cubes[whichCube].transform.position;
        camOrbit.transform.SetParent(cubes[whichCube].transform);

        while (Vector3.Distance(cam.transform.position, camOrbit.transform.position) > camDistance) {
            cam.transform.position += cam.transform.forward * Time.deltaTime * camMoveSpeed;
            if (Vector3.Distance(cam.transform.position, camOrbit.transform.position) <= camDistance) {
                SetControlsText(1);
                CheckIfLocked();
                StartCoroutine(FixCamRotation(levelSelects[whichCube]));
                yield break;
            }
            yield return null;
        }
    }
    // Camera looks at level side
    IEnumerator RotateCamOrbit(int whichLevel) {
        camIsRotating = true;
        Quaternion camRotation = Quaternion.Euler(0, whichLevel * -90, 0);

        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {           
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.deltaTime * camRotateSpeed);
            if (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) <= 0.1f) {
                camOrbit.transform.localRotation = camRotation;
                camIsRotating = false;
                yield break;
            }
            yield return null;
        }

    }
    // Camera looks at How To
    IEnumerator HowToCamOrbit(int whichCube) {
        camIsRotating = true;
        Transform[] cubeChildren = cubes[whichCube].GetComponentsInChildren<Transform>();
        cubeChildren[5].transform.localRotation = Quaternion.Euler(cubeChildren[5].transform.localRotation.eulerAngles.x, 
                                                              camOrbit.transform.localRotation.eulerAngles.y + 180, 
                                                              cubeChildren[5].transform.localRotation.eulerAngles.z);
        Quaternion camRotation = camOrbit.transform.localRotation * Quaternion.Euler(90, 0, 0);

        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.deltaTime * camRotateSpeed);
            if (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) <= 0.1f) {
                SetControlsText(2);
                camOrbit.transform.localRotation = camRotation;
                camIsRotating = false;
                yield break;
            }
            yield return null;
        }
    }
    // Orients camera to cube's last selected level
    IEnumerator FixCamRotation(int whichLevel) {
        cam.transform.SetParent(camOrbit.transform);
        Quaternion camRotation = Quaternion.Euler(0, whichLevel * -90, 0);

        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.deltaTime * camRotateSpeed);
            if (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) <= 0.1f) {
                camOrbit.transform.localRotation = camRotation;
                if (selectState == 2) {
                    SetControlsText(1);
                    selectState = 1;
                }
                camIsMoving = false;
                yield break;
            }
            yield return null;
        }
    }
    // Camera Orbit points back to center
    IEnumerator LookAtCenter() {
        camIsMoving = true;
        camOrbit.transform.SetParent(null);
        Quaternion camRotation = Quaternion.LookRotation(Vector3.zero - camOrbit.transform.position);

        while (Quaternion.Angle(camOrbit.transform.rotation, camRotation) > 0.1f) {
            camOrbit.transform.rotation = Quaternion.Slerp(camOrbit.transform.rotation, camRotation, Time.deltaTime * camRotateSpeed);
            if (Quaternion.Angle(camOrbit.transform.rotation, camRotation) <= 0.1f) {
                camOrbit.transform.rotation = camRotation;
                SetControlsText(0);
                cubeSelectText.text = "<- " + cubeNames[currentCube] + " ->";
                StartCoroutine(MoveToCenter());
                yield break;
            }
            yield return null;
        }
    }
    // Camera moves back to center
    IEnumerator MoveToCenter() {
        cam.transform.SetParent(null);

        while (Vector3.Distance(cam.transform.position, Vector3.zero) > 5) {
            cam.transform.position -= cam.transform.forward * Time.deltaTime * camMoveSpeed;
            if (Vector3.Distance(cam.transform.position, Vector3.zero) <= 5) {
                cam.transform.position = Vector3.zero;
                camIsMoving = false;
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

        /* Insert where needed
        if (!selectStateCooldown) {
            Invoke("SelectStateCooldown", 0.5f);
            selectStateCooldown = true;
        }
        */
    }
    // Checks if selected level is unlocked
    void CheckIfLocked() {
        if (levelUnlocks[currentCube] >= levelSelects[currentCube])
            cubeSelectText.text = "<- Level " + (levelSelects[currentCube] + 1) + " ->";
        else
            cubeSelectText.text = "<- Locked ->";
    }
    // Change controls
    void SetControlsText(int whichText) {
        switch (whichText) {
            case 0:
                controlsText.text = "Z: Select \nX: Back \nLeft/Right: Change Selection";
                break;
            case 1:
                controlsText.text = "Z: Select \nX: Back \nLeft/Right: Change Selection \nDown: How To Play";
                break;
            case 2:
                controlsText.text = "Z: Select \nX: Back \nUp: Level Select";
                break;
        }
    }
}
