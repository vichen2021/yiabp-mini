# 代码生成模块 API 接口清单

## 接口检查结果

### ✅ 已实现的接口

#### 1. CodeGenService (`/code-gen`)
- ✅ `POST /code-gen/web-build-code` - Web To Code（从Web配置生成代码）
- ✅ `POST /code-gen/code-build-web` - Code To Web（从代码反向生成表结构）
- ✅ `POST /code-gen/dir/{path}` - 打开目录

#### 2. TableService (`/table`)
- ✅ `GET /table` - 获取数据表列表（分页）
- ✅ `GET /table/{id}` - 获取数据表详情
- ✅ `POST /table` - 新增数据表
- ✅ `PUT /table/{id}` - 更新数据表
- ✅ `DELETE /table` - 删除数据表（支持批量）

#### 3. FieldService (`/field`)
- ✅ `GET /field` - 获取字段列表（分页，支持按表ID和名称筛选）
- ✅ `GET /field/{id}` - 获取字段详情
- ✅ `POST /field` - 新增字段
- ✅ `PUT /field/{id}` - 更新字段
- ✅ `DELETE /field` - 删除字段（支持批量）
- ✅ `GET /field/type` - 获取字段类型枚举

#### 4. TemplateService (`/template`)
- ✅ `GET /template` - 获取模板列表（分页，支持按名称筛选）
- ✅ `GET /template/{id}` - 获取模板详情
- ✅ `POST /template` - 新增模板
- ✅ `PUT /template/{id}` - 更新模板
- ✅ `DELETE /template` - 删除模板（支持批量）

### ❌ 未实现的接口（后端抛出异常，无需实现）

- `POST /code-gen/web-build-db` - Web To Db（未实现）
- `POST /code-gen/code-build-db` - Code To Db（未实现）

## 注意事项

1. **字段类型枚举接口**：后端返回的字段名是 `lable`（拼写错误），前端已做映射处理，统一转换为 `label`。

2. **代码生成接口**：后端目前会使用所有模板生成代码，不支持选择特定模板。

3. **打开目录接口**：仅支持 Windows 系统，路径需要 URL 编码。

## 接口使用示例

```typescript
// 生成代码
await postWebBuildCode(['table-id-1', 'table-id-2']);

// 从代码反向生成表结构
await postCodeBuildWeb();

// 获取字段类型枚举
const types = await getFieldTypeEnum();
// 返回: [{ label: 'String', value: 0 }, { label: 'Int', value: 1 }, ...]

// 打开目录
await postOpenDir('D:\\code\\Entities');
```
