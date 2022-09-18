using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NPCPointLogic : MonoBehaviour
{
    #region Instance
    public static NPCPointLogic Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion
    [SerializeField] private Image chaosBar;
    public int pointForBeingAgresivve;
    public int maxCaptureBodyCount;
    public int chaosPointPerPunch;

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
