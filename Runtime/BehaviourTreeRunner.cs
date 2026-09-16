using UnityEngine;

namespace Trea
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        [SerializeReference] public BTNode root;
        public float tickInterval = 0.1f;

        [HideInInspector] public Blackboard blackboard = new();

        public BTNode Root => root;

        private float nextTickTime;

        private void Awake()
        {
            if (root != null)
                root.Bind(blackboard);
        }

        public void SetTree(BTNode tree)
        {
            root = tree;
            if (root != null)
                root.Bind(blackboard);
            nextTickTime = 0f;
        }

        private void Update()
        {
            if (root == null) return;
            if (Time.time >= nextTickTime)
            {
                nextTickTime = Time.time + tickInterval;
                root.Tick();
            }
        }

        public void ResetTree()
        {
            if (root != null)
                root.Abort();
        }
    }
}
