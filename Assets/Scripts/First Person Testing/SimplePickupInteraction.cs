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
    //private Vector3 lerpedPosition2;

    private GameObject currentObject;
    private float initialDistance;
    private Vector3 initialScale;

    private void Update()
    {
        if (currentObject)
        {
            Ray anchorRay = new Ray(anchor.position, anchor.forward);
            Physics.Raycast(anchorRay, out RaycastHit anchorRayHit, Mathf.Infinity, excludePickupLayerMask);

            if (lerpedPosition == Vector3.zero)
            {
                lerpedPosition = anchorRayHit.point;
            }

            lerpedPosition = Vector3.Lerp(lerpedPosition, anchorRayHit.point, lerpSpeed * Time.deltaTime);

            Ray ray = new Ray(transform.position, anchorRayHit.point - transform.position);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, excludePickupLayerMask))
            {
                if (Vector3.Distance(hit.point, currentObject.transform.position) > distanceTreshold * currentObject.transform.localScale.x)
                {
                    //lerpedPosition2 = Vector3.Lerp(lerpedPosition2, hit.point, lerpSpeed * Time.deltaTime);

                    Bounds bounds = currentObject.GetComponent<Renderer>().bounds;
                    currentObject.transform.position = hit.point - ray.direction * bounds.size.x;
                    float scale = Vector3.Distance(transform.position, currentObject.transform.position) / initialDistance;
                    currentObject.transform.localScale = initialScale * scale;
                }
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
            lerpedPosition = Vector3.zero;
            initialDistance = (transform.position - currentObject.transform.position).magnitude;
            initialScale = pickupObject.transform.localScale;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<Collider>().enabled = false;
        }
    }
}