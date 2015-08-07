using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Controls the main menu.
/// </summary>
public class MainMenu : MonoBehaviour
{

    public Canvas quitMenu;
    public Button startButton;
    public Button quitButton;
    public Button optionsButton;
    public OptionsMenu optionsMenu;

    // Use this for initialization
    void Start()
    {
        quitMenu.transform.gameObject.SetActive(true);
        quitMenu.enabled = false;
    }

    public void ExitPress()
    {
        quitMenu.enabled = true;
        ToggleButtons(false);
    }

    public void NoQuitPress()
    {
        quitMenu.enabled = false;
        ToggleButtons(true);
    }

    private void ToggleButtons(bool enable)
    {
        startButton.enabled = enable;
        quitButton.enabled = enable;
        optionsButton.enabled = enable;
    }

    public void StartLevel()
    {
        Application.LoadLevel(1);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowOptions()
    {
        var mainMenu = gameObject.GetComponent<Canvas>();
        mainMenu.enabled = false;
        optionsMenu.gameObject.SetActive(true);
        optionsMenu.LoadOptions();
        optionsMenu.GetComponent<Canvas>().enabled = true;
    }
}
