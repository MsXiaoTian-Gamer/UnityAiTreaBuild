using UnityEngine;

public class ConditionNode : BTNode
{
    private readonly System.Func<bool> condition;
    public ConditionNode(System.Func<bool> condition)
    {
        this.condition = condition;
    }
    public override State Evaluate()
    {
        return condition() ? State.Success : State.Failure;
    }
}