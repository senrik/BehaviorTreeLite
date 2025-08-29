using UnityEngine;

namespace BehaviorTree
{
    public class BehaviorTree
    {
        BehaviorTreeNode root;
        public void EvaluateTree(IBehaviorTreeAgent instance)
        {
            root.EvaluateNode(instance);
        }

        public BehaviorTreeNode Root
        {
            get { return root; }
        }
    }

    public enum BehaviorTreeState
    {
        None = 0,
        Running = 1,
        Success = 2,
        Failure = 3,
    }

    public interface IBehaviorTreeAgent
    {

    }

    public class BehaviorStats
    {
        public float attackRange;
        public float attackDotProd;
        public float movementSpeed;
        public float wanderMovementSpeed;
        public float tetherRange;
    }
}

