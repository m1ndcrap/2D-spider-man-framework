using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private CanvasGroup canvasGroup;
    public float visibleDuration = 3f;
    public float fadeSpeed = 6f;
    private float visibleTimer = 0f;

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        slider.value = currentHealth / maxHealth;
        visibleTimer = visibleDuration;
        canvasGroup.alpha = 1f;
    }

    void Update()
    {
        transform.position = target.position + offset;
        transform.rotation = Camera.main.transform.rotation;

        if (visibleTimer > 0f)
        {
            visibleTimer -= Time.deltaTime;
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
        }
    }
}