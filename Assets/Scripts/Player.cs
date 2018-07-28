using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
    public float maxJumpHeight=  4f;
    public float minJumpHeight = 1;
    public float timeToJumpApex = 0.4f;
    public int moveSpeed = 6;
    float acclerationTimeAirborne =.2f;
    float accelerationTimeGrounded=.1f;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;

    Vector3 velocity;
    float velocityXsmoothing;
    float time;
    float targetVelocityX;

    public float wallSlideSpeedMax =3;
    float wallStickTime = .25f;
    float timeToUnstick;

    bool jumpKey;

    bool isBoosting;
    public float boostSpeed;
    public float boostCooldown;
    public float boostDur;
    bool dashKey;
    bool canDash;

    bool dashJumping = false;
    public Vector2 dashJump;

    public Vector2 wallClimb;
    public Vector2 wallJump;
    public Vector2 wallLeap;
    bool wallJumping;
    Vector2 directionalInput;
    Controller2D controller;

    bool wallSliding;
    int wallDirX;

	// Use this for initialization
	void Start () {
        controller = GetComponent<Controller2D>();
        gravity = -(2 * maxJumpHeight) / (Mathf.Pow(timeToJumpApex, 2));
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        print(" Gravity " + gravity + " jumpVelocity " + maxJumpVelocity);
        canDash= true;
        isBoosting=false;
        minJumpVelocity = Mathf.Sqrt((2 * Mathf.Abs(gravity) * minJumpVelocity));

    }

    void Update()
    {
        //Input code
        dashKey = (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Fire2"));

        CalculateVelocity();
        HandleWallSliding();

        if (controller.colisions.below)
        {
            dashJumping = false;
        }

        //Dash code
        if (dashKey && canDash && (controller.colisions.below || isBoosting))
        {
            time = 0;
            StartCoroutine(Boost(boostDur, controller.colisions.faceDir));
        }

        //Stop y velocity if colliding up or down

        if (!isBoosting) controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.colisions.above || controller.colisions.below)
        {
            if (controller.colisions.slidingDownMaxSlope)
            {
                velocity.y += controller.colisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else {
                velocity.y = 0;
            }

        }
    }


    public void SetDirectionalInput(Vector2 input) {
        directionalInput = input;
    }
    public void OnJumpKeyDown()
    {
        if (wallSliding)
        {
            if ((wallDirX == directionalInput.x))
            {
                velocity.x = -wallDirX * wallClimb.x;
                velocity.y = wallClimb.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJump.x;
                velocity.y = wallJump.y;
            }

            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
       

        else if (controller.colisions.below)
        {
            if (controller.colisions.slidingDownMaxSlope) {
                if (directionalInput.x != -Mathf.Sign(controller.colisions.slopeNormal.x)) {
                    velocity.y = maxJumpVelocity * controller.colisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.colisions.slopeNormal.x;

                }
            }
            else{
                velocity.y = maxJumpVelocity;
            }
        }
    }
    public void OnJumpKeyUp()
    {
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Fire1"))
        {
            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }
    }

    // Update is called once per frame
    

    void CalculateVelocity() {
        targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXsmoothing, controller.colisions.below ? accelerationTimeGrounded : acclerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    void HandleWallSliding() {
        wallDirX = (controller.colisions.left ? -1 : 1);
        wallSliding = false;
        if ((controller.colisions.left || controller.colisions.right) && (!controller.colisions.below) && velocity.y < 0)
        {
            wallSliding = true;
            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }
            if (timeToUnstick > 0)
            {
                velocity.x = 0;
                velocityXsmoothing = 0;
                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToUnstick -= Time.deltaTime;
                }
                else { timeToUnstick = wallStickTime; }
            }
            else
            {
                timeToUnstick = wallStickTime;
            }
            if (dashKey)
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
    }
    IEnumerator Boost(float boostDur,int boostDir) {
        canDash = false;
        isBoosting = true;
        print("dash");
        while (boostDur > time) {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
            {
                dashJumping = true;
                velocity.y = dashJump.y;
                velocity.x = dashJump.x *boostDir;
                //  print(velocity);

            }
            if (!dashJumping || controller.colisions.collideAny())
            {
                time += Time.deltaTime;
            }
            if (controller.colisions.collideAny() && dashJumping) {
                dashJumping = false;
                time = boostDur + 1;
            }

            
            targetVelocityX = boostDir * boostSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXsmoothing, controller.colisions.below ? accelerationTimeGrounded : acclerationTimeAirborne);
           // velocity.x = targetVelocityX;
            velocity.y += gravity*.2f * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime,new Vector2(boostDir,0));
            yield return 0;
        };
        print(time);
        isBoosting = false;
        yield return new WaitForSeconds(boostCooldown); //Cooldown time for being able to boost again, if you'd like.
        canDash = true; //set back to true so that we can boost again.
    }
}
