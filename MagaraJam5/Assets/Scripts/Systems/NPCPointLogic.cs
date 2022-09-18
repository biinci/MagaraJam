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
    public float decreasingAmount;
    public int increasingAmount;
    public int maxAmount;

    private int totalChaos;
    public int TotalChaos
    {
        get => totalChaos;
        set
        {
            totalChaos = value;
            if (totalChaos < 0) totalChaos = 0;   
            Debug.Log(totalChaos);
            chaosBar.fillAmount = totalChaos / (float)maxAmount;
        }
    }

    private void Start() => StartCoroutine(DecreasingChaos());

    
    
    private IEnumerator DecreasingChaos(){
        TotalChaos = 0;
        while (true) {
            TotalChaos -= 1;
            yield return new WaitForSeconds(1/decreasingAmount);
        }
    }
}
