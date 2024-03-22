using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamDriver.Decal;

public class AnamorphicDecalCamera : MonoBehaviour
{
    [SerializeField] private GameObject decalObject;
    [SerializeField] private List<DecalMesh> decals;
    [SerializeField] private float closeEnoughDistance;
    [SerializeField] private float closeEnoughAngle;
    [SerializeField] private Vector3 positionOffset;
    private Camera decalCamera;
    private Transform mainCam;
    private bool initalized = false;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (!initalized)
        {
            return;
        }

        if (PositionIsCloseEnough() && AngleIsCloseEnough())
        {
            decalObject.layer = 6;
            foreach (DecalMesh decal in decals)
            {
                decal.gameObject.SetActive(false);
            }
        }
    }

    public void InitalizeDecals()
    {
        if (initalized)
        {
            return;
        }

        initalized = true;

        mainCam = Camera.main.transform;
        //float playerHeight = mainCam.parent.parent.GetComponent<CharacterController>().height;
        float playerHeight = Mathf.Abs(mainCam.position.y - mainCam.parent.parent.position.y);
        transform.localPosition = new Vector3(transform.localPosition.x, playerHeight, transform.localPosition.z);

        SnapshotRenderTexture();
    }

    public void ProjectDecals()
    {
        SnapshotRenderTexture();

        foreach (DecalMesh decal in decals)
        {
            ScaleToCamera(decal);
        }
    }

    private void SnapshotRenderTexture()
    {
        decalCamera = GetComponent<Camera>();
        decalCamera.enabled = true;
        decalCamera.Render();
        decalCamera.enabled = false;
    }

    private void ScaleToCamera(DecalMesh decal)
    {
        float planeToCamDist = Vector3.Distance(decal.transform.position, transform.position);
        float planeHeightScale = (2f * Mathf.Tan(0.5f * decalCamera.fieldOfView * Mathf.Deg2Rad) * planeToCamDist);
        float planeWidthScale = planeHeightScale * decalCamera.aspect;
        decal.transform.localScale = new Vector3(planeWidthScale, planeHeightScale, 1);
    }

    private bool PositionIsCloseEnough()
    {
        if (Vector3.Distance(transform.position + positionOffset, mainCam.position) <= closeEnoughDistance)
        {
            return true;
        }
        return false;
    }

    private bool AngleIsCloseEnough()
    {
        if (Mathf.Abs(transform.rotation.eulerAngles.y - mainCam.root.eulerAngles.y) <= closeEnoughAngle)
        {
            return true;
        }
        return false;
    }
}
