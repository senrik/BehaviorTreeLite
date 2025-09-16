using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

namespace BehaviorTree
{

    [System.Serializable]
    public abstract class BehaviorTreeNode
    {
        protected static int ID;
        [SerializeField]
        protected int _id;
        [SerializeReference]
        protected BehaviorTreeNode firstChildNode;
        [SerializeReference]
        protected BehaviorTreeNode siblingNode;
        public BehaviorTreeNode() {
            _id = ID;
            ID++;
        }

        public BehaviorTreeNode(BehaviorTreeNode other)
        {
            this._id = other._id;
        }

        public virtual void AddNode(BehaviorTreeNode other)
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
        }

        public virtual void RemoveNode(BehaviorTreeNode other)
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
            }
        }

        public virtual BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance) { return BehaviorTreeState.None; }

        protected abstract BehaviorTreeNode CloneSelf();
        public BehaviorTreeNode Clone()
        {
            var clone = CloneSelf();
            //clone.firstChildNode = firstChildNode?.Clone();
            //clone.siblingNode = siblingNode?.Clone();
            return clone;
        }

        public int id
        {
            get { return _id; }
        }

        public BehaviorTreeNode FirstChildNode
        {
            get { return  firstChildNode; }
        }
        public BehaviorTreeNode SiblingNode
        {
            get { return siblingNode; }
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

        protected override BehaviorTreeNode CloneSelf()
        {
            return new BehaviorTreeSequencerNode(this);
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
        protected override BehaviorTreeNode CloneSelf()
        {
            return new BehaviorTreeSelectorNode(this);
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
        protected override BehaviorTreeNode CloneSelf()
        {
            return new BehaviorTreeExecutorNode(this);
        }

        public void AssignTask(BehaviorTreeTask task)
        {
            this.task = task;
        }

    }
}

