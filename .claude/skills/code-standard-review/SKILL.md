---
name: code-standard-review
description: 执行代码规范审查脚本，对指定工作副本进行后端静态分析（跨模块引用、层依赖方向、DDD 构造块位置、AppService 命名、Repository 位置、Entity 基类等 12 项检查）、前端类型检查（pnpm check:type），可选 dotnet build。当用户提到"代码规范审查"、"code standard review"、"规范检查"、"跑一下检查"、"检查一下分支"、"规范扫描"、"验证代码规范"、"检查后端规范"、"执行 code-standard-review"、"对 xx 分支跑检查"时使用此技能。即使用户只说"检查一下"或"跑脚本"，只要上下文涉及代码规范、构建验证或分支质量，都应触发此技能。
---

# 代码规范审查

通过执行 PowerShell 规范审查脚本对工作副本进行多维度代码规范验证。

## 脚本位置

脚本随工作副本携带，路径固定为：

```
<工作副本根目录>\.claude\skills\code-standard-review\scripts\code-standard-review.ps1
```

## 参数说明

| 参数 | 类型 | 默认行为 | 用途 |
|------|------|----------|------|
| `-Root` | string | 必填 | 目标工作副本根目录（含 `Yi.Abp` / `Yi.Vben5`） |
| `-RunBuild` | switch | 不传=跳过 | 启用 `dotnet build` 后端构建检查 |
| `-SkipTypeCheck` | switch | 不传=执行 | 跳过前端 `pnpm check:type` |
| `-SkipBackendLint` | switch | 不传=执行 | 跳过后端静态分析（2b~2m） |

## 工作流程

### 步骤 1：确认目标工作副本

基于当前上下文确定要检查的工作副本根目录。不明确时询问用户。

### 步骤 2：选择检查范围

默认运行除 `dotnet build` 外的全部检查。常见组合：

| 场景 | 命令模板 |
|------|----------|
| 标准检查（推荐） | `-Root "<工作副本>"` |
| 含后端构建 | `-Root "<工作副本>" -RunBuild` |
| 仅后端静态分析 | `-Root "<工作副本>" -SkipTypeCheck` |
| 仅前端类型检查 | `-Root "<工作副本>" -SkipBackendLint` |
| 前端 `node_modules` 未安装时 | 加 `-SkipTypeCheck` |

### 步骤 3：执行脚本

使用 PowerShell 7 (`pwsh`) 调用，脚本路径和 `-Root` 均使用绝对路径，不要使用 `cd`：

```powershell
pwsh -File "<工作副本根目录>\.claude\skills\code-standard-review\scripts\code-standard-review.ps1" -Root "<工作副本绝对路径>" [其他开关]
```

### 步骤 4：输出脚本原始内容

脚本以分节方式输出，每节使用 `== N. 标题 ==` 标识：

- **[PASS]** 绿色：通过
- **[FAIL]** 红色：违规，阻断结果
- **[WARN]** 黄色：警告，不阻断
- **[SKIP]** 黄色：该项已跳过

末尾汇总：
- `== 代码规范审查结束：PASS ==` 退出码 0
- `== 代码规范审查结束：FAIL ==` 退出码 1

**强制要求**：必须将脚本的原始输出完整粘贴到回复中（包裹在 ` ``` ` 代码块内），保留所有 `==` 分节标题、`[PASS]`/`[FAIL]`/`[WARN]`/`[SKIP]` 标记和具体条目。**不要省略任何一节，不要用"全部通过"等概括代替原始输出。**

**唯一允许的精简**：当某个 `[FAIL]` 块包含多行报错堆栈（例如 TypeScript 编译错误、dotnet build 错误堆栈、pnpm/turbo 多行输出）时，将该 `[FAIL]` 块的堆栈部分压缩为一行摘要，格式为：
```
[FAIL] <原始失败描述>
    [堆栈精简]：<文件>(行,列) <错误码> <核心原因>；<下一处>...
```
其他所有 PASS / WARN / SKIP 节必须原样输出。

### 步骤 5：报告与建议

在步骤 4 原始输出之后，附加一个简短的结论与修复建议章节：

1. 整体结果（PASS / FAIL，退出码）
2. FAIL 项的修复方向（不要直接修改代码，除非用户授权）
3. 若 SKIP 是因为缺少 `node_modules`、`sln` 或目录不存在，提示用户
4. WARN 项不在结论中展开，仅在用户索取时详细说明

## 检查项清单

| 编号 | 检查项 | 类型 | 可跳过开关 |
|------|--------|------|------------|
| 1 | dotnet build 构建 | FAIL | 默认跳过（`-RunBuild` 启用） |
| 2 | 跨模块非法直接引用 | FAIL | 无 |
| 2b | 层依赖方向 | FAIL | `-SkipBackendLint` |
| 2c | DDD 构造块位置（Manager / Dto） | FAIL | `-SkipBackendLint` |
| 2d | AppService 命名规范 | FAIL | `-SkipBackendLint` |
| 2e | Repository 接口位置 | FAIL | `-SkipBackendLint` |
| 2f | Repository 实现位置 | FAIL | `-SkipBackendLint` |
| 2g | Application 层直接 DB 操作 | FAIL | `-SkipBackendLint` |
| 2h | Domain 层 ASP.NET Core 污染 | FAIL | `-SkipBackendLint` |
| 2i | Entity ABP 基类继承 | FAIL | `-SkipBackendLint` |
| 2j | AppService 接口继承规范 | FAIL | `-SkipBackendLint` |
| 2k | Domain.Shared 反向引用 | FAIL | `-SkipBackendLint` |
| 2l | 项目 AbpModule 类完整性 | FAIL | `-SkipBackendLint` |
| 2m | Service 方法接口声明 | WARN | `-SkipBackendLint` |
| 3 | 前端类型检查 | FAIL | `-SkipTypeCheck` |

## 输出报告模板

报告格式固定为「原始输出 + 结论建议」两段：

````markdown
```
== 1. 后端构建检查 (dotnet build) ==
  [SKIP] ...

== 2. 跨模块非法引用检测 ==
  [PASS] ...

...（原样保留所有节，仅 FAIL 节内的堆栈可压缩为一行 [堆栈精简]）...

== 代码质量检查结束：FAIL ==
```

---

**结果**：FAIL（退出码 1）

修复方向：
- <FAIL 项 1 的修复建议>
- <FAIL 项 2 的修复建议>
````

## 注意事项

- 脚本输出中 `[SKIP]` 是黄色不是失败，不要误判为问题。
- 前端类型检查首次运行需先在 `Yi.Vben5` 目录执行 `pnpm install`。
- `dotnet build` 较慢（30s~2min），仅在用户明确要求或有理由怀疑构建失败时启用 `-RunBuild`。
- 警告级别（WARN）的项目除非用户明确要求，否则不主动修复。
