<#
.SYNOPSIS
    项目代码质量检查脚本
.DESCRIPTION
    执行后端构建（默认跳过）、跨模块引用检测、层依赖方向校验、DDD构造块位置检查、AppService命名规范检查、前端类型检查。
    全部通过后输出 PASS；任意失败则以非零退出码退出。
.PARAMETER Root
    指定目标工作副本的根目录路径（即含 Yi.Abp、Yi.Vben5 的目录）。
    默认自动推导为脚本所在目录的上四级（当前工作副本根）。
    示例：-Root "E:\WCG\Private\Agent-Work\WorkTrees\master-branch"
.PARAMETER RunBuild
    显式启用 dotnet build 后端构建检查（默认跳过，传入此开关才执行）。
.EXAMPLE
    .\.claude\skills\quality-gate\scripts\quality-gate.ps1 -Root "E:\WCG\Private\-Agent-Work\master-branch"
    .\.claude\skills\quality-gate\scripts\quality-gate.ps1 -Root "." -RunBuild
    .\.claude\skills\quality-gate\scripts\quality-gate.ps1 -Root "." -SkipTypeCheck -SkipBackendLint
#>

param(
    [switch]$RunBuild,
    [switch]$SkipTypeCheck,
    [switch]$SkipBackendLint,
    [string]$Root = ""
)

$ErrorActionPreference = 'Stop'
if (-not $Root) {
    # 脚本位于 <worktree>\.claude\skills\quality-gate\scripts\ 下，向上 4 级到达 worktree 根
    $Root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
}
$Fail = $false

function Write-Pass([string]$msg) { Write-Host "  [PASS] $msg" -ForegroundColor Green }
function Write-Fail([string]$msg) { Write-Host "  [FAIL] $msg" -ForegroundColor Red; $script:Fail = $true }
function Write-Warn([string]$msg) { Write-Host "  [WARN] $msg" -ForegroundColor Yellow }
function Write-Step([string]$msg) { Write-Host "`n== $msg ==" -ForegroundColor Cyan }

# ---------------------------------------------------------------------------
# 1. 后端构建检查
# ---------------------------------------------------------------------------
Write-Step "1. 后端构建检查 (dotnet build)"
if ($RunBuild) {
    $backendSln = Join-Path $Root "Yi.Abp\Yi.Abp.slnx"
    if (Test-Path $backendSln) {
        $buildOut = dotnet build $backendSln --no-restore -v quiet 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Fail "dotnet build 失败"
            $buildOut | Where-Object { $_ -match "error" } | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        } else {
            Write-Pass "dotnet build 成功"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端 sln 文件，跳过构建检查" -ForegroundColor Yellow
    }
} else {
    Write-Host "  [SKIP] 后端构建检查已跳过（加 -RunBuild 开关可启用）" -ForegroundColor Yellow
}

# ---------------------------------------------------------------------------
# 2. 跨模块非法直接引用检测
#    规则：Domain/Application/SqlSugarCore 层禁止直接引用其他模块同层命名空间
# ---------------------------------------------------------------------------
Write-Step "2. 跨模块非法引用检测"

$backendSrc = Join-Path $Root "Yi.Abp\module"
if (Test-Path $backendSrc) {
    $modules = Get-ChildItem $backendSrc -Directory | Select-Object -ExpandProperty Name
    $illegalPatterns = @()

    foreach ($mod in $modules) {
        $modPath = Join-Path $backendSrc $mod
        $layers = @("Domain", "Application", "SqlSugarCore")
        foreach ($layer in $layers) {
            $layerPath = Join-Path $modPath "*.$layer"
            $csFiles = Get-ChildItem $layerPath -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
            foreach ($file in $csFiles) {
                $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                if (-not $content) { continue }
                foreach ($otherMod in $modules) {
                    if ($otherMod -eq $mod) { continue }
                    $illegalUsing = "using Yi\.Module\.$([regex]::Escape($otherMod))\.$([regex]::Escape($layer))"
                    if ($content -match $illegalUsing) {
                        $illegalPatterns += "  $($file.FullName) -> 非法引用 $otherMod.$layer"
                    }
                }
            }
        }
    }

    if ($illegalPatterns.Count -gt 0) {
        Write-Fail "发现跨模块非法直接引用 ($($illegalPatterns.Count) 处)："
        $illegalPatterns | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    } else {
        Write-Pass "未发现跨模块非法直接引用"
    }
} else {
    Write-Host "  [SKIP] 未找到后端模块目录，跳过跨模块检测" -ForegroundColor Yellow
}

# ---------------------------------------------------------------------------
# 2b. 层依赖方向检查
#     Domain 层不得引用 Application / SqlSugarCore 层
#     Application 层不得直接引用 SqlSugarCore 层
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2b. 层依赖方向检查"

    if (Test-Path $backendSrc) {
        $dirViolations = @()
        $dirModules = Get-ChildItem $backendSrc -Directory
        $dirRules = @(
            @{ SrcPattern = '\.Domain$';      ForbidSuffixes = @('Application', 'SqlSugarCore') },
            @{ SrcPattern = '\.Application$'; ForbidSuffixes = @('SqlSugarCore') }
        )

        foreach ($modDir in $dirModules) {
            foreach ($rule in $dirRules) {
                $layerDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                             Where-Object { $_.Name -match $rule.SrcPattern }
                foreach ($layerDir in $layerDirs) {
                    $csFiles = Get-ChildItem $layerDir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                    foreach ($file in $csFiles) {
                        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                        if (-not $content) { continue }
                        foreach ($forbid in $rule.ForbidSuffixes) {
                            if ($content -match "using Yi\.Module\.\w+\.$([regex]::Escape($forbid))\b") {
                                $layerLabel = ($rule.SrcPattern -replace '\\\.', '.') -replace '\$', ''
                                $dirViolations += "  [$layerLabel -> $forbid] $($file.FullName)"
                            }
                        }
                    }
                }
            }
        }

        if ($dirViolations.Count -gt 0) {
            Write-Fail "层依赖方向违规 ($($dirViolations.Count) 处)："
            $dirViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "层依赖方向正确"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2c. DDD 构造块位置规范检查
#     Manager（领域服务）不得出现在 Application 层 → 应在 Domain 层
#     Dto 不得出现在 Application.App / Application 的 Dtos 目录 → 应在 Application.Contracts
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2c. DDD 构造块位置规范检查"

    if (Test-Path $backendSrc) {
        $dddViolations = @()
        $dddModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $dddModules) {
            # Manager 类不得在 Application 层（精确匹配 .Application 结尾，排除 .Application.Contracts/.Application.App）
            $appOnlyDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                           Where-Object { $_.Name -match '\.Application$' }
            foreach ($appDir in $appOnlyDirs) {
                $managerFiles = Get-ChildItem $appDir.FullName -Recurse -Filter "*Manager.cs" -ErrorAction SilentlyContinue
                foreach ($f in $managerFiles) {
                    $fc = Get-Content $f.FullName -Raw -ErrorAction SilentlyContinue
                    if ($fc -match 'class \w+Manager\b') {
                        $dddViolations += "  [Manager@Application] $($f.FullName) -> 应移至 Domain/Managers/"
                    }
                }
            }

            # Dto 不得在 Application.App/Dtos 或 Application/Dtos（应在 Application.Contracts/Dtos）
            $wrongLayerDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                              Where-Object { $_.Name -match '\.(Application\.App|Application)$' }
            foreach ($wDir in $wrongLayerDirs) {
                $dtosPath = Join-Path $wDir.FullName "Dtos"
                if (Test-Path $dtosPath) {
                    $dtoFiles = Get-ChildItem $dtosPath -Filter "*Dto.cs" -ErrorAction SilentlyContinue
                    foreach ($f in $dtoFiles) {
                        $dddViolations += "  [Dto@$($wDir.Name)] $($f.FullName) -> 应移至 Application.Contracts/Dtos/"
                    }
                }
            }
        }

        if ($dddViolations.Count -gt 0) {
            Write-Fail "DDD 构造块位置违规 ($($dddViolations.Count) 处)："
            $dddViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "DDD 构造块位置规范正确"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2d. AppService 命名规范检查
#     Application 层中实现 I*AppService 接口的 public class 名必须以 AppService 结尾
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2d. AppService 命名规范检查"

    if (Test-Path $backendSrc) {
        $namingViolations = @()
        $namingModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $namingModules) {
            $appOnlyDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                           Where-Object { $_.Name -match '\.Application$' }
            foreach ($appDir in $appOnlyDirs) {
                $csFiles = Get-ChildItem $appDir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }
                    $classMatches = [regex]::Matches($content, 'public\s+class\s+(\w+)[^{]*I\w+AppService')
                    foreach ($m in $classMatches) {
                        $className = $m.Groups[1].Value
                        if ($className -notmatch 'AppService$') {
                            $namingViolations += "  [命名违规] $($file.FullName): '$className' 实现 AppService 接口但类名未以 AppService 结尾"
                        }
                    }
                }
            }
        }

        if ($namingViolations.Count -gt 0) {
            Write-Fail "AppService 命名违规 ($($namingViolations.Count) 处)："
            $namingViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "AppService 命名规范正确"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2e. Repository 接口位置检查
#     IXxxRepository 接口必须声明在 Domain 层
#     不得出现在 Application / Application.Contracts / SqlSugarCore 层
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2e. Repository 接口位置检查"

    if (Test-Path $backendSrc) {
        $repoIfaceViolations = @()
        $repoModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $repoModules) {
            $nonDomainDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                             Where-Object { $_.Name -notmatch '\.Domain$' }
            foreach ($dir in $nonDomainDirs) {
                $csFiles = Get-ChildItem $dir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }
                    if ($content -match 'interface I\w+Repository\b') {
                        $repoIfaceViolations += "  [Repository接口错位] $($file.FullName) -> 应移至 Domain/Repositories/"
                    }
                }
            }
        }

        if ($repoIfaceViolations.Count -gt 0) {
            Write-Fail "Repository 接口位置违规 ($($repoIfaceViolations.Count) 处)："
            $repoIfaceViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "Repository 接口位置正确"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2f. Repository 实现位置检查
#     Repository 具体实现类只能出现在 SqlSugarCore 层
#     Domain / Application 层不得包含非 abstract 的 Repository 实现类
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2f. Repository 实现位置检查"

    if (Test-Path $backendSrc) {
        $repoImplViolations = @()
        $repoImplModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $repoImplModules) {
            $wrongDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                         Where-Object { $_.Name -match '\.(Domain|Application)$' }
            foreach ($dir in $wrongDirs) {
                $csFiles = Get-ChildItem $dir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }
                    if ($content -match 'public\s+class\s+\w+Repository\b' -and
                        $content -notmatch 'public\s+abstract\s+class\s+\w+Repository\b') {
                        $repoImplViolations += "  [Repository实现错位] $($file.FullName) -> 实现类应移至 SqlSugarCore 层"
                    }
                }
            }
        }

        if ($repoImplViolations.Count -gt 0) {
            Write-Fail "Repository 实现位置违规 ($($repoImplViolations.Count) 处)："
            $repoImplViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "Repository 实现位置正确"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2g. Application 层直接 DB 操作检查
#     Application 层不得直接使用 ISqlSugarClient / ISugarQueryable / ISimpleClient
#     应通过 IXxxRepository 接口访问数据
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2g. Application 层直接 DB 操作检查"

    if (Test-Path $backendSrc) {
        $dbDirectViolations = @()
        $dbModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $dbModules) {
            $appDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                       Where-Object { $_.Name -match '\.Application$' }
            foreach ($appDir in $appDirs) {
                $csFiles = Get-ChildItem $appDir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }
                    if ($content -match 'ISqlSugarClient\b|ISugarQueryable<|ISimpleClient<|SqlSugarClient\b') {
                        $dbDirectViolations += "  [直接DB操作] $($file.FullName) -> 应通过 IXxxRepository 接口访问数据"
                    }
                }
            }
        }

        if ($dbDirectViolations.Count -gt 0) {
            Write-Fail "Application 层直接 DB 操作违规 ($($dbDirectViolations.Count) 处)："
            $dbDirectViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "Application 层未发现直接 DB 操作"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2h. Domain 层 ASP.NET Core 污染检查
#     Domain 层不得引用 Microsoft.AspNetCore 命名空间
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2h. Domain 层 ASP.NET Core 污染检查"

    if (Test-Path $backendSrc) {
        $aspnetViolations = @()
        $aspnetModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $aspnetModules) {
            $domainDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                          Where-Object { $_.Name -match '\.Domain$' }
            foreach ($dir in $domainDirs) {
                $csFiles = Get-ChildItem $dir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }
                    if ($content -match 'using Microsoft\.AspNetCore') {
                        $aspnetViolations += "  [AspNetCore污染] $($file.FullName)"
                    }
                }
            }
        }

        if ($aspnetViolations.Count -gt 0) {
            Write-Fail "Domain 层 AspNetCore 污染 ($($aspnetViolations.Count) 处)："
            $aspnetViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "Domain 层无 ASP.NET Core 污染"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2i. Entity 必须继承 ABP 实体基类
#     Domain/Entities 目录下的 public class 必须继承
#     Entity<T>、AggregateRoot<T> 或其审计变体
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2i. Entity ABP 基类继承检查"

    if (Test-Path $backendSrc) {
        $entityViolations = @()
        $entityModules = Get-ChildItem $backendSrc -Directory
        $abpEntityPattern = 'Entity<|AggregateRoot<|FullAuditedAggregateRoot<|CreationAuditedAggregateRoot<|AuditedAggregateRoot<|FullAuditedEntity<|CreationAuditedEntity<|AuditedEntity<|YiEntity<|YiAggregateRoot<'

        foreach ($modDir in $entityModules) {
            $domainDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                          Where-Object { $_.Name -match '\.Domain$' }
            foreach ($domainDir in $domainDirs) {
                $entitiesDir = Join-Path $domainDir.FullName "Entities"
                if (-not (Test-Path $entitiesDir)) { continue }
                $csFiles = Get-ChildItem $entitiesDir -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }
                    if ($content -match 'public\s+class\s+\w+' -and $content -notmatch $abpEntityPattern) {
                        $entityViolations += "  [Entity基类缺失] $($file.FullName) -> 应继承 AggregateRoot<T> 或 Entity<T> 等 ABP 基类"
                    }
                }
            }
        }

        if ($entityViolations.Count -gt 0) {
            Write-Fail "Entity 基类缺失 ($($entityViolations.Count) 处)："
            $entityViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "Entity 基类继承规范正确"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2j. AppService 接口继承规范检查
#     Application.Contracts 层的服务接口必须继承 IApplicationService 或框架等价接口
#     （IApplicationService / IYiXxxAppService / ICrudAppService 等）
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2j. AppService 接口继承规范检查"

    if (Test-Path $backendSrc) {
        $ifaceViolations = @()
        $ifaceModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $ifaceModules) {
            $contractsDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                             Where-Object { $_.Name -match '\.Application\.Contracts$' }
            foreach ($dir in $contractsDirs) {
                $csFiles = Get-ChildItem $dir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }
                    $ifaceMatches = [regex]::Matches($content, 'public\s+interface\s+(I\w+Service)\b')
                    foreach ($m in $ifaceMatches) {
                        $ifaceName = $m.Groups[1].Value
                        if ($content -notmatch 'IApplicationService|IYi\w+|ICrudAppService|IReadOnlyAppService') {
                            $ifaceViolations += "  [接口未继承服务基接口] $($file.FullName): '$ifaceName' 应继承 IApplicationService 或框架等价接口"
                        }
                    }
                }
            }
        }

        if ($ifaceViolations.Count -gt 0) {
            Write-Fail "AppService 接口继承违规 ($($ifaceViolations.Count) 处)："
            $ifaceViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "AppService 接口继承规范正确"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2k. Domain.Shared 反向引用检查
#     Domain.Shared 是最底层，不得引用 Domain / Application 任何层
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2k. Domain.Shared 反向引用检查"

    if (Test-Path $backendSrc) {
        $sharedViolations = @()
        $sharedModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $sharedModules) {
            $sharedDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                          Where-Object { $_.Name -match '\.Domain\.Shared$' }
            foreach ($sharedDir in $sharedDirs) {
                $csFiles = Get-ChildItem $sharedDir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }
                    if ($content -match 'using Yi\.Module\.\w+\.(Domain|Application)(?!\.Shared)\b') {
                        $sharedViolations += "  [Shared反向引用] $($file.FullName)"
                    }
                }
            }
        }

        if ($sharedViolations.Count -gt 0) {
            Write-Fail "Domain.Shared 反向引用违规 ($($sharedViolations.Count) 处)："
            $sharedViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "Domain.Shared 无反向引用"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2l. 每个项目必须有 AbpModule 类
#     每个 .csproj 所在目录必须有继承 AbpModule（或带 [DependsOn]）的 Module 类
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2l. 项目 AbpModule 类完整性检查"

    if (Test-Path $backendSrc) {
        $moduleViolations = @()
        $csprojFiles = Get-ChildItem $backendSrc -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue

        foreach ($csproj in $csprojFiles) {
            $projDir = $csproj.DirectoryName
            $csFiles = Get-ChildItem $projDir -Filter "*.cs" -ErrorAction SilentlyContinue
            $hasModule = $false
            foreach ($file in $csFiles) {
                $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                if ($content -match 'class \w+Module\b' -and ($content -match ':\s*AbpModule\b' -or $content -match '\[DependsOn')) {
                    $hasModule = $true
                    break
                }
            }
            if (-not $hasModule) {
                $moduleViolations += "  [缺少Module类] $($csproj.FullName)"
            }
        }

        if ($moduleViolations.Count -gt 0) {
            Write-Fail "缺少 AbpModule 类 ($($moduleViolations.Count) 个项目)："
            $moduleViolations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Pass "所有项目均有 AbpModule 类"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 2m. Service 方法必须在对应 IService 接口中声明（警告级别）
#     Application 层 Service 类的 public 非 override 方法
#     必须在其实现的 IXxxService 接口中显式声明
# ---------------------------------------------------------------------------
if (-not $SkipBackendLint) {
    Write-Step "2m. Service 方法接口声明检查 [WARN]"

    if (Test-Path $backendSrc) {
        $ifaceMethodWarnings = @()
        $svcModules = Get-ChildItem $backendSrc -Directory

        foreach ($modDir in $svcModules) {
            $appDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                       Where-Object { $_.Name -match '\.Application$' }
            foreach ($appDir in $appDirs) {
                $csFiles = Get-ChildItem $appDir.FullName -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
                foreach ($file in $csFiles) {
                    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                    if (-not $content) { continue }

                    # 提取类名
                    $classNameMatch = [regex]::Match($content, 'public\s+class\s+(\w+)')
                    if (-not $classNameMatch.Success) { continue }
                    $className = $classNameMatch.Groups[1].Value

                    # 提取类声明区域（到第一个 { 为止），找到实现的 IXxxService 接口
                    $classDeclMatch = [regex]::Match($content, 'public\s+class\s+[\s\S]*?(?=\{)')
                    if (-not $classDeclMatch.Success) { continue }
                    $classDecl = $classDeclMatch.Value

                    $implInterfaces = [regex]::Matches($classDecl, '\bI\w+Service\b') |
                                      ForEach-Object { $_.Value } | Select-Object -Unique
                    if ($implInterfaces.Count -eq 0) { continue }

                    # 查找 Application.Contracts 层中对应的接口文件
                    $contractsDirs = Get-ChildItem $modDir.FullName -Directory -ErrorAction SilentlyContinue |
                                     Where-Object { $_.Name -match '\.Application\.Contracts$' }

                    foreach ($iface in $implInterfaces) {
                        $ifaceFile = $null
                        foreach ($contractsDir in $contractsDirs) {
                            $found = Get-ChildItem $contractsDir.FullName -Recurse -Filter "$iface.cs" -ErrorAction SilentlyContinue |
                                     Select-Object -First 1
                            if ($found) { $ifaceFile = $found; break }
                        }
                        if (-not $ifaceFile) { continue }

                        $ifaceContent = Get-Content $ifaceFile.FullName -Raw -ErrorAction SilentlyContinue
                        if (-not $ifaceContent) { continue }

                        # 提取接口中直接声明的方法名（以 ; 结尾的方法声明行）
                        $ifaceMethodNames = @()
                        foreach ($line in ($ifaceContent -split "`n")) {
                            $t = $line.Trim()
                            if ($t.StartsWith('//') -or $t.StartsWith('*')) { continue }
                            if ($t -match '\b(\w+)\s*(?:<[^>]*>)?\s*\([^)]*\)\s*;') {
                                $ifaceMethodNames += $Matches[1]
                            }
                        }
                        $ifaceMethodNames = $ifaceMethodNames | Select-Object -Unique

                        # 提取 Service 中 public 非 override、非构造函数的方法名
                        $svcMethods = @()
                        foreach ($line in ($content -split "`n")) {
                            $t = $line.Trim()
                            if ($t -match '^public\s+' -and $t -notmatch '\boverride\b' -and $t -match '\(') {
                                if ($t -match 'public\s+.*\s+(\w+)\s*(?:<[^>]*>)?\s*\(') {
                                    $methodName = $Matches[1]
                                    if ($methodName -ne $className -and $methodName -notmatch '^(class|interface|base|this|where|new)$') {
                                        $svcMethods += $methodName
                                    }
                                }
                            }
                        }
                        $svcMethods = $svcMethods | Select-Object -Unique

                        foreach ($method in $svcMethods) {
                            if ($method -notin $ifaceMethodNames) {
                                $ifaceMethodWarnings += "  $($file.Name): '$method' 未在 $iface 中声明"
                            }
                        }
                    }
                }
            }
        }

        if ($ifaceMethodWarnings.Count -gt 0) {
            Write-Warn "Service 方法未在接口声明 ($($ifaceMethodWarnings.Count) 处)："
            $ifaceMethodWarnings | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
        } else {
            Write-Pass "Service 方法接口声明完整"
        }
    } else {
        Write-Host "  [SKIP] 未找到后端模块目录" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------
# 3. 前端类型检查
# ---------------------------------------------------------------------------
Write-Step "3. 前端类型检查 (pnpm check:type)"
if (-not $SkipTypeCheck) {
    $vben5Path = Join-Path $Root "Yi.Vben5"
    if (Test-Path (Join-Path $vben5Path "package.json")) {
        Push-Location $vben5Path
        try {
            $typeOut = pnpm check:type 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Fail "Vben5 check:type 失败"
                $typeOut | Select-Object -Last 20 | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
            } else {
                Write-Pass "Vben5 check:type 通过"
            }
        } finally {
            Pop-Location
        }
    } else {
        Write-Host "  [SKIP] 未找到 Yi.Vben5，跳过类型检查" -ForegroundColor Yellow
    }
} else {
    Write-Host "  [SKIP] 前端类型检查已跳过（移除 -SkipTypeCheck 可启用）" -ForegroundColor Yellow
}

# ---------------------------------------------------------------------------
# 汇总
# ---------------------------------------------------------------------------
Write-Host ""
if ($Fail) {
    Write-Host "== 代码质量检查结束：FAIL ==" -ForegroundColor Red
    exit 1
} else {
    Write-Host "== 代码质量检查结束：PASS ==" -ForegroundColor Green
    exit 0
}
