using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SceneHandler : MonoBehaviour
{
    public VideoPlayer player;
    AsyncOperation scene;
    private void Start()
    {
        player.loopPointReached += ChangeScene;

        scene = SceneManager.LoadSceneAsync("Home");
        scene.allowSceneActivation = false;
    }

    public void ChangeScene(VideoPlayer vp)
    {
        if(player != null)
        {
            scene.allowSceneActivation = true;
        }
    }
}
