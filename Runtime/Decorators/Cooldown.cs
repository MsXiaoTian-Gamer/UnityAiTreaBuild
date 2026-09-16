using UnityEngine;

namespace Trea
{
    public class Cooldown : DecoratorNode
    {
        public float cooldown = 1f;
        private float lastFinishTime = float.NegativeInfinity;

        protected override NodeState OnUpdate()
        {
            if (child == null) return NodeState.Failure;
            if (Time.time - lastFinishTime < cooldown) return NodeState.Failure;
            var state = child.Tick();
            if (state != NodeState.Running) lastFinishTime = Time.time;
            return state;
        }
    }
}
