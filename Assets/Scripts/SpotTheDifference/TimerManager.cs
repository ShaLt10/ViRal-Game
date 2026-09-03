using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public UnityEvent onTimerExpired;

    private float remaining;
    private bool running;

    void Update()
    {
        if (!running) return;
        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            running = false;
            UpdateUI();
            onTimerExpired?.Invoke();
            SpotDifferenceManager.Instance.OnTimerExpired();
            return;
        }
        UpdateUI();
    }

    public void StartTimer(float duration)
    {
        remaining = duration;
        running = true;
    }

    public void StopTimer() => running = false;

    public void ApplyPenalty(float seconds)
    {
        remaining = Mathf.Max(0f, remaining - seconds);
        // Flash merah singkat
        StartCoroutine(FlashRed());
    }

    void UpdateUI()
    {
        int m = (int)(remaining / 60);
        int s = (int)(remaining % 60);
        timerText.text = $"{m:00}:{s:00}";
        timerText.color = remaining <= 10f ? Color.red : Color.white;
    }

    System.Collections.IEnumerator FlashRed()
    {
        timerText.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        // warna normal dikembalikan di UpdateUI
    }
}