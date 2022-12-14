using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GhostManager : MonoBehaviour
{
    public static GhostManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    [SerializeField] private float enterNPCRadius;
    [SerializeField] private LayerMask NPCLayer;

    [SerializeField] private TMP_Text captureBodyCountText;
    public bool shouldStartEndCoroutine;
    bool isStartedEndCoroutine;
    private int captureBodyCount = 7;
    public Transform clampUp, clampDown, clampLeft, clampRight;
    public int CaptureBodyCount
    {
        get => captureBodyCount;
        set
        {
            captureBodyCount = value;
            captureBodyCountText.text = $"x{captureBodyCount}";
        }
    }

    public NPCManager AvailableNPC { get; set; }
    private void Update()
    {
        AvailableNPC = FindAvailableNPC();
        if (AvailableNPC != null && Input.GetKeyDown(KeyCode.E) && CaptureBodyCount > 0)
        {
            AvailableNPC.GetComponent<ConvertedNPCManager>().enabled = true;
        }
        if (shouldStartEndCoroutine && !isStartedEndCoroutine && NPCPointLogic.Instance.TotalChaos <= 0)
        {
            NPCPointLogic.Instance.StartCoroutine(NPCPointLogic.Instance.OnEndGameCoroutine(false));
            isStartedEndCoroutine = true;
        }

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, clampLeft.position.x, clampRight.position.x), Mathf.Clamp(transform.position.y, clampDown.position.y, clampUp.position.y));

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
