using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyScenesManager : MonoBehaviourPunCallbacks
{
    public static MyScenesManager instance;

    void Start()
    {
        if(instance == null)
            instance = this;    
    }

    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(((int)scene));
    }
    public void LoadLevelSelectScene()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        SceneManager.LoadScene(((int)Scene.LevelSelect));
    }
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(((int)Scene.MainMenu));
    }
    public void LoadTwoPlayerScene()
    {
        SceneManager.LoadScene(((int)Scene.TwoPlayerMatch));
    }
    public void LoadCompMatchScene()
    {
        SceneManager.LoadScene(((int)Scene.CompVsPlayer));
    }
    public void LoadMultplayerScene()
    {
        SceneManager.LoadScene(((int)Scene.Multiplayer));
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
