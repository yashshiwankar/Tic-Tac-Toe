using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField roomNameInput;
    [SerializeField] TextMeshProUGUI errorText;

    public void CreateRoom()
    {
        if (roomNameInput.text == string.Empty)
        {
            errorText.text = "Enter valid room code";
            Debug.LogError("Enter valid room code");
            errorText.gameObject.SetActive(true);
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInput.text, new RoomOptions { MaxPlayers = 2 });
    }

    public void JoinRoom()
    {
        if (roomNameInput.text == string.Empty)
        {
            errorText.text = "Enter valid room code";
            Debug.LogError("Enter valid room code");
            errorText.gameObject.SetActive(true);
            return;
        }
        PhotonNetwork.JoinRoom(roomNameInput.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        errorText.gameObject.SetActive(false);
        PhotonNetwork.LoadLevel(Scene.TwoPlayerMatch.ToString()); // Load game for both players
    }
}
