using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static PlayerStep;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class RobotStep : MonoBehaviour
{
    public Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] public SpriteRenderer sprite;
    private BoxCollider2D coll;
    private float lastspd = 0f;
    private float dirX = 0f;

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] public float hsp = 1f; // Horizontal speed
    [SerializeField] private int waitTime = 120;

    private enum MovementState { idle, running, falling, hurt1, hurt2, launched, shocked, sprinting, alertidle, punch1, punch2, kick }
    public enum EnemyState { normal, death, hurt, shocked, alert, attack }
    public EnemyState eState;

    // Sound Files
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip sndAlert;
    [SerializeField] private AudioClip sndAlert2;
    [SerializeField] private AudioClip sndAlert3;
    [SerializeField] private AudioClip sndAttack;
    [SerializeField] private AudioClip sndAttack2;
    [SerializeField] private AudioClip sndHit;
    [SerializeField] private AudioClip sndHit2;
    [SerializeField] private AudioClip sndHit3;
    [SerializeField] private AudioClip sndLand;
    [SerializeField] private AudioClip sndStep;
    private bool wasGrounded = false;
    private bool hasPlayedStep1;
    private bool hasPlayedStep2;

    // Alarms
    private int alarm1;
    private int alarm2 = 0;
    [SerializeField] private int alarm3 = 0;
    [SerializeField] private int alarm4 = 0;
    private bool startAlarm1 = true;
    private bool startAlarm2 = false;
    [SerializeField] private float distanceFromPlayer = 0f;

    // Combat
    private Material outline;
    [SerializeField] private PlayerStep player;
    [SerializeField] private bool noHitWall;
    [SerializeField] private bool shocked = false;
    public UnityEvent<PlayerStep> OnAttack;
    public bool kick = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        eState = EnemyState.normal;
        dirX = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
        alarm1 = waitTime;
        outline = sprite.material;
        player.OnHit.AddListener((x) => OnPlayerHit(x));
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, (player.transform.position - transform.position).normalized * (player.transform.position - transform.position).normalized.magnitude, Color.red);

        if (player.currentTarget == this)
        {
            outline.color = Color.white;
        }
        else if (eState == EnemyState.attack)
        {
            outline.color = Color.red;
        }
        else
        {
            outline.color = Color.black;
        }

        if (eState == EnemyState.hurt || player.pState == PlayerStep.PlayerState.dashenemy || (player.transform.position.y - transform.position.y > 0.15f && Grounded()))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), true);
        else
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), false);

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        distanceFromPlayer = Vector3.Distance(player.transform.position, transform.position);
        noHitWall = !Physics2D.Raycast(transform.position, (player.transform.position - transform.position).normalized, distanceFromPlayer, jumpableGround);

        if (startAlarm1)
        {
            if (alarm1 > 0)
            {
                alarm1 -= 1;
            }
            else
            {
                if (eState == EnemyState.normal)
                {
                    lastspd = dirX;
                    dirX = 0;
                    startAlarm1 = false;
                    startAlarm2 = true;
                }

                alarm2 = 240;
            }
        }

        if (startAlarm2)
        {
            if (alarm2 > 0)
            {
                alarm2 -= 1;
            }
            else
            {
                if (eState == EnemyState.normal)
                {
                    if (lastspd == 0) { lastspd = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1; }
                    dirX = -lastspd;
                    startAlarm2 = false;
                    startAlarm1 = true;
                }

                alarm1 = waitTime;
            }
        }

        if (alarm3 > 0)
        {
            alarm3 -= 1;
        }
        else
        {
            if (shocked && eState != EnemyState.attack)
            {
                if ((distanceFromPlayer <= 4.5f) && ((!sprite.flipX && transform.position.x < player.transform.position.x) || (sprite.flipX && transform.position.x > player.transform.position.x)) && noHitWall)
                    alarm3 = 300;
                else
                    shocked = false;
            }
        }

        if (alarm4 > 0)
        {
            alarm4 -= 1;
        }
        else
        {
            if (eState == EnemyState.alert)
            {
                if (distanceFromPlayer >= 4.5f || !noHitWall)
                {
                    eState = EnemyState.normal;
                }

                if ((Vector3.Distance(player.transform.position, transform.position) <= 2.05f) && ((!sprite.flipX && transform.position.x < player.transform.position.x) || (sprite.flipX && transform.position.x > player.transform.position.x)) && noHitWall)
                {
                    eState = EnemyState.attack;

                    rb.gravityScale = 0;

                    int hitIndex = UnityEngine.Random.Range(0, 3); // random number 0-6

                    MovementState mstate = MovementState.idle;

                    switch (hitIndex)
                    {
                        case 0: { mstate = MovementState.punch1; } break;
                        case 1: { mstate = MovementState.punch2; } break;
                        case 2: { mstate = MovementState.kick; kick = true; } break;
                    }

                    anim.SetInteger("mstate", (int)mstate);
                }
                else
                {
                    int hitIndex = UnityEngine.Random.Range(0, 3);

                    switch (hitIndex)
                    {
                        case 0: { alarm4 = 300; } break;
                        case 1: { alarm4 = 400; } break;
                        case 2: { alarm4 = 500; } break;
                    }
                }
            }
        }

        switch (eState)
        {
            case EnemyState.normal:
            {
                rb.velocity = new Vector2(dirX * hsp, rb.velocity.y);    // Moving character based on left or right arrow key

                /*AudioClip[] clips = { sndJump, sndJump2 };
                int index = UnityEngine.Random.Range(0, clips.Length + 1); // +1 to include "no sound"

                if (index < clips.Length)
                    audioSrc.PlayOneShot(clips[index]);*/

                if ((Math.Abs(transform.position.x - player.transform.position.x) <= 5f) && !shocked && ((!sprite.flipX && transform.position.x < player.transform.position.x) || (sprite.flipX && transform.position.x > player.transform.position.x)) && Grounded() && noHitWall)
                {
                    eState = EnemyState.shocked;
                    MovementState mstate = MovementState.shocked;
                    anim.SetInteger("mstate", (int)mstate);
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    anim.speed = 1f;
                    shocked = true;
                    alarm3 = 300;
                }

                if (!wasGrounded && Grounded() && eState == EnemyState.normal) // Landing Sound Code
                    audioSrc.PlayOneShot(sndLand);

                wasGrounded = Grounded();
            }
            break;

            case EnemyState.hurt:
            {
                if ((stateInfo.IsName("Enemy_Launched")))
                {
                    if (stateInfo.normalizedTime >= 1f)
                        anim.speed = 0f;
                    
                    if (Grounded())
                    {
                        anim.speed = 1f;
                        eState = EnemyState.normal;
                    }
                }
                else
                {
                    anim.speed = 1f;
                    if ((stateInfo.IsName("Enemy_Hit1") && stateInfo.normalizedTime >= 1f) || (stateInfo.IsName("Enemy_Hit2") && stateInfo.normalizedTime >= 1f))
                            eState = EnemyState.normal;
                }
            }
            break;

            case EnemyState.shocked:
            {
                if (stateInfo.IsName("Enemy_Shocked") && stateInfo.normalizedTime >= 1f)
                {
                    eState = EnemyState.alert;

                    int hitIndex = UnityEngine.Random.Range(0, 3);

                    switch(hitIndex)
                    {
                        case 0: { alarm4 = 300; } break;
                        case 1: { alarm4 = 400; } break;
                        case 2: { alarm4 = 500; } break;
                    }
                }
            }
            break;

            case EnemyState.alert:
            {
                if (Math.Abs(transform.position.x - player.transform.position.x) > 1.9f)
                {
                    if (transform.position.x < player.transform.position.x)
                    {
                        dirX = 1f;
                        sprite.flipX = false;
                    }
                    else
                    {
                        dirX = -1f;
                        sprite.flipX = true;
                    }
                }
                else
                {
                    dirX = 0f;
                }

                rb.velocity = new Vector2(dirX * (3f * hsp), rb.velocity.y);

                if (!wasGrounded && Grounded() && eState == EnemyState.alert) // Landing Sound Code
                    audioSrc.PlayOneShot(sndLand);

                wasGrounded = Grounded();
            }
            break;

            case EnemyState.attack:
            {
                rb.velocity = new Vector2(0f, 0f);

                if ((Math.Abs(player.transform.position.x - transform.position.x) >= 0.45f))
                {
                    float step = 4f * Time.deltaTime;
                    transform.position = Vector2.MoveTowards(transform.position, player.transform.position, step);
                }

                if (stateInfo.normalizedTime >= 1f)
                {
                    int hitIndex = UnityEngine.Random.Range(0, 3);

                    switch (hitIndex)
                    {
                        case 0: { alarm4 = 300; } break;
                        case 1: { alarm4 = 400; } break;
                        case 2: { alarm4 = 500; } break;
                    }
                    eState = EnemyState.alert;
                    kick = false;
                    rb.gravityScale = 1;
                }
            }
            break;
        }

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (dirX > 0f)
            sprite.flipX = false;
        else if (dirX < 0f)
            sprite.flipX = true;

        if (eState == EnemyState.hurt) return;
        if (eState == EnemyState.shocked) return;
        if (eState == EnemyState.attack) return;

        MovementState mstate = MovementState.idle;

        if (eState == EnemyState.normal)
        {
            if (dirX > 0f)                  // Controlling running animation by controlling boolean variable responsible for triggering running animation based on horizontal speed
                mstate = MovementState.running;
            else if (dirX < 0f)
                mstate = MovementState.running;
            else
                mstate = MovementState.idle;

            if (rb.velocity.y < -0.1f)
                mstate = MovementState.falling;
        }

        if (eState == EnemyState.alert)
        {
            if (dirX > 0f)                  // Controlling running animation by controlling boolean variable responsible for triggering running animation based on horizontal speed
                mstate = MovementState.sprinting;
            else if (dirX < 0f)
                mstate = MovementState.sprinting;
            else
                mstate = MovementState.alertidle;

            if (rb.velocity.y < -0.1f)
                mstate = MovementState.falling;
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        float normalizedTime = stateInfo.normalizedTime % 1f;

        if (mstate == MovementState.running)
        {
            if (normalizedTime >= 0.21f && normalizedTime <= 0.24f && !hasPlayedStep1)
            {
                audioSrc.PlayOneShot(sndStep);
                hasPlayedStep1 = true;
            }
            else if (normalizedTime >= 0.67f && normalizedTime <= 0.70f && !hasPlayedStep2)
            {
                audioSrc.PlayOneShot(sndStep);
                hasPlayedStep2 = true;
            }

            // Reset flags when the animation loops
            if (normalizedTime < 0.05f)
            {
                hasPlayedStep1 = false;
                hasPlayedStep2 = false;
            }
        }
        else
        {
            // Reset if not running
            hasPlayedStep1 = false;
            hasPlayedStep2 = false;
        }

        anim.SetInteger("mstate", (int)mstate);
    }

    public bool Grounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }

    private void OnDrawGizmosSelected()
    {

    }

    //Listened event from Player Animation
    void OnPlayerHit(RobotStep target)
    {
        if (target == this)
        {
            Debug.Log("Hit!");
            //StopEnemyCoroutines();
            //DamageCoroutine = StartCoroutine(HitCoroutine());

            // player.currentTarget = null;
            // isLockedTarget = false;
            //OnDamage.Invoke(this);
            float dir = 0;

            if (!player.sprite.flipX)
            {
                dir = 1f;
                dirX = -1f;
            }
            else
            {
                dir = -1f;
                dirX = 1f;
            }

            if (player.uppercut)
                rb.velocity = new Vector2(dir, 5f);
            else
                rb.velocity = new Vector2(dir, 0f);

            anim.speed = 1f;
            eState = EnemyState.hurt;
            MovementState mstate;

            if (player.uppercut)
            {
                mstate = MovementState.launched;
            }
            else
            {
                int hitIndex = UnityEngine.Random.Range(0, 2); // 0 or 1

                if (hitIndex == 0)
                    mstate = MovementState.hurt1;
                else
                    mstate = MovementState.hurt2;
            }

            anim.SetInteger("mstate", (int)mstate);

            Vector2 hitPoint = transform.position + new Vector3(0f, 0f); // Offset to torso or desired point
            player.SpawnHitEffect(hitPoint);
            //transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);

            //StopMoving();
        }
        /*
        IEnumerator HitCoroutine()
        {
            isStunned = true;
            yield return new WaitForSeconds(.5f);
            isStunned = false;
        }*/
    }

    public void AttackEvent()
    {
        player.Damage(this);
    }
}