# Troubleshooting Guide

Common issues and solutions when creating business modules.

## Build Errors

### Error: "未能找到类型或命名空间名 PagedInfraInput"

**Problem**: Using wrong base class for GetListInputVo.

**Solution**: Use `PagedAllResultRequestDto` instead:

```csharp
using Yi.Framework.Ddd;
using Yi.Framework.Ddd.Application.Contracts;

public class {EntityName}GetListInputVo : PagedAllResultRequestDto
{
    // Properties
}
```

### Error: "文件被进程锁定" (File locked by process)

**Problem**: Application is running and locking DLL files.

**Solution**:
1. Stop the running application (Yi.Abp.Web process)
2. Close any debuggers (JetBrains.Debugger.Worker)
3. Run `dotnet build` again

### Error: Missing using statements

**Problem**: Compilation errors about missing types.

**Solution**: Add required using statements:

```csharp
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Yi.Framework.Core.Data;
using Yi.Framework.Ddd.Application.Contracts;
```

## Menu Issues

### Menu not appearing in UI

**Problem**: Forgot to create menu seed data or didn't add to existing menu file.

**Solution**:
1. Check if menu seed data file exists
2. Verify permission codes match frontend v-access directives
3. Ensure menu is added to correct parent menu
4. Check `SeedAsync` condition doesn't skip seeding

### Permission denied errors

**Problem**: Permission codes don't match between backend and frontend.

**Solution**: Ensure consistent format:
- Backend: `{module-name}:{entity-name}:{action}`
- Frontend: `v-access:code="['{module-name}:{entity-name}:add']"`

## Frontend Issues

### API calls returning 404

**Problem**: API endpoint doesn't match backend route.

**Solution**: Check API root path matches backend route:
```typescript
enum Api {
  root = '/{module-name}/{entity-name}',  // Must match backend route
}
```

### Table not loading data

**Problem**: Pagination parameters don't match backend expectations.

**Solution**: Ensure query uses correct parameter names:
```typescript
return await {entityName}List({
  SkipCount: page.currentPage,
  MaxResultCount: page.pageSize,
  ...formValues,
});
```

### Drawer form not populating on edit

**Problem**: Data transformation needed for complex fields.

**Solution**: Transform data in `onOpenChange`:
```typescript
if (isUpdate.value && id) {
  const record = await {entityName}Info(id);
  // Transform if needed (e.g., string to array)
  const formValues = {
    ...record,
    relatedIds: record.relatedIds?.split(',') || [],
  };
  await formApi.setValues(formValues);
}
```

## Common Mistakes

### 1. Forgetting Menu Seed Data

**Symptom**: Module works in backend but doesn't appear in UI.

**Fix**: Always create menu seed data as part of Step 2.5.

### 2. Wrong DTO Base Class

**Symptom**: Compilation error about missing type.

**Fix**: Use `PagedAllResultRequestDto` for GetListInputVo, not `PagedInfraInput`.

### 3. Inconsistent Naming

**Symptom**: Routes don't match, API calls fail.

**Fix**: Follow naming conventions:
- Entity: PascalCase (`UserConfig`)
- API paths: kebab-case (`/user-config`)
- Frontend dirs: kebab-case (`api/system/user-config/`)

### 4. Missing Build Verification

**Symptom**: Errors discovered later in development.

**Fix**: Always run `dotnet build` after creating all files.

### 5. Skipping Documentation

**Symptom**: Team members don't know about new module.

**Fix**: Create documentation in `.docs/` directory.

## Debugging Tips

### Check Backend

1. Verify entity is registered in DbContext
2. Check service is registered in module
3. Verify API route with Swagger/OpenAPI
4. Check database table was created

### Check Frontend

1. Verify API functions are exported
2. Check component imports are correct
3. Verify permission codes in v-access directives
4. Check browser console for errors

### Check Menu

1. Verify menu seed data was executed
2. Check database for menu records
3. Verify user has required permissions
4. Check menu parent-child relationships

## Getting Help

If issues persist:
1. Check existing modules for reference (Config, Role, Dept in rbac module)
2. Review reference files in `references/` directory
3. Verify all checklist items are completed
4. Check git diff to see what changed
