namespace Trea
{
    public class Inverter : DecoratorNode
    {
        protected override NodeState OnUpdate()
        {
            if (child == null) return NodeState.Failure;
            switch (child.Tick())
            {
                case NodeState.Success: return NodeState.Failure;
                case NodeState.Failure: return NodeState.Success;
                default: return NodeState.Running;
            }
        }
    }
}
