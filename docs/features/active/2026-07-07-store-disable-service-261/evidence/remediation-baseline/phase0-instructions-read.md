# Phase 0 — Policy Instructions Read (Remediation Cycle 1)

- Timestamp: 2026-07-08T00-05
- Policy Order: CLAUDE.md -> general-code-change.md -> general-unit-test.md -> csharp.md (Policy Compliance Order)

## Files Read

1. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\CLAUDE.md` (full file)
2. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\general-code-change.md` (full file)
3. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\general-unit-test.md` (full file)
4. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\csharp.md` (full file)

## [P0-T1] CLAUDE.md — Key Clauses Recorded Verbatim

Coverage exemption (General Unit Test Policy §UT2, as embedded in CLAUDE.md):

> **COM/VSTO/WinForms coverage exemption (testable denominator).** The 80% floor applies to the
> **testable denominator** — production-only first-party code, after excluding:
> - (a) VSTO add-in lifecycle classes (entry points, ribbon event handlers, COM utility
>   registration) that cannot be unit-tested without a live Outlook process;
> - (b) WinForms form-derived classes and Designer-generated code;
> - (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`,
>   `ToDoModel`, and `Tags` that directly depend on `Microsoft.Office.Interop.Outlook.Application`,
>   `MailItem`, `Store`, or `MAPIFolder` without an injectable seam.
>
> These classes are formally exempted from the 80% floor. ... Testable seams within otherwise-COM-bound
> assemblies (e.g., `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, path/settings helpers) are
> explicitly NOT exempt and must meet the `>= 80%` floor.
>
> Any new modules, classes, or methods added must target `>= 90%` coverage.

File-size limit (General Code Change Policy §4, "Module & File Structure"):

> Keep modules **cohesive** — A module/file should have a clear purpose. Avoid dumping unrelated
> classes/functions into the same file. Do not exceed 500 lines for any one file.

## [P0-T2] general-code-change.md — Policy Order Confirmation

- Policy Order: CLAUDE.md (§ embedded) -> General Code Change Policy (this file) -> General Unit
  Test Policy -> C# Code Change Policy / C# Unit Test Policy (from CLAUDE.md), per Policy
  Compliance Order.
- Confirmed mandatory toolchain loop (format -> lint -> type-check -> test, restart on any
  failure/file-change) and 500-line file size limit apply to this remediation's test-file edits.

## [P0-T3] general-unit-test.md — Precedence Note

This repo-wide rule file states line coverage >= 85% / branch coverage >= 75% uniformly across
tiers T1-T4. Per the Policy Compliance Order (CLAUDE.md first, § "Policy Compliance Order"), this
remediation follows **CLAUDE.md's explicit COM/VSTO coverage-exemption thresholds** (80% testable
denominator floor / 90% new-code floor) instead of the generic 85%/75% figures in this file. This
is the established baseline for this feature per `spec.md` AC15 delivery annotations from the
original (non-remediation) implementation cycle, and CLAUDE.md is first in the reading/authority
order, so its coverage clause supersedes this file's generic figures for this feature's coverage
gate.

## [P0-T4] csharp.md — Toolchain Commands Confirmed

- Format: `dotnet tool run csharpier .` / `csharpier .`
- Lint: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Type-check: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- Test: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Order: format -> lint -> type-check -> test; restart from step 1 on any failure or file change.

This completes the explicit list of files read required by the Phase 0 contract.
