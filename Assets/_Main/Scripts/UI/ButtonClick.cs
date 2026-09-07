using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class ButtonClick : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }

    private void Play()
    {
        AudioManager.Instance?.PlaySfx(clickSound);
    }
}
