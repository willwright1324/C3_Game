﻿using System.Collections;
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
        GameController.Instance.Init();
        sceneText = GameObject.Find("SceneText").GetComponent<Text>();
        sceneImage = GameObject.Find("SceneImage").GetComponent<Image>();

        CanvasScaler cs = GameObject.Find("MainCanvas").GetComponent<CanvasScaler>();
        Rect r = sceneImage.gameObject.GetComponent<RectTransform>().rect;
        r.width = cs.referenceResolution.x;
        r.height = cs.referenceResolution.y;

        TextAsset file = Resources.Load("Cutscenes/" + cutscene + "/cutscene_text" + cutscene) as TextAsset;
        text = file.text.Split("\n"[0]);
        AudioController.Instance.PlayMusic(AudioController.Instance.puzzleMusic);
        Next();
    }
    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Action 1")) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.selectConfirm);
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
