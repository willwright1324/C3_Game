using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour {
    public GameObject[] sceneText;
    int textIndex = -1;
    // Start is called before the first frame update
    void Start() {
        sceneText = GameObject.FindGameObjectsWithTag("Text");
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Action 1")) {
            if (textIndex < 0)
                GameObject.Find("Title").SetActive(false);
            textIndex++;
            if (textIndex > 2) {
                SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
                return;
            }
            for (int i = 0; i < sceneText.Length; i++) {
                if (sceneText[i].name.Contains(textIndex + "")) {
                    GameObject.Find("SceneText (-1)").GetComponent<Text>().text = sceneText[i].GetComponent<Text>().text;

                }
            }
           // GameObject.Find("SceneText (" + textIndex + ")").SetActive(false);
            //GameObject.Find("SceneText (" + ++textIndex + ")").SetActive(true);

            //if (textIndex > 2)
                //SceneManager.LoadScene();

        }
    }
}
