# 测试规范

## 框架与工具

- **测试框架**：xUnit
- **断言库**：Shouldly
- **测试数据库**：SQLite（每次运行生成唯一数据库文件）

## 测试命名

格式：`{Action}_{Entity}_Test`

```csharp
[Fact]
public async Task Get_User_Test() { ... }

[Fact]
public async Task Create_User_Test() { ... }

[Fact]
public async Task Update_User_Test() { ... }

[Fact]
public async Task Delete_User_Test() { ... }
```

## 测试风格

测试通过应用服务层进行（集成测试风格），使用 `ServiceProvider.GetRequiredService<T>()` 解析服务。

## 相关文档

- [编码规范](/guide/standards/coding) - 了解编码规范

