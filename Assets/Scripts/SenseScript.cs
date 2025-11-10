using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SenseScript : MonoBehaviour
{
    [SerializeField] private PlayerStep player;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerStep>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player.currentCounter == null || player.pState == PlayerStep.PlayerState.hurt)
        {
            Destroy(gameObject);
            return;
        }

        // cache the mstate and normalizedTime
        int mstate = player.anim.GetInteger("mstate");
        float normalizedTime = player.anim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
        float dir = 1f;
        if (player.sprite.flipX) dir = -1f; else dir = 1f;

        switch (mstate)
        {
            // positioning when idle
            case 0:
                transform.position = ToWorld(player.transform, new Vector2(0.023f * dir, 0.284f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            
            // positioning when running
            case 1:
                transform.position = ToWorld(player.transform, new Vector2(0.125f * dir, 0.248f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // positioning when jumping
            case 2:
                if (normalizedTime >= 0f && normalizedTime < 0.077f) transform.position = ToWorld(player.transform, new Vector2(-0.014f * dir, 0.348f));
                else if (normalizedTime >= 0.077f && normalizedTime < 0.231f) transform.position = ToWorld(player.transform, new Vector2(-0.032f * dir, 0.348f));
                else if (normalizedTime >= 0.231f && normalizedTime < 0.308f) transform.position = ToWorld(player.transform, new Vector2(-0.01f * dir, 0.348f));
                else if (normalizedTime >= 0.308f && normalizedTime < 0.385f) transform.position = ToWorld(player.transform, new Vector2(0.027f * dir, 0.348f));
                else if (normalizedTime >= 0.385f && normalizedTime < 0.462f) transform.position = ToWorld(player.transform, new Vector2(0.079f * dir, 0.343f));
                else if (normalizedTime >= 0.462f && normalizedTime < 0.538f) transform.position = ToWorld(player.transform, new Vector2(0.116f * dir, 0.325f));
                else if (normalizedTime >= 0.538f && normalizedTime < 0.615f) transform.position = ToWorld(player.transform, new Vector2(0.137f * dir, 0.328f));
                else if (normalizedTime >= 0.615f && normalizedTime < 0.692f) transform.position = ToWorld(player.transform, new Vector2(0.166f * dir, 0.307f));
                else if (normalizedTime >= 0.692f && normalizedTime < 0.769f) transform.position = ToWorld(player.transform, new Vector2(0.184f * dir, 0.307f));
                else if (normalizedTime >= 0.769f && normalizedTime < 0.846f) transform.position = ToWorld(player.transform, new Vector2(0.197f * dir, 0.297f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f * dir, 0.289f));
                else transform.position = ToWorld(player.transform, new Vector2(0.200f * dir, 0.278f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // positioning when falling
            case 3:
                transform.position = ToWorld(player.transform, new Vector2(0.183f * dir, 0.210f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // positioning when swinging
            case 4:
                if (normalizedTime >= 0f && normalizedTime < 0.25f) transform.position = ToWorld(player.transform, new Vector2(-0.146f * dir, -0.063f));
                else if (normalizedTime >= 0.25f && normalizedTime < 0.50f) transform.position = ToWorld(player.transform, new Vector2(-0.146f * dir, -0.017f));
                else if (normalizedTime >= 0.50f && normalizedTime < 0.75f) transform.position = ToWorld(player.transform, new Vector2(-0.143f * dir, -0.052f));
                else if (normalizedTime >= 0.75f && normalizedTime < 0.906f) transform.position = ToWorld(player.transform, new Vector2(-0.103f * dir, -0.081f));
                else transform.position = ToWorld(player.transform, new Vector2(-0.08f * dir, -0.087f));
                transform.rotation = player.transform.rotation;
                break;

            // position when ending swing
            case 5:
                transform.position = ToWorld(player.transform, new Vector2(0.109f * dir, 0.235f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when crawling
            case 6:
                transform.position = ToWorld(player.transform, new Vector2(0.266f * dir, -0.093f));
                transform.rotation = player.transform.rotation;
                break;

            // position when zipping
            case 7:
                transform.position = ToWorld(player.transform, new Vector2(-0.315f * dir, -0.133f));
                transform.rotation = Quaternion.Euler(0f, 0f, player.transform.rotation.z + (38.84f * dir));
                break;

            // position when aiming on ground
            case 8:
                if (normalizedTime >= 0f && normalizedTime < 0.333f) transform.position = ToWorld(player.transform, new Vector2(0.042f * dir, 0.285f));
                else if (normalizedTime >= 0.333f && normalizedTime < 0.667f) transform.position = ToWorld(player.transform, new Vector2(-0.098f * dir, 0.287f));
                else transform.position = ToWorld(player.transform, new Vector2(-0.229f * dir, 0.231f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when aiming in air
            case 9:
                if (normalizedTime >= 0f && normalizedTime < 0.333f) transform.position = ToWorld(player.transform, new Vector2(0.042f * dir, 0.285f));
                else if (normalizedTime >= 0.333f && normalizedTime < 0.667f) transform.position = ToWorld(player.transform, new Vector2(-0.098f * dir, 0.287f));
                else transform.position = ToWorld(player.transform, new Vector2(-0.229f * dir, 0.231f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when aiming during crawl
            case 10:
                if (normalizedTime >= 0f && normalizedTime < 0.333f) transform.position = ToWorld(player.transform, new Vector2(-0.052f * dir, 0.007f));
                else if (normalizedTime >= 0.333f && normalizedTime < 0.667f) transform.position = ToWorld(player.transform, new Vector2(-0.11f * dir, 0.007f));
                else transform.position = ToWorld(player.transform, new Vector2(-0.168f * dir, -0.031f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 1st punch anim
            case 11:
                if (normalizedTime >= 0f && normalizedTime < 0.0655f) transform.position = ToWorld(player.transform, new Vector2(-0.011f * dir, 0.288f));
                else if (normalizedTime >= 0.0655f && normalizedTime < 0.131f) transform.position = ToWorld(player.transform, new Vector2(-0.0325f * dir, 0.284f));
                else if (normalizedTime >= 0.131f && normalizedTime < 0.1965f) transform.position = ToWorld(player.transform, new Vector2(-0.054f * dir, 0.280f));
                else if (normalizedTime >= 0.1965f && normalizedTime < 0.262f) transform.position = ToWorld(player.transform, new Vector2(-0.076f * dir, 0.276f));
                else if (normalizedTime >= 0.262f && normalizedTime < 0.295f) transform.position = ToWorld(player.transform, new Vector2(-0.098f * dir, 0.272f));
                else if (normalizedTime >= 0.295f && normalizedTime < 0.328f) transform.position = ToWorld(player.transform, new Vector2(-0.0565f * dir, 0.2725f));
                else if (normalizedTime >= 0.328f && normalizedTime < 0.3605f) transform.position = ToWorld(player.transform, new Vector2(-0.015f * dir, 0.273f));
                else if (normalizedTime >= 0.3605f && normalizedTime < 0.393f) transform.position = ToWorld(player.transform, new Vector2(0.0265f * dir, 0.2735f));
                else if (normalizedTime >= 0.393f && normalizedTime < 0.4175f) transform.position = ToWorld(player.transform, new Vector2(0.068f * dir, 0.274f));
                else if (normalizedTime >= 0.4175f && normalizedTime < 0.442f) transform.position = ToWorld(player.transform, new Vector2(0.0815f * dir, 0.2775f));
                else if (normalizedTime >= 0.442f && normalizedTime < 0.467f) transform.position = ToWorld(player.transform, new Vector2(0.095f * dir, 0.281f));
                else if (normalizedTime >= 0.467f && normalizedTime < 0.492f) transform.position = ToWorld(player.transform, new Vector2(0.1085f * dir, 0.2845f));
                else if (normalizedTime >= 0.492f && normalizedTime < 0.586f) transform.position = ToWorld(player.transform, new Vector2(0.122f * dir, 0.288f));
                else if (normalizedTime >= 0.586f && normalizedTime < 0.680f) transform.position = ToWorld(player.transform, new Vector2(0.089f * dir, 0.288f));
                else if (normalizedTime >= 0.680f && normalizedTime < 0.7745f) transform.position = ToWorld(player.transform, new Vector2(0.056f * dir, 0.288f));
                else if (normalizedTime >= 0.7745f && normalizedTime < 0.869f) transform.position = ToWorld(player.transform, new Vector2(0.0225f * dir, 0.288f));
                else transform.position = ToWorld(player.transform, new Vector2(-0.011f * dir, 0.288f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 2nd punch anim
            case 12:
                if (normalizedTime >= 0f && normalizedTime < 0.303f) transform.position = ToWorld(player.transform, new Vector2(-0.014f * dir, 0.284f));
                else if (normalizedTime >= 0.303f && normalizedTime < 0.697f) transform.position = ToWorld(player.transform, new Vector2(0.141f * dir, 0.283f));
                else transform.position = ToWorld(player.transform, new Vector2(0.07f * dir, 0.294f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 3rd punch anim
            case 13:
                if (normalizedTime >= 0f && normalizedTime < 0.09425f) transform.position = ToWorld(player.transform, new Vector2(-0.031f * dir, 0.272f));
                else if (normalizedTime >= 0.09425f && normalizedTime < 0.1885f) transform.position = ToWorld(player.transform, new Vector2(0.03375f * dir, 0.25625f));
                else if (normalizedTime >= 0.1885f && normalizedTime < 0.28275f) transform.position = ToWorld(player.transform, new Vector2(0.0985f * dir, 0.2405f));
                else if (normalizedTime >= 0.28275f && normalizedTime < 0.377f) transform.position = ToWorld(player.transform, new Vector2(0.16325f * dir, 0.22475f));
                else if (normalizedTime >= 0.377f && normalizedTime < 0.4345f) transform.position = ToWorld(player.transform, new Vector2(0.228f * dir, 0.209f));
                else if (normalizedTime >= 0.4345f && normalizedTime < 0.492f) transform.position = ToWorld(player.transform, new Vector2(0.18375f * dir, 0.2225f));
                else if (normalizedTime >= 0.492f && normalizedTime < 0.5495f) transform.position = ToWorld(player.transform, new Vector2(0.1395f * dir, 0.236f));
                else if (normalizedTime >= 0.5495f && normalizedTime < 0.607f) transform.position = ToWorld(player.transform, new Vector2(0.09525f * dir, 0.2495f));
                else if (normalizedTime >= 0.607f && normalizedTime < 0.652f) transform.position = ToWorld(player.transform, new Vector2(0.051f * dir, 0.263f));
                else if (normalizedTime >= 0.652f && normalizedTime < 0.697f) transform.position = ToWorld(player.transform, new Vector2(0.0305f * dir, 0.26525f));
                else if (normalizedTime >= 0.697f && normalizedTime < 0.742f) transform.position = ToWorld(player.transform, new Vector2(0.010f * dir, 0.2675f));
                else if (normalizedTime >= 0.742f && normalizedTime < 0.787f) transform.position = ToWorld(player.transform, new Vector2(-0.0105f * dir, 0.26975f));
                else transform.position = ToWorld(player.transform, new Vector2(-0.031f * dir, 0.272f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 4th punch anim
            case 14:
                if (normalizedTime >= 0f && normalizedTime < 0.0595f) transform.position = ToWorld(player.transform, new Vector2(0.07f * dir, 0.249f));
                else if (normalizedTime >= 0.0595f && normalizedTime < 0.119f) transform.position = ToWorld(player.transform, new Vector2(0.114625f * dir, 0.244125f));
                else if (normalizedTime >= 0.119f && normalizedTime < 0.1785f) transform.position = ToWorld(player.transform, new Vector2(0.15925f * dir, 0.23925f));
                else if (normalizedTime >= 0.1785f && normalizedTime < 0.238f) transform.position = ToWorld(player.transform, new Vector2(0.203875f * dir, 0.234375f));
                else if (normalizedTime >= 0.238f && normalizedTime < 0.2975f) transform.position = ToWorld(player.transform, new Vector2(0.2485f * dir, 0.2295f));
                else if (normalizedTime >= 0.2975f && normalizedTime < 0.357f) transform.position = ToWorld(player.transform, new Vector2(0.29275f * dir, 0.22425f));
                else if (normalizedTime >= 0.357f && normalizedTime < 0.4165f) transform.position = ToWorld(player.transform, new Vector2(0.337f * dir, 0.219f));
                else if (normalizedTime >= 0.4165f && normalizedTime < 0.44625f) transform.position = ToWorld(player.transform, new Vector2(0.3015f * dir, 0.223375f));
                else if (normalizedTime >= 0.44625f && normalizedTime < 0.476f) transform.position = ToWorld(player.transform, new Vector2(0.266f * dir, 0.2305f));
                else if (normalizedTime >= 0.476f && normalizedTime < 0.49975f) transform.position = ToWorld(player.transform, new Vector2(0.2305f * dir, 0.23625f));
                else if (normalizedTime >= 0.49975f && normalizedTime < 0.5235f) transform.position = ToWorld(player.transform, new Vector2(0.195f * dir, 0.242f));
                else if (normalizedTime >= 0.5235f && normalizedTime < 0.54725f) transform.position = ToWorld(player.transform, new Vector2(0.17725f * dir, 0.24525f));
                else if (normalizedTime >= 0.54725f && normalizedTime < 0.571f) transform.position = ToWorld(player.transform, new Vector2(0.160f * dir, 0.2485f));
                else if (normalizedTime >= 0.571f && normalizedTime < 0.6425f) transform.position = ToWorld(player.transform, new Vector2(0.15125f * dir, 0.249875f));
                else if (normalizedTime >= 0.6425f && normalizedTime < 0.714f) transform.position = ToWorld(player.transform, new Vector2(0.1425f * dir, 0.25125f));
                else if (normalizedTime >= 0.714f && normalizedTime < 0.7855f) transform.position = ToWorld(player.transform, new Vector2(0.134f * dir, 0.252625f));
                else if (normalizedTime >= 0.7855f && normalizedTime < 0.857f) transform.position = ToWorld(player.transform, new Vector2(0.1295f * dir, 0.2533125f));
                else transform.position = ToWorld(player.transform, new Vector2(0.125f * dir, 0.254f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when doing air kick
            case 15:
                if (normalizedTime >= 0f && normalizedTime < 0.11675f) transform.position = ToWorld(player.transform, new Vector2(0.182f * dir, 0.219f));
                else if (normalizedTime >= 0.11675f && normalizedTime < 0.2335f) transform.position = ToWorld(player.transform, new Vector2(0.0945f * dir, 0.24225f));
                else if (normalizedTime >= 0.2335f && normalizedTime < 0.35025f) transform.position = ToWorld(player.transform, new Vector2(0.007f * dir, 0.2655f));
                else if (normalizedTime >= 0.35025f && normalizedTime < 0.467f) transform.position = ToWorld(player.transform, new Vector2(-0.082f * dir, 0.312f));
                else if (normalizedTime >= 0.467f && normalizedTime < 0.51175f) transform.position = ToWorld(player.transform, new Vector2(-0.017f * dir, 0.31325f));
                else if (normalizedTime >= 0.51175f && normalizedTime < 0.5565f) transform.position = ToWorld(player.transform, new Vector2(0.028f * dir, 0.3145f));
                else if (normalizedTime >= 0.5565f && normalizedTime < 0.6f) transform.position = ToWorld(player.transform, new Vector2(0.06f * dir, 0.31475f));
                else if (normalizedTime >= 0.6f && normalizedTime < 0.7f) transform.position = ToWorld(player.transform, new Vector2(0.076f * dir, 0.315f));
                else if (normalizedTime >= 0.7f && normalizedTime < 0.8f) transform.position = ToWorld(player.transform, new Vector2(0.084f * dir, 0.315f));
                else if (normalizedTime >= 0.8f && normalizedTime < 0.9f) transform.position = ToWorld(player.transform, new Vector2(0.088f * dir, 0.315f));
                else transform.position = ToWorld(player.transform, new Vector2(0.092f * dir, 0.315f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when doing air punch
            case 16:
                transform.position = ToWorld(player.transform, new Vector2(0.149f * dir, 0.334f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 1st kick anim
            case 17:
                transform.position = ToWorld(player.transform, new Vector2(-0.008f * dir, 0.312f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 2nd kick anim
            case 18:
                if (normalizedTime >= 0f && normalizedTime < 0.093f) transform.position = ToWorld(player.transform, new Vector2(-0.076f * dir, 0.279f));
                else if (normalizedTime >= 0.093f && normalizedTime < 0.186f) transform.position = ToWorld(player.transform, new Vector2(-0.070f * dir, 0.319f));
                else if (normalizedTime >= 0.186f && normalizedTime < 0.259f) transform.position = ToWorld(player.transform, new Vector2(-0.067f * dir, 0.339f));
                else if (normalizedTime >= 0.259f && normalizedTime < 0.333f) transform.position = ToWorld(player.transform, new Vector2(-0.065f * dir, 0.349f));
                else if (normalizedTime >= 0.333f && normalizedTime < 0.407f) transform.position = ToWorld(player.transform, new Vector2(-0.064f * dir, 0.359f));
                else if (normalizedTime >= 0.407f && normalizedTime < 0.456f) transform.position = ToWorld(player.transform, new Vector2(-0.128f * dir, 0.375f));
                else if (normalizedTime >= 0.456f && normalizedTime < 0.506f) transform.position = ToWorld(player.transform, new Vector2(-0.193f * dir, 0.391f));
                else if (normalizedTime >= 0.506f && normalizedTime < 0.556f) transform.position = ToWorld(player.transform, new Vector2(-0.257f * dir, 0.406f));
                else if (normalizedTime >= 0.556f && normalizedTime < 0.692f) transform.position = ToWorld(player.transform, new Vector2(-0.265f * dir, 0.362f));
                else if (normalizedTime >= 0.692f && normalizedTime < 0.827f) transform.position = ToWorld(player.transform, new Vector2(-0.267f * dir, 0.341f));
                else if (normalizedTime >= 0.827f && normalizedTime < 0.963f) transform.position = ToWorld(player.transform, new Vector2(-0.269f * dir, 0.320f));
                else if (normalizedTime >= 0.963f && normalizedTime < 0.982f) transform.position = ToWorld(player.transform, new Vector2(-0.075f * dir, 0.244f));
                else if (normalizedTime >= 0.982f && normalizedTime < 1f) transform.position = ToWorld(player.transform, new Vector2(0.022f * dir, 0.206f));
                else transform.position = ToWorld(player.transform, new Vector2(0.120f * dir, 0.169f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when uppercut
            case 19:
                transform.position = ToWorld(player.transform, new Vector2(0.054f * dir, 0.297f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when launched
            case 20:
                if (normalizedTime >= 0f && normalizedTime < 0.083f) transform.position = ToWorld(player.transform, new Vector2(-0.085f * dir, 0.282f));
                else if (normalizedTime >= 0.083f && normalizedTime < 0.125f) transform.position = ToWorld(player.transform, new Vector2(-0.1785f * dir, 0.3605f));
                else if (normalizedTime >= 0.125f && normalizedTime < 0.167f) transform.position = ToWorld(player.transform, new Vector2(-0.226f * dir, 0.39975f));
                else if (normalizedTime >= 0.167f && normalizedTime < 0.233f) transform.position = ToWorld(player.transform, new Vector2(-0.272f * dir, 0.439f));
                else if (normalizedTime >= 0.233f && normalizedTime < 0.3f) transform.position = ToWorld(player.transform, new Vector2(-0.300f * dir, 0.2977f));
                else if (normalizedTime >= 0.3f && normalizedTime < 0.367f) transform.position = ToWorld(player.transform, new Vector2(-0.314f * dir, 0.1563f));
                else if (normalizedTime >= 0.367f && normalizedTime < 0.467f) transform.position = ToWorld(player.transform, new Vector2(-0.328f * dir, 0.015f));
                else if (normalizedTime >= 0.467f && normalizedTime < 0.567f) transform.position = ToWorld(player.transform, new Vector2(-0.223f * dir, 0.006f));
                else if (normalizedTime >= 0.567f && normalizedTime < 0.667f) transform.position = ToWorld(player.transform, new Vector2(-0.170f * dir, 0.0015f));
                else if (normalizedTime >= 0.667f && normalizedTime < 0.767f) transform.position = ToWorld(player.transform, new Vector2(-0.117f * dir, -0.003f));
                else if (normalizedTime >= 0.767f && normalizedTime < 0.867f) transform.position = ToWorld(player.transform, new Vector2(-0.1195f * dir, -0.104f));
                else if (normalizedTime >= 0.867f && normalizedTime < 0.967f) transform.position = ToWorld(player.transform, new Vector2(-0.121f * dir, -0.1545f));
                else if (normalizedTime >= 0.967f && normalizedTime < 0.983f) transform.position = ToWorld(player.transform, new Vector2(-0.1215f * dir, -0.1798f));
                else if (normalizedTime >= 0.983f && normalizedTime < 1f) transform.position = ToWorld(player.transform, new Vector2(-0.122f * dir, -0.1924f));
                else transform.position = ToWorld(player.transform, new Vector2(-0.122f * dir, -0.205f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 1st hurt anim
            case 21:
                if (normalizedTime >= 0f && normalizedTime < 0.111f) transform.position = ToWorld(player.transform, new Vector2(-0.046f * dir, 0.280f));
                else if (normalizedTime >= 0.111f && normalizedTime < 0.222f) transform.position = ToWorld(player.transform, new Vector2(-0.110f * dir, 0.252f));
                else if (normalizedTime >= 0.222f && normalizedTime < 0.333f) transform.position = ToWorld(player.transform, new Vector2(-0.174f * dir, 0.224f));
                else if (normalizedTime >= 0.333f && normalizedTime < 0.349f) transform.position = ToWorld(player.transform, new Vector2(-0.238f * dir, 0.196f));
                else if (normalizedTime >= 0.349f && normalizedTime < 0.365f) transform.position = ToWorld(player.transform, new Vector2(-0.158f * dir, 0.214f));
                else if (normalizedTime >= 0.365f && normalizedTime < 0.381f) transform.position = ToWorld(player.transform, new Vector2(-0.078f * dir, 0.232f));
                else if (normalizedTime >= 0.381f && normalizedTime < 0.514f) transform.position = ToWorld(player.transform, new Vector2(-0.051f * dir, 0.238f));
                else if (normalizedTime >= 0.514f && normalizedTime < 0.648f) transform.position = ToWorld(player.transform, new Vector2(-0.025f * dir, 0.244f));
                else if (normalizedTime >= 0.648f && normalizedTime < 0.781f) transform.position = ToWorld(player.transform, new Vector2(-0.012f * dir, 0.247f));
                else if (normalizedTime >= 0.781f && normalizedTime < 0.905f) transform.position = ToWorld(player.transform, new Vector2(-0.005f * dir, 0.249f));
                else transform.position = ToWorld(player.transform, new Vector2(0.002f * dir, 0.250f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 2nd hurt anim
            case 22:
                if (normalizedTime >= 0f && normalizedTime < 0.193f) transform.position = ToWorld(player.transform, new Vector2(0.029f * dir, 0.299f));
                else if (normalizedTime >= 0.193f && normalizedTime < 0.386f) transform.position = ToWorld(player.transform, new Vector2(-0.0537f * dir, 0.2793f));
                else if (normalizedTime >= 0.386f && normalizedTime < 0.579f) transform.position = ToWorld(player.transform, new Vector2(-0.1363f * dir, 0.2597f));
                else if (normalizedTime >= 0.579f && normalizedTime < 0.702f) transform.position = ToWorld(player.transform, new Vector2(-0.219f * dir, 0.240f));
                else if (normalizedTime >= 0.702f && normalizedTime < 0.825f) transform.position = ToWorld(player.transform, new Vector2(-0.2027f * dir, 0.2417f));
                else if (normalizedTime >= 0.825f && normalizedTime < 0.947f) transform.position = ToWorld(player.transform, new Vector2(-0.1863f * dir, 0.2433f));
                else if (normalizedTime >= 0.947f && normalizedTime < 0.974f) transform.position = ToWorld(player.transform, new Vector2(-0.1781f * dir, 0.2442f));
                else transform.position = ToWorld(player.transform, new Vector2(-0.170f * dir, 0.245f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 1st block anim
            case 23:
                transform.position = ToWorld(player.transform, new Vector2(-0.032f * dir, 0.302f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 2nd block anim
            case 24:
                transform.position = ToWorld(player.transform, new Vector2(-0.032f * dir, 0.302f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 3rd block anim
            case 25:
                if (normalizedTime >= 0f && normalizedTime < 0.035f) transform.position = ToWorld(player.transform, new Vector2(0.019f * dir, 0.264f));
                else if (normalizedTime >= 0.035f && normalizedTime < 0.07f) transform.position = ToWorld(player.transform, new Vector2(0.0207f * dir, 0.2634f));
                else if (normalizedTime >= 0.07f && normalizedTime < 0.104f) transform.position = ToWorld(player.transform, new Vector2(0.0223f * dir, 0.2627f));
                else if (normalizedTime >= 0.104f && normalizedTime < 0.13f) transform.position = ToWorld(player.transform, new Vector2(0.0236f * dir, 0.2615f));
                else if (normalizedTime >= 0.13f && normalizedTime < 0.156f) transform.position = ToWorld(player.transform, new Vector2(0.0257f * dir, 0.2603f));
                else if (normalizedTime >= 0.156f && normalizedTime < 0.197f) transform.position = ToWorld(player.transform, new Vector2(0.027f * dir, 0.258f));
                else if (normalizedTime >= 0.197f && normalizedTime < 0.268f) transform.position = ToWorld(player.transform, new Vector2(-0.0232f * dir, 0.2485f));
                else if (normalizedTime >= 0.268f && normalizedTime < 0.321f) transform.position = ToWorld(player.transform, new Vector2(-0.0747f * dir, 0.239f));
                else if (normalizedTime >= 0.321f && normalizedTime < 0.382f) transform.position = ToWorld(player.transform, new Vector2(-0.1255f * dir, 0.2295f));
                else if (normalizedTime >= 0.382f && normalizedTime < 0.403f) transform.position = ToWorld(player.transform, new Vector2(-0.1763f * dir, 0.220f));
                else if (normalizedTime >= 0.403f && normalizedTime < 0.459f) transform.position = ToWorld(player.transform, new Vector2(-0.244f * dir, 0.2103f));
                else if (normalizedTime >= 0.459f && normalizedTime < 0.572f) transform.position = ToWorld(player.transform, new Vector2(-0.278f * dir, 0.201f));
                else if (normalizedTime >= 0.572f && normalizedTime < 0.657f) transform.position = ToWorld(player.transform, new Vector2(-0.2287f * dir, 0.2086f));
                else if (normalizedTime >= 0.657f && normalizedTime < 0.741f) transform.position = ToWorld(player.transform, new Vector2(-0.1787f * dir, 0.2227f));
                else if (normalizedTime >= 0.741f && normalizedTime < 0.825f) transform.position = ToWorld(player.transform, new Vector2(-0.1285f * dir, 0.2365f));
                else if (normalizedTime >= 0.825f && normalizedTime < 0.909f) transform.position = ToWorld(player.transform, new Vector2(-0.0793f * dir, 0.2443f));
                else if (normalizedTime >= 0.909f && normalizedTime < 0.931f) transform.position = ToWorld(player.transform, new Vector2(-0.0285f * dir, 0.251f));
                else if (normalizedTime >= 0.931f && normalizedTime < 0.948f) transform.position = ToWorld(player.transform, new Vector2(0.022f * dir, 0.266f));
                else if (normalizedTime >= 0.948f && normalizedTime < 0.967f) transform.position = ToWorld(player.transform, new Vector2(0.0213f * dir, 0.2655f));
                else if (normalizedTime >= 0.967f && normalizedTime < 0.987f) transform.position = ToWorld(player.transform, new Vector2(0.0205f * dir, 0.265f));
                else if (normalizedTime >= 0.987f && normalizedTime < 1f) transform.position = ToWorld(player.transform, new Vector2(0.0212f * dir, 0.2655f));
                else transform.position = ToWorld(player.transform, new Vector2(0.022f * dir, 0.266f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            // position when 4th block anim
            case 26:
                if (normalizedTime >= 0f && normalizedTime < 0.035f) transform.position = ToWorld(player.transform, new Vector2(0.019f * dir, 0.264f));
                else if (normalizedTime >= 0.035f && normalizedTime < 0.07f) transform.position = ToWorld(player.transform, new Vector2(0.0207f * dir, 0.2634f));
                else if (normalizedTime >= 0.07f && normalizedTime < 0.104f) transform.position = ToWorld(player.transform, new Vector2(0.0223f * dir, 0.2627f));
                else if (normalizedTime >= 0.104f && normalizedTime < 0.13f) transform.position = ToWorld(player.transform, new Vector2(0.0236f * dir, 0.2615f));
                else if (normalizedTime >= 0.13f && normalizedTime < 0.156f) transform.position = ToWorld(player.transform, new Vector2(0.0257f * dir, 0.2603f));
                else if (normalizedTime >= 0.156f && normalizedTime < 0.197f) transform.position = ToWorld(player.transform, new Vector2(0.027f * dir, 0.258f));
                else if (normalizedTime >= 0.197f && normalizedTime < 0.268f) transform.position = ToWorld(player.transform, new Vector2(-0.0232f * dir, 0.2485f));
                else if (normalizedTime >= 0.268f && normalizedTime < 0.321f) transform.position = ToWorld(player.transform, new Vector2(-0.0747f * dir, 0.239f));
                else if (normalizedTime >= 0.321f && normalizedTime < 0.382f) transform.position = ToWorld(player.transform, new Vector2(-0.1255f * dir, 0.2295f));
                else if (normalizedTime >= 0.382f && normalizedTime < 0.403f) transform.position = ToWorld(player.transform, new Vector2(-0.1763f * dir, 0.220f));
                else if (normalizedTime >= 0.403f && normalizedTime < 0.459f) transform.position = ToWorld(player.transform, new Vector2(-0.244f * dir, 0.2103f));
                else if (normalizedTime >= 0.459f && normalizedTime < 0.572f) transform.position = ToWorld(player.transform, new Vector2(-0.278f * dir, 0.201f));
                else if (normalizedTime >= 0.572f && normalizedTime < 0.657f) transform.position = ToWorld(player.transform, new Vector2(-0.2287f * dir, 0.2086f));
                else if (normalizedTime >= 0.657f && normalizedTime < 0.741f) transform.position = ToWorld(player.transform, new Vector2(-0.1787f * dir, 0.2227f));
                else if (normalizedTime >= 0.741f && normalizedTime < 0.825f) transform.position = ToWorld(player.transform, new Vector2(-0.1285f * dir, 0.2365f));
                else if (normalizedTime >= 0.825f && normalizedTime < 0.909f) transform.position = ToWorld(player.transform, new Vector2(-0.0793f * dir, 0.2443f));
                else if (normalizedTime >= 0.909f && normalizedTime < 0.931f) transform.position = ToWorld(player.transform, new Vector2(-0.0285f * dir, 0.251f));
                else if (normalizedTime >= 0.931f && normalizedTime < 0.948f) transform.position = ToWorld(player.transform, new Vector2(0.022f * dir, 0.266f));
                else if (normalizedTime >= 0.948f && normalizedTime < 0.967f) transform.position = ToWorld(player.transform, new Vector2(0.0213f * dir, 0.2655f));
                else if (normalizedTime >= 0.967f && normalizedTime < 0.987f) transform.position = ToWorld(player.transform, new Vector2(0.0205f * dir, 0.265f));
                else if (normalizedTime >= 0.987f && normalizedTime < 1f) transform.position = ToWorld(player.transform, new Vector2(0.0212f * dir, 0.2655f));
                else transform.position = ToWorld(player.transform, new Vector2(0.022f * dir, 0.266f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
        }
    }

    // Convert player-local offset to world position fast (no need for TransformPoint for simple addition)
    private Vector3 ToWorld(Transform playerTransform, Vector2 offset)
    {
        // If player flips horizontally (scale.x negative) you might want to flip the X offset:
        // float signedOffsetX = offset.x * Mathf.Sign(playerTransform.localScale.x);
        // return new Vector3(playerTransform.position.x + signedOffsetX, playerTransform.position.y + offset.y, transform.position.z);

        // For now we assume offsets are already correct for facing direction:
        return new Vector3(playerTransform.position.x + offset.x, playerTransform.position.y + offset.y, transform.position.z);
    }
}
