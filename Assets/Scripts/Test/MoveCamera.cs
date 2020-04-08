﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum CameraRotation { Up = 0, Right = 270, Left = 90, Down = 180 };
public enum CameraRotation { Up = -45, Right = 225, Left = 45, Down = 135 };
public class MoveCamera : MonoBehaviour
{
    [SerializeField, Tooltip("Units per second"), Range(1f, 10f)]
    float movementSpeed = 5f;
    [SerializeField, Tooltip("Units per second"), Range(1f, 100f)]
    float rotationSpeed = 5f;
    [SerializeField, Tooltip("Units per second"), Range(1f, 100f)]
    float zoomSpeed = 5f;

    [SerializeField]
    CameraRotation currentRot;
    [SerializeField]
    float currentZoom = 8;

    public bool freeMovement = true;
    public GameObject objectToFollow;
    [SerializeField]
    bool isFollowing = false;
    Camera cam;

    private void Awake()
    {
        cam = transform.GetComponentInChildren<Camera>();
    }
    private void Update()
    {
        if (freeMovement || objectToFollow == null)
        {
            Vector3 playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), /*Input.GetButton("Fire1") ? -1 : Input.GetButton("Fire2") ? 1 :*/ 0, Input.GetAxisRaw("Vertical"));
            float auxPercentage = 1f / 15f * (currentZoom + 5f);
            float auxSpeed = movementSpeed * auxPercentage;
            Debug.Log($"Zoom: {currentZoom}, percentage: {auxPercentage}");
            transform.position += Quaternion.Euler(0, (int)currentRot, 0) * (playerInput * (auxSpeed * Time.deltaTime));
        }
        else
        {
            if (objectToFollow != null)
            {
                Vector2 pos = Vector2.Lerp(new Vector2(transform.position.x, transform.position.z), new Vector2(objectToFollow.transform.position.x, objectToFollow.transform.position.z), movementSpeed * Time.deltaTime);
                transform.position = new Vector3(pos.x, 3.5f, pos.y);
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateCamera(-1);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateCamera(1);
        }
        if (Input.GetKey(KeyCode.R))
        {
            ZoomCamera(-1);
        }
        else if (Input.GetKey(KeyCode.F))
        {
            ZoomCamera(1);
        }
        if (Input.GetMouseButtonDown(1))
        {

        }

        transform.rotation = Quaternion.Euler(0, Mathf.LerpAngle(transform.rotation.eulerAngles.y, (int)currentRot, rotationSpeed * Time.deltaTime), 0);
        cam.orthographicSize = currentZoom;
    }
    void RotateCamera(int value)
    {
        int auxRot = ((int)currentRot + 45) / 90 + value;
        auxRot = auxRot < 0 ? 3 : auxRot > 3 ? 0 : auxRot;
        currentRot = (CameraRotation)(auxRot * 90 - 45);
    }
    void ZoomCamera(int zoom)
    {
        float zoomFactor = zoomSpeed * Time.deltaTime * zoom;
        if (currentZoom + zoomFactor < 1.5f)
        {
            currentZoom = 1.5f;
        }
        else if (currentZoom + zoomFactor > 10f)
        {
            currentZoom = 10f;
        }
        else
        {
            currentZoom += zoomFactor;
        }
    }
}