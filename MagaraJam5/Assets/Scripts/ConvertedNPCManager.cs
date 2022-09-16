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
        ghostManager.gameObject.SetActive(false);

        nPCManager = GetComponent<NPCManager>();
        NPCConversationManager.Instance.LeaveNPCFromConversation(nPCManager);
        nPCManager.enabled = false;
    }
    private void Update()
    {
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
