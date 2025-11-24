using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningScript : MonoBehaviour
{
    [SerializeField] GameObject trigger;
    public int phase = 0;
    private bool destroyed = false;
    private int alarm1 = 0;
    private AudioSource audioSrc;
    [SerializeField] private AudioClip sndElectric;
    private Animator anim;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (phase == 0)
        {
            anim.Play("LightningActive");

            if (!audioSrc.isPlaying)
                audioSrc.PlayOneShot(sndElectric);
        }

        if (alarm1 > 0)
            alarm1 -= 1;
        else
        {
            if (phase == 1)
            {
                anim.Play("LightningTurnOff");
                audioSrc.Stop();
                phase = 2;
            }
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (phase == 2 && stateInfo.IsName("LightningTurnOff") && stateInfo.normalizedTime >= 1f) { phase = 3; }

        if (trigger != null)
        {
            if (trigger.CompareTag("Switch"))
            {
                if (trigger.GetComponent<SwitchScript>().phase == 2)
                {
                    phase = 1;
                }
            }

            if (trigger.CompareTag("Generator"))
            {
                if (trigger.GetComponent<GeneratorScript>().phase == 2)
                {
                    phase = 1;
                }
            }
        }

        if (phase == 1 && !destroyed)
        {
            alarm1 = 10;
            destroyed = true;
        }

        if (phase == 3) { anim.Play("LightningInactive"); }
    }
}