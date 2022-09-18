using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPointManager : MonoBehaviour
{
    [SerializeField] private int chaosPoint;
    [SerializeField] public SpriteRenderer angryIcon;

    public int ChaosPoint
    {
        get => chaosPoint;
        set
        {
            NPCPointLogic.Instance.TotalChaos += NPCPointLogic.Instance.increasingAmount;

            chaosPoint = value;

            angryIcon.gameObject.SetActive(chaosPoint > NPCPointLogic.Instance.pointForBeingAgresivve);
        }
    }
    public void TransferChaosPoint(NPCPointManager to, bool isPlayer)
    {
        if (isPlayer)
        {
            to.ChaosPoint += NPCPointLogic.Instance.chaosPointPerPunch;
        }
        else
        {
            to.ChaosPoint += chaosPoint / 3;
            ChaosPoint = chaosPoint / 3;
        }
    }
}
