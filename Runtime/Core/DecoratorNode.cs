using System;
using UnityEngine;

namespace Trea
{
    [Serializable]
    public abstract class DecoratorNode : BTNode
    {
        [SerializeReference] public BTNode child;

        public BTNode SetChild(BTNode node)
        {
            node.parent = this;
            child = node;
            return node;
        }

        public void RemoveChild()
        {
            if (child != null) child.parent = null;
            child = null;
        }

        public override BTNode Clone()
        {
            var copy = (DecoratorNode)MemberwiseClone();
            if (child != null)
            {
                copy.child = child.Clone();
                copy.child.parent = copy;
            }
            return copy;
        }

        protected override void OnExit()
        {
            if (child != null && child.State == NodeState.Running)
                child.Abort();
        }

        protected override void OnBindChildren()
        {
            if (child != null)
                child.Bind(blackboard);
        }
    }
}
