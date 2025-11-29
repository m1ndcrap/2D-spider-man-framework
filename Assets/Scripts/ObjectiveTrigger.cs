using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static PlayerStep;

public class ObjectiveTrigger : MonoBehaviour
{
    [SerializeField] public int missionType;   // 1 for hostage rescue, 2 for beat all enemies in area, 3 for chasing
    [SerializeField] public GameObject missionObjective; // for hostage rescue
    [SerializeField] public List<GameObject> missionList; // for beating all enemies
    [SerializeField] private GameObject bgmController;
    [SerializeField] private List<GameObject> barriers;

    public Image uiStart;
    public Image uiArrow;
    public Image uiFound;
    public Image uiBG;
    public Image uiIcons;
    public Image uiComplete;
    public Image uiTimer;

    public Sprite[] sprTimerStart;       // 20 frames (0–19)
    public Sprite[] sprTimerFound;       // 13 frames (0–12)
    public Sprite[] sprTimer;            // 32 frames (0–31)
    public Sprite sprTimerArrow;         // 1 frame
    public Sprite sprTimerBG;
    public Sprite[] sprTimerComplete;    // 21 frames (0–20)
    public Sprite[] sprTimerIcons;       // (use index 2)

    private PlayerStep player;

    public bool countdown = false;
    public bool start = false;
    private bool found = false;
    private bool completed = false;
    private bool done = false;
    public bool active = false;

    private int indexS = 0;
    private int indexF = 0;
    private int indexC = 0;

    private bool animateS = true;
    private bool animateF = true;
    private bool animateC = true;

    [SerializeField] private Canvas canvas;
    [SerializeField] public RectTransform uiParent;

    private int alarm1 = 0;
    private int alarm2 = 0;
    private int alarm3 = 0;

    void Start()
    {
        player = FindObjectOfType<PlayerStep>();
        uiStart.canvasRenderer.SetAlpha(0);
        uiArrow.canvasRenderer.SetAlpha(0);
        uiFound.canvasRenderer.SetAlpha(0);
        uiTimer.canvasRenderer.SetAlpha(0);
        uiComplete.canvasRenderer.SetAlpha(0);
        uiBG.canvasRenderer.SetAlpha(0);
        uiIcons.canvasRenderer.SetAlpha(0);
        uiParent.anchoredPosition = new Vector2(0, 171);

        foreach (GameObject b in barriers)
        {
            if (b != null) b.SetActive(false);
        }
    }

    void Update()
    {
        if (alarm1 > 0)
        {
            alarm1 -= 1;
        }
        else
        {
            if (start)
            {
                if (indexS >= 19)
                {
                    start = false;
                }
                else
                {
                    indexS++;
                    animateS = true;
                }
            }
        }

        if (alarm2 > 0)
        {
            alarm2 -= 1;
        }
        else
        {
            if (found)
            {
                if (indexF >= 12)
                {
                    indexF = 0;
                }
                else
                {
                    indexF++;
                }
            }

            animateF = true;
        }

        if (alarm3 > 0)
        {
            alarm3 -= 1;
        }
        else
        {
            if (completed)
            {
                if (indexC >= 20)
                {
                    done = true;
                    completed = false;
                }
                else
                {
                    indexC++;
                    animateC = true;
                }
            }
        }

        if (countdown)
        {
            uiBG.sprite = sprTimerBG;
            uiBG.rectTransform.anchoredPosition = Vector2.zero;
            uiBG.canvasRenderer.SetAlpha(1);
            uiIcons.canvasRenderer.SetAlpha(1);
            uiArrow.rectTransform.anchoredPosition = Vector2.zero;
            uiArrow.sprite = sprTimerArrow;

            foreach (GameObject b in barriers)
            {
                if (b != null) b.SetActive(true);
            }

            if (missionType == 1)
                uiIcons.sprite = sprTimerIcons[2];
            else if (missionType == 2)
                uiIcons.sprite = sprTimerIcons[1];
            else
                uiIcons.sprite = sprTimerIcons[0];

            uiIcons.rectTransform.anchoredPosition = Vector2.zero;
            bgmController.GetComponent<BGMController>().intensity = 1;

            if (missionType == 1)
            {
                if (missionObjective.GetComponent<HostageScript>().phase == 0)
                {
                    if (Vector3.Distance(player.transform.position, missionObjective.transform.position) < 2f)
                    {
                        found = true;
                        uiArrow.canvasRenderer.SetAlpha(0);
                        uiFound.canvasRenderer.SetAlpha(1);
                    }
                    else
                    {
                        found = false;
                        indexF = 0;
                        animateF = false;
                        alarm2 = 0;
                        uiArrow.canvasRenderer.SetAlpha(1);
                        uiFound.canvasRenderer.SetAlpha(0);
                    }
                }
            }
        }

        if (missionType == 1 && !done)
        {
            if (missionObjective.GetComponent<HostageScript>().phase != 0)
            {
                countdown = false;
                bgmController.GetComponent<BGMController>().intensity = 0;
                completed = true;
                uiStart.canvasRenderer.SetAlpha(0);
                uiArrow.canvasRenderer.SetAlpha(0);
                uiFound.canvasRenderer.SetAlpha(0);
                uiTimer.canvasRenderer.SetAlpha(0);
                uiBG.canvasRenderer.SetAlpha(0);
                uiIcons.canvasRenderer.SetAlpha(0);
            }
        }

        if (start)
        {
            uiStart.canvasRenderer.SetAlpha(1);
            uiStart.sprite = sprTimerStart[indexS];
            uiStart.rectTransform.anchoredPosition = new Vector2(-1.2f, -1.3f);

            if (animateS)
            {
                alarm1 = 1;
                animateS = false;
            }
        }

        if (!start) uiStart.canvasRenderer.SetAlpha(0);

        if (found)
        {
            uiFound.rectTransform.anchoredPosition = Vector2.zero;
            uiFound.sprite = sprTimerFound[indexF];

            if (animateF)
            {
                alarm2 = 2;
                animateF = false;
            }
        }

        if (completed && !done)
        {
            uiComplete.canvasRenderer.SetAlpha(1);
            uiComplete.rectTransform.anchoredPosition = Vector2.zero;
            uiComplete.sprite = sprTimerComplete[indexC];

            if (animateC)
            {
                alarm3 = 2;
                animateC = false;
            }
        }

        Vector2 objScreenPos = Camera.main.WorldToScreenPoint(missionObjective.transform.position);
        Vector2 objLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uiArrow.rectTransform.parent as RectTransform, objScreenPos, null, out objLocalPos);
        Vector2 dir = objLocalPos - (Vector2)uiArrow.rectTransform.localPosition;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        uiArrow.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);

        if (done)
        {
            uiComplete.canvasRenderer.SetAlpha(0);

            foreach (GameObject b in barriers)
            {
                if (b != null)
                    Destroy(b);
            }

            Destroy(gameObject);
        }
    }
}