using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    /* 
     * Racing:      1
     * Shooter:     2
     * Rhythm:      3
     * Platformer:  4
     * Gravity:     5
     * Maze:        6
     * Ball Bounce: 7
     * Puzzle:      8
     */
    GameObject cam, colorcube;
    public string[] cubeNames = { "Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "Ball Bounce", "Puzzle" };
    public int[] levelUnlocks = new int[32];
    public int currentLevel = 1;
    public int currentCube = 1;
    public int selectState;
    public GameObject[] cubes;
    public int camLookSpeed = 10;
    Text cubeSelect;
    IEnumerator LookAtCubeCoroutine;
    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < levelUnlocks.Length; i += 4)
            levelUnlocks[i] = 1;
        cam = GameObject.FindWithTag("MainCamera");
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
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (selectState == 0)
                SelectCube(0);
            else {
                if (selectState == 1)
                    SelectLevel(0);
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (selectState == 0)
                SelectCube(1);
            else {
                if (selectState == 1)
                    SelectLevel(1);
            }
        }

        /*
        while (Input.GetAxis("Horizontal") > 0) {

        } */
    }
    void SelectCube(int direction) {
        if (direction == 0)
            currentCube = currentCube - 1 < 1 ? 8 : --currentCube;
        else
            currentCube = currentCube + 1 > 8 ? 1 : ++currentCube;

        cubeSelect.text = cubeNames[currentCube - 1];

        if (LookAtCubeCoroutine != null)
            StopCoroutine(LookAtCubeCoroutine);
        LookAtCubeCoroutine = LookAtCube(currentCube);
        StartCoroutine(LookAtCubeCoroutine);
    }
    void SelectLevel(int direction) {
        if (direction == 0)
            currentLevel = currentLevel - 1 < 1 ? 4 : --currentLevel;
        else
            currentLevel = currentLevel + 1 > 4 ? 1 : ++currentLevel;

        //StartCoroutine(LookAtCube(currentCube));
    }
    IEnumerator LookAtCube(int whichCube) {
        Vector3 cubeRotation = cubes[whichCube - 1].transform.position - cam.transform.position;
        while (cam.transform.rotation.eulerAngles != cubeRotation) {
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(cubeRotation), Time.deltaTime * camLookSpeed);
            yield return null;
        }
    }
}
