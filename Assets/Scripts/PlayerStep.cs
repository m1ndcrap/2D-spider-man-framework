using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static Cinemachine.CinemachineTargetGroup;
using static RobotStep;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class PlayerStep : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] public SpriteRenderer sprite;
    [SerializeField] private Transform visual;
    private BoxCollider2D coll;
    private float dirX = 0f;
    private float dirY = 0f;

    // Swinging Variables
    private float ropeAngle = 0f;
    private float ropeAngleVelocity = 0f;
    private float ropeX = 0f;
    private float ropeY = 0f;
    private float grappleX = 0f;
    private float grappleY = 0f;
    private float ropeLength = 0f;
    private bool swingEnd = false;
    [SerializeField] private float accelerationRate = -0.02f;

    // Crawling Variables
    [SerializeField] private bool groundDetected;
    [SerializeField] private bool wallDetected;
    [SerializeField] private Transform groundPositionChecker;
    [SerializeField] private Transform wallPositionChecker;
    [SerializeField] private Transform ceilingPositionChecker;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float ceilingCheckDistance;
    [SerializeField] private bool hasTurn;
    private float ZaxisAdd;
    [SerializeField] private int direction;
    private bool isTurning = false;
    private float crawlDir = 0f;
    private bool shoot = false;

    // Zip Variables
    [SerializeField] private Transform quickZipTarget;
    [SerializeField] private Tilemap tilemap; // Assign in inspector
    private Vector2? moveTarget = null;

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private float hsp = 4f; // Horizontal speed
    [SerializeField] private float jspd = 5f;    // Jump speed
    [SerializeField] public GameObject ropeSegmentPrefab; // Assign in Inspector
    private float ropeSegmentLength = 0.15f; // Distance between segments
    private List<GameObject> ropeSegments = new List<GameObject>(); // Track segments
    private Queue<GameObject> ropeSegmentPool = new Queue<GameObject>();
    private int maxPoolSize = 200; // Optional limit

    private enum MovementState { idle, running, jumping, falling, swinging, endswing, crawling, zip, groundshoot, airshoot, crawlshoot, punch1, punch2, punch3, punch4, airkick, airpunch, kick1, kick2, uppercut, launched, hurt1, hurt2, block1, block2, block3, block4 }
    public enum PlayerState { normal, swing, crawl, quickzip, dashenemy, hurt }
    public PlayerState pState;

    // Sound Files
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip sndJump;
    [SerializeField] private AudioClip sndJump2;
    [SerializeField] private AudioClip sndSwing;
    [SerializeField] private AudioClip sndSwing2;
    [SerializeField] private AudioClip sndSwing3;
    [SerializeField] private AudioClip sndLand;
    [SerializeField] private AudioClip sndLand2;
    [SerializeField] private AudioClip sndHardLand;
    [SerializeField] private AudioClip sndHardLand2;
    [SerializeField] private AudioClip sndWebSnap;
    [SerializeField] private AudioClip sndWebRelease;
    [SerializeField] private AudioClip sndWebTension;
    [SerializeField] private AudioClip sndWebTension2;
    [SerializeField] private AudioClip sndWebTension3;
    [SerializeField] private AudioClip sndStep;
    [SerializeField] private AudioClip sndStep2;
    [SerializeField] private AudioClip sndCrawlStep;
    [SerializeField] private AudioClip sndCrawlStep2;
    [SerializeField] private AudioClip sndAttack;
    [SerializeField] private AudioClip sndAttack2;
    [SerializeField] private AudioClip sndAttack3;
    [SerializeField] private AudioClip sndSwipe;
    [SerializeField] private AudioClip sndSwipe2;
    [SerializeField] private AudioClip sndSwipe3;
    [SerializeField] public AudioClip sndQuickHit;
    [SerializeField] public AudioClip sndQuickHit2;
    [SerializeField] public AudioClip sndStrongHit;
    [SerializeField] public AudioClip sndStrongHit2;
    [SerializeField] private AudioClip sndHurt;
    [SerializeField] private AudioClip sndHurt2;
    [SerializeField] private AudioClip sndHurt3;
    private bool wasGrounded = false;

    // Alarms
    private int alarm1 = 0;

    // combat
    public RobotStep currentTarget = null;
    public RobotStep currentCounter = null;
    public bool isEnemyAttacking = false;
    [SerializeField] private LayerMask enemyMask;
    private float dash_spd = 0f;
    public UnityEvent<RobotStep> OnHit;
    private bool waitingToHit = false;
    [SerializeField] private GameObject hitParticlePrefab; // Assign prefab in inspector
    [SerializeField] private GameObject hurtParticlePrefab; // Assign prefab in inspector
    public bool uppercut = false;
    public Vector3 enemyHitSpawn = new Vector3(0f, 0f, 0f);
    public bool attacking = false; // boolean for if player is actually going to attack or only needs to play attack anim with no consequence
    public bool countering = false;
    [SerializeField] private bool pastHitEvent = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        pState = PlayerState.normal;
        direction = 1;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // Update is called once per frame
    void Update()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (swingEnd && stateInfo.IsName("Player_Swing_End") && stateInfo.normalizedTime >= 1f)
            swingEnd = false;

        if (dirX > 0)
            wallPositionChecker.localPosition = new Vector2(0.325f, -0.389f);
        else
            wallPositionChecker.localPosition = new Vector2(-0.325f, -0.389f);

        Vector2? bestCorner = FindClosestTileTopCorner(transform.position);

        if (bestCorner.HasValue)
        {
            quickZipTarget.position = bestCorner.Value;
            quickZipTarget.gameObject.SetActive(true);
        }
        else
        {
            quickZipTarget.gameObject.SetActive(false);
        }

        if (alarm1 > 0)
        {
            alarm1 -= 1;
        }
        else
        {
            if (pState == PlayerState.swing)
            {
                AudioClip[] clips = { sndWebTension, sndWebTension2, sndWebTension3 };
                AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
                audioSrc.PlayOneShot(randomClip);
                alarm1 = 400;
            }
        }

        switch (pState)
        {
            case PlayerState.normal:
            {
                visual.rotation = Quaternion.Euler(0, 0, 0);

                dirX = Input.GetAxisRaw("Horizontal");   // If left arrow key is pressed, returns -1. If right arrow key is pressed, returns +1. Using GetAxisRaw instead of GetAxis to make player not feel like they are on ice.
                dirY = -Input.GetAxisRaw("Vertical"); //key up returns -1, key down returns +1
                rb.velocity = new Vector2(dirX * hsp, rb.velocity.y);    // Moving character based on left or right arrow key

                if (Input.GetKeyDown("space") && Grounded() && !shoot)      // Jump code
                {
                    AudioClip[] clips = { sndJump, sndJump2 };
                    int index = UnityEngine.Random.Range(0, clips.Length + 1); // +1 to include "no sound"

                    if (index < clips.Length)
                        audioSrc.PlayOneShot(clips[index]);

                    rb.velocity = new Vector2(rb.velocity.x, jspd);
                }

                if (Input.GetKeyDown("space") && !Grounded() && !shoot)      // Swing code
                {
                    Vector2 playerPos = transform.position;
                    Vector2 inputDir = new Vector2(dirX * 2.5f, -dirY * 1.25f);
                    Vector2 searchOrigin = (Vector2)playerPos + inputDir;

                    Debug.DrawLine(transform.position, searchOrigin, UnityEngine.Color.cyan);

                    Collider2D[] hits = Physics2D.OverlapCircleAll(searchOrigin, 3f, jumpableGround);
                    float closestDistance = float.MaxValue;
                    Vector2 bestAttachPoint = Vector2.zero;
                    bool found = false;

                    foreach (Collider2D hit in hits)
                    {
                        Vector2 point = hit.ClosestPoint(searchOrigin); // get closest contact point
                        Vector2 directionToPoint = (point - playerPos).normalized;

                        if (point.y <= playerPos.y) continue; // skip if below player
                        if (dirX > 0 && point.x <= playerPos.x) continue; // player facing right, point must be ahead
                        if (dirX < 0 && point.x >= playerPos.x) continue; // player facing left, point must be behind

                        //Debug.DrawLine(playerPos, point, Color.green);

                        float dist = Vector2.Distance(playerPos, point);

                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            bestAttachPoint = point;
                            found = true;
                        }
                    }

                    if (found)
                    {
                        rb.gravityScale = 0;
                        grappleX = bestAttachPoint.x;
                        grappleY = bestAttachPoint.y;
                        ropeX = transform.position.x;
                        ropeY = transform.position.y;
                        ropeAngleVelocity = 0f;
                        ropeAngle = Mathf.Atan2(ropeY - grappleY, ropeX - grappleX) * Mathf.Rad2Deg;
                        ropeLength = Vector2.Distance(new Vector2(grappleX, grappleY), new Vector2(ropeX, ropeY));
                        coll.size = new Vector2(1.339648f, 1.561783f);
                        coll.offset = new Vector2(-0.6135812f, -0.6907219f);
                        AudioClip[] clips = { sndSwing, sndSwing2, sndSwing3 };
                        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
                        audioSrc.PlayOneShot(randomClip);
                        alarm1 = 400;
                        swingEnd = false;
                        pState = PlayerState.swing;
                    }
                }

                if (CanStartCrawling())
                {
                    pState = PlayerState.crawl;
                    rb.gravityScale = 0;
                }

                if (Input.GetKeyDown(KeyCode.I))    // Quick Zip Code
                {
                    Vector2 playerPos = transform.position;

                    if (bestCorner.HasValue)
                    {
                        moveTarget = bestCorner.Value; // <- trigger movement
                        coll.size = new Vector2(0.7719507f, 1.863027f);
                        coll.offset = new Vector2(-0.3766563f, -0.968719f);
                        AudioClip[] clips = { sndSwing, sndSwing2, sndSwing3 };
                        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
                        audioSrc.PlayOneShot(randomClip);
                        pState = PlayerState.quickzip;
                        rb.gravityScale = 0;
                    }
                    else
                    {
                        moveTarget = null;
                    }
                }

                if (Input.GetKey(KeyCode.U))    // Zip Code
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    shoot = true;

                    if (Grounded())
                    {
                        if ((dirX > 0 || dirX < 0) && dirY >= 0)
                            anim.Play("Player_Ground_Shoot", 0, 0.33f);
                        else if ((dirX > 0 || dirX < 0) && dirY < 0)
                            anim.Play("Player_Ground_Shoot", 0, 0.66f);
                        else if (dirX == 0 && dirY < 0)
                            anim.Play("Player_Ground_Shoot", 0, 0.99f);
                    }
                    else
                    {
                        if ((dirX > 0 || dirX < 0) && dirY >= 0)
                            anim.Play("Player_Air_Shoot", 0, 0.33f);
                        else if ((dirX > 0 || dirX < 0) && dirY < 0)
                            anim.Play("Player_Air_Shoot", 0, 0.66f);
                        else if (dirX == 0 && dirY < 0)
                            anim.Play("Player_Air_Shoot", 0, 0.99f);
                    }

                    Vector2 playerPos = transform.position;
                    Vector2 inputDir = new Vector2(dirX * 4.5f, -dirY * 1.25f);
                    Vector2 searchOrigin = (Vector2)playerPos + inputDir;

                    Debug.DrawLine(transform.position, searchOrigin, UnityEngine.Color.cyan);

                    Collider2D[] hits = Physics2D.OverlapCircleAll(searchOrigin, 3.72f, jumpableGround);
                    float closestDistance = float.MaxValue;
                    Vector2 bestAttachPoint = Vector2.zero;
                    bool found = false;

                    // Allowable angle threshold from the input direction (in degrees)
                    float angleThreshold = 45f;

                    foreach (Collider2D hit in hits)
                    {
                        Vector2 point = hit.ClosestPoint(searchOrigin); // get closest contact point
                        Vector2 directionToPoint = (point - playerPos).normalized;

                        // Check direction
                        float angle = Vector2.Angle(inputDir, directionToPoint);
                        if (angle > angleThreshold) continue; // skip if point is not in desired direction
                        if (point.y <= playerPos.y) continue; // skip if below player
                        if (dirX > 0 && point.x <= playerPos.x) continue; // player facing right, point must be ahead
                        if (dirX < 0 && point.x >= playerPos.x) continue; // player facing left, point must be behind

                        float dist = Vector2.Distance(playerPos, point);

                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            bestAttachPoint = point;
                            found = true;
                        }
                    }

                    if (found && Input.GetKeyDown("space"))
                    {
                        rb.gravityScale = 0;
                        moveTarget = bestAttachPoint;
                        coll.size = new Vector2(0.7719507f, 1.863027f);
                        coll.offset = new Vector2(-0.3766563f, -0.968719f);
                        transform.position = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
                        AudioClip[] clips = { sndSwing, sndSwing2, sndSwing3 };
                        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
                        audioSrc.PlayOneShot(randomClip);
                        pState = PlayerState.quickzip;
                    }
                }
                else
                {
                    shoot = false;
                }

                if (!wasGrounded && Grounded() && pState == PlayerState.normal) // Landing Sound Code
                {
                    float fallSpeed = rb.velocity.y;

                    if (fallSpeed < -10f)
                    {
                        AudioClip[] clips = { sndHardLand, sndHardLand2 };
                        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
                        audioSrc.PlayOneShot(randomClip);
                    }
                    else
                    {
                        AudioClip[] clips = { sndLand, sndLand2 };
                        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
                        audioSrc.PlayOneShot(randomClip);
                    }
                }

                wasGrounded = Grounded();

                // combat
                bool facingLeft = sprite.flipX;
                Vector2 origin = transform.position;

                // Get all enemies in a radius
                Collider2D[] ehits = Physics2D.OverlapCircleAll(origin, 5.2f, enemyMask);
                Collider2D[] ehitsC = Physics2D.OverlapCircleAll(origin, 5.2f, enemyMask);

                float closestEDistance = Mathf.Infinity;
                float closestEDistanceC = Mathf.Infinity;
                RobotStep closestEnemy = null;
                RobotStep closestCounter = null;

                foreach (var ehit in ehits)
                {
                    RobotStep enemy = ehit.GetComponent<RobotStep>();
                    if (enemy == null || enemy.eState == RobotStep.EnemyState.death)
                        continue;

                    // Linecast to check if anything blocks the path
                    RaycastHit2D hit = Physics2D.Linecast(transform.position, enemy.transform.position, jumpableGround);

                    if (hit.collider != null)
                        if ((Vector2)hit.point != (Vector2)enemy.transform.position) continue;

                    float dx = enemy.transform.position.x - origin.x;

                    // Check if in front based on flipX
                    if ((facingLeft && dx > 0) || (!facingLeft && dx < 0))
                        continue;

                    float dist = Mathf.Abs(dx);
                    if (dist < closestEDistance)
                    {
                        closestEDistance = dist;
                        closestEnemy = enemy;
                    }
                }

                foreach (var ehitC in ehitsC)
                {
                    RobotStep enemyC = ehitC.GetComponent<RobotStep>();
                    if (enemyC == null || enemyC.eState == RobotStep.EnemyState.death)
                        continue;

                    if (enemyC.eState != RobotStep.EnemyState.attack)
                        continue;

                    // Linecast to check if anything blocks the path
                    RaycastHit2D hitC = Physics2D.Linecast(transform.position, enemyC.transform.position, jumpableGround);

                    if (hitC.collider != null)
                        if ((Vector2)hitC.point != (Vector2)enemyC.transform.position) continue;

                    float dxC = enemyC.transform.position.x - origin.x;

                    float distC = Mathf.Abs(dxC);
                    if (distC < closestEDistanceC)
                    {
                        closestEDistanceC = distC;
                        closestCounter = enemyC;
                    }
                }

                currentTarget = closestEnemy;
                currentCounter = closestCounter;

                if (Input.GetKey(KeyCode.O) && currentTarget != null)   // normal attack
                {
                    if (Math.Abs(currentTarget.transform.position.x - transform.position.x) > 3.75f && Math.Abs(currentTarget.transform.position.x - transform.position.x) <= 5f) {dash_spd = 16f;}
                    if (Math.Abs(currentTarget.transform.position.x - transform.position.x) > 2.5f && Math.Abs(currentTarget.transform.position.x - transform.position.x) <= 3.75f) {dash_spd = 12;}
                    if (Math.Abs(currentTarget.transform.position.x - transform.position.x) > 1.25f && Math.Abs(currentTarget.transform.position.x - transform.position.x) <= 2.5f) {dash_spd = 8f;}
                    if (Math.Abs(currentTarget.transform.position.x - transform.position.x) >= 0f && Math.Abs(currentTarget.transform.position.x - transform.position.x) <= 1.25f) {dash_spd = 4f;}
                    attacking = true;

                    if (pState != PlayerState.dashenemy)
                    {
                        AudioClip[] clips = { sndAttack, sndAttack2, sndAttack3 };
                        int index = UnityEngine.Random.Range(0, clips.Length + 1); // +1 to include "no sound"
                        if (index < clips.Length) {audioSrc.PlayOneShot(clips[index]);}
                        
                        AudioClip[] clips2 = { sndSwipe, sndSwipe2, sndSwipe3 };
                        int index2 = UnityEngine.Random.Range(0, clips2.Length);
                        if (index2 < clips2.Length) {audioSrc.PlayOneShot(clips2[index2]);}

                        pState = PlayerState.dashenemy;
                    }

                    //pState = PlayerState.dashenemy;
                    anim.speed = 2f;
                    MovementState mstate = MovementState.idle;

                    if (Grounded())
                    {
                        int hitIndex = UnityEngine.Random.Range(0, 7); // random number 0-6

                        switch (hitIndex)
                        {
                            case 0: { mstate = MovementState.punch1; } break;
                            case 1: { mstate = MovementState.punch2; } break;
                            case 2: { mstate = MovementState.punch3; } break;
                            case 3: { mstate = MovementState.punch4; } break;
                            case 4: { mstate = MovementState.kick1; } break;
                            case 5: { mstate = MovementState.kick2; } break;
                            case 6: { mstate = MovementState.airpunch; } break;
                        }
                    }else{
                        mstate = MovementState.airkick;
                    }

                    anim.SetInteger("mstate", (int)mstate);
                    rb.gravityScale = 0;
                }
                else if (Input.GetKey(KeyCode.O) && currentTarget == null)
                {
                    dash_spd = 0f;
                    attacking = false;
                    if (pState != PlayerState.dashenemy)
                    {
                        AudioClip[] clips = { sndAttack, sndAttack2, sndAttack3 };
                        int index = UnityEngine.Random.Range(0, clips.Length + 1); // +1 to include "no sound"
                        if (index < clips.Length) {audioSrc.PlayOneShot(clips[index]);}
                        
                        AudioClip[] clips2 = { sndSwipe, sndSwipe2, sndSwipe3 };
                        int index2 = UnityEngine.Random.Range(0, clips2.Length);
                        if (index2 < clips2.Length) {audioSrc.PlayOneShot(clips2[index2]);}

                        pState = PlayerState.dashenemy;
                    }
                    anim.speed = 1.5f;
                    MovementState mstate = MovementState.idle;

                    if (Grounded())
                    {
                        int hitIndex = UnityEngine.Random.Range(0, 7); // random number 0-6

                        switch (hitIndex)
                        {
                            case 0: { mstate = MovementState.punch1; } break;
                            case 1: { mstate = MovementState.punch2; } break;
                            case 2: { mstate = MovementState.punch3; } break;
                            case 3: { mstate = MovementState.punch4; } break;
                            case 4: { mstate = MovementState.kick1; } break;
                            case 5: { mstate = MovementState.kick2; } break;
                            case 6: { mstate = MovementState.airpunch; } break;
                        }
                    }
                    else
                    {
                        mstate = MovementState.airkick;
                    }

                    anim.SetInteger("mstate", (int)mstate);
                }

                if (Input.GetKey(KeyCode.L) && currentTarget != null && Mathf.Abs(currentTarget.transform.position.x - origin.x) <= 1f && Grounded())   // uppercut
                {
                    dash_spd = 4f;
                    pState = PlayerState.dashenemy;
                    anim.speed = 2f;
                    MovementState mstate = MovementState.uppercut;
                    anim.SetInteger("mstate", (int)mstate);
                    attacking = true;
                    uppercut = true;
                    rb.gravityScale = 0;

                    AudioClip[] clips = { sndAttack, sndAttack2, sndAttack3 };
                    int index = UnityEngine.Random.Range(0, clips.Length + 1); // +1 to include "no sound"
                    if (index < clips.Length) {audioSrc.PlayOneShot(clips[index]);}
                        
                    AudioClip[] clips2 = { sndSwipe, sndSwipe2, sndSwipe3 };
                    int index2 = UnityEngine.Random.Range(0, clips2.Length);
                    if (index2 < clips2.Length) {audioSrc.PlayOneShot(clips2[index2]);}
                }
                else if (Input.GetKey(KeyCode.L) && (currentTarget == null || (currentTarget != null && Mathf.Abs(currentTarget.transform.position.x - origin.x) > 1f)) && Grounded())
                {
                    dash_spd = 0f;
                    pState = PlayerState.dashenemy;
                    anim.speed = 2f;
                    MovementState mstate = MovementState.uppercut;
                    anim.SetInteger("mstate", (int)mstate);
                    attacking = false;
                    uppercut = false;

                    AudioClip[] clips = { sndAttack, sndAttack2, sndAttack3 };
                    int index = UnityEngine.Random.Range(0, clips.Length + 1); // +1 to include "no sound"
                    if (index < clips.Length) {audioSrc.PlayOneShot(clips[index]);}
                        
                    AudioClip[] clips2 = { sndSwipe, sndSwipe2, sndSwipe3 };
                    int index2 = UnityEngine.Random.Range(0, clips2.Length);
                    if (index2 < clips2.Length) {audioSrc.PlayOneShot(clips2[index2]);}
                }

                if (Input.GetKey(KeyCode.P) && Grounded() && currentCounter != null)   // countering
                {
                    if (Math.Abs(currentCounter.transform.position.x - transform.position.x) > 3.75f && Math.Abs(currentCounter.transform.position.x - transform.position.x) <= 5f) {dash_spd = 24f;}
                    if (Math.Abs(currentCounter.transform.position.x - transform.position.x) > 2.5f && Math.Abs(currentCounter.transform.position.x - transform.position.x) <= 3.75f) {dash_spd = 18f;}
                    if (Math.Abs(currentCounter.transform.position.x - transform.position.x) > 1.25f && Math.Abs(currentCounter.transform.position.x - transform.position.x) <= 2.5f) {dash_spd = 12f;}
                    if (Math.Abs(currentCounter.transform.position.x - transform.position.x) >= 0f && Math.Abs(currentCounter.transform.position.x - transform.position.x) <= 1.25f) {dash_spd = 6f;}

                    countering = true;
                    currentCounter.anim.speed = 0f;
                    pState = PlayerState.dashenemy;

                    AudioClip[] clips = { sndAttack, sndAttack2, sndAttack3 };
                    int index = UnityEngine.Random.Range(0, clips.Length + 1); // +1 to include "no sound"
                    if (index < clips.Length) {audioSrc.PlayOneShot(clips[index]);}
                        
                    AudioClip[] clips2 = { sndSwipe, sndSwipe2, sndSwipe3 };
                    int index2 = UnityEngine.Random.Range(0, clips2.Length);
                    if (index2 < clips2.Length) {audioSrc.PlayOneShot(clips2[index2]);}

                    if (currentCounter.transform.position.x < transform.position.x)
                        sprite.flipX = true;
                    else if (currentCounter.transform.position.x > transform.position.x)
                        sprite.flipX = false;
                    
                    anim.speed = 2f;
                    MovementState mstate = MovementState.idle;

                    int hitIndex = UnityEngine.Random.Range(0, 4);

                    switch (hitIndex)
                    {
                        case 0: { mstate = MovementState.block1; } break;
                        case 1: { mstate = MovementState.block2; } break;
                        case 2: { mstate = MovementState.block3; } break;
                        case 3: { mstate = MovementState.block4; } break;
                    }

                    anim.SetInteger("mstate", (int)mstate);
                    rb.gravityScale = 0;
                }
                else if (Input.GetKey(KeyCode.P) && Grounded() && currentCounter == null)
                {
                    dash_spd = 0f;
                    countering = false;
                    pState = PlayerState.dashenemy;
                    anim.speed = 1.5f;
                    MovementState mstate = MovementState.idle;

                    AudioClip[] clips = { sndAttack, sndAttack2, sndAttack3 };
                    int index = UnityEngine.Random.Range(0, clips.Length + 1); // +1 to include "no sound"
                    if (index < clips.Length) {audioSrc.PlayOneShot(clips[index]);}
                        
                    AudioClip[] clips2 = { sndSwipe, sndSwipe2, sndSwipe3 };
                    int index2 = UnityEngine.Random.Range(0, clips2.Length);
                    if (index2 < clips2.Length) {audioSrc.PlayOneShot(clips2[index2]);}

                    int hitIndex = UnityEngine.Random.Range(0, 4);

                    switch (hitIndex)
                    {
                        case 0: { mstate = MovementState.block1; } break;
                        case 1: { mstate = MovementState.block2; } break;
                        case 2: { mstate = MovementState.block3; } break;
                        case 3: { mstate = MovementState.block4; } break;
                    }

                    anim.SetInteger("mstate", (int)mstate);
                }
            }
            break;

            case PlayerState.swing:
            {
                float ropeAngleAcceleration = accelerationRate * Mathf.Cos(ropeAngle * Mathf.Deg2Rad); //-0.02
                dirX = Input.GetAxisRaw("Horizontal");
                dirY = -Input.GetAxisRaw("Vertical"); //key up returns -1, key down returns +1
                ropeAngleAcceleration += dirX * 0.04f;
                ropeLength += dirY * 0.01f;
                ropeLength = Mathf.Max(ropeLength, 0f);
                ropeAngleVelocity += ropeAngleAcceleration;
                ropeAngle += ropeAngleVelocity;
                ropeAngleVelocity *= 0.99f;
                ropeX = grappleX + Mathf.Cos(ropeAngle * Mathf.Deg2Rad) * ropeLength;
                ropeY = grappleY + Mathf.Sin(ropeAngle * Mathf.Deg2Rad) * ropeLength;

                rb.MovePosition(new Vector2(ropeX, ropeY));

                Vector2 ropeDirection = new Vector2(ropeX - grappleX, ropeY - grappleY).normalized;
                float ropeAngleDeg = Mathf.Atan2(ropeDirection.y, ropeDirection.x) * Mathf.Rad2Deg;

                visual.rotation = Quaternion.Euler(0, 0, ropeAngleDeg + 90); // Apply rotation to visual so it faces along the rope

                if (Input.GetKeyUp("space"))
                {
                    rb.velocity = new Vector2(rb.velocity.x, jspd);
                    rb.gravityScale = 1;

                    MovementState mstate;
                    mstate = MovementState.endswing;
                    anim.SetInteger("mstate", (int)mstate);

                    coll.size = new Vector2(0.8397379f, 1.615343f);
                    coll.offset = new Vector2(-0.03511286f, -0.03012538f);

                    audioSrc.PlayOneShot(sndWebRelease);

                    pState = PlayerState.normal;

                    swingEnd = true;

                    ReturnAllRopeSegmentsToPool(); // Destroy old rope segments
                }

                float dirOff = 0f;
                if (!sprite.flipX)
                    dirOff = 1f;
                else
                    dirOff = -1f;

                bool nearWall = Physics2D.Raycast(new Vector2(wallPositionChecker.position.x - 0.315f, wallPositionChecker.position.y - 0.372f), transform.right * dirX, wallCheckDistance, jumpableGround);
                bool nearCeiling = Physics2D.Raycast(new Vector2(ceilingPositionChecker.position.x - 0.53f, ceilingPositionChecker.position.y - 0.68f), transform.up, ceilingCheckDistance, jumpableGround);
                bool onGround = Grounded();

                if (onGround)
                {
                    visual.rotation = Quaternion.Euler(0, 0, 0);
                    coll.size = new Vector2(0.8397379f, 1.615343f);
                    coll.offset = new Vector2(-0.03511286f, -0.03012538f);
                    audioSrc.PlayOneShot(sndWebSnap);
                    pState = PlayerState.normal;
                    ReturnAllRopeSegmentsToPool();
                    rb.gravityScale = 1;
                }
                else if (nearWall && dirOff > 0)
                {
                    hasTurn = false;
                    visual.rotation = Quaternion.Euler(0, 0, 0);
                    StartCoroutine(RotateAroundCorner(new Vector3(-0.1f, 0.1f, 0), 90f, 4));
                }
                else if (nearWall && dirOff < 0)
                {
                    hasTurn = false;
                    visual.rotation = Quaternion.Euler(0, 0, 0);
                    StartCoroutine(RotateAroundCorner(new Vector3(0.1f, 0.1f, 0), -90f, 2));
                }
                else if (nearCeiling)
                {
                    hasTurn = false;
                    visual.rotation = Quaternion.Euler(0, 0, 0);
                    StartCoroutine(RotateAroundCorner(new Vector3(0f, 0.15f, 0), 180f, 3));
                }

                if (nearWall || nearCeiling)
                {
                    coll.size = new Vector2(0.8397379f, 1.615343f);
                    coll.offset = new Vector2(-0.03511286f, -0.03012538f);
                    audioSrc.PlayOneShot(sndWebSnap);
                    pState = PlayerState.crawl;
                    ReturnAllRopeSegmentsToPool();
                    rb.gravityScale = 0;
                }
            }
            break;

            case PlayerState.crawl:
            {
                swingEnd = false;
                wasGrounded = true;
                dirX = Input.GetAxisRaw("Horizontal");

                if (direction == 1)
                    crawlDir = (Input.GetAxisRaw("Horizontal") * 2.75f);
                else if (direction == 3)
                    crawlDir = (Input.GetAxisRaw("Horizontal") * -2.75f);
                else if (direction == 2)
                    crawlDir = (Input.GetAxisRaw("Vertical") * -2.75f);
                else
                    crawlDir = (Input.GetAxisRaw("Vertical") * 2.75f);

                rb.velocity = transform.right * crawlDir;

                if (isTurning) return;

                groundDetected = Physics2D.Raycast(groundPositionChecker.position, -transform.up, groundCheckDistance, jumpableGround);
                wallDetected = Physics2D.Raycast(wallPositionChecker.position, transform.right * dirX, wallCheckDistance, jumpableGround);

                if (!groundDetected && !hasTurn)
                {
                    if (crawlDir > 0) // moving right (clockwise)
                    {
                        ZaxisAdd -= 90;
                        switch (direction)
                        {
                            case 1:
                                StartCoroutine(RotateAroundCorner(new Vector3(0.3f, -0.3f, 0), -90f, 2));
                                break;
                            case 2:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.3f, -0.3f, 0), -90f, 3));
                                break;
                            case 3:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.3f, 0.3f, 0), -90f, 4));
                                break;
                            case 4:
                                StartCoroutine(RotateAroundCorner(new Vector3(0.3f, 0.3f, 0), -90f, 1));
                                break;
                        }
                    }
                    else if (crawlDir < 0) // moving left (counter-clockwise)
                    {
                        ZaxisAdd += 90;
                        switch (direction)
                        {
                            case 1:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.35f, -0.3f, 0), 90f, 4));
                                break;
                            case 4:
                                StartCoroutine(RotateAroundCorner(new Vector3(0.1f, -0.35f, 0), 90f, 3));
                                break;
                            case 3:
                                StartCoroutine(RotateAroundCorner(new Vector3(0.35f, 0.3f, 0), 90f, 2));
                                break;
                            case 2:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.1f, 0.35f, 0), 90f, 1));
                                break;
                        }
                    }
                }

                if (groundDetected)
                    hasTurn = false;

                if (wallDetected && !hasTurn)
                {
                    if (crawlDir > 0) // (clockwise)
                    {
                        ZaxisAdd += 90;
                        switch (direction)
                        {
                            case 1:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.1f, 0.1f, 0), 90f, 4));
                                break;
                            case 2:
                                StartCoroutine(RotateAroundCorner(new Vector3(0.3f, 0.1f, 0), 90f, 1));
                                break;
                            case 3:
                                StartCoroutine(RotateAroundCorner(new Vector3(0.3f, -0.3f, 0), 90f, 2));
                                break;
                            case 4:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.3f, -0.3f, 0), 90f, 3));
                                break;
                        }
                    }
                    else if (crawlDir < 0) // (counter-clockwise)
                    {
                        ZaxisAdd -= 90;
                        switch (direction)
                        {
                            case 1:
                                StartCoroutine(RotateAroundCorner(new Vector3(0.1f, 0.1f, 0), -90f, 2));
                                break;
                            case 2:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.3f, -0.1f, 0), -90f, 3));
                                break;
                            case 3:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.3f, 0.3f, 0), -90f, 4));
                                break;
                            case 4:
                                StartCoroutine(RotateAroundCorner(new Vector3(-0.1f, 0.1f, 0), -90f, 1));
                                break;
                        }
                    }
                }

                if (Input.GetKeyDown("space"))
                {
                    if (direction == 1)
                    {
                        rb.gravityScale = 1;
                        pState = PlayerState.normal;
                    }
                    else
                    {
                        if (direction == 4)
                        {
                            dirX = -1;
                            transform.eulerAngles = new Vector3(0f, 0f, 0f);
                            transform.Translate(new Vector3(-0.1f, 0f, 0f));
                            rb.velocity = new Vector2(-1f, jspd);
                        }
                        else if (direction == 2)
                        {
                            dirX = 1;
                            transform.eulerAngles = new Vector3(0f, 0f, 0f);
                            transform.Translate(new Vector3(0.1f, 0f, 0f));
                            rb.velocity = new Vector2(1f, jspd);
                        }
                        else if (direction == 3)
                        {
                            transform.eulerAngles = new Vector3(0f, 0f, 0f);
                            transform.Translate(new Vector3(0f, -0.1f, 0f));
                            rb.velocity = new Vector2(0f, -1f);

                        }

                        rb.gravityScale = 1;
                        pState = PlayerState.normal;
                    }
                }
            }
            break;

            case PlayerState.quickzip:
            {
                swingEnd = false;

                if (moveTarget.HasValue)
                {
                    Vector2 currentPos = rb.position;
                    Vector2 target = moveTarget.Value;
                    Vector2 zipDir = (target - currentPos).normalized;

                    if (zipDir != Vector2.zero)
                    {
                        float angle = Mathf.Atan2(zipDir.y, zipDir.x) * Mathf.Rad2Deg;
                        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);
                    }

                    // Move with velocity
                    rb.velocity = zipDir * 4f;

                    // Stop when close enough
                    if (Vector2.Distance(currentPos, target) < 0.25f)
                    {
                        rb.velocity = Vector2.zero;
                        moveTarget = null;
                        rb.gravityScale = 1;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        coll.size = new Vector2(0.8397379f, 1.615343f);
                        coll.offset = new Vector2(-0.03511286f, -0.03012538f);
                        pState = PlayerState.normal;
                        ReturnAllRopeSegmentsToPool();
                    }

                    float xOff = 0f;
                    if (transform.eulerAngles.z < 315 && transform.eulerAngles.z >= 225)
                        xOff = -0.4f;
                    else if (transform.eulerAngles.z < 45 && transform.eulerAngles.z >= 315)
                        xOff = 0f;
                    else if (transform.eulerAngles.z < 135 && transform.eulerAngles.z >= 45)
                        xOff = 0.4f;
                    else if (transform.eulerAngles.z < 225 && transform.eulerAngles.z >= 135)
                        xOff = 0f;

                    float dirOff = 0f;
                    if (!sprite.flipX)
                        dirOff = 1f;
                    else
                        dirOff = -1f;

                    Vector2 wallDir = new Vector2(dirOff, 0); // left or right
                    Vector2 ceilingDir = Vector2.up;

                    // Raycast for wall
                    bool nearWall = Physics2D.Raycast(new Vector2(transform.position.x + xOff, transform.position.y), wallDir, 0.5f, jumpableGround);
                    // Raycast for ceiling
                    bool nearCeiling = Physics2D.Raycast(new Vector2(transform.position.x + xOff, transform.position.y), ceilingDir, 0.5f, jumpableGround);

                    // Debugging
                    //Debug.Log(nearCeiling);
                    //Debug.DrawRay(new Vector2(transform.position.x + xOff, transform.position.y), wallDir * 0.5f, Color.red);
                    //Debug.DrawRay(new Vector2(transform.position.x + xOff, transform.position.y), ceilingDir * 0.5f, Color.blue);

                    if (nearWall && dirOff > 0)
                    {
                        hasTurn = false;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        StartCoroutine(RotateAroundCorner(new Vector3(-0.1f, 0.1f, 0), 90f, 4));
                    }
                    else if (nearWall && dirOff < 0)
                    {
                        hasTurn = false;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        StartCoroutine(RotateAroundCorner(new Vector3(0.1f, 0.1f, 0), -90f, 2));
                    }
                    else if (nearCeiling)
                    {
                        hasTurn = false;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        StartCoroutine(RotateAroundCorner(new Vector3(0f, 0.15f, 0), 180f, 3));
                    }

                    if (nearWall || nearCeiling)
                    {
                        moveTarget = null;
                        coll.size = new Vector2(0.8397379f, 1.615343f);
                        coll.offset = new Vector2(-0.03511286f, -0.03012538f);
                        pState = PlayerState.crawl;
                        ReturnAllRopeSegmentsToPool();
                        rb.gravityScale = 0;
                    }
                }
            }
            break;

            case PlayerState.dashenemy:
            {
                if (attacking && currentTarget == null) { attacking = false; }

                if (attacking)
                {
                    currentTarget.rb.velocity = new Vector2(0f, currentTarget.rb.velocity.y);
                    rb.velocity = new Vector2(0f, 0f);

                    if (currentTarget.Grounded())
                    {
                        if ((Math.Abs(currentTarget.transform.position.x - transform.position.x) >= 0.45f) && !waitingToHit && ((stateInfo.IsName("Player_Air_Kick") && stateInfo.normalizedTime <= 0.86f) || (stateInfo.IsName("Player_Air_Punch") && stateInfo.normalizedTime <= 0.67f) || (stateInfo.IsName("Player_Kick1") && stateInfo.normalizedTime <= 0.65f) || (stateInfo.IsName("Player_Kick2") && stateInfo.normalizedTime <= 0.46f) || (stateInfo.IsName("Player_Punch1") && stateInfo.normalizedTime <= 0.52f) || (stateInfo.IsName("Player_Punch2") && stateInfo.normalizedTime <= 0.48f) || (stateInfo.IsName("Player_Punch3") && stateInfo.normalizedTime <= 0.25f) || (stateInfo.IsName("Player_Punch4") && stateInfo.normalizedTime <= 0.45f) || (stateInfo.IsName("Player_Uppercut") && stateInfo.normalizedTime <= 0.33f)))
                        {
                            float step = dash_spd * Time.deltaTime;
                            transform.position = Vector2.MoveTowards(transform.position, currentTarget.transform.position, step);
                        }

                        if (waitingToHit)
                        {
                            float dist = Mathf.Abs(currentTarget.transform.position.x - transform.position.x);
                            if (dist < 0.45f)
                            {
                                anim.speed = 1;
                                waitingToHit = false;
                            }
                            else
                            {
                                // Keep moving toward the enemy
                                float step = dash_spd * Time.deltaTime;
                                transform.position = Vector2.MoveTowards(transform.position, currentTarget.transform.position, step);
                            }
                        }
                    }
                    else
                    {
                        if ((Vector2.Distance(transform.position, currentTarget.transform.position) >= 0.2f) && !waitingToHit && ((stateInfo.IsName("Player_Air_Kick") && stateInfo.normalizedTime <= 0.86f) || (stateInfo.IsName("Player_Air_Punch") && stateInfo.normalizedTime <= 0.67f) || (stateInfo.IsName("Player_Kick1") && stateInfo.normalizedTime <= 0.65f) || (stateInfo.IsName("Player_Kick2") && stateInfo.normalizedTime <= 0.46f) || (stateInfo.IsName("Player_Punch1") && stateInfo.normalizedTime <= 0.52f) || (stateInfo.IsName("Player_Punch2") && stateInfo.normalizedTime <= 0.48f) || (stateInfo.IsName("Player_Punch3") && stateInfo.normalizedTime <= 0.25f) || (stateInfo.IsName("Player_Punch4") && stateInfo.normalizedTime <= 0.45f) || (stateInfo.IsName("Player_Uppercut") && stateInfo.normalizedTime <= 0.33f)))
                        {
                            float step = dash_spd * Time.deltaTime;
                            transform.position = Vector2.MoveTowards(transform.position, currentTarget.transform.position, step);
                        }

                        if (waitingToHit)
                        {
                            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
                            if (dist < 0.2f)
                            {
                                anim.speed = 1;
                                waitingToHit = false;
                            }
                            else
                            {
                                // Keep moving toward the enemy
                                float step = dash_spd * Time.deltaTime;
                                transform.position = Vector2.MoveTowards(transform.position, currentTarget.transform.position, step);
                            }
                        }
                    }

                    if (stateInfo.normalizedTime >= 1f)
                    {
                        pastHitEvent = false;
                        pState = PlayerState.normal;
                        attacking = false;
                        uppercut = false;
                        currentTarget.rb.gravityScale = 1;
                        currentTarget.hsp = 1f;
                        rb.gravityScale = 1;
                    }

                    if (pastHitEvent)
                    {
                        // combat
                        bool facingLeft = sprite.flipX;

                        if (Input.GetAxisRaw("Horizontal") > 0)
                            facingLeft = false;
                        else if (Input.GetAxisRaw("Horizontal") < 0)
                            facingLeft = true;
                        else
                            facingLeft = sprite.flipX;

                        Vector2 origin = transform.position;

                        // Get all enemies in a radius
                        Collider2D[] ehits = Physics2D.OverlapCircleAll(origin, 5.2f, enemyMask);
                        Collider2D[] ehitsC = Physics2D.OverlapCircleAll(origin, 5.2f, enemyMask);

                        float closestEDistance = Mathf.Infinity;
                        float closestEDistanceC = Mathf.Infinity;
                        RobotStep closestEnemy = null;
                        RobotStep closestCounter = null;

                        foreach (var ehit in ehits)
                        {
                            RobotStep enemy = ehit.GetComponent<RobotStep>();
                            if (enemy == null || enemy.eState == RobotStep.EnemyState.death)
                                continue;

                            // Linecast to check if anything blocks the path
                            RaycastHit2D hit = Physics2D.Linecast(transform.position, enemy.transform.position, jumpableGround);

                            if (hit.collider != null)
                                if ((Vector2)hit.point != (Vector2)enemy.transform.position) continue;

                            float dx = enemy.transform.position.x - origin.x;

                            // Check if in front based on flipX
                            if ((facingLeft && dx > 0) || (!facingLeft && dx < 0))
                                continue;

                            float dist = Mathf.Abs(dx);
                            if (dist < closestEDistance)
                            {
                                closestEDistance = dist;
                                closestEnemy = enemy;
                            }
                        }

                        foreach (var ehitC in ehitsC)
                        {
                            RobotStep enemyC = ehitC.GetComponent<RobotStep>();
                            if (enemyC == null || enemyC.eState == RobotStep.EnemyState.death)
                                continue;

                            if (enemyC.eState != RobotStep.EnemyState.attack)
                                continue;

                            // Linecast to check if anything blocks the path
                            RaycastHit2D hitC = Physics2D.Linecast(transform.position, enemyC.transform.position, jumpableGround);

                            if (hitC.collider != null)
                                if ((Vector2)hitC.point != (Vector2)enemyC.transform.position) continue;

                            float dxC = enemyC.transform.position.x - origin.x;

                            float distC = Mathf.Abs(dxC);
                            if (distC < closestEDistanceC)
                            {
                                closestEDistanceC = distC;
                                closestCounter = enemyC;
                            }
                        }

                        currentTarget = closestEnemy;
                        currentCounter = closestCounter;

                        if (Input.GetKey(KeyCode.O) && currentTarget != null)   // normal attack
                        {
                            if (Math.Abs(currentTarget.transform.position.x - transform.position.x) > 3.75f && Math.Abs(currentTarget.transform.position.x - transform.position.x) <= 5f) { dash_spd = 16f; }
                            if (Math.Abs(currentTarget.transform.position.x - transform.position.x) > 2.5f && Math.Abs(currentTarget.transform.position.x - transform.position.x) <= 3.75f) { dash_spd = 12; }
                            if (Math.Abs(currentTarget.transform.position.x - transform.position.x) > 1.25f && Math.Abs(currentTarget.transform.position.x - transform.position.x) <= 2.5f) { dash_spd = 8f; }
                            if (Math.Abs(currentTarget.transform.position.x - transform.position.x) >= 0f && Math.Abs(currentTarget.transform.position.x - transform.position.x) <= 1.25f) { dash_spd = 4f; }
                            attacking = true;
                            pState = PlayerState.dashenemy;
                            anim.speed = 2f;
                            MovementState mstate = MovementState.idle;

                            if (Grounded())
                            {
                                int hitIndex = UnityEngine.Random.Range(0, 7); // random number 0-6

                                switch (hitIndex)
                                {
                                    case 0: { mstate = MovementState.punch1; } break;
                                    case 1: { mstate = MovementState.punch2; } break;
                                    case 2: { mstate = MovementState.punch3; } break;
                                    case 3: { mstate = MovementState.punch4; } break;
                                    case 4: { mstate = MovementState.kick1; } break;
                                    case 5: { mstate = MovementState.kick2; } break;
                                    case 6: { mstate = MovementState.airpunch; } break;
                                }
                            }
                            else
                            {
                                mstate = MovementState.airkick;
                            }

                            anim.SetInteger("mstate", (int)mstate);
                            rb.gravityScale = 0;
                            pastHitEvent = false;
                        }
                        else if (Input.GetKey(KeyCode.O) && currentTarget == null)
                        {
                            dash_spd = 0f;
                            attacking = false;
                            pState = PlayerState.dashenemy;
                            anim.speed = 1.5f;
                            MovementState mstate = MovementState.idle;

                            if (Grounded())
                            {
                                int hitIndex = UnityEngine.Random.Range(0, 7); // random number 0-6

                                switch (hitIndex)
                                {
                                    case 0: { mstate = MovementState.punch1; } break;
                                    case 1: { mstate = MovementState.punch2; } break;
                                    case 2: { mstate = MovementState.punch3; } break;
                                    case 3: { mstate = MovementState.punch4; } break;
                                    case 4: { mstate = MovementState.kick1; } break;
                                    case 5: { mstate = MovementState.kick2; } break;
                                    case 6: { mstate = MovementState.airpunch; } break;
                                }
                            }
                            else
                            {
                                mstate = MovementState.airkick;
                            }

                            anim.SetInteger("mstate", (int)mstate);
                            pastHitEvent = false;
                        }

                        if (Input.GetKey(KeyCode.L) && currentTarget != null && Mathf.Abs(currentTarget.transform.position.x - origin.x) <= 1f && Grounded())   // uppercut
                        {
                            dash_spd = 4f;
                            pState = PlayerState.dashenemy;
                            anim.speed = 2f;
                            MovementState mstate = MovementState.uppercut;
                            anim.SetInteger("mstate", (int)mstate);
                            attacking = true;
                            uppercut = true;
                            rb.gravityScale = 0;
                            pastHitEvent = false;
                        }
                        else if (Input.GetKey(KeyCode.L) && (currentTarget == null || (currentTarget != null && Mathf.Abs(currentTarget.transform.position.x - origin.x) > 1f)) && Grounded())
                        {
                            dash_spd = 0f;
                            pState = PlayerState.dashenemy;
                            anim.speed = 2f;
                            MovementState mstate = MovementState.uppercut;
                            anim.SetInteger("mstate", (int)mstate);
                            attacking = false;
                            uppercut = false;
                            pastHitEvent = false;
                        }

                        if (Input.GetKey(KeyCode.P) && Grounded() && currentCounter != null)   // countering
                        {
                            if (Math.Abs(currentCounter.transform.position.x - transform.position.x) > 3.75f && Math.Abs(currentCounter.transform.position.x - transform.position.x) <= 5f) { dash_spd = 24f; }
                            if (Math.Abs(currentCounter.transform.position.x - transform.position.x) > 2.5f && Math.Abs(currentCounter.transform.position.x - transform.position.x) <= 3.75f) { dash_spd = 18f; }
                            if (Math.Abs(currentCounter.transform.position.x - transform.position.x) > 1.25f && Math.Abs(currentCounter.transform.position.x - transform.position.x) <= 2.5f) { dash_spd = 12f; }
                            if (Math.Abs(currentCounter.transform.position.x - transform.position.x) >= 0f && Math.Abs(currentCounter.transform.position.x - transform.position.x) <= 1.25f) { dash_spd = 6f; }

                            countering = true;
                            currentCounter.anim.speed = 0f;
                            pState = PlayerState.dashenemy;

                            if (currentCounter.transform.position.x < transform.position.x)
                                sprite.flipX = true;
                            else if (currentCounter.transform.position.x > transform.position.x)
                                sprite.flipX = false;

                            anim.speed = 2f;
                            MovementState mstate = MovementState.idle;

                            int hitIndex = UnityEngine.Random.Range(0, 4);

                            switch (hitIndex)
                            {
                                case 0: { mstate = MovementState.block1; } break;
                                case 1: { mstate = MovementState.block2; } break;
                                case 2: { mstate = MovementState.block3; } break;
                                case 3: { mstate = MovementState.block4; } break;
                            }

                            anim.SetInteger("mstate", (int)mstate);
                            rb.gravityScale = 0;
                            pastHitEvent = false;
                        }
                        else if (Input.GetKey(KeyCode.P) && Grounded() && currentCounter == null)
                        {
                            dash_spd = 0f;
                            countering = false;
                            pState = PlayerState.dashenemy;
                            anim.speed = 1.5f;
                            MovementState mstate = MovementState.idle;

                            int hitIndex = UnityEngine.Random.Range(0, 4);

                            switch (hitIndex)
                            {
                                case 0: { mstate = MovementState.block1; } break;
                                case 1: { mstate = MovementState.block2; } break;
                                case 2: { mstate = MovementState.block3; } break;
                                case 3: { mstate = MovementState.block4; } break;
                            }

                            anim.SetInteger("mstate", (int)mstate);
                            pastHitEvent = false;
                        }
                    }
                }
                else if (countering)
                {
                    currentCounter.rb.velocity = new Vector2(0f, currentCounter.rb.velocity.y);
                    rb.velocity = new Vector2(0f, 0f);

                    int hitIndex = UnityEngine.Random.Range(0, 3);

                    switch (hitIndex)
                    {
                        case 0: { currentCounter.alarm4 = 300; } break;
                        case 1: { currentCounter.alarm4 = 400; } break;
                        case 2: { currentCounter.alarm4 = 500; } break;
                    }

                    currentCounter.kick = false;
                    currentCounter.rb.gravityScale = 1;

                    if ((Math.Abs(currentCounter.transform.position.x - transform.position.x) >= 0.45f) && !waitingToHit && ((stateInfo.IsName("Player_Block1") && stateInfo.normalizedTime <= 0.28f) || (stateInfo.IsName("Player_Block2") && stateInfo.normalizedTime <= 0.30f) || (stateInfo.IsName("Player_Block3") && stateInfo.normalizedTime <= 0.32f) || (stateInfo.IsName("Player_Block4") && stateInfo.normalizedTime <= 0.38f)))
                    {
                        float step = dash_spd * Time.deltaTime;
                        transform.position = Vector2.MoveTowards(transform.position, currentCounter.transform.position, step);
                    }

                    if (waitingToHit)
                    {
                        float dist = Mathf.Abs(currentCounter.transform.position.x - transform.position.x);
                        if (dist < 0.45f)
                        {
                            anim.speed = 1;
                            waitingToHit = false;
                        }
                        else
                        {
                            // Keep moving toward the enemy
                            float step = dash_spd * Time.deltaTime;
                            transform.position = Vector2.MoveTowards(transform.position, currentCounter.transform.position, step);
                        }
                    }

                    if (stateInfo.normalizedTime >= 1f)
                    {
                        pastHitEvent = false;
                        pState = PlayerState.normal;
                        countering = false;
                        uppercut = false;
                        currentCounter.rb.gravityScale = 1;
                        currentCounter.hsp = 1f;
                        rb.gravityScale = 1;
                    }
                }
                else
                {
                    if (Grounded()) { rb.velocity = new Vector2(0f, 0f); }

                    if (stateInfo.normalizedTime >= 1f)
                    {
                        pastHitEvent = false;
                        pState = PlayerState.normal;
                        countering = false;
                        anim.speed = 1;
                        uppercut = false;
                    }
                }
            }
            break;

            case PlayerState.hurt:
            {
                uppercut = false;
                rb.gravityScale = 1;
                attacking = false;
                countering = false;

                if ((stateInfo.IsName("Player_Launched")))
                {
                    if (stateInfo.normalizedTime >= 1f)
                        anim.speed = 0f;

                    if (Grounded())
                    {
                        anim.speed = 1f;
                        pState = PlayerState.normal;
                    }
                }
                else
                {
                    anim.speed = 1f;

                    if ((stateInfo.IsName("Player_Hurt1") && stateInfo.normalizedTime >= 1f) || (stateInfo.IsName("Player_Hurt2") && stateInfo.normalizedTime >= 1f))
                        pState = PlayerState.normal;
                }
            }
            break;
        }

        if (pState == PlayerState.swing)
            DrawRope(new Vector2(grappleX, grappleY), new Vector2(ropeX, ropeY));

        if (pState == PlayerState.quickzip)
            DrawRope(moveTarget.Value, new Vector2(transform.position.x, transform.position.y));

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (pState != PlayerState.crawl)
        {
            if (dirX > 0f)
                sprite.flipX = false;
            else if (dirX < 0f)
                sprite.flipX = true;
        } else {
            if (crawlDir > 0f)
                sprite.flipX = false;
            else if (crawlDir < 0f)
                sprite.flipX = true;
        }

        if (swingEnd) return; // Let Animator handle transitioning after animation finishes
        if (pState == PlayerState.dashenemy) return;
        if (pState == PlayerState.hurt) return;

        MovementState mstate = MovementState.idle;

        if (pState == PlayerState.normal)
        {
            if (shoot)
            {
                anim.speed = 0f;

                if (Grounded())
                    mstate = MovementState.groundshoot;
                else
                    mstate = MovementState.airshoot;
            }
            else
            {
                anim.speed = 1f; // Normal animation speed

                if (dirX > 0f)                  // Controlling running animation by controlling boolean variable responsible for triggering running animation based on horizontal speed
                    mstate = MovementState.running;
                else if (dirX < 0f)
                    mstate = MovementState.running;
                else
                    mstate = MovementState.idle;

                if (rb.velocity.y > 0.1f)
                    mstate = MovementState.jumping;
                else if (rb.velocity.y < -0.1f)
                    mstate = MovementState.falling;
            }
        }
        else if (pState == PlayerState.swing)
        {
            mstate = MovementState.swinging;
            anim.speed = 1f; // Normal animation speed
        }
        else if (pState == PlayerState.crawl)
        {
            mstate = MovementState.crawling;

            if (Mathf.Abs(crawlDir) > 0)
                anim.speed = 1f; // Normal animation speed
            else
                anim.speed = 0f; // Pause animation
        }
        else if (pState == PlayerState.quickzip)
        {
            mstate = MovementState.zip;
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        float normalizedTime = stateInfo.normalizedTime % 1f;

        if (mstate == MovementState.running)
        {
            if (normalizedTime >= 0.35f && normalizedTime <= 0.38f)
                audioSrc.PlayOneShot(sndStep2);

            if (normalizedTime >= 0.83f && normalizedTime <= 0.86f)
                audioSrc.PlayOneShot(sndStep);
        }

        if (pState == PlayerState.crawl && Mathf.Abs(crawlDir) > 0)
        {
            if (normalizedTime >= 0.41f && normalizedTime <= 0.44f)
                audioSrc.PlayOneShot(sndCrawlStep);

            if (normalizedTime >= 0.82f && normalizedTime <= 0.85f)
                audioSrc.PlayOneShot(sndCrawlStep2);
        }

        anim.SetInteger("mstate", (int)mstate);
    }

    public bool Grounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }

    void DrawRope(Vector2 start, Vector2 end)
    {
        ReturnAllRopeSegmentsToPool();

        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        int segmentCount = Mathf.CeilToInt(distance / ropeSegmentLength);

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 position = start + direction * ropeSegmentLength * i;
            GameObject seg = GetRopeSegment(position);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            seg.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    GameObject GetRopeSegment(Vector2 position)
    {
        GameObject segment;

        if (ropeSegmentPool.Count > 0)
        {
            segment = ropeSegmentPool.Dequeue();
            segment.SetActive(true);
        }
        else
        {
            segment = Instantiate(ropeSegmentPrefab);
        }

        segment.transform.position = position;
        ropeSegments.Add(segment);
        return segment;
    }

    void ReturnAllRopeSegmentsToPool()
    {
        foreach (var seg in ropeSegments)
        {
            if (ropeSegmentPool.Count < maxPoolSize)
            {
                seg.SetActive(false);
                ropeSegmentPool.Enqueue(seg);
            }
            else
            {
                Destroy(seg); // If we exceed the pool, just clean up
            }
        }

        ropeSegments.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new UnityEngine.Color(1f, 1f, 0f, 0.75f);
        Gizmos.DrawLine(groundPositionChecker.position, new Vector2(groundPositionChecker.position.x, groundPositionChecker.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallPositionChecker.position, new Vector2(wallPositionChecker.position.x + wallCheckDistance, wallPositionChecker.position.y));
    }

    private IEnumerator RotateAroundCorner(Vector3 positionDelta, float rotationDelta, int newDirection)
    {
        isTurning = true;
        hasTurn = true;
        float duration = 0.2f;
        float time = 0f;
        Vector3 startRotation = transform.eulerAngles;
        Vector3 endRotation = startRotation + new Vector3(0, 0, rotationDelta);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + positionDelta;

        while (time < duration)
        {
            float t = time / duration;
            transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, t);
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.eulerAngles = endRotation;
        transform.position = endPosition;
        direction = newDirection;
        isTurning = false;
    }

    bool CanStartCrawling()
    {
        bool nearWall = Physics2D.Raycast(wallPositionChecker.position, transform.right * dirX, wallCheckDistance, jumpableGround);
        bool onGround = Grounded();
        bool nearCeiling = Physics2D.Raycast(ceilingPositionChecker.position, transform.up, ceilingCheckDistance, jumpableGround);

        if (dirY > 0 && onGround)
            direction = 1;
        else if (nearWall && dirX > 0)
        {
            hasTurn = false;
            StartCoroutine(RotateAroundCorner(new Vector3(-0.1f, 0.1f, 0), 90f, 4));
        }
        else if (nearWall && dirX < 0)
        {
            hasTurn = false;
            StartCoroutine(RotateAroundCorner(new Vector3(0.1f, 0.1f, 0), -90f, 2));
        }
        else if (nearCeiling)
        {
            hasTurn = false;
            StartCoroutine(RotateAroundCorner(new Vector3(0f, 0.15f, 0), 180f, 3));
        }

        return ((dirY > 0 && onGround) || nearWall || nearCeiling);
    }

    Vector2? FindClosestTileTopCorner(Vector2 playerPos)
    {
        BoundsInt bounds = tilemap.cellBounds;
        float closestDistance = float.MaxValue;
        Vector2 bestCorner = Vector2.zero;
        bool found = false;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;
            
            // Check if there is a tile directly above
            Vector3Int abovePos = pos + Vector3Int.up;
            if (tilemap.HasTile(abovePos)) continue; // skip if blocked from above
            Vector3 worldPos = tilemap.GetCellCenterWorld(pos);
            Vector2 topLeft = worldPos + new Vector3(-0.5f, 0.5f);
            Vector2 topRight = worldPos + new Vector3(0.5f, 0.5f);
            List<Vector2> corners = new() { topLeft, topRight };

            foreach (Vector2 corner in corners)
            {
                if (Vector2.Distance(corner, playerPos) > 6f) continue;
                if (corner.y <= playerPos.y) continue;
                if (!sprite.flipX && corner.x <= playerPos.x) continue;
                if (sprite.flipX && corner.x >= playerPos.x) continue;

                // Linecast to check if anything blocks the path
                RaycastHit2D hit = Physics2D.Linecast(playerPos, corner, jumpableGround);

                if (hit.collider != null)
                    if ((Vector2)hit.point != corner) continue; // If the line hits something before reaching the corner, skip it

                float dist = Vector2.Distance(playerPos, corner);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestCorner = corner;
                    found = true;
                }
            }
        }

        return found ? bestCorner : null;
    }

    public void HitEvent()
    {
        if (attacking && ((Grounded() && (Vector3.Distance(currentTarget.transform.position, transform.position) <= 0.45f)) || (!Grounded() && (Vector3.Distance(currentTarget.transform.position, transform.position) <= 0.9f)))) { OnHit.Invoke(currentTarget); if (!pastHitEvent) { pastHitEvent = true; } }
        if (countering && ((Grounded() && (Vector3.Distance(currentCounter.transform.position, transform.position) <= 0.45f)) || (!Grounded() && (Vector3.Distance(currentCounter.transform.position, transform.position) <= 0.9f)))) { OnHit.Invoke(currentCounter); }
    }

    public void PauseBeforeHit()
    {
        if (attacking)
        {
            anim.speed = 0;
            currentTarget.anim.speed = 0;
            currentTarget.rb.velocity = new Vector2(0f, 0f);
            waitingToHit = true; // Flag to resume when player reaches enemy
        }

        if (countering)
        {
            anim.speed = 0;
            currentCounter.anim.speed = 0;
            currentCounter.rb.velocity = new Vector2(0f, 0f);
            waitingToHit = true; // Flag to resume when player reaches enemy
        }
    }

    public void SpawnHitEffect(Vector2 impactPoint)
    {
        Vector3 hitPosition = (transform.position + currentTarget.transform.position) / 2f;
        GameObject hitFX = Instantiate(hitParticlePrefab, impactPoint, Quaternion.identity);
    }

    public void SpawnHurtEffect(Vector2 impactPoint)
    {
        Vector3 hitPosition = (transform.position + enemyHitSpawn) / 2f;
        GameObject hitFX = Instantiate(hurtParticlePrefab, impactPoint, Quaternion.identity);
    }

    public void Damage(RobotStep target)
    {
        if (!countering)
        {
            float dir = 0;

            if (!target.sprite.flipX)
            {
                dir = 1f;
                dirX = -1f;
            }
            else
            {
                dir = -1f;
                dirX = 1f;
            }

            if (target.kick)
                rb.velocity = new Vector2(dir, 5f);
            else
                rb.velocity = new Vector2(dir, 0f);

            anim.speed = 1f;
            pState = PlayerState.hurt;
            MovementState mstate;

            if (target.kick)
            {
                mstate = MovementState.launched;

                AudioClip[] clips2 = { sndStrongHit, sndStrongHit2, };
                int index2 = UnityEngine.Random.Range(0, clips2.Length);
                if (index2 < clips2.Length) { audioSrc.PlayOneShot(clips2[index2]); }
            }
            else
            {
                int hitIndex = UnityEngine.Random.Range(0, 2); // 0 or 1

                if (hitIndex == 0)
                    mstate = MovementState.hurt1;
                else
                    mstate = MovementState.hurt2;

                AudioClip[] clips2 = { sndQuickHit, sndQuickHit2 };
                int index2 = UnityEngine.Random.Range(0, clips2.Length);
                if (index2 < clips2.Length) { audioSrc.PlayOneShot(clips2[index2]); }
            }

            anim.SetInteger("mstate", (int)mstate);

            Vector2 hitPoint = target.transform.position + new Vector3(0f, 0f); // Offset to torso or desired point
            enemyHitSpawn = currentTarget.transform.position;
            SpawnHurtEffect(hitPoint);
            AudioClip[] clips = { sndHurt, sndHurt2, sndHurt3 };
            int index = UnityEngine.Random.Range(0, clips.Length);
            if (index < clips.Length) { audioSrc.PlayOneShot(clips[index]); }
        }
        else
        {
            currentCounter.OnPlayerHit(currentCounter);
        }
    }
}