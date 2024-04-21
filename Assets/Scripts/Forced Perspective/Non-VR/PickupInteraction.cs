using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TAKEN FROM https://itch.io/blog/547361/unity-superliminal-tutorial (Atrwae)
public class PickupInteraction : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float pickupRange;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private LayerMask heldMask;
    [SerializeField] private LayerMask terrainMask;
    [SerializeField] private float nudgeDistance;

    private GameObject currentObject;
    private float initialDistanceScaleRatio;
    private Vector3 initialScale;
    private Vector3 initialViewportPos;
    private Vector3 left;
    private Vector3 right;
    private Vector3 top;
    private Vector3 bottom;
    private List<Vector3> shapedGrid = new List<Vector3>();

    [SerializeField] private int NUMBER_OF_GRID_ROWS = 10;
    [SerializeField] private int NUMBER_OF_GRID_COLUMNS = 10;

    private void FixedUpdate()
    {
        if (currentObject == null)
        {
            return;
        }

        MoveInFrontOfObstacles();
        UpdateScale();
    }

    private void MoveInFrontOfObstacles()
    {
        if (shapedGrid.Count == 0) throw new System.Exception("Shaped grid calculation error");

        float closestZ = 1000;
        for (int i = 0; i < shapedGrid.Count; i++)
        {
            RaycastHit hit = CastTowardsGridPoint(shapedGrid[i], terrainMask + pickupMask);
            if (hit.collider == null) continue;

            Vector3 wallPoint = mainCamera.transform.InverseTransformPoint(hit.point);
            if (i == 0 || wallPoint.z < closestZ)
            {
                //Find the closest point of the obstacle(s) to the camera
                closestZ = wallPoint.z;
            }
        }

        //Move the held object in front of the closestZ
        float boundsMagnitude = currentObject.GetComponent<Renderer>().localBounds.extents.magnitude * currentObject.transform.localScale.x;
        Vector3 newLocalPos = currentObject.transform.localPosition;
        newLocalPos.z = closestZ - boundsMagnitude;
        currentObject.transform.localPosition = newLocalPos;
    }

    private void UpdateScale()
    {
        float newScale = (mainCamera.transform.position - currentObject.transform.position).magnitude / initialDistanceScaleRatio;
        //if (Mathf.Abs(newScale - currentObject.transform.localScale.x) < SCALE_MARGIN) return;

        currentObject.transform.localScale = new Vector3(newScale, newScale, newScale);
        //By scaling we're actually changing the viewportPosition of heldObject and we don't want that
        Vector3 newPos = mainCamera.ViewportToWorldPoint(new Vector3(initialViewportPos.x, initialViewportPos.y,
            (currentObject.transform.position - mainCamera.transform.position).magnitude));
        currentObject.transform.position = newPos;
    }

    private void OnDrawGizmos()
    {
        if (currentObject == null) return;

        //Hits
        Gizmos.matrix = mainCamera.transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        foreach (Vector3 point in shapedGrid)
        {
            Gizmos.DrawSphere(point, .01f);
        }
    }

    public void SetCurrentObject(GameObject pickupObject)
    {
        if (pickupObject == null)
        {
            currentObject = null;
        }
        else
        {
            currentObject = pickupObject;

            initialScale = pickupObject.transform.localScale;
            currentObject = pickupObject.transform.gameObject;

            currentObject.layer = 7;
            currentObject.GetComponent<Rigidbody>().useGravity = false;
            currentObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            //currentObject.transform.parent = mainCamera.transform;
            initialDistanceScaleRatio = (mainCamera.transform.position - currentObject.transform.position).magnitude / initialScale.x;
            initialViewportPos = mainCamera.WorldToViewportPoint(currentObject.transform.position);

            Vector3[] boundingBoxPoints = GetBoundingBoxPoints();
            SetupShapedGrid(boundingBoxPoints);
        }
    }

    public void PickupRayHit()
    {
        if (currentObject)
        {
            // Drop
            currentObject.layer = pickupMask;
            currentObject.GetComponent<Rigidbody>().useGravity = true;
            currentObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            currentObject.transform.parent = null;
            currentObject = null;
            return;
        }

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask))
        {
            // Pickup
            initialScale = hit.transform.localScale;
            currentObject = hit.transform.gameObject;

            currentObject.layer = heldMask;
            currentObject.GetComponent<Rigidbody>().useGravity = false;
            currentObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            currentObject.transform.parent = mainCamera.transform;
            initialDistanceScaleRatio = (mainCamera.transform.position - currentObject.transform.position).magnitude / initialScale.x;
            initialViewportPos = mainCamera.WorldToViewportPoint(currentObject.transform.position);

            Vector3[] boundingBoxPoints = GetBoundingBoxPoints();
            SetupShapedGrid(boundingBoxPoints);
        }
    }

    private Vector3[] GetBoundingBoxPoints()
    {
        Vector3 size = currentObject.GetComponent<Renderer>().localBounds.size;
        Vector3 x = new Vector3(size.x, 0, 0);
        Vector3 y = new Vector3(0, size.y, 0);
        Vector3 z = new Vector3(0, 0, size.z);
        Vector3 min = currentObject.GetComponent<Renderer>().localBounds.min;
        Vector3[] bbPoints =
            {
            min,
            min + x,
            min + y,
            min + x + y,
            min + z,
            min + z + x,
            min + z + y,
            min + z + x + y
            };
        return bbPoints;
    }

    private void SetupShapedGrid(Vector3[] bbPoints)
    {
        left = right = top = bottom = Vector2.zero;
        GetRectConfines(bbPoints);

        Vector3[,] grid = SetupGrid();
        GetShapedGrid(grid);
    }

    private void GetRectConfines(Vector3[] bbPoints)
    {
        Vector3 bbPoint;
        Vector3 cameraPoint;
        Vector2 viewportPoint;
        Vector3 closestPoint = currentObject.GetComponent<Renderer>().localBounds.ClosestPoint(Camera.main.transform.position);
        float closestZ = mainCamera.transform.InverseTransformPoint(currentObject.transform.TransformPoint(closestPoint)).z;
        if (closestZ <= 0) throw new System.Exception("HeldObject's inside the player!");

        for (int i = 0; i < bbPoints.Length; i++)
        {
            bbPoint = currentObject.transform.TransformPoint(bbPoints[i]);
            viewportPoint = mainCamera.WorldToViewportPoint(bbPoint);
            cameraPoint = mainCamera.transform.InverseTransformPoint(bbPoint);
            cameraPoint.z = closestZ;

            if (viewportPoint.x < 0 || viewportPoint.x > 1
                || viewportPoint.y < 0 || viewportPoint.y > 1) continue;

            if (i == 0) left = right = top = bottom = cameraPoint;

            if (cameraPoint.x < left.x) left = cameraPoint;
            if (cameraPoint.x > right.x) right = cameraPoint;
            if (cameraPoint.y > top.y) top = cameraPoint;
            if (cameraPoint.y < bottom.y) bottom = cameraPoint;
        }
    }

    private Vector3[,] SetupGrid()
    {
        float rectHrLength = right.x - left.x;
        float rectVertLength = top.y - bottom.y;
        Vector3 hrStep = new Vector2(rectHrLength / (NUMBER_OF_GRID_COLUMNS - 1), 0);
        Vector3 vertStep = new Vector2(0, rectVertLength / (NUMBER_OF_GRID_ROWS - 1));

        Vector3[,] grid = new Vector3[NUMBER_OF_GRID_ROWS, NUMBER_OF_GRID_COLUMNS];
        grid[0, 0] = new Vector3(left.x, bottom.y, left.z);

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int w = 0; w < grid.GetLength(1); w++)
            {
                if (i == 0 & w == 0) continue;
                else if (w == 0)
                {
                    grid[i, w] = grid[i - 1, 0] + vertStep;
                }
                else grid[i, w] = grid[i, w - 1] + hrStep;
            }
        }
        return grid;
    }
    private void GetShapedGrid(Vector3[,] grid)
    {
        shapedGrid.Clear();
        foreach (Vector3 point in grid)
        {
            RaycastHit hit = CastTowardsGridPoint(point, LayerMask.GetMask("Holding"));
            if (hit.collider != null) shapedGrid.Add(point);
        }
    }

    private RaycastHit CastTowardsGridPoint(Vector3 gridPoint, LayerMask layers)
    {
        Vector3 worldPoint = mainCamera.transform.TransformPoint(gridPoint);
        Vector3 origin = mainCamera.WorldToViewportPoint(worldPoint);
        origin.z = 0;
        origin = mainCamera.ViewportToWorldPoint(origin);
        Vector3 direction = worldPoint - origin;
        RaycastHit hit;
        Physics.Raycast(origin, direction, out hit, 1000, layers);
        return hit;
    }
}
