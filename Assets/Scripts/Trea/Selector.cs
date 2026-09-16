using UnityEngine;
using System.Collections.Generic;
public class Selector : BTNode
{
    private readonly List<BTNode> children;
    public Selector(List<BTNode> children)
    {
        this.children = children;
    }
    public override State Evaluate()
    {
        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case State.Success:
                    return State.Success;
                case State.Running:
                    return State.Running;
                case State.Failure:
                    continue;
            }
        }
        return State.Failure;
    }
}