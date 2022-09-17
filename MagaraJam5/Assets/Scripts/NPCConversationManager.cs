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
        if (conversations.FindIndex(x => x.members.Contains(from)) != -1) return;

        var conversationIndex = conversations.FindIndex(x => x.members.Contains(to));
        if (conversationIndex != -1)
        {
            conversations[conversationIndex].JoinNPCToConversation(from);
        }
        else
        {
            to.OnStartConversation();
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
                JoinNPCToConversation(member);
            }
            Instance.StartCoroutine(ConversationCoroutine());
        }
        public void JoinNPCToConversation(NPCManager newNPC)
        {
            members.Add(newNPC);

            float averageX = FindAvarage(members.ConvertAll<float>(x => x.transform.position.x).ToArray());
            Debug.Log(averageX);
            foreach (var member in members)
            {
                var newLookDirection = member.transform.position.x > averageX ? 180 : 0;
                member.transform.rotation = Quaternion.Euler(0, newLookDirection, 0);
            }
        }
        private IEnumerator ConversationCoroutine()
        {
            yield return new WaitForSeconds(5);
            for (var i = 0; i < members.Count; i++)
            {
                members[i].OnEndConversation(i % 2 == 0 ? Direction.right : Direction.left);
                yield return new WaitForSeconds(0.7f);
            }
            Instance.conversations.Remove(this);
        }
        static float FindAvarage(float[] arr)
        {
            float sum = 0;
            float average = 0.0F;

            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }

            average = (float)sum / arr.Length;
            return average;
        }
    }
}
