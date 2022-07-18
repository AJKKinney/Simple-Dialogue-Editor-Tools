using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem
{
    
    //This Class Controls the Dialogue Graph Editor Window
    public class DialogueGraph : EditorWindow
    {
        private string fileName = "New Narrative";
        private DialogueGraphView graphView;


        //Opens the Dialogue Editor Window
        [MenuItem("Dialgoue/Dialogue Graph")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }


        //constructs new UI Display
        private void ConstructGraph()
        {
            graphView = new DialogueGraphView
            {
                name = "Dialogue Graph"
            };

            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }


        //Creates Dialogue Editor Window Toolbar
        private void GenerateToolbar()
        {
            //toolbar
            var toolbar = new Toolbar();

            //Create Editor Window
            var fileNameTextField = new TextField("File Name:");
            fileNameTextField.SetValueWithoutNotify("New Narrative");
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => fileName = evt.newValue);
            toolbar.Add(fileNameTextField);

            //Data Saving and Loading
            var fileDropdown = new ToolbarMenu();
            fileDropdown.text = "File";
            fileDropdown.menu.AppendAction("Save Sequence", a => { RequestDataOperation(true); }, a => DropdownMenuAction.Status.Normal);
            fileDropdown.menu.AppendAction("Save Sequence As...", a => { RequestDataOperation(true); }, a => DropdownMenuAction.Status.Normal);
            fileDropdown.menu.AppendAction("Open Sequence", a => { RequestDataOperation(false); }, a => DropdownMenuAction.Status.Normal);
            toolbar.Add(fileDropdown);

            //Create Dropdown Menu For Creating Nodes
            var newNodeDropdown = new ToolbarMenu();
            newNodeDropdown.text = "Create New Node";
            newNodeDropdown.menu.AppendAction("Dialogue Node", a => { graphView.CreateNode("Dialogue Node"); }, a => DropdownMenuAction.Status.Normal);
            newNodeDropdown.menu.AppendAction("Choice Node", a => { graphView.CreateNode("Choice Node", DialogueNode.NodeType.Choice); }, a => DropdownMenuAction.Status.Normal);
            newNodeDropdown.menu.AppendAction("Exit Node", a => { graphView.CreateNode("Exit Node", DialogueNode.NodeType.Exit); }, a => DropdownMenuAction.Status.Normal);
            toolbar.Add(newNodeDropdown);


            rootVisualElement.Add(toolbar);
        }


        //Calls Saving and loading of sequences
        private void RequestDataOperation(bool save)
        {
            if(string.IsNullOrEmpty(fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
                return;
            }

            var saveUtility = GraphSaveUtility.GetInstance(graphView);

            //for saving
            if(save)
            {
                saveUtility.SaveGraph(fileName);
            }
            //for loading
            else
            {
                saveUtility.LoadGraph(fileName);
            }
        }


        //Creates a minimap for the user to view their basic graph setup and manuever more easily
        private void GenerateMinimap()
        {
            var minimap = new MiniMap { anchored = true };
            minimap.SetPosition(new Rect(10, 30, 200, 200));
            graphView.Add(minimap);
        }


        private void OnEnable()
        {
            ConstructGraph();
            GenerateToolbar();
            GenerateMinimap();
        }


        private void OnDisable()
        {
            //Remove Graph
            rootVisualElement.Remove(graphView);
        }
    }
}
