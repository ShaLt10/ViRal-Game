using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Digunakan untuk menyimpan dialog.
/// </summary>
[CreateAssetMenu(fileName = "DialogueSequence", menuName = "Game/Dialogue/DialogueSequence")]
public class DialogueSequence : ScriptableObject
{
    [SerializeField] public string sequenceName;
    [SerializeField] private string sequenceDescription;
    [SerializeField] private Dialogue[] dialogue;

    public Dialogue[] GetDialogue()
    {
        return dialogue;
    }
}


/// <summary>
/// Dialogue yang akan dimainkan.
/// </summary>
[Serializable]
public class Dialogue
{
    /// <summary>
    /// Nama pembicara.
    /// </summary>
    public string speaker;
    /// <summary>
    /// Isi Dialogue.
    /// </summary>
    [TextArea]
    public string dialogue;
    /// <summary>
    /// Gambar dari potrait yang akan ditampilkan
    /// </summary>
    public ImagePotrait imagePotrait;

    public Action Doafter;

}

/// <summary>
/// Potrait dari gambar yang ditampilkan ketika berbicara
/// </summary>
[Serializable]
public class ImagePotrait
{
    /// <summary>
    /// Gambar dari image potrait.
    /// </summary>
    public Sprite potraitImage;
    /// <summary>
    /// Lokasi dari gambar yang ditampilkan.
    /// </summary>
    public ImageLocation imageLocation;
    /// <summary>
    /// Menghilangkan gambar potrait dari lokasi.
    /// </summary>
    public bool clearImage;
}

/// <summary>
/// Menyimpan enum dari lokasi gambar potrait.
/// </summary>
[Serializable]
public enum ImageLocation
{ 
    Left,
    Right,
    Middle,
}
