using UnityEngine;

namespace Trea.Example
{
    public class IsPlayerVisibleCondition : ConditionNode
    {
        public ExampleEnemyAI ai;

        protected override bool Check()
        {
            if (ai == null || ai.Player == null) return false;
            return Vector3.Distance(ai.transform.position, ai.Player.position) <= ai.sightRange;
        }
    }

    public class InAttackRangeCondition : ConditionNode
    {
        public ExampleEnemyAI ai;

        protected override bool Check()
        {
            if (ai == null || ai.Player == null) return false;
            return Vector3.Distance(ai.transform.position, ai.Player.position) <= ai.attackRange;
        }
    }

    public class SetTargetAction : ActionNode
    {
        public ExampleEnemyAI ai;

        protected override NodeState OnAction()
        {
            if (ai == null || ai.Player == null) return NodeState.Failure;
            blackboard.Set("Target", ai.Player);
            return NodeState.Success;
        }
    }

    public class ChaseAction : ActionNode
    {
        public ExampleEnemyAI ai;

        protected override NodeState OnAction()
        {
            if (ai == null) return NodeState.Failure;
            if (!blackboard.TryGet<Transform>("Target", out var target)) return NodeState.Failure;
            ai.MoveTowards(target.position, ai.chaseSpeed);
            if (Vector3.Distance(ai.transform.position, target.position) <= ai.attackRange)
                return NodeState.Success;
            return NodeState.Running;
        }
    }

    public class AttackAction : ActionNode
    {
        public ExampleEnemyAI ai;
        private float timer;

        protected override void OnStart()
        {
            timer = 0f;
        }

        protected override NodeState OnAction()
        {
            if (ai == null) return NodeState.Failure;
            timer += Time.deltaTime;
            if (timer >= ai.attackDuration)
            {
                ai.DealDamage();
                return NodeState.Success;
            }
            return NodeState.Running;
        }
    }

    public class PatrolAction : ActionNode
    {
        public ExampleEnemyAI ai;

        protected override NodeState OnAction()
        {
            if (ai == null) return NodeState.Failure;
            ai.Patrol();
            return NodeState.Running;
        }
    }

    public class ExampleEnemyAI : MonoBehaviour
    {
        public BehaviourTreeRunner runner;
        public Transform player;
        public float sightRange = 10f;
        public float attackRange = 2f;
        public float patrolSpeed = 2f;
        public float chaseSpeed = 5f;
        public float attackDuration = 0.5f;
        public float attackDamage = 10f;
        public Vector3[] patrolPoints = { new Vector3(5, 0, 0), new Vector3(-5, 0, 0) };

        private int patrolIndex;

        public Transform Player => player;

        private void Start()
        {
            if (runner == null) runner = GetComponent<BehaviourTreeRunner>();
            if (runner != null && runner.root == null)
                BuildTree();
        }

        [ContextMenu("构建行为树")]
        public void BuildTree()
        {
            if (runner == null) runner = GetComponent<BehaviourTreeRunner>();
            if (player == null)
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }

            var attack = new Sequence { nodeName = "攻击" };
            attack.AddChild(new InAttackRangeCondition { ai = this });
            attack.AddChild(new AttackAction { ai = this });

            var chase = new Sequence { nodeName = "追击" };
            chase.AddChild(new IsPlayerVisibleCondition { ai = this });
            chase.AddChild(new SetTargetAction { ai = this });
            var timeout = new Timeout { nodeName = "追击超时", duration = 5f };
            timeout.SetChild(new ChaseAction { ai = this });
            chase.AddChild(timeout);

            var root = new Selector { nodeName = "根节点" };
            root.AddChild(attack);
            root.AddChild(chase);
            root.AddChild(new PatrolAction { ai = this, nodeName = "巡逻" });

            if (runner != null)
                runner.SetTree(root);
        }

        public void MoveTowards(Vector3 position, float speed)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, speed * Time.deltaTime);
        }

        public void Patrol()
        {
            if (patrolPoints.Length == 0) return;
            var target = patrolPoints[patrolIndex];
            MoveTowards(target, patrolSpeed);
            if (Vector3.Distance(transform.position, target) < 0.2f)
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }

        public void DealDamage()
        {
            if (player == null) return;
            var health = player.GetComponent<Health>();
            if (health != null) health.TakeDamage(attackDamage);
            else Debug.Log($"{name} 攻击了 {player.name}，造成 {attackDamage} 点伤害");
        }
    }
}
