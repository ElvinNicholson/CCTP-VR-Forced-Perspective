using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    public UnityEvent onPressedAction;
    public UnityEvent onExitAction;

    private int objectsColliding = 0;
    private static float normalPosY = 0.07f;
    private static float pressedPosY = 0.04f;
    private Transform button;

    private void Awake()
    {
        button = transform.GetChild(0).transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        objectsColliding++;
        if (objectsColliding == 1)
        {
            OnFirstObjectEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        objectsColliding--;
        if (objectsColliding == 0)
        {
            OnLastObjectExit();
        }
    }

    private void OnFirstObjectEnter()
    {
        onPressedAction?.Invoke();
        button.localPosition = new Vector3(0, pressedPosY, 0);
    }

    private void OnLastObjectExit()
    {
        StopAllCoroutines();

        onExitAction?.Invoke();
        button.localPosition = new Vector3(0, normalPosY, 0);
    }
}
