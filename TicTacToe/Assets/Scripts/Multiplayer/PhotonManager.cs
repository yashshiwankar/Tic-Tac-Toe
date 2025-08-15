using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject loadingPanel;
    [SerializeField] float loadingFadeDuration = 0.75f;
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings(); // Connect to Photon
        loadingPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon!");
        PhotonNetwork.JoinLobby(); // Go to lobby
        StartCoroutine("LoadingScreenOut");
    }


    IEnumerator LoadingScreenOut()
    {
        WaitForSeconds wait = new WaitForSeconds(loadingFadeDuration);
        loadingPanel.GetComponent<Image>().CrossFadeAlpha(0, loadingFadeDuration, true);
        yield return wait;
        loadingPanel.SetActive(false);
    }
}
