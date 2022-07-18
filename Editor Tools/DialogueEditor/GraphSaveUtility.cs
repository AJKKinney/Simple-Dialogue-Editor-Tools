using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class GraphSaveUtility
    {
        private DialogueGraphView targetGraphView;
        private DialogueSequence sequence;

        private List<Edge> Edges => targetGraphView.edges.ToList();
        private List<DialogueNode> Nodes => targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

        public static GraphSaveUtility GetInstance(DialogueGraphView graphView)
        {
            return new GraphSaveUtility
            {
                targetGraphView = graphView
            };
        }

        public void SaveGraph(string fileName)
        {
            if(Edges.Any() != true)
            {
                return;
            }

            var dialogueSequence = ScriptableObject.CreateInstance<DialogueSequence>();

            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

            for(int i = 0; i < connectedPorts.Length; i++)
            {
                var outputNode = connectedPorts[i].output.node as DialogueNode;
                var inputNode = connectedPorts[i].input.node as DialogueNode;

                dialogueSequence.nodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGuid = outputNode.GUID,
                    PortName = connectedPorts[i].output.portName,
                    TargetNodeGuid = inputNode.GUID
                });
            }

            //save the nodes
            foreach(var dialogueNode in Nodes.Where(node => node.entryPoint == false))
            {
                dialogueSequence.dialogueNodeData.Add(new DialogueNodeData
                {
                    guid = dialogueNode.GUID,
                    dialogueText = dialogueNode.dialogueText,
                    speaker = dialogueNode.speaker,
                    Position = dialogueNode.GetPosition().position,
                    nodeType = dialogueNode.nodeType
                });;;
            }

            //Creates a folder for dialogue data if one doesn't exist
            if(AssetDatabase.IsValidFolder("Assets/Resources") == false)
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if(AssetDatabase.IsValidFolder("Assets/Resources/Dialogue") == false)
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Dialogue");
            }

            AssetDatabase.CreateAsset(dialogueSequence, $"Assets/Resources/Dialogue/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }

        public void LoadGraph(string fileName)
        {
            sequence = Resources.Load<DialogueSequence>($"Dialogue/{ fileName}");

            if(sequence == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exist!", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
        }



        private void ConnectNodes()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                var connections = sequence.nodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();

                for (int z = 0; z < connections.Count; z++)
                {
                    var targetNodeGuid = connections[z].TargetNodeGuid;
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);

                    LinkNodes(Nodes[i].outputContainer[z].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(sequence.dialogueNodeData.First(x => x.guid == targetNodeGuid).Position,
                        targetGraphView.defaultNodeSize));
                }
            }
        }


        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            
            targetGraphView.Add(tempEdge);
        }


        private void CreateNodes()
        {
            foreach(var nodeData in sequence.dialogueNodeData)
            {
                DialogueNode tempNode;

                if (nodeData.nodeType == DialogueNode.NodeType.Dialogue)
                {
                   tempNode = targetGraphView.CreateDialogueNode(nodeData.dialogueText, false);
                }
                else if(nodeData.nodeType == DialogueNode.NodeType.Choice)
                {
                    tempNode = targetGraphView.CreateChoiceNode(nodeData.dialogueText, false);
                }
                else
                {
                    tempNode = targetGraphView.GenerateExitPointNode();
                }

                tempNode.GUID = nodeData.guid;
                targetGraphView.AddElement(tempNode);

                var nodePorts = sequence.nodeLinks.Where(x => x.BaseNodeGuid == nodeData.guid).ToList();
                //nodePorts.ForEach(x => targetGraphView.AddBranchPort(tempNode, x.PortName)); Reimplemented

                for(int i = 0; i < nodePorts.Count; i++)
                {

                    targetGraphView.AddBranchPort(tempNode, nodePorts[i].PortName, i != 0);
                }
            }
        }


        private void ClearGraph()
        {
            Nodes.Find(x => x.entryPoint).GUID = sequence.nodeLinks[0].BaseNodeGuid;

            foreach(var node in Nodes)
            {
                if(node.entryPoint == true)
                {
                    continue;
                }

                //Remove edges that are connected to this node
                Edges.Where(x => x.input.node == node).ToList().ForEach(edge => targetGraphView.RemoveElement(edge));

                //then remove node
                targetGraphView.RemoveElement(node);
            }
        }


        public void CreateSpeaker(string fileName)
        {
            var dialogueSpeaker = ScriptableObject.CreateInstance<DialogueSpeaker>();

            dialogueSpeaker.speakerName = fileName;

            //Creates a folder for dialogue data if one doesn't exist
            if (AssetDatabase.IsValidFolder("Assets/Resources") == false)
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (AssetDatabase.IsValidFolder("Assets/Resources/Dialogue") == false)
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Dialogue");
            }

            AssetDatabase.CreateAsset(dialogueSpeaker, $"Assets/Resources/Dialogue/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }
    }
}
