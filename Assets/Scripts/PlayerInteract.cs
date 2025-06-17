using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField]
    private LayerMask layer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            var ray = Physics2D.Raycast(transform.position, transform.up, 2, layer);

            if (ray.collider != null)
            {

                if (ray.collider.TryGetComponent(out IInteract interact))
                {
                    interact.Interaction();
                }
            }
        }
    }
}
