namespace Trea
{
    public class UntilSuccess : DecoratorNode
    {
        protected override NodeState OnUpdate()
        {
            if (child == null) return NodeState.Failure;
            var state = child.Tick();
            if (state == NodeState.Success) return NodeState.Success;
            return NodeState.Running;
        }
    }
}
