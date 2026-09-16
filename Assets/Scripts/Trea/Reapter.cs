using UnityEngine;
using System.Collections.Generic;
public class Reapter : BTNode
{
    private readonly BTNode child;
    private readonly int maxRepeats;//-1表示无限循环
    private int currentRepeats;
    private Reapter(BTNode child)
    {
        this.child = child;
    }
    public override State Evaluate()
    {
        if (maxRepeats > 0 && currentRepeats >= maxRepeats)
        {
            return State.Success;
        }
        State state = child.Evaluate();
        if (state != State.Running)
        {
            currentRepeats++;
            return currentRepeats>maxRepeats?state:State.Running;
        }
        return State.Running;
    }
}
