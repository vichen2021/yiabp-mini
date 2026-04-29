# Git 提交规范

## 格式

遵循 **Conventional Commits** 规范，描述使用中文：

```
<type>(<scope>): <中文描述>
```

## 类型（type）

| 类型 | 含义 | 示例 |
|------|------|------|
| `feat` | 新功能 | `feat(rbac): 新增部门树形查询接口` |
| `fix` | 修复缺陷 | `fix(rbac): 修复角色删除时未清理关联数据` |
| `refactor` | 重构 | `refactor(domain): 重构领域服务以统一事件发布` |
| `chore` | 杂项维护 | `chore(deps): 升级 ABP 至 10.0.2` |
| `docs` | 文档 | `docs(project): 更新项目规则文档` |

## 作用域（scope）

作用域优先对应模块或变更边界：`rbac`, `tenant`, `audit`, `setting`, `framework`, `frontend`, `docs`, `deps`, `project` 等。

~~`codegen` 作用域用于旧版 code-gen 模块。~~ 2.0 起该模块已移除，代码生成能力改为 Skills；仅在修改历史兼容说明时使用 `docs` 或 `project`。

## 示例

```
feat(rbac): 新增用户管理功能
fix(rbac): 修复角色权限分配问题
refactor(domain): 重构用户聚合根
chore(deps): 升级依赖版本
docs(project): 更新 README
```

