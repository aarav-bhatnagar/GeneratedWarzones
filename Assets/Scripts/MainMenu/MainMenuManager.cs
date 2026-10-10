using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject customizeButton;

    [SerializeField] private GameObject CustomizationMenu;

    [SerializeField] private Customizer customizer;
    [SerializeField] private GameObject Title;


    public void ToggleCustomizationMenu(bool isShowing)
    {

        CustomizationMenu.SetActive(isShowing);

        playButton.SetActive(!isShowing);
        customizeButton.SetActive(!isShowing);
        Title.SetActive(!isShowing);
        customizer.enabled = isShowing;
    }
}
