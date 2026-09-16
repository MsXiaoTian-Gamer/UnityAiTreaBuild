# MengShenBehaviorTree

> 新手友好的 Unity 行为树（Behavior Tree）AI 插件：全程在 **Inspector** 里配置，无需写可视化编辑器。
> 项目正在建设中并**持续更新中**，接口可能随版本调整。

MengShenBehaviorTree（包名 `com.trea.behaviortree`，当前版本 `0.1.0`）是一套轻量、无外部依赖的行为树 AI 插件，命名空间统一为 `Trea`。仓库根目录即 UPM 包，可直接通过 Package Manager 安装。

## 功能特性

- **完整的运行时**：节点生命周期管理（Enter / Update / Exit）、`Abort` 中断、`Clone` 深拷贝、固定间隔 Tick 驱动；
- **两套搭建方式**：
  - 直接在 `BehaviourTreeRunner` 上配置（`[SerializeReference]` 递归树，Inspector 可视化编辑）；
  - 用 `TreaAction` 资产 + `TreaActionReg` 注册搭建（ScriptableObject 资产树，自动转换为运行时树，含循环引用检测）；
- **内置节点齐全**：组合（Sequence / Selector / Parallel / RandomSelector）、装饰（Inverter / Repeater / UntilSuccess / Timeout / Cooldown / Succeeder）、通用叶节点（Wait / Log / Chance / Animation / MoveTo）；
- **Blackboard 黑板**：跨节点共享数据（`Set` / `Get` / `TryGet` / `Has` / `Remove` / `Clear`），自动绑定到整棵树；
- **自定义节点零配置**：继承 `ActionNode` / `ConditionNode` 写一个类，自动出现在类型菜单里；
- **编辑器工具完善**：运行模式 Inspector 实时显示节点状态（Running 黄 / Success 绿 / Failure 红）和黑板数据、一键创建示例敌人 / 示例节点资产菜单、结构校验提示；
- **UPM 规范**：`Runtime` / `Editor` 程序集隔离（asmdef）、`Samples~` 官方示例、MIT License。

## 使用说明

- [USE.md](USE.md) — 插件使用说明（功能定位、安装接入、节点详解、Blackboard、自定义节点、编辑器工具、示例、FAQ、路线图）

## 目录 / 包结构

```
UnityAiTreaBuild/
├── package.json                  UPM 包清单（name/version/unity/samples）
├── README.md / LICENSE.md / USE.md
├── Runtime/                      运行时源码（程序集 Trea.Runtime）
│   ├── BehaviourTreeRunner.cs    树驱动器（MonoBehaviour，固定间隔 Tick）
│   ├── TreaAction.cs             ScriptableObject 节点资产
│   ├── TreaActionReg.cs          资产树 → 运行时树转换器（含循环引用检测）
│   ├── Core/                     BTNode / CompositeNode / DecoratorNode / NodeState
│   ├── Composites/               Sequence / Selector / Parallel / RandomSelector
│   ├── Decorators/               Inverter / Repeater / UntilSuccess / Timeout / Cooldown / Succeeder
│   ├── Leaves/                   ActionNode / ConditionNode / 通用叶节点
│   └── Blackboard/               Blackboard.cs（共享数据黑板）
├── Editor/                       编辑器源码（程序集 Trea.Editor）
│   ├── TreaMenu.cs               菜单：Trea/示例/创建示例节点资产
│   ├── BTNodeDrawer.cs           Inspector 递归节点绘制器（含运行时状态色）
│   ├── BehaviourTreeRunnerEditor.cs  Runner 自定义 Inspector
│   └── TreaActionEditors.cs      TreaAction / TreaActionReg 自定义 Inspector
└── Samples~/                     示例（UPM 规范，需手动 Import）
    └── TreaExample/              ExampleEnemyAI / Health / 创建示例敌人菜单
```

## 安装

**方式一：Git URL（推荐）**

1. 打开 `Window → Package Manager`
2. 点左上角 `+` → `Add package from git URL...`
3. 粘贴仓库地址，例如 `https://github.com/MsXiaoTian-Gamer/UnityAiTreaBuild.git`

**方式二：本地包**

把仓库内容复制到项目的 `Packages/com.trea.behaviortree/` 目录（Unity 会自动识别）。

**方式三：手动复制脚本**

把 `Runtime/` 和 `Editor/` 下的 `.cs` 文件复制到项目的 `Assets/` 下任意位置。

> 环境要求：**Unity 6000.0（Unity 6.0 系列）及以上**，无第三方运行时依赖。

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

> 菜单 `Trea/示例/创建示例节点资产` 可一键生成一套示例资产链。

### 方式 C：纯代码构建（推荐开发使用）

继承节点类写逻辑，用组合 / 装饰节点组装树，交给 Runner：

```csharp
using Trea;
using UnityEngine;

public class ChaseAction : ActionNode
{
    public Transform self;
    public Transform target;
    public float speed = 5f;

    protected override NodeState OnAction()
    {
        if (self == null || target == null) return NodeState.Failure;
        self.position = Vector3.MoveTowards(self.position, target.position, speed * Time.deltaTime);
        return Vector3.Distance(self.position, target.position) <= 0.2f
            ? NodeState.Success
            : NodeState.Running;
    }
}

public class Enemy : MonoBehaviour
{
    private void Awake()
    {
        var runner = GetComponent<BehaviourTreeRunner>();
        if (runner == null) runner = gameObject.AddComponent<BehaviourTreeRunner>();

        var root = new Selector { nodeName = "根节点" };
        var chase = new Sequence { nodeName = "追击" };
        chase.AddChild(new IsPlayerVisible());
        chase.AddChild(new ChaseAction { self = transform, speed = 5f });
        root.AddChild(chase);
        root.AddChild(new PatrolAction { self = transform, nodeName = "巡逻" });

        runner.SetTree(root);
    }
}
```

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
- 写完类后自动出现在 Runner 树和 TreaAction（类型选 `Custom`）的类型菜单里（基于 `TypeCache` 自动收集，无需手动注册）
- 自定义节点参数在构建后的树上配置（Runner 的 Inspector），会随场景保存
- 资产方式选 `Custom` 时通过反射创建，类需有公共无参构造函数
- 节点间共享数据用黑板：`blackboard.Set("Target", t)` / `blackboard.Get<Transform>("Target")`

## 内置节点一览

### 组合节点（Composite）

| 类型 | 说明 | 参数 |
| --- | --- | --- |
| `Sequence` | 顺序执行子节点，任一失败即失败 | — |
| `Selector` | 依次尝试子节点，任一成功即成功 | — |
| `Parallel` | 同时执行所有子节点 | `successPolicy`：`RequireAll`（默认，全成功才成功）/ `RequireOne`（至少一个成功） |
| `RandomSelector` | 进入时随机打乱子节点顺序，再按选择逻辑执行 | — |

### 装饰节点（Decorator，唯一子节点）

| 类型 | 说明 | 参数 |
| --- | --- | --- |
| `Inverter` | 结果取反 | — |
| `Repeater` | 重复执行 N 次 | `repeatCount`（-1 = 无限） |
| `UntilSuccess` | 直到子节点成功为止 | — |
| `Timeout` | 子节点运行超过时限则中断并失败 | `duration`（秒） |
| `Cooldown` | 子节点结束后冷却，冷却期间直接失败 | `cooldown`（秒） |
| `Succeeder` | 子节点结果强制为成功 | — |

### 叶节点（Leaf）

| 类型 | 说明 | 参数 |
| --- | --- | --- |
| `Wait` | 等待 N 秒后成功 | `duration` |
| `Log` | 打印日志后成功 | `message` |
| `Chance` | 按概率判定成功 | `probability`（0~1） |
| `Animation` | 播放动画片段，播完成功（查找所在对象及子物体的 Animator） | `animationClip` |
| `MoveTo` | 移动到目标点后成功 | `targetPosition` / `moveSpeed` / `arriveDistance` |
| `Custom` | 反射创建自定义 BTNode 类型 | `customTypeName` |

> Tick 返回值语义：`Running` = 未完成，下个 tick 继续；`Success` / `Failure` = 本次执行结束。完整生命周期与各节点返回规则详见 [USE.md](USE.md) 第 4 章。

## Blackboard（黑板）简要说明

- `BehaviourTreeRunner` 在 `SetTree` / `Awake` 时自动把黑板绑定到整棵树，所有节点通过 `blackboard` 字段直接访问；
- 键为 `string`，值为任意对象；`Get<T>` / `TryGet<T>` 会做类型检查，读写需类型一致；
- 外部访问：`runner.blackboard.Set<T>(key, value)` / `Get<T>(key, defaultValue)`；
- 运行模式 Runner Inspector 可实时查看黑板数据。

## 编辑器工具

| 工具 | 说明 |
| --- | --- |
| `Trea/示例/创建示例节点资产` | 一键生成示例节点资产链（根选择节点 + 追击序列 + 巡逻序列）到 `Assets/TreaExample` |
| `Trea/创建示例敌人` | 一键创建 `ExampleEnemy` 对象（含 Runner + 示例 AI），自动构建行为树 |
| Runner Inspector | 递归编辑整棵树：换类型 / 增删 / 排序 / 改名称；运行时显示节点状态色与黑板数据、提供「重置行为树」按钮 |
| TreaAction Inspector | 按类型动态显示参数，含结构校验提示（组合至少 1 子、装饰只能 1 子、叶不能有子） |

## 示例

Package Manager 里选中本包 → Samples → Import，导入「示例敌人 AI」：

- `ExampleEnemyAI.cs`：巡逻 → 发现玩家 → 追击 → 攻击完整示例（含自定义节点写法：`IsPlayerVisibleCondition` / `ChaseAction` / `AttackAction` / `PatrolAction` 等）
- 树结构：`Selector`（攻击 > 追击 > 巡逻）；追击分支用 `Timeout(5s)` 限制追击时长，`SetTargetAction` 把玩家写入黑板
- 菜单 `Trea/创建示例敌人` 一键生成，需给玩家物体加 `Health` 组件并设置 Tag 为 `Player`

## 路线图 / 建设进度

**已实现**
- 完整行为树运行时（生命周期 / Abort / Clone / Bind）、4 组合 + 6 装饰 + 5 通用叶节点；
- Blackboard 共享数据；Runner 驱动（tickInterval / SetTree / ResetTree）；
- TreaAction 资产 + TreaActionReg 转换（循环引用保护、启动自动构建）；
- UPM 包结构（package.json / asmdef / Samples~）；编辑器工具（菜单、递归绘制、状态色、黑板查看）；
- 示例敌人 AI 与血量组件。

**待办 / 已知限制**
- 未发布正式版本（Alpha / 0.1.0），接口可能调整；
- 无独立可视化节点图编辑器（GraphView 拖拽画布），当前为 Inspector 递归列表绘制；
- `Animation` 节点依赖所在对象（含子物体）的 `Animator`；
- `Custom` 节点在资产 Inspector 中无参数面板，需构建后在 Runner 上配置；
- 无内置单元测试用例。

## 环境要求

- Unity 6000.0 及以上（package.json `unity` 字段）
- 无第三方运行时依赖

## License

MIT
