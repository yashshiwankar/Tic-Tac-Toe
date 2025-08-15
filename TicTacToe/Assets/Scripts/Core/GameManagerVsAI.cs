using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerVsAI : MonoBehaviour
{
    [Header("UI (optional assign in Inspector)")]
    public Button[] cellArray;                      // If you leave empty, script will auto-find using gridTag
    public string gridTag = "Grid";                 // Tag of the parent that contains 9 Button children
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI resultText;
    public GameObject resultPanel;
    public float aiMoveDelay = 0.5f;

    [SerializeField] GameObject pausePanel;

    // internal
    private State[] board = new State[9];
    private State currentPlayer = State.Player1;
    private readonly int[][] winPatterns = new int[][]
    {
        new int[]{0,1,2}, new int[]{3,4,5}, new int[]{6,7,8},
        new int[]{0,3,6}, new int[]{1,4,7}, new int[]{2,5,8},
        new int[]{0,4,8}, new int[]{2,4,6}
    };

    void Start()
    {
        // Auto-find buttons if not assigned
        if (cellArray == null || cellArray.Length != 9)
            TryAutoPopulateButtons();

        if (cellArray == null || cellArray.Length != 9)
        {
            Debug.LogError("GameManagerVsAI: cellArray not set and auto-find failed. Make sure Grid tag exists and has 9 Buttons as children.");
            enabled = false;
            return;
        }

        // Attach listeners safely
        for (int i = 0; i < cellArray.Length; i++)
        {
            int idx = i; // capture
            cellArray[i].onClick.RemoveAllListeners();
            cellArray[i].onClick.AddListener(() => OnCellClicked(idx));
        }

        ResetBoard();
        currentPlayer = State.Player1; // Human starts
        UpdateText();
    }

    void TryAutoPopulateButtons()
    {
        GameObject grid = GameObject.FindWithTag(gridTag);
        if (grid == null) return;
        var buttons = grid.GetComponentsInChildren<Button>(true);
        if (buttons.Length >= 9)
        {
            cellArray = new Button[9];
            // assuming first 9 children are the board cells in correct order
            for (int i = 0; i < 9; i++)
                cellArray[i] = buttons[i];
        }
    }

    public void OnCellClicked(int index)
    {
        if (currentPlayer != State.Player1) return;
        Debug.Log($"Clicked Index: {index} | CurrentPlayer: {currentPlayer}");

        if (board[index] != State.Blank)
        {
            Debug.Log("Cell already filled!");
            return;
        }

        // Player move (offline human)
        SoundManager.instance.PlaySoundButton();
        MakeMove(index, State.Player1);

        // After player move, if game not over, trigger AI
        if (!CheckWinner(State.Player1) && !IsBoardFull())
        {
            currentPlayer = State.Player2;
            UpdateText();
            StartCoroutine(AIMoveWithDelay());
        }
    }

    private IEnumerator AIMoveWithDelay()
    {
        yield return new WaitForSeconds(aiMoveDelay);

        int best = GetBestMove();
        if (best < 0)
        {
            Debug.LogWarning("AI found no move (board full?)");
            yield break;
        }

        MakeMove(best, State.Player2);

        // If AI wins or draw, end handled inside MakeMove
        if (CheckWinner(State.Player2))
        {
            EndGame("AI wins!");
            yield break;
        }
        if (IsBoardFull())
        {
            EndGame("Draw!");
            yield break;
        }

        // Back to player
        currentPlayer = State.Player1;
        UpdateText();
    }

    // Core move placer (local only)
    private void MakeMove(int index, State player)
    {
        board[index] = player;

        // find child "X" or "O" and enable it
        string childName = (player == State.Player1) ? "X" : "O";
        Transform child = cellArray[index].transform.Find(childName);
        if (child == null)
        {
            Debug.LogWarning($"Cell {index} missing child '{childName}'. Attempting to enable first child instead.");
            if (cellArray[index].transform.childCount > 0)
                cellArray[index].transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            child.gameObject.SetActive(true);
        }

        cellArray[index].interactable = false;
        Debug.Log($"Placed {childName} at {index}");

        // Win or draw check
        if (CheckWinner(player))
        {
            EndGame($"{(player == State.Player1 ? "You" : "AI")} wins!");
            return;
        }
        if (IsBoardFull())
        {
            EndGame("Draw!");
            return;
        }
    }

    private void EndGame(string message)
    {
        turnText.text = "";
        resultText.text = message;
        resultPanel.SetActive(true);
        foreach (var b in cellArray) b.interactable = false;
    }

    private void UpdateText()
    {
        turnText.text = (currentPlayer == State.Player1) ? "Your Turn" : "AI is thinking...";
    }

    private bool CheckWinner(State player)
    {
        foreach (var p in winPatterns)
        {
            if (board[p[0]] == player && board[p[1]] == player && board[p[2]] == player)
                return true;
        }
        return false;
    }

    private bool IsBoardFull()
    {
        foreach (var s in board)
            if (s == State.Blank) return false;
        return true;
    }

    private void ResetBoard()
    {
        for (int i = 0; i < 9; i++)
        {
            board[i] = State.Blank;
            if (cellArray[i] != null)
            {
                // Disable named children if present
                Transform x = cellArray[i].transform.Find("X");
                Transform o = cellArray[i].transform.Find("O");
                if (x != null) x.gameObject.SetActive(false);
                if (o != null) o.gameObject.SetActive(false);

                // if no child names, hide all children
                if (x == null && o == null)
                {
                    for (int c = 0; c < cellArray[i].transform.childCount; c++)
                        cellArray[i].transform.GetChild(c).gameObject.SetActive(false);
                }

                cellArray[i].interactable = true;
            }
        }

        if (resultPanel != null) resultPanel.SetActive(false);
    }

    // --- Minimax AI ---
    private int GetBestMove()
    {
        int bestScore = int.MinValue;
        int move = -1;

        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == State.Blank)
            {
                board[i] = State.Player2;
                int score = Minimax(board, 0, false);
                board[i] = State.Blank;

                if (score > bestScore)
                {
                    bestScore = score;
                    move = i;
                }
            }
        }

        return move;
    }

    private int Minimax(State[] newBoard, int depth, bool isMaximizing)
    {
        if (CheckWinner(State.Player2)) return 10 - depth;
        if (CheckWinner(State.Player1)) return -10 + depth;
        if (IsBoardFull()) return 0;

        if (isMaximizing)
        {
            int best = int.MinValue;
            for (int i = 0; i < newBoard.Length; i++)
            {
                if (newBoard[i] == State.Blank)
                {
                    newBoard[i] = State.Player2;
                    int score = Minimax(newBoard, depth + 1, false);
                    newBoard[i] = State.Blank;
                    best = Mathf.Max(score, best);
                }
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            for (int i = 0; i < newBoard.Length; i++)
            {
                if (newBoard[i] == State.Blank)
                {
                    newBoard[i] = State.Player1;
                    int score = Minimax(newBoard, depth + 1, true);
                    newBoard[i] = State.Blank;
                    best = Mathf.Min(score, best);
                }
            }
            return best;
        }
    }

    public void PauseGame()
    {
        foreach (var cell in cellArray)
            cell.interactable = false;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        UpdateText();
        pausePanel.SetActive(false);
    }
}
