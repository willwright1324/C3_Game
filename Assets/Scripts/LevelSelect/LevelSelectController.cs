using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour {
    public GameState gameState;
    public SelectState selectState;
    public int currentCube;
    public int[,] levelHowToBoss = null;
    public int[] levelUnlocks = null;
    public int[] levelSelects = null;
    public string[] cubeNames = null;

    GameObject colorCube;
    public GameObject[] cubes;
    int[] wip = {1, 1, 1, 1, 0, 1, 0, 0 };
    Text cubeSelectText;
    Text controlsText;
    public float selectCubeCooldown;
    public bool selectStateCooldown;

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
        currentCube = GameController.Instance.currentCube;
        gameState = GameController.Instance.gameState;
        selectState = GameController.Instance.selectState;
        levelHowToBoss = GameController.Instance.levelHowToBoss;
        levelUnlocks = GameController.Instance.levelUnlocks;
        levelSelects = GameController.Instance.levelSelects;
        cubeNames = GameController.Instance.cubeNames;

        colorCube = GameObject.Find("ColorCube");
        cubes = new GameObject[cubeNames.Length];
        for (int i = 0; i < cubeNames.Length; i++) {
            cubes[i] = GameObject.Find(cubeNames[i] + "Cube");
            cubes[i].transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        }

        cubeSelectText = GameObject.Find("CubeSelect").GetComponent<Text>();
        controlsText = GameObject.Find("Controls").GetComponent<Text>();

        cam = GameObject.FindWithTag("MainCamera");
        camOrbit = GameObject.Find("CameraOrbit");

        InitCamera();
    }

    // Update is called once per frame
    void Update() {
        colorCube.transform.Rotate(Time.deltaTime * 10, Time.deltaTime * -15, Time.deltaTime * 1);
        foreach (GameObject cube in cubes)
            cube.transform.Rotate(Time.deltaTime * 10, Time.deltaTime * -15, Time.deltaTime * 1);

        if (selectState == SelectState.CUBES)
            WatchCube(currentCube);

        if (gameState == GameState.LEVEL_SELECT) {
            // Switch Cube/Level
            if (Input.GetAxisRaw("Horizontal") != 0 && selectState != SelectState.HOW_TO) {
                if (SelectCubeCooldown() && !camIsMoving) {
                    if (selectState == SelectState.CUBES) {
                        SelectCube(Input.GetAxisRaw("Horizontal"));
                        if (wip[currentCube] == 1)
                            cubeSelectText.text = "<- " + cubeNames[currentCube] + " WIP ->";
                    }
                    if (selectState == SelectState.LEVELS)
                        SelectLevel(Input.GetAxisRaw("Horizontal"));
                }
            }
            else
                selectCubeCooldown = 0;

            // Switch to How To and back
            if (Input.GetAxisRaw("Vertical") != 0) {
                if (!selectStateCooldown && !camIsLooking && !camIsMoving && !camIsRotating && (selectState == SelectState.LEVELS || selectState == SelectState.HOW_TO) && levelHowToBoss[currentCube, 0] > 0) {
                    Invoke("SelectStateCooldown", 0.5f);
                    selectStateCooldown = true;
                    if (Input.GetAxisRaw("Vertical") == -1 && selectState == SelectState.LEVELS) {
                        levelHowToBoss[currentCube, 0] = 1;
                        selectState = SelectState.HOW_TO;
                        cubeSelectText.text = "How To Play";
                        StartCoroutine(HowToCamOrbit(currentCube));
                    }
                    if (Input.GetAxisRaw("Vertical") == 1 && selectState == SelectState.HOW_TO) {
                        levelHowToBoss[currentCube, 0] = 2;
                        CheckIfUnlocked();
                        StartCoroutine(FixCamRotation(levelSelects[currentCube]));
                    }
                }
            }

            // Select Cube/Level
            if (Input.GetAxisRaw("Action 1") != 0) {
                if (!camIsLooking && !camIsMoving && !camIsRotating) {
                    if (selectState == SelectState.CUBES) {
                        if (wip[currentCube] == 1)
                            return;
                        selectState = SelectState.LEVELS;
                        StartCoroutine(MoveToCube(currentCube));
                    }
                    else {
                        if (selectState == SelectState.LEVELS && CheckIfUnlocked()) {
                            gameState = GameState.GAME;
                            if (levelSelects[currentCube] == 1)
                                selectState = SelectState.BOSS;
                            SaveToGameController();
                            SceneManager.LoadScene(2 + (currentCube * 6) + levelSelects[currentCube]);
                        }
                        else {
                            if (selectState == SelectState.HOW_TO) {
                                gameState = GameState.GAME;
                                SaveToGameController();
                                SceneManager.LoadScene(currentCube * 6 + 1);
                            }
                        }
                    }
                }
            }

            // Go Back
            if (Input.GetAxisRaw("Action 2") != 0) {
                if ((selectState == SelectState.LEVELS || selectState == SelectState.HOW_TO) && !camIsLooking && !camIsMoving && !camIsRotating) {
                    selectState = SelectState.CUBES;
                    StartCoroutine(LookAtCenter());
                }
            }
        }
    }
    // Sets camera to correct position
    void InitCamera() {
        switch (selectState) {
            case SelectState.CUBES:
                if (wip[currentCube] == 1)
                    cubeSelectText.text = "<- " + cubeNames[currentCube] + " WIP ->";
                else
                    cubeSelectText.text = "<- " + cubeNames[currentCube] + " ->";
                SetControlsText(0);
                cam.transform.LookAt(cubes[currentCube].transform.position);
                break;
            case SelectState.LEVELS:
                CheckIfUnlocked();
                SetControlsText(1);
                camOrbit.transform.position = cubes[currentCube].transform.position;
                camOrbit.transform.SetParent(cubes[currentCube].transform);
                camOrbit.transform.localRotation = Quaternion.Euler(0, levelSelects[currentCube] * -90, 0);
                cam.transform.position = camOrbit.transform.position + camOrbit.transform.forward * camDistance;
                cam.transform.SetParent(camOrbit.transform);
                cam.transform.localRotation = Quaternion.Euler(0, 180, 0);
                break;
            case SelectState.HOW_TO:
                cubeSelectText.text = "How To Play";
                SetControlsText(2);
                camOrbit.transform.position = cubes[currentCube].transform.position;
                camOrbit.transform.SetParent(cubes[currentCube].transform);
                camOrbit.transform.localRotation = Quaternion.Euler(90, levelSelects[currentCube] * -90, 0);
                Transform[] cubeChildren = cubes[currentCube].GetComponentsInChildren<Transform>();
                cubeChildren[5].transform.localRotation = Quaternion.Euler(cubeChildren[5].transform.localRotation.eulerAngles.x,
                                                              camOrbit.transform.localRotation.eulerAngles.y + 180,
                                                              cubeChildren[5].transform.localRotation.eulerAngles.z);
                cam.transform.position = camOrbit.transform.position + camOrbit.transform.forward * camDistance;
                cam.transform.SetParent(camOrbit.transform);
                cam.transform.localRotation = Quaternion.Euler(0, 180, 0);
                break;
        }
    }
    // Switch through cubes
    void SelectCube(float direction) {
        if (direction < 0)
            currentCube = currentCube - 1 < 0 ? 7 : --currentCube;
        else
            currentCube = currentCube + 1 > 7 ? 0 : ++currentCube;

        cubeSelectText.text = "<- " + cubeNames[currentCube] + " ->";
    }
    // Switch through levels
    void SelectLevel(float direction) {
        if (direction < 0)
            levelSelects[currentCube] = levelSelects[currentCube] - 1 < 0 ? 3 : --levelSelects[currentCube];
        else
            levelSelects[currentCube] = levelSelects[currentCube] + 1 > 3 ? 0 : ++levelSelects[currentCube];

        CheckIfUnlocked();

        if (rotateCamOrbitCoroutine != null)
            StopCoroutine(rotateCamOrbitCoroutine);
        rotateCamOrbitCoroutine = RotateCamOrbit(levelSelects[currentCube]);
        StartCoroutine(rotateCamOrbitCoroutine);
    }
    // Camera looks at selected cube
    void WatchCube(int whichCube) {
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(cubes[whichCube].transform.position - cam.transform.position), Time.deltaTime * camLookSpeed);
    }
    // Camera moves to selected cube
    IEnumerator MoveToCube(int whichCube) {
        camIsMoving = true;
        cam.transform.SetParent(null);
        camOrbit.transform.SetParent(null);
        camOrbit.transform.position = cubes[whichCube].transform.position;
        camOrbit.transform.SetParent(cubes[whichCube].transform);
        camOrbit.transform.rotation = Quaternion.LookRotation(cam.transform.position - camOrbit.transform.position);
        cam.transform.SetParent(camOrbit.transform);

        while (Vector3.Distance(cam.transform.position, camOrbit.transform.position) > camDistance) {
            cam.transform.position += cam.transform.forward * Time.deltaTime * camMoveSpeed;
            yield return null;
        }
        cam.transform.localRotation = Quaternion.Euler(0, 180, 0);
        cam.transform.localPosition = new Vector3(0, 0, cam.transform.localPosition.z);
        SetControlsText(1);
        CheckIfUnlocked();
        StartCoroutine(FixCamRotation(levelSelects[whichCube]));
    }
    // Camera looks at level side
    IEnumerator RotateCamOrbit(int whichLevel) {
        camIsRotating = true;
        Quaternion camRotation = Quaternion.Euler(0, whichLevel * -90, 0);

        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.deltaTime * camRotateSpeed);
            yield return null;
        }
        camOrbit.transform.localRotation = camRotation;
        camIsRotating = false;
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
            yield return null;
        }
        SetControlsText(2);
        camOrbit.transform.localRotation = camRotation;
        camIsRotating = false;
    }
    // Orients camera to cube's last selected level
    IEnumerator FixCamRotation(int whichLevel) {
        camIsMoving = true;
        Quaternion camRotation = Quaternion.Euler(0, whichLevel * -90, 0);

        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.deltaTime * camRotateSpeed);
            yield return null;
        }
        camOrbit.transform.localRotation = camRotation;
        if (selectState == SelectState.HOW_TO) {
            SetControlsText(1);
            selectState = SelectState.LEVELS;
        }
        camIsMoving = false;
        if (levelHowToBoss[currentCube, 0] < 2) {
            selectState = SelectState.HOW_TO;
            cubeSelectText.text = "How To Play";
            StartCoroutine(HowToCamOrbit(currentCube));
        }
    }
    // Camera Orbit points back to center
    IEnumerator LookAtCenter() {
        camIsMoving = true;
        camOrbit.transform.SetParent(null);
        Quaternion camRotation = Quaternion.LookRotation(Vector3.zero - camOrbit.transform.position);

        while (Quaternion.Angle(camOrbit.transform.rotation, camRotation) > 0.1f) {
            camOrbit.transform.rotation = Quaternion.Slerp(camOrbit.transform.rotation, camRotation, Time.deltaTime * camRotateSpeed);
            yield return null;
        }
        camOrbit.transform.rotation = camRotation;
        SetControlsText(0);
        cubeSelectText.text = "<- " + cubeNames[currentCube] + " ->";
        StartCoroutine(MoveToCenter());
    }
    // Camera moves back to center
    IEnumerator MoveToCenter() {
        cam.transform.SetParent(null);

        while (Vector3.Distance(cam.transform.position, Vector3.zero) > 5) {
            WatchCube(currentCube);
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, Vector3.zero, Time.deltaTime * camMoveSpeed);
            //cam.transform.position -= cam.transform.forward * Time.deltaTime * camMoveSpeed;
            yield return null;
        }
        cam.transform.position = Vector3.zero;
        camIsMoving = false;
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
    bool CheckIfUnlocked() {
        if (levelUnlocks[currentCube] >= levelSelects[currentCube]) {
            cubeSelectText.text = "<- Level " + (levelSelects[currentCube] + 1) + " ->";
            return true;
        }
        else {
            cubeSelectText.text = "<- Locked ->";
            return false;
        }
    }
    // Change controls
    void SetControlsText(int whichText) {
        switch (whichText) {
            case 0:
                controlsText.text = "Z: Select \nX: Back \n\nLeft/Right: Change Selection";
                break;
            case 1:
                controlsText.text = "Z: Select \nX: Back \n\nLeft/Right: Change Selection \n\nDown: How To Play";
                break;
            case 2:
                controlsText.text = "Z: Select \nX: Back \n\nUp: Level Select";
                break;
            case 3:
                controlsText.text = "Z: Select \nX: Back \n\nDown: Level Select";
                break;
        }
    }
    // Saves relevant data to the Game Controller
    void SaveToGameController() {
        GameController.Instance.currentCube = currentCube;
        GameController.Instance.gameState = gameState;
        GameController.Instance.selectState = selectState;
    }
}
