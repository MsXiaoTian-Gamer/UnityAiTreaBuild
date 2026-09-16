namespace Trea
{
    public class Succeeder : DecoratorNode
    {
        protected override NodeState OnUpdate()
        {
            if (child == null) return NodeState.Success;
            var state = child.Tick();
            return state == NodeState.Running ? NodeState.Running : NodeState.Success;
        }
    }
}
