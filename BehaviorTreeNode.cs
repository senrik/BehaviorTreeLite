using UnityEngine;

namespace BehaviorTree
{
    [System.Serializable]
    public class BehaviorTreeNode
    {
        protected static int ID;
        [SerializeField]
        protected int id;
        [SerializeReference]
        protected BehaviorTreeNode firstChildNode;
        [SerializeReference]
        protected BehaviorTreeNode siblingNode;
        public virtual BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance) { return BehaviorTreeState.None; }
        public void AddNode(BehaviorTreeNode other) {
            if(firstChildNode == null)
                firstChildNode = other;
            else
            {
                var current = firstChildNode;
                while (current != null)
                {
                    current = current.siblingNode;
                }
                current = other;
            }
        }

        public void RemoveNode(BehaviorTreeNode other)
        {
            if(this.firstChildNode != null)
            {
                if(this.firstChildNode.id == other.id)
                {
                    var temp = this.firstChildNode.siblingNode;
                    this.firstChildNode = temp;
                }
                else
                {
                    var current = this.firstChildNode;
                    var prev = this.firstChildNode;
                    while(current != null)
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
    }

    [System.Serializable]
    public class BehaviorTreeSequencerNode : BehaviorTreeNode
    {
        public BehaviorTreeSequencerNode() {
            id = ID;
            ID++;
        }
        public BehaviorTreeSequencerNode(BehaviorTreeSequencerNode other)
        {
            this.id = other.id;
        }
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
        public BehaviorTreeSelectorNode()
        {
            id = ID;
            ID++;
        }
        public BehaviorTreeSelectorNode(BehaviorTreeSelectorNode other)
        {
            this.id = other.id;
        }
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

