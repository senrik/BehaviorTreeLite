using UnityEngine;
namespace BehaviorTree
{
    public abstract class BehaviorTreeTask
    {
        public string label;
        public abstract BehaviorTreeState PerformTask(IBehaviorTreeAgent instance);
    }

    [System.Serializable]
    public class WanderTask : BehaviorTreeTask
    {
        public override BehaviorTreeState PerformTask(IBehaviorTreeAgent instance)
        {
            if(instance.MovementType == MovementType.Wander)
            {
                return BehaviorTreeState.Running;
            }
            else
            {
                return BehaviorTreeState.Failure;
            }
                
        }
    }

    [System.Serializable]
    public class PatrolTask : BehaviorTreeTask
    {
        public override BehaviorTreeState PerformTask(IBehaviorTreeAgent instance)
        {
            if (instance.MovementType == MovementType.Patrol)
            {
                return BehaviorTreeState.Running;
            }
            else
            {
                return BehaviorTreeState.Failure;
            }
        }
    }
}

