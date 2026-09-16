using System;
using UnityEngine;

namespace Trea
{
    [Serializable]
    public abstract class BTNode
    {
        public string nodeName;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 graphPosition;

        [NonSerialized] public BTNode parent;
        [NonSerialized] public Blackboard blackboard;

        public NodeState State { get; private set; }
        public bool Started { get; private set; }

        public BTNode()
        {
            nodeName = GetType().Name;
            guid = Guid.NewGuid().ToString();
        }

        public NodeState Tick()
        {
            if (!Started)
            {
                OnEnter();
                Started = true;
            }
            State = OnUpdate();
            if (State != NodeState.Running)
            {
                OnExit();
                Started = false;
            }
            return State;
        }

        public void Abort()
        {
            if (!Started && State != NodeState.Running) return;
            OnExit();
            Started = false;
            State = NodeState.Failure;
        }

        public void Bind(Blackboard target)
        {
            blackboard = target;
            OnBindChildren();
        }

        public virtual BTNode Clone()
        {
            return (BTNode)MemberwiseClone();
        }

        protected virtual void OnEnter() { }
        protected abstract NodeState OnUpdate();
        protected virtual void OnExit() { }
        protected virtual void OnBindChildren() { }
    }
}
