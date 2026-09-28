---
name: reference-invoke-mstest-single-searchroot-defect
description: scripts/vscode/Invoke-MSTest.ps1 throws PropertyNotFoundException when -SearchRoot matches exactly one test assembly — always cite -SearchRoot . in plan tasks
metadata:
  type: reference
---

`scripts/vscode/Invoke-MSTest.ps1` cannot be invoked with a `-SearchRoot` that resolves to a **single** `*.Test.dll`. Lines 107-113 pipe discovery through `Select-Object -ExpandProperty FullName`, which yields a scalar `String` (not an array) for one match; lines 115 and 120 then evaluate `$testAssemblies.Count` under the `Set-StrictMode -Version Latest` set at line 77, which throws `PropertyNotFoundException` before `vstest.console.exe` is ever reached. Verified empirically 2026-08-04 during issue #418 planning.

**Why it matters for planning:** a plan task that cites `-SearchRoot <SingleProject>.Test` is unexecutable. Cite `-SearchRoot .` instead — the repo-wide form is proven (nine assemblies, 6112 tests) and has the side benefit of proving no regression across the other test assemblies. The one-line remedy is `@($testAssemblies).Count`, but fixing it drags the PowerShell/PoshQC toolchain plus a `artifacts/pester/powershell-coverage.xml` artifact into an otherwise C#-only feature's review gate, which is usually disproportionate — prefer filing it as a separate entry and noting it out of scope.

The sibling coverage runner [reference_invoke_mstest_with_coverage_script](reference_invoke_mstest_with_coverage_script.md) is the right citation when numeric coverage evidence is required. **The coverage runner does NOT share this defect** (verified 2026-09-12, #743 R1): `Invoke-MSTestWithCoverage.ps1` line 296 wraps discovery in `@(Get-ChildItem ...)`, so `-SearchRoot QuickFiler.Test` (single assembly) is accepted; `-SearchRoot` is joined to the repo root at line 272. Use the single-assembly form when the nine-assembly run would hit the local UtilitiesCS shell-icon stalls.
