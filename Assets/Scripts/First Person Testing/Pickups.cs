using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pickups : MonoBehaviour
{
    [SerializeField] private SimplePickupInteraction playerScript;
    private XRSimpleInteractable xrScript;

    [SerializeField] private InteractionLayerMask pickupMask;
    private int coroutineFrameDuration = 3;
    private bool coroutineRunning;

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

    /// <summary>
    /// Object picked up
    /// </summary>
    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        playerScript.SetCurrentObject(gameObject, args.interactorObject.transform);
        args.interactorObject.transform.gameObject.GetComponent<XRInteractorLineVisual>().enabled = false;
        gameObject.layer = 7;
    }

    /// <summary>
    /// Object dropped
    /// </summary>
    private void OnSelectExit(SelectExitEventArgs args)
    {
        playerScript.SetCurrentObject(null, null);
        args.interactorObject.transform.gameObject.GetComponent<XRInteractorLineVisual>().enabled = true;

        gameObject.layer = 6;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        dirToPlayer = (playerScript.transform.position - transform.position).normalized;
        initialScale = transform.localScale;
        initialDistance = Vector3.Distance(playerScript.transform.position, transform.position);
        initialRotation = transform.rotation;

        StartCoroutine(OnDropped());
    }

    /// <summary>
    /// Failsafe incase object clips into another
    /// </summary>
    IEnumerator OnDropped()
    {
        float framesLeft = coroutineFrameDuration;
        coroutineRunning = true;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;

        while (framesLeft > 0)
        {
            framesLeft -= 1;
            yield return null;
        }

        GetComponent<Collider>().isTrigger = false;
        GetComponent<Rigidbody>().isKinematic = false;
        rigidbody.useGravity = true;

        coroutineRunning = false;
    }

    /// <summary>
    /// Moves object towards player to avoid clipping
    /// </summary>
    private void FixObjectClipping()
    {
        // Move object towards player
        Bounds bounds = GetComponent<Renderer>().bounds;
        float moveDist = Vector3.Distance(bounds.min, bounds.max) * 0.5f;

        transform.position += dirToPlayer * moveDist;
        float scale = Vector3.Distance(transform.position, playerScript.transform.position) / initialDistance;
        transform.localScale = initialScale * scale;
        transform.rotation = initialRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Player is colliding with object, dont allow interaction
            xrScript.interactionLayers = 0;
        }

        if (coroutineRunning)
        {
            // Collision occurs after dropping object (Most likely object clipped through another)
            FixObjectClipping();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Player no longer colliding with object, allow interaction
            xrScript.interactionLayers = pickupMask;
        }
    }
}
