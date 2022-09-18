using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Rendering.Universal;

public class NPCPointLogic : MonoBehaviour
{
    [SerializeField] public AnimationData[] animationDatas;

    #region Instance
    public static NPCPointLogic Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion
    [SerializeField] private TMPro.TMP_Text endGameText;
    [SerializeField] private float endGameTextMoveSpeed;
    [SerializeField] private float lastWaitAmount;
    [SerializeField] private Image chaosBar;
    public int pointForBeingAgresivve;
    public int maxCaptureBodyCount;
    public int chaosPointPerPunch;
    public float decreasingAmount;
    public int increasingAmount;
    public int maxAmount;
    public AudioClip backgroundMusic;
    
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

            if (totalChaos > maxAmount)
            {
                StartCoroutine(OnEndGameCoroutine(true));
            }
        }
    }

    private void Start(){
        SoundManager.Instance.musicSource.clip = backgroundMusic;
        SoundManager.Instance.musicSource.Play();
        
        
        StartCoroutine(DecreasingChaos());
        
        
    }

    private IEnumerator DecreasingChaos()
    {
        TotalChaos = 0;
        while (true)
        {
            TotalChaos -= 1;
            yield return new WaitForSeconds(1 / decreasingAmount);
        }
    }

    private void Update(){
        // Camera.main.transform.position = new Vector2(Camera.main.transform.position.x, 0.2f);
    }

    public IEnumerator OnEndGameCoroutine(bool didWin)
    {
        endGameText.text = didWin ? "Kazandın!" : "Kaybettin..";

        Vector3 targetPos = new(Screen.width / 2, Screen.height / 2);
        RectTransform textTransform = endGameText.GetComponent<RectTransform>();
        while (Vector3.Distance(textTransform.position, targetPos) > 1f)
        {
            textTransform.position = Vector3.Lerp(
                textTransform.position,
                targetPos,
                endGameTextMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(lastWaitAmount);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

}
