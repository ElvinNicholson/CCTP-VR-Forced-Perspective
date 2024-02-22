using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pickups : MonoBehaviour
{
    [SerializeField] private SimplePickupInteraction playerScript;
    private XRSimpleInteractable xrScript;
    private bool pickedUp;
    private bool lastPickedUpState;

    private int adjustmentFrames = 1;
    private int framesLeft;

    private float initialDistance;
    private Vector3 initialScale;
    private Vector3 dirToPlayer;
    private Quaternion initialRotation;

    private void Start()
    {
        xrScript = GetComponent<XRSimpleInteractable>();
        xrScript.selectEntered.AddListener(OnSelectEnter);
        xrScript.selectExited.AddListener(OnSelectExit);
    }

    private void Update()
    {
        if (lastPickedUpState != pickedUp)
        {
            lastPickedUpState = pickedUp;
            // State change
            if (pickedUp == false)
            {
                // Object just got dropped
                framesLeft = adjustmentFrames;
            }
        }

        if (framesLeft > 0)
        {
            framesLeft -= 1;
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        pickedUp = true;
        playerScript.SetCurrentObject(gameObject, args.interactorObject.transform);
        args.interactorObject.transform.gameObject.GetComponent<XRInteractorLineVisual>().enabled = false;
        gameObject.layer = 7;
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        pickedUp = false;
        playerScript.SetCurrentObject(null, null);
        args.interactorObject.transform.gameObject.GetComponent<XRInteractorLineVisual>().enabled = true;

        gameObject.layer = 6;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        dirToPlayer = (playerScript.transform.position - transform.position).normalized;
        initialScale = transform.localScale;
        initialDistance = (transform.position - playerScript.transform.position).magnitude;
        initialRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (framesLeft > 0)
        {
            Debug.Log("Colliding");

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            Bounds bounds = GetComponent<Renderer>().bounds;
            float moveDist = Vector3.Distance(bounds.min, bounds.max) * 0.5f;

            transform.position = transform.position + (dirToPlayer * moveDist);
            float scale = Vector3.Distance(transform.position, playerScript.transform.position) / initialDistance;
            transform.localScale = initialScale * scale;
            transform.rotation = initialRotation;
        }
    }
}
