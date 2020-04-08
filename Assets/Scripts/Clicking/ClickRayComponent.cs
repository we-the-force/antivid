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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Debug.Log($"Hit something! ({hit.transform.name})");
                hit.transform.SendMessage("Click");
            }
        }
    }
}
