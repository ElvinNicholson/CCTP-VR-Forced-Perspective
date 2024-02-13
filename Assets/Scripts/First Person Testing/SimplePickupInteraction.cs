using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePickupInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask excludePickupLayerMask;
    [SerializeField] private LayerMask holdingLayerMask;
    [SerializeField] private float distanceTreshold;
    [SerializeField] private float lerpSpeed;

    private Transform anchor;
    private Vector3 lerpedPosition = Vector3.zero;
    private Vector3 lastRayHit;

    private GameObject currentObject;
    private float initialDistance;
    private Vector3 initialScale;

    private List<Vector3> boundingBoxPoints;
    private List<Vector3> validBoundingBoxPoints;
    [SerializeField] private int gridSize;

    private void OnDrawGizmos()
    {
        if (validBoundingBoxPoints == null)
        {
            return;
        }

        Gizmos.color = Color.red;

        foreach (Vector3 point in validBoundingBoxPoints)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }
    }

    private void Update()
    {
        if (currentObject)
        {
            boundingBoxPoints = GetBoundingBoxPoints();
            validBoundingBoxPoints = RaycastPoint();

            Ray anchorRay = new Ray(anchor.position, anchor.forward);
            Physics.Raycast(anchorRay, out RaycastHit anchorRayHit, Mathf.Infinity, excludePickupLayerMask);

            if (Vector3.Distance(anchorRayHit.point, lastRayHit) < distanceTreshold * anchorRayHit.distance)
            {
                // Small movements
            }
            else
            {
                // Large movements
                lastRayHit = anchorRayHit.point;
            }

            lerpedPosition = Vector3.Lerp(lerpedPosition, lastRayHit, lerpSpeed * Time.deltaTime);

            Ray ray = new Ray(transform.position, lerpedPosition - transform.position);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, excludePickupLayerMask))
            {
                Bounds bounds = currentObject.GetComponent<Renderer>().bounds;
                currentObject.transform.position = hit.point - ray.direction * bounds.size.x;
                float scale = Vector3.Distance(transform.position, currentObject.transform.position) / initialDistance;
                currentObject.transform.localScale = initialScale * scale;
            }
        }
    }

    public void SetCurrentObject(GameObject pickupObject, Transform controllerTransform)
    {
        if (pickupObject == null)
        {
            // Object Dropped
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.GetComponent<Collider>().enabled = true;
            currentObject = null;
            anchor = null;
        }
        else
        {
            // Object Picked Up
            currentObject = pickupObject;
            anchor = controllerTransform;
            lerpedPosition = currentObject.transform.position;
            initialDistance = (transform.position - currentObject.transform.position).magnitude;
            initialScale = pickupObject.transform.localScale;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<Collider>().enabled = false;
        }
    }

    private List<Vector3> GetBoundingBoxPoints()
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 cellSize = currentObject.GetComponent<Renderer>().bounds.size / gridSize;

        // Iterate through grid points
        for (int x = 0; x < gridSize + 1; x++)
        {
            for (int y = 0; y < gridSize + 1; y++)
            {
                for (int z = 0; z < gridSize + 1; z++)
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

    private List<Vector3> RaycastPoint()
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
}