using UnityEngine;

namespace BehaviorTree
{
    public abstract class BehaviorTreeNode
    {
        public static int ID;
        protected int id;
        public abstract BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance);
    }

    public class BehaviorTreeSequencerNode : BehaviorTreeNode
    {
        public BehaviorTreeSequencerNode() {
            id = ID;
            ID++;
        }
        public override BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BehaviorTreeSelectorNode : BehaviorTreeNode
    {
        public BehaviorTreeSelectorNode()
        {
            id = ID;
            ID++;
        }
        public override BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BehaviorTreeExecutorNode : BehaviorTreeNode
    {
        private BehaviorTreeTask task;
        public override BehaviorTreeState EvaluateNode(IBehaviorTreeAgent instance)
        {
            throw new System.NotImplementedException();
        }

        public void AssignTask(BehaviorTreeTask task)
        {
            this.task = task;
        }
    }
}

