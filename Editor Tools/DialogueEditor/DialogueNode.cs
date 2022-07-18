using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem
{

    //This Class contains node data for the dialogue system
    public class DialogueNode : Node
    {
        public string GUID;
        public string dialogueText;
        public DialogueSpeaker speaker;
        public bool entryPoint = false;
        public NodeType nodeType = NodeType.Dialogue;


        public enum NodeType
        {
            Entry,
            Dialogue,
            Choice,
            Test,
            Exit
        }
    }
}
