using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] float rotateSpeed;
    private bool isOpen = false;
    private Transform door;
    private Quaternion targetAngle;
    private Quaternion initialRotation;

    private void Awake()
    {
        door = transform.GetChild(0).transform;
        initialRotation = door.rotation;
        targetAngle = initialRotation;
    }

    private void Update()
    {
        door.rotation = Quaternion.Lerp(door.rotation, targetAngle, Time.deltaTime * rotateSpeed);
    }

    public void OpenDoor()
    {
        isOpen = true;
        //door.Rotate(Vector3.up, 90);

        targetAngle = Quaternion.AngleAxis(90f, Vector3.up) * initialRotation;
    }

    public void CloseDoor()
    {
        isOpen = false;
        //door.Rotate(Vector3.up, -90);

        targetAngle = Quaternion.AngleAxis(0f, Vector3.up) * initialRotation;
    }
}
