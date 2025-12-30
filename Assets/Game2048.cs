using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class Game2048 : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Transform gameBoardTransform;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameOverPanel; // Reference to the Game Over UI

    [Header("Game Settings")]
    [SerializeField] private float inputDelay = 0.2f;

    private int[,] board = new int[4, 4];
    private GameObject[,] tileObjects = new GameObject[4, 4];
    private int score = 0;
    private float lastInputTime;
    private bool isGameOver = false; // Flag to stop input

    // Vivid Color Palette
    private readonly Dictionary<int, Color> tileColors = new Dictionary<int, Color>()
    {
        { 0, new Color32(205, 193, 180, 255) },
        { 2, new Color32(255, 255, 255, 255) },
        { 4, new Color32(255, 245, 200, 255) },
        { 8, new Color32(255, 170, 80, 255) },
        { 16, new Color32(255, 120, 50, 255) },
        { 32, new Color32(255, 80, 60, 255) },
        { 64, new Color32(255, 30, 30, 255) },
        { 128, new Color32(255, 230, 80, 255) },
        { 256, new Color32(255, 210, 60, 255) },
        { 512, new Color32(255, 190, 40, 255) },
        { 1024, new Color32(255, 170, 20, 255) },
        { 2048, new Color32(255, 215, 0, 255) }
    };

    private void Start() {
        // Set Background Color
        if (Camera.main != null) {
            Camera.main.backgroundColor = new Color32(250, 248, 239, 255);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }

        // Hide Game Over panel at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        InitializeBoardUI();
        StartGame();
    }

    private void Update() {
        // Stop input if game is over
        if (isGameOver) return;
        if (Time.time - lastInputTime < inputDelay) return;

        bool moved = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) moved = MoveLeft();
        else if (Input.GetKeyDown(KeyCode.RightArrow)) moved = MoveRight();
        else if (Input.GetKeyDown(KeyCode.UpArrow)) moved = MoveUp();
        else if (Input.GetKeyDown(KeyCode.DownArrow)) moved = MoveDown();

        if (moved) {
            SpawnTile();
            UpdateBoardVisuals();
            CheckGameOver();
            lastInputTime = Time.time;
        }
    }

    // --- Public Method for Button ---
    public void RestartGame() {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        isGameOver = false;
        StartGame();
    }

    void InitializeBoardUI() {
        foreach (Transform child in gameBoardTransform) Destroy(child.gameObject);

        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < 4; c++) {
                GameObject tile = Instantiate(tilePrefab, gameBoardTransform);
                tileObjects[r, c] = tile;

                TMP_Text textComp = tile.GetComponentInChildren<TMP_Text>();
                if (textComp != null) {
                    textComp.alignment = TextAlignmentOptions.Center;
                    textComp.enableAutoSizing = true;
                }
            }
        }
    }

    void StartGame() {
        board = new int[4, 4];
        score = 0;
        SpawnTile();
        SpawnTile();
        UpdateBoardVisuals();
    }

    void SpawnTile() {
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < 4; c++) {
                if (board[r, c] == 0) emptyTiles.Add(new Vector2Int(r, c));
            }
        }

        if (emptyTiles.Count > 0) {
            Vector2Int randomPos = emptyTiles[Random.Range(0, emptyTiles.Count)];
            board[randomPos.x, randomPos.y] = Random.value < 0.9f ? 2 : 4;
        }
    }

    void UpdateBoardVisuals() {
        scoreText.text = "Score: " + score;

        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < 4; c++) {
                int value = board[r, c];
                GameObject tile = tileObjects[r, c];

                Image bgImage = tile.GetComponent<Image>();
                if (tileColors.ContainsKey(value))
                    bgImage.color = tileColors[value];
                else
                    bgImage.color = tileColors[2048];

                TMP_Text textComp = tile.GetComponentInChildren<TMP_Text>();
                textComp.text = value > 0 ? value.ToString() : "";
                textComp.color = (value == 2 || value == 4) ? new Color32(119, 110, 101, 255) : Color.white;
            }
        }
    }

    // --- Movement Logic ---
    int[] SlideRow(int[] row) {
        List<int> filtered = row.Where(x => x != 0).ToList();

        for (int i = 0; i < filtered.Count - 1; i++) {
            if (filtered[i] == filtered[i + 1]) {
                filtered[i] *= 2;
                score += filtered[i];
                filtered[i + 1] = 0;
            }
        }

        filtered = filtered.Where(x => x != 0).ToList();
        while (filtered.Count < 4) filtered.Add(0);

        return filtered.ToArray();
    }

    bool MoveLeft() {
        bool hasChanged = false;
        for (int r = 0; r < 4; r++) {
            int[] row = { board[r, 0], board[r, 1], board[r, 2], board[r, 3] };
            int[] newRow = SlideRow(row);

            for (int c = 0; c < 4; c++) {
                if (board[r, c] != newRow[c]) hasChanged = true;
                board[r, c] = newRow[c];
            }
        }
        return hasChanged;
    }

    bool MoveRight() {
        bool hasChanged = false;
        for (int r = 0; r < 4; r++) {
            int[] row = { board[r, 3], board[r, 2], board[r, 1], board[r, 0] };
            int[] newRow = SlideRow(row);

            if (board[r, 3] != newRow[0]) hasChanged = true; board[r, 3] = newRow[0];
            if (board[r, 2] != newRow[1]) hasChanged = true; board[r, 2] = newRow[1];
            if (board[r, 1] != newRow[2]) hasChanged = true; board[r, 1] = newRow[2];
            if (board[r, 0] != newRow[3]) hasChanged = true; board[r, 0] = newRow[3];
        }
        return hasChanged;
    }

    bool MoveUp() {
        bool hasChanged = false;
        for (int c = 0; c < 4; c++) {
            int[] col = { board[0, c], board[1, c], board[2, c], board[3, c] };
            int[] newCol = SlideRow(col);

            for (int r = 0; r < 4; r++) {
                if (board[r, c] != newCol[r]) hasChanged = true;
                board[r, c] = newCol[r];
            }
        }
        return hasChanged;
    }

    bool MoveDown() {
        bool hasChanged = false;
        for (int c = 0; c < 4; c++) {
            int[] col = { board[3, c], board[2, c], board[1, c], board[0, c] };
            int[] newCol = SlideRow(col);

            if (board[3, c] != newCol[0]) hasChanged = true; board[3, c] = newCol[0];
            if (board[2, c] != newCol[1]) hasChanged = true; board[2, c] = newCol[1];
            if (board[1, c] != newCol[2]) hasChanged = true; board[1, c] = newCol[2];
            if (board[0, c] != newCol[3]) hasChanged = true; board[0, c] = newCol[3];
        }
        return hasChanged;
    }

    void CheckGameOver() {
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                if (board[r, c] == 0) return;

        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 3; c++)
                if (board[r, c] == board[r, c + 1]) return;

        for (int c = 0; c < 4; c++)
            for (int r = 0; r < 3; r++)
                if (board[r, c] == board[r + 1, c]) return;

        // --- Game Over Logic ---
        Debug.Log("Game Over!");
        isGameOver = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
}