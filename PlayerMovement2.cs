using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerMovement2 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float jumpSpeed = 4.2f;
    [SerializeField] float umbrellaSpeed = 2f;
    [SerializeField] float rocketSpeed = 2f;
    [SerializeField] float acceleration = 9f;
    [SerializeField] float deceleration = 9f;
    [SerializeField] float velPower = 1.2f;
    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] Animator animator;
    [SerializeField] GameObject followPoint;
    [SerializeField] TextMeshProUGUI deathCounterText;
    [SerializeField] TravelCalculator travelCalculator;
    [SerializeField] Camera2DFollow camera2DFollow;
    [SerializeField] StarManager starManager;

    public static float topSpeed;
    public static float deathCounter = 0f;

    private float freezePlayer = 0f;
    private float screenWidth;
    private float coyoteTime = 0.05f;
    private float coyoteTimeCounter;

    // Better fall settings
    public float fallMultiplier;
    public float fallUmbrella;
    public float fallRocket;
    public float fallDash;
    private float fallTimer = 0.2f;

    // Player state
    private int health = 1;
    private int dashCounter;
    private bool canDoubleJump;
    private bool rocketSlow;
    private bool umbrellaSlow;
    private bool dashSlow;
    private bool isFallingAfterDash;
    private bool isAlive = true;
    private RigidbodyConstraints2D originalConstraints;

    // Input
    public PlayerInput playerInput;
    public InputAction touchPositionAction;
    public InputAction touchPressAction;

    // Player Components
    public Animator crossFade;
    public Animator counter;
    public Rigidbody2D player;
    public BoxCollider2D playerColl;
    public CircleCollider2D playerCColl;
    public ParticleSystem dust;
    public ParticleSystem speedLines;
    public ParticleSystem dashPS;

    // Jump States
    public enum JumpState { GROUNDED, JUMPING, ROCKET_JUMPING, UMBRELLA_JUMPING, DASHING, DASHINGOUT }
    public JumpState jumpState;

    private bool didJump;
    private bool didJump2;
    private bool didJump3;

    //Start the game
    void Start()
    {
        animator.Play("PigsterRun");
        crossFade.Play("Crossfade_End");
        StartCoroutine(Displaycounter());
        dashCounter = 0;
    }

    //Display how many times the level has been attempted
    IEnumerator Displaycounter()
    {
        yield return new WaitForSeconds(0.5f);
        counter.Play("CounterPopin");
    }
    //Cache screen size for better performance
    public void Awake()
    {
        originalConstraints = player.constraints;
        touchPressAction = playerInput.actions["Jump"];
        touchPositionAction = playerInput.actions["TouchPosition"];

        screenWidth = Screen.width / 2;
        if (deathCounter != 0)
        {
            deathCounterText.text = deathCounter.ToString();
        }
    }

    //Set booleans to true/false on inputs (Spaghetti)
    private void OnJump(InputValue value)
    {
        didJump = true;
    }
    private void OnJump2(InputValue value)
    {
        didJump2 = true;
    }
    private void OnJump3(InputValue value)
    {
        didJump3 = true;
    }


    private void FixedUpdate()
    {
        //player.velocity = new Vector2(topSpeed, player.velocity.y);

        //Calculate how much the player should be accelerating/deaccelerating depending on current jump
        float speedDif = topSpeed - player.velocity.x;

        float accelRate = (Mathf.Abs(topSpeed) > 0.01f) ? acceleration : decceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        player.AddForce(movement * Vector2.right);

        //Handle all necessary changes to physics and animations on different states
        switch (jumpState)
        {
            case JumpState.GROUNDED:
                if (IsGrounded())
                {
                    dashCounter = 0;
                    coyotoTimeCounter = coyoteTime;
                    animator.SetBool("IsGrounded", true);
                    topSpeed = 1.8f;
                    isFallingAfterDash = false;
                }
                else
                {
                    animator.SetBool("IsGrounded", false);
                    coyotoTimeCounter -= Time.deltaTime;
                }
                break;
            case JumpState.JUMPING:
                animator.Play("PigsterJump");
                JumpThePlayer();
                topSpeed = 1.8f;
                jumpState = JumpState.GROUNDED;
                break;
            case JumpState.ROCKET_JUMPING:
                player.constraints = originalConstraints;
                animator.Play("PigsterRocket");
                JumpThePlayerRocket();
                topSpeed = 2.2f;
                jumpState = JumpState.GROUNDED;
                isFallingAfterDash = false;
                break;
            case JumpState.UMBRELLA_JUMPING:
                player.constraints = originalConstraints;
                animator.Play("PigsterUmbrella");
                JumpThePlayerUmbrella();
                topSpeed = 1.5f;
                StartCoroutine(Umbrellajumptest());
                jumpState = JumpState.GROUNDED;
                isFallingAfterDash = false;
                break;
            case JumpState.DASHING:
                camera2DFollow.OnDash();
                player.constraints = originalConstraints;
                player.constraints = RigidbodyConstraints2D.FreezePositionY;
                animator.Play("PigsterDash");
                topSpeed = 3.5f;
                jumpState = JumpState.GROUNDED;
                dashCounter++;
                StartCoroutine(StopDashing());
                //CreateDashEffect();
                didJump = false;
                didJump2 = false;
                break;
            case JumpState.DASHINGOUT:
                player.constraints = originalConstraints;
                didJump = false;
                didJump2 = false;
                isFallingAfterDash = true;
                topSpeed = 1.8f;
                animator.SetTrigger("FallingOutOfDash");
                jumpState = JumpState.GROUNDED;
                break;
        }
        //Placehodler for future implementation
        if (player.velocity.y < 0)
        {
            //animator.SetTrigger("Fall");
        }

        //if the player is alive and still play an idle animation
        if (player.velocity.x == 0f && player.velocity.y == 0f)
        {
            if (health == 1)
            {
                animator.Play("PigsterIdle");
            }
        }
        //Else set the next animation through trigger
        else
        {
            animator.SetTrigger("NotIdling");
        }

        //If jumped and is grounded perform basic jump and enable the possibility to double jump
        if (didJump & coyotoTimeCounter > 0)
        {
            jumpState = JumpState.JUMPING;
            didJump = false;
            canDouble = true;
        }
        //If jumped through the left side of the screen do same as before
        if (didJump2 & coyotoTimeCounter > 0)
        {
            jumpState = JumpState.JUMPING;
            didJump2 = false;
            canDouble = true;
        }
        //If tapped left side of screen during jump play the UmbrellaJump and disable the ability to jump again
        if (didJump2 & canDouble & !IsGrounded())
        {
            jumpState = JumpState.UMBRELLA_JUMPING;
            animator.SetTrigger("Umbrella");
            canDouble = false;
            didJump2 = false;
        }
        //If tapped right side of the screen during jump play the RocketJump and disable the ability to jump again
        if (didJump & canDouble & !IsGrounded())
        {
            jumpState = JumpState.ROCKET_JUMPING;
            animator.SetTrigger("Rocketing");
            canDouble = false;
            didJump = false;
        }
        //If did Dash play the dash
        if (didJump3 & !IsGrounded() & dashCounter < 1)
        {
            jumpState = JumpState.DASHING;
            didJump3 = false;
        }


        //To enable jump que swap these
        /*if (!didDouble)
        {
            didJump = false;
            didJump2 = false;
        }*/
        didJump3 = false;

        /*if (player.velocity.y > -0.5f)
        {
            didJump = false;
            didJump2 = false;
        }*/


        #region adjust slow fall for umbrella+rocket

        //During Umbrella and RocketJump the player should fall down to the ground slower
        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("PigsterUmbrella") && !IsGrounded())
        {
            umbrellaSlow = true;
        }
        else
        {
            umbrellaSlow = false;
        }

        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("PigsterRocket") && !IsGrounded())
        {
            rocketSlow = true;
        }
        else
        {
            rocketSlow = false;
        }



        //Set the fallspeed lower during UmbrellaJump and freeze it during Dash
        if (umbrellaSlow)
        {
            player.velocity += Vector2.up * Physics2D.gravity.y * (fallUmbrella - 1) * Time.deltaTime;
        }
        else if (player.velocity.y < 0 && !umbrellaSlow)
        {
            if (!isFallingAfterDash)
            {
                player.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                fallTimer -= Time.deltaTime;
            }
        }

        //Set the fallspeed lower during RocketJump and freeze it during Dash
        if (rocketSlow)
        {
            player.velocity += Vector2.up * Physics2D.gravity.y * (fallRocket - 1) * Time.deltaTime;
        }
        else if (player.velocity.y < 0 && !rocketSlow)
        {
            if (!isFallingAfterDash)
            {
                player.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                fallTimer -= Time.deltaTime;
            }
        }

        //Freeze if dashing
        if (isFallingAfterDash)
        {
            player.velocity += Vector2.up * Physics2D.gravity.y * (fallDash - 1) * Time.deltaTime;
        }
        #endregion

        //Check if the player is alive, Freeze him if he dies
        if (health <= 0)
        {
            topSpeed = 0f;
            //Setting player Alive to Dead to execute scripts only once
            if (isAlive)
            {
                //Do all death related stuff and start the game again
                player.constraints = RigidbodyConstraints2D.FreezeAll;
                UnSubscribe();
                Handheld.Vibrate();
                deathCounter++;
                isAlive = false;
                travelCalculator.CalculateDistance();
            }
            StartCoroutine(StartDeathStuff());
            animator.Play("PigsterDeath");
        }
    }

    #region PlayerJumps
    //Set the UmbrellaJump to slow the player down only after .3 seconds to make the effect align with the actual opening of the umbrella
    IEnumerator Umbrellajumptest()
    {
        yield return new WaitForSeconds(0.3f);
        topSpeed = 0.6f;
    }
    IEnumerator StartDeathStuff()
    {
        yield return new WaitForSeconds(1f);
        crossFade.Play("Crossfade_Start");
        StartCoroutine(StartGame());
    }

    private void JumpThePlayer()
    {
        player.velocity = new Vector2(player.velocity.x, freezeplayer);
        player.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        coyotoTimeCounter = 0f;
        CreateDust();
    }
    private void JumpThePlayerUmbrella()
    {
        player.velocity = new Vector2(player.velocity.x, freezeplayer);
        player.AddForce(Vector2.up * umbrellaSpeed, ForceMode2D.Impulse);
        //didDouble = true;
        camera2DFollow.OnUmbrella();
    }
    private void JumpThePlayerRocket()
    {
        player.velocity = new Vector2(player.velocity.x, freezeplayer);
        player.AddForce(Vector2.up * rocketSpeed, ForceMode2D.Impulse);
        CreateSpeedLines();
        //didDouble = true;
        camera2DFollow.OnJump();
    }
    #endregion


    //##################### CHECK IF THE PLAYER HIT SOMETHING ##################
    void OnTriggerEnter2D(Collider2D coll)
    {
        //Hit an obstacle and died
        if (coll.gameObject.CompareTag("FallTrigger"))
        {
            health--;
            topSpeed = 0f;
        }
        //Level Finished
        if (coll.gameObject.CompareTag("GoalTrigger"))
        {
            PassTheLevel();
            StartCoroutine(ShowLvlCleared());
            StartCoroutine(LvlClearAnimationPlayer());
            starManager.WhenLvlPasses();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            health--;
            topSpeed = 0f;
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            CreateDust();
        }
    }
    //###################### CHECK IF PLAYER IS GOUNDED ###################

    //Check grounding through raycast to make the gamefeel better
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(playercoll.bounds.center, playercoll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }


    //End dash after .25 seconds
    IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(0.25F);
        jumpState = JumpState.DASHINGOUT;
    }


    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        isAlive = true;
    }
    private void BeginGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        isAlive = true;
    }
    public void CreateDust()
    {
        dust.Play();
    }
    public void CreateSpeedLines()
    {
        speedLines.Play();
    }
    public void CreateDashEffect()
    {
        dashPS.Play();
    }


    #region scene transitions
    IEnumerator ShowLvlCleared()
    {
        deathCounter = 0;
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(9);
        crossFade.Play("Crossfade_End");
    }
    IEnumerator LvlClearAnimationPlayer()
    {
        yield return new WaitForSeconds(1f);
        crossFade.Play("Crossfade_Start");
    }

    public void PassTheLevel()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;


        if (currentLevel >= PlayerPrefs.GetInt("levelsUnlocked"))
        {
            if (currentLevel != 8)
            {
                PlayerPrefs.SetInt("levelsUnlocked", currentLevel + 1);
            }
        }
    }
    #endregion
}

