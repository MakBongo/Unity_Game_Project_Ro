using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel; // Contains buttons for StartGame and OpenShop
    public GameObject weaponSelectionPanel; // Shows weapon choices
    public GameObject sceneSelectionPanel; // Shows scene choices
    public GameObject shopPanel; // Shop panel with ShopSystem

    [Header("Weapon Selection UI")]
    public Transform weaponButtonParent; // Parent for dynamic weapon buttons
    public GameObject weaponButtonPrefab; // Prefab with Button and Text
    public List<WeaponData> availableWeapons; // List of WeaponData SOs

    [Header("Scene Selection UI")]
    public Transform sceneButtonParent; // Parent for dynamic scene buttons
    public GameObject sceneButtonPrefab; // Prefab with Button and Text
    public List<string> availableScenes; // List of scene names in Build Settings

    [Header("Audio")]
    public AudioClip buttonClickSound; // Sound to play when a button is clicked
    private AudioSource audioSource; // AudioSource for playing button sounds

    void Start()
    {
        // Initialize AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        if (buttonClickSound != null)
        {
            audioSource.clip = buttonClickSound;
        }

        // Initialize UI state
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        else Debug.LogError("MainMenu: MainMenuPanel not assigned!");

        if (weaponSelectionPanel != null) weaponSelectionPanel.SetActive(false);
        else Debug.LogError("MainMenu: WeaponSelectionPanel not assigned!");

        if (sceneSelectionPanel != null) sceneSelectionPanel.SetActive(false);
        else Debug.LogError("MainMenu: SceneSelectionPanel not assigned!");

        if (shopPanel != null) shopPanel.SetActive(false);
        else Debug.LogError("MainMenu: ShopPanel not assigned!");

        // Populate weapon selection buttons
        SetupWeaponButtons();

        // Populate scene selection buttons
        SetupSceneButtons();

        // Ensure time scale is normal
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (weaponSelectionPanel != null) weaponSelectionPanel.SetActive(true);
        Debug.Log("MainMenu: Start Game initiated, showing weapon selection");
    }

    public void OpenShop()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(true);
        Debug.Log("MainMenu: Opened shop panel");
    }

    public void CloseShop()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (shopPanel != null) shopPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        Debug.Log("MainMenu: Closed shop panel, returned to main menu");
    }

    // New method to return from weapon selection to main menu
    public void BackToMainMenu()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (weaponSelectionPanel != null) weaponSelectionPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        Debug.Log("MainMenu: Returned to main menu from weapon selection");
    }

    // New method to return from scene selection to weapon selection
    public void BackToWeaponSelection()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        if (sceneSelectionPanel != null) sceneSelectionPanel.SetActive(false);
        if (weaponSelectionPanel != null) weaponSelectionPanel.SetActive(true);
        Debug.Log("MainMenu: Returned to weapon selection from scene selection");
    }

    public void QuitGame()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("MainMenu: Game Quit");
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
            if (text != null)
            {
                text.text = weapon.name;
            }
            else
            {
                Debug.LogWarning($"MainMenu: Weapon button {buttonObj.name} missing Text!");
            }

            button.onClick.AddListener(() =>
            {
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.Play();
                }
                OnWeaponSelected(weapon);
            });
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
        if (sceneButtonParent == null || sceneButtonPrefab == null)
        {
            Debug.LogError("MainMenu: SceneButtonParent or SceneButtonPrefab not assigned!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in sceneButtonParent)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each scene
        for (int i = 0; i < availableScenes.Count; i++)
        {
            if (string.IsNullOrEmpty(availableScenes[i])) continue;

            GameObject buttonObj = Instantiate(sceneButtonPrefab, sceneButtonParent);
            string sceneName = availableScenes[i];

            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"MainMenu: Scene button prefab {buttonObj.name} missing Button component!");
                continue;
            }

            Text text = buttonObj.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = sceneName;
            }
            else
            {
                Debug.LogWarning($"MainMenu: Scene button {buttonObj.name} missing Text!");
            }

            button.onClick.AddListener(() =>
            {
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.Play();
                }
                OnSceneSelected(sceneName);
            });
        }

        Debug.Log($"MainMenu: Created {availableScenes.Count} scene buttons");
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