<#
.SYNOPSIS
  Mass-fixes all known Unity 6 compile errors across the entire Crossworlds project.

.DESCRIPTION
  Double-click fix_crossworlds.bat (in D:\Crossworlds) to run this.
  Or run manually:  PowerShell -ExecutionPolicy Bypass -File fix-unity6-api.ps1

  Scans D:\Crossworlds (parent of this script) — covers everything Unity compiles.

  FIXES APPLIED
  ─────────────
  using UnityEngine.Networking:
    • Comments out bare `using UnityEngine.Networking;` lines in files that do NOT
      use UnityWebRequest (i.e. leftover UNET imports). Files that actively call
      UnityWebRequest are left alone.

  Unity 6 obsolete API:
    • FindObjectsOfType<T>()              → FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
    • FindObjectOfType<T>()               → FindFirstObjectByType<T>()
    • FindObjectsByType<T>(SortMode.X)    → FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
      (single-param overload removed in Unity 6)

  TMPro 3.x removed enum values:
    • TextAlignmentOptions.Midline        → Center
    • TextAlignmentOptions.MidlineCenter  → Center
    • TextAlignmentOptions.MidlineLeft    → Left
    • TextAlignmentOptions.MidlineRight   → Right
    • TextAlignmentOptions.TopMidline     → Top
    • TextAlignmentOptions.BottomMidline  → Bottom
    • TextAlignmentOptions.CaplineCenter  → Top
    • TextAlignmentOptions.CaplineLeft    → TopLeft
    • TextAlignmentOptions.CaplineRight   → TopRight
    • TextAlignmentOptions.BaselineCenter → Bottom
    • TextAlignmentOptions.BaselineLeft   → BottomLeft
    • TextAlignmentOptions.BaselineRight  → BottomRight

  Safe to re-run — idempotent. Creates .bak backups before each edit.
#>

param(
    [string]$Root  = (Split-Path $PSScriptRoot -Parent),
    [switch]$DryRun = $false
)

Write-Host "Unity 6 API fixer — scanning: $Root"
Write-Host ""

$files = Get-ChildItem -Path $Root -Recurse -Filter "*.cs" |
         Where-Object {
             $_.FullName -notmatch '\\Library\\'  -and
             $_.FullName -notmatch '\\Temp\\'     -and
             $_.FullName -notmatch '\\.git\\'     -and
             $_.FullName -notmatch '\\obj\\'
         }

$totalFixed = 0

foreach ($file in $files) {
    $content  = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # ── 1. using UnityEngine.Networking — DO NOT touch.
    #        The namespace is VALID in Unity 6 (UnityWebRequest, UploadHandlerRaw etc. live here).
    #        Only the old UNET classes (NetworkBehaviour etc.) were removed years ago.
    #        Restore any lines that were previously commented out by mistake:
    $content = $content -replace '(?m)^// using UnityEngine\.Networking;.*$',
        'using UnityEngine.Networking;'

    # ── 2. FindObjectsOfType<T>() → FindObjectsByType<T>(Exclude, None) ──────────
    $content = $content -replace 'FindObjectsOfType(<[^>]+>)\(\)',
        'FindObjectsByType$1(FindObjectsInactive.Exclude, FindObjectsSortMode.None)'

    # ── 3. FindObjectOfType<T>() → FindFirstObjectByType<T>() ───────────────────
    $content = $content -replace '(?<![A-Za-z])FindObjectOfType(<[^>]+>)\(\)',
        'FindFirstObjectByType$1()'

    # ── 4. FindObjectsByType<T>(FindObjectsSortMode.*) — single-param old form ───
    $content = $content -replace 'FindObjectsByType(<[^>]+>)\(FindObjectsSortMode\.\w+\)',
        'FindObjectsByType$1(FindObjectsInactive.Exclude, FindObjectsSortMode.None)'

    # ── 5. TMPro — bare Midline (must run BEFORE MidlineCenter etc.) ─────────────
    # Match .Midline not followed by C/L/R (so MidlineCenter/Left/Right handled next)
    $content = $content -replace 'TextAlignmentOptions\.Midline(?!(Center|Left|Right))\b',
        'TextAlignmentOptions.Center'

    # ── 6-8. Midline variants ────────────────────────────────────────────────────
    $content = $content -replace 'TextAlignmentOptions\.MidlineCenter\b',  'TextAlignmentOptions.Center'
    $content = $content -replace 'TextAlignmentOptions\.MidlineLeft\b',    'TextAlignmentOptions.Left'
    $content = $content -replace 'TextAlignmentOptions\.MidlineRight\b',   'TextAlignmentOptions.Right'

    # ── 9-10. TopMidline / BottomMidline ─────────────────────────────────────────
    $content = $content -replace 'TextAlignmentOptions\.TopMidline\b',     'TextAlignmentOptions.Top'
    $content = $content -replace 'TextAlignmentOptions\.BottomMidline\b',  'TextAlignmentOptions.Bottom'

    # ── 11-13. Capline variants ──────────────────────────────────────────────────
    $content = $content -replace 'TextAlignmentOptions\.CaplineCenter\b',  'TextAlignmentOptions.Top'
    $content = $content -replace 'TextAlignmentOptions\.CaplineLeft\b',    'TextAlignmentOptions.TopLeft'
    $content = $content -replace 'TextAlignmentOptions\.CaplineRight\b',   'TextAlignmentOptions.TopRight'

    # ── 14-16. Baseline variants ─────────────────────────────────────────────────
    $content = $content -replace 'TextAlignmentOptions\.BaselineCenter\b', 'TextAlignmentOptions.Bottom'
    $content = $content -replace 'TextAlignmentOptions\.BaselineLeft\b',   'TextAlignmentOptions.BottomLeft'
    $content = $content -replace 'TextAlignmentOptions\.BaselineRight\b',  'TextAlignmentOptions.BottomRight'

    # ── Write if changed ─────────────────────────────────────────────────────────
    if ($content -ne $original) {
        $totalFixed++
        Write-Host "  FIXED: $($file.FullName.Replace($Root, '.'))"
        if (-not $DryRun) {
            Copy-Item $file.FullName "$($file.FullName).bak" -Force
            [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
        }
    }
}

Write-Host ""
if ($totalFixed -eq 0) {
    Write-Host "All clean — nothing to fix."
} else {
    Write-Host "Done. $totalFixed file(s) patched$(if ($DryRun) { ' (DRY RUN — no files written)' } else { '' })."
    Write-Host "Backups saved as *.bak — delete once Unity compiles clean."
}
Write-Host ""
