using UnityEngine;
namespace BehaviorTree
{
    public abstract class BehaviorTreeTask
    {
        public string label;

        public abstract BehaviorTreeTask Clone();
        public abstract BehaviorTreeState PerformTask(IBehaviorTreeAgent instance);
    }

    [System.Serializable]
    public class WanderTask : BehaviorTreeTask
    {
        public WanderTask()
        {
            label = "Wander";
        }
        public override BehaviorTreeTask Clone()
        {
            return new WanderTask();
        }
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
        public PatrolTask()
        {
            label = "Patrol";
        }

        public override BehaviorTreeTask Clone()
        {
            return new PatrolTask();
        }
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

