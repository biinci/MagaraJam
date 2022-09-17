using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NPCPointLogic : MonoBehaviour
{
    public static NPCPointLogic Instance;
    private void Awake()
    {
        Instance = this;
    }
    [SerializeField] private Image chaosBar;

    [SerializeField] private int maxCaptureBodyCount;
    public static int MaxCaptureBodyCount => Instance.maxCaptureBodyCount;

    [SerializeField] private int chaosPointPerPunch;
    public static int ChaosPointPerPunch => Instance.chaosPointPerPunch;

    private int totalChaos;
    public int TotalChaos
    {
        get => totalChaos;
        set
        {
            totalChaos = value;
            Debug.Log(totalChaos);
            chaosBar.fillAmount = (float)totalChaos / (float)(chaosPointPerPunch * maxCaptureBodyCount);
        }
    }

    private void Start() => TotalChaos = 0;
}
