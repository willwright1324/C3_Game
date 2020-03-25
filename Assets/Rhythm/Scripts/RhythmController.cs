using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmController : MonoBehaviour {
    public AudioClip song;
    public int notefile = 1;
    public float offset;
    public float bpm = 80;
    float spb;
    string[] lines = null;
    int lineNum = -1;
    public GameObject[] hitboxes = new GameObject[4];
    int waitAmount;
    GameObject player;

    // Start is called before the first frame update
    void Start() {
        spb = (60 / bpm) / 4;
        player = GameObject.FindWithTag("Player");
        Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, Camera.main.transform.position.z);
        for (int i = 0; i < 4; i++)
            hitboxes[i] = GameObject.Find("HitBox" + (i + 1));

        TextAsset file = Resources.Load("Rhythm/note_file" + notefile) as TextAsset;
        lines = file.text.Split("\n"[0]);

        GetComponent<AudioSource>().PlayOneShot(song);
        StartCoroutine(PlayNotes());
    }
    IEnumerator PlayNotes() {
        if (waitAmount > 0)
            waitAmount--;
        else {
            string[] types = lines[++lineNum].Split(' ');
            if (types.Length == 1) {
                waitAmount = int.Parse(types[0]) - 1;
            }
            else {
                for (int i = 0; i < types.Length; i++) {
                    int type = int.Parse(types[i]);
                    if (type != 0) {
                        GameObject note = Resources.Load("Rhythm/Note") as GameObject;
                        if (type > 1) {

                        }
                        else {
                            Instantiate(note, hitboxes[i].transform.position + hitboxes[i].transform.right * 50, hitboxes[i].transform.rotation);
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(spb);
        StartCoroutine(PlayNotes());
    }
}
