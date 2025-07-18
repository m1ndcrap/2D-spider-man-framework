using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpriteScript : MonoBehaviour
{
    [SerializeField] private PlayerStep player;
    public UnityEvent<RobotStep> OnHit;

    public void HitEvent()
    {
        player.HitEvent(); // Call the parent's HitEvent
    }

    public void PauseBeforeHit()
    {
        player.PauseBeforeHit(); // Call the parent's HitEvent
    }
}
