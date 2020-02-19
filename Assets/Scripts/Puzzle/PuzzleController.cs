using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour {
    public GameObject[,] boardPositionGrid;
    public GameObject[,] puzzlePieceGrid;
    GameObject boardPositions;
    GameObject puzzlePieces;
    GameObject cam;
    GameObject puzzlePiece;
    GameObject cursor;
    GameObject enemy;
    GameObject eye;
    GameObject arm;
    GameObject block;
    Transform[] playerHealth;
    public float attackTime = 3f;
    public int healthCount;
    public int cursorX;
    public int cursorY;
    public int emptyX;
    public int emptyY;
    int attackX;
    int attackY;
    int boardSize = 4;
    public bool canSelect = true;
    public bool canMove = true;
    bool isAttacking;
    bool won;
    float xMargin = 0;
    float yMargin = 0;
    string folderName = "Circle";

    public static PuzzleController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }
    // Start is called before the first frame update
    void Start() {
        boardPositions = GameObject.Find("BoardPositions");
        puzzlePieces = GameObject.Find("PuzzlePieces");
        cam = GameObject.FindWithTag("MainCamera");
        puzzlePiece = Resources.Load("Puzzle/PuzzlePiece") as GameObject;
        cursor = GameObject.Find("Cursor");
        enemy = GameObject.FindWithTag("Enemy");
        eye = GameObject.Find("Enemy/Eye");
        arm = GameObject.Find("Enemy/Eye/Arm");
        block = GameObject.FindWithTag("Damage");
        playerHealth = GameObject.Find("PlayerHealth").GetComponentsInChildren<Transform>();
        healthCount = playerHealth.Length - 1;

        Bounds puzzleSize = (Resources.Load("Puzzle/PuzzlePiece") as GameObject).GetComponent<Renderer>().bounds;
        Vector3 camPos = cam.transform.position;
        boardPositionGrid = new GameObject[boardSize, boardSize];
        puzzlePieceGrid = new GameObject[boardSize, boardSize];

        int amount = 0;
        for (int y = 0; y < boardSize; y++) {
            for (int x = 0; x < boardSize; x++) {
                amount++;
                GameObject bp = new GameObject();
                bp.name = "BoardPosition|" + amount;
                bp.transform.position = new Vector3(camPos.x + (puzzleSize.size.x + xMargin) * x, camPos.y - (puzzleSize.size.y + yMargin) * y, camPos.z + 200);
                bp.transform.SetParent(boardPositions.transform);
                boardPositionGrid[x, y] = bp;
                GameObject pp = Instantiate(Resources.Load("Puzzle/PuzzlePiece") as GameObject);
                pp.name = "PuzzlePiece|" + amount;
                pp.transform.position = bp.transform.position;
                pp.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Puzzle/" + folderName + "/image_" + amount);
                pp.transform.SetParent(puzzlePieces.transform);
                puzzlePieceGrid[x, y] = pp;
            }
        }
        GameObject image = GameObject.Find("Canvas/Image");
        image.GetComponent<Image>().sprite = Resources.Load<Sprite>("Puzzle/" + folderName + "/image_0");

        for (int y = 0; y < boardSize; y++) {
            for (int x = 0; x < boardSize; x++) {
                GameObject p = puzzlePieceGrid[x, y];
                int randX = Random.Range(0, boardSize - 1);
                int randY = Random.Range(0, boardSize - 1);

                puzzlePieceGrid[x, y] = puzzlePieceGrid[randX, randY];
                puzzlePieceGrid[randX, randY] = p;
                p.transform.position = boardPositionGrid[randX, randY].transform.position;
                puzzlePieceGrid[x, y].transform.position = boardPositionGrid[x, y].transform.position;
            }
        }
        int destroyX = Random.Range(0, boardSize - 1);
        int destroyY = Random.Range(0, boardSize - 1);
        Destroy(puzzlePieceGrid[destroyX, destroyY]);
        emptyX = cursorX = destroyX;
        emptyY = cursorY = destroyY;

        if (emptyX - 1 >= 0)
            cursorX--;
        else
            cursorX++;
        cursor.transform.position = new Vector3(boardPositionGrid[cursorX, emptyY].transform.position.x, boardPositionGrid[cursorX, emptyY].transform.position.y, cursor.transform.position.z);
        enemy.transform.position = new Vector3(boardPositionGrid[emptyX, emptyY].transform.position.x, boardPositionGrid[emptyX, emptyY].transform.position.y, cursor.transform.position.z - 1);
        cam.transform.position = new Vector2(boardPositionGrid[0, 0].transform.position.x 
                                    - (puzzleSize.size.x / 2) + (puzzleSize.size.x * boardSize) / 2, 
                                    boardPositionGrid[0, 0].transform.position.y 
                                    + (puzzleSize.size.y / 2) - (puzzleSize.size.y * boardSize) / 2);
    }
    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0)
            return;
        if (attackTime > 0)
            attackTime -= Time.deltaTime;
        else {
            if (!isAttacking) {
                isAttacking = true;
                int tempX = emptyX - cursorX;
                int tempY = emptyY - cursorY;
                attackX = cursorX;
                attackY = cursorY;
                if (tempX != 0) {
                    eye.transform.localPosition = new Vector3(-0.6f * tempX, 0, 0);
                    eye.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                }
                if (tempY != 0) {
                    eye.transform.localPosition = new Vector3(0, 0.6f * tempY, 0);
                    eye.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                Invoke("DoAttack", 1);
            }
        }
        if (canSelect && canMove && !won) {
            if (Input.GetButtonDown("Horizontal")) {
                canSelect = false;
                MoveX(Input.GetAxisRaw("Horizontal"));
            }
            else {
                if (Input.GetButtonDown("Vertical")) {
                    canSelect = false;
                    MoveY(Input.GetAxisRaw("Vertical"));
                }
                else {
                    if (Input.GetButtonDown("Action 1") && !isAttacking) {
                        canSelect = false;
                        canMove = false;
                        DoMovePiece();
                    }
                }
            }
        }
        if (won) {
            if (Input.GetButtonDown("Horizontal")) {
                canSelect = false;
                WinMoveX(Input.GetAxisRaw("Horizontal"));
            }
            else {
                if (Input.GetButtonDown("Vertical")) {
                    canSelect = false;
                    WinMoveY(Input.GetAxisRaw("Vertical"));
                }
                else {
                    if (Input.GetButtonDown("Action 1")) {
                        CheckCube();
                    }
                }
            }
        }
    }
    void CheckBoard() {
        bool clear = true;
        for (int y = 0; y < boardSize; y++) {
            for (int x = 0; x < boardSize; x++) {
                if (!(x == emptyX && y == emptyY)) {
                    string board = boardPositionGrid[x, y].name;
                    board = board.Split('|')[1];
                    string piece = puzzlePieceGrid[x, y].name;
                    piece = piece.Split('|')[1];
                    if (!(board.Equals(piece))) {
                        clear = false;
                        break;
                    }
                }
            }
            if (!clear)
                break;
        }
        if (clear) {
            Destroy(enemy);
            GameObject.FindWithTag("PowerCube").transform.position = boardPositionGrid[emptyX, emptyY].transform.position;
            won = true;
        }          
    }
    void DoAttack() {
        StartCoroutine(Attack());
    }
    public void DamagePlayer() {
        if (attackX == cursorX && attackY == cursorY) {
            healthCount--;
            if (healthCount <= 0)
                GameController.Instance.ResetLevel();
            else
                Destroy(playerHealth[healthCount + 1].gameObject);
        }
    }
    void MoveX(float direction) {
        if (direction == 0) {
            canSelect = true;
            return;
        }
        float newX = emptyX + direction;

        if (newX >= 0 && newX <= boardSize - 1) {
            cursorX = (int)newX;
            cursorY = emptyY;
            StartCoroutine(MoveCursor());
        }
        else
            canSelect = true;
    }
    void MoveY(float direction) {
        if (direction == 0) {
            canSelect = true;
            return;
        }
        float newY = emptyY - direction;

        if (newY >= 0 && newY <= boardSize - 1) {
            cursorY = (int)newY;
            cursorX = emptyX;
            StartCoroutine(MoveCursor());
        }
        else
            canSelect = true;
    }
    void WinMoveX(float direction) {
        if (direction == 0) {
            canSelect = true;
            return;
        }
        float newX = cursorX + direction;

        if (newX >= 0 && newX <= boardSize - 1) {
            cursorX = (int)newX;
            StartCoroutine(MoveCursor());
        }
        else
            canSelect = true;
    }
    void WinMoveY(float direction) {
        if (direction == 0) {
            canSelect = true;
            return;
        }
        float newY = cursorY - direction;

        if (newY >= 0 && newY <= boardSize - 1) {
            cursorY = (int)newY;
            StartCoroutine(MoveCursor());
        }
        else
            canSelect = true;
    }
    void CheckCube() {
        if (cursorX == emptyX && cursorY == emptyY)
            GameController.Instance.CompleteLevel();
    }
    void DoMovePiece() {
        int tempX = cursorX;
        int tempY = cursorY;
        cursorX = emptyX;
        cursorY = emptyY;
        emptyX = tempX;
        emptyY = tempY;
        StartCoroutine(MovePiece());
    }
    IEnumerator Attack() {
        Quaternion armRotation = Quaternion.Euler(Vector2.zero);

        while (Quaternion.Angle(arm.transform.localRotation, armRotation) > 0.1f) {
            arm.transform.localRotation = Quaternion.Slerp(arm.transform.localRotation, armRotation, Time.deltaTime * 10);
            yield return null;
        }
        arm.transform.localRotation = armRotation;
        block.transform.position = boardPositionGrid[attackX, attackY].transform.position + Vector3.back * 1;

        yield return new WaitForSeconds(0.5f);

        block.transform.position = new Vector3(500, 500, 0);
        arm.transform.localRotation = Quaternion.Euler(new Vector2(90, 0));
        eye.transform.localPosition = Vector2.zero;
        attackTime = 5f;
        isAttacking = false;
    }
    IEnumerator MoveCursor() {
        enemy.transform.position = new Vector3(boardPositionGrid[emptyX, emptyY].transform.position.x, boardPositionGrid[emptyX, emptyY].transform.position.y, cursor.transform.position.z - 1);
        Vector3 cursorPosition = new Vector3(boardPositionGrid[cursorX, cursorY].transform.position.x, boardPositionGrid[cursorX, cursorY].transform.position.y, cursor.transform.position.z);

        while (Vector3.Distance(cursor.transform.position, cursorPosition) > 1) {
            cursor.transform.position = Vector3.MoveTowards(cursor.transform.position, cursorPosition, Time.deltaTime * 500);
            yield return null;
        }
        cursor.transform.position = cursorPosition;
        canSelect = true;
    }
    IEnumerator MovePiece() {
        Vector3 piecePosition = boardPositionGrid[cursorX, cursorY].transform.position;
        StartCoroutine(MoveCursor());

        while (Vector3.Distance(puzzlePieceGrid[emptyX, emptyY].transform.position, piecePosition) > 1) {
            puzzlePieceGrid[emptyX, emptyY].transform.position = Vector3.MoveTowards(puzzlePieceGrid[emptyX, emptyY].transform.position, piecePosition, Time.deltaTime * 500);
            yield return null;
        }
        puzzlePieceGrid[emptyX, emptyY].transform.position = piecePosition;
        puzzlePieceGrid[cursorX, cursorY] = puzzlePieceGrid[emptyX, emptyY];
        puzzlePieceGrid[emptyX, emptyY] = null;
        canMove = true;
        CheckBoard();
    }
}
