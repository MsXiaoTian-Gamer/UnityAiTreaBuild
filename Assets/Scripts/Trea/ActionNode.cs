using UnityEngine;

public class ActionNode : BTNode
{
    private readonly System.Func<State> action;
    public ActionNode(System.Func<State> action)
    {
       this.action = action;
    }
    public override State Evaluate()
    {
       return action();
    }
}