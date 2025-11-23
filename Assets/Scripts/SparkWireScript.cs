using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerStep;

public class SparkWireScript : MonoBehaviour
{
    public int alarm1 = 180;
    public int alarm2 = 0;
    public int wirePhase = 0;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip sndElectric;

    void Update()
    {
        if (alarm1 > 0)
        {
            alarm1 -= 1;
        }
        else
        {
            if (wirePhase == 0)
            {
                alarm2 = 120;
                wirePhase = 1;
            }
        }
        
        if (alarm2 > 0)
        {
            alarm2 -= 1;
        }
        else
        {
            if (wirePhase == 2)
            {
                alarm1 = 180;
                audioSrc.Stop();
                wirePhase = 0;
            }
            else if (wirePhase == 1)
            {
                alarm2 = 60;
            }
        }

        if (wirePhase == 1)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("WiresStart") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                audioSrc.PlayOneShot(sndElectric);
                wirePhase = 2;
            }
        }

        anim.SetInteger("state", wirePhase);
    }
}
