using UnityEngine;

public class QuitGameButton : MonoBehaviour
{
    // Call this from the OnClick() of your "Quit Game" button.
    public void DoQuitGame()
    {
        Debug.Log("[QuitGameButton] Quit Game requested.");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}