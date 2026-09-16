using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trea
{
    public class TreaActionReg : MonoBehaviour
    {
        public TreaAction rootAction;
        public BehaviourTreeRunner runner;
        public bool buildOnStart = true;

        private void Start()
        {
            if (runner == null) runner = GetComponent<BehaviourTreeRunner>();
            if (runner == null) return;
            if (buildOnStart && runner.root == null)
                BuildAction();
        }

        [ContextMenu("构建行为树")]
        public void BuildAction()
        {
            if (runner == null) runner = GetComponent<BehaviourTreeRunner>();
            if (runner == null)
            {
                Debug.LogWarning("TreaActionReg：未找到 BehaviourTreeRunner", this);
                return;
            }
            if (rootAction == null)
            {
                Debug.LogWarning("TreaActionReg：未指定根节点资产", this);
                return;
            }
            runner.SetTree(Convert(rootAction, new HashSet<TreaAction>()));
        }

        private BTNode Convert(TreaAction action, HashSet<TreaAction> path)
        {
            if (action == null) return null;
            if (!path.Add(action))
            {
                Debug.LogWarning($"TreaActionReg：检测到循环引用，已跳过 {action.name}", this);
                return null;
            }

            BTNode node = CreateNode(action);
            if (node != null)
            {
                if (node is CompositeNode composite)
                {
                    foreach (var child in action.children)
                    {
                        var converted = Convert(child, path);
                        if (converted != null)
                            composite.AddChild(converted);
                    }
                }
                else if (node is DecoratorNode decorator && action.children.Count > 0)
                {
                    var converted = Convert(action.children[0], path);
                    if (converted != null)
                        decorator.SetChild(converted);
                }
            }

            path.Remove(action);
            return node;
        }

        private BTNode CreateNode(TreaAction action)
        {
            BTNode node = action.actionType switch
            {
                ActionType.Sequence => new Sequence(),
                ActionType.Selector => new Selector(),
                ActionType.Parallel => new Parallel(),
                ActionType.RandomSelector => new RandomSelector(),
                ActionType.Inverter => new Inverter(),
                ActionType.Repeater => new Repeater { repeatCount = action.repeatCount },
                ActionType.UntilSuccess => new UntilSuccess(),
                ActionType.Timeout => new Timeout { duration = action.duration },
                ActionType.Cooldown => new Cooldown { cooldown = action.cooldown },
                ActionType.Succeeder => new Succeeder(),
                ActionType.Wait => new WaitAction { duration = action.duration },
                ActionType.Log => new LogAction { message = string.IsNullOrEmpty(action.message) ? action.name : action.message },
                ActionType.Chance => new ChanceCondition { probability = action.probability },
                ActionType.Animation => new AnimationAction { clip = action.animationClip, owner = this },
                ActionType.MoveTo => new MoveToAction { self = transform, targetPosition = action.targetPosition, speed = action.moveSpeed, arriveDistance = action.arriveDistance },
                ActionType.Custom => CreateCustomNode(action),
                _ => null
            };

            if (node == null) return null;
            node.nodeName = string.IsNullOrEmpty(action.actionName) ? action.name : action.actionName;
            return node;
        }

        private BTNode CreateCustomNode(TreaAction action)
        {
            if (string.IsNullOrEmpty(action.customTypeName))
            {
                Debug.LogWarning($"TreaActionReg：{action.name} 未指定自定义节点类型", this);
                return null;
            }
            try
            {
                var type = Type.GetType(action.customTypeName);
                if (type == null)
                {
                    Debug.LogWarning($"TreaActionReg：找不到自定义节点类型 {action.customTypeName}", this);
                    return null;
                }
                return Activator.CreateInstance(type) as BTNode;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"TreaActionReg：创建自定义节点失败 {action.customTypeName}：{e.Message}", this);
                return null;
            }
        }
    }
}
