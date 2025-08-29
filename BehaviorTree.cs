using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


namespace BehaviorTree
{
    [System.Serializable]
    public class BehaviorTree
    {
        [SerializeReference]
        BehaviorTreeNode root;
        [SerializeField]
        List<BehaviorTreeNode> nodes;

        public BehaviorTree()
        {
            nodes = new List<BehaviorTreeNode>();
        }
        public BehaviorTree(BehaviorTree other)
        {
            // TODO: Implement deep copy
        }

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
        public MovementType MoveType
        {
            get; set;
        }

        public List<Vector3> PatrolPath
        {
            get; set;
        }

        public int CurrentPatrolPoint
        {
            get; set;
        }

        public float PatrolThreshold
        {
            get;
        }

        public Transform Target
        {
            get;

        }
        public NavMeshAgent Agent
        {
            get;
        }

        public Animator CharAnim
        {
            get;
        }

        public Vector3 TetherPoint
        {
            get;
        }

        public Transform Transform
        {
            get;
        }

        public bool InAttackRange
        {
            get;
            set;
        }

        public MovementType MovementType { get; }

        public float WanderWaitTimer
        {
            get; set;
        }

        public Vector3 WanderPoint
        {
            get; set;
        }

        public float WanderRange
        {
            get; set;
        }

        public BehaviorStats Stats
        {
            get;
        }
    }

    [System.Serializable]
    public class BehaviorStats
    {
        public float attackRange;
        public float attackDotProd;
        public float movementSpeed;
        public float wanderMovementSpeed;
        public float tetherRange;
    }
}

