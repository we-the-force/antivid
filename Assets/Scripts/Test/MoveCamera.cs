using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [Tooltip("Units per second"), Range(1f, 10f)]
    public float speed;

    public bool freeMovement = true;
    public GameObject objectToFollow;

    private void Update()
    {
        if (freeMovement || objectToFollow == null)
        {
            Vector3 playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), /*Input.GetButton("Fire1") ? -1 : Input.GetButton("Fire2") ? 1 :*/ 0, Input.GetAxisRaw("Vertical"));
            transform.position += playerInput * (speed * Time.deltaTime);
        }
        else
        {
            if (objectToFollow != null)
            {
                Vector2 pos = Vector2.Lerp(new Vector2(transform.position.x, transform.position.z), new Vector2(objectToFollow.transform.position.x, objectToFollow.transform.position.z), speed * Time.deltaTime);
                transform.position = new Vector3(pos.x, 2.5f, pos.y);
            }
        }
    }
}