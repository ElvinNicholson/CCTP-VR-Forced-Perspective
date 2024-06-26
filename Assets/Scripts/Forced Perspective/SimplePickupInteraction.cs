using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class SimplePickupInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask excludeHoldingLayer;
    private LayerMask holdingLayerMask;
    private LayerMask initialLayerMask;
    [SerializeField] private float distanceTreshold;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private float rotateSpeed;

    private Transform anchor;
    private Vector3 lerpedPosition = Vector3.zero;
    private Vector3 lastRayHit;

    private GameObject currentObject;
    private float initialDistance;
    private Vector3 initialScale;

    private List<Vector3> shapedGridPoints;
    [SerializeField] private int gridSize;
    private Vector3 rayHitPoint;
    private Vector3 closestGridPoint;

    private Quaternion objectAngle;

    [Header("XR Interactors")]
    [SerializeField] private InputActionReference leftMoveAction;
    [SerializeField] private InputActionReference rightTurnAction;
    [SerializeField] private InputActionReference rightThumbstickAction;
    [SerializeField] private XRRayInteractor leftRay;
    [SerializeField] private XRRayInteractor rightRay;

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(lastRayHit, 0.1f);

        Gizmos.color = Color.red;
        if (shapedGridPoints != null)
        {
            foreach (Vector3 point in shapedGridPoints)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rayHitPoint, 0.1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(closestGridPoint, 0.1f);
    }

    private void Start()
    {
        holdingLayerMask = LayerMask.GetMask("Holding");
        initialLayerMask = leftRay.raycastMask;

        rightThumbstickAction.action.Disable();
        rightThumbstickAction.action.performed += OnThumbstick;
    }

    private void Update()
    {
        if (currentObject)
        {
            // Angle
            currentObject.transform.rotation = Quaternion.Lerp(currentObject.transform.rotation, objectAngle, Time.deltaTime * rotateSpeed);

            // Create gridpoints
            shapedGridPoints = GetShapedGridPoints(GetBoundingGridPoints(gridSize));
            // Get distance to nearest collided obstacle
            float closestDistance = RaycastGridPoints();

            // Aim smoothing
            Ray anchorRay = new Ray(anchor.position, anchor.forward);
            Physics.Raycast(anchorRay, out RaycastHit anchorRayHit, Mathf.Infinity, excludeHoldingLayer);

            if (Vector3.Distance(anchorRayHit.point, lastRayHit) > distanceTreshold * anchorRayHit.distance)
            {
                // Large movements
                lastRayHit = anchorRayHit.point;
            }

            lerpedPosition = Vector3.Lerp(lerpedPosition, lastRayHit, lerpSpeed * Time.deltaTime);

            Vector3 direction = (lerpedPosition - transform.position).normalized;
            Bounds bounds = currentObject.GetComponent<Renderer>().bounds;
            float longestBoundDist = Vector3.Distance(bounds.min, bounds.max);

            // Move and scale
            currentObject.transform.position = transform.position + (direction * closestDistance) - (direction * longestBoundDist / 2);
            float scale = Vector3.Distance(transform.position, currentObject.transform.position) / initialDistance;
            currentObject.transform.localScale = initialScale * scale;
        }
    }

    public void SetCurrentObject(GameObject pickupObject, Transform controllerTransform)
    {
        if (pickupObject == null)
        {
            // Enable interactors
            ToggleInteractor(true, null);

            // Object Dropped
            ImproveDropPos(5);

            // Enable movement, disable turning
            rightTurnAction.action.Enable();
            rightThumbstickAction.action.Disable();

            currentObject = null;
            anchor = null;
            lastRayHit = Vector3.zero;
        }
        else
        {
            // Disable interactors
            ToggleInteractor(false, controllerTransform.parent.name);

            // Object Picked Up
            currentObject = pickupObject;
            anchor = controllerTransform;

            // Set initial values
            lerpedPosition = currentObject.transform.position;
            initialDistance = (transform.position - currentObject.transform.position).magnitude;
            initialScale = pickupObject.transform.localScale;

            // Disable object physics
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<Collider>().isTrigger = true;

            // Round currentObject angle to nearest axis
            Vector3 roundedAngle = currentObject.transform.eulerAngles;
            roundedAngle.x = Mathf.Round(roundedAngle.x / 90f) * 90f;
            roundedAngle.y = Mathf.Round(roundedAngle.y / 90f) * 90f;
            roundedAngle.z = Mathf.Round(roundedAngle.z / 90f) * 90f;
            objectAngle = Quaternion.Euler(roundedAngle);

            leftMoveAction.action.Enable();
            rightTurnAction.action.Disable();
            rightThumbstickAction.action.Enable();
        }
    }

    /// <summary>
    /// Creates a List of Vector3 grid of currentObject bounds
    /// </summary>
    /// /// <param name="size">How many points per axis</param>
    private List<Vector3> GetBoundingGridPoints(int size)
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 cellSize = currentObject.GetComponent<Renderer>().bounds.size / size;

        // Iterate through grid points
        for (int x = 0; x < size + 1; x++)
        {
            for (int y = 0; y < size + 1; y++)
            {
                for (int z = 0; z < size + 1; z++)
                {
                    // Calculate grid point position
                    Vector3 point = currentObject.GetComponent<Renderer>().bounds.min +
                        new Vector3(x * cellSize.x, y * cellSize.y, z * cellSize.z);

                    points.Add(point);
                }
            }
        }

        return points;
    }

    /// <summary>
    /// Raycasts boundingGridPoints to currentObject to get a more precise grid from player's perspective
    /// </summary>
    /// <param name="boundingBoxPoints">Output from GetBoundingGridPoints function</param>
    private List<Vector3> GetShapedGridPoints(List<Vector3> boundingBoxPoints)
    {
        List<Vector3> validPoints = new List<Vector3>();

        foreach (Vector3 point in boundingBoxPoints)
        {
            Vector3 direction = point - transform.position;
            if (Physics.Raycast(transform.position, direction, Mathf.Infinity, holdingLayerMask))
            {
                validPoints.Add(point);
            }
        }

        return validPoints;
    }

    /// <summary>
    /// Performs raycasts of shapedGridPoints list to the world to find nearest collision
    /// </summary>
    /// <returns>Distance from transform to collision</returns>
    private float RaycastGridPoints()
    {
        float closestDistance = Mathf.Infinity;
        foreach (Vector3 point in shapedGridPoints)
        {
            Vector3 direction = (point - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity, excludeHoldingLayer))
            {
                if (closestDistance > hit.distance)
                {
                    closestDistance = hit.distance;
                    rayHitPoint = hit.point;
                    closestGridPoint = point;
                }
            }
        }

        return closestDistance;
    }

    /// <summary>
    /// Called when object is dropped, improves dropped location accuracy
    /// </summary>
    private void ImproveDropPos(int maxIteration)
    {
        Vector3 closestGridToRayHit = currentObject.GetComponent<Collider>().ClosestPoint(rayHitPoint);
        float distance = Vector3.Distance(closestGridToRayHit, rayHitPoint);
        if (distance > 0.1f && maxIteration > 0)
        {
            Vector3 moveDirection = (lerpedPosition - transform.position).normalized;
            currentObject.transform.position = currentObject.transform.position + (moveDirection * distance * 0.5f);
            float newScale = Vector3.Distance(transform.position, currentObject.transform.position) / initialDistance;
            currentObject.transform.localScale = initialScale * newScale;

            shapedGridPoints = GetShapedGridPoints(GetBoundingGridPoints(gridSize));
            RaycastGridPoints();

            ImproveDropPos(maxIteration - 1);
        }
    }

    /// <summary>
    /// currentObject rotation control
    /// </summary>
    private void OnThumbstick(InputAction.CallbackContext context)
    {
        Vector2 thumbstickValue = context.ReadValue<Vector2>();

        if (Mathf.Abs(thumbstickValue.x) > 0.3f)
        {
            objectAngle = Quaternion.AngleAxis(thumbstickValue.x / Mathf.Abs(thumbstickValue.x) * 90f, Vector3.down) * currentObject.transform.rotation;
        }

        else if (Mathf.Abs(thumbstickValue.y) > 0.3f)
        {
            // Get rotational axis
            Vector3 axis = (transform.position - currentObject.transform.position).normalized;
            axis.y = 0;
            if (Mathf.Abs(axis.x) >= Mathf.Abs(axis.z))
            {
                axis.x = 0;
                axis.z = axis.z / Mathf.Abs(axis.z);
            }
            else
            {
                axis.x = axis.x / Mathf.Abs(axis.x);
                axis.z = 0;
            }

            objectAngle = Quaternion.AngleAxis(thumbstickValue.y / Mathf.Abs(thumbstickValue.y) * 90f, axis) * currentObject.transform.rotation;
        }
    }

    /// <summary>
    /// Toggles wheter hand rays interactors can detect object in PickUp layer
    /// </summary>
    private void ToggleInteractor(bool toggle, string controllerName)
    {
        if (toggle)
        {
            leftRay.raycastMask = initialLayerMask;
            rightRay.raycastMask = initialLayerMask;
        }
        else
        {
            if (controllerName.Contains("Left"))
            {
                leftRay.raycastMask = holdingLayerMask;
                rightRay.raycastMask = 0;
            }
            else
            {
                leftRay.raycastMask = 0;
                rightRay.raycastMask = holdingLayerMask;
            }
        }

    }

    public void DisableInteractionLayer(bool toggle)
    {
        if (toggle)
        {
            // Disable
            leftRay.interactionLayers = new InteractionLayerMask();
            rightRay.interactionLayers = new InteractionLayerMask();
        }
        else
        {
            // Enable
            leftRay.interactionLayers = InteractionLayerMask.GetMask("Pickup Objects");
            rightRay.interactionLayers = InteractionLayerMask.GetMask("Pickup Objects");
        }
    }
}