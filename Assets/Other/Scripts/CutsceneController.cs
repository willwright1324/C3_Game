using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour {
    public int cutscene = 1;
    string[] text = null;
    public int index = -1;

    Text sceneText;
    Image sceneImage;
    // Start is called before the first frame update
    void Start() {
        sceneText = GameObject.Find("SceneText").GetComponent<Text>();
        sceneImage = GameObject.Find("SceneImage").GetComponent<Image>();

        CanvasScaler cs = GameObject.Find("MainCanvas").GetComponent<CanvasScaler>();
        Rect r = sceneImage.gameObject.GetComponent<RectTransform>().rect;
        r.width = cs.referenceResolution.x;
        r.height = cs.referenceResolution.y;

        TextAsset file = Resources.Load("Cutscenes/" + cutscene + "/cutscene_text" + cutscene) as TextAsset;
        text = file.text.Split("\n"[0]);
        Next();
    }
    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Action 1")) {
            Next();
        }
    }
    void Next() {
        index++;
        if (index < text.Length) {
            sceneText.text = text[index];
            sceneText.text += "[Press Z to Continue]";
            sceneImage.sprite = Resources.Load<Sprite>("Cutscenes/" + cutscene + "/image" + index);
        }
        else {
            SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
        }
    }
}
