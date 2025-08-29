using UnityEngine;

[CreateAssetMenu(fileName = "BehaviorTreeAsset", menuName = "Scriptable Objects/BehaviorTreeAsset")]
public class BehaviorTreeAsset : ScriptableObject
{
    public BehaviorTree.BehaviorTree tree;
}
