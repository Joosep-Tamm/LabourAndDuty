using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerPositionDebug : MonoBehaviour
{
    void Update()
    {
        Debug.Log($"{transform.name} position: {transform.position}, rotation: {transform.rotation}");
    }
}
