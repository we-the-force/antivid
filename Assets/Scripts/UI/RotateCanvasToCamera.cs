using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCanvasToCamera : MonoBehaviour
{
    [SerializeField]
    Camera cam;
    private void Start()
    {
        cam = Camera.main;
    }
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
    }
}
