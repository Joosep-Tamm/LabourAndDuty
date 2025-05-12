using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Vector3 carPositionOffset;
    [SerializeField] private Vector3 carRotation;
    [SerializeField] private Vector3 boxPositionOffset;
    [SerializeField] private Vector3 boxRotation;
    [SerializeField] private Vector3 vesselPositionOffset;
    [SerializeField] private Vector3 vesselRotation;

    // This method was created with the help of Claude Sonnet LLM
    public GameObject Spawn(GameObject prefab, float timeOnBelt)
    {
        Quaternion rotation;
        Vector3 position;
        switch (prefab.tag)
        {
            case "Box":
                rotation = Quaternion.Euler(boxRotation);
                position = boxPositionOffset;
                break;
            case "Car":
                rotation = Quaternion.Euler(carRotation);
                position = carPositionOffset;
                break;
            case "Vessel":
                rotation = Quaternion.Euler(vesselRotation);
                position = vesselPositionOffset;
                break;
            default:
                Debug.Log("Failed to spawn, prefab tag: " + prefab.tag);
                return null;
        }
        position += transform.position;
        GameObject spawnedObject = Instantiate(prefab, position, rotation);
        spawnedObject.GetComponent<MovementOnBelt>().MoveToInteractionArea(timeOnBelt);
        Debug.Log("Spawning object: " + prefab.name + ", time: " + Time.timeAsDouble);
        return spawnedObject;
    }
}
