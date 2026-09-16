using System.Collections.Generic;

namespace Trea
{
    public class RandomSelector : CompositeNode
    {
        private readonly List<int> order = new();
        private int index;

        protected override void OnEnter()
        {
            order.Clear();
            for (int i = 0; i < children.Count; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            index = 0;
        }

        protected override NodeState OnUpdate()
        {
            for (; index < order.Count; index++)
            {
                var state = children[order[index]].Tick();
                if (state == NodeState.Success) return NodeState.Success;
                if (state == NodeState.Running) return NodeState.Running;
            }
            return NodeState.Failure;
        }
    }
}
