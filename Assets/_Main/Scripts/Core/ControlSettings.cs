using UnityEngine;

public enum ControlMode
{
    Joystick,
    TapToMove
}

public enum UiLanguage
{
    Indonesian,
    English
}

public static class ControlSettings
{
    private const string ControlKey = "ControlMode";
    private const string VolumeKey = "MasterVolume";
    private const string LanguageKey = "Language";

    public static ControlMode Current => PlayerPrefs.GetInt(ControlKey, 0) == 1
        ? ControlMode.TapToMove
        : ControlMode.Joystick;

    public static float Volume => PlayerPrefs.GetFloat(VolumeKey, 1f);

    public static UiLanguage Language => PlayerPrefs.GetInt(LanguageKey, 1) == 1
        ? UiLanguage.English
        : UiLanguage.Indonesian;

    public static void Set(ControlMode mode)
    {
        PlayerPrefs.SetInt(ControlKey, mode == ControlMode.TapToMove ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
        PlayerPrefs.Save();
    }

    public static void SetLanguage(UiLanguage language)
    {
        PlayerPrefs.SetInt(LanguageKey, language == UiLanguage.English ? 1 : 0);
        PlayerPrefs.Save();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyVolume() => AudioListener.volume = Volume;
}
