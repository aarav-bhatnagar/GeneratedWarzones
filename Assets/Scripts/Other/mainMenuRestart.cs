using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuRestart : NetworkBehaviour
{
    public void MainMenu()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Host clicked restart.");

            GameLoader.Instance.Disconnect.Value = true;
        }
        else
        {
            HandleDisconnect();
        }
    }

    private void Update()
    {
        print(GameLoader.Instance.Disconnect.Value);
        // All peers react when the network variable flips to true
        if (GameLoader.Instance.Disconnect.Value)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                HandleDisconnect();
            }
            else
            {
                // Host exits only after clients disconnect
                if (NetworkManager.Singleton.IsServer)
                {
                    if (NetworkManager.Singleton.ConnectedClients.Count <= 1)
                    {
                        HandleDisconnect();

                        NetworkManager.Singleton.Shutdown();
                    }
                }
            }
        }
    }

    private void HandleDisconnect()
    {
        if (NetworkManager.Singleton != null)
        {

            if (NetworkManager.Singleton.gameObject != null)
            {
                Destroy(NetworkManager.Singleton.gameObject);
            }
        }

        // Reset timescale
        Time.timeScale = 1f;

        // Load main menu safely
        SceneManager.LoadScene(0);
    }
}
