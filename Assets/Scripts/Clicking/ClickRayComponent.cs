using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickRayComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            HandleClick(true);
        }
        if (Input.GetMouseButtonUp(1))
        {
            HandleClick(false);
        }
    }
    void HandleClick(bool leftClick)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            //Debug.Log($"Hit something! ({hit.transform.name})");
            hit.transform.SendMessage(leftClick ? "LeftClick" : "RightClick", SendMessageOptions.DontRequireReceiver);
        }
    }
}
