using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [Tooltip("Units per second"), Range(1f, 10f)]
    public float speed;

    private void Update()
    {
        Vector3 playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetButton("Fire1") ? -1 : Input.GetButton("Fire2") ? 1 : 0 , Input.GetAxisRaw("Vertical"));
        transform.position += playerInput * (speed * Time.deltaTime);
    }
}
