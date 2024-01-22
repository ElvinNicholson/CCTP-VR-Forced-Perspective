using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pickups : MonoBehaviour
{
    //[SerializeField] private PickupInteraction playerScript;
    [SerializeField] private SimplePickupInteraction playerScript;
    private XRSimpleInteractable xrScript;

    private void Start()
    {
        xrScript = GetComponent<XRSimpleInteractable>();
        xrScript.selectEntered.AddListener(OnSelectEnter);
        xrScript.selectExited.AddListener(OnSelectExit);
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log("Picked Up");
        playerScript.SetCurrentObject(gameObject, args.interactorObject.transform);
        args.interactorObject.transform.gameObject.GetComponent<XRInteractorLineVisual>().enabled = false;
        gameObject.layer = 7;
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        Debug.Log("Dropped");
        playerScript.SetCurrentObject(null, null);
        args.interactorObject.transform.gameObject.GetComponent<XRInteractorLineVisual>().enabled = true;

        gameObject.layer = 6;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
}
