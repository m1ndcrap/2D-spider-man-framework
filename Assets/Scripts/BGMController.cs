using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    [SerializeField] public AudioSource lightSource;
    [SerializeField] public AudioSource mediumSource;
    [SerializeField] public AudioSource intenseSource;

    public int intensity = 0;     // 0 = light, 1 = medium, 2 = intense

    private float lightVol = 1f;
    private float mediumVol = 0f;
    private float intenseVol = 0f;

    private float lightVolMax = 0.6f;
    private float mediumVolMax = 0.65f;
    private float intenseVolMax = 0.45f;

    private void Start()
    {
        // Ensure looping and start all tracks.
        lightSource.loop = true;
        mediumSource.loop = true;
        intenseSource.loop = true;

        lightSource.volume = lightVol;
        mediumSource.volume = mediumVol;
        intenseSource.volume = intenseVol;

        lightSource.Play();
        mediumSource.Play();
        intenseSource.Play();
    }

    private void Update()
    {
        // If any stopped (Unity can sometimes stop if not enough audio voices)
        if (!intenseSource.isPlaying)
        {
            lightSource.Play();
            mediumSource.Play();
            intenseSource.Play();
        }

        switch (intensity)
        {
            case 0:
                // Light fades up, others fade down
                lightVol = Mathf.MoveTowards(lightVol, lightVolMax, lightVolMax / 10f * Time.deltaTime * 60f);
                mediumVol = Mathf.MoveTowards(mediumVol, 0f, mediumVolMax / 10f * Time.deltaTime * 60f);
                intenseVol = Mathf.MoveTowards(intenseVol, 0f, 0.045f * Time.deltaTime * 60f);
                break;

            case 1:
                // Medium fades up
                lightVol = Mathf.MoveTowards(lightVol, 0f, lightVolMax / 10f * Time.deltaTime * 60f);
                mediumVol = Mathf.MoveTowards(mediumVol, mediumVolMax, mediumVolMax / 10f * Time.deltaTime * 60f);
                intenseVol = Mathf.MoveTowards(intenseVol, 0f, 0.045f * Time.deltaTime * 60f);
                break;

            case 2:
                // Intense fades up
                lightVol = Mathf.MoveTowards(lightVol, 0f, lightVolMax / 10f * Time.deltaTime * 60f);
                mediumVol = Mathf.MoveTowards(mediumVol, 0f, mediumVolMax / 10f * Time.deltaTime * 60f);
                intenseVol = Mathf.MoveTowards(intenseVol, intenseVolMax, 0.045f * Time.deltaTime * 60f);
                break;
        }

        // Apply volume changes
        lightSource.volume = lightVol;
        mediumSource.volume = mediumVol;
        intenseSource.volume = intenseVol;
    }
}