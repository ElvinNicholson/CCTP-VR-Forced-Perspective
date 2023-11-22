using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonPickup : MonoBehaviour
{
    [SerializeField] private float pickupRange;
    [SerializeField] private LayerMask pickupLayerMask;
    [SerializeField] private LayerMask excludePickupLayerMask;
    private GameObject currentObject;
    private float initialDistance;
    private Vector3 initialScale;

    private void Update()
    {
        if (currentObject)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, excludePickupLayerMask))
            {
                Bounds bounds = currentObject.GetComponent<Renderer>().bounds;
                currentObject.transform.position = hit.point - ray.direction * bounds.size.x;
                float scale = Vector3.Distance(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)), currentObject.transform.position) / initialDistance;
                currentObject.transform.localScale = initialScale * scale;
            }
        }
    }

    public void PickupRayHit()
    {
        if (currentObject)
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.GetComponent<Collider>().enabled = true;
            currentObject.transform.parent = null;
            currentObject = null;
            return;
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayerMask))
        {
            initialDistance = hit.distance;
            initialScale = hit.transform.localScale;

            currentObject = hit.transform.gameObject;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<Collider>().enabled = false;
            currentObject.transform.parent = transform;
        }
    }
}
