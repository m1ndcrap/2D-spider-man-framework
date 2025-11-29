using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RobotStep;

public class HostageScript : MonoBehaviour
{
    private int civ = 1;
    private string civScaredAnim = "Civ1_Scared";
    private string civThanksAnim = "Civ1_Thanks1";
    [SerializeField] GameObject trigger;
    public int phase = 0;
    private bool rescued = false;
    private int alarm1 = 0;
    private int alarm2 = 300;
    private SpriteRenderer sr;
    private bool fading = false;
    private float fadeSpeed = 1.5f;
    private AudioSource audioSrc;
    [SerializeField] private AudioClip sndHelp;
    [SerializeField] private AudioClip sndThanks1;
    [SerializeField] private AudioClip sndThanks2;
    [SerializeField] private AudioClip sndThanks3;
    [SerializeField] private AudioClip sndThanks4;
    [SerializeField] private AudioClip sndThanks5;
    [SerializeField] private AudioClip sndThanks6;
    [SerializeField] private AudioClip sndThanks7;
    [SerializeField] private AudioClip sndThanks8;
    private Animator anim;
    [SerializeField] private PlayerStep player;

    void Start()
    {
        civ = UnityEngine.Random.Range(1, 6);
        sr = GetComponent<SpriteRenderer>();
        audioSrc = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        switch (civ)
        {
            case 1:
            {
                civScaredAnim = "Civ1_Scared";
                string[] anims = { "Civ1_Thanks1", "Civ1_Thanks2" };
                civThanksAnim = anims[UnityEngine.Random.Range(0, anims.Length)];
            }
            break;

            case 2:
            {
                civScaredAnim = "Civ2_Scared";
                string[] anims = { "Civ2_Thanks", "Civ2_Thanks2" };
                civThanksAnim = anims[UnityEngine.Random.Range(0, anims.Length)];
            }
            break;

            case 3:
            {
                civScaredAnim = "Civ3_Scared";
                string[] anims = { "Civ3_Thanks1", "Civ3_Thanks2" };
                civThanksAnim = anims[UnityEngine.Random.Range(0, anims.Length)];
            }
            break;

            case 4:
            {
                civScaredAnim = "Civ4_Scared";
                string[] anims = { "Civ4_Thanks1", "Civ4_Thanks2" };
                civThanksAnim = anims[UnityEngine.Random.Range(0, anims.Length)];
            }
            break;

            case 5:
            {
                civScaredAnim = "Civ5_Scared";
                string[] anims = { "Civ5_Thanks1", "Civ5_Thanks2" };
                civThanksAnim = anims[UnityEngine.Random.Range(0, anims.Length)];
            }
            break;
        }

        if (phase == 0) { anim.Play(civScaredAnim); }

        if (alarm1 > 0)
            alarm1 -= 1;
        else
        {
            if (phase == 1)
            {
                anim.Play(civThanksAnim);
                if (audioSrc.isPlaying && audioSrc.clip == sndHelp) { audioSrc.Stop(); }
                AudioClip[] clips = { sndThanks1, sndThanks2, sndThanks3, sndThanks4, sndThanks5, sndThanks6, sndThanks7, sndThanks8 };
                audioSrc.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
                phase = 2;
            }
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (phase == 2 && stateInfo.IsName(civThanksAnim) && stateInfo.normalizedTime >= 1f)
            phase = 3;

        if (alarm2 > 0)
            alarm2 -= 1;
        else
        {
            if ((Math.Abs(transform.position.x - player.transform.position.x) <= 5f) && phase == 0)
            {
                AudioClip[] clips = { sndHelp };
                int index = UnityEngine.Random.Range(0, clips.Length + 1);

                if (index < clips.Length)
                    audioSrc.PlayOneShot(clips[index]);
            }

            alarm2 = 300;
        }

        if (trigger != null)
        {
            if (trigger.CompareTag("Enemy"))
            {
                if (phase == 0)
                {
                    if (trigger.GetComponent<RobotStep>().eState == RobotStep.EnemyState.death)
                    {
                        phase = 1;
                    }
                }
            }

            if (trigger.CompareTag("Lightning"))
            {
                if (phase == 0)
                {
                    if (trigger.GetComponent<LightningScript>().phase == 3)
                    {
                        phase = 1;
                    }
                }
            }

            if (trigger.CompareTag("Door"))
            {
                if (phase == 0)
                {
                    if (trigger.GetComponent<BreakableDoor>().phase == 2)
                    {
                        phase = 1;
                    }
                }
            }
        }

        if (phase == 1 && !rescued)
        {
            alarm1 = 10;
            rescued = true;
        }

        if (phase == 3 && !fading)
        {
            fading = true;
        }

        if (fading)
        {
            Color c = sr.color;
            c.a -= Time.deltaTime / fadeSpeed;
            sr.color = c;

            if (c.a <= 0f)
            {
                c.a = 0f;
                sr.color = c;
                fading = false;
                Destroy(gameObject); // gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (trigger.CompareTag("Player"))
        {
            if (collision.gameObject.CompareTag("Player") && phase == 0)
            {
                phase = 1;
            }
        }
    }
}