using UnityEngine;
namespace BehaviorTree
{
    public abstract class BehaviorTreeTask
    {
        public string label;
        public abstract BehaviorTreeState PerformTask(IBehaviorTreeAgent instance);
    }
}

