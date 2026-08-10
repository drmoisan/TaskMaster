---
name: project-512-toolchain-gate-fidelity-plan-seams
description: Plan seams for #512 C# toolchain gate fidelity — red PoshQC baseline, same-line grep hazard for the AC6 gate, and the exit-0 discriminating proof for the deprecated -EnableNullable switch
metadata:
  type: project
---

Planning seams discovered while writing `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/plan.2026-08-10T14-08.md` (2026-08-10).

**Why:** The feature corrects the documented C# toolchain (CSharpier v0 syntax; `/t:Build` warm builds that skip `CoreCompile`; `/p:Nullable=enable` that CI deliberately omits). Several obvious gate formulations are unsatisfiable or vacuous against this repository.

**How to apply:**

1. **A repo-wide grep gate for `Nullable=enable` is unsatisfiable as a bare-token search.** After the fix, the corrected sites *quote* the flag in prohibition prose (`Do not add /p:Nullable=enable`), and `scripts/vscode/Invoke-VSBuild.ps1` quotes it in the deprecation `Write-Warning`. The only workable AC6 gate is a **same-line conjunction** of `/t:Build` and `Nullable=enable`, plus an exclusion list for `docs/features/**`, `docs/research/**` and `.claude/agent-memory/**` (hundreds of historical hits) and an allowlist for the SD1 mirror tree. Corollary: the inserted rationale blocks must keep their line breaks, or the gate self-trips. See [[agent-memory-is-tracked-scope-git-gates]].

2. **The `-EnableNullable` no-op has a genuinely discriminating proof.** Run `Invoke-VSBuild.ps1 -Target Rebuild -EnableNullable -TreatWarningsAsErrors` and require `EXIT_CODE: 0`. If the switch still emitted `Nullable=enable`, the run would fail with the ~195 `CS86xx` population. This is stronger than asserting the absence of a string.

3. **PoshQC analyze is red at the merge base (16 findings, 3 in the file being edited).** Never write `EXIT_CODE: 0` acceptance for `run_poshqc_analyze`; use "table identical to the Phase 0 baseline". Do not rename `Get-MSBuildBuildArguments`/`Get-RequestedMSBuildProperties` (Pester references them) and do not add a plural-noun function or a `Write-Host`.

4. **Non-vacuity for MSBuild is a zero `Skipping target "CoreCompile"` count in an `/fl` log** — not a `csc.exe` count (zero at `verbosity=normal` even for genuine compiles) and not `CoreCompile:` header lines (they print when skipped). Error counts come from MSBuild's `N Error(s)` line; a naive grep doubles them.

5. **The CSharpier manifest is `dotnet-tools.json` at the repo root**, not `.config/dotnet-tools.json`.

6. **`/t:Rebuild` failures delete every project's `bin`/`obj`.** Any failing rebuild (the AC12 debt probe, the AC4 negative control) needs an ordered restorative rebuild task after it, or later steps have no assemblies.
