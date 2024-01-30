using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerMovement2 : MonoBehaviour
{
    [SerializeField] float jumpSpeed = 4.2f;
    [SerializeField] float umbrellaSpeed = 2f;
    [SerializeField] float rocketSpeed = 2f;
    float freezeplayer = 0f;
    public static float topSpeed;
    int health = 1;
    bool canDouble;
    private int dashCounter;
    public bool didJump;
    public bool didJump2;
    public bool didJump3;
    public bool isAlive = true;
    public ParticleSystem dust;
    public ParticleSystem speedLines;
    public ParticleSystem dashPS;
    public static float deathCounter = 0f;
    [SerializeField] StarManager starManager;

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] GameObject followPoint;
    [SerializeField] TextMeshProUGUI deathCounterText;
    [SerializeField] TravelCalculator travelCalculator;
    [SerializeField] Camera2DFollow camera2DFollow;
    public PlayerInput playerInput;
    public InputAction touchPositionAction;
    public InputAction touchPressAction;

    private float screenWidth;
    private float coyoteTime = 0.05f;
    private float coyotoTimeCounter;

    private float acceleration = 9f;
    private float decceleration = 9f;
    private float velPower = 1.2f;
    //private bool didDouble;

    [SerializeField] Animator animator;
    public Animator crossFade;
    public Animator counter;
    public Rigidbody2D player;
    public BoxCollider2D playercoll;
    public CircleCollider2D playerCcoll;

    public enum JumpState
    {
        GROUNDED, JUMPING, ROCKET_JUMPING, UMBRELLA_JUMPING, DASHING, DASHINGOUT
    }

    public JumpState jumpState;

    // Betterfall stuff
    public float fallMultiplier;
    public float fallUmbrella;
    public float fallRocket;
    public float fallDash;
    private float fallTimer = 0.2f;
    private bool rocketSlow;
    private bool umbrellaSlow;
    private bool dashSlow;
    private bool isFallingAfterDash;
    private RigidbodyConstraints2D originalConstraints;

    //private float intensity = 0.1f;


    // Use this for initialization
    void Start()
    {
        animator.Play("PigsterRun");
        crossFade.Play("Crossfade_End");
        StartCoroutine(Displaycounter());
        dashCounter = 0;
    }

    IEnumerator Displaycounter()
    {
        yield return new WaitForSeconds(0.5f);
        counter.Play("CounterPopin");
    }
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
        float speedDif = topSpeed - player.velocity.x;

        float accelRate = (Mathf.Abs(topSpeed) > 0.01f) ? acceleration : decceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        player.AddForce(movement * Vector2.right);


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

        if (player.velocity.y < 0)
        {
            //animator.SetTrigger("Fall");
        }

        if (player.velocity.x == 0f && player.velocity.y == 0f)
        {
            if (health == 1)
            {
                animator.Play("PigsterIdle");
            }
        }
        else
        {
            animator.SetTrigger("NotIdling");
        }

        if (didJump & coyotoTimeCounter > 0)
        {
            jumpState = JumpState.JUMPING;
            didJump = false;
            canDouble = true;
        }
        if (didJump2 & coyotoTimeCounter > 0)
        {
            jumpState = JumpState.JUMPING;
            didJump2 = false;
            canDouble = true;
        }
        if (didJump2 & canDouble & !IsGrounded())
        {
            jumpState = JumpState.UMBRELLA_JUMPING;
            animator.SetTrigger("Umbrella");
            canDouble = false;
            didJump2 = false;
        }
        if (didJump & canDouble & !IsGrounded())
        {
            jumpState = JumpState.ROCKET_JUMPING;
            animator.SetTrigger("Rocketing");
            canDouble = false;
            didJump = false;
        }
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

        //checks if the pig is supposed to hover or not
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
        

           
        //slow fall if umbrella
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

        //slow if rocket
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

        //slow if dash
        if (isFallingAfterDash)
        {
            player.velocity += Vector2.up * Physics2D.gravity.y * (fallDash - 1) * Time.deltaTime;
        }
        #endregion
        if (health <= 0)
        {
            topSpeed = 0f;
            //Setting player Alive to Dead to execute scripts only once
            if (isAlive)
            {
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
        if (coll.gameObject.CompareTag("FallTrigger"))
        {
            health--;
            topSpeed = 0f;
        }
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
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(playercoll.bounds.center, playercoll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }



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