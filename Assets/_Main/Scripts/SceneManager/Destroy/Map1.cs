using UnityEngine;

public class Map1 : SingletonDestroy<Map1>
{
    private void Start()
    {
        // Pastikan orientasi
        ScreenRotateControl.Instance.SetLandscape();

        // Pastikan DialogManager ada (DDOL via AutoBootstrap atau sudah ada di scene)
        if (DialogManager.Instance == null)
        {
            Debug.LogError("[Map1] DialogManager.Instance null. Pastikan AutoBootstrap/Prefab Managers sudah jalan.");
            return;
        }

        // Ambil nama karakter player dari CharacterManager (Gavi/Raline)
        bool isGavi = CharacterManager.Instance != null
            && CharacterManager.Instance.selectedCharacter == CharacterManager.SelectedMC.Gavi;
        string key = isGavi
            ? DialoguesNames.Area1_PlayerOpening_Gavi
            : DialoguesNames.Area1_PlayerOpening_Raline;

        Debug.Log($"[Map1] Triggering dialog: {key}");

        // Mainkan dialog pembuka; setelah selesai kamu bisa lanjut apa pun (opsional)
        DialogManager.Instance.PlaySequenceThen(key, onDone: () =>
        {
            Debug.Log("[Map1] Opening dialog finished.");
            // TODO: taruh logic lanjutan map di sini (enable input, spawn NPC marker, dsb) 
        });
    }
}
