using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvertedNPCManager : MonoBehaviour
{
    private NPCManager nPCManager;
    private GhostManager ghostManager;
    private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");

    private void OnEnable()
    {
        GetComponent<SpriteRenderer>().material.SetFloat(OutlineThickness, 1);
        FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().Follow = transform;
        
        ghostManager = GhostManager.Instance;
        ghostManager.AvailableNPC = null;
        ghostManager.gameObject.SetActive(false);

        nPCManager = GetComponent<NPCManager>();
        NPCConversationManager.Instance.LeaveNPCFromConversation(nPCManager);
        nPCManager.CheckIcon();
        nPCManager.enabled = false;
    }
    private void Update()
    {
        if(nPCManager.canMove)
            GetComponent<Rigidbody2D>().velocity = new Vector2(Input.GetAxis("Horizontal") * nPCManager._speed, 0);
        nPCManager.CheckAnimations();
        nPCManager.CheckRotations();

        if (Input.GetKeyDown(KeyCode.E))
        {
            this.enabled = false;
        }
        
        if (Input.GetMouseButtonDown(0)) {
            nPCManager.anim.ChangeAnimation(nPCManager.punchOne);
        }
    }
    private void OnDisable()
    {
        GetComponent<SpriteRenderer>().material.SetFloat(OutlineThickness, 0);
        ghostManager.transform.position = transform.position + new Vector3(0, 1f);
        ghostManager.gameObject.SetActive(true);
        FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().Follow = ghostManager.transform;

        nPCManager.enabled = true;
    }
}
