using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootScript : MonoBehaviour
{
    [Range(1, 10)]
    [SerializeField] private float speed = 10f;

    [Range(1, 10)]
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody2D rb;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip sndWebDestroy;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.right * speed; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            audioSrc.PlayOneShot(sndWebDestroy);
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Enemy"))
        {
            RobotStep enemy = other.GetComponent<RobotStep>();
            enemy.eState = RobotStep.EnemyState.webbed;
            enemy.anim.SetInteger("mstate", 13);
            enemy.alarm5 = 240;
            audioSrc.PlayOneShot(sndWebDestroy);
            Destroy(gameObject);
        }
    }
}