using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel; // Contains a button calling StartGame
    public GameObject weaponSelectionPanel; // Shows weapon choices
    public GameObject sceneSelectionPanel; // Shows scene choices

    [Header("Weapon Selection UI")]
    public Transform weaponButtonParent; // Parent for dynamic weapon buttons
    public GameObject weaponButtonPrefab; // Prefab with Button and Text
    public List<WeaponData> availableWeapons; // List of WeaponData SOs

    [Header("Scene Selection UI")]
    public Button[] sceneButtons; // Assign buttons for each scene
    public string[] sceneNames; // Corresponding scene names in Build Settings

    void Start()
    {
        // Initialize UI state
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        else Debug.LogError("MainMenu: MainMenuPanel not assigned!");

        if (weaponSelectionPanel != null) weaponSelectionPanel.SetActive(false);
        else Debug.LogError("MainMenu: WeaponSelectionPanel not assigned!");

        if (sceneSelectionPanel != null) sceneSelectionPanel.SetActive(false);
        else Debug.LogError("MainMenu: SceneSelectionPanel not assigned!");

        // Populate weapon selection buttons
        SetupWeaponButtons();

        // Setup scene selection buttons
        SetupSceneButtons();

        // Ensure time scale is normal
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (weaponSelectionPanel != null) weaponSelectionPanel.SetActive(true);
        Debug.Log("MainMenu: Start Game initiated, showing weapon selection");
    }

    void SetupWeaponButtons()
    {
        if (weaponButtonParent == null || weaponButtonPrefab == null)
        {
            Debug.LogError("MainMenu: WeaponButtonParent or WeaponButtonPrefab not assigned!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in weaponButtonParent)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each weapon
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            if (availableWeapons[i] == null) continue;

            GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponButtonParent);
            WeaponData weapon = availableWeapons[i];

            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"MainMenu: Weapon button prefab {buttonObj.name} missing Button component!");
                continue;
            }

            Text text = buttonObj.GetComponentInChildren<Text>();
            TextMeshProUGUI tmpText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
            {
                text.text = weapon.name;
            }
            else if (tmpText != null)
            {
                tmpText.text = weapon.name;
            }
            else
            {
                Debug.LogWarning($"MainMenu: Weapon button {buttonObj.name} missing Text or TextMeshProUGUI!");
            }

            button.onClick.AddListener(() => OnWeaponSelected(weapon));
        }

        Debug.Log($"MainMenu: Created {availableWeapons.Count} weapon buttons");
    }

    void OnWeaponSelected(WeaponData weapon)
    {
        GameData.SetSelectedWeapon(weapon);
        if (weaponSelectionPanel != null) weaponSelectionPanel.SetActive(false);
        if (sceneSelectionPanel != null) sceneSelectionPanel.SetActive(true);
        Debug.Log($"MainMenu: Selected weapon {weapon.name}, showing scene selection");
    }

    void SetupSceneButtons()
    {
        if (sceneButtons == null || sceneNames == null || sceneButtons.Length != sceneNames.Length)
        {
            Debug.LogError("MainMenu: SceneButtons or SceneNames not properly assigned or mismatched!");
            return;
        }

        for (int i = 0; i < sceneButtons.Length; i++)
        {
            if (sceneButtons[i] == null || string.IsNullOrEmpty(sceneNames[i])) continue;

            int index = i;
            sceneButtons[i].onClick.RemoveAllListeners();
            sceneButtons[i].onClick.AddListener(() => OnSceneSelected(sceneNames[index]));

            Text text = sceneButtons[i].GetComponentInChildren<Text>();
            TextMeshProUGUI tmpText = sceneButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = sceneNames[i];
            }
            else if (tmpText != null)
            {
                tmpText.text = sceneNames[i];
            }
        }

        Debug.Log($"MainMenu: Set up {sceneButtons.Length} scene buttons");
    }

    void OnSceneSelected(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("MainMenu: Attempted to load empty scene name!");
            return;
        }

        Debug.Log($"MainMenu: Loading scene {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}