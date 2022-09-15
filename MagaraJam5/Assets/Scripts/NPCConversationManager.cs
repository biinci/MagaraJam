using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCConversationManager : MonoBehaviour
{
    public static NPCConversationManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    private List<Conversation> conversations = new List<Conversation>();
    public void MakeInterractionWith(NPCManager from, NPCManager to)
    {
        from.OnStartConversation();

        int conversationIndex = conversations.FindIndex(x => x.Members.Contains(to));
        if (conversationIndex != -1)
        {
            conversations[conversationIndex].Members.Add(from);
        }
        else
        {
            conversations.Add(new Conversation(from, to));
        }
    }
    class Conversation
    {
        public List<NPCManager> Members = new List<NPCManager>();
        public Conversation(params NPCManager[] firstMembers)
        {
            Debug.Log("Diyalog başladı");
            foreach (var member in firstMembers)
            {
                Members.Add(member);
            }
            NPCConversationManager.Instance.StartCoroutine(ConversationCoroutine());
        }
        IEnumerator ConversationCoroutine()
        {
            yield return new WaitForSeconds(10);
            for (int i = 0; i < Members.Count; i++)
            {
                Members[i].OnEndConversation(i % 2 == 0 ? Direction.right : Direction.left);
                Debug.Log(Members[i].name);
                yield return new WaitForSeconds(0.7f);
            }
            NPCConversationManager.Instance.conversations.Remove(this);
        }
    }
}
