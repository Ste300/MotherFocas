using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimSystem3D : MonoBehaviour
{
    public LineRenderer line;
    public Transform originTransform;
    public float maxDragDistance = 6f; 
    public LayerMask groundMask;       

    // Grabs the LineRenderer component.
    void Reset()
    {
        line = GetComponent<LineRenderer>();
    }

    // Ensures the LineRenderer is ready.
    void Awake()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        line.positionCount = 0;
    }
    
    // Draws the aim line from the origin to the target, clamped by max distance.
    public void DrawAim(Vector3 visualTarget, bool pullToLaunch = true)
    {
        if (originTransform == null) return;

        Vector3 origin = originTransform.position;
        origin.y = visualTarget.y = origin.y;

        Vector3 dir = visualTarget - origin;
        if (pullToLaunch) dir = -dir;

        Vector3 visPoint = origin + Vector3.ClampMagnitude(dir, maxDragDistance);

        line.positionCount = 2;
        line.SetPosition(0, origin);
        line.SetPosition(1, visPoint);
    }

    // Hides the aim line.
    public void Hide()
    {
        line.positionCount = 0;
    }
    
    // Calculates launch strength (0-1) based on drag distance.
    public float CalculateStrength(Vector3 worldDragPoint)
    {
        if (originTransform == null) return 0f;
        Vector3 origin = originTransform.position;
        origin.y = worldDragPoint.y = origin.y;
        float dist = Vector3.Distance(origin, worldDragPoint);
        return Mathf.Clamp01(dist / maxDragDistance);
    }
    
    // Gets the 2D direction vector from origin to the world point.
    public Vector3 GetDirection(Vector3 worldPoint)
    {
        if (originTransform == null) return Vector3.zero;
        Vector3 dir = worldPoint - originTransform.position;
        dir.y = 0f;
        return dir;
    }
    
    // Converts a screen position to a 3D world point on the ground plane.
    public bool ScreenPointToGroundPoint(Vector3 screenPos, Camera cam, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 200f, groundMask))
        {
            worldPoint = hit.point;
            return true;
        }
        
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float enter;
        if (plane.Raycast(ray, out enter))
        {
            worldPoint = ray.GetPoint(enter);
            return true;
        }
        return false;
    }
}