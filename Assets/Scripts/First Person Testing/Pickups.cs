using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pickups : MonoBehaviour
{
    [SerializeField] private SimplePickupInteraction playerScript;
    private XRSimpleInteractable xrScript;
    private bool pickedUp = false;

    [SerializeField] private InteractionLayerMask pickupMask;
    [SerializeField] private LayerMask triggerMask;
    private int coroutineFrameDuration = 2;
    private bool coroutineRunning;

    private float initialDistance;
    private Vector3 initialScale;
    private Vector3 dirToPlayer;
    private Quaternion initialRotation;

    private Rigidbody objectRigidbody;
    private Collider objectCollider;
    private Renderer objectRenderer;

    private string matThicknessID = "_Outline_Thickness";
    private string matColorID = "_Outline_Color";

    private int controllerHovering = 0;

    private void Start()
    {
        xrScript = GetComponent<XRSimpleInteractable>();
        objectRigidbody = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();

        xrScript.selectEntered.AddListener(OnSelectEnter);
        xrScript.selectExited.AddListener(OnSelectExit);
        xrScript.hoverEntered.AddListener(OnHoverEnter);
        xrScript.hoverExited.AddListener(OnHoverExit);
    }

    private void Update()
    {
        if (!pickedUp)
        {
            if (IsTooCloseToPlayer())
            {
                // Too close to pickup
                objectRenderer.material.SetColor(matColorID, Color.red);
            }
            else
            {
                // Can pickup
                objectRenderer.material.SetColor(matColorID, Color.white);
            }
        }
    }

    /// <summary>
    /// Object picked up
    /// </summary>
    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        if (IsTooCloseToPlayer())
        {
            return;
        }

        pickedUp = true;

        playerScript.SetCurrentObject(gameObject, args.interactorObject.transform);
        args.interactorObject.transform.gameObject.GetComponent<XRInteractorLineVisual>().enabled = false;

        // Set Layer to Holding
        gameObject.layer = 7;
    }

    /// <summary>
    /// Object dropped
    /// </summary>
    private void OnSelectExit(SelectExitEventArgs args)
    {
        if (IsTooCloseToPlayer())
        {
            return;
        }

        pickedUp = false;

        playerScript.SetCurrentObject(null, null);
        args.interactorObject.transform.gameObject.GetComponent<XRInteractorLineVisual>().enabled = true;

        // Set Layer to PickUp
        gameObject.layer = 6;

        // Enable gravity
        objectRigidbody.useGravity = true;
        objectRigidbody.constraints = RigidbodyConstraints.None;

        // Initial on dropped info
        dirToPlayer = (playerScript.transform.position - transform.position).normalized;
        initialScale = transform.localScale;
        initialDistance = Vector3.Distance(playerScript.transform.position, transform.position);
        initialRotation = transform.rotation;

        StartCoroutine(OnDropped());
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        // Enable outline
        controllerHovering++;
        objectRenderer.material.SetFloat(matThicknessID, 0.025f);
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        controllerHovering--;
        if (!pickedUp && controllerHovering <= 0)
        {
            // Disable outline
            objectRenderer.material.SetFloat(matThicknessID, 0f);
        }
    }

    /// <summary>
    /// Failsafe incase object clips into another
    /// </summary>
    IEnumerator OnDropped()
    {
        float framesLeft = coroutineFrameDuration;
        coroutineRunning = true;

        objectRigidbody.useGravity = false;
        objectRigidbody.excludeLayers = triggerMask;

        while (framesLeft > 0)
        {
            framesLeft -= 1;
            yield return null;
        }

        objectCollider.isTrigger = false;
        objectRigidbody.isKinematic = false;
        objectRigidbody.useGravity = true;
        objectRigidbody.excludeLayers = new LayerMask();
        xrScript.interactionLayers = pickupMask;

        coroutineRunning = false;
    }

    /// <summary>
    /// Moves object towards player to avoid clipping
    /// </summary>
    private void FixObjectClipping()
    {
        // Move object towards player
        Bounds bounds = objectRenderer.bounds;
        float moveDist = Vector3.Distance(bounds.min, bounds.max) * 0.5f;

        transform.position += dirToPlayer * moveDist;
        float scale = Vector3.Distance(transform.position, playerScript.transform.position) / initialDistance;
        transform.localScale = initialScale * scale;
        transform.rotation = initialRotation;
    }

    /// <summary>
    /// Force drops currentObject if its size is bigger than distance from player to avoid clipping
    /// </summary>
    private bool IsTooCloseToPlayer()
    {
        float objectMaxSize = Vector3.Distance(objectRenderer.bounds.min, objectRenderer.bounds.max);
        float distToObject = Vector3.Distance(playerScript.transform.position, transform.position);
        if (objectMaxSize > distToObject)
        {
            return true;
        }
        return false;
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
