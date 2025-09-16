using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


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
            var queue = new List<(BehaviorTreeNode, BehaviorTreeNode)>();
            
            
            this.root = other.root.Clone();
            queue.Add((other.root, this.root));
            BehaviorTreeNode header = this.root;

            while (queue.Count > 0) {
                var temp = queue[0];
                queue.RemoveAt(0);
                nodes.Add(temp.Item2);
                var current = temp.Item1.FirstChildNode;
                
                while(current != null)
                {

                    var child = current.Clone();
                    temp.Item2.AddNode(child);
                    queue.Add((current, child));
                    current = current.SiblingNode;
                }
            }
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

    public class BehaviorTreeTasks
    {
        private static BehaviorTreeTasks instance;
        private static Dictionary<string, Type> tasks;
        public static BehaviorTreeTask GetTask(string label)
        {
            BehaviorTreeTask output = null;
            try
            {
                output = (BehaviorTreeTask)Activator.CreateInstance(tasks[label]);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }

            return output;
        }

        private BehaviorTreeTasks()
        {
            tasks = new Dictionary<string, Type>();
            foreach (System.Type t in Assembly.GetAssembly(typeof(BehaviorTreeTask)).GetTypes())
            {
                if (t != typeof(BehaviorTreeTask) && t.IsSubclassOf(typeof(BehaviorTreeTask)))
                {
                    tasks.Add(t.Name, t);
                }
            }
        }

        public static BehaviorTreeTasks Instance{
            get { 
                if(instance == null)
                    instance = new BehaviorTreeTasks();
                return instance;
            }
        }
    }
}

