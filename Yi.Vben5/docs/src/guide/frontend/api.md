# API 调用

## API 文件结构

API 文件位于 `apps/web-antd/src/api/` 目录下，按模块组织：

```
api/
├── rbac/
│   ├── user/
│   │   └── index.ts
│   └── role/
│       └── index.ts
```

## API 调用示例

```typescript
import { getUserList, createUser, updateUser, deleteUser } from '@/api/rbac/user';

// 获取列表
const list = await getUserList(params);

// 创建
await createUser(data);

// 更新
await updateUser(id, data);

// 删除
await deleteUser(ids);
```

## 请求拦截

API 请求通过 Axios 拦截器统一处理：

- 请求拦截：添加 Token、设置请求头
- 响应拦截：统一错误处理、数据格式化

## 相关文档

- [本地开发](/guide/frontend/development) - 了解开发环境
- [组件使用](/guide/frontend/components) - 了解组件使用

