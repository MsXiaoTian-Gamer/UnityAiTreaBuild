using UnityEngine;

namespace Trea
{
    public class Timeout : DecoratorNode
    {
        public float duration = 1f;
        private float startTime;

        protected override void OnEnter()
        {
            startTime = Time.time;
        }

        protected override NodeState OnUpdate()
        {
            if (child == null) return NodeState.Failure;
            var state = child.Tick();
            if (state == NodeState.Running && Time.time - startTime >= duration)
            {
                child.Abort();
                return NodeState.Failure;
            }
            return state;
        }
    }
}
