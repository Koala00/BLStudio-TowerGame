using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Controls the main features of the options menu.
/// </summary>
public class OptionsMenu : MonoBehaviour
{
    public Canvas mainMenu;
    public Button Save;
    public Button Cancel;

    public void LoadOptions()
    {
        var optionGroups = GetComponentsInChildren<IOptionsGroup>();
        foreach (var group in optionGroups)
            group.Load();
    }

    private void GoToMainMenu()
    {
        var optionsMenu = gameObject.GetComponent<Canvas>();
        optionsMenu.enabled = false;
        mainMenu.enabled = true;
    }

    public void OnSave()
    {
        // TODO: Save changes somewhere - config file?
        var optionGroups = GetComponentsInChildren<IOptionsGroup>();
        foreach (var group in optionGroups)
            group.Save();
        GlobalSettings.Instance.SaveInPlayerPrefs();
        GoToMainMenu();
    }

    public void OnCancel()
    {
        GoToMainMenu();
    }

}
