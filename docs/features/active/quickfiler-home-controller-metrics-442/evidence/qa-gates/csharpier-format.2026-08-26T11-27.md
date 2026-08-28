# Phase 6 — CSharpier Format (Scoped to the Seven Owned Files)

Timestamp: 2026-08-26T11-27
Task: [P6-T1]
Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier format "QuickFiler\Controllers\QfcHomeController.cs" "QuickFiler\Controllers\QfcHomeController.Metrics.cs" "QuickFiler\Controllers\EfcHomeController.cs" "QuickFiler\Controllers\EfcHomeController.Metrics.cs" "QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs" "QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs" "QuickFiler.Test\Controllers\EfcHomeControllerMetricsTests.cs"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

The mutating pass is scoped to the seven owned files by explicit path, so it cannot rewrite an
unowned file and break the ownership gates in Phase 7. Three sibling features are executing
concurrently against the same integration branch, which is why a directory-scoped or
repository-wide `format` was not used here. The repository-wide check in [P6-T2] is read-only and
therefore safe.

## Which files were rewritten

Determined by comparing each file's SHA-256 before and after the command, not by reading the tool's
processed-file count. CSharpier reports "Formatted 7 files" on every invocation because that counter
is the number of files it processed, not the number it changed; the counter is identical on a pass
that rewrites four files and on a pass that rewrites none, so it cannot answer this question.

### Pass 1 — four files rewritten

| Owned file | SHA-256 before | SHA-256 after | Rewritten |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | `A6AEAD1B…8857` | `D1A5C682…82BF` | **yes** |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | `1C1DC031…4BCD` | `F881B577…DE15` | **yes** |
| `QuickFiler/Controllers/EfcHomeController.cs` | `E608AD01…BF5B` | `E608AD01…BF5B` | no |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | `AB9F5720…D53B` | `E9F04746…D02A` | **yes** |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | `15566147…86E0` | `A81BC145…132C` | **yes** |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | `92643991…E8A5` | `92643991…E8A5` | no |
| `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | `ED88ADE3…EA82` | `ED88ADE3…EA82` | no |

Files rewritten in pass 1: **4**.

The two test files were already canonical because each had been formatted individually earlier, at
[P5-T15] and during the Phase 1 compaction. `EfcHomeController.cs` was unchanged because its edits
were single-line substitutions that left the surrounding layout canonical.

Because the formatter modified files, the phase preamble requires restarting Phase 6 from [P6-T1].
The remaining pass-1 steps were therefore not run; the phase restarted immediately.

### Pass 2 — zero files rewritten (final pass)

| Owned file | SHA-256 after pass 2 | Changed from pass 1 result |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | `D1A5C682…82BF` | no |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | `F881B577…DE15` | no |
| `QuickFiler/Controllers/EfcHomeController.cs` | `E608AD01…BF5B` | no |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | `E9F04746…D02A` | no |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | `A81BC145…132C` | no |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | `92643991…E8A5` | no |
| `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | `ED88ADE3…EA82` | no |

Files rewritten in the final pass: **0**. Every hash is byte-identical to its pass-1 result, so the
formatter reached a fixed point and [P6-T2] through [P6-T5] were run against this state.

## Output Summary

```
Formatted 7 files in 3299ms.
EXIT_CODE=0
```

EXIT_CODE 0 and zero files rewritten in the final pass, satisfying the acceptance condition and the
[P6-T9] loop-closure condition.
