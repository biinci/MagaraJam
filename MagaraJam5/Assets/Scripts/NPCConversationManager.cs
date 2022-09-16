using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCConversationManager : MonoBehaviour
{
    public static NPCConversationManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    private readonly List<Conversation> conversations = new();
    public void MakeConversationWith(NPCManager from, NPCManager to)
    {
        from.OnStartConversation();

        var conversationIndex = conversations.FindIndex(x => x.members.Contains(to));
        if (conversationIndex != -1)
        {
            conversations[conversationIndex].members.Add(from);
        }
        else
        {
            conversations.Add(new Conversation(from, to));
        }
    }
    public void LeaveNPCFromConversation(NPCManager npc)
    {
        var conversationIndex = conversations.FindIndex(x => x.members.Contains(npc));
        if (conversationIndex != -1)
        {
            conversations[conversationIndex].members.Remove(npc);
        }
    }
    private class Conversation
    {
        public readonly List<NPCManager> members = new();
        public Conversation(params NPCManager[] firstMembers)
        {
            foreach (var member in firstMembers)
            {
                members.Add(member);
            }
            Instance.StartCoroutine(ConversationCoroutine());
        }
        private IEnumerator ConversationCoroutine()
        {
            yield return new WaitForSeconds(10);
            for (var i = 0; i < members.Count; i++)
            {
                members[i].OnEndConversation(i % 2 == 0 ? Direction.right : Direction.left);
                Debug.Log(members[i].name);
                yield return new WaitForSeconds(0.7f);
            }
            Instance.conversations.Remove(this);
        }
    }
}
