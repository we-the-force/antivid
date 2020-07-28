using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TouchGesture { Undefined, Tap, Swipe, Canceled };

public class SwipeMovement
{
    private Vector2 _force;
    private float _cost;

    public Vector2 Force
    {
        get { return _force; }
        set { _force = value; }
    }
    public float Cost
    {
        get { return _cost; }
        set { _cost = value; }
    }

    public SwipeMovement(Vector2 force, float cost)
    {
        _force = force;
        _cost = cost;
    }
}

public class TouchManager : MonoBehaviour
{
    public bool isActive;

    [SerializeField, Range(0.01f, 1f)]
    float cameraSpeed;
    [SerializeField, Range(0.01f, 0.25f)]
    float zoomSpeed;

    [SerializeField]
    [Range(1, 5)]
    int touchQuantity = 4;
    [SerializeField]
    [Range(0, 1f)]
    float tapTimeLimit = 0.25f;
    [SerializeField]
    [Range(0.05f, 0.3f)]
    float swipeDistance = 0.05f;

    [SerializeField]
    [Range(0f, 1f)]
    float maxForceDistance = 0.5f;

    CameraController camCon;
    [SerializeField]
    bool[] isTouchActive = new bool[4] { false, false, false, false };

    [SerializeField]
    bool movingCamera;

    [SerializeField]
    float previousDistance = 0;

    public static TouchManager Instance = null;

    private void Awake()
    {
        movingCamera = false;

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        //DontDestroyOnLoad(gameObject);

        isTouchActive = new bool[touchQuantity];
        for (int i = 0; i < isTouchActive.Length; i++)
        {
            isTouchActive[i] = false;
        }
        camCon = GameObject.Find("CameraObject").GetComponent<CameraController>();
        camCon.CanFollow = false;
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    if (IsPointerOverUIElement(0))
                    //if (EventSystem.current.IsPointerOverGameObject())
                    {
                        Debug.Log($"Pointer was on top of something     ({Time.time.ToString("F4")}, {Input.GetTouch(0).deltaTime.ToString("F4")})");
                        movingCamera = false;
                    }
                    else
                    {
                        movingCamera = true;
                        Debug.Log($"Pointer wasn't on top of something     ({Time.time.ToString("F4")}, {Input.GetTouch(0).deltaTime.ToString("F4")})");
                    }
                }
                if (movingCamera)
                {
                    Vector2 asd = Input.GetTouch(0).deltaPosition;
                    Debug.Log($"({Input.GetTouch(0).position} - {Input.GetTouch(0).deltaPosition}) = {asd.ToString("F2")}");
                    camCon.transform.Translate((new Vector3(-asd.x, 0, -asd.y) /** Time.unscaledDeltaTime*/ * cameraSpeed * camCon.TouchSpeed));
                }
            }
            else if (Input.touchCount == 2)
            {
                Vector2 t1 = Input.GetTouch(0).position;
                Vector2 t2 = Input.GetTouch(1).position;

                float distance = Vector2.Distance(t1, t2);
                if (previousDistance == 0)
                {
                    previousDistance = distance;
                }
                float distanceThing = distance - previousDistance;
                float finalDistance = distanceThing * -1f * zoomSpeed;
                //Debug.Log($"D: {distance.ToString("F4")}, PD: {previousDistance.ToString("F4")} = {distanceThing.ToString("F4")}, ({finalDistance.ToString("F4")})");
                camCon.ChangeZoom(finalDistance);
                previousDistance = distance;
            }
            else
            {
                movingCamera = false;
                previousDistance = 0;
            }
        }
        else
        {
            movingCamera = false;
            previousDistance = 0;
        }
    }

    ///Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement(int fingerIndex)
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults(fingerIndex));
    }
    ///Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }
    ///Gets all event systen raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults(int fingerIndex)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.GetTouch(fingerIndex).position;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    private void FixedUpdate()
    {
        
        //if (Input.touchCount == 1)
        //{
        //    Vector2 asd = Input.GetTouch(0).deltaPosition;
        //    Debug.Log($"({Input.GetTouch(0).position} - {Input.GetTouch(0).deltaPosition}) = {asd.ToString("F2")}");
        //    camCon.transform.Translate((new Vector3(-asd.x, 0, -asd.y) * Time.unscaledDeltaTime * cameraSpeed));
        //    //Move.
        //}
        //else if (Input.touchCount == 2)
        //{
        //    //Zoom.
        //}

        //for (int i = 0; i < Input.touches.Length; i++)
        //{
        //    Touch touch = Input.GetTouch(i);

        //    if (touch.fingerId < touchQuantity)
        //    {

        //        //if (!isTouchActive[touch.fingerId])
        //        //{
        //        //    StartCoroutine(MonitorTouchRedux(touch.fingerId));
        //        //}
        //    }
        //}
    }

    /// Esta corrutina se usa para trackear un touch cada que se manda a llamar desde el FixedUpdate
    /// Guarda la info relevante (Por lo pronto, startPos y endPos) y al terminar el touch
    /// Decide si fue un tap o un flick.

    IEnumerator MonitorTouchRedux(int id)
    {
        if (TouchExists(id) && !isTouchActive[id])
        {
            Touch touch = GetTouchById(id);
            TouchGesture detectedGesture = TouchGesture.Undefined;
            isTouchActive[touch.fingerId] = true;
            float touchStarted = Time.time;
            float timeHeld = 0;
            float swipeHeld = 0;
            float forceMult = 0;

            Vector2 startPos = camCon.Cam.ScreenToViewportPoint(touch.position);
            while (true)
            {
                touch = GetTouchById(id);
                Vector2 currentPos = camCon.Cam.ScreenToViewportPoint(touch.position);
                float distance = Vector2.Distance(startPos, currentPos);
                forceMult = distance / maxForceDistance;

                timeHeld = Time.time - touchStarted;
                detectedGesture = (distance < swipeDistance) ? ((timeHeld < tapTimeLimit) ? TouchGesture.Tap : TouchGesture.Canceled) : TouchGesture.Swipe;

                if (detectedGesture == TouchGesture.Swipe)
                {
                    Vector2 startPosCentered = GetCenteredViewportVector(startPos);
                    Vector2 currentPosCentered = GetCenteredViewportVector(currentPos);
                    float angle = GetAngle(startPosCentered, currentPosCentered);

                    swipeHeld += Time.deltaTime ;
                }
                else
                {
                    swipeHeld = 0 - Time.deltaTime;
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    Time.timeScale = 1f;

                    if(detectedGesture == TouchGesture.Swipe)
                    {

                    }
                    else if (detectedGesture == TouchGesture.Tap)
                    {

                    }

                    break;
                }
                yield return null;
            }
            isTouchActive[touch.fingerId] = false;
        }
        yield return null;
    }


    bool TouchExists(int id)
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == id)
            {
                //Debug.Log(string.Format("The touch {0} do exists dude", id));
                return true;
            }
        }
        Debug.LogWarning(string.Format("The touch {0} doesnt exist", id));
        return false;
    }

    Touch GetTouchById(int id)
    {
        Touch aux = new Touch();
        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == id)
            {
                return touch;
                //aux = touch;
                //break;
            }
        }
        Debug.LogError(string.Format("Ayy tried to get a non-existent touch\r\nTouchManager.GeTouchById({0})", id));
        return aux;
    }

    float GetAngle(Vector2 start, Vector2 end)
    {
        Vector2 centered = end - start;
        return Mathf.Atan2(centered.y, centered.x) * Mathf.Rad2Deg;
    }

    Vector2 GetCenteredViewportVector(Vector2 orig)
    {
        Vector2 aux = new Vector2(orig.x - (camCon.Cam.rect.width / 2f), orig.y - (camCon.Cam.rect.height / 2f));
        return aux;
    }
}