using System.Collections.Generic;
using UnityEngine;

namespace Trea
{
    [CreateAssetMenu(fileName = "TreaAction", menuName = "Trea/节点资产")]
    public class TreaAction : ScriptableObject
    {
        public ActionType actionType = ActionType.Sequence;
        public string actionName;
        public List<TreaAction> children = new();

        [Header("Repeater 参数")]
        public int repeatCount = -1;

        [Header("Timeout / Wait 参数")]
        public float duration = 1f;

        [Header("Cooldown 参数")]
        public float cooldown = 1f;

        [Header("Chance 参数")]
        [Range(0f, 1f)] public float probability = 0.5f;

        [Header("Log 参数")]
        public string message = "";

        [Header("Animation 参数")]
        public AnimationClip animationClip;

        [Header("MoveTo 参数")]
        public Vector3 targetPosition;
        public float moveSpeed = 3f;
        public float arriveDistance = 0.2f;

        [Header("Custom 自定义节点")]
        public string customTypeName;
    }

    public enum ActionType
    {
        Sequence,
        Selector,
        Parallel,
        RandomSelector,
        Inverter,
        Repeater,
        UntilSuccess,
        Timeout,
        Cooldown,
        Succeeder,
        Wait,
        Log,
        Chance,
        Animation,
        MoveTo,
        Custom
    }
}
