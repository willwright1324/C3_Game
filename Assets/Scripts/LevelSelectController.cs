﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour {
    public GameState gameState;
    public SelectState selectState;
    public int currentCube;
    public int[] levelUnlocks = null;
    public int[] levelSelects = null;

    GameObject colorCube;
    public GameObject[] cubes;
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
        levelUnlocks = GameController.Instance.levelUnlocks;
        levelSelects = GameController.Instance.levelSelects;

        colorCube = GameObject.Find("ColorCube");
        cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in cubes)
            cube.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

        cubeSelectText = GameObject.Find("CubeSelect").GetComponent<Text>();
        cubeSelectText.text = "<- " + GameController.Instance.cubeNames[0] + " ->";
        controlsText = GameObject.Find("Controls").GetComponent<Text>();
        controlsText.text = "Z: Select \nX: Back \nLeft/Right: Change Selection";

        cam = GameObject.FindWithTag("MainCamera");
        camOrbit = GameObject.Find("CameraOrbit");
        cam.transform.rotation = Quaternion.LookRotation(cubes[0].transform.position - cam.transform.position);
    }

    // Update is called once per frame
    void Update() {
        if (gameState == GameState.LEVEL_SELECT) {
            // Switch Cube/Level
            if (Input.GetAxisRaw("Horizontal") != 0 && selectState != SelectState.HOW_TO) {
                if (SelectCubeCooldown() && !camIsMoving) {
                    if (selectState == SelectState.CUBES)
                        SelectCube(Input.GetAxisRaw("Horizontal"));
                    if (selectState == SelectState.LEVELS)
                        SelectLevel(Input.GetAxisRaw("Horizontal"));
                }
            }
            else
                selectCubeCooldown = 0;

            // Switch to How To and back
            if (Input.GetAxisRaw("Vertical") != 0) {
                if (!selectStateCooldown && !camIsLooking && !camIsMoving && !camIsRotating && (selectState == SelectState.LEVELS || selectState == SelectState.HOW_TO)) {
                    Invoke("SelectStateCooldown", 0.5f);
                    selectStateCooldown = true;
                    if (Input.GetAxisRaw("Vertical") == -1 && selectState == SelectState.LEVELS) {
                        selectState = SelectState.HOW_TO;
                        cubeSelectText.text = "How To Play";
                        StartCoroutine(HowToCamOrbit(currentCube));
                    }
                    if (Input.GetAxisRaw("Vertical") == 1 && selectState == SelectState.HOW_TO) {
                        if (CheckIfUnlocked())
                            StartCoroutine(FixCamRotation(levelSelects[currentCube]));
                    }
                }
            }

            // Select Cube/Level
            if (Input.GetAxisRaw("Action 1") != 0) {
                if (!camIsLooking && !camIsMoving && !camIsRotating) {
                    if (selectState == SelectState.CUBES) {
                        selectState = SelectState.LEVELS;
                        StartCoroutine(MoveToCube(currentCube));
                    }
                    else {
                        if (selectState == SelectState.LEVELS) {
                            gameState = GameState.GAME;
                            SaveToGameController();
                            SceneManager.LoadScene(currentCube + 1);
                        }
                    }
                }
            }

            // Go Back
            if (Input.GetAxisRaw("Action 2") != 0) {
                if (selectState == SelectState.LEVELS && !camIsLooking && !camIsMoving && !camIsRotating) {
                    selectState = SelectState.CUBES;
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

        cubeSelectText.text = "<- " + GameController.Instance.cubeNames[currentCube] + " ->";

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

        CheckIfUnlocked();

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
            yield return null;
        }
        cam.transform.rotation = camRotation;
        camIsLooking = false;
    }
    // Camera moves to selected cube
    IEnumerator MoveToCube(int whichCube) {
        camIsMoving = true;
        camOrbit.transform.rotation = cam.transform.rotation * Quaternion.Euler(Vector3.up * 180);
        camOrbit.transform.position = cubes[whichCube].transform.position;
        camOrbit.transform.SetParent(cubes[whichCube].transform);

        while (Vector3.Distance(cam.transform.position, camOrbit.transform.position) > camDistance) {
            cam.transform.position += cam.transform.forward * Time.deltaTime * camMoveSpeed;
            yield return null;
        }
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
        cam.transform.SetParent(camOrbit.transform);
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
        cubeSelectText.text = "<- " + GameController.Instance.cubeNames[currentCube] + " ->";
        StartCoroutine(MoveToCenter());
    }
    // Camera moves back to center
    IEnumerator MoveToCenter() {
        cam.transform.SetParent(null);

        while (Vector3.Distance(cam.transform.position, Vector3.zero) > 5) {
            cam.transform.position -= cam.transform.forward * Time.deltaTime * camMoveSpeed;
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
    // Saves relevant data to the Game Controller
    void SaveToGameController() {
        GameController.Instance.currentCube = currentCube;
        GameController.Instance.gameState = gameState;
        GameController.Instance.selectState = selectState;
    }
}
