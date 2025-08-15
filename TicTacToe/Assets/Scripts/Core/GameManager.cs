using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    private State currentPlayer = State.Player1;
    private State[] board = new State[9];

    [SerializeField] private Button[] cellArray;
    [SerializeField] private TextMeshProUGUI turnText, resultText, playerInfoText, p1ScoreText, p2ScoreText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TextMeshProUGUI rematchText;
    [SerializeField] private Button retryBtn;
    private bool wantsRematch;
    private GameObject grid;

    private int p1Score = 0, p2Score = 0;


    private int[][] winPatterns = new int[][]
    {
        new int[]{0,1,2}, new int[]{3,4,5}, new int[]{6,7,8}, // rows
        new int[]{0,3,6}, new int[]{1,4,7}, new int[]{2,5,8}, // cols
        new int[]{0,4,8}, new int[]{2,4,6}                    // diagonals
    };

    private void Start()
    {
        wantsRematch = false;
        if (instance == null)
            instance = this;

        // Auto-populate buttons
        grid = GameObject.FindWithTag("Grid");
        cellArray = new Button[9];
        for (int i = 0; i < grid.transform.childCount; i++)
            cellArray[i] = grid.transform.GetChild(i).GetComponent<Button>();

        ResetBoard();

        // Mode handling
        if (!PhotonNetwork.IsConnected) // Offline
        {
            currentPlayer = State.Player1;
            playerInfoText.text = "Offline Mode";
        }
        else // Online
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("SetCurrentPlayer", RpcTarget.All, (int)State.Player1);
            }
            playerInfoText.text = PhotonNetwork.IsMasterClient ? "You are Player 1 (X)" : "You are Player 2 (O)";

        }

        UpdateText();
        UpdateInteraction();
    }

    public void OnCellClicked(int index)
    {
        // Prevent clicking filled cell
        if (board[index] != State.Blank)
            return;

        // Block wrong player's turn in online mode
        if (PhotonNetwork.IsConnected)
        {
            if ((currentPlayer == State.Player1 && !PhotonNetwork.IsMasterClient) ||
                (currentPlayer == State.Player2 && PhotonNetwork.IsMasterClient))
                return;
        }

        // Make move (offline or online)
        if (PhotonNetwork.IsConnected)
            photonView.RPC("MakeMove", RpcTarget.All, index, (int)currentPlayer);
        else
            MakeMove(index, (int)currentPlayer);
    }

    [PunRPC]
    void MakeMove(int index, int playerInt)
    {
        State player = (State)playerInt;
        board[index] = player;

        // Display symbol
        //cellArray[index].transform.Find(player == State.Player1 ? "X" : "O").gameObject.SetActive(true);
        if(player == State.Player1)
        {
            cellArray[index].transform.Find("X").gameObject.SetActive(true);
        }
        else if (player == State.Player2)
        {
            cellArray[index].transform.Find("O").gameObject.SetActive(true);
        }
        cellArray[index].interactable = false;

        // Check win
        if (CheckWinner((int)player))
        {
            EndGame(player + " wins!");
            return;
        }

        // Check draw
        if (IsBoardFull())
        {
            EndGame("Draw!");
            return;
        }

        SoundManager.instance.PlaySoundButton();
        // Switch turn
        currentPlayer = (player == State.Player1) ? State.Player2 : State.Player1;
        UpdateText();
        UpdateInteraction();
    }
    public void OnRestartClicked()
    {
        if (!PhotonNetwork.IsConnected)
        {
            ResetBoard();
            resultPanel.SetActive(false);
            return;
        }

        wantsRematch = true;
        photonView.RPC("NotifyRematchRequest", RpcTarget.Others);
        CheckRematch();
    }
    [PunRPC]
    void NotifyRematchRequest()
    {
        rematchText.text = "Opponent wants a rematch";
        wantsRematch = true;
        CheckRematch();
    }

    void CheckRematch()
    {
        // If both players want rematch
        if (wantsRematch && PhotonNetwork.IsMasterClient == false)
            photonView.RPC("ResetBoardRPC", RpcTarget.All);
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

    [PunRPC]
    void SetCurrentPlayer(int playerValue)
    {
        currentPlayer = (State)playerValue;
        UpdateInteraction();
        UpdateText(); // if you're showing turn info
    }

    void UpdateInteraction()
    {
        if (!PhotonNetwork.IsConnected) return; // In offline mode, all buttons stay interactable

        bool myTurn = (currentPlayer == State.Player1 && PhotonNetwork.IsMasterClient) ||
                      (currentPlayer == State.Player2 && !PhotonNetwork.IsMasterClient);

        for (int i = 0; i < cellArray.Length; i++)
        {
            if (board[i] == State.Blank)
                cellArray[i].interactable = myTurn;
            else
                cellArray[i].interactable = false;
        }
    }


    private bool CheckWinner(int player)
    {
        foreach (var pattern in winPatterns)
            if (board[pattern[0]] == (State)player &&
                board[pattern[1]] == (State)player &&
                board[pattern[2]] == (State)player)
            {
                if(player == (int)State.Player1)
                {
                    p1Score++;
                    p1ScoreText.text = $"P1: {p1Score.ToString()}";
                }
                else if (player == (int)State.Player2)
                {
                    p2Score++;
                    p2ScoreText.text = $"P2: {p2Score.ToString()}";
                }
                return true;
            }
        return false;
    }

    public void PauseGame()
    {
        foreach (var cell in cellArray)
            cell.interactable = false;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        UpdateInteraction();
        pausePanel.SetActive(false);
    }

    bool IsBoardFull()
    {
        foreach (var cell in board)
            if (cell == State.Blank) return false;
        return true;
    }

    [PunRPC]
    void ResetBoardRPC()
    {
        resultPanel.SetActive(false);
        rematchText.text = "";
        wantsRematch = false;
        ResetBoard();
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SetCurrentPlayer", RpcTarget.All, (int)State.Player1);
        }

    }
    void ResetBoard()
    {
        for (int i = 0; i < cellArray.Length; i++)
        {
            board[i] = State.Blank;
            cellArray[i].transform.Find("X").gameObject.SetActive(false);
            cellArray[i].transform.Find("O").gameObject.SetActive(false);
            cellArray[i].interactable = true;
        }
    }

    public void OnLeaveButton()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.LeaveRoom();
        else
            MyScenesManager.instance.LoadLevelSelectScene();
    }

    public override void OnLeftRoom()
    {
        MyScenesManager.instance.LoadLevelSelectScene();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        resultText.text = "Opponent left the match!";
        resultPanel.SetActive(true);
        retryBtn.gameObject.SetActive(false);
    }

}
