using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace DialogueSystem
{

    //This Class Controls the Visual UI Elements of the Dialogue Graph
    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

        SerializedProperty speaker;


        //Constructor
        public DialogueGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
            //Enable zoom
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            //Enable Manipulation of UI Elements
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            //Add Background
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            //Generate Start Node
            AddElement(GenerateEntryPointNode());
        }


        private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); //Arbitrary type
        }


        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach((port) =>
            {
                if(startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }


        public void CreateNode(string nodeName, DialogueNode.NodeType type = DialogueNode.NodeType.Dialogue)
        {
            if (type == DialogueNode.NodeType.Dialogue)
            {
                AddElement(CreateDialogueNode(nodeName));
            }
            else if(type == DialogueNode.NodeType.Choice)
            {
                AddElement(CreateChoiceNode(nodeName));
            }
            else if(type == DialogueNode.NodeType.Exit)
            {
                AddElement(GenerateExitPointNode());
            }
        }


        //Generates the Dialogue Start Node
        private DialogueNode GenerateEntryPointNode()
        {
            //Create Node
            var node = new DialogueNode
            {
                title = "START",
                GUID = Guid.NewGuid().ToString(),
                dialogueText = "ENTRYPOINT",
                entryPoint = true,
                nodeType = DialogueNode.NodeType.Entry
            };

            //generate output port
            var port = GeneratePort(node, Direction.Output);
            port.portName = "Next";
            node.outputContainer.Add(port);

            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= ~Capabilities.Deletable;

            //refresh UI
            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));
            return node;
        }


        //Generates the Dialogue Exit Node
        public DialogueNode GenerateExitPointNode()
        {
            //Create Node
            var node = new DialogueNode
            {
                title = "EXIT",
                GUID = Guid.NewGuid().ToString(),
                dialogueText = "EXITPOINT",
                nodeType = DialogueNode.NodeType.Exit
            };

            //generate input port
            var port = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            port.portName = "Input";
            node.inputContainer.Add(port);

            //stylize
            node.styleSheets.Add(Resources.Load<StyleSheet>("ExitNode"));

            //refresh UI
            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));

            return node;
        }


        //creates a standard dialogue node
        public DialogueNode CreateDialogueNode(string nodeName, bool newNode = true)
        {
            var dialogueNode = new DialogueNode
            {
                title = nodeName,
                dialogueText = nodeName,
                GUID = Guid.NewGuid().ToString()
            };

            //create input ports
            var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            dialogueNode.inputContainer.Add(inputPort);

            dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("DialogueNode"));

            if (newNode == true)
            {
                //create ouput
                AddBranchPort(dialogueNode, "", false);
            }

            //add speaker field

            //add dialogue text
            var textField = new TextField(string.Empty, int.MaxValue, true, false, '*'); 
            textField.RegisterValueChangedCallback(evt =>
            {
                dialogueNode.dialogueText = evt.newValue;
                dialogueNode.title = evt.newValue;
            });
            textField.SetValueWithoutNotify(dialogueNode.title);
            dialogueNode.mainContainer.Add(textField);

            //refresh ui
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();

            //set size
            dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

            return dialogueNode;
        }


        //creates dialogue branches with player choices
        public DialogueNode CreateChoiceNode(string nodeName, bool newNode = true)
        {
            var dialogueNode = CreateDialogueNode(nodeName, newNode);

            dialogueNode.styleSheets.Remove(Resources.Load<StyleSheet>("DialogueNode"));
            dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("ChoiceNode"));

            //create dialogue branch button
            var button = new Button(() => { AddBranchPort(dialogueNode); });
            button.text = "New Branch";
            dialogueNode.titleContainer.Add(button);

            //set type
            dialogueNode.nodeType = DialogueNode.NodeType.Choice;

            return dialogueNode;
        }


        //Adds a new branch to a dialogue node
        public void AddBranchPort(DialogueNode dialogueNode, string overriddenPortName = "", bool canDelete = true)
        {
            var port = GeneratePort(dialogueNode, Direction.Output);

            var oldLabel = port.contentContainer.Q<Label>("type");
            port.contentContainer.Remove(oldLabel);

            var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;

            var branchPortName = string.IsNullOrEmpty(overriddenPortName)
                ? $"Branch {outputPortCount + 1}"
                : overriddenPortName;

            var textField = new TextField
            {
                name = string.Empty,
                value = branchPortName
            };

            textField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
            port.contentContainer.Add(new Label(" "));
            port.contentContainer.Add(textField);

            if (canDelete == true)
            {
                //add delete button
                var deleteButton = new Button(() => RemovePort(dialogueNode, port))
                {
                    text = "X"
                };
                port.contentContainer.Add(deleteButton);
            }

            port.portName = branchPortName;

            dialogueNode.outputContainer.Add(port);

            //refresh UI
            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();
        }


        private void RemovePort(DialogueNode dialogueNode, Port port)
        {
            var targetEdge = edges.ToList().Where(x => x.output.portName == port.portName && x.output.node == port.node);

            dialogueNode.outputContainer.Remove(port);
            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();

            if (targetEdge.Any() != true)
            {
                return;
            }

            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }
    }
}
