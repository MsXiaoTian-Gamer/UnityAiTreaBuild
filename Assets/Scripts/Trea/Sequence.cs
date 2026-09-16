using UnityEngine;
using System.Collections.Generic;

public class Sequence : BTNode
{
    private readonly List<BTNode> children;
    public Sequence(List<BTNode> children)
    {
        this.children = children;
    }
    public override State Evaluate()
    {
        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case State.Failure:
                    return State.Failure;
                case State.Running:
                    return State.Running;
                case State.Success:
                    continue;
            }
        }
        return State.Success;
    }
}