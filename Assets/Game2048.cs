using UnityEngine;
using UnityEngine.UI;
using TMPro; // using TextMeshPro for sharp text
using System.Collections.Generic;
using System.Linq;

public class Game2048 : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Transform gameBoardTransform; // The Panel with GridLayoutGroup
    [SerializeField] private GameObject tilePrefab;        // Prefab with Image & TMP_Text
    [SerializeField] private TMP_Text scoreText;

    [Header("Game Settings")]
    [SerializeField] private float inputDelay = 0.2f;      // Prevent accidental double swipes

    private int[,] board = new int[4, 4];
    private GameObject[,] tileObjects = new GameObject[4, 4];
    private int score = 0;
    private float lastInputTime;

    // Dictionary to map values to original HTML colors
    private readonly Dictionary<int, Color> tileColors = new Dictionary<int, Color>()
    {
        { 0, new Color32(205, 193, 180, 255) },    // Empty
        { 2, new Color32(238, 228, 218, 255) },
        { 4, new Color32(237, 224, 200, 255) },
        { 8, new Color32(242, 177, 121, 255) },
        { 16, new Color32(245, 149, 99, 255) },
        { 32, new Color32(246, 124, 95, 255) },
        { 64, new Color32(246, 94, 59, 255) },
        { 128, new Color32(237, 207, 114, 255) },
        { 256, new Color32(237, 204, 97, 255) },
        { 512, new Color32(237, 200, 80, 255) },
        { 1024, new Color32(237, 197, 63, 255) },
        { 2048, new Color32(237, 194, 46, 255) }
    };

    private void Start() {
        InitializeBoardUI();
        StartGame();
    }

    private void Update() {
        // Input Handling with simple delay check
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

    // --- Initialization ---
    void InitializeBoardUI() {
        // Clear existing children if any
        foreach (Transform child in gameBoardTransform) Destroy(child.gameObject);

        // Create 16 slots
        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < 4; c++) {
                GameObject tile = Instantiate(tilePrefab, gameBoardTransform);
                tileObjects[r, c] = tile;
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

    // --- Core Logic (Translated from JS) ---
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

    // Updates the UI (Color and Text) based on the int array
    void UpdateBoardVisuals() {
        scoreText.text = "Score: " + score;

        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < 4; c++) {
                int value = board[r, c];
                GameObject tile = tileObjects[r, c];

                // Update Image Color
                Image bgImage = tile.GetComponent<Image>();
                if (tileColors.ContainsKey(value))
                    bgImage.color = tileColors[value];
                else
                    bgImage.color = tileColors[2048]; // Fallback for higher numbers

                // Update Text
                TMP_Text textComp = tile.GetComponentInChildren<TMP_Text>();
                textComp.text = value > 0 ? value.ToString() : "";

                // Text Color (Dark for 2/4, Light for others logic from CSS)
                textComp.color = (value == 2 || value == 4) ? new Color32(119, 110, 101, 255) : Color.white;
            }
        }
    }

    // --- Movement Logic ---
    // C# Implementation of the JS "slideRow" logic
    int[] SlideRow(int[] row) {
        // 1. Filter out zeros
        List<int> filtered = row.Where(x => x != 0).ToList();

        // 2. Merge
        for (int i = 0; i < filtered.Count - 1; i++) {
            if (filtered[i] == filtered[i + 1]) {
                filtered[i] *= 2;
                score += filtered[i];
                filtered[i + 1] = 0; // Mark for removal
            }
        }

        // 3. Filter zeros again (post-merge)
        filtered = filtered.Where(x => x != 0).ToList();

        // 4. Pad with zeros
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
            int[] row = { board[r, 3], board[r, 2], board[r, 1], board[r, 0] }; // Reverse
            int[] newRow = SlideRow(row);

            // Assign back in reverse
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
            int[] col = { board[3, c], board[2, c], board[1, c], board[0, c] }; // Reverse
            int[] newCol = SlideRow(col);

            // Assign back in reverse
            if (board[3, c] != newCol[0]) hasChanged = true; board[3, c] = newCol[0];
            if (board[2, c] != newCol[1]) hasChanged = true; board[2, c] = newCol[1];
            if (board[1, c] != newCol[2]) hasChanged = true; board[1, c] = newCol[2];
            if (board[0, c] != newCol[3]) hasChanged = true; board[0, c] = newCol[3];
        }
        return hasChanged;
    }

    void CheckGameOver() {
        // 1. Look for empty spots
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                if (board[r, c] == 0) return;

        // 2. Look for horizontal matches
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 3; c++)
                if (board[r, c] == board[r, c + 1]) return;

        // 3. Look for vertical matches
        for (int c = 0; c < 4; c++)
            for (int r = 0; r < 3; r++)
                if (board[r, c] == board[r + 1, c]) return;

        Debug.Log("Game Over! Score: " + score);
        // Optional: Add UI Panel for Game Over here
    }
}