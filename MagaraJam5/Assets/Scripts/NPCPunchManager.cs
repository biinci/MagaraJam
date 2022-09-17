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
    [SerializeField] private float _punchKnockback;
    [SerializeField] private Transform _punchTransform;
    private NPCManager nPCManager;
    private bool canPunch = true;
    private void Start()
    {
        nPCManager = GetComponent<NPCManager>();
        nPCManager.anim.AddListener("Punch", Punch);
    }
    private void Update()
    {
        if (ChaosPoint > 3 && nPCManager.enabled)
        {
            NPCPunchManager npc = PunchableNPC();
            if (npc != null && canPunch)
            {
                nPCManager.anim.ChangeAnimation(nPCManager.punchOne);
                StartPunchCoroutine();
            }
        }
    }
    public void Punch()
    {
        Debug.Log("çağrıldı");
        int chaosPoint;
        if (nPCManager.enabled == false)
            chaosPoint = NPCPointLogic.ChaosPointPerPunch;
        else
            chaosPoint = ChaosPoint;

        var npcCols = Physics2D.OverlapCircleAll(
                _punchTransform.position,
                0.15f,
                nPCManager._interractLayer
            );

        foreach (var npcCol in npcCols)
        {
            NPCPunchManager npc = npcCol.GetComponent<NPCPunchManager>();
            if (npc == null) continue;
            if (npc == this) continue;

            npc.ChaosPoint += chaosPoint / 3;
            npc.GetComponent<Rigidbody2D>().velocity = _punchKnockback * (npc.transform.position.x - transform.position.x) * Vector2.right;
            npc.StartCoroutine(npc.TargetingCoroutine(this));
            ChaosPoint = chaosPoint / 3;
            break;
        }
    }
    IEnumerator TargetingCoroutine(NPCPunchManager target)
    {
        yield return new WaitForSeconds(0.5f);
        var startPoint = ChaosPoint;
        NPCConversationSystem.Instance.LeaveNPCFromConversation(nPCManager);
        while (ChaosPoint == startPoint)
        {
            nPCManager.CurrentDirection = transform.position.x > target.transform.position.x ? Direction.left : Direction.right;
            yield return null;
        }


    }

    public void StartPunchCoroutine()
    {
        if (canPunch)
            StartCoroutine(PunchCoroutine());
    }

    IEnumerator PunchCoroutine()
    {
        canPunch = false;
        yield return new WaitForSeconds(0.25f);
        Punch();
        yield return new WaitForSeconds(_punchCooldown);
        canPunch = true;
    }

    private NPCPunchManager PunchableNPC()
    {
        var npcCols = Physics2D.OverlapCircleAll(
                _punchTransform.position,
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
        Gizmos.DrawWireSphere(_punchTransform.position, 0.15f);
    }
}
