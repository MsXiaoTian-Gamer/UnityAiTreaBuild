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



# MengShenBehaviorTree 使用说明（USE.md）

> MengShenBehaviorTree 是一个基于 Unity 的**行为树（Behavior Tree）AI 插件**，当前处于建设中（Alpha / 早期开发），**持续更新中**。
> 本文档基于项目当前代码（仓库根目录即 UPM 包：`Runtime/`、`Editor/`、`Samples~/`）编写，说明插件的定位、安装接入方式、节点体系与使用方法。

---

## 1. 插件简介与功能定位

### 1.1 这是什么

MengShenBehaviorTree（包名 `com.trea.behaviortree`，当前版本 `0.1.0`）是一套**新手友好、轻量、无外部依赖**的 Unity 行为树 AI 插件，命名空间统一为 `Trea`。它提供：

- 一套完整的行为树**运行时**：节点生命周期管理、组合 / 装饰 / 叶节点、黑板共享数据、固定间隔 Tick 驱动；
- 两套**搭建方式**：
  - **纯代码**：继承 `ActionNode` / `ConditionNode` 写逻辑类，用组合 / 装饰节点组装树；
  - **资产可视化**：用 `TreaAction`（ScriptableObject 节点资产）在 Inspector 里搭树，再交给 `TreaActionReg` 自动转换成运行时树；
- 一套**编辑器工具**：`BehaviourTreeRunner` 递归节点绘制（含运行时状态颜色）、`TreaAction` 资产参数面板、一键创建示例敌人 / 示例节点资产的菜单。

### 1.2 设计特点

- **节点即普通 C# 类**：所有节点继承自 `BTNode`，使用 `[SerializeReference]` 序列化，可直接在 Inspector 中展开编辑；
- **无需可视化节点图编辑器**：树结构通过 Inspector 的递归列表展示，避免了 GraphView 拖拽画布的复杂度；
- **Tick 驱动简单**：`BehaviourTreeRunner` 在 `Update` 中按 `tickInterval`（默认 0.1 秒）调用根节点 `Tick()`；
- **自动绑定黑板**：`SetTree` / `Awake` 时 `root.Bind(blackboard)` 会把共享黑板沿树向下传递给所有节点；
- **支持动态换树与重置**：`SetTree(newRoot)` 可整体替换树，`ResetTree()` 中断当前运行并复位状态。

### 1.3 包信息

| 项 | 值 |
|---|---|
| 包名（package.json `name`） | `com.trea.behaviortree` |
| 版本（package.json `version`） | `0.1.0` |
| 显示名（`displayName`） | MengShenBehaviorTree |
| 最低 Unity 版本（`unity` 字段） | `6000.0`（Unity 6.0 及以上） |
| License | MIT（见 `LICENSE.md`） |
| 作者 | Trea（仓库：`https://github.com/MsXiaoTian-Gamer/UnityAiTreaBuild`） |

---

## 2. 安装与接入

### 2.1 目录结构（UPM 包）

本插件**仓库根目录即 UPM 包**，标准结构如下：

```
UnityAiTreaBuild/
├── package.json                  UPM 包清单（name/version/unity/samples）
├── README.md                     仓库说明（安装方式 / 快速开始 / 内置节点）
├── LICENSE.md                    MIT 许可证
├── USE.md                        本文档
├── Runtime/                      运行时源码（编译为 Trea.Runtime 程序集）
│   ├── Trea.Runtime.asmdef
│   ├── BehaviourTreeRunner.cs    树驱动器（MonoBehaviour）
│   ├── TreaAction.cs             ScriptableObject 节点资产
│   ├── TreaActionReg.cs          资产树 → 运行时树转换器
│   ├── Core/                     节点框架基类
│   │   ├── BTNode.cs             所有节点的抽象基类
│   │   ├── CompositeNode.cs      组合节点基类（children 列表）
│   │   ├── DecoratorNode.cs      装饰节点基类（单 child）
│   │   └── NodeState.cs          节点状态枚举
│   ├── Composites/               组合节点
│   │   ├── Sequence.cs
│   │   ├── Selector.cs
│   │   ├── Parallel.cs
│   │   └── RandomSelector.cs
│   ├── Decorators/               装饰节点
│   │   ├── Inverter.cs / Repeater.cs / UntilSuccess.cs
│   │   ├── Timeout.cs / Cooldown.cs / Succeeder.cs
│   ├── Leaves/                   叶节点
│   │   ├── ActionNode.cs         动作节点抽象基类
│   │   ├── ConditionNode.cs      条件节点抽象基类
│   │   └── GenericNodes.cs       内置通用叶节点
│   └── Blackboard/
│       └── Blackboard.cs         跨节点共享数据黑板
├── Editor/                       编辑器源码（编译为 Trea.Editor 程序集）
│   ├── Trea.Editor.asmdef
│   ├── TreaMenu.cs               菜单：Trea/示例/创建示例节点资产
│   ├── BTNodeDrawer.cs           Inspector 递归节点绘制器
│   ├── BehaviourTreeRunnerEditor.cs  Runner 自定义 Inspector
│   └── TreaActionEditors.cs      TreaAction / TreaActionReg 自定义 Inspector
└── Samples~/                     示例（UPM 规范：以 ~ 结尾不会被打入包）
    └── TreaExample/
        ├── TreaExample.asmdef / Editor/TreaExample.Editor.asmdef
        ├── ExampleEnemyAI.cs     巡逻 / 追击 / 攻击完整示例
        ├── Health.cs             示例血量组件
        └── Editor/SampleMenu.cs  菜单：Trea/创建示例敌人
```

### 2.2 环境要求

- **Unity 6000.0（Unity 6.0 系列）及以上**（package.json `unity` 字段）。
- **无第三方运行时依赖**：仅使用 Unity 内置模块。
- 若在 Unity 6.0 之前（如 2021/2022 LTS）使用，请自行验证 `[SerializeReference]` 与 `managedReferenceValue` 相关编辑器 API 的兼容性——目前未做低版本适配。

### 2.3 安装方式（任选其一）

**方式一：Git URL（推荐）**

1. 打开 `Window → Package Manager`；
2. 点左上角 `+` → `Add package from git URL...`；
3. 粘贴仓库地址：
   ```
   https://github.com/MsXiaoTian-Gamer/UnityAiTreaBuild.git
   ```
4. 等待解析与编译完成。若机器配置了 SSH 且希望走 SSH，也可使用 `git@github.com:MsXiaoTian-Gamer/UnityAiTreaBuild.git`。

**方式二：本地包**

把整个仓库内容复制到目标项目的 `Packages/com.trea.behaviortree/` 目录，Unity 会自动识别为本地包（或通过 Package Manager `Add package from disk...` 选择 `package.json`）。

**方式三：手动复制脚本（非 UPM）**

把 `Runtime/` 与 `Editor/` 下的 `.cs` 文件复制到目标项目 `Assets/` 下任意位置即可（`TreaAction.cs` / `TreaActionReg.cs` 在 `Runtime/` 内，会自动一并复制）。此方式不会获得 UPM 包管理能力，示例需自行从 `Samples~/` 复制。

### 2.4 程序集（asmdef）说明

| 程序集 | 位置 | 引用 | 平台 |
|---|---|---|---|
| `Trea.Runtime` | `Runtime/` | 无（仅 UnityEngine 内置） | 全平台，`autoReferenced` |
| `Trea.Editor` | `Editor/` | `Trea.Runtime` | 仅 Editor |
| `TreaExample` | `Samples~/TreaExample/` | `Trea.Runtime` | 全平台 |
| `TreaExample.Editor` | `Samples~/TreaExample/Editor/` | `Trea.Runtime` + `TreaExample` | 仅 Editor |

> 自己的代码若要使用插件，需在 asmdef 中引用 `Trea.Runtime`；不使用 asmdef 的脚本（默认 `Assembly-CSharp`）因 `autoReferenced` 可直接访问 `Trea` 命名空间。

### 2.5 导入示例

`Samples~` 目录遵循 UPM 规范，**不会**被自动编译或打入包。在 Package Manager 中选中 `MengShenBehaviorTree` → `Samples` → `Import` 导入「示例敌人 AI」后，即可在项目中出现 `TreaExample` 示例（含 `Trea/创建示例敌人` 菜单）。

---

## 3. 快速开始

### 3.1 方式 A：纯代码构建（推荐开发使用）

**第 1 步：写自定义节点**

继承 `ActionNode` 实现动作（每 tick 调用 `OnAction`，返回 `Running` 表示未完成、`Success` / `Failure` 表示结束）：

```csharp
using Trea;
using UnityEngine;

public class ChaseAction : ActionNode
{
    public Transform self;
    public Transform target;
    public float speed = 5f;

    protected override void OnStart()
    {
        // 节点首次进入时调用一次（可做初始化）
    }

    protected override NodeState OnAction()
    {
        if (self == null || target == null) return NodeState.Failure;
        self.position = Vector3.MoveTowards(self.position, target.position, speed * Time.deltaTime);
        return Vector3.Distance(self.position, target.position) <= 0.2f
            ? NodeState.Success   // 到达目标，本次结束
            : NodeState.Running;  // 未完成，下个 tick 继续
    }

    protected override void OnStop()
    {
        // 节点退出（Success/Failure/Abort）时调用一次（可做收尾）
    }
}
```

继承 `ConditionNode` 实现条件（只需实现 `Check`，返回 `true` = Success，`false` = Failure）：

```csharp
using Trea;

public class IsPlayerVisible : ConditionNode
{
    protected override bool Check()
    {
        // 例如：视野半径内是否有玩家
        return true;
    }
}
```

**第 2 步：组装树并交给 Runner**

在任意 `MonoBehaviour` 上获取（或挂载）`BehaviourTreeRunner`，用组合 / 装饰节点组装树，调用 `SetTree`：

```csharp
using Trea;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private BehaviourTreeRunner runner;

    private void Awake()
    {
        runner = GetComponent<BehaviourTreeRunner>();
        if (runner == null) runner = gameObject.AddComponent<BehaviourTreeRunner>();

        // 根：选择器（按优先级尝试：追击 > 巡逻）
        var root = new Selector { nodeName = "根节点" };

        // 追击分支：顺序（条件通过 → 执行追击）
        var chase = new Sequence { nodeName = "追击" };
        chase.AddChild(new IsPlayerVisible());
        chase.AddChild(new ChaseAction { self = transform, speed = 5f });
        root.AddChild(chase);

        // 巡逻分支（自定义 PatrolAction）
        root.AddChild(new PatrolAction { self = transform, nodeName = "巡逻" });

        // 交给 Runner（内部会 Bind 黑板并开始驱动）
        runner.SetTree(root);
    }
}
```

**第 3 步：运行**

- Runner 在 `Update` 中按 `tickInterval`（默认 0.1s）自动调用 `root.Tick()`；
- 运行时可在 Inspector 中观察节点状态颜色（见 §7.3）；
- 需要整体复位时调用 `runner.ResetTree()`（中断当前运行中的节点并复位状态）。

### 3.2 方式 B：TreaAction 资产可视化配置（Inspector 搭树）

**第 1 步：创建节点资产**

- `Assets → Create → Trea/节点资产`（或右键 `Create > Trea/节点资产`）创建 `TreaAction` 资产；
- 每个资产对应一个节点：在 Inspector 中设置 **节点类型**（`ActionType` 枚举，见 §4.6）、**节点名称**与对应参数；
- 组合类型（Sequence / Selector / Parallel / RandomSelector）在 **子节点** 列表中添加子资产，形成树状层级。

**第 2 步：挂载并构建**

1. 场景对象挂 `BehaviourTreeRunner` + `TreaActionReg`；
2. 将根节点资产拖到 `TreaActionReg` 的 **根节点资产** 字段（`Runner` 留空会自动 `GetComponent<BehaviourTreeRunner>()`）；
3. 勾选 **启动时自动构建（树为空时）**（默认开启）：`Start` 时若 `runner.root == null` 自动调用 `BuildAction()`；
4. 也可手动构建：点击 Inspector 中的 **「构建行为树」** 按钮，或组件右键菜单 **「构建行为树」**（`[ContextMenu]`）。

**第 3 步：转换逻辑（TreaActionReg.BuildAction）**

`Convert` 会递归将资产树转换为运行时 `BTNode` 树并调用 `runner.SetTree(...)`：

- 组合节点：遍历 `children` 逐个 `Convert` 后 `AddChild`；
- 装饰节点：**只取 `children[0]`** 作为唯一子节点（多余子节点被忽略）；
- 叶节点：无子节点；
- **循环引用保护**：`Convert` 用 `HashSet<TreaAction>` 记录当前递归路径，检测到环时 `Debug.LogWarning` 并跳过，避免无限递归；
- 资产未命名（`actionName` 为空）时，节点名回退为资产名 `action.name`。

**第 4 步：运行与调参**

- 构建出的树会显示在 `BehaviourTreeRunner` 的 Inspector 中，运行时可继续调整节点参数（修改会随场景序列化保存）。

> 提示：菜单 `Trea/示例/创建示例节点资产` 可一键在 `Assets/TreaExample` 下生成一套示例资产链（见 §7.1）。

---

## 4. 节点详解

### 4.0 节点生命周期（BTNode 基类）

所有节点继承 `BTNode`（抽象类），核心成员：

| 成员 | 说明 |
|---|---|
| `string nodeName` | 节点名称（Inspector 中可改，默认类名） |
| `NodeState State` | 最近一次 Tick 的返回状态（`Running` / `Success` / `Failure`） |
| `bool Started` | 是否已进入（Enter 后为 true，Exit 后为 false） |
| `BTNode parent` | 父节点（`AddChild` / `SetChild` 时自动设置） |
| `Blackboard blackboard` | 共享黑板（`Bind` 时注入） |
| `NodeState Tick()` | 驱动入口：首次进入调 `OnEnter` → 调 `OnUpdate` → 非 Running 时调 `OnExit` |
| `void Abort()` | 中断：对运行中的节点调 `OnExit` 并置为 `Failure` |
| `void Bind(Blackboard)` | 绑定黑板并向下传播（`OnBindChildren`） |
| `BTNode Clone()` | 深拷贝树（组合/装饰子节点一并克隆） |

子类可重写的钩子：

| 钩子 | 时机 | 默认行为 |
|---|---|---|
| `OnEnter()` | 每次节点开始（首次 Tick 或上次结束后再次进入） | 空 |
| `OnUpdate()` | 每次 Tick | **必须实现**（返回 `NodeState`） |
| `OnExit()` | 节点结束（返回非 Running）或 `Abort` | 空 |
| `OnBindChildren()` | `Bind` 时 | 空（组合/装饰基类覆写为向下绑定） |

`ActionNode` / `ConditionNode` 封装了更友好的接口（见 §6.1）。

### 4.1 组合节点（CompositeNode，多个子节点）

| 节点 | 行为语义 | 参数 | Tick 返回规则 |
|---|---|---|---|
| `Sequence` | 按序执行子节点；任一子节点失败则整体失败 | — | 子节点 Failure → 返回 Failure；子节点 Running → 返回 Running（记录当前位置，下 tick 从该子继续）；全部 Success → Success |
| `Selector` | 按序尝试子节点；任一子节点成功则整体成功 | — | 子节点 Success → 返回 Success；子节点 Running → 返回 Running；全部失败 → Failure |
| `Parallel` | 同时 tick 所有子节点 | `successPolicy`：`RequireAll`（默认，全部成功才成功）/ `RequireOne`（至少一个成功即成功） | `RequireAll`：任一 Failure → Failure；有 Running → Running；全 Success → Success。`RequireOne`：有 Success → Success；无 Success 但有 Running → Running；全 Failure → Failure |
| `RandomSelector` | 进入时打乱子节点顺序（Fisher-Yates），再按 Selector 逻辑执行 | — | 同 Selector（每次 OnEnter 重新洗牌，Running 期间保持当前顺序与位置） |

> 组合节点退出（`OnExit`）时会把仍处于 `Running` 的子节点 `Abort()`，防止子节点悬挂。

### 4.2 装饰节点（DecoratorNode，唯一子节点）

| 节点 | 行为语义 | 参数 | Tick 返回规则 |
|---|---|---|---|
| `Inverter` | 结果取反 | — | 子 Success → Failure；子 Failure → Success；子 Running → Running |
| `Repeater` | 重复执行子节点 | `repeatCount`（`-1` = 无限；`>= 0` 时执行满 N 次后成功） | 子 Running → Running（不计数）；子完成 → 计数 +1，未到 N 次返回 Running 继续；到达 N 次返回子节点结果 |
| `UntilSuccess` | 反复执行直到子节点成功 | — | 子 Success → Success；子 Failure / Running → Running |
| `Timeout` | 子节点运行超过 `duration` 秒则中断并失败 | `duration`（秒） | 子 Running 且超时 → 先 `Abort()` 子节点再返回 Failure；其余情况透传子节点结果 |
| `Cooldown` | 子节点结束（非 Running）后进入冷却，冷却期间直接失败 | `cooldown`（秒） | 冷却期内 → Failure（不执行子节点）；否则执行子节点并透传结果；子节点结束瞬间记录冷却开始时间 |
| `Succeeder` | 子节点结果强制为成功 | — | 子 Running → Running；子 Success / Failure → Success |

> 装饰节点无子节点（`child == null`）时：`Inverter` / `Repeater` / `Timeout` / `UntilSuccess` / `Cooldown` 返回 `Failure`，`Succeeder` 返回 `Success`。

### 4.3 叶节点（Leaf）

| 节点类 | 行为语义 | 参数 | Tick 返回规则 |
|---|---|---|---|
| `WaitAction` | 等待一段时间后成功 | `duration`（秒） | 计时未满 → Running；满 → Success |
| `LogAction` | 打印日志后成功 | `message` | 立即 Success |
| `ChanceCondition` | 按概率判定成功 | `probability`（0~1，`[Range]`） | `Random.value < probability` → Success，否则 Failure |
| `AnimationAction` | 播放动画片段至结束 | `clip`（`AnimationClip`） | `clip == null` 或找不到 Animator → Failure；播放中 → Running；播完（`timer >= clip.length`）→ Success。查找方式：`owner.GetComponentInChildren<Animator>()`（资产方式下 owner = TreaActionReg 所在对象） |
| `MoveToAction` | 向目标移动至到达判定距离内 | `self`（移动对象）/ `target`（目标 Transform，可选）/ `targetPosition`（无 target 时用此坐标）/ `speed` / `arriveDistance` | `self == null` → Failure；距离 ≤ `arriveDistance` → Success；否则 Running（`Vector3.MoveTowards`） |

### 4.4 资产方式下可选的节点类型（ActionType 枚举）

`TreaAction` 的 `actionType` 即 `ActionType` 枚举，与上述节点一一对应：

```
Sequence, Selector, Parallel, RandomSelector,          // 组合
Inverter, Repeater, UntilSuccess, Timeout, Cooldown, Succeeder,  // 装饰
Wait, Log, Chance, Animation, MoveTo, Custom           // 叶（Custom 见 §6.3）
```

对应资产参数（Inspector 按类型显示，见 §7.5）：

| ActionType | 使用的 TreaAction 字段 |
|---|---|
| Repeater | `repeatCount` |
| Timeout / Wait | `duration` |
| Cooldown | `cooldown` |
| Chance | `probability` |
| Log | `message` |
| Animation | `animationClip` |
| MoveTo | `targetPosition` / `moveSpeed` / `arriveDistance` |
| Custom | `customTypeName` |

---

## 5. Blackboard（黑板）使用

`Blackboard` 是键值对共享存储，用于节点之间、以及外部与树之间传递数据。

### 5.1 API

```csharp
public class Blackboard
{
    public void Set<T>(string key, T value);                          // 写入/覆盖
    public T Get<T>(string key, T defaultValue = default);            // 读取，类型不符或不存在时返回 defaultValue
    public bool TryGet<T>(string key, out T value);                   // 安全读取（推荐）
    public bool Has(string key);                                      // 是否包含键
    public void Remove(string key);                                   // 删除键
    public void Clear();                                              // 清空全部
    public IEnumerable<string> Keys { get; }                          // 所有键（编辑器面板遍历用）
}
```

### 5.2 在节点中读写

节点内部通过 `blackboard` 字段直接访问（由 Runner 在 `SetTree` / `Awake` 时注入）：

```csharp
public class SetTargetAction : ActionNode
{
    protected override NodeState OnAction()
    {
        blackboard.Set("Target", playerTransform);          // 写入
        return NodeState.Success;
    }
}

public class ChaseAction : ActionNode
{
    protected override NodeState OnAction()
    {
        if (!blackboard.TryGet<Transform>("Target", out var target))
            return NodeState.Failure;                        // 没有目标
        // 使用 target ...
        return NodeState.Running;
    }
}
```

### 5.3 从外部访问

`BehaviourTreeRunner.blackboard` 是公开字段（`[HideInInspector]`），运行时可从外部读写：

```csharp
runner.blackboard.Set<int>("hp", 100);
int hp = runner.blackboard.Get<int>("hp", 0);   // 带默认值
```

### 5.4 类型说明

- 键为 `string`，值为任意 `object`（`Set<T>` 按 T 装箱存储）；
- `Get<T>` / `TryGet<T>` 会做类型检查：存储类型与请求的 `T` 不一致时视为不存在（返回默认值 / false），因此**读写请使用相同类型**；
- 运行时可在 Runner Inspector 的「黑板数据」区域查看所有键值（见 §7.4）。

---

## 6. 自定义节点开发

### 6.1 继承基类

**动作节点**：继承 `ActionNode`，实现 `OnAction()`，可选覆写 `OnStart()` / `OnStop()`（对应 BTNode 的 `OnEnter` / `OnExit`，已被 `sealed` 封装）：

```csharp
public class MyAction : ActionNode
{
    protected override void OnStart() { }                 // 进入时一次
    protected override NodeState OnAction() { ... }        // 每 tick
    protected override void OnStop() { }                  // 退出时一次
}
```

**条件节点**：继承 `ConditionNode`，实现 `Check()`（返回 bool，自动映射 Success / Failure）：

```csharp
public class MyCondition : ConditionNode
{
    protected override bool Check() { ... }
}
```

**组合 / 装饰节点**：也可继承 `CompositeNode` / `DecoratorNode` 直接扩展（参考内置实现）。

### 6.2 自动注册

插件使用 `TypeCache.GetTypesDerivedFrom<BTNode>()` 自动收集**项目中所有非抽象、无泛型参数**的 `BTNode` 派生类型，无需手动注册：

- 运行时树的类型菜单（BTNodeDrawer：选择 / 更换节点类型）自动包含新类；
- `TreaAction` 资产类型 `Custom` 的下拉列表自动包含新类（显示 `FullName`）。

> 新类编译完成后即可出现；若编辑器菜单未刷新，重新编译或重启编辑器即可。

### 6.3 资产方式使用自定义节点

1. 在 `TreaAction` 资产上选择节点类型 `Custom`；
2. Inspector 会出现 **自定义节点类** 下拉框，选择你的类（存入 `customTypeName`，值为 `AssemblyQualifiedName`）；
3. 构建时 `TreaActionReg.CreateCustomNode` 通过 `Type.GetType(customTypeName)` + `Activator.CreateInstance(type)` 反射创建实例；
4. 反射要求：类必须有**公共无参构造函数**（默认构造函数即可），且继承 `BTNode`；
5. **自定义节点的参数**不在资产面板中配置——构建完成后在 Runner 的 Inspector 里直接配置该节点实例的字段（会随场景序列化保存）。

失败场景（均输出 `Debug.LogWarning`）：未填 `customTypeName`；类型不存在；`Activator.CreateInstance` 抛异常。

### 6.4 参考实现

`Samples~/TreaExample/ExampleEnemyAI.cs` 中包含 5 个自定义节点示例：`IsPlayerVisibleCondition`、`InAttackRangeCondition`（ConditionNode）、`SetTargetAction`、`ChaseAction`、`AttackAction`、`PatrolAction`（ActionNode），可对照学习。

---

## 7. 编辑器工具

### 7.1 菜单入口

| 菜单路径 | 来源 | 作用 |
|---|---|---|
| `Trea/示例/创建示例节点资产` | `TreaMenu.cs`（Editor） | 在 `Assets/TreaExample` 下创建一套示例节点资产：`Root_Selector`（根，Selector）、`Chase_Sequence`（追击序列：发现玩家 Chance 0.8 → 等待1秒 → 攻击动画）、`Patrol_Sequence`（巡逻序列：打印巡逻 → 等待2秒），并自动搭好子节点关系。创建后选中 `Root_Selector`，可将它挂到 `TreaActionReg` 的根节点资产使用 |
| `Trea/创建示例敌人` | `SampleMenu.cs`（Samples~ 导入后） | 创建 `ExampleEnemy` 对象（含 `BehaviourTreeRunner` + `ExampleEnemyAI`），自动调用 `BuildTree()` 构建行为树并选中该对象；控制台提示在 ExampleEnemyAI 上指定 Player 或使用 Tag 为 Player 的物体 |

### 7.2 节点创建 / 更换

- 右键资产创建入口：`Assets → Create → Trea/节点资产`（`[CreateAssetMenu]`，menuName = `Trea/节点资产`）。

### 7.3 BTNodeDrawer（Runner 根节点递归绘制）

`[CustomPropertyDrawer(typeof(BTNode), true)]`，作用于 `BehaviourTreeRunner.root`（`[SerializeReference]`）：

- **头部**：类型按钮（点击弹出类型菜单，可**更换节点类型**，参数会重置）；名称文本框（编辑 `nodeName`）；右侧 `×` 删除节点（带 Undo）；
- **组合节点**：「子节点」列表：`+` 添加子节点（类型菜单）、`▲▼` 上下排序、`×` 删除（均带 Undo）；
- **节点底色**按类型区分：条件=黄、动作=绿、组合=蓝、装饰=青、其他=灰；
- **运行时状态色**：Play 模式下每个已开始 / 已结束的节点右侧显示状态色块：绿 = Success、红 = Failure、黄 = Running；
- 空节点显示「选择节点类型...」下拉按钮。

### 7.4 BehaviourTreeRunnerEditor（Runner Inspector）

- 显示 **根节点**（可展开整棵树）与 **Tick 间隔（秒）**；
- Play 模式下：
  - **「重置行为树」** 按钮（调用 `ResetTree()`）；
  - **黑板数据** 实时列表（遍历 `blackboard.Keys` 显示键值）。

### 7.5 TreaAction 资产 Inspector（TreaActionEditors）

- 始终显示：节点类型、节点名称、子节点列表；
- 按类型动态显示参数（见 §4.4 表格）；
- 类型 `Custom` 时显示自定义节点类下拉框；
- 结构校验提示（HelpBox）：
  - 组合节点无子节点 → 警告「组合节点至少需要 1 个子节点」；
  - 装饰节点子节点多于 1 → 警告「装饰节点只能有 1 个子节点」；
  - 叶节点带子节点 → 警告「叶节点不能有子节点」；
  - 自定义类型找不到 → 错误提示。

### 7.6 TreaActionReg Inspector

- 字段：根节点资产、行为树 Runner、启动时自动构建（树为空时）；
- **「构建行为树」** 按钮（调用 `BuildAction()`，等价于组件右键菜单项）。

---

## 8. 示例演示（ExampleEnemyAI）

`Trea/创建示例敌人` 生成的敌人（`ExampleEnemy`）演示了完整用法。构建出的树结构（`ExampleEnemyAI.BuildTree()`）：

```
Selector「根节点」
├── Sequence「攻击」
│   ├── InAttackRangeCondition   条件：与 Player 距离 ≤ attackRange
│   └── AttackAction             攻击：计时 attackDuration 秒后 DealDamage() 造成 attackDamage 伤害
├── Sequence「追击」
│   ├── IsPlayerVisibleCondition 条件：与 Player 距离 ≤ sightRange
│   ├── SetTargetAction          动作：把 Player 写入黑板 "Target"
│   └── Timeout「追击超时」duration=5
│       └── ChaseAction          动作：向黑板 "Target" 移动（chaseSpeed），进入攻击范围返回 Success
└── PatrolAction「巡逻」          动作：在 patrolPoints 之间往返移动（patrolSpeed）
```

优先级逻辑：攻击 > 追击 > 巡逻（Selector 语义：先尝试攻击分支，条件不满足则尝试追击，再不行就巡逻）。

关键字段（`ExampleEnemyAI` Inspector）：

| 字段 | 默认值 | 说明 |
|---|---|---|
| `runner` | — | 树驱动器（为空时 `GetComponent`） |
| `player` | — | 玩家 Transform；为空时按 Tag `Player` 查找 |
| `sightRange` | 10 | 视野半径 |
| `attackRange` | 2 | 攻击 / 追击终止距离 |
| `patrolSpeed` / `chaseSpeed` | 2 / 5 | 巡逻 / 追击速度 |
| `attackDuration` | 0.5 | 攻击动作时长 |
| `attackDamage` | 10 | 单次伤害 |
| `patrolPoints` | (5,0,0) / (-5,0,0) | 巡逻路径点 |

配套组件：`Health`（示例血量，`TakeDamage` 扣血，归零打印「被击败了」）。玩家物体挂 `Health` 即可被扣血；未挂则 `DealDamage` 只打印日志。

---

## 9. 常见问题与注意事项

| 现象 / 问题 | 说明与处理 |
|---|---|
| Runner 不运行 | 确认 `root` 不为空（代码方式需调用 `SetTree`；资产方式需构建成功），且 `tickInterval` > 0 |
| 资产方式构建无反应 | 检查 `TreaActionReg` 是否绑定 Runner（留空会自动获取）、根节点资产是否指定；控制台会有 `Debug.LogWarning` |
| 装饰节点子节点“丢失” | 资产方式下装饰节点只读取 `children[0]`，多余子节点被忽略（Inspector 有警告） |
| 循环引用导致树异常 | 资产方式构建会自动检测并跳过循环引用并警告；请检查资产树是否存在环 |
| `Custom` 构建失败 | 确认 `customTypeName` 下拉已正确选择、类有公共无参构造函数、继承 `BTNode` |
| 自定义节点参数没地方填 | 资产面板不提供自定义参数；构建后到 Runner 的 Inspector 中配置该节点实例 |
| `Animation` 不播放 | `clip` 为空、或 TreaActionReg 所在对象（含子物体）上找不到 `Animator` |
| 黑板读取不到值 | `Get<T>` / `TryGet<T>` 要求读写类型一致（内部做 `is T` 检查）；先 `Set` 再 `Read` |
| 更换节点类型后参数丢失 | BTNodeDrawer 更换类型会 `Activator.CreateInstance` 重建实例，旧参数不保留（属预期行为） |
| 版本兼容 | 最低 Unity 6000.0；低版本（2021/2022 LTS）未适配，`[SerializeReference]` 编辑器绘制可能异常 |
| 插件处于建设中 | 接口可能调整；未发布正式版本（Alpha / 0.1.0） |

---

## 10. 路线图 / 建设进度

### 已实现

- 完整行为树运行时：节点生命周期（Enter / Update / Exit）、`Abort` 中断、`Clone` 深拷贝、`Bind` 黑板绑定；
- 4 种组合节点（Sequence / Selector / Parallel / RandomSelector）、6 种装饰节点（Inverter / Repeater / UntilSuccess / Timeout / Cooldown / Succeeder）、5 种内置叶节点（Wait / Log / Chance / Animation / MoveTo）；
- 黑板共享数据（Set / Get / TryGet / Has / Remove / Clear / Keys）；
- `BehaviourTreeRunner` 驱动：固定间隔 Tick、`SetTree` 动态换树、`ResetTree` 重置；
- `TreaAction` 节点资产 + `TreaActionReg` 资产转运行时树（含循环引用检测、启动自动构建）；
- UPM 包结构：`package.json`（`com.trea.behaviortree` 0.1.0，unity 6000.0）、Runtime / Editor 程序集隔离（asmdef）、`Samples~` 官方示例导入；
- 编辑器支持：菜单入口、Runner / 资产 / 注册组件自定义 Inspector、递归节点绘制（换类型 / 增删 / 排序 / 运行时状态色 / 黑板查看 / 重置按钮）；
- 示例敌人 AI（巡逻 / 追击 / 攻击）与血量组件。

### 待办 / 已知限制

- 尚未发布正式版本，接口可能调整；
- 无独立可视化节点图编辑器（GraphView 拖拽画布）——当前为 Inspector 递归列表绘制；
- `Animation` 节点依赖所在对象（含子物体）的 `Animator`（`GetComponentInChildren`）；
- `Custom` 节点在资产 Inspector 中无参数面板，需构建后在 Runner 上配置；
- 无内置单元测试用例；示例 `Samples~` 需手动 Import 导入。

---

*（内容由AI生成，仅供参考）*
