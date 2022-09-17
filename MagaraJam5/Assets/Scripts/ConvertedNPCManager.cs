using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvertedNPCManager : MonoBehaviour
{
    private NPCManager nPCManager;
    private GhostManager ghostManager;
    private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    private float input;

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


    private void Update(){
        input = Input.GetAxis("Horizontal");
        if (nPCManager.canMove)
            GetComponent<Rigidbody2D>().velocity = new Vector2(input * nPCManager._speed, 0);

        nPCManager.CheckAnimations();

        switch (input) {
            case < 0:
                nPCManager.SetFacingDirection(-1);
                break;
            case > 0:
                nPCManager.SetFacingDirection(1);
                break;
        }


        if (Input.GetKeyDown(KeyCode.E)) {
            this.enabled = false;
        }

        if (Input.GetMouseButtonDown(0) && nPCManager.anim.CurrentAnimation != nPCManager.punchOne) {
            nPCManager.anim.ChangeAnimation(nPCManager.punchOne);
            
        }
    }

    private void OnDisable()
    {
        GetComponent<SpriteRenderer>().material.SetFloat(OutlineThickness, 0);

        if (ghostManager == null) return;

        ghostManager.gameObject.SetActive(true);
        ghostManager.transform.position = transform.position + new Vector3(0, 1f);
        FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().Follow = ghostManager.transform;

        nPCManager.enabled = true;
    }
}
