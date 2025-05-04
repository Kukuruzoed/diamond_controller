using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MammothProtect : MonoBehaviour
{
    [Header("UI")]
    public GameObject passwordPanel;
    public TMP_InputField passwordInput;
    public Button submitButton;

    [Header("Настройки")]
    public string mammoth = "mammoth438";
    public float timeUntilLock = 60f;
    public float timeToEnterPassword = 30f;

    private float appTimer = 0f;
    private float passwordTimer = 0f;
    private bool isUnlocked = true;
    private bool isPasswordPanelShown = false;

    private const string PlayerPrefsKey = "mammoth";

    void Start()
    {
        isUnlocked = PlayerPrefs.GetInt(PlayerPrefsKey, 0) == 1;
        Debug.Log($"Unlocked = {isUnlocked}");
        if (isUnlocked)
        {
            passwordPanel.SetActive(false);
            enabled = false;
        }
        else
        {
            passwordPanel.SetActive(false);
            submitButton.onClick.AddListener(CheckPassword);
        }
        FindFirstObjectByType<CommandServer>().OnCommandSended.RemoveAllListeners();
    }

    void Update()
    {
        if (isUnlocked) return;

        appTimer += Time.deltaTime;

        if (!isPasswordPanelShown && appTimer >= timeUntilLock)
        {
            ShowPasswordPanel();
        }

        if (isPasswordPanelShown)
        {
            passwordTimer += Time.deltaTime;

            if (passwordTimer >= timeToEnterPassword)
            {
                FindFirstObjectByType<CommandServer>().SendCommandToAll(Commands.PAUSE);
                Debug.LogWarning("Время на ввод пароля истекло. Закрытие приложения.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }

    void ShowPasswordPanel()
    {
        passwordPanel.SetActive(true);
        isPasswordPanelShown = true;
        passwordTimer = 0f;
    }

    void CheckPassword()
    {
        if (passwordInput.text == mammoth)
        {
            Debug.Log("Пароль верный. Блокировка снята.");
            PlayerPrefs.SetInt(PlayerPrefsKey, 1);
            PlayerPrefs.Save();

            passwordPanel.SetActive(false);
            isUnlocked = true;
            enabled = false;
        }
        else
        {
            Debug.LogWarning("Неверный пароль!");
        }
    }
}
