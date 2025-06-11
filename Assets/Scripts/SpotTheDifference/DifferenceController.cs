using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifferenceController : MonoBehaviour
{
    [SerializeField]
    Image timeImage;
    private float time;
    private float timer;

    private bool isActiveTimer=true;
    private void OnEnable()
    {
        timer = 1;

        EventManager.Subscribe<Win>(Win);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<Win>(Win);
    }
    // Update is called once per frame
    void Update()
    {
        if (isActiveTimer||timer>0)
        {
            timeImage.fillAmount = time;
            timer -= Time.deltaTime / time;
        }
        if (isActiveTimer)
        {
            isActiveTimer = false;
            //TimesUp();
        }
        
    }

    private void TimesUp()
    {
    }

    private void Win(Win win)
    {
        Debug.Log($"{win.win}");
    }
}

public class Win
{
    public string win;

    public Win(string win)
    {
        this.win = win;
    }
}