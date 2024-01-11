using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickups : MonoBehaviour
{
    //[SerializeField] private PickupInteraction playerScript;
    [SerializeField] private SimplePickupInteraction playerScript;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private LayerMask heldMask;


    public void OnPickUp()
    {
        Debug.Log("Picked Up");
        playerScript.SetCurrentObject(gameObject);
    }

    public void OnDrop()
    {
        Debug.Log("Dropped");
        playerScript.SetCurrentObject(null);
        gameObject.layer = 6;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
}
