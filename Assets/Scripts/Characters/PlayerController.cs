using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Analog joystick;
    public float moveSpeed = 5f;
    public bool is3D = false; // Set true if 3D
    [SerializeField]
    private Animator animator;
    [SerializeField]
    Transform graph;
    [SerializeField]
    private Rigidbody2D rb;
    private static int Movex = Animator.StringToHash("MoveX");
    private static int Movey = Animator.StringToHash("MoveY");
    private static int Speed = Animator.StringToHash("Speed");

    void Update()
    {
        Vector2 input = joystick.Direction;
        Vector3 move = is3D ? new Vector3(input.x, 0, input.y) : new Vector3(input.x, input.y, 0);
        //rb.velocity = move * moveSpeed * Time.deltaTime;
        transform.Translate(move * moveSpeed * Time.deltaTime);
        animator.SetFloat(Movex, Mathf.Abs(move.x));
        animator.SetFloat(Movey, move.y);
        animator.SetFloat(Speed, Vector3.Magnitude(move));
        if (move.x < 0)
        {
            graph.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            graph.rotation = Quaternion.Euler(0, 180, 0);
        }
}

}
