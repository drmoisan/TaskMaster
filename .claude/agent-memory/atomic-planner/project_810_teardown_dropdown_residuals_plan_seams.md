---
name: project-810-teardown-dropdown-residuals-plan-seams
description: Preflight revision seams for issue #810 (QuickFiler teardown and dropdown residuals) R1+R2 — inherited paths must be a RULE not a list, no upper bound on a csharpier checked-file delta, loop-closure must define clean when a step expects non-zero, stranded using System.Linq, TokenSource fail-before unreachability, baseline-keyed exit codes
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
The two-argument ctor at `QuickFiler/Controllers/QfcHomeController.cs:29-33` (line 28 is blank;
the signature is at `:29`) assigns only
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

## Round 2 — four defects, all against the round-1 fixes

**The round-1 inherited-paths LIST was itself the next defect.** It was short by two
(`.claude/agent-memory/atomic-planner/MEMORY.md` and the per-issue planner seams file — the
planner's own memory writes land on the branch it is planning), and it fails a second way that
no amount of enumeration fixes: the executing agent writes ITS persistent memory into the same
tracked `.claude/agent-memory/` subtree DURING execution, producing paths no list authored at
plan time can name. **Why:** an enumeration is a snapshot of a tree that keeps moving.
**How to apply:** express inherited paths as a two-clause RULE, not a list. Clause A =
"already changed relative to the base ref before the first task ran", captured mechanically by
a Phase 0 task into an `INHERITED-CLAUSE-A:` block (`git diff --name-only` UNION
`git status --porcelain --untracked-files=all`, because a name-listing diff cannot see an
untracked path). Clause B = a path prefix, here `.claude/agent-memory/`. The scope gate
subtracts the CAPTURED set and the prefix, never the authored list, and subtracts neither from
the Write Set so a Write Set path that moved unexpectedly is still reported. Supersedes the
round-1 advice above. Generalises [[agent-memory-is-tracked-scope-git-gates]].

**Never gate an UPPER bound on a CSharpier `Checked N files` delta.** Round 1 bounded it to
2..3. The only comparable observation in this repo contradicts that: on the immediately
preceding feature (#796) the same repo-wide command moved 1601 -> 1608
(`<796>/evidence/baseline/p0-t7-csharpier-check-baseline.md:16` and
`<796>/evidence/qa-gates/p9-t2-csharpier-check.md:25`), a delta of 7 on a branch contributing
three countable files. Four of the seven were never attributed. **Why:** the counter is
repo-wide and is not confined to the files the plan creates. **How to apply:** gate the LOWER
bound only (one per new countable source file — that is the wiring-sensitive part), and record
the remainder as `DELTA-ATTRIBUTION: COMPLETE` or `RESIDUAL: <n>`, an observation rather than a
failure. Related: [[csharpier-formatted-n-is-processed-count]].

**A loop-closure task must DEFINE "clean" when any step declares a non-zero
`ExpectedExitCode`.** The final coverage step's exit code is keyed to the baseline failing set,
so a legitimate run records `EXIT_CODE: 1`, while the closure task demanded `LOOP: CLEAN PASS`
and the plan's fail-closed evidence rule pushes anything not reading clean to BLOCKED.
**How to apply:** state that clean means every step met its OWN declared expectation, add one
`EXPECTATION-MET:` line per step, and write `LOOP: CLEAN PASS` only when all read `YES`. This
matters most when the closure artifact is the sole cited evidence for a toolchain AC.
Related: [[two-run-gates-need-a-measured-vs-confirming-split]].

**Deleting the last consumer of a LINQ expression strands `using System.Linq;`.** Extracting
`_breadcrumbPopupOwners.Values.Any(...)` out of `QuickFiler/Viewers/QfcFormViewer.cs:246` left
the directive at `:7` with no consumer, and the final analyzer gate is capped at the Phase 0
baseline, so a newly reported unnecessary-using diagnostic would fail a gate five phases after
the edit that caused it. **How to apply:** when a task removes the only call site of an
extension method, grep the whole file for other consumers of that namespace (extension-method
syntax AND query syntax AND `Enumerable.`), remove the directive in the SAME task, gate it with
a file-scoped zero-hit grep, and add the token to the plan's disappear list with its pathspec —
the same directive is present in 17 other files under `QuickFiler/Viewers` alone, so an
unscoped assertion would be false. Related: [[zero-hit-grep-gates-need-carveouts]].
