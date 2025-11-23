using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BreakableCar : MonoBehaviour
{
    void Update()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CarBreak"))
        {
            if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                GetComponent<Animator>().Play("CarBroken");
            }
        }
    }
}
