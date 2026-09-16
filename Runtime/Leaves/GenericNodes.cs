using UnityEngine;

namespace Trea
{
    public class WaitAction : ActionNode
    {
        public float duration = 1f;
        private float timer;

        protected override void OnStart()
        {
            timer = 0f;
        }

        protected override NodeState OnAction()
        {
            timer += Time.deltaTime;
            return timer >= duration ? NodeState.Success : NodeState.Running;
        }
    }

    public class LogAction : ActionNode
    {
        public string message = "Hello";

        protected override NodeState OnAction()
        {
            Debug.Log(message);
            return NodeState.Success;
        }
    }

    public class ChanceCondition : ConditionNode
    {
        [Range(0f, 1f)] public float probability = 0.5f;

        protected override bool Check()
        {
            return Random.value < probability;
        }
    }

    public class AnimationAction : ActionNode
    {
        public AnimationClip clip;
        public Component owner;
        private float timer;

        protected override void OnStart()
        {
            timer = 0f;
            var animator = GetAnimator();
            if (animator != null && clip != null)
                animator.Play(clip.name);
        }

        protected override NodeState OnAction()
        {
            if (clip == null) return NodeState.Failure;
            var animator = GetAnimator();
            if (animator == null) return NodeState.Failure;
            timer += Time.deltaTime;
            return timer >= clip.length ? NodeState.Success : NodeState.Running;
        }

        private Animator GetAnimator()
        {
            if (owner == null) return null;
            return owner.GetComponentInChildren<Animator>();
        }
    }

    public class MoveToAction : ActionNode
    {
        public Transform self;
        public Transform target;
        public Vector3 targetPosition;
        public float speed = 3f;
        public float arriveDistance = 0.2f;

        protected override NodeState OnAction()
        {
            if (self == null) return NodeState.Failure;
            var destination = target != null ? target.position : targetPosition;
            self.position = Vector3.MoveTowards(self.position, destination, speed * Time.deltaTime);
            if (Vector3.Distance(self.position, destination) <= arriveDistance)
                return NodeState.Success;
            return NodeState.Running;
        }
    }
}
