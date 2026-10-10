using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapCamera : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<Camera>().orthographicSize = 86;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1)
        {
            //Lobby
            GetComponent<Camera>().orthographicSize = 86;
        }
        else if (scene.buildIndex == 2)
        {
            GetComponent<Camera>().orthographicSize = 270;
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
