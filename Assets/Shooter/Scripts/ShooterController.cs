using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShooterController : MonoBehaviour {
    public int enemySpawns = 1;
    int[,] enemies = null;

    public GameObject[] spawnSpaces;
    public GameObject powerCubeSpawn;
    public int totalEnemies;
    public int currentWave;
    public int waveEnemies;
    Text enemiesText;

    public static ShooterController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        GameController.Instance.InitHealth();
        enemiesText = GameObject.Find("EnemiesText").GetComponent<Text>();

        TextAsset file = Resources.Load("Shooter/enemy_spawns" + enemySpawns) as TextAsset;
        string[] lines = file.text.Split("\n"[0]);

        enemies = new int[3, lines.Length];

        int lineNum = -1;

        foreach (string line in lines) {
            lineNum++;
            string[] types = line.Split(' ');

            for (int i = 0; i < enemies.GetLength(0); i++) {
                enemies[i, lineNum] = int.Parse(types[i]);
                totalEnemies += int.Parse(types[i]);
            }
        }

        enemiesText.text = "Enemies: " + totalEnemies;
        spawnSpaces = GameObject.FindGameObjectsWithTag("Spawn");
        GameController.Instance.DoStartGame(AudioController.Instance.shooterMusic);
        Invoke("StartWave", 5f);
    }
    void StartWave() {
        GameController.Instance.ResetHealth();
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.enemySpawn);

        List<GameObject> spawnsLeft = new List<GameObject>();
        for (int i = 0; i < spawnSpaces.Length; i++)
            spawnsLeft.Add(spawnSpaces[i]);

        GameObject enemy = null;
        for (int i = 0; i < enemies.GetLength(0); i++) {
            int enemyCount = enemies[i, currentWave];
            waveEnemies += enemyCount;

            for (int j = 0; j < enemyCount; j++) {
                switch(i) {
                    case 0:
                        enemy = Resources.Load("Shooter/PistolEnemy") as GameObject;
                        break;
                    case 1:
                        enemy = Resources.Load("Shooter/MachineGunEnemy") as GameObject;
                        break;
                    case 2:
                        enemy = Resources.Load("Shooter/SniperEnemy") as GameObject;
                        break;
                }
                GameObject spawn = spawnsLeft[Random.Range(0, spawnsLeft.Count)];
                Instantiate(enemy, spawn.transform.position, spawn.transform.rotation);
                spawnsLeft.Remove(spawn);
            }
        }
        enemiesText.text = "Enemies: " + waveEnemies;
    }
    private void Update() {
        if (totalEnemies == 1)
            powerCubeSpawn = GameObject.FindWithTag("Enemy");
    }
    public void RemoveEnemy() {
        totalEnemies--;
        waveEnemies--;
        enemiesText.text = "Enemies: " + waveEnemies;
        if (waveEnemies <= 0) {
            if (totalEnemies > 0) {
                currentWave++;
                Invoke("StartWave", 2f);
            }
            else {
                GameObject.Find("ReticleTrigger").SetActive(false);
                GameObject.FindWithTag("PowerCube").transform.position = powerCubeSpawn.transform.position - Vector3.forward * 30;
            }
        }
    }
}
