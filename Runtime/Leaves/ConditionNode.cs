using System;

namespace Trea
{
    [Serializable]
    public abstract class ConditionNode : BTNode
    {
        protected abstract bool Check();

        protected sealed override NodeState OnUpdate()
        {
            return Check() ? NodeState.Success : NodeState.Failure;
        }
    }
}
