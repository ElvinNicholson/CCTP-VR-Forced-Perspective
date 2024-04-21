using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamDriver.Decal;
using UnityEngine.Events;

public class AnamorphicDecalCamera : MonoBehaviour
{
    [SerializeField] private GameObject decalObject;
    [SerializeField] private List<DecalMesh> decals;
    [SerializeField] private float closeEnoughDistance;
    [SerializeField] private float closeEnoughAngle;
    [SerializeField] private GameObject hint;
    [SerializeField] private float timeToHint;
    [SerializeField] private UnityEvent OnActivated;
    private Camera decalCamera;
    private Transform mainCam;
    private bool initalized = false;
    private bool activated = false;
    private float timer = 0;
    private bool countdownActive = false;

    private void Update()
    {
        if (!initalized || activated)
        {
            return;
        }

        if (PositionIsCloseEnough() && AngleIsCloseEnough())
        {
            activated = true;
            decalObject.layer = 6;
            foreach (DecalMesh decal in decals)
            {
                decal.gameObject.SetActive(false);
            }
            hint.SetActive(false);
            countdownActive = false;

            OnActivated?.Invoke();
        }

        if (countdownActive)
        {
            CountdownHint();
        }
    }

    /// <summary>
    /// Snapshots decal camera to render texture and display it on the decal
    /// </summary>
    public void InitalizeDecals()
    {
        if (initalized)
        {
            return;
        }

        initalized = true;

        // Adjust decal camera height to player height
        mainCam = Camera.main.transform;
        float playerHeight = Mathf.Abs(mainCam.position.y - mainCam.parent.parent.position.y) + 0.025f;
        transform.localPosition = new Vector3(transform.localPosition.x, playerHeight, transform.localPosition.z);

        SnapshotRenderTexture();
    }

    /// <summary>
    /// Fits the decal object to decal camera view
    /// </summary>
    public void ProjectDecals()
    {
        SnapshotRenderTexture();

        foreach (DecalMesh decal in decals)
        {
            ScaleToCamera(decal);
        }
    }

    /// <summary>
    /// Snapshots the decal camera render texture, render textures will overlap each other when called in the same frame!
    /// </summary>
    private void SnapshotRenderTexture()
    {
        decalCamera = GetComponent<Camera>();
        decalCamera.enabled = true;
        decalCamera.Render();
        decalCamera.enabled = false;
    }

    /// <summary>
    /// Taken from: https://www.youtube.com/watch?v=8hCl4-Y6TFQ (DA LAB)
    /// </summary>
    private void ScaleToCamera(DecalMesh decal)
    {
        float planeToCamDist = Vector3.Distance(decal.transform.position, transform.position);
        float planeHeightScale = (2f * Mathf.Tan(0.5f * decalCamera.fieldOfView * Mathf.Deg2Rad) * planeToCamDist);
        float planeWidthScale = planeHeightScale * decalCamera.aspect;
        decal.transform.localScale = new Vector3(planeWidthScale, planeHeightScale, 1);
    }

    /// <summary>
    /// Checks if player camera position is close enough to decal camera position
    /// </summary>
    /// <returns>True if close enough, false otherwise</returns>
    private bool PositionIsCloseEnough()
    {
        if (Vector3.Distance(transform.position, mainCam.position) <= closeEnoughDistance)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if player camera rotation is close enough to decal camera rotation
    /// </summary>
    /// <returns>True if close enough, false otherwise</returns>
    private bool AngleIsCloseEnough()
    {
        float angle = Mathf.Abs(transform.rotation.eulerAngles.y - mainCam.root.eulerAngles.y);
        angle = Mathf.Min(angle, 360 - angle);
        if (angle <= closeEnoughAngle)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Displays hint after timeToHint elapsed
    /// </summary>
    private void CountdownHint()
    {
        if (timer < timeToHint)
        {
            timer += Time.deltaTime;
        }
        else
        {
            hint.SetActive(true);
        }
    }

    public void StartCountdown()
    {
        countdownActive = true;
    }
}
