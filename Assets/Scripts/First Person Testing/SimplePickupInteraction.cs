using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePickupInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask excludePickupLayerMask;
    [SerializeField] private float distanceTreshold;
    [SerializeField] private float lerpSpeed;

    private Transform anchor;
    private Vector3 lerpedPosition = Vector3.zero;
    private Vector3 lastRayHit;

    private GameObject currentObject;
    private float initialDistance;
    private Vector3 initialScale;

    private void Update()
    {
        if (currentObject)
        {
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
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.GetComponent<Collider>().enabled = true;
            currentObject = null;
            anchor = null;
        }
        else
        {
            currentObject = pickupObject;
            anchor = controllerTransform;
            lerpedPosition = currentObject.transform.position;
            initialDistance = (transform.position - currentObject.transform.position).magnitude;
            initialScale = pickupObject.transform.localScale;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<Collider>().enabled = false;
        }
    }
}