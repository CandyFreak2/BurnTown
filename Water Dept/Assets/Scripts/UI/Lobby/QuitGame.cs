using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void TriggerQuit(float delay)
    {
        Debug.Log("Quit Application called. This is not a networked Function");
        Invoke("Quit", delay);
    }

    void Quit()
    {
        Application.Quit();
    }
}
