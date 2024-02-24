using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float jumpStartingPush;
    [SerializeField] float jumpHeldTimerMax;
    [SerializeField] float gravity;
    [SerializeField] float fallSpeedMax;
    [SerializeField] float maxJumpPreload;
    [SerializeField] float coyoteTimerMax;
    [SerializeField] float apexTimerMax;
    [SerializeField] Collider2D bodyCollider;
    [SerializeField] BoxCollider2D footCollider;
    [SerializeField] float maxDashTimer;
    [SerializeField] AnimationCurve dashCooldown;
    [SerializeField] float maxDashCooldownTimer;
    [SerializeField] GameObject afterImage;

    Rigidbody2D rb;
    float inputX;
    float jumpHeldTimer;
    float coyoteTimer;
    float apexTimer;
    float dashMultiplier = 1f;
    float dashTimer;
    int afterImagesRemaining;
    float dashCooldownTimer;

    bool dropDown;

    bool onPlatform, jumping;
    bool jumpTriggered;
    bool jumpHeld;
    bool apexReached;
    bool enteredPlatformJumpNotResetYet;

    Collider2D passthroughObject;
    float lockDirection = 0f;

    float jumpPreload;

    Vector2 velocity;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        inputX = 0;
        velocity = Vector2.zero;
        jumpPreload = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (jumpPreload > 0)
        {
            jumpPreload -= Time.deltaTime;
        }

        if (coyoteTimer > 0)
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpTriggered)
        {
            if (!onPlatform)
            {
                if (coyoteTimer > 0)
                {
                    BeginJump();
                }
                else
                {
                    jumpPreload = maxJumpPreload;
                }
            }
            else
            {
                BeginJump();
            }

            jumpTriggered = false;
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        velocity.x = inputX * speed * Time.fixedDeltaTime * dashMultiplier;

        if (dashMultiplier > 1f)
        {
            float dashProgress = dashTimer / maxDashTimer;

            dashMultiplier = dashCooldown.Evaluate(dashProgress);

            dashTimer -= Time.fixedDeltaTime;

            if ((dashProgress < 0.25f && afterImagesRemaining == 1) || (dashProgress < 0.5f && afterImagesRemaining == 2) || (dashProgress < 0.75f && afterImagesRemaining == 3))
            {
                Instantiate(afterImage, transform.position, afterImage.transform.rotation);
                afterImagesRemaining--;
            }

            if (dashMultiplier <= 1f)
            {
                dashMultiplier = 1f;
            }
        }

        if (jumping && jumpHeldTimer < jumpHeldTimerMax)
        {
            if (jumpHeld)
            {
                jumpHeldTimer += Time.fixedDeltaTime;

                //velocity.y -= gravity * Time.fixedDeltaTime * 0.25f;
            }
            else
            {
                jumpHeldTimer = jumpHeldTimerMax;
            }
        }
        else if (apexReached && apexTimer > 0f)
        {
            apexTimer -= Time.fixedDeltaTime;

            velocity.y -= gravity * Time.fixedDeltaTime * 0.5f;

            if (apexTimer <= 0)
            {
                footCollider.enabled = true;
            }

        }
        else
        {
            velocity.y -= gravity * Time.fixedDeltaTime;

            if (!apexReached && jumping && velocity.y < 0f)
            {
                apexReached = true;
                apexTimer = apexTimerMax;
            }
        }

        if (velocity.y < fallSpeedMax)
            velocity.y = fallSpeedMax;

        rb.MovePosition(rb.position + velocity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform" || collision.gameObject.tag == "Ground")
        {
            //enteredPlatformJumpNotResetYet = true;

            foreach (ContactPoint2D point in collision.contacts)
            {
                if (Mathf.Abs(point.normal.y) > Mathf.Abs(point.normal.x))
                {
                    // If our collision is in a vertical direction
                    if (point.normal.y > 0)
                    {
                        ResetJump();

                        if (jumpPreload > 0)
                        {
                            jumpPreload = 0;
                            BeginJump();
                        }
                    }
                    else
                    {
                        bodyCollider.isTrigger = true;
                        passthroughObject = collision.collider;
                    }
                }
                else
                {
                    
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            CheckDirectionLock(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            CheckDirectionLock(collision);
        }
    }

    void CheckDirectionLock(Collider2D collision)
    {
        //enteredPlatformJumpNotResetYet = true;

        Vector2 flatPos = new Vector2(transform.position.x, transform.position.y);

        Vector3 normal = collision.ClosestPoint(transform.position) - flatPos;

        if (Mathf.Abs(normal.y) < Mathf.Abs(normal.x))
        {
            //Debug.Log("HORIZONTAL: " + collision.name + " - " + normal);

            inputX = 0;

            if (normal.x > 0)
            {
                lockDirection = 1f;

                //Vector3 newPos = transform.position;
                //newPos.x -= normal.magnitude;
                //transform.position = newPos;
            }
            else
            {
                lockDirection = -1f;

                //Vector3 newPos = transform.position;
                //newPos.x += normal.magnitude;
                //transform.position = newPos;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform" || collision.gameObject.tag == "Ground")
        {
            foreach (ContactPoint2D point in collision.contacts)
            {
                if (Mathf.Abs(point.normal.y) > Mathf.Abs(point.normal.x))
                {
                    // If our collision is in a vertical direction
                    if (point.normal.y > 0)
                    {
                        // above
                        onPlatform = true;
                        if (velocity.y < 0)
                        {
                            velocity.y = 0;
                        }

                        if (collision.gameObject.tag == "Platform" && dropDown)
                        {
                            bodyCollider.isTrigger = true;
                            footCollider.enabled = false;
                            passthroughObject = collision.collider;
                        }

                        //if (enteredPlatformJumpNotResetYet)
                        //{
                        //    ResetJump();

                        //    if (jumpPreload > 0)
                        //    {
                        //        jumpPreload = 0;
                        //        BeginJump();
                        //    }
                        //}
                    }
                    else
                    {
                        // below
                        if (velocity.y > 0)
                            velocity.y = 0;
                    }
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform" || collision.gameObject.tag == "Ground")
        {
            if (onPlatform)
            {
                onPlatform = false;
                coyoteTimer = coyoteTimerMax;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == passthroughObject)
        {
            lockDirection = 0f;

            bodyCollider.isTrigger = false;
            passthroughObject = null;
            footCollider.enabled = true;
        }
        else if (collision.gameObject.tag == "Platform")
        {
            Vector2 flatPos = new Vector2(transform.position.x, transform.position.y);

            Vector3 normal = collision.ClosestPoint(transform.position) - flatPos;

            if (Mathf.Abs(normal.y) < Mathf.Abs(normal.x))
            {
                lockDirection = 0f;
            }
        }
    }

    void BeginJump()
    {
        velocity.y = jumpStartingPush;
        jumpHeldTimer = 0f;
        jumping = true;

        footCollider.enabled = false;
    }

    void ResetJump()
    {
        jumping = false;
        apexReached = false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 input = context.ReadValue<Vector2>();

            if (input.x * lockDirection <= 0)
            {
                inputX = input.x;
            }

            if (input.y < -0.5f && !dropDown)
                dropDown = true;
            else if (input.y > -0.5f && dropDown)
                dropDown = false;
        }
        else if (context.canceled)
        {
            inputX = 0f;
            dropDown = false;
        }

        if (inputX > 0.95f)
            inputX = 1f;
        else if (inputX < -0.95f)
            inputX = -1f;
        else if (inputX > 0 && inputX < 0.05f)
            inputX = 0f;
        else if (inputX < 0 && inputX > -0.05f)
            inputX = 0f;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpTriggered = true;
            jumpHeld = true;
        }
        else if (context.performed)
        {
            jumpHeld = true;
        }
        else
        {
            jumpHeld = false;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && dashCooldownTimer <= 0)
        {
            dashMultiplier = 4f;
            dashTimer = maxDashTimer;
            afterImagesRemaining = 3;

            dashCooldownTimer = maxDashCooldownTimer;

            Instantiate(afterImage, transform.position, afterImage.transform.rotation);
        }
    }
}
