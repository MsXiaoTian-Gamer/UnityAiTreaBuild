# Trea Behaviour Tree

> 新手友好的 Unity 行为树插件：全程在 **Inspector** 里配置，无需写可视化编辑器。
> 项目正在建设中并**持续更新中**，接口可能随版本调整。

- 内置常用节点：组合（Sequence/Selector/Parallel/RandomSelector）、装饰（Inverter/Repeater/Timeout/Cooldown/UntilSuccess/Succeeder）、通用叶子（Wait/Log/Chance/Animation/MoveTo）
- 两种搭建方式：直接在 `BehaviourTreeRunner` 上配置，或用 `TreaAction` 资产 + `TreaActionReg` 注册搭建
- 自定义节点只需继承 `ActionNode` / `ConditionNode` 写一个类，自动出现在类型菜单里
- 运行模式 Inspector 实时显示节点状态（Running 黄 / Success 绿 / Failure 红）和黑板数据

## 使用说明

- [USE.md](USE.md) — 插件使用说明（功能定位、安装接入、节点类型与使用方法）

## 安装

**方式一：Git URL（推荐）**

1. 打开 `Window → Package Manager`
2. 点左上角 `+` → `Add package from git URL...`
3. 粘贴仓库地址，例如 `https://github.com/MsXiaoTian-Gamer/UnityAiTreaBuild.git`

**方式二：本地包**

把仓库内容复制到项目的 `Packages/com.trea.behaviortree/` 目录（Unity 会自动识别）。

**方式三：手动复制脚本**

把 `Runtime/` 和 `Editor/` 下的 `.cs` 文件复制到项目的 `Assets/` 下任意位置。

## 快速开始

### 方式 A：Runner 直接配置

1. 任意 GameObject 挂上 `BehaviourTreeRunner`
2. Inspector 里点「选择节点类型...」选根节点（如 `Selector`）
3. 展开节点，在「子节点」列表点 `+` 添加子节点、`▲▼` 排序、`×` 删除
4. 运行即可，节点头会实时变色显示状态

### 方式 B：TreaAction 资产

1. `Assets → Create → Trea/节点资产` 创建 `TreaAction`
2. 每个资产选一个节点类型（Sequence、Selector、Wait、MoveTo...），把子资产拖进「子节点」列表
3. GameObject 上挂 `TreaActionReg` + `BehaviourTreeRunner`，指定「根节点资产」，点「构建行为树」
4. 构建出的树会出现在 `BehaviourTreeRunner` 的 Inspector 里，可继续调参

菜单 `Trea/示例/创建示例节点资产` 可一键生成一套示例资产链。

## 自定义行为节点

写一个类继承 `ActionNode`（动作）或 `ConditionNode`（条件），比如 `Move.cs`：

```csharp
using UnityEngine;
using Trea;

public class Move : ActionNode
{
    public Transform self;            // 谁移动
    public Transform target;          // 跟随目标（可选）
    public Vector3 targetPosition;    // 没有 target 时用这个坐标
    public float speed = 3f;

    protected override NodeState OnAction()
    {
        if (self == null) return NodeState.Failure;
        var destination = target != null ? target.position : targetPosition;
        self.position = Vector3.MoveTowards(self.position, destination, speed * Time.deltaTime);
        return Vector3.Distance(self.position, destination) < 0.1f
            ? NodeState.Success
            : NodeState.Running;
    }
}
```

- `Running` = 这一帧没做完，下帧继续（移动类都这样写）；`Success`/`Failure` = 立即结束
- `OnStart()` 做初始化，`OnStop()` 做收尾
- 写完类后自动出现在 Runner 树和 TreaAction（类型选 `Custom`）的类型菜单里
- 自定义节点参数在构建后的树上配置（Runner 的 Inspector），会随场景保存
- 节点间共享数据用黑板：`blackboard.Set("Target", t)` / `blackboard.Get<Transform>("Target")`

## 内置节点

| 类型 | 说明 |
| --- | --- |
| Sequence | 顺序执行子节点，任一失败即失败 |
| Selector | 依次尝试子节点，任一成功即成功 |
| Parallel | 同时执行所有子节点，可配 RequireOne/RequireAll |
| RandomSelector | 随机顺序的 Selector |
| Inverter | 结果取反 |
| Repeater | 重复 N 次（-1 无限） |
| UntilSuccess | 直到成功为止 |
| Timeout | 超时失败 |
| Cooldown | 冷却期间不执行 |
| Succeeder | 永远成功 |
| Wait | 等待 N 秒 |
| Log | 打印日志 |
| Chance | 按概率判定 |
| Animation | 播放动画片段，播完成功 |
| MoveTo | 移动到目标点 |

## 示例

Package Manager 里选中本包 → Samples → Import，导入「示例敌人 AI」：

- `ExampleEnemyAI.cs`：巡逻 → 发现玩家 → 追击 → 攻击完整示例（含自定义节点写法）
- 菜单 `Trea/创建示例敌人` 一键生成，需给玩家物体加 `Health` 组件并设置 Tag 为 `Player`

## 环境要求

- Unity 6000.0 及以上

## License

MIT
