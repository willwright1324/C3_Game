using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {
    public GameObject[] buttons = new GameObject[3];
    public GameObject select;
    public GameObject creditsScreen;
    Text selectText;
    public int selection;
    public int menuMode;
    int selectionRange;

    // Start is called before the first frame update
    void Start() {
        GameController.Instance.Init();
        select = GameObject.Find("Select");
        creditsScreen = GameObject.Find("CreditsScreen");
        creditsScreen.SetActive(false);

        selectText = select.GetComponent<Text>();

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
                if (!GameController.Instance.didCutscene[0]) {
                    GameController.Instance.didCutscene[0] = true;
                    SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 3);
                }
                else
                    SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 2);
            }
            if (buttons[selection].name == "Credits") {
                menuMode = 1;
                creditsScreen.SetActive(true);
            }
            if (buttons[selection].name == "Quit") {
                GameController.Instance.Save();
                Application.Quit();
            }
        }
    }
    void Select(float direction) {
        if (direction > 0)
            selection = selection - 1 < 0 ? selectionRange : --selection;
        else
            selection = selection + 1 > selectionRange ? 0 : ++selection;

        selectText.text = "<";
        for (int i = 0; i < buttons[selection].GetComponent<Text>().text.Length + 2; i++)
            selectText.text += " ";
        selectText.text += ">";
        select.transform.position = buttons[selection].transform.position;
    }
}
