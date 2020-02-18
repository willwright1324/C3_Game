using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour {
    public GameObject[,] boardPositionGrid;
    public GameObject[,] puzzlePieceGrid;
    GameObject boardPositions;
    GameObject puzzlePieces;
    GameObject cam;
    GameObject puzzlePiece;
    GameObject cursor;
    public int cursorX;
    public int cursorY;
    public int emptyX;
    public int emptyY;
    int boardSize = 4;
    public bool canSelect = true;
    public bool canMove = true;
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
        cam.transform.position = new Vector2(boardPositionGrid[0, 0].transform.position.x 
                                    - (puzzleSize.size.x / 2) + (puzzleSize.size.x * boardSize) / 2, 
                                    boardPositionGrid[0, 0].transform.position.y 
                                    + (puzzleSize.size.y / 2) - (puzzleSize.size.y * boardSize) / 2);
    }
    // Update is called once per frame
    void Update() {
        if (Time.timeScale == 0)
            return;
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
                    if (Input.GetButtonDown("Action 1")) {
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
            GameObject.FindWithTag("PowerCube").transform.position = boardPositionGrid[emptyX, emptyY].transform.position;
            won = true;
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
    IEnumerator MoveCursor() {
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
