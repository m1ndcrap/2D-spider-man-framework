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
        if (player.currentCounter == null)
        {
            Destroy(gameObject);
            return;
        }

        // cache the mstate and normalizedTime
        int mstate = player.anim.GetInteger("mstate");
        float normalizedTime = player.anim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;

        switch (mstate)
        {
            case 0:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case 1:
                transform.position = ToWorld(player.transform, new Vector2(0.125f, 0.248f));
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case 2:
                if (normalizedTime >= 0f && normalizedTime < 0.077f) transform.position = ToWorld(player.transform, new Vector2(-0.014f, 0.348f));
                else if (normalizedTime >= 0.077f && normalizedTime < 0.231f) transform.position = ToWorld(player.transform, new Vector2(-0.032f, 0.348f));
                else if (normalizedTime >= 0.231f && normalizedTime < 0.308f) transform.position = ToWorld(player.transform, new Vector2(-0.01f, 0.348f));
                else if (normalizedTime >= 0.308f && normalizedTime < 0.385f) transform.position = ToWorld(player.transform, new Vector2(0.027f, 0.348f));
                else if (normalizedTime >= 0.385f && normalizedTime < 0.462f) transform.position = ToWorld(player.transform, new Vector2(0.079f, 0.343f));
                else if (normalizedTime >= 0.462f && normalizedTime < 0.538f) transform.position = ToWorld(player.transform, new Vector2(0.116f, 0.325f));
                else if (normalizedTime >= 0.538f && normalizedTime < 0.615f) transform.position = ToWorld(player.transform, new Vector2(0.137f, 0.328f));
                else if (normalizedTime >= 0.615f && normalizedTime < 0.692f) transform.position = ToWorld(player.transform, new Vector2(0.166f, 0.307f));
                else if (normalizedTime >= 0.692f && normalizedTime < 0.769f) transform.position = ToWorld(player.transform, new Vector2(0.184f, 0.307f));
                else if (normalizedTime >= 0.769f && normalizedTime < 0.846f) transform.position = ToWorld(player.transform, new Vector2(0.197f, 0.297f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.278f)); // 0.923f -> 1.0
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case 3:
                transform.position = ToWorld(player.transform, new Vector2(0.449f, 0.231f));
                break;
            case 4:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 5:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 6:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 7:
                transform.position = ToWorld(player.transform, new Vector2(-0.322f, -0.124f));
                transform.rotation = Quaternion.Euler(0f, 0f, 41.9f);
                break;
            case 8:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 9:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 10:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 11:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 12:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 13:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 14:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 15:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 16:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 17:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 18:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 19:
                if (normalizedTime >= 0f && normalizedTime < 0.024f) transform.position = ToWorld(player.transform, new Vector2(0.03f, 0.284f));
                else if (normalizedTime >= 0.024f && normalizedTime < 0.122f) transform.position = ToWorld(player.transform, new Vector2(0.05f, 0.284f));
                else if (normalizedTime >= 0.122f && normalizedTime < 0.171f) transform.position = ToWorld(player.transform, new Vector2(0.07f, 0.274f));
                else if (normalizedTime >= 0.171f && normalizedTime < 0.268f) transform.position = ToWorld(player.transform, new Vector2(0.081f, 0.259f));
                else if (normalizedTime >= 0.268f && normalizedTime < 0.538f) transform.position = ToWorld(player.transform, new Vector2(0.116f, 0.325f));

                else if (normalizedTime >= 0.538f && normalizedTime < 0.615f) transform.position = ToWorld(player.transform, new Vector2(0.137f, 0.328f));
                else if (normalizedTime >= 0.615f && normalizedTime < 0.692f) transform.position = ToWorld(player.transform, new Vector2(0.166f, 0.307f));
                else if (normalizedTime >= 0.692f && normalizedTime < 0.769f) transform.position = ToWorld(player.transform, new Vector2(0.184f, 0.307f));
                else if (normalizedTime >= 0.769f && normalizedTime < 0.846f) transform.position = ToWorld(player.transform, new Vector2(0.197f, 0.297f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else if (normalizedTime >= 0.846f && normalizedTime < 0.923f) transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.289f));
                else transform.position = ToWorld(player.transform, new Vector2(0.200f, 0.278f)); // 0.923f -> 1.0
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case 20:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 21:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 22:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 23:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 24:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 25:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
                break;
            case 26:
                transform.position = ToWorld(player.transform, new Vector2(0.023f, 0.284f));
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
