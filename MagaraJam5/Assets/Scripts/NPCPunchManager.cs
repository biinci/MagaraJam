using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPunchManager : MonoBehaviour
{
    [SerializeField] private int chaosPoint;
    public int ChaosPoint
    {
        get => chaosPoint;
        set
        {
            NPCPointLogic.Instance.TotalChaos += value - chaosPoint;
            chaosPoint = value;
        }
    }
    [SerializeField] private float _punchCooldown;
    [SerializeField] private float _punchDistance;
    private NPCManager nPCManager;
    private bool canPunch = true;
    private void Start()
    {
        nPCManager = GetComponent<NPCManager>();
    }
    private void Update()
    {
        if (ChaosPoint > 10 && nPCManager.enabled == true)
        {
            NPCPunchManager npc = PunchableNPC();
            if (npc != null)
            {
                npc.Punch(ChaosPoint);
            }
        }
    }
    public void Punch(int chaosPoint)
    {
        if (nPCManager.anim.CurrentAnimation != nPCManager.punchOne && canPunch)
        {
            var npcCols = Physics2D.OverlapCircleAll(
                transform.position + transform.right * _punchDistance,
                0.15f,
                nPCManager._interractLayer
            );

            foreach (var npcCol in npcCols)
            {
                NPCPunchManager npc = npcCol.GetComponent<NPCPunchManager>();
                if (npc == null) continue;
                if (npc == this) continue;

                npc.ChaosPoint += chaosPoint / 3;
                ChaosPoint = chaosPoint / 3;
                break;
            }

            nPCManager.anim.ChangeAnimation(nPCManager.punchOne);

            StartCoroutine(PunchCooldownCoroutine());
        }
    }
    IEnumerator PunchCooldownCoroutine()
    {
        canPunch = false;
        yield return new WaitForSeconds(_punchCooldown);
        canPunch = true;
    }
    private NPCPunchManager PunchableNPC()
    {
        var npcCols = Physics2D.OverlapCircleAll(
                transform.position + transform.right * _punchDistance,
                0.15f,
                nPCManager._interractLayer
            );

        foreach (var npcCol in npcCols)
        {
            NPCPunchManager npc = npcCol.GetComponent<NPCPunchManager>();
            if (npc == null) continue;
            if (npc == this) continue;

            return npc;
        }
        return null;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.right * _punchDistance, 0.15f);
    }
}
