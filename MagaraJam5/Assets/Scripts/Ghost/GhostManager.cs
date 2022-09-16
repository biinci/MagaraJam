using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostManager : MonoBehaviour
{
    public static GhostManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    [SerializeField] private float enterNPCRadius;
    [SerializeField] private LayerMask NPCLayer;

    public NPCManager AvailableNPC { get; set; }
    private void Update()
    {
        AvailableNPC = FindAvailableNPC();
        if (AvailableNPC != null && Input.GetKeyDown(KeyCode.E))
        {
            AvailableNPC.GetComponent<ConvertedNPCManager>().enabled = true;
        }
    }
    private NPCManager FindAvailableNPC()
    {
        var npcColliders = Physics2D.OverlapCircleAll(transform.position, enterNPCRadius, NPCLayer);
        foreach (var npcCollider in npcColliders)
        {
            var nPCManager = npcCollider.GetComponent<NPCManager>();
            if (nPCManager == null) continue;

            return nPCManager;
        }
        return null;
    }
}
