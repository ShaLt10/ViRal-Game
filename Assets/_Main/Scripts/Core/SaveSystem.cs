using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string FileName = "viral-save.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);
    private static string TempPath => SavePath + ".tmp";
    private static string BackupPath => SavePath + ".bak";

    public static bool HasSave => File.Exists(SavePath) || File.Exists(BackupPath);

    public static bool Save(GameSaveData data)
    {
        if (data == null || !data.IsValid())
        {
            Debug.LogError("[SaveSystem] Data save tidak valid.");
            return false;
        }

        try
        {
            File.WriteAllText(TempPath, JsonUtility.ToJson(data, true));

            if (File.Exists(SavePath))
                ReplaceSave();
            else
                File.Move(TempPath, SavePath);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveSystem] Gagal menyimpan game: {exception.Message}");
            return false;
        }
    }

    public static bool TryLoad(out GameSaveData data)
    {
        if (TryRead(SavePath, out data))
            return true;

        if (TryRead(BackupPath, out data))
        {
            Debug.LogWarning("[SaveSystem] Save utama rusak/tidak ada. Backup berhasil dipakai.");
            return true;
        }

        data = null;
        return false;
    }

    public static bool TryGetSaveTime(out DateTime savedAt)
    {
        string path = TryRead(SavePath, out _) ? SavePath
            : TryRead(BackupPath, out _) ? BackupPath
            : null;

        try
        {
            savedAt = path == null ? default : File.GetLastWriteTime(path);
            return path != null;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SaveSystem] Gagal membaca tanggal save: {exception.Message}");
            savedAt = default;
            return false;
        }
    }

    public static bool Delete()
    {
        bool success = DeleteIfExists(SavePath);
        success &= DeleteIfExists(TempPath);
        success &= DeleteIfExists(BackupPath);
        return success;
    }

    private static bool TryRead(string path, out GameSaveData data)
    {
        data = null;
        if (!File.Exists(path))
            return false;

        try
        {
            data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path));
            if (data == null)
                return false;

            int previousVersion = data.version;
            if (!data.TryUpgrade())
                return false;

            if (previousVersion != data.version && !Save(data))
            {
                data = null;
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SaveSystem] Gagal membaca '{Path.GetFileName(path)}': {exception.Message}");
            data = null;
            return false;
        }
    }

    private static void ReplaceSave()
    {
        try
        {
            File.Replace(TempPath, SavePath, BackupPath);
        }
        catch (PlatformNotSupportedException)
        {
            // ponytail: fallback copy cukup untuk platform tanpa File.Replace; revisi jika crash-safe mobile save menjadi kebutuhan terukur.
            File.Copy(SavePath, BackupPath, true);
            File.Copy(TempPath, SavePath, true);
            File.Delete(TempPath);
        }
    }

    private static bool DeleteIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SaveSystem] Gagal menghapus '{Path.GetFileName(path)}': {exception.Message}");
            return false;
        }
    }
}
