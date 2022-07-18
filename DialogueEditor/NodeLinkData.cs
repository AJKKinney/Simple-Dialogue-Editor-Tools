using System;


namespace DialogueSystem
{
    [Serializable]
    //contains data about the connections between nodes
    public class NodeLinkData
    {

        public string BaseNodeGuid;
        public string PortName;
        public string TargetNodeGuid;
    }
}
