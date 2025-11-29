using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static PlayerStep;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class RobotStep : MonoBehaviour
{
    public Rigidbody2D rb;
    [SerializeField] public Animator anim;
    [SerializeField] public SpriteRenderer sprite;
    private BoxCollider2D coll;
    private float lastspd = 0f;
    [SerializeField] private float dirX = 0f;
    [SerializeField] private bool setCustomStartingDir = false;
    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] public float hsp = 1f; // Horizontal speed
    [SerializeField] private int waitTime = 120;
    private Camera cam;

    public enum MovementState { idle, running, falling, hurt1, hurt2, launched, shocked, sprinting, alertidle, punch1, punch2, kick, backstep, webbed, death, breakfree }
    public enum EnemyState { normal, death, hurt, shocked, alert, attack, webbed }
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
    [SerializeField] private AudioClip sndWebbedStruggle;
    [SerializeField] private AudioClip sndWebbedEscape;
    private AudioClip sndQuickHit;
    private AudioClip sndQuickHit2;
    private AudioClip sndStrongHit;
    private AudioClip sndStrongHit2;
    private AudioClip sndCarBreak;
    private bool wasGrounded = false;
    private bool hasPlayedStep1;
    private bool hasPlayedStep2;

    // Alarms
    private int alarm1;
    private int alarm2 = 0;
    [SerializeField] private int alarm3 = 0;
    [SerializeField] public int alarm4 = 0;
    public int alarm5 = 0;
    [SerializeField] private int alarm6 = 0;
    private bool startAlarm1 = true;
    private bool startAlarm2 = false;
    private bool startAlarm6 = false;
    [SerializeField] private float distanceFromPlayer = 0f;
    public int alarm7 = 0;

    // Combat
    private Material outline;
    [SerializeField] private PlayerStep player;
    [SerializeField] private bool noHitWall;
    private bool noHitHazard;
    [SerializeField] private bool shocked = false;
    public UnityEvent<PlayerStep> OnAttack;
    public bool kick = false;
    public bool attacking = false;
    public bool collidedWithPlayer = false;
    private bool backstep = false;
    [SerializeField] private bool breakingWeb = false;
    [SerializeField] private GameObject hitParticlePrefab;

    // health bar
    private int health = 55;
    private int maxHealth = 55;
    HealthBar healthbar;

    // specialized vars for level objects
    private float wireHitCooldown = 0f;
    private bool wireWasActive = false;
    private float lightningHitCooldown = 0f;
    private bool lightningWasActive = false;
    private float hitCooldown = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        eState = EnemyState.normal;
        if (!setCustomStartingDir) { dirX = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1; }
        alarm1 = waitTime;
        outline = sprite.material;
        player.OnHit.AddListener((x) => OnPlayerHit(x));
        sndQuickHit = player.sndQuickHit;
        sndQuickHit2 = player.sndQuickHit2;
        sndStrongHit = player.sndStrongHit;
        sndStrongHit2 = player.sndStrongHit2;
        sndCarBreak = player.sndCarBreak;
        healthbar = GetComponentInChildren<HealthBar>();
        healthbar.UpdateHealthBar(health, maxHealth);
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsInsideExtendedView(3.5f))
        {
            return;
        }

        //Debug.Log(anim.speed);
        //Debug.DrawRay(transform.position, (player.transform.position - transform.position).normalized * (player.transform.position - transform.position).normalized.magnitude, Color.red);

        // Outline Shader Color Control
        if (eState == EnemyState.attack) { outline.color = Color.red; }
        else if (player.currentTarget == this) { outline.color = Color.white; }
        else { outline.color = Color.black; }

        collidedWithPlayer = Physics2D.Raycast(transform.position, transform.right * -dirX, 0.65f, playerMask);

        if (eState == EnemyState.hurt || player.pState == PlayerStep.PlayerState.dashenemy || eState == EnemyState.attack || (player.transform.position.y - transform.position.y > 0.015f && Grounded()))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), true);
        else
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), false);

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        distanceFromPlayer = Vector3.Distance(player.transform.position, transform.position);
        noHitWall = !Physics2D.Raycast(transform.position, (player.transform.position - transform.position).normalized, distanceFromPlayer, jumpableGround);


        Vector2 start = transform.position;
        Vector2 end = player.transform.position;
        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end);
        noHitHazard = true;
        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                LightningScript lightning = hit.collider.GetComponent<LightningScript>();
                if (lightning != null && lightning.phase == 0)
                {
                    noHitHazard = false;
                    break;
                }
            }
        }

        if (startAlarm1)
        {
            if (alarm1 > 0)
                alarm1 -= 1;
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
                alarm2 -= 1;
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
            alarm3 -= 1;
        else
        {
            if (shocked && eState != EnemyState.attack && eState != EnemyState.webbed)
            {
                if ((distanceFromPlayer <= 3f) && ((!sprite.flipX && transform.position.x < player.transform.position.x) || (sprite.flipX && transform.position.x > player.transform.position.x)) && noHitWall)
                    alarm3 = 300;
                else
                    shocked = false;
            }
            
            if (eState == EnemyState.attack || eState == EnemyState.webbed)
            {
                shocked = true;
                alarm3 = 3;
            }
        }

        if (alarm4 > 0)
            alarm4 -= 4;
        else
        {
            if (eState == EnemyState.alert)
            {
                if (distanceFromPlayer >= 4.5f || !noHitWall)
                {
                    eState = EnemyState.normal;
                }

                if ((!player.isEnemyAttacking) && (Vector3.Distance(player.transform.position, transform.position) <= 2.05f) && ((!sprite.flipX && transform.position.x < player.transform.position.x) || (sprite.flipX && transform.position.x > player.transform.position.x)) && noHitWall)
                {
                    eState = EnemyState.attack;
                    AudioClip[] clips = { sndAttack, sndAttack2 };
                    int index = UnityEngine.Random.Range(0, clips.Length);
                    if (index < clips.Length) { audioSrc.PlayOneShot(clips[index]); }
                    rb.gravityScale = 0;
                    int hitIndex = UnityEngine.Random.Range(0, 3); // random number 0-6
                    MovementState mstate = MovementState.idle;

                    switch (hitIndex)
                    {
                        case 0: { mstate = MovementState.punch1; anim.speed = 0.6f; } break;
                        case 1: { mstate = MovementState.punch2; anim.speed = 0.6f; } break;
                        case 2: { mstate = MovementState.kick; kick = true; anim.speed = 1f; } break;
                    }

                    anim.SetInteger("mstate", (int)mstate);
                    player.isEnemyAttacking = true;
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

        if (alarm5 > 0)
            alarm5 -= 1;
        else
        {
            if (eState == EnemyState.webbed && !breakingWeb)
            {
                if (audioSrc.isPlaying && audioSrc.clip == sndWebbedStruggle) { audioSrc.Stop(); }
                audioSrc.PlayOneShot(sndWebbedEscape);
                anim.SetInteger("mstate", 15);
                breakingWeb = true;
            }
        }

        if (startAlarm6)
        {
            if (alarm6 > 0)
            {
                alarm6 -= 1;
            }
            else
            {
                if (eState == EnemyState.death)
                {
                    Destroy(gameObject);
                }
            }
        }

        if (alarm7 > 0)
        {
            alarm7 -= 1;
        }
        else
        {
            if (eState == EnemyState.webbed && !breakingWeb)
            {
                AudioClip[] clips = { sndWebbedStruggle };
                int index = UnityEngine.Random.Range(0, clips.Length + 1); ;
                if (index < clips.Length) { audioSrc.PlayOneShot(clips[index]); }
                alarm7 = 30;
            }
        }

        if (health <= 0)
        {
            eState = EnemyState.death;
        }

        if (eState != EnemyState.webbed)
            breakingWeb = false;

        switch (eState)
        {
            case EnemyState.normal:
            {
                rb.velocity = new Vector2(dirX * hsp, rb.velocity.y);

                if ((((Math.Abs(transform.position.x - player.transform.position.x) <= 3f) && ((!sprite.flipX && transform.position.x < player.transform.position.x) || (sprite.flipX && transform.position.x > player.transform.position.x))) || collidedWithPlayer) && !shocked && Grounded() && noHitWall && noHitHazard)
                {
                    eState = EnemyState.shocked;
                    AudioClip[] clips = { sndAlert, sndAlert2, sndAlert3 };
                    int index = UnityEngine.Random.Range(0, clips.Length);
                    if (index < clips.Length) { audioSrc.PlayOneShot(clips[index]); }
                    MovementState mstate = MovementState.shocked;
                    anim.SetInteger("mstate", (int)mstate);
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    anim.speed = 1f;
                    shocked = true;
                    alarm3 = 300;
                    collidedWithPlayer = false;
                }

                if (!wasGrounded && Grounded() && eState == EnemyState.normal)
                    audioSrc.PlayOneShot(sndLand);

                wasGrounded = Grounded();
            }
            break;

            case EnemyState.hurt:
            {
                if ((stateInfo.IsName("Enemy_Launched")))
                {
                    if (stateInfo.normalizedTime >= 1f) {anim.speed = 0f;}
                    
                    if (Grounded())
                    {
                        anim.speed = 1f;
                        eState = EnemyState.alert;
                    }
                }
                else
                {
                    anim.speed = 1f;
                    if ((stateInfo.IsName("Enemy_Hit1") && stateInfo.normalizedTime >= 1f) || (stateInfo.IsName("Enemy_Hit2") && stateInfo.normalizedTime >= 1f)) { eState = EnemyState.alert; }
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
                    backstep = false;

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
                else if (Math.Abs(transform.position.x - player.transform.position.x) < 1.7f)
                {
                    backstep = true;

                    if (transform.position.x < player.transform.position.x)
                    {
                        dirX = -0.6f;
                        sprite.flipX = false;
                    }
                    else
                    {
                        dirX = 0.6f;
                        sprite.flipX = true;
                    }
                }
                else
                {
                    backstep = false;
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

                if (Math.Abs(player.transform.position.x - transform.position.x) >= 0.45f && ((stateInfo.IsName("Enemy_Kick") && stateInfo.normalizedTime <= 0.31f) || (stateInfo.IsName("Enemy_Punch1") && stateInfo.normalizedTime <= 0.45f) || (stateInfo.IsName("Enemy_Punch2") && stateInfo.normalizedTime <= 0.29f)))
                {
                    float step = 4f * Time.deltaTime;
                    Vector2 targetPosition = new Vector2(player.transform.position.x, transform.position.y);
                    transform.position = Vector2.MoveTowards(transform.position, targetPosition, step);
                    if (targetPosition.x < transform.position.x) { sprite.flipX = true; } else { sprite.flipX = false; }
                }

                if ((stateInfo.IsName("Enemy_Punch1") && stateInfo.normalizedTime >= 1f) || (stateInfo.IsName("Enemy_Punch2") && stateInfo.normalizedTime >= 1f) || (stateInfo.IsName("Enemy_Kick") && stateInfo.normalizedTime >= 1f))
                {
                    int hitIndex = UnityEngine.Random.Range(0, 3);

                    switch (hitIndex)
                    {
                        case 0: { alarm4 = 300; } break;
                        case 1: { alarm4 = 400; } break;
                        case 2: { alarm4 = 500; } break;
                    }

                    eState = EnemyState.alert;
                    player.isEnemyAttacking = false;
                    anim.speed = 1f;
                    kick = false;
                    rb.gravityScale = 1;
                }
            }
            break;

            case EnemyState.webbed:
            {
                rb.velocity = new Vector2(0f, 0f);
                shocked = true;

                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Enemy_BreakFree") && (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f))
                {
                    eState = EnemyState.alert;
                }
            }
            break;
        }

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (!((eState == EnemyState.alert && backstep) || (eState == EnemyState.attack)))
        {
            if (dirX > 0f)
                sprite.flipX = false;
            else if (dirX < 0f)
                sprite.flipX = true;
        }

        if (eState == EnemyState.hurt) return;
        if (eState == EnemyState.webbed) return;
        if (eState == EnemyState.shocked) return;
        if (eState == EnemyState.attack) return;
        MovementState mstate = MovementState.idle;

        if (eState == EnemyState.normal)
        {
            if (dirX > 0f)
                mstate = MovementState.running;
            else if (dirX < 0f)
                mstate = MovementState.running;
            else
                mstate = MovementState.idle;

            if (rb.velocity.y < -0.1f) { mstate = MovementState.falling; }
        }

        if (eState == EnemyState.alert)
        {
            if (dirX > 0f)
                if (backstep) mstate = MovementState.backstep; else mstate = MovementState.sprinting;
            else if (dirX < 0f)
                if (backstep) mstate = MovementState.backstep; else mstate = MovementState.sprinting;
            else
                mstate = MovementState.alertidle;

            if (rb.velocity.y < -0.1f) { mstate = MovementState.falling; }
        }

        if (eState == EnemyState.death)
        {
            anim.speed = 1f;
            mstate = MovementState.death;
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

            if (normalizedTime < 0.05f)
            {
                hasPlayedStep1 = false;
                hasPlayedStep2 = false;
            }
        }
        else if (mstate == MovementState.sprinting)
        {
            if (normalizedTime >= 0.45f && normalizedTime <= 0.55f && !hasPlayedStep1)
            {
                audioSrc.PlayOneShot(sndStep);
                hasPlayedStep1 = true;
            }
            else if (normalizedTime >= 0.90f && normalizedTime <= 1.00f && !hasPlayedStep2)
            {
                audioSrc.PlayOneShot(sndStep);
                hasPlayedStep2 = true;
            }

            if (normalizedTime < 0.05f)
            {
                hasPlayedStep1 = false;
                hasPlayedStep2 = false;
            }
        }
        else if (mstate == MovementState.backstep)
        {
            if (normalizedTime >= 0.60f && normalizedTime <= 0.68f && !hasPlayedStep1)
            {
                audioSrc.PlayOneShot(sndStep);
                hasPlayedStep1 = true;
            }
            else if (normalizedTime >= 0.90f && normalizedTime <= 1.00f && !hasPlayedStep2)
            {
                audioSrc.PlayOneShot(sndStep);
                hasPlayedStep2 = true;
            }

            if (normalizedTime < 0.05f)
            {
                hasPlayedStep1 = false;
                hasPlayedStep2 = false;
            }
        }
        else
        {
            hasPlayedStep1 = false;
            hasPlayedStep2 = false;
        }

        if (mstate == MovementState.death)
        {
            if (normalizedTime >= 0.352f && normalizedTime <= 0.389f)
            {
                if (Grounded()) audioSrc.PlayOneShot(sndLand);

                if (!startAlarm6)
                {
                    alarm6 = 240;
                    startAlarm6 = true;
                }
            }

            if (normalizedTime == 1f)
            {
                anim.speed = 0f;
                normalizedTime = 1f;
            }
        }

        anim.SetInteger("mstate", (int)mstate);
    }

    public bool Grounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }

    public void OnPlayerHit(RobotStep target)
    {
        player.isEnemyAttacking = false;

        if (target == this)
        {
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
            else if ((player.combo - 4) % 5 == 0)
                rb.velocity = new Vector2(2.5f * dir, 0f);
            else
                rb.velocity = new Vector2(dir, 0f);

            anim.speed = 1f;
            eState = EnemyState.hurt;

            int attackTime = UnityEngine.Random.Range(0, 3);

            switch (attackTime)
            {
                case 0: { alarm4 = 300; } break;
                case 1: { alarm4 = 400; } break;
                case 2: { alarm4 = 500; } break;
            }

            MovementState mstate;

            if (player.uppercut)
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

                if ((player.combo - 4) % 5 == 0)
                {
                    AudioClip[] clips2 = { sndStrongHit, sndStrongHit2, };
                    int index2 = UnityEngine.Random.Range(0, clips2.Length);
                    if (index2 < clips2.Length) { audioSrc.PlayOneShot(clips2[index2]); }
                }
                else
                {
                    AudioClip[] clips2 = { sndQuickHit, sndQuickHit2 };
                    int index2 = UnityEngine.Random.Range(0, clips2.Length);
                    if (index2 < clips2.Length) { audioSrc.PlayOneShot(clips2[index2]); }
                }
            }

            anim.SetInteger("mstate", (int)mstate);
            Vector2 hitPoint = transform.position;
            player.SpawnHitEffect(hitPoint);

            if (health > 0)
            {
                if ((player.combo - 4) % 5 == 0)
                    health -= 7;
                else if (player.countering)
                    health -= 3;
                else if (player.uppercut)
                    health -= 5;
                else
                    health -= 4;

                healthbar.UpdateHealthBar(health, maxHealth);
            }

            AudioClip[] clips = { sndHit, sndHit2, sndHit3 };
            int index = UnityEngine.Random.Range(0, clips.Length);
            if (index < clips.Length) { audioSrc.PlayOneShot(clips[index]); }
        }
    }

    public void AttackEvent()
    {
        Debug.Log(Vector3.Distance(player.transform.position, transform.position));
        if (Vector3.Distance(player.transform.position, transform.position) <= 0.45f) { player.Damage(this); }
    }
    public bool IsOnScreen(Camera cam)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        return viewportPos.x > 0 && viewportPos.x < 1 &&
               viewportPos.y > 0 && viewportPos.y < 1 &&
               viewportPos.z > 0;
    }

    public void SpawnObjectHitEffect(Vector2 impactPoint, GameObject other)
    {
        Vector3 hitPosition = (transform.position + other.transform.position) / 2f;
        GameObject hitFX = Instantiate(hitParticlePrefab, impactPoint, Quaternion.identity);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {
            rb.WakeUp();

            Animator carAnim = collision.GetComponent<Animator>();
            bool carNormal = carAnim.GetCurrentAnimatorStateInfo(0).IsName("CarNormal");
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            if (carNormal && eState == EnemyState.attack && ((transform.position.x > collision.transform.position.x && sprite.flipX) || (transform.position.x < collision.transform.position.x && !sprite.flipX)) && ((stateInfo.IsName("Enemy_Kick") && stateInfo.normalizedTime <= 0.31f) || (stateInfo.IsName("Enemy_Punch1") && stateInfo.normalizedTime <= 0.45f) || (stateInfo.IsName("Enemy_Punch2") && stateInfo.normalizedTime <= 0.29f)))
            {
                rb.WakeUp();
                rb.position = rb.position;
                audioSrc.PlayOneShot(sndCarBreak);
                collision.GetComponent<Animator>().Play("CarBreak");
            }
        }

        if (collision.gameObject.CompareTag("Wires"))
        {
            rb.WakeUp();
            if (eState == EnemyState.death) return;

            Animator wireAnim = collision.GetComponent<Animator>();
            bool wireIsActive = wireAnim.GetCurrentAnimatorStateInfo(0).IsName("WiresActive");

            if (wireIsActive && !wireWasActive)
            {
                wireHitCooldown = 0f;
                rb.WakeUp();
                rb.position = rb.position;
            }

            wireWasActive = wireIsActive;

            if (!wireIsActive)
            {
                wireHitCooldown = 0f;
                return;
            }

            if (wireHitCooldown > 0f)
            {
                wireHitCooldown -= Time.deltaTime;
                return;
            }

            wireHitCooldown = 0.05f;

            float dir = sprite.flipX ? 1 : -1;
            rb.velocity = new Vector2(dir, 5f);
            anim.speed = 1f;
            eState = EnemyState.hurt;

            int attackTime = UnityEngine.Random.Range(0, 3);

            switch (attackTime)
            {
                case 0: { alarm4 = 300; } break;
                case 1: { alarm4 = 400; } break;
                case 2: { alarm4 = 500; } break;
            }

            MovementState mstate = MovementState.launched;

            AudioClip[] clips2 = { sndStrongHit, sndStrongHit2 };
            audioSrc.PlayOneShot(clips2[UnityEngine.Random.Range(0, clips2.Length)]);

            anim.SetInteger("mstate", (int)mstate);

            Vector2 hitPoint = transform.position;
            SpawnObjectHitEffect(hitPoint, collision.gameObject);

            health -= 8;
            healthbar.UpdateHealthBar(health, maxHealth);
        }

        if (collision.gameObject.CompareTag("Lightning"))
        {
            rb.WakeUp();
            if (eState == EnemyState.death) return;

            Animator wireAnim = collision.GetComponent<Animator>();
            bool wireIsActive = wireAnim.GetCurrentAnimatorStateInfo(0).IsName("LightningActive");

            if (wireIsActive && !lightningWasActive)
            {
                lightningHitCooldown = 0f;
                rb.WakeUp();
                rb.position = rb.position;
            }

            lightningWasActive = wireIsActive;

            if (!wireIsActive)
            {
                return;
            }

            if (lightningHitCooldown > 0f)
            {
                lightningHitCooldown -= Time.deltaTime;
                return;
            }

            lightningHitCooldown = 0.05f;

            float dir = sprite.flipX ? 1 : -1;
            rb.velocity = new Vector2(dir, 5f);
            anim.speed = 1f;
            eState = EnemyState.hurt;

            int attackTime = UnityEngine.Random.Range(0, 3);

            switch (attackTime)
            {
                case 0: { alarm4 = 300; } break;
                case 1: { alarm4 = 400; } break;
                case 2: { alarm4 = 500; } break;
            }

            MovementState mstate = MovementState.launched;

            AudioClip[] clips2 = { sndStrongHit, sndStrongHit2 };
            audioSrc.PlayOneShot(clips2[UnityEngine.Random.Range(0, clips2.Length)]);

            anim.SetInteger("mstate", (int)mstate);

            Vector2 hitPoint = transform.position;
            SpawnObjectHitEffect(hitPoint, collision.gameObject);

            health -= 8;
            healthbar.UpdateHealthBar(health, maxHealth);
        }

        if (collision.gameObject.CompareTag("OneHitHazard"))
        {
            if (eState == EnemyState.death) return;

            rb.WakeUp();
            rb.position = rb.position;

            if (hitCooldown > 0f)
            {
                hitCooldown -= Time.deltaTime;
                return;
            }

            hitCooldown = 0.15f;

            float dir = sprite.flipX ? 1f : -1f;
            rb.velocity = new Vector2(dir * 2f, 5f);
            anim.speed = 1f;
            eState = EnemyState.hurt;

            MovementState mstate;
            int hitIndex = UnityEngine.Random.Range(0, 2); // 0 or 1

            if (hitIndex == 0)
                mstate = MovementState.hurt1;
            else
                mstate = MovementState.hurt2;

            AudioClip[] clips2 = { sndQuickHit, sndQuickHit2 };
            int index2 = UnityEngine.Random.Range(0, clips2.Length);
            if (index2 < clips2.Length) { audioSrc.PlayOneShot(clips2[index2]); }

            anim.SetInteger("mstate", (int)mstate);

            Vector2 hitPoint = transform.position;
            SpawnObjectHitEffect(hitPoint, collision.gameObject);

            health -= 8;
            healthbar.UpdateHealthBar(health, maxHealth);
        }
    }

    bool IsInsideExtendedView(float extensionFactor)
    {
        if (!cam) return false;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float extWidth = camWidth * extensionFactor;
        float extHeight = camHeight * extensionFactor;

        Vector3 camPos = cam.transform.position;

        float minX = camPos.x - extWidth / 2f;
        float maxX = camPos.x + extWidth / 2f;
        float minY = camPos.y - extHeight / 2f;
        float maxY = camPos.y + extHeight / 2f;

        Bounds b = sprite.bounds;

        bool overlap = b.max.x >= minX && b.min.x <= maxX && b.max.y >= minY && b.min.y <= maxY;

        return overlap;
    }
}