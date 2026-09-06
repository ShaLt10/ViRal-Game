using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TitleMenuButtonAnimator : MonoBehaviour
{
    [SerializeField, Min(1f)] private float breathingScale = 1.015f;
    [SerializeField, Min(0.05f)] private float breathingDuration = 1.2f;
    [SerializeField, Range(0f, 1f)] private float appearScale = 0.92f;
    [SerializeField, Min(0.05f)] private float appearDuration = 0.3f;
    [SerializeField, Min(0f)] private float appearInterval = 0.08f;
    [SerializeField] private Color clickHighlight = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField, Min(0.01f)] private float clickDuration = 0.08f;

    private void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        Debug.Assert(buttons.Length > 0, "Title menu button animator found no buttons.", this);
        int visibleIndex = 0;

        foreach (Button button in buttons)
        {
            Transform target = button.transform;
            Vector3 baseScale = target.localScale;

            if (button.gameObject.activeInHierarchy)
            {
                CanvasGroup group = button.GetComponent<CanvasGroup>();
                if (!group) group = button.gameObject.AddComponent<CanvasGroup>();
                float delay = visibleIndex++ * appearInterval;
                group.alpha = 0f;
                group.interactable = false;
                target.localScale = baseScale * appearScale;

                group.DOFade(1f, appearDuration)
                    .SetDelay(delay)
                    .SetUpdate(true)
                    .SetLink(button.gameObject, LinkBehaviour.PauseOnDisableRestartOnEnable)
                    .OnComplete(() => group.interactable = true);
                target.DOScale(baseScale, appearDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true)
                    .SetLink(button.gameObject, LinkBehaviour.PauseOnDisableRestartOnEnable)
                    .OnComplete(() => Breathe(button, baseScale));
            }
            else
            {
                Breathe(button, baseScale);
            }

            if (button.targetGraphic == null) continue;

            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (!trigger) trigger = button.gameObject.AddComponent<EventTrigger>();
            trigger.triggers ??= new List<EventTrigger.Entry>();

            var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            click.callback.AddListener(_ => Highlight(button));
            trigger.triggers.Add(click);
        }
    }

    private void Breathe(Button button, Vector3 baseScale)
    {
        button.transform.DOScale(baseScale * breathingScale, breathingDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(button.gameObject, LinkBehaviour.PauseOnDisableRestartOnEnable);
    }

    private void Highlight(Button button)
    {
        Graphic graphic = button.targetGraphic;
        if (graphic == null) return;

        Color highlight = clickHighlight;
        highlight.a = graphic.color.a;

        graphic.DOKill(true);
        graphic.DOColor(highlight, clickDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .SetLink(button.gameObject);
    }
}
