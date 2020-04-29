using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    public GameObject[] buttons = new GameObject[2];
    public GameObject select;
    public GameObject creditsScreen;
    public int selection;
    public int menuMode;
    int selectionRange;

    // Start is called before the first frame update
    void Start() {
        GameController.Instance.Init();
        select = GameObject.Find("Select");
        creditsScreen = GameObject.Find("CreditsScreen");
        creditsScreen.SetActive(false);

        selectionRange = buttons.Length - 1;
        select.transform.position = buttons[0].transform.position;

        AudioController.Instance.PlayMusic(AudioController.Instance.menuMusic);
    }
    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Vertical") && menuMode == 0) {
            Select(Input.GetAxisRaw("Vertical"));
        }
        if (Input.GetButtonDown("Action 1")) {
            AudioController.Instance.PlaySoundOnce(AudioController.Instance.selectConfirm);
            if (menuMode == 1) {
                creditsScreen.SetActive(false);
                menuMode = 0;
                return;
            }
            if (buttons[selection].name == "Play") {
                SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 3);
            }
            if (buttons[selection].name == "Credits") {
                menuMode = 1;
                creditsScreen.SetActive(true);
            }
        }
    }
    void Select(float direction) {
        if (direction > 0)
            selection = selection - 1 < 0 ? selectionRange : --selection;
        else
            selection = selection + 1 > selectionRange ? 0 : ++selection;

        select.transform.position = buttons[selection].transform.position;
    }
}
