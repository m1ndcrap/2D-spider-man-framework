using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    public int phase = 0;
    private bool destroyed = false;
    private Animator anim;
    private AudioSource audioSrc;
    [SerializeField] private AudioClip sndExplosion1;
    [SerializeField] private AudioClip sndExplosion2;
    private int alarm1 = 0;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (phase == 0) { anim.Play("GeneratorActive"); }

        if (phase == 1 && !destroyed)
        {
            alarm1 = 10;
            destroyed = true;
        }

        if (alarm1 > 0)
            alarm1 -= 1;
        else
        {
            if (phase == 1)
            {
                anim.Play("GeneratorBreak");
                AudioClip[] clips = { sndExplosion1, sndExplosion2 };
                audioSrc.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
                phase = 2;
            }
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (phase == 2 && stateInfo.IsName("GeneratorBreak") && stateInfo.normalizedTime >= 1f) { phase = 3; }

        if (phase == 3) { anim.Play("GeneratorInactive"); }
    }
}