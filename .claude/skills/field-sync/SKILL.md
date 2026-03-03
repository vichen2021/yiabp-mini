---
name: "field-sync"
description: "Synchronizes entity field changes across DTOs, services, frontend, and dictionary seeds. Invoke when adding/removing fields from entities."
---

# Field Sync

Synchronizes entity field changes across the entire codebase including DTOs, Service implementations, frontend API and views, and dictionary seed data.

## When to Use

- When adding new fields to an entity
- When removing fields from an entity
- When changing field types (e.g., int to bool, adding enum)
- When renaming entity fields

## Workflow

### Step 1: Understand the Changes

Ask the user to clarify:
- Which entity is being modified?
- What fields are being added/removed/changed?
- What are the new field types?
- Any new enums that need to be created?

### Step 2: Update Backend DTOs

#### 2.1 Find DTO Location

DTOs are located in: `Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/Dtos/{EntityName}/`

#### 2.2 Update 5 DTO Files

For each of the following files, add/remove/update the corresponding field:

1. `{EntityName}GetOutputDto.cs` - Single entity retrieval output
2. `{EntityName}GetListOutputDto.cs` - List query output
3. `{EntityName}CreateInputVo.cs` - Creation input
4. `{EntityName}UpdateInputVo.cs` - Update input
5. `{EntityName}GetListInputVo.cs` - Query parameters (remove if field no longer needed for filtering)

**Type Mapping:**
- C# `bool` → TypeScript `boolean`
- C# `int` → TypeScript `number`
- C# `Guid` → TypeScript `string`
- C# `Guid?` → TypeScript `string | null`
- C# Enum → TypeScript `number` (use enum name in imports)
- New Enum → Add import: `import type { EnumName } from '{module-path}/enums';`

### Step 3: Update Service Implementation

Location: `Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application/Services/{EntityName}Service.cs`

#### 3.1 Update GetListAsync Method

- Update `WhereIF` conditions in the query
- Update `Select` mapping to include new fields or remove old fields

### Step 4: Update Frontend API Model

Location: `Yi.Vben5/apps/web-antd/src/api/{module-name}/{entity-name}/model.d.ts`

Update the TypeScript interface to match the DTO changes.

### Step 5: Update Frontend View Data

Location: `Yi.Vben5/apps/web-antd/src/views/{module-name}/{entity-name}/data.ts`

#### 5.1 Update columns (for table display)

Add or remove column definitions:
```typescript
{
  field: 'fieldName',
  title: 'Display Name',
  minWidth: 100,
  formatter({ cellValue }) {
    // For booleans
    return cellValue ? 'Yes' : 'No';
    // For enums
    return cellValue === 1 ? 'Value1' : 'Value0';
  },
}
```

#### 5.2 Update drawerSchema (for create/edit form)

Add or remove form field definitions:
```typescript
// Boolean field
{
  component: 'Switch',  // or 'Select' with options
  fieldName: 'fieldName',
  label: 'Display Name',
}

// Enum field
{
  component: 'Select',
  componentProps: {
    options: [
      { label: 'Option 1', value: 0 },
      { label: 'Option 2', value: 1 },
    ],
  },
  fieldName: 'fieldName',
  label: 'Display Name',
}

// Foreign key field (Guid)
{
  component: 'Input',  // or ApiSelect for dropdown
  fieldName: 'fieldName',
  label: 'Display Name',
}
```

#### 5.3 Update querySchema (for search filters)

Add or remove query conditions.

### Step 6: Add Dictionary Seed Data (If Needed)

If the field is a boolean or enum used in dropdowns, add dictionary data.

#### 6.1 Update DictionaryTypeDataSeed.cs

Location: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryTypeDataSeed.cs`

Add new dictionary type:
```csharp
DictionaryTypeAggregateRoot dictNew = new DictionaryTypeAggregateRoot()
{
    DictName = "Dictionary Type Name",
    DictType = "dict_type_key",
    OrderNum = 100,
    Remark = "Description",
    IsDeleted = false,
    State = true
};
entities.Add(dictNew);
```

#### 6.2 Update DictionaryDataSeed.cs

Location: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryDataSeed.cs`

Add dictionary data entries:
```csharp
DictionaryEntity dictData1 = new DictionaryEntity()
{
    DictLabel = "Label 1",
    DictValue = "Value1",
    DictType = "dict_type_key",
    OrderNum = 100,
    Remark = "Description",
    IsDeleted = false,
    State = true,
    ListClass = "default"  // success, danger, warning, info, primary, default
};
entities.Add(dictData1);

DictionaryEntity dictData2 = new DictionaryEntity()
{
    DictLabel = "Label 2",
    DictValue = "Value2",
    DictType = "dict_type_key",
    OrderNum = 99,
    Remark = "Description",
    IsDeleted = false,
    State = true,
    ListClass = "primary"
};
entities.Add(dictData2);
```

### Step 7: Verification

Run build and typecheck to verify changes:

```powershell
# Backend
dotnet build Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application/Yi.Framework.{ModuleName}.Application.csproj

# Frontend
cd Yi.Vben5/apps/web-antd
npm run typecheck
```

## Example: Adding ParseApiId to Player Entity

This example shows the actual changes made for adding `ParseApiId` (Guid) and changing `IsServerParser` (bool) to `ParserType` (ParserTypeEnum) to the Player entity.

### Backend Changes:

**PlayerGetOutputDto.cs:**
- Add: `public Guid? ParseApiId { get; set; }`
- Change: `public bool IsParse { get; set; }` (was int)
- Change: `public ParserTypeEnum ParserType { get; set; }` (was bool IsServerParser)
- Add import: `using Yi.Framework.Video.Domain.Shared.Enums;`

**PlayerService.cs:**
- Change WhereIF: `input.IsParse.HasValue` (was `input.IsServerParser`)
- Add to Select: `ParseApiId = x.ParseApiId`
- Change Select: `ParserType = x.ParserType` (was `IsServerParser = x.IsServerParser`)

### Frontend Changes:

**api/video/player/model.d.ts:**
- Add: `parseApiId?: string | null;`
- Change: `isParse: boolean` (was `isParse: number`)
- Change: `parserType: number` (was `isServerParser: boolean`)

**views/video/player/data.ts:**
- Change isParse formatter: `cellValue ? '是' : '否'` (was `cellValue === 1`)
- Change isParse component: `Switch` with `defaultValue: false` (was `Select` with options)
- Add ParseApiId field in drawerSchema
- Change isServerParser to parserType: Select with options [{label: '客户端解析', value: 0}, {label: '服务端解析', value: 1}]

### Dictionary Seeds (Not needed for this example):

No dictionary seeds were added as the parserType uses a backend enum directly.
