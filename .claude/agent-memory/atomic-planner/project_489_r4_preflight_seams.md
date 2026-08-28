---
name: project-489-r4-preflight-seams
description: Round-4 preflight seams for issue #489 (itemviewer-surface-defects) — named-pin baselines, IDE0005 impossibility, .gitignore vs .log evidence, full C# project pathspec
metadata:
  type: project
---

Correction seams landed in the round-4 revision of
`docs/features/active/itemviewer-surface-defects-489/plan.2026-08-25T01-04.md`.

**Why:** three consecutive rounds produced "residual" defects — a fix applied at the site the round
named while the same fact survived elsewhere. Each item below is a fact class, not a single site.

**How to apply:** when planning against a co-owned C# test assembly, or any plan that writes msbuild
logs or scope-lock porcelain, check all four classes before handing off.

1. **A per-CLASS baseline does not cover a per-TEST assertion.** `BaselinePerClass:` relativizes a
   class's failed count; a task that asserts "2 passed" over two *named* pre-existing tests still
   needs a `BaselineNamedPins:` block giving `passed`/`failed` per named test. Sweep every task that
   states an absolute pass/fail/skip count and ask whether this feature *created* the test. Include
   unfiltered full-assembly runs: an `EXIT_CODE: 0` over a co-owned assembly is the same defect
   wearing different clothes. See [[expect-fail-needs-a-synchronous-seam]].

2. **`IDE0005` cannot be emitted in this repository.** `QuickFiler/QuickFiler.csproj` wires only
   Meziantou.Analyzer, Roslynator.Analyzers, AsyncFixer, Microsoft.CodeAnalysis.BannedApiAnalyzers
   and SonarAnalyzer.CSharp; there is no `.globalconfig`; `.editorconfig` sets no `IDE0005`
   severity; and `CS8019` is hidden severity, which `/p:TreatWarningsAsErrors=true` does not
   promote. Any plan clause conditioned on "if the analyzer reports the `using` as unused" is dead,
   and — worse — a conditional `using` removal above a line the plan pins by number silently breaks
   that pin.

3. **`.gitignore:84` is `*.log`.** An msbuild `/flp:LogFile=…​.log` under `<FEATURE>/evidence/` can
   never be committed, so the audit trail loses it while every porcelain gate still reports clean.
   Write `.msbuild.txt`. `*.md` and `*.trx` are not ignored; `coverage/*` is, deliberately.

4. **A scope-lock pathspec must enumerate all 18 C# project directories plus `TaskVisualizer/`.**
   `QuickFiler/ QuickFiler.Test/ UtilitiesCS/` covers 3 of 18 and cannot observe a csharpier rewrite
   or a stray edit in the other fifteen. Distinguish scope-lock/clean-tree gates (full set) from
   targeted presence/absence checks (one directory), and say which is which in the plan's execution
   conventions. See [[agent-memory-is-tracked-scope-git-gates]].

5. **`ExpectedExitCode:` is not just for `[expect-fail]`.** `csharpier check .` exits 1 on any
   unformatted file, `vstest.console.exe` exits non-zero on any failed test, and
   `Invoke-MSTestWithCoverage.ps1` throws both on a failing test and on a sub-80% line rate. Each of
   those is a passing outcome under a relative gate, and without `ExpectedExitCode:` the artifact
   normalizes to `fail`. The field is per-FILE, so each such gate needs its own artifact.
