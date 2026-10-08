#requires -Version 5.1
<#
  Story of Ellen - isolated Level 3 bake.
  Rebuilds Assets/Scenes/ThreeScene.unity from committed Level 3 design code.
  Preserves OneScene/TwoScene and never resets unrelated local changes.
#>
param(
    [switch]$Publish
)

$ErrorActionPreference = "Stop"
$project = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$sceneRelative = "Assets/Scenes/ThreeScene.unity"
$scenePath = Join-Path $project $sceneRelative
$versionPath = Join-Path $project "ProjectSettings/ProjectVersion.txt"
$backupFolder = Join-Path (Split-Path $project -Parent) "Ellen-ThreeScene-Backup"
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"

function Invoke-Git {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)
    & $script:GitExe -C $project @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Git command failed ($LASTEXITCODE): git $($Arguments -join ' ')"
    }
}

# Fail closed if Unity can still be holding the project's Library or scene.
if (Get-Process -Name Unity -ErrorAction SilentlyContinue) {
    throw "Unity Editor is open. Save the scene, close the Editor, and retry."
}

$GitExe = $null
$gitCommand = Get-Command git.exe -ErrorAction SilentlyContinue
if ($gitCommand) { $GitExe = $gitCommand.Source }
if (-not $GitExe) {
    foreach ($candidate in @(
        "$env:ProgramFiles\Git\cmd\git.exe",
        "$env:LOCALAPPDATA\Programs\Git\cmd\git.exe"
    )) {
        if (Test-Path $candidate) { $GitExe = $candidate; break }
    }
}
if (-not $GitExe) { throw "Git for Windows is not installed or cannot be found." }

$branch = (& $GitExe -C $project branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne "main") {
    throw "Switch to the main branch before baking Level 3 (current: $branch)."
}

$origin = (& $GitExe -C $project remote get-url origin).Trim()
if ($LASTEXITCODE -ne 0 -or $origin -notmatch "JustVolly/Story-of-Ellen") {
    throw "Unexpected Git remote: $origin"
}

# Update code without touching uncommitted level scene modifications.
Invoke-Git -Arguments @("fetch", "origin", "--prune")
Invoke-Git -Arguments @("merge", "--ff-only", "origin/main")

$versionText = Get-Content -LiteralPath $versionPath -Raw
$match = [regex]::Match($versionText, '(?m)^m_EditorVersion:\s*(\S+)')
if (-not $match.Success) { throw "Unity Editor version not found in ProjectVersion.txt." }
$unityVersion = $match.Groups[1].Value

$editorCandidates = @(
    "D:\Unity\Hub\Editor\$unityVersion\Editor\Unity.exe",
    "$env:ProgramFiles\Unity\Hub\Editor\$unityVersion\Editor\Unity.exe",
    "$env:ProgramFiles\Unity Hub\Editor\$unityVersion\Editor\Unity.exe"
)
$editor = $editorCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $editor) {
    throw "Unity $unityVersion not found. Install the matching Editor from Unity Hub and retry."
}

New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null
$backup = Join-Path $backupFolder "ThreeScene-before-$stamp.unity"
$log = Join-Path $backupFolder "ThreeScene-build-$stamp.log"
Copy-Item -LiteralPath $scenePath -Destination $backup -Force
Write-Host "ThreeScene backup: $backup"

Write-Host "Baking Astral Crypt in Unity $unityVersion..."
& $editor -batchmode -quit -projectPath $project -executeMethod EllenLevelThreeBuilder.BuildLevelThree -logFile $log

if ($LASTEXITCODE -ne 0) {
    throw "Unity exited with code $LASTEXITCODE. Check $log. Backup: $backup"
}

if (-not (Test-Path -LiteralPath $log)) {
    throw "Unity did not write a build log. Cannot verify scene bake."
}
$logText = Get-Content -LiteralPath $log -Raw
if ($logText -notmatch '\[Ellen Level 3\] Astral Crypt saved to') {
    throw "Unity did not confirm a successful Level 3 bake. Check $log."
}
Write-Host "Level 3 scene baked and saved."

Invoke-Git -Arguments @("status", "--short", "--branch")
Invoke-Git -Arguments @("diff", "--stat", "--", $sceneRelative)

if ($Publish) {
    # Opt-in publication: stage ONLY the Level 3 Unity scene.
    Invoke-Git -Arguments @("add", "--", $sceneRelative)
    & $GitExe -C $project diff --cached --quiet -- $sceneRelative
    if ($LASTEXITCODE -eq 0) {
        Write-Host "ThreeScene has no staged changes; nothing to publish."
    } elseif ($LASTEXITCODE -eq 1) {
        Invoke-Git -Arguments @("commit", "--only", "-m", "feat(level3): bake Astral Crypt into ThreeScene", "--", $sceneRelative)
        Invoke-Git -Arguments @("push", "origin", "main")
        Write-Host "Level 3 committed and pushed."
    } else {
        throw "Unable to inspect staged scene diff."
    }
} else {
    Write-Host "Build complete. Open ThreeScene in Unity and run a Play Mode"
    Write-Host "smoke test before committing/publishing the scene."
}
Write-Host "Build log: $log"
Write-Host "Backup: $backup"
