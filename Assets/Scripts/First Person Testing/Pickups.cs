using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickups : MonoBehaviour
{
    [SerializeField] private Collider myCollider;

    public Collider GetCollider()
    {
        return myCollider;
    }
}
