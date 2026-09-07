using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }

    private GameObject panel;
    private Button openButton;
    private Button joystickButton;
    private Button tapToMoveButton;
    private Button indonesianButton;
    private Button englishButton;
    private Button backButton;
    private Slider volumeSlider;
    private TMP_Text controlModeLabel;
    private TMP_Text volumeLabel;
    private TMP_Text titleLabel;
    private TMP_Text controllerLabel;
    private TMP_Text languageLabel;
    private TMP_Text backLabel;
    private TMP_Text openLabel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Canvas canvas = GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;

        panel = transform.Find("SettingContainer").gameObject;
        openButton = Find<Button>("InGameSettingsButton");
        joystickButton = Find<Button>("JoystickControlButton");
        tapToMoveButton = Find<Button>("TapToMoveControlButton");
        indonesianButton = Find<Button>("IndonesianLanguageButton");
        englishButton = Find<Button>("EnglishLanguageButton");
        backButton = Find<Button>("SettingsBackButton");
        volumeSlider = Find<Slider>("VolumeSlider");
        controlModeLabel = Find<TMP_Text>("ControlModeLabel");
        volumeLabel = Find<TMP_Text>("VolumeLabel");
        titleLabel = Find<TMP_Text>("Title");
        controllerLabel = Find<TMP_Text>("ControllerTitle");
        languageLabel = Find<TMP_Text>("LanguageTitle");
        backLabel = backButton.GetComponentInChildren<TMP_Text>(true);
        openLabel = openButton.GetComponentInChildren<TMP_Text>(true);

        openButton.onClick.AddListener(Open);
        backButton.onClick.AddListener(Close);
        joystickButton.onClick.AddListener(() => SetControlMode(ControlMode.Joystick));
        tapToMoveButton.onClick.AddListener(() => SetControlMode(ControlMode.TapToMove));
        indonesianButton.onClick.AddListener(() => SetLanguage(UiLanguage.Indonesian));
        englishButton.onClick.AddListener(() => SetLanguage(UiLanguage.English));
        volumeSlider.onValueChanged.AddListener(SetVolume);

        RectTransform buttonRect = (RectTransform)openButton.transform;
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = Vector2.one;
        buttonRect.anchoredPosition = new Vector2(-24f, -24f);

        panel.SetActive(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Open()
    {
        Refresh();
        panel.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(backButton.gameObject);
    }

    public void Close()
    {
        panel.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(openButton.gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Close();
        openButton.gameObject.SetActive(ShouldShowButton(scene.name));
        Refresh();
    }

    public static bool ShouldShowButton(string sceneName) => sceneName != "TitleScene";

    private void SetControlMode(ControlMode mode)
    {
        ControlSettings.Set(mode);
        Refresh();
    }

    private void SetVolume(float volume)
    {
        ControlSettings.SetVolume(volume);
        Refresh();
    }

    private void SetLanguage(UiLanguage language)
    {
        ControlSettings.SetLanguage(language);
        Refresh();
        FindObjectOfType<MainMenuManager>()?.RefreshMenuLabels();
    }

    private void Refresh()
    {
        bool english = ControlSettings.Language == UiLanguage.English;
        bool joystick = ControlSettings.Current == ControlMode.Joystick;

        volumeSlider.SetValueWithoutNotify(ControlSettings.Volume);
        controlModeLabel.text = joystick
            ? (english ? "ACTIVE: JOYSTICK" : "AKTIF: JOYSTICK")
            : (english ? "ACTIVE: TAP TO MOVE" : "AKTIF: TAP TO MOVE");
        volumeLabel.text = $"VOLUME: {Mathf.RoundToInt(ControlSettings.Volume * 100f)}%";
        titleLabel.text = openLabel.text = english ? "SETTINGS" : "PENGATURAN";
        controllerLabel.text = english ? "CONTROL MODE" : "PILIH KONTROL";
        languageLabel.text = english ? "LANGUAGE" : "BAHASA";
        backLabel.text = english ? "BACK" : "KEMBALI";

        joystickButton.interactable = !joystick;
        tapToMoveButton.interactable = joystick;
        indonesianButton.interactable = english;
        englishButton.interactable = !english;
    }

    private T Find<T>(string objectName) where T : Component
    {
        foreach (T component in GetComponentsInChildren<T>(true))
            if (component.name == objectName)
                return component;

        throw new MissingReferenceException($"{objectName} tidak ditemukan di prefab settings.");
    }
}
