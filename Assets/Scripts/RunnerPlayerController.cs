using UnityEngine;
using System.Collections;

public class RunnerPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 0f;
    public float laneDistance = 2.25f;
    public float horizontalSmooth = 10f;

    public float jumpForce = 6f;
    public float gravity = -22f;
    public float slideDuration = 0.68f;

    [Header("Reference")]
    public Animator animator;
    public CharacterController controller;

    private int desiredLane = 1;
    private float verticalVelocity = 0f;
    private bool isSliding = false;
    private bool isRunning = false;

    [Header("Swipe Controls")]
    private Vector2 startTouchPos;
    private Vector2 swipeDelta;
    public bool isSwiping = false;
    public float swipeTreshold = 50f;

    void OnEnable()
    {
        GameManager.OnGameStart += StartRunning;
        GameManager.OnGameOver += StopRunning;
    }

    void OnDisable()
    {
        GameManager.OnGameStart -= StartRunning;
        GameManager.OnGameOver -= StopRunning;
    }

    void StartRunning()
    {
        forwardSpeed = 8f;
        isRunning = true;
        if (animator != null)
        {
            animator.SetBool("gameStart", true);
        }
    }

    void StopRunning()
    {
        forwardSpeed = 0f;
        isRunning = false;
    }

    private void Update()
    {
        if (!isRunning) return;

        //Player Movement
        PlayerMove();
        PcControls();
        DetectSwipe();
    }

    public void PlayerMove()
    {
        //HorizontalMovement
        Vector3 move = Vector3.forward * forwardSpeed;
        float targetX = (desiredLane - 1) * laneDistance;
        float newX = Mathf.Lerp(transform.position.x, targetX, horizontalSmooth * Time.deltaTime);
        move.x = (newX - transform.position.x) / Time.deltaTime;

        //VerticalMovement
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1f;
            animator.SetBool("IsJumping", false);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

    public void PcControls()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            desiredLane = Mathf.Min(desiredLane + 1, 2);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            desiredLane = Mathf.Max(desiredLane - 1, 0);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Slide();
        }
#endif
    }

    public void Jump()
    {
        if (!controller.isGrounded) return;
        verticalVelocity = jumpForce;
        animator.SetBool("IsJumping", true);
    }

    public void Slide()
    {
        if (!isSliding && controller.isGrounded)
        {
            StartCoroutine(DoSlide());
        }
    }

    private IEnumerator DoSlide()
    {
        isSliding = true;
        animator.SetBool("IsSliding", true);

        float origH = controller.height;
        Vector3 origC = controller.center;

        controller.height = origH / 2f;
        controller.center = new Vector3(origC.x, origC.y / 2f, origC.z);

        yield return new WaitForSeconds(slideDuration);

        controller.height = origH;
        controller.center = origC;

        animator.SetBool("IsSliding", false);
        isSliding = false;
    }

    void DetectSwipe()
    {
        if (Input.touchCount <= 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            isSwiping = true;
            startTouchPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended)
        {
            swipeDelta = touch.position - startTouchPos;

            if (isSwiping && swipeDelta.magnitude > swipeTreshold)
            {
                float x = swipeDelta.x;
                float y = swipeDelta.y;

                //Means horizontal swipe
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    //Right Swipe
                    if (x > 0)
                    {
                        desiredLane = Mathf.Min(desiredLane + 1, 2);
                    }
                    //Left Swipe
                    else
                    {
                        desiredLane = Mathf.Max(desiredLane - 1, 0);
                    }
                }
                else
                {
                    if (y > 0)
                    {
                        Jump();
                    }
                    else
                    {
                        Slide();
                    }
                }
                isSwiping = false;
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Obstacle"))
        {
            // 1. Trigger the visual death animation
            if (animator != null)
            {
                animator.SetTrigger("Die");
                animator.SetBool("gameStart", false); // Ensures the state machine exits the running animation
            }

            // 2. Notify the GameManager (which will open the Game Over UI and fire events)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Die();
            }

            // Note: No "this.enabled = false" here. GameManager fires OnGameOver, 
            // which calls StopRunning() in this script to set forwardSpeed to zero safely.
        }
    }
}