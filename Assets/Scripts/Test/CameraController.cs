using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum CameraRotation { Up = 0, Right = 270, Left = 90, Down = 180 };
public enum CameraRotation { Up = -45, Right = 225, Left = 45, Down = 135 };
public class CameraController : MonoBehaviour
{
    static CameraController _instance = null;

    public bool CanFollow;

    [SerializeField, Tooltip("Units per second"), Range(1f, 10f)]
    float movementSpeed = 5f;
    [SerializeField, Tooltip("The speed used when using touch controls")]
    float _touchSpeed;
    [SerializeField, Tooltip("Units per second"), Range(1f, 100f)]
    float rotationSpeed = 5f;
    [SerializeField, Tooltip("Units per second"), Range(1f, 100f)]
    float zoomSpeed = 5f;
    [SerializeField, Range(0.5f, 1f)]
    float minZoom;
    [SerializeField, Range (3f, 10f)]
    float maxZoom;

    [SerializeField]
    CameraRotation currentRot;
    [SerializeField]
    float currentZoom = 8;

    Vector3 playerInput;
    public bool freeMovement = true;
    public GameObject objectToFollow;
    [SerializeField]
    bool isFollowing = false;
    Camera _cam;



    public float TouchSpeed
    {
        get { return _touchSpeed; }
    }

    public Camera Cam
    {
        get { return _cam; }
    }

    public static CameraController Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        _instance = this;

        _cam = transform.GetComponentInChildren<Camera>();

        SetTouchSpeed();
    }
    private void Update()
    {
        playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), /*Input.GetButton("Fire1") ? -1 : Input.GetButton("Fire2") ? 1 :*/ 0, Input.GetAxisRaw("Vertical"));
        if (playerInput != Vector3.zero || objectToFollow == null)
        {
            freeMovement = true;
            isFollowing = false;
            objectToFollow = null;
        }
        if (freeMovement)
        {
            float auxPercentage = 1f / 15f * (currentZoom + 5f);
            float auxSpeed = movementSpeed * auxPercentage;
            //Debug.Log($"Zoom: {currentZoom}, percentage: {auxPercentage}");
            transform.position += Quaternion.Euler(0, (int)currentRot, 0) * (playerInput * (auxSpeed * Time.deltaTime));
        }
        else
        {
            if (CanFollow)
            {
                if (objectToFollow != null)
                {
                    Vector2 pos = Vector2.Lerp(new Vector2(transform.position.x, transform.position.z), new Vector2(objectToFollow.transform.position.x, objectToFollow.transform.position.z), movementSpeed * Time.deltaTime);
                    transform.position = new Vector3(pos.x, 0, pos.y);
                }
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

        transform.rotation = Quaternion.Euler(0, Mathf.LerpAngle(transform.rotation.eulerAngles.y, (int)currentRot, rotationSpeed * Time.deltaTime), 0);
        _cam.orthographicSize = currentZoom;
    }
    public void ResetPosition()
    {
        transform.position = Vector3.zero;
    }
    public void RotateCamera(int value)
    {
        int auxRot = ((int)currentRot + 45) / 90 + value;
        auxRot = auxRot < 0 ? 3 : auxRot > 3 ? 0 : auxRot;
        currentRot = (CameraRotation)(auxRot * 90 - 45);
    }
    public void ChangeZoom(float quantity)
    {
        if (currentZoom + quantity < minZoom)
        {
            currentZoom = minZoom;
        }
        else if (currentZoom + quantity > maxZoom)
        {
            currentZoom = maxZoom;
        }
        else
        {
            currentZoom += quantity;
        }
        SetTouchSpeed();
    }
    void ZoomCamera(int zoom)
    {
        float zoomFactor = zoomSpeed * Time.deltaTime * zoom;
        if (currentZoom + zoomFactor < minZoom)
        {
            currentZoom = minZoom;
        }
        else if (currentZoom + zoomFactor > maxZoom)
        {
            currentZoom = maxZoom;
        }
        else
        {
            currentZoom += zoomFactor;
        }
    }
    void SetTouchSpeed()
    {
        _touchSpeed = 1f / maxZoom * (currentZoom);
    }
    public void SetObjectToFollow(GameObject toFollow)
    {
        isFollowing = true;
        freeMovement = false;
        objectToFollow = toFollow;
    }
}