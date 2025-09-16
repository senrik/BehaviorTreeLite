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
    
    HashSet<NodeEdge> edges;
    NodeEdge openEdge;
    bool makingConnection = false;
    Vector2 pointerPosition;
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
        edges = new HashSet<NodeEdge>();
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
                    rootVisualElement.Q<ScrollView>("TreeView").MarkDirtyRepaint();

                }
                
            }
        });
        rootVisualElement.Q<Button>("AddSelectorNodeButton").clicked += () =>
        {
            var selector = new NodeDisplay();
            selector.node = new BehaviorTreeSelectorNode();
            selector.nodeElem = DrawNode(selector.node, 0);
            selector.nodeElem.Q<VisualElement>("OutConnections").RegisterCallback<PointerDownEvent>((callback) =>
            {
                Debug.Log("Start Connection...");
                openEdge.start = selector;
                makingConnection = true;
            });
            selector.nodeElem.Q<VisualElement>("InConnections").RegisterCallback<PointerDownEvent>((callback) =>
            {
                if(openEdge.start.nodeElem != null)
                {
                    Debug.Log("End Connection");
                    openEdge.end = selector;
                    edges.Add(openEdge);
                    openEdge = new NodeEdge();
                    rootVisualElement.MarkDirtyRepaint();
                }
            });
            rootVisualElement.Q<ScrollView>("TreeView").Add(selector.nodeElem);
            nodes.Add(selector);
        };
        rootVisualElement.Q<Button>("AddSequencerNodeButton").clicked += () =>
        {
            var sequencer = new NodeDisplay();
            sequencer.node = new BehaviorTreeSequencerNode();
            sequencer.nodeElem = DrawNode(sequencer.node, 0);
            sequencer.nodeElem.Q<VisualElement>("OutConnections").RegisterCallback<PointerDownEvent>((callback) =>
            {
                Debug.Log("Start Connection...");
                openEdge.start = sequencer;
                makingConnection = true;
            });
            sequencer.nodeElem.Q<VisualElement>("InConnections").RegisterCallback<PointerDownEvent>((callback) =>
            {
                if (openEdge.start.nodeElem != null)
                {
                    Debug.Log("End Connection");
                    openEdge.end = sequencer;
                    edges.Add(openEdge);
                    openEdge = new NodeEdge();
                    rootVisualElement.MarkDirtyRepaint();
                }
            });
            rootVisualElement.Q<ScrollView>("TreeView").Add(sequencer.nodeElem);
            nodes.Add(sequencer);
        };
        rootVisualElement.Q<Button>("AddExecutorNodeButton").clicked += () =>
        {
            var executor = new NodeDisplay();
            executor.node = new BehaviorTreeExecutorNode();
            executor.nodeElem = DrawNode(executor.node, 0);
            executor.nodeElem.Q<VisualElement>("OutConnections").RegisterCallback<PointerDownEvent>((callback) =>
            {
                Debug.Log("Start Connection...");
                openEdge.start = executor;
                makingConnection = true;
            });
            executor.nodeElem.Q<VisualElement>("InConnections").RegisterCallback<PointerDownEvent>((callback) =>
            {
                if (openEdge.start.nodeElem != null)
                {
                    Debug.Log("End Connection");
                    openEdge.end = executor;
                    edges.Add(openEdge);
                    openEdge = new NodeEdge();
                    rootVisualElement.MarkDirtyRepaint();
                }
            });
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

            if(openEdge.start.nodeElem != null)
            {
                if (makingConnection)
                {
                    makingConnection = false;
                }else
                {
                    
                    if(openEdge.end.nodeElem == null)
                    {
                        Debug.Log("Clearing Connection");
                        openEdge.start = new NodeDisplay();
                    }
                        
                }
            }

        });

        rootVisualElement.generateVisualContent += DrawEdges;

    }

    void DrawEdges(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;
        painter.lineWidth = 10;
        painter.strokeColor = Color.white;
        foreach(var edge in edges)
        {
            painter.BeginPath();
            var lineStart = edge.start.nodeElem.Q<VisualElement>("OutConnections").worldBound.center;
            lineStart.y -= edge.start.nodeElem.Q<VisualElement>("OutConnections").worldBound.height;
            var lineEnd = edge.end.nodeElem.Q<VisualElement>("InConnections").worldBound.center;
            painter.MoveTo(lineStart);
            painter.LineTo(lineEnd);
            painter.Stroke();
            
        }
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
            

            var child = current.FirstChildNode;
            while(child != null)
            {
                queue.Add(child);

                child = child.SiblingNode;
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
            rootVisualElement.MarkDirtyRepaint();
        });
        

        return root;
    }

    private void Update()
    {
        Debug.Log($"Edges Count: {edges.Count}");
    }

    struct NodeDisplay
    {
        public BehaviorTreeNode node;
        public VisualElement nodeElem;
    }

    struct NodeEdge : IComparable<NodeEdge> {
        public NodeDisplay start;
        public NodeDisplay end;

        public readonly int CompareTo(NodeEdge other)
        {
            if(this.start.nodeElem == other.start.nodeElem && this.end.nodeElem == other.end.nodeElem)
            {
                return 0;
            }
            else
            {
                return -1;
            }

        }
    }
}
