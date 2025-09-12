using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

namespace BehaviorTree
{
    [System.Serializable]
    public abstract class BehaviorTreeNodeBase
    {
        protected static int ID;
        [SerializeField]
        protected int _id;
        [SerializeField]
        protected int childCount;
        [SerializeReference]
        public BehaviorTreeNodeBase firstChildNode;
        [SerializeReference]
        public BehaviorTreeNodeBase siblingNode;

        public virtual BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance) { return BehaviorTreeState.None; }
        public abstract void AddNode(BehaviorTreeNodeBase other);

        public abstract void RemoveNode(BehaviorTreeNodeBase other);

        public int id
        {
            get { return _id; }
        }
    }

    [System.Serializable]
    public class BehaviorTreeNode : BehaviorTreeNodeBase
    {
        public BehaviorTreeNode() {
            _id = ID;
            ID++;
        }

        public BehaviorTreeNode(BehaviorTreeNode other)
        {
            this._id = other._id;
            this.childCount = other.childCount;
        }

        public override void AddNode(BehaviorTreeNodeBase other)
        {
            if (firstChildNode == null)
                firstChildNode = other;
            else
            {
                var current = firstChildNode;
                while (current.siblingNode != null)
                {
                    current = current.siblingNode;
                }
                current.siblingNode = other;
            }
            this.childCount++;
        }

        public override void RemoveNode(BehaviorTreeNodeBase other)
        {
            if (this.firstChildNode != null)
            {
                if (this.firstChildNode.id == other.id)
                {
                    var temp = this.firstChildNode.siblingNode;
                    this.firstChildNode = temp;
                }
                else
                {
                    var current = this.firstChildNode;
                    var prev = this.firstChildNode;
                    while (current != null)
                    {
                        if (current.id == other.id)
                        {
                            prev.siblingNode = current.siblingNode;
                            break;
                        }
                        prev = current;
                        current = current.siblingNode;
                    }
                }
                this.childCount--;
            }
        }

        
    }

    [System.Serializable]
    public class BehaviorTreeSequencerNode : BehaviorTreeNode
    {
        public BehaviorTreeSequencerNode() : base() { }
        public BehaviorTreeSequencerNode(BehaviorTreeSequencerNode other) : base(other) { }
        public override BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance)
        {
            var output = BehaviorTreeState.None;
            var current = this.firstChildNode;
            while (current != null)
            {
                output = current.EvaluateNode(instance);
                if (output == BehaviorTreeState.Failure)
                    break;
            }
            return output;
        }
    }

    [System.Serializable]
    public class BehaviorTreeSelectorNode : BehaviorTreeNode
    {
        public BehaviorTreeSelectorNode() : base() { }
        public BehaviorTreeSelectorNode(BehaviorTreeSelectorNode other) : base(other) { }
        public override BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance)
        {
            var output = BehaviorTreeState.None;
            var current = this.firstChildNode;
            while(current!= null)
            {
                output = current.EvaluateNode(instance);
                if (output != BehaviorTreeState.Failure)
                    break;
            }
            return output;
        }
    }

    [System.Serializable]
    public class BehaviorTreeExecutorNode : BehaviorTreeNode
    {
        [SerializeReference]
        private BehaviorTreeTask task;

        public BehaviorTreeExecutorNode() : base()
        {
            this.task = null;
        }

        public BehaviorTreeExecutorNode(BehaviorTreeExecutorNode other) :base(other)
        {
            this.task = other.task.Clone();
        }
        public override BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance)
        {
            return this.task.PerformTask(instance);
        }

        public void AssignTask(BehaviorTreeTask task)
        {
            this.task = task;
        }
    }
}

