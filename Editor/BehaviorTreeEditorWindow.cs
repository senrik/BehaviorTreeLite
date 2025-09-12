using BehaviorTree;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviorTreeEditorWindow : EditorWindow
{
    VisualTreeAsset behaviorTreeEditorWindowAsset, selectorNodeAsset,sequencerNodeAsset, executorNodeAsset;
    private string btEditorAssetPath = "Assets/Scripts/NPC/";
    BehaviorTreeAsset currentAsset;
    NodeDisplay selectedNode;
    bool selectingNode = false;
    List<NodeDisplay> nodes;
    BehaviorTreeTasks tasks;

    [MenuItem("Behavior Trees/Tree Builder")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<BehaviorTreeEditorWindow>();
        wnd.titleContent = new GUIContent("Tree Builder");
        
    }

    public void OnEnable()
    {
        behaviorTreeEditorWindowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(btEditorAssetPath+ "Behavior Trees/Editor/BehaviorTreeEditorWindow_UXML.uxml");
        selectorNodeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(btEditorAssetPath + "Behavior Trees/Editor/BehaviorTreeSelectorNode_UXML.uxml");
        sequencerNodeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(btEditorAssetPath + "Behavior Trees/Editor/BehaviorTreeSequencerNode_UXML.uxml");
        executorNodeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(btEditorAssetPath + "Behavior Trees/Editor/BehaviorTreeExecutorNode_UXML.uxml");

        nodes = new List<NodeDisplay>();
        selectedNode = new NodeDisplay();
        tasks = BehaviorTreeTasks.Instance;
    }

    public void CreateGUI()
    {
        behaviorTreeEditorWindowAsset.CloneTree(rootVisualElement);
        rootVisualElement.Q<DropdownField>("TaskDropdown").SetEnabled(false);
        foreach(System.Type t in Assembly.GetAssembly(typeof(BehaviorTreeTask)).GetTypes())
        {
            if(t != typeof(BehaviorTreeTask) && t.IsSubclassOf(typeof(BehaviorTreeTask)))
            {
                rootVisualElement.Q<DropdownField>("TaskDropdown").choices.Add(t.Name);
            }
        }
        rootVisualElement.Q<ObjectField>("BehaviorTreeAssetField").RegisterValueChangedCallback<UnityEngine.Object>((callback) =>{
            if(currentAsset != (BehaviorTreeAsset)callback.newValue)
            {
                currentAsset = (BehaviorTreeAsset)callback.newValue;
                var assetName = currentAsset != null ? currentAsset.name : "None";
                Debug.Log($"BehaviorTreeAsset: {assetName} loaded.");
                rootVisualElement.Q<ScrollView>("TreeView").Clear();
                ParseTree(currentAsset.tree);
                foreach(var node in nodes)
                {
                    rootVisualElement.Q<ScrollView>("TreeView").Add(node.nodeElem);
                }
            }
        });
        rootVisualElement.Q<Button>("AddSelectorNodeButton").clicked += () =>
        {
            var selector = new NodeDisplay();
            selector.node = new BehaviorTreeSelectorNode();
            selector.nodeElem = DrawNode(selector.node, 0);
            rootVisualElement.Q<ScrollView>("TreeView").Add(selector.nodeElem);
            nodes.Add(selector);
        };
        rootVisualElement.Q<Button>("AddSequencerNodeButton").clicked += () =>
        {
            var sequencer = new NodeDisplay();
            sequencer.node = new BehaviorTreeSequencerNode();
            sequencer.nodeElem = DrawNode(sequencer.node, 0);
            rootVisualElement.Q<ScrollView>("TreeView").Add(sequencer.nodeElem);
            nodes.Add(sequencer);
        };
        rootVisualElement.Q<Button>("AddExecutorNodeButton").clicked += () =>
        {
            var executor = new NodeDisplay();
            executor.node = new BehaviorTreeExecutorNode();
            executor.nodeElem = DrawNode(executor.node, 0);
            rootVisualElement.Q<ScrollView>("TreeView").Add(executor.nodeElem);
            nodes.Add(executor);
        };
        rootVisualElement.Q<DropdownField>("TaskDropdown").RegisterValueChangedCallback((callback) =>
        {
            var task = BehaviorTreeTasks.GetTask(callback.newValue);
            ((BehaviorTreeExecutorNode)selectedNode.node).AssignTask(task);
            selectedNode.nodeElem.Q<Label>("NodeLabel").text = callback.newValue;
        });
        rootVisualElement.Q<VisualElement>("TreeView").RegisterCallback<PointerDownEvent>((callback) =>
        {
            if(selectedNode.nodeElem != null)
            {
                if (selectingNode)
                {
                    selectingNode = false;
                }
                else
                {
                    selectedNode.nodeElem.Q<VisualElement>("NodeBodyElem").RemoveFromClassList("selected-node");
                    selectedNode.node = null;
                    selectedNode.nodeElem = null;
                    rootVisualElement.Q<DropdownField>("TaskDropdown").SetEnabled(false);
                }
                
            }
        });

    }

    private void ParseTree(BehaviorTree.BehaviorTree t)
    {
        var queue = new List<BehaviorTreeNode>();
        queue.Add(t.Root);
        nodes.Clear();
        while(queue.Count > 0)
        {
            var current = queue[0];
            NodeDisplay display = new NodeDisplay();
            display.node = current;
            display.nodeElem = DrawNode(current, queue.Count);
            nodes.Add(display);
            queue.RemoveAt(0);

            var child = current.firstChildNode;
            while(child != null)
            {
                queue.Add((BehaviorTreeNode)child);

                child = child.siblingNode;
            }
        }
    }

    private VisualElement DrawNode(BehaviorTreeNode node, int width)
    {
        var root = new VisualElement();
        if(node is BehaviorTreeSelectorNode)
        {
            selectorNodeAsset.CloneTree(root);
        }else if (node is BehaviorTreeSequencerNode)
        {
            sequencerNodeAsset.CloneTree(root);
        }else if(node is BehaviorTreeExecutorNode)
        {
            executorNodeAsset.CloneTree(root);
        }else
        {
            Debug.Log($"Root Node");
        }
        var position = Vector2.zero;

        
        position.x = rootVisualElement.Q<ScrollView>("TreeView").worldBound.width / 2;
        position.y = rootVisualElement.Q<ScrollView>("TreeView").worldBound.height / 2;
        root.transform.position = position;
        DragAndDropManipulator manipulator = new(root);

        root.Q<VisualElement>("NodeBodyElem").RegisterCallback<PointerDownEvent>((callback) =>
        {
            selectingNode = true;
            if (selectedNode.node != node)
            {
                if(selectedNode.nodeElem != null)
                {
                    selectedNode.nodeElem.Q<VisualElement>("NodeBodyElem").RemoveFromClassList("selected-node");
                }
                selectedNode.node = node;
                selectedNode.nodeElem = root;
                if(node is BehaviorTreeExecutorNode)
                {
                    rootVisualElement.Q<DropdownField>("TaskDropdown").SetEnabled(true);
                }
                    
                root.Q<VisualElement>("NodeBodyElem").AddToClassList("selected-node");
                
            }
            
        });

        return root;
    }

    struct NodeDisplay
    {
        public BehaviorTreeNode node;
        public VisualElement nodeElem;
    }
}
