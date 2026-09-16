using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trea
{
    [Serializable]
    public abstract class CompositeNode : BTNode
    {
        [SerializeReference] public List<BTNode> children = new();

        public BTNode AddChild(BTNode child)
        {
            child.parent = this;
            children.Add(child);
            return child;
        }

        public void RemoveChild(BTNode child)
        {
            child.parent = null;
            children.Remove(child);
        }

        public void ClearChildren()
        {
            foreach (var child in children)
                child.parent = null;
            children.Clear();
        }

        public override BTNode Clone()
        {
            var copy = (CompositeNode)MemberwiseClone();
            copy.children = new List<BTNode>(children.Count);
            foreach (var child in children)
            {
                var cloned = child.Clone();
                cloned.parent = copy;
                copy.children.Add(cloned);
            }
            return copy;
        }

        protected override void OnExit()
        {
            foreach (var child in children)
            {
                if (child.State == NodeState.Running)
                    child.Abort();
            }
        }

        protected override void OnBindChildren()
        {
            foreach (var child in children)
                child.Bind(blackboard);
        }
    }
}
