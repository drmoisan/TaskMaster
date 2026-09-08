---
name: project-810-teardown-dropdown-residuals-plan-seams
description: Preflight revision seams for issue #810 (QuickFiler teardown and dropdown residuals) — inherited-path subtraction, TokenSource fail-before unreachability, baseline-keyed exit codes, root dotnet-tools.json, unmeasured csharpier delta, 490-line headroom threshold
metadata:
  type: project
---

Round-1 preflight on the #810 atomic plan returned eight defects. The six that generalise:

**`OUT-OF-WRITE-SET: NONE` anchored on `origin/main` was unsatisfiable.** The branch's own
preparation commits had already added `.claude/agent-memory/task-researcher/MEMORY.md`, a
per-issue researcher memory file, and `<FEATURE>/research/research.*.md` ahead of the base
commit. Every `origin/main`-anchored diff reports them on a tree where no task has run.
**Why:** the base commit is not the branch head; the gate compared a superset against the
Write Set. **How to apply:** declare an inherited-paths list in the Write Set and evaluate
`OUT-OF-WRITE-SET` over `CHANGED-PATHS` minus `INHERITED-PATHS`, with an explicit
`INHERITED-PATHS:` field so the subtraction is auditable rather than a waiver. Generalises
[[agent-memory-is-tracked-scope-git-gates]].

**A "null after Cleanup" assertion on `QfcHomeController.TokenSource` is null before too.**
The two-argument ctor at `QuickFiler/Controllers/QfcHomeController.cs:28-33` assigns only
`Globals` and `ParentCleanup`; `TokenSource` at `:470-473` is a plain getter over
`_tokenSource` (`:469`), written only by `CreateCancellationToken()` (`:463-467`) and the
init path. **Why:** the test passed against unmodified production code, so the phase's
`FAILED: 2` fail-before was unreachable. **How to apply:** inject with
`SetPrivateField(controller, "_tokenSource", new CancellationTokenSource())` (the helper is
already at `QfcHomeControllerCleanupTests.cs:29`) AND assert non-null before the act step.
`Cleanup()` (`:371-406`) wraps `_datamodel?.Cleanup(); _tokenSource?.Dispose();` in one
`try`, so an uninjected `_datamodel` is harmless.

**`EXIT_CODE: 0` beside a baseline-failing-set comparison is self-contradictory.** Two tasks
pinned `EXIT_CODE: 0` and `POST-FAILED-TESTS: 0` while also establishing `NEWLY-FAILING: NONE`
against a baseline set the baseline task deliberately left unpinned. **How to apply:** record
the observed integer and set `ExpectedExitCode:` to 0 when the baseline failing count is 0 and
1 otherwise; add a `POST-FAILED-SET:` and require subset containment. Related:
[[project_647_fileio2_retry_plan_seams]].

**There is no `.config/dotnet-tools.json` in this checkout.** The only manifest is the
repository-root `dotnet-tools.json`, pinning `tools.csharpier.version` `1.2.6` with the single
command name `csharpier`. **How to apply:** cite the root path, and read the version with the
Read tool from the manifest rather than asserting over an unobserved `--version` probe.

**`.csharpierignore` excludes neither `coverage/` nor `*.config`.** Its full set is
`**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`,
`*.props`, `*.targets`. A plan that writes a coverage settings file *after* the csharpier
baseline cannot pin `CHECKED-FILES-DELTA` to an exact integer. **How to apply:** give the
delta a bounded range plus a `DELTA-COMPOSITION:` block that records which outcome occurred.

**`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` is exactly 490 lines.** A
`greater than 490` headroom threshold silently omits it, and the plan grows it by three lines
(the rewritten `RunTeardownStage("park-focus", ...)` call reaches ~114 columns at 16-space
indentation, so CSharpier splits it into a four-line argument list). **How to apply:** use an
`at least 480` threshold for headroom records on files a task grows. Related:
[[project_680_menu_mode_plan_seams]].

Also: an evidence filename that hardcodes a future timestamp (`issue-810.2026-09-08T00-00.md`)
predicts its own write time. Use `issue-810.<TS>.md` read from the clock at write time, and
gate on "exactly one file matching the glob, whose filename timestamp equals its own
`Timestamp:` field".
