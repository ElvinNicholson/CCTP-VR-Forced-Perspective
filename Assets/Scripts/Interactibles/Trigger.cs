using UnityEngine;
using UnityEngine.Events;

public class Forcefield : MonoBehaviour
{
    public UnityEvent onEnterAction;
    public UnityEvent onExitAction;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterController>() != null)
        {
            // is player
            onEnterAction?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterController>() != null)
        {
            // is player
            onExitAction?.Invoke();
        }
    }
}
