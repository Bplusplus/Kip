using UnityEngine;
using System.Collections;
public class RaycastController : MonoBehaviour {

    public LayerMask collisionMask;
    const float dstBetweenRays = .25f;
    [HideInInspector]
    public int horizontalRayCount;
    [HideInInspector]
    public int verticalRayCount;

    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    public const float skinWidth = 0.015f;

    [HideInInspector]
    public BoxCollider2D collider;
    public RaycastOrigins raycastOrigins;

    public virtual void  Awake()
    {
        collider = GetComponent<BoxCollider2D>();

    }
    public virtual void Start() {
        CalculateRaySpacing();
    }
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);

        Debug.DrawLine(raycastOrigins.topLeft, raycastOrigins.topRight, Color.magenta);
        Debug.DrawLine(raycastOrigins.topRight, raycastOrigins.bottomRight, Color.magenta);
        Debug.DrawLine(raycastOrigins.bottomRight, raycastOrigins.bottomLeft, Color.magenta);
        Debug.DrawLine(raycastOrigins.bottomLeft, raycastOrigins.topLeft, Color.magenta);
    }

   public void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);


        horizontalRaySpacing = Mathf.Clamp(horizontalRaySpacing, 2, int.MaxValue);
        verticalRaySpacing = Mathf.Clamp(horizontalRaySpacing, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }
}
