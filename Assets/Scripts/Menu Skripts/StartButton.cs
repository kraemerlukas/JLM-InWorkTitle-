using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void StartGame(string _mode)
    {
        SceneManager.LoadScene(_mode);
    }
   public void NormalGame()
    {
        SceneManager.LoadScene("Normal");
    }
   
}
