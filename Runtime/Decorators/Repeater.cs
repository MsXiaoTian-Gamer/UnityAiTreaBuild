namespace Trea
{
    public class Repeater : DecoratorNode
    {
        public int repeatCount = -1;
        private int currentCount;

        protected override void OnEnter()
        {
            currentCount = 0;
        }

        protected override NodeState OnUpdate()
        {
            if (child == null) return NodeState.Failure;
            if (repeatCount >= 0 && currentCount >= repeatCount)
                return NodeState.Success;

            var state = child.Tick();
            if (state == NodeState.Running) return NodeState.Running;

            currentCount++;
            if (repeatCount >= 0 && currentCount >= repeatCount)
                return state;
            return NodeState.Running;
        }
    }
}
