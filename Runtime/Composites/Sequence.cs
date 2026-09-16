namespace Trea
{
    public class Sequence : CompositeNode
    {
        private int current;

        protected override void OnEnter()
        {
            current = 0;
        }

        protected override NodeState OnUpdate()
        {
            for (int i = current; i < children.Count; i++)
            {
                current = i;
                var state = children[i].Tick();
                if (state == NodeState.Failure) return NodeState.Failure;
                if (state == NodeState.Running) return NodeState.Running;
            }
            return NodeState.Success;
        }
    }
}
