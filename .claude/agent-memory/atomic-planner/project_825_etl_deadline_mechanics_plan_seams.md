---
name: project-825-etl-deadline-mechanics-plan-seams
description: "#825 planning seams R1-R6 — latch ArmingBarrier encodes a FIXED timer order; `Task \"Csc\"` never names a project; a green vstest prints no Failed:/Skipped: line; the coverage runner appends .*\\.Test\\.dll$ at run time so the test dll is NOT in the denominator; a comment inserted ABOVE the line range it cites invalidates its own citation, in permanent source"
metadata:
  type: project
---

Authored 2026-09-09 for docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825.

**1. A latch-based arming barrier cannot survive a new upstream timer.**
`UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs` signals `TrySetResult` on every
`CreateTimer` (line 47) against a single TCS that `ReArm()` replaces. Consumers therefore encode a
FIXED timer ordering. `DfDeedleEtlTimeoutTests.cs:135` awaits signal 1 (column-add 3000 ms),
re-arms, releases gate A, awaits signal 2 (ETL hop 250 ms), advances 250. Putting a deadline under
an injected clock ANYWHERE EARLIER on the call chain inserts a new signal 1 and shifts everything;
the test then advances past nothing and hangs inside its `try`, so the `finally` that releases the
gates never runs. There is no implementation trick that avoids it: "deadline under the caller's
clock" *means* a timer on that clock, and `TimeProviderTaskExtensions.CreateCancellationTokenSource`
arms eagerly.

Determinism is only recoverable by gating each production step that arms a timer — one gate per
await/re-arm window. Consuming two signals in one window races: the second `TrySetResult` can land
on the already-set TCS before `ReArm()`, and the next await then blocks forever.

**How to apply:** before planning a `TimeProvider` thread-through, grep for
`ArmingBarrierTimeProvider` consumers and ask whether the new seam sits UPSTREAM of an existing
counted signal. Consumers that merely pass a never-advanced `FakeTimeProvider` (here
`DfDeedle_COM_Tests.cs:473`, `DfDeedleEtlTimeoutTests.cs:187`) are unaffected and in fact get more
deterministic. Only the signal-counting ones break.

**2. Two spec ACs were defective in opposite directions; both were caught by reading, not by a
validator.**

- **AC11 could not pass.** It demanded a zero-hit search of `OlTableExtensions.Etl.cs` for the
  null-forgiving `data!`. That literal appears TWICE — line 63 in the synchronous `ETL` method and
  line 131 in `EtlAsync` — and only the second is in scope. Replaced with a 2-to-1 occurrence-count
  transition plus an anchored diff naming the removed line. See
  [[acceptance-edits-must-be-false-before-true-after]].
- **AC22 could not fail.** It demanded that `new CancellationTokenSource(` with a NUMERIC argument
  return no hits in `OlTableExtensions_Tests.cs`. All five occurrences (965, 1005, 1284, 1328, 1686)
  were already no-argument before any change: the real 2000 ms source is built by the DEFAULT factory
  inside `TimeOutTask.cs:53`, not in the test file. Replaced with the substance — the named test at
  1646 now supplies a `FakeTimeProvider` — keeping the zero-hit search only as a corroborating guard
  explicitly labelled already-true.

**3. `DfDeedle.cs` has `#nullable enable` INSIDE the namespace (line 23), not at line 1.** A grep
for `#nullable` at file head misses it. Making `EtlAsync`'s tuple element nullable therefore puts
`tableSnapshot.Item1` at line 193 into flow analysis; plan the rename to the tuple's `data` name so
the guard at line 182 carries, rather than discovering CS8602 in the TreatWarningsAsErrors gate.

**Round 2 (2026-09-09) — three defects found in my own round-1 output.**

- **I gave the EXECUTOR the AC6/AC20 amendments.** Wrong agent; the orchestrator applied them during
  preparation instead (commit `945659cd`) and had me replace the three tasks with ONE read-only
  verification task. See [[acceptance-criteria-are-amended-by-planners-not-executors]].
- **A "spec is unamended-state" verification task placed in Phase 3 cannot assert the PRE-PLAN box
  state.** My first draft of that task asserted `unchecked == 35, checked == 0`, but Phase 1's P1-T4
  already checks off AC8, so it was unsatisfiable at its own position. Fix: assert the box-state
  INDEPENDENT inventory (`^- \[[ x]\] \*\*AC[0-9]+\*\*` == 35) plus a checked count expressed as a
  transition from Phase 1 (1 normally, 0 on the P1-T4 fallback branch).
- **Sibling-attribution error in `OlTableExtensions_Tests.cs`.** The single pre-change
  `new FakeTimeProvider()` at line 974 sits in
  `EtlAsync_WithBinaryAndObjectFieldsAndProgress_ReturnsTransformedData` (948-982), NOT in
  `GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot`. The latter is the test
  DECLARED at 1646, whose `InvokeAsyncResult` argument list is 1662-1677 and which is where AC22's
  provider is ADDED. Round 1 asserted "one of the two is inside" the 1646 test, which reads as
  attributing 974 to it. `typeof(TimeProvider)` occurs 0 times pre-change, so the count-4 assertion
  is a clean false-before/true-after.

**Also confirmed round 2:** `BuildExplorer` in `DfDeedleEtlTimeoutTests.cs` is declared at 72 and
called at 142, 190, 218 — a bare `BuildExplorer(` grep returns 4, so "three call sites" needs the
declaration carved out. That file's `.Should()` lines are 170, 203, 205, 224, 225, which makes a
zero-added/zero-removed `\.Should\(\)` anchored diff the machine-checkable form of AC6's
"adding no assertion and removing none" clause.

**Round 3 (2026-09-09) — seventeen executor-preflight defects, eight of them one class.**

- **`"/p:Platform=Any CPU"` is a SOLUTION platform name and cannot be passed to a `.csproj`.** Both
  `UtilitiesCS.csproj` (lines 9/22) and `UtilitiesCS.Test.csproj` (lines 11/47) default `$(Platform)`
  to `AnyCPU` and condition their Debug group on `Debug|AnyCPU`, so the spaced form matches no
  property group, leaves `OutputPath` unset, and fails before compiling. Project-scoped builds omit
  the property entirely; only `TaskMaster.sln` builds carry it.
- **A DELETION earlier in the plan makes a later "zero removed lines" diff gate unsatisfiable.** Six
  of the seventeen were this: an acceptance condition pinned to the PRE-CHANGE tree but running after
  an intervening phase moved or deleted the thing it cites. `250 * rowCount` and `tokenSource.Cancel`
  each existed twice (EtlAsync + EtlAsyncOld); deleting EtlAsyncOld forces "exactly one removed,
  zero added" instead of "zero removed". Line citations at `TableAccess.cs` 55/71/79/95/97/118,
  `DfDeedleEtlTimeoutTests.cs` 212 and `OlTableExtensions_Tests.cs` 1646 all move by the same plan's
  own earlier tasks. **How to apply:** for every citation, ask which earlier task last touched that
  file ABOVE that line.
- **The test assembly is in the coverage denominator.** `coverage.config` `ModulePaths` excludes only
  third-party modules, so `UtilitiesCS.Test.dll` is instrumented: a repository-wide `LinesValid`
  no-growth gate is falsified by ADDING tests. Gate the `UtilitiesCS` package element only. Same
  arithmetic breaks a deletion-only attribution: the total is deletions NET of additions, so the
  reconciliation needs an `AdditionAccounting:` term or its residual can never be zero.
- **`Invoke-MSTestWithCoverage.ps1` throws at 235-237 BEFORE the Cobertura post-processing at
  334-344.** One red test therefore leaves no processed XML at all — not a low number, no file. A
  plan that only handles non-termination has no branch for the common case.
- **113 columns > CSharpier's default 100.** There is no `.csharpierrc` anywhere in this tree, so any
  single-line assertion on a call the plan itself lengthens past 100 columns is true only until the
  mandatory `csharpier format .`. Assert the removed pre-change token plus a count instead.
- **A repo-wide `csharpier format .` runs AFTER mid-plan boundary gates.** An ownership-boundary
  check (`Console.WriteLine` survives byte-identical) checked off in Phase 3 must be RE-RUN inside the
  format task, or a reflow silently falsifies an already-checked criterion. Same reason a non-zero
  Phase-0 formatter baseline must HALT: the format would repair unrelated drift into the branch diff.
- **`git log $b..HEAD` is an empty range until some task commits.** A commit-message gate placed
  before the plan's only commit returns zero lines whatever the executor wrote.
- **A comment "immediately above the new parameter" falsifies a line-adjacency assertion**, and an
  asserted literal containing `<`/`>` is skipped by the plan-acceptance gate as a documented command
  shape. Put the comment above the METHOD and shorten the asserted token past its generics.
- **A fallback branch that refutes an AC must state the INCOMPLETE terminal accounting.** Leaving
  P1-T4's fallback silent left P9-T1/P9-T2 asserting 35-of-35 unconditionally — an internal
  contradiction. Thread the 34/1 counts through every downstream reconciliation task.
- **The plan file's own final check-off cannot be inside the commit it records.** Permit exactly that
  one residual by name, verified with `git diff --numstat HEAD -- <plan path>` = 1 insertion,
  1 deletion, 1 file.

**Round 4 (2026-09-09) — seven defects, six of them "the tool prints nothing on its success path".**

- **`Task "Csc"` can never be attributed to a project on one line.** MSBuild prefixes a task-start
  line with the project INSTANCE ID, not the project path, so a count of lines carrying both
  `Task "Csc"` and `UtilitiesCS.csproj` is 0 at every verbosity. The compilation IS observable on one
  line by the echoed csc command line, which carries `/out:obj\Debug\<Assembly>.dll`. Confirmed in
  the committed NORMAL-verbosity log
  `docs/features/active/2026-08-26-...-633/evidence/qa-gates/p7-t4-analyze.msbuild.txt` line 932
  (`/out:obj\Debug\VBFunctions.dll`); that same log carries ZERO `Task "Csc"` lines. Supersedes
  [[msbuild-task-csc-literal-needs-detailed-verbosity]] for the per-project attribution case.
- **A green vstest run prints NO `Failed:` and NO `Skipped:` line, and omits `Passed:` at zero.** It
  prints `Test Run Successful.`, `Total tests: N`, `Passed: N`, `Total time:` and nothing else. Six
  tasks demanded `TestsFailed: 0` / `TestsSkipped:` from lines that do not exist. Recorded verbatim
  in `docs/features/active/2026-07-09-...-287/evidence/baseline/full-test-run.md` as
  `Failed: 0 (omitted category)`. Fix: one plan-level decision stating the omitted counters are
  TRANSCRIBED as 0 with an annotation, corroborated by the printed header — not a per-task patch.
- **A repo-wide `csharpier format` restart trigger CANNOT be a porcelain-status delta.** Every file
  the formatter can rewrite at that point is already modified and already listed, and the Phase-0
  baseline excluded drift in everything still clean, so the two status SETS are identical by
  construction and the count is 0 whatever was rewritten. Use an anchored `git diff --numstat`
  before/after and compare the insertion/deletion FIGURES — status compares membership, numstat
  compares content. Add an `--intent-to-add` span or the plan's own new untracked test file is
  invisible to both numstat runs.
- **`.*\.Test\.dll$` is appended AT RUN TIME by `Invoke-MSTestWithCoverage.ps1` (lines 99-112).**
  This REVERSES round 3's finding above: `coverage.config` on disk excludes only third-party modules,
  but the runner adds the test-assembly pattern to the settings it derives, so `UtilitiesCS.Test.dll`
  is NOT instrumented. Every processed Cobertura under `docs/features/` carries production packages
  only. Never reason about the coverage denominator from `coverage.config` alone.
- **A file with only DELETIONS, or whose sole addition is a comment, has ZERO changed lines in
  Cobertura.** A changed-line-coverage gate demanding a decimal per file is 0/0 for those, and
  recording 0 pulls the aggregate below the floor by ARITHMETIC. Require `n/a` and exclude them from
  the sum's denominator.
- **MSBuild's file logger does not create intermediate directories: MSB1029.** A `/flp:logfile=`
  into `evidence/other/` fails outright if no earlier task wrote there. Check which task first
  creates each evidence sub-folder.
- **A branch the plan itself opens must carry alternative acceptance at the task that DOES the work,
  not only at the task that decides.** Round 3 closed the fallback at the decision point and the two
  terminal reconciliations; the implementing task still asserted a call site the fallback never
  creates. Sweep every task downstream of a branch, not just the accounting ones.

**Round 6 (2026-09-09) — two relayed defects, and two more of the second class found by sweeping it.**

- **A derivation rule stated at two readers of a data source and not at the third is a defect at the
  third.** P0-T9 and P8-T7 both carried the Cobertura per-filename derivation rule
  (`Get-CoberturaClassLineSummary`, Helpers.ps1 line 158, merge by line number resolving duplicates
  by MAXIMUM hits); P8-T9 read the same file and stated nothing. Set intersection makes the two
  repeated views harmless for a changed-line COUNT but not for a changed-line COVERED count, because
  the two views can disagree on hits. `GetTableInViewAsync` is `async`
  (`OlTableExtensions.TableAccess.cs:32`), so its state machine splits that filename across class
  elements — the aggregation case where they disagree. **How to apply:** when a rule is added at one
  task, grep for every other task reading the same artifact before closing the round.
- **A comment that cites a line range and is inserted ABOVE that range invalidates its own
  citation.** This is the sharp form of the stale-line-number class and it lands in PERMANENT SOURCE,
  not an artifact, so it survives the merge. Three instances in this one plan: P7-T1 cited
  `OlTableExtensions_Tests.cs` line 1646 (moved by P3-T9's three earlier binding sites and P4-T3's
  deletion at 984-1015); P6-T1's budget comment cited `OlTableExtensions.Etl.cs` lines 127-130 while
  inserting itself above line 84; P7-T4's reason comment cited `TimeOutTask_Tests.cs` lines 27-37 and
  40-50 while inserting itself between lines 9 and 10. The last two are self-invalidating with no
  other task involved. The relayed delta asserted P7-T1 was the only permanent-source case; the sweep
  falsified that. Fix shape: name the subject by method name or by its literal, add a zero-hit gate on
  the comment block, and keep the pre-change range in the plan's own prose as locating information.
- **Carve the zero-hit line-number gate to the file being edited.** A blanket `line [0-9]` gate at
  P6-T1 would have been unsatisfiable: that comment legitimately cites `TaskMaster/log4net.config`
  line 4, a file outside the Write Set that no task moves. `lines? [0-9]+-[0-9]+` matches the moving
  range and not the stable single-line citation. See [[zero-hit-grep-gates-need-carveouts]].

Related: [[dispatcher-repro-hang-trap]], [[project_791_hc_deadline_cancel_teardown_plan_seams]],
[[deletion-adjusted-coverage-no-regression-gate]],
[[acceptance-criteria-are-amended-by-planners-not-executors]].
