using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private State currentPlayer = State.Player1;

    private State[] board = new State[9];
    [SerializeField] private Button[] cellArray = new Button[9];
    [SerializeField] TextMeshProUGUI turnText, resultText;
    [SerializeField] GameObject resultPanel;
    [SerializeField] GameObject pausePanel;   

    private int[][] winPatterns = new int[][]
{
        new int[]{0,1,2}, new int[]{3,4,5}, new int[]{6,7,8}, // rows
        new int[]{0,3,6}, new int[]{1,4,7}, new int[]{2,5,8}, // cols
        new int[]{0,4,8}, new int[]{2,4,6}                    // diagonals
};
    private void Start()
    {
        if(instance == null)
            instance = this;

        ResetBoard();
    }
    public void OnCellClicked(int index)
    {
        if (board[index] != State.Blank) return;

        board[index] = currentPlayer;

        //display symbol
        if (currentPlayer == State.Player1) 
        {
            cellArray[index].transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            cellArray[index].transform.GetChild(1).gameObject.SetActive(true);
        }
        cellArray[index].interactable = false;
        // Check win
        if (CheckWinner(currentPlayer))
        {
            EndGame(currentPlayer + " wins!");
            return;
        }

        // Check draw
        if (IsBoardFull())
        {
            EndGame("Draw!");
            return;
        }
        currentPlayer = (currentPlayer == State.Player1) ? State.Player2: State.Player1;
        UpdateText();
    }
    private void EndGame(string message)
    {
        turnText.text = "";
        resultText.text = message;
        resultPanel.SetActive(true);
        foreach (var btn in cellArray) btn.interactable = false;
    }

    void UpdateText()
    {
        turnText.text = (currentPlayer == State.Player1) ? "Player 1's Turn" : "Player 2's Turn";
    }
    private bool CheckWinner(State player)
    {
        foreach (var pattern in winPatterns)
        {
            if (board[pattern[0]] == player &&
                board[pattern[1]] == player &&
                board[pattern[2]] == player)
                return true;
        }
        return false;
    }
    public void PauseGame()
    {
        foreach (var cell in cellArray)
        {
            cell.interactable = false;
        }
        pausePanel.SetActive(true);
    }
    public void ResumeGame()
    {
        foreach (var cell in cellArray)
        {
            cell.interactable = true;
        }
        pausePanel.SetActive(false);
    }
    bool IsBoardFull()
    {
        foreach (var cell in board)
        {
            if (cell == State.Blank) return false;
        }
        return true;
    }
    void ResetBoard()
    {
        for (int i = 0; i < cellArray.Length; i++)
        {
            board[i] = State.Blank;
            cellArray[i].transform.GetChild(0).gameObject.SetActive(false);
            cellArray[i].transform.GetChild(1).gameObject.SetActive(false);
            cellArray[i].interactable = true;
        }
    }
}
