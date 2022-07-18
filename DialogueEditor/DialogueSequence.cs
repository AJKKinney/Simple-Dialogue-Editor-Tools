using System;
using System.Collections.Generic;
using UnityEngine;


namespace DialogueSystem
{
    [Serializable]
    public class DialogueSequence : ScriptableObject
    {
        public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
        public List<DialogueNodeData> dialogueNodeData = new List<DialogueNodeData>();
    }
}
