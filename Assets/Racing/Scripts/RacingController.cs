using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingController : MonoBehaviour {
    public int level = 1;
    public int laps = 3;
    int currentLap = 0;
    GameObject player;
    GameObject temps;
    GameObject trackPieces;
    GameObject[,] tracks;
    int[,] trackCoords = null;
    int startX;
    int startY;
    int trackCount;

    GameObject enemy;
    GameObject enemyPath;

    public static RacingController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        player = GameObject.FindWithTag("Player");
        enemy = GameObject.FindWithTag("Enemy");
        enemyPath = GameObject.Find("EnemyPath");

        float trackSize = (Resources.Load("Racing/StraightTrack") as GameObject).transform.lossyScale.x;

        TextAsset file = Resources.Load("Racing/racing_track" + level) as TextAsset;
        string[] lines = file.text.Split("\n"[0]);
        tracks = new GameObject[lines.Length, lines.Length];

        temps = new GameObject("Temps");
        trackPieces = new GameObject("Tracks");

        int lineNum = -1;

        foreach (string line in lines) {
            lineNum++;
            string[] types = line.Split(' ');
            int length = types.Length;
            for (int i = 0; i < length; i++) {
                if (types[i].StartsWith("0"))
                    continue;
                GameObject t = new GameObject("Temp");
                int rot = int.Parse(types[i].Substring(1));
                t.transform.SetParent(temps.transform);

                if (types[i].StartsWith("S") || types[i].StartsWith("B")) {
                    t = Instantiate(Resources.Load("Racing/StraightTrack") as GameObject);
                    t.transform.SetParent(trackPieces.transform);

                }
                if (types[i].StartsWith("L") || types[i].StartsWith("R")) {
                    t = Instantiate(Resources.Load("Racing/CornerTrack") as GameObject);
                    t.transform.SetParent(trackPieces.transform);

                    if (types[i].StartsWith("L")) {
                        t.transform.localScale = new Vector2(-t.transform.localScale.x, t.transform.localScale.y);
                        rot = -rot;
                    }
                }
                t.transform.rotation = Quaternion.Euler(0, 0, 90 * rot);
                float size = t.transform.lossyScale.y * 10;
                t.transform.position = new Vector3((player.transform.position.x - ((length * size) / 2) + (size / 2) + (i * size)), 
                                                    player.transform.position.y + ((length * size) / 2) - (size / 2) - (lineNum * size), 
                                                    player.transform.position.z + 1);

                Track tt = t.GetComponent<Track>();
                tt.coordX = i;
                tt.coordY = lineNum;
                tracks[i, lineNum] = t;
                if (types[i].StartsWith("B")) {
                    startX = i;
                    startY = lineNum;
                }
            }
        }
        GameObject start = tracks[startX, startY];
        enemy.transform.rotation = player.transform.rotation = start.transform.rotation;
        player.transform.position = start.transform.position + Vector3.back + Vector3.left * 30;
        enemy.transform.position = enemyPath.transform.position = player.transform.position + Vector3.right * 60;
        trackCoords = new int[lineNum, lineNum];
        Destroy(temps);
        trackCount = tracks.Length;
    }

    // Update is called once per frame
    void Update() {
        
    }
    public void CheckLap() {
        foreach (int track in trackCoords) {
            if (track == 0)
                return;
        }
        if (currentLap < laps) {
            currentLap++;
        }
        else {

        }
    }
}
