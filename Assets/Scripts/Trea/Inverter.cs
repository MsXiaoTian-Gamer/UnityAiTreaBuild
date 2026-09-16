using UnityEngine;
using System.Collections.Generic;
public class Inverter : BTNode
{
    private readonly BTNode child;
    private Inverter(BTNode child)
    {
        this.child = child;
    }
    public override State Evaluate()
    {
        switch (child.Evaluate())
        {
            case State.Success:
                return State.Failure;
            case State.Failure:
                return State.Success;
            default:
                return State.Failure;
        }
    }
}
