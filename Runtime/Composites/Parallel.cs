namespace Trea
{
    public class Parallel : CompositeNode
    {
        public enum Policy
        {
            RequireOne,
            RequireAll
        }

        public Policy successPolicy = Policy.RequireAll;

        protected override NodeState OnUpdate()
        {
            int successCount = 0;
            bool anyRunning = false;

            foreach (var child in children)
            {
                var state = child.Tick();
                if (state == NodeState.Success) successCount++;
                else if (state == NodeState.Running) anyRunning = true;
                else if (successPolicy == Policy.RequireAll) return NodeState.Failure;
            }

            if (successPolicy == Policy.RequireAll)
                return anyRunning ? NodeState.Running : NodeState.Success;

            return successCount > 0 ? NodeState.Success : (anyRunning ? NodeState.Running : NodeState.Failure);
        }
    }
}
