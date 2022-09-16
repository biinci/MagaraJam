using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvertedNPCManager : MonoBehaviour
{
    private NPCManager nPCManager;
    private GhostManager ghostManager;
    private void OnEnable()
    {
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
        GetComponent<Rigidbody2D>().velocity = new(Input.GetAxis("Horizontal") * nPCManager._speed, 0);
        nPCManager.ChechkAnimations();
        nPCManager.CheckRotations();

        if (Input.GetKeyDown(KeyCode.E))
        {
            this.enabled = false;
        }
    }
    private void OnDisable()
    {
        ghostManager.transform.position = transform.position + new Vector3(0, 1f);
        ghostManager.gameObject.SetActive(true);
        FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().Follow = ghostManager.transform;

        nPCManager.enabled = true;
    }
}
