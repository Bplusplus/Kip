
using UnityEngine;
using System.Collections;
public class Controller2D : RaycastController {
   
    public float maxSlopeAngle = 80;
   
    [HideInInspector]
    public Vector2 playerInput;
    
    public CollisionInfo colisions;
    // Use this for initialization

    public override void Start()
    {
        base.Start();
        colisions.faceDir = 1;
    }
    public void Move(Vector2 moveAmount, bool isStandingOnPlatform) {
        Move(moveAmount, Vector2.zero,isStandingOnPlatform);
    }
    public void Move(Vector2 moveAmount,Vector2 input, bool isStandingOnplatform = false)
    {
        playerInput = input;
        UpdateRaycastOrigins();
        colisions.Reset();
        colisions.moveAmountOld = moveAmount;
        
        
        if (moveAmount.y < 0) {
            DescendSlope(ref moveAmount);
        }
        if (moveAmount.x != 0)
        {
            colisions.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        HorizontalCollisions(ref moveAmount);
        
        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }
        transform.Translate(moveAmount);
        if (isStandingOnplatform)
        {
            colisions.below = true;
        }
    }


    void VerticalCollisions(ref Vector2 moveAmount) {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) +skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY *rayLength, Color.red);
            if (hit) {
                if (hit.collider.tag == "Through") {
                    if (colisions.fallingThroughPlatform)
                    {
                        continue;
                    }
                    if (directionY == 1 || hit.distance==0) {
                        continue;
                        colisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPLatform",.5f);
                    }
                    if (playerInput.y == -1) {
                        continue;
                    }
                }
                moveAmount.y = (hit.distance - skinWidth) * directionY;
                if (colisions.climbingSlope) {
                    moveAmount.x = moveAmount.y/Mathf.Tan(colisions.slopeAngle * Mathf.Deg2Rad) *Mathf.Sign(moveAmount.x);
                }
                rayLength = hit.distance;
                colisions.above = directionY == 1;
                colisions.below = directionY == -1;
            }
        }
        if (colisions.climbingSlope) {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
            Vector2 raycastOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) +Vector2.up *moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.right * directionX, rayLength, collisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != colisions.slopeAngle) {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    colisions.slopeAngle = slopeAngle;
                    colisions.slopeNormal = hit.normal;
                }
            }
            }

    }
    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = colisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
        if (Mathf.Abs(moveAmount.x) < skinWidth) {
            rayLength = 2 * skinWidth;
        }
       // print(directionX);
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit)
            {

                if (hit.distance == 0) {
                    continue;
                }
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle<=maxSlopeAngle)
                {
                    if (colisions.descendingSlope) {
                        colisions.descendingSlope = false;
                        moveAmount = colisions.moveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != colisions.oldSlopeAngle) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle,hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }
                if (!colisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;
                    colisions.left = directionX == -1;
                    colisions.right = directionX == 1;
                }
                if (colisions.climbingSlope) {
                    moveAmount.y = Mathf.Tan(colisions.slopeAngle *Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                }
            }
        }

    }
    
    void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal) {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbSlopemoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (moveAmount.y <= climbSlopemoveAmountY)
      
        {
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            moveAmount.y = climbSlopemoveAmountY;
            colisions.below = true;
            colisions.climbingSlope = true;
            colisions.slopeAngle = slopeAngle;
            colisions.slopeNormal = slopeNormal;

        }
    }
    void DescendSlope(ref Vector2 moveAmount) {

        RaycastHit2D maxSlopeLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        if (maxSlopeLeft ^ maxSlopeRight)
        {
            SlideDownMaxSlope(maxSlopeLeft, ref moveAmount);
            SlideDownMaxSlope(maxSlopeRight, ref moveAmount);
        } 
        if (!colisions.slidingDownMaxSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomRight);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendSlopemoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendSlopemoveAmountY;
                            colisions.slopeAngle = slopeAngle;
                            colisions.descendingSlope = true;
                            colisions.below = true;
                            colisions.slopeNormal = hit.normal;

                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount) {
        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle) {
                moveAmount.x = Mathf.Sign(hit.normal.x) *(Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle*Mathf.Deg2Rad);
                colisions.slopeAngle = slopeAngle;
                colisions.slidingDownMaxSlope = true;
                colisions.slopeNormal = hit.normal;
            }
        }

    }
    void ResetFallingThroughPlatform() {
        colisions.fallingThroughPlatform = false;
    }
   
    public struct CollisionInfo {
        public bool above, below, right, left, climbingSlope, descendingSlope,slidingDownMaxSlope, fallingThroughPlatform;
        public int faceDir;
        public Vector2 slopeNormal;
        public float slopeAngle, oldSlopeAngle;
        public Vector2 moveAmountOld;
        public void Reset() {
            oldSlopeAngle = slopeAngle;
            slopeAngle = 0;
            above = below = left = right =climbingSlope = slidingDownMaxSlope =descendingSlope = false;
            slopeNormal = Vector2.zero;
        }
        public bool collideAny() {
            return (above || below ||right||left);
        }
    }
}
