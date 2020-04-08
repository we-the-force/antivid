using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    [SerializeField]
    float rotationValue = 1;
    private void Update()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(0, rotationValue * Time.time, 0));
    }
}
