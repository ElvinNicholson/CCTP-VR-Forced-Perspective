using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePickupInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask excludePickupLayerMask;
    [SerializeField] private Transform parent;
    private GameObject currentObject;
    private float initialDistance;
    private Vector3 initialScale;

    private void Update()
    {
        if (currentObject)
        {
            Ray ray = new Ray(transform.position, parent.position - transform.position);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, excludePickupLayerMask))
            {
                Bounds bounds = currentObject.GetComponent<Renderer>().bounds;
                currentObject.transform.position = hit.point - ray.direction * bounds.size.x;
                float scale = Vector3.Distance(transform.position, currentObject.transform.position) / initialDistance;
                currentObject.transform.localScale = initialScale * scale;
            }
        }
    }

    public void SetCurrentObject(GameObject pickupObject)
    {
        if (pickupObject == null)
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.GetComponent<Collider>().enabled = true;
            currentObject = null;
        }
        else
        {
            currentObject = pickupObject;
            initialDistance = (transform.position - currentObject.transform.position).magnitude;
            initialScale = pickupObject.transform.localScale;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<Collider>().enabled = false;
        }
    }
}