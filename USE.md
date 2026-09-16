---
AIGC:
    Label: "1"
    ContentProducer: 001191440300708461136T1XGW3
    ProduceID: 52ccc64068be0e7a58ed8e5cc027dde0_75a3485fb1d211f18721525400cd780f
    ReservedCode1: YM3vMoDVL+oTx7eO43gKnfmnhKwoTdCZDDKQPHvJ+6lYExXyWueNUE4owF6NpcbDQdfEovI3BpeiAyPam/kQsXazWVuczozg/Msi9Ft+u9d70Zpwqb2GGsnwC8/fya+lHuiA0IuR/qnoxN8rEoLbLYODbz1SntF284Hy18KOB7mlAUzhYPHFm2zibb0=
    ContentPropagator: 001191440300708461136T1XGW3
    PropagateID: 52ccc64068be0e7a58ed8e5cc027dde0_75a3485fb1d211f18721525400cd780f
    ReservedCode2: YM3vMoDVL+oTx7eO43gKnfmnhKwoTdCZDDKQPHvJ+6lYExXyWueNUE4owF6NpcbDQdfEovI3BpeiAyPam/kQsXazWVuczozg/Msi9Ft+u9d70Zpwqb2GGsnwC8/fya+lHuiA0IuR/qnoxN8rEoLbLYODbz1SntF284Hy18KOB7mlAUzhYPHFm2zibb0=
---



# AiTrea 使用说明

> AiTrea 是一个基于 Unity 的**行为树（Behavior Tree）AI 插件**，当前处于建设中（Alpha / 早期开发），**持续更新中**。
> 本文档基于项目当前代码（`Assets/Scripts/Trea`）编写，说明插件的定位、接入方式与使用方法。

---

## 1. 插件定位与模块结构

AiTrea 提供一套轻量的行为树运行时与配套编辑器工具，命名空间为 `Trea`。核心设计：节点是普通 C# 类（可序列化），通过 `BehaviourTreeRunner` 驱动树在 `Update` 中按固定 `tickInterval` 执行。

```
Assets/Scripts/Trea/
├── Core/          节点框架基类
│   ├── BTNode.cs          所有节点的抽象基类（生命周期 Tick / Abort / Bind / Clone）
│   ├── CompositeNode.cs   组合节点基类（children 列表）
│   ├── DecoratorNode.cs   装饰节点基类（单 child）
│   └── NodeState.cs       节点状态枚举 Running / Success / Failure
├── Composites/    组合节点
│   ├── Sequence.cs       顺序节点
│   ├── Selector.cs       选择节点
│   ├── Parallel.cs       并行节点（RequireOne / RequireAll 策略）
│   └── RandomSelector.cs 随机顺序选择节点
├── Decorators/    装饰节点
│   ├── Inverter.cs       结果取反
│   ├── Repeater.cs       重复执行（repeatCount = -1 无限）
│   ├── UntilSuccess.cs   直到成功
│   ├── Timeout.cs        超时中断（duration 秒后失败）
│   ├── Cooldown.cs       冷却（cooldown 秒内不再次执行）
│   └── Succeeder.cs      强制成功
├── Leaves/        叶节点
│   ├── ActionNode.cs     动作节点抽象基类（OnStart / OnAction / OnStop）
│   ├── ConditionNode.cs  条件节点抽象基类（Check）
│   └── GenericNodes.cs   内置通用节点
│       ├── WaitAction         等待 duration 秒后成功
│       ├── LogAction          打印 message 日志
│       ├── ChanceCondition    按 probability 概率成功
│       ├── AnimationAction    播放 AnimationClip 至结束
│       └── MoveToAction       向 target/targetPosition 移动至 arriveDistance 内
├── Blackboard/    黑板（跨节点共享数据）
│   └── Blackboard.cs    Set<T> / Get<T> / TryGet<T> / Has / Remove / Clear
├── Runtime/       运行时驱动器
│   └── BehaviourTreeRunner.cs   MonoBehaviour：持有 root、tickInterval、驱动 Tick、SetTree / ResetTree
├── Editor/        编辑器工具
│   ├── TreaMenu.cs              菜单入口
│   ├── BTNodeDrawer.cs          Inspector 递归节点绘制器（运行时显示节点状态颜色）
│   ├── BehaviourTreeRunnerEditor.cs  Runner 自定义 Inspector（黑板数据查看 / 重置按钮）
│   └── TreaActionEditors.cs     TreaAction 资产与 TreaActionReg 自定义 Inspector
└── Example/       示例
    ├── ExampleEnemyAI.cs       巡逻 / 追击 / 攻击的示例敌人 AI
    └── Health.cs               示例血量组件
```

> 注意：以下两个脚本**不在** `Assets/Scripts/Trea/` 目录内，而是位于 `Assets/Scripts/` 下（与 `Trea/` 平级），但同属 `Trea` 命名空间，是插件的组成部分。

| 脚本（真实路径） | 作用 |
|---|---|
| `Assets/Scripts/TreaAction.cs` | `ScriptableObject` 节点资产，右键 `Create > Trea/节点资产` 创建，可在 Inspector 中配置节点类型、参数与子节点 |
| `Assets/Scripts/TreaActionReg.cs` | `MonoBehaviour`，将 `TreaAction` 资产树转换为运行时 `BTNode` 树并注入 Runner（自动检测循环引用） |

---

## 2. 环境与接入

- **Unity 版本**：项目基于 **Unity 6000.4.10f1**（Unity 6.0 系列，见 `ProjectSettings/ProjectVersion.txt`）。
- **接入方式**：当前为 **Assets 内嵌**（直接放入 `Assets/` 即可，无 `package.json`，尚未打包为 UPM 包）。
- **依赖**：无第三方依赖，仅使用 Unity 内置模块（`com.unity.modules.*` 与 URP 模板包）。`com.unity.inputsystem` 为示例项目模板自带，插件本身不依赖。

### 接入步骤

1. 将 `Assets/Scripts/Trea/` 整个目录复制到目标项目的 `Assets/` 下；
2. **同时**复制 `Assets/Scripts/TreaAction.cs` 与 `Assets/Scripts/TreaActionReg.cs` 两个脚本（它们在 `Trea/` 目录之外、`Assets/Scripts/` 下，但属于插件的资产驱动部分；`Trea/Editor` 内的 `TreaMenu.cs`、`TreaActionEditors.cs` 依赖它们，缺失会编译报错）；
3. 等待 Unity 编译完成（`Trea` 命名空间即可用）；
4. 可选：将 `Assets/Scripts/Trea/Example/` 移入你自己的示例目录或删除。

> 注意：当前未提供 `.asmdef` 程序集定义，插件代码会进入默认的 `Assembly-CSharp` 程序集。

---

## 3. 使用方法

### 3.1 方式 A：纯代码构建行为树（推荐开发使用）

继承 `ActionNode` / `ConditionNode` 实现自定义逻辑，用组合 / 装饰节点组装树，交给 `BehaviourTreeRunner` 驱动。

```csharp
using Trea;

public class IsPlayerVisible : ConditionNode
{
    protected override bool Check()
    {
        // 返回 true / false，决定该条件节点成功或失败
        return true;
    }
}

public class ChaseAction : ActionNode
{
    protected override void OnStart() { }          // 节点首次进入时调用一次
    protected override NodeState OnAction()        // 每帧/tick 调用
    {
        // 移动逻辑...
        return NodeState.Running;                  // 未完成
        // return NodeState.Success;               // 完成
    }
    protected override void OnStop() { }           // 节点退出时调用一次
}
```

组装并挂载：

```csharp
using Trea;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private BehaviourTreeRunner runner;

    private void Awake()
    {
        runner = GetComponent<BehaviourTreeRunner>();

        var root = new Selector { nodeName = "根节点" };

        var chase = new Sequence { nodeName = "追击" };
        chase.AddChild(new IsPlayerVisible());
        chase.AddChild(new ChaseAction());

        root.AddChild(chase);
        root.AddChild(new PatrolAction { nodeName = "巡逻" }); // 自定义

        runner.SetTree(root);   // 绑定黑板并开始驱动
    }
}
```

### 3.2 方式 B：节点资产可视化配置（Inspector 中搭树）

1. 右键 `Create > Trea/节点资产` 创建 `TreaAction` 资产，命名为根节点（如 `Root_Selector`）；
2. 在资产 Inspector 中设置 **节点类型**（`Sequence` / `Selector` / `Parallel` / `RandomSelector` / `Inverter` / `Repeater` / `UntilSuccess` / `Timeout` / `Cooldown` / `Succeeder` / `Wait` / `Log` / `Chance` / `Animation` / `MoveTo` / `Custom`）、**节点名称**与对应参数（组合节点挂 `children` 子节点）；
3. 在场景对象上挂 `BehaviourTreeRunner` + `TreaActionReg`；
4. 将根节点资产拖到 `TreaActionReg` 的 **根节点资产**，`Runner` 留空会自动 `GetComponent` 获取；
5. 勾选 **启动时自动构建（树为空时）**（默认开启），或点击 Inspector 中的 **「构建行为树」** 按钮 / 右键组件菜单 **「构建行为树」** 手动构建。

> 资产方式构建时会自动检测并跳过循环引用（`Debug.LogWarning` 提示）。装饰节点在资产方式下只取第一个子节点。

### 3.3 菜单入口（Editor）

| 菜单 | 作用 |
|---|---|
| `Trea/创建示例敌人` | 一键创建 `ExampleEnemy` 对象（含 Runner + ExampleEnemyAI），并自动构建行为树，控制台提示在 ExampleEnemyAI 上指定 Player |
| `Trea/示例/创建示例节点资产` | 在 `Assets/TreaExample` 下生成一套示例节点资产（根选择节点 + 追击序列 + 巡逻序列等），可将根资产挂到 TreaActionReg 使用 |

### 3.4 Runner Inspector（运行时）

- 可在 Inspector 中直接编辑 `根节点`（`BTNodeDrawer` 递归绘制：节点行点击类型名可**更换节点类型**，`×` 删除节点，组合节点 `+` 添加子节点、`▲▼` 排序）；
- 运行时节点显示**状态色块**：绿 = Success、红 = Failure、黄 = Running；
- 运行时面板提供 **「重置行为树」** 按钮与**黑板数据**实时查看。

### 3.5 黑板（Blackboard）跨节点共享数据

```csharp
// 写入（在任意节点或外部）
blackboard.Set("Target", playerTransform);

// 读取
if (blackboard.TryGet<Transform>("Target", out var target)) { ... }
var hp = blackboard.Get<int>("hp", 100);   // 带默认值
```

`BehaviourTreeRunner` 在 `Awake` 中会自动将 `blackboard` 绑定到根节点（`root.Bind(blackboard)` 沿树向下传递）。

---

## 4. 节点类型总表

### 组合节点（Composite）

| 类型 | 行为 | 参数 |
|---|---|---|
| `Sequence` | 按序执行，任一失败则失败 | — |
| `Selector` | 按序尝试，任一成功则成功 | — |
| `Parallel` | 并行执行全部子节点 | `successPolicy`：`RequireAll`（全部成功才成功，任一失败即失败）/ `RequireOne`（至少一个成功） |
| `RandomSelector` | 随机打乱子节点顺序后按选择逻辑执行 | — |

### 装饰节点（Decorator，只能有一个子节点）

| 类型 | 行为 | 参数 |
|---|---|---|
| `Inverter` | 子结果取反 | — |
| `Repeater` | 重复执行子节点 | `repeatCount`（-1 = 无限） |
| `UntilSuccess` | 重复直到子节点成功 | — |
| `Timeout` | 子节点运行超过 `duration` 秒则中断并失败 | `duration` |
| `Cooldown` | 子节点结束后进入冷却，冷却期间直接失败 | `cooldown` |
| `Succeeder` | 子节点结果强制为成功 | — |

### 叶节点（Leaf）

| 类型 | 行为 | 参数 |
|---|---|---|
| `Wait` | 等待后成功 | `duration` |
| `Log` | 打印日志后成功 | `message` |
| `Chance` | 按概率成功 | `probability`（0~1） |
| `Animation` | 播放动画至结束 | `animationClip`（通过资产方式挂在 TreaActionReg 所在对象上查找 Animator） |
| `MoveTo` | 移动到目标后成功 | `targetPosition` / `moveSpeed` / `arriveDistance` |
| `Custom` | 反射创建自定义 BTNode 类型 | `customTypeName`（资产 Inspector 下拉选择，或手填程序集限定名） |

---

## 5. 自定义节点

- **代码继承**：继承 `ActionNode`（实现 `OnAction`，可选 `OnStart` / `OnStop`）或 `ConditionNode`（实现 `Check`）即可，组合/装饰基类同样可扩展。
- **资产方式**：在 `TreaAction` 资产上选择节点类型 `Custom`，Inspector 会列出项目中所有非抽象的 `BTNode` 派生类型供选择（`customTypeName` 存储为程序集限定名）。自定义节点的参数在构建后的 Runner Inspector 中配置（资产里没有参数面板）。
- 参考实现：`Assets/Scripts/Trea/Example/ExampleEnemyAI.cs` 中的 `IsPlayerVisibleCondition`、`ChaseAction`、`AttackAction` 等。

---

## 6. 示例演示（ExampleEnemyAI）

`Trea/创建示例敌人` 生成的敌人 AI 演示了完整用法：

- **根节点**：`Selector`（优先攻击 > 追击 > 巡逻）
- **攻击分支**：`Sequence`（在攻击范围内 → 攻击 0.5 秒造成伤害）
- **追击分支**：`Sequence`（可见玩家 → 记录 Target 到黑板 → `Timeout(5s)` 内追击靠近）
- **巡逻分支**：`PatrolAction` 在 `patrolPoints` 间往返移动
- 玩家对象需要挂 `Health` 组件以扣血，或用 Tag `Player` 自动查找

---

## 7. 当前建设进度与已知限制

**已实现**
- 完整行为树运行时：节点生命周期（Enter / Update / Exit）、`Abort` 中断、`Clone` 克隆、`Bind` 黑板绑定；
- 4 种组合节点、6 种装饰节点、5 种内置通用叶节点；
- 黑板共享数据；`BehaviourTreeRunner` 驱动与动态换树（`SetTree` / `ResetTree`）；
- 节点资产（`TreaAction`）+ 资产转运行时树（`TreaActionReg`，含循环引用检测）；
- 编辑器支持：菜单入口、Runner / 资产 / 注册组件自定义 Inspector、递归节点绘制（换类型 / 增删 / 排序 / 运行时状态色）；
- 示例敌人 AI 与血量组件。

**未完成 / 占位**
- 项目 `README.md` 明确标注**建设中（Alpha）**，尚未发布正式版本，接口可能调整；
- 无 `package.json`，**未打包为 UPM 插件包**（当前为 Assets 内嵌，安装需手动复制目录）；
- 无 `.asmdef` 程序集定义，未做程序集隔离；
- 无独立可视化节点图编辑器（当前为 Inspector 递归列表绘制，非 GraphView 拖拽画布）；
- 资产方式下装饰节点仅支持单子节点、`Animation` 节点依赖所在对象的 Animator（`GetComponentInChildren`）；
- 自定义节点（`Custom`）在资产 Inspector 中无参数面板，需构建后在 Runner 上配置；
- 无内置单元测试用例（`com.unity.test-framework` 为模板自带）。

---

## 8. 常见问题

| 现象 | 说明 |
|---|---|
| Runner 未运行 | 确认 `root` 不为空（代码方式需调用 `SetTree`；资产方式需构建成功），`tickInterval` 大于 0 |
| 资产方式构建无反应 | 检查 `TreaActionReg` 是否绑定 Runner、根节点资产是否指定，控制台有 `Debug.LogWarning` 提示 |
| 装饰节点子节点丢失 | 资产方式装饰节点只读取 `children[0]` |
| 循环引用 | 资产方式构建会自动检测并跳过循环，控制台会警告 |
*（内容由AI生成，仅供参考）*
*（内容由AI生成，仅供参考）*
