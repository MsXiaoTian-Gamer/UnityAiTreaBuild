using System;

namespace Trea
{
    [Serializable]
    public abstract class ActionNode : BTNode
    {
        protected virtual void OnStart() { }
        protected abstract NodeState OnAction();
        protected virtual void OnStop() { }

        protected sealed override void OnEnter() => OnStart();
        protected sealed override NodeState OnUpdate() => OnAction();
        protected sealed override void OnExit() => OnStop();
    }
}
