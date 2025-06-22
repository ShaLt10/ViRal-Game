using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MovingNPC : DialogueInteraction
{
    [SerializeField]
    private Transform moveTo;
    // Start is called before the first frame update

    [SerializeField]
    private bool IsArriveAtTarget;

    [SerializeField]
    Animator animator;

    private static int Speed = Animator.StringToHash("Speed");


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var dis = Vector3.Distance(moveTo.position, transform.position) >= 0.5f;
        if (dis)
        { 
            var dir = moveTo.position - transform.position;
            dir.Normalize();
            animator.SetFloat(Speed, 1);
            transform.position =  Vector3.MoveTowards(transform.position, moveTo.position, 2*Time.deltaTime);
        }
        if (!IsArriveAtTarget && !dis)
        {
            var scene = SceneManager.GetActiveScene();
            animator.SetFloat(Speed, 0);
            IsArriveAtTarget = true;
            if (DialogueContainer.Instance.CharacterSelected.GetName() == StringContainer.Gavi)
            {
                EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.Map3_Gavi}", () => SceneManager.LoadScene(scene.buildIndex + 1)));
            }
            else if (DialogueContainer.Instance.CharacterSelected.GetName() == StringContainer.Raline)
            {
                EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.Map3_Gavi}", () => SceneManager.LoadScene(scene.buildIndex + 1)));
            }
        }
    }
}
