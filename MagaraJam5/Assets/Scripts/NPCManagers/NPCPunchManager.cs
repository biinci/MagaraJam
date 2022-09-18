using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPunchManager : MonoBehaviour
{
    [SerializeField] private float _punchCooldown;
    [SerializeField] private Transform _punchTransform;
    [SerializeField] private float punchRadius;
    
    [HideInInspector] public NPCManager nPCManager;
    [HideInInspector] public NPCPointManager nPCPointManager;

    public bool canPunch = true;
    public bool isKnockbacking = false;
    public bool IsAgressive => nPCPointManager.ChaosPoint > NPCPointLogic.Instance.pointForBeingAgresivve;

    private void Start()
    {
        nPCManager = GetComponent<NPCManager>();
        nPCPointManager = GetComponent<NPCPointManager>();
    }
    private void Update()
    {
        if (IsAgressive && nPCManager.enabled)
        {
            NPCPunchManager npc = PunchableNPC();
            if (npc != null && canPunch && !isKnockbacking)
            {
                StartCoroutine(PunchCoroutine());
            }
        }
    }
    public IEnumerator PunchCoroutine()
    {
        nPCManager.anim.ChangeAnimation(nPCManager.punchOne);

        canPunch = false;
        yield return new WaitForSeconds(0.25f);

        var punchableNPC = PunchableNPC();
        if (punchableNPC != null)
        {
            bool isPlayer = !nPCManager.enabled;
            nPCPointManager.TransferChaosPoint(punchableNPC.nPCPointManager, isPlayer);
            punchableNPC.StartCoroutine(punchableNPC.KnockbackCoroutine(this));
        }

        yield return new WaitForSeconds(_punchCooldown);
        canPunch = true;
    }
    IEnumerator KnockbackCoroutine(NPCPunchManager from)
    {
        NPCConversationSystem.Instance.LeaveNPCFromConversation(nPCManager);
        nPCManager.CurrentDirection = Direction.none;
        if (nPCManager.enabled == false) {
            GetComponent<ConvertedNPCManager>().enabled = false;
        }
        isKnockbacking = true;

        yield return new WaitForSeconds(1f);

        isKnockbacking = false;
        nPCManager.CurrentDirection = from.transform.position.x > transform.position.x ? Direction.right : Direction.left;

    }

    private NPCPunchManager PunchableNPC()
    {
        var npcCols = Physics2D.OverlapCircleAll(
                _punchTransform.position,
                punchRadius,
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
        Gizmos.DrawWireSphere(_punchTransform.position, punchRadius);
    }
}
