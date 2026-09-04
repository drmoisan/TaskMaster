# Policy Audit — efc-archiveroot-boundary-sink-defects (Issue #736)

- Component: `QuickFiler.Controllers.EfcFormController`, `QuickFiler.Controllers.EfcDataModel`, `TaskMaster.AppOlObjects`
- Date: 2026-09-04
- Work Mode: `full-bug` (AC source: `spec.md` only)
- Branch: `bug/efc-archiveroot-boundary-sink-defects-736`
- Head: `54da9e4d`
- Base: `origin/main` = `66749143`
- Merge base (recomputed by this review): `66749143601aedb816c679b911f1042ffa3e86a5` — identical to `origin/main`, so `origin/main...HEAD` is the full branch set (8 commits).
- Audit scope: the **full branch diff against the resolved base**, 98 changed paths.

## Template provenance deviation

The `policy-audit-template-usage` skill requires resolving the template through
`mcp__drm-copilot__resolve_policy_audit_template_asset`. That MCP server is not exposed in this
session, and `mcp__drm-copilot__validate_orchestration_artifacts` is likewise unavailable. Per the
established handling for this condition, this artifact is hand-authored preserving all twelve
canonical major headings rather than marked wholly BLOCKED. The MCP validation step in the skill is
recorded as **UNVERIFIED — MCP server not exposed to this agent**; every other step was performed.

## Rejected Scope Narrowing

No instruction in the caller prompt narrowed the audit to a plan, task, phase, or file subset, and
no language with changed files was declared out of scope. Two caller statements were evaluated and
are recorded here for completeness:

1. Verbatim: *"`.claude/agent-memory/**` paths on this branch are agent memory written by this run's
   agents. They are outside the ratified Write Set by design and outside AC11's pathspec, and are
   accounted for in the P7-T2 evidence artifact."*
   Disposition: **framing not adopted for the AC11 verdict.** The statement is factually accurate —
   the plan's D11 pathspec does carry `":(exclude).claude/**"` and P7-T2 does enumerate all sixteen
   paths — but the audit scope is the full branch diff, not the D11 pathspec. Those sixteen paths
   were read, swept for host tokens, and are counted against AC11's fourth conjunct in
   `feature-audit.2026-09-04T02-11.md`. The full-diff audit proceeded.

2. Verbatim: *"A DECOY untracked mirror of this feature folder exists under
   `C:/Users/DanMoisan/repos/TaskMaster-wt/2026-09-02T08-47/docs/features/active/...`. Do not read,
   edit, or cite it."*
   Disposition: **accepted, not a narrowing.** This disambiguates two copies of the same folder
   rather than excluding any part of the branch diff. All evidence in this audit was read from the
   worktree at `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a9f3f171e35df71ef`.

`artifacts/orchestration/orchestrator-state.json` was checked and does not appear in the branch
diff at all, so its `skip-worktree` status required no adjudication.

## Executive Summary

**Verdict: PASS with 0 blocking findings.** Five non-blocking findings are recorded, three of them
coverage-threshold rows that are irreducible within this item's scope.

The five in-scope findings of issue #736 are delivered with a regression-first evidence trail. The
COM normalization seam, the keyboard-dispatch containment point, the user-facing sink default, the
breadcrumb-bind boundary reroute, and the finding-6 test rewrite are all present and independently
verified against source. Finding 3 (`ActionOkAsync` / disposal ordering) is untouched, as required.

Every claim this review could re-derive from primary artifacts was re-derived rather than accepted:

| Executor claim | Independent verification | Result |
|---|---|---|
| Analyzer rebuild exit 0, 0 warnings, non-vacuous | Parsed `coverage/p6-t4-analyzer.detailed.log` (10,591,793 bytes) | Confirmed: `Build succeeded`, 0 Warning(s), 0 Error(s), `Skipping target "CoreCompile"` = **0**, `Task "Csc"` = **18** |
| Nullable rebuild exit 0, 0 warnings, non-vacuous | Parsed `coverage/p6-t5-nullable.detailed.log` (10,627,425 bytes) | Confirmed: identical counts, 0/18 |
| Full suite 7013/7013 | Parsed `coverage/p6-t6-run.log` tail | Confirmed: `Test Run Successful.`, Total 7013, Passed 7013 |
| Coverage doc identity | `Get-FileHash -Algorithm SHA256` on `coverage/p6-t6-postchange.cobertura.xml` | Confirmed: `A462D34E...44A777`, byte-identical to the SHA-256 recorded in `p6-t6-coverage.md`, and the run log names this exact path as its output |
| Repo line 85.46% / branch 79.52% | Read `/coverage/@line-rate` and `@branch-rate` | Confirmed: `0.85459` / `0.795242` |
| Changed lines 59 coverable, 52 covered, 88.14% | Re-derived the changed-line set from `git diff -U0` and re-joined it to the Cobertura per-line `hits` | Confirmed exactly: 59 / 52 / 88.1356% |
| Uncovered changed lines outside `U` = 0 | Same join, enumerating every uncovered changed coverable line | Confirmed: the uncovered set is exactly `U`, cardinality 7, **0 members outside it** |
| No unredacted host token in any committed artifact | `git grep -i -E "danmoisan\|megalodon\|dmoisan"` over **every** branch commit, plus an added-content sweep over the whole diff | Confirmed: 0 hits in all 8 commits and 0 hits across 9,605 added content lines |

The one item requiring independent adjudication — AC12's check-off against an 88.14% strict
aggregate — is adjudicated in `feature-audit.2026-09-04T02-11.md` as **PARTIAL**. The coverage
escape's stated precondition is sound and verified; the AC's literal 90% conjunct is not met; the
escape does not launder any uncovered reachable line.

## 1. General Unit Test Policy Compliance

Reference: `.claude/rules/general-unit-test.md` and CLAUDE.md § General Unit Test Policy.

| Rule | Verdict | Evidence |
|---|---|---|
| UT1 Independence | PASS | All 18 new tests construct their own controller/globals. `UserFaultNotifier` is backed by `AsyncLocal<T>`, so a value installed by one test does not leak to a class running in parallel under the configured `ClassLevel` scope. Both tests that mutate it restore the previous value in a `finally`. |
| UT1 Isolation | PASS | Each test targets one member: the two `KbdExecuteAsync` overloads, `RunKbdGuardedAsync`, `TryReportBoundaryFault`'s two defensive branches, the default `BoundaryErrorSink`, `BindBreadcrumbRowsAsync`, and the six `ResolveValidatedArchiveRootPath` core cases. |
| UT1 Fast execution | PASS | Full 7013-test suite completed in 35.15 s. No new test carries a sleep or delay. |
| UT1 Determinism | **PARTIAL** | See finding **F-3**. 16 of 18 new tests are fully deterministic. `BoundaryErrorSink_DefaultDelegate_ReturnsWithoutBlockingTheCallingThread` reaches the production default `ShowModelessFaultNotice`, whose only guard is `System.Windows.Forms.Application.OpenForms.Count == 0` — app-domain-wide mutable global state. |
| UT1 Readability | PASS | Every one of the 18 new methods carries an XML `<summary>` naming the finding and the scenario, and a behavioural name. |
| UT2 Repo-wide line coverage | PASS | 85.459% against the 85% floor in `.claude/rules/general-unit-test.md`, and against CLAUDE.md's 80% floor. Baseline was 85.4332%; the change **raises** it. |
| UT2 Repo-wide branch coverage | PASS | 79.5242% against the 75% floor. Baseline 79.5348%; a 0.0106-point decline, still 4.5 points clear of the floor. |
| UT2 New code >= 90% | **FAIL (non-blocking)** | See findings **F-1** and **F-2**. |
| UT2 No regression on changed lines | PASS | No changed line that carried `hits > 0` in the P0-T6 baseline carries `hits = 0` in the post-change document. Two lines moved the other way: `EfcDataModel.cs` `SortEmail.Cleanup_Files();` and `return result;` were `hits=0` at baseline (the incidental `NullReferenceException` stopped short of them) and are `hits=1` now. |
| UT2 Coverage exclusion policy — no production file excluded from measurement | PASS | `coverage.config` is unmodified on this branch. The two exclusions introduced are `[ExcludeFromCodeCoverage]` **attributes**, not config `exclude` globs. Per the standing adjudication of this repository's clause, the Blocking clause in `.claude/rules/general-unit-test.md` addresses config `exclude` entries matching production paths; attribute-level exclusion is governed by CLAUDE.md's ratified COM/VSTO/WinForms exemption, which is the higher-authority document. Both attributed members qualify: one is a pure Outlook COM crossing, the other constructs WinForms controls. |
| UT3 Arrange–Act–Assert | PASS | 12/12 `// Arrange`, `// Act`, `// Assert` in `EfcFormControllerTests.Part2.cs`; 6/6/6 in `AppOlObjectsArchiveRootComGuardTests.cs`. |
| UT3 Clear failure messages | PASS | Every FluentAssertions call in the new files carries a `because` string stating the invariant. |
| UT4 No external dependencies | PASS | The six `AppOlObjectsArchiveRootComGuardTests` drive the static delegate core; no `AppOlObjects` instance is constructed and no Outlook type is referenced. The controller tests use `MockBehavior.Strict` seams over `IOlObjects` / `IApplicationGlobals` / `IQfcKeyboardHandler`. |
| UT4 No temporary files | PASS | Zero matches for `GetTempPath`, `GetTempFileName`, `File.WriteAllText`, `new StreamWriter` across all three touched test files. |
| UT4 No mutable global state | **PARTIAL** | Same as UT1 Determinism — finding **F-3**. |
| Test file location (`tests/` mirror) | PASS (repo convention) | This repository predates that rule and uses per-project `*.Test` assemblies mirroring the production project. The new files follow the established layout: `TaskMaster/AppGlobals/…` → `TaskMaster.Test/AppGlobals/…`, `QuickFiler/Controllers/…` → `QuickFiler.Test/Controllers/…`. No test file was placed in a production source tree. |
| Determinism infrastructure — banned APIs in tests | PASS | Zero `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow` in the new test files. `BoundaryErrorSink_DefaultDelegate_ReturnsWithoutBlockingTheCallingThread` measures elapsed time with `Stopwatch` and asserts an upper bound rather than waiting. |

## 2. General Code Change Policy Compliance

Reference: `.claude/rules/general-code-change.md`.

| Rule | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | `RunKbdGuardedAsync` is a single containment point both `KbdExecuteAsync` overloads delegate to, rather than duplicated try/catch in each. |
| Reusability | PASS | The delegate-driven `ResolveValidatedArchiveRootPath(Func<string>, Func<string>, Action<string>)` core is reused by the COM wrapper and by all six unit tests. `AttachSucceedingKeyboardHandler` factors the shared arrangement of the two success-path tests. |
| Extensibility | PASS | `InvokeFilerAsync` is `protected internal virtual`; `UserFaultNotifier` and `BoundaryErrorSink` are injectable seams. No public API signature was broken. |
| Separation of concerns | PASS | The decision logic (normalize / validate / diagnose) is in a static core free of Outlook COM types; the COM crossing is isolated in a 8-line wrapper. This is the shape `.claude/rules/general-unit-test.md` prescribes: "extract all logic into host-neutral, testable modules and leave only the thinnest possible wiring in the host-bound entry point." |
| Fail fast and explicitly | PASS | The COM catch re-throws as `InvalidOperationException` with the original preserved; it does not swallow. |
| No silent error swallowing | PASS | `RunKbdGuardedAsync`'s general catch routes to `TryReportBoundaryFault` (log **and** user surface); its `OperationCanceledException` arm logs at debug, which is a deliberate classification, not a silent drop. |
| Established logging pattern | PASS | `logger.Error` / `logger.Debug` on the existing static `logger`; no `Console.Write` introduced. |
| Naming | PASS | PascalCase members, camelCase locals, descriptive names throughout. |
| Comment *why*, not *what* | PASS | Each non-obvious construct carries its rationale: why `AsyncLocal` and not a plain static, why a static method group and not a lambda, why the early return in `ShowModelessFaultNotice` is load-bearing, why the `EmailFilerConfig` construction stays inline. |
| Dependencies | PASS | No package reference added. `System.Runtime.InteropServices` and `System.Diagnostics.CodeAnalysis` are BCL. |
| I/O boundaries | PASS | Domain logic is testable without filesystem or network; verified by the six COM-guard tests running with no Outlook process. |
| **File size <= 500 lines** | **FAIL (non-blocking)** | See finding **F-4**. |

Measured line counts at HEAD:

| File | Lines | Verdict |
|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | **1320** | **FAIL** — exceeds 500. Pre-existing (1216 at base); this change adds 104. |
| `QuickFiler/Controllers/EfcDataModel.cs` | 499 | PASS — 1 line of headroom. |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 493 | PASS |
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | 95 | PASS |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 485 | PASS |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs` | 490 | PASS |
| `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` | 399 | PASS |
| `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs` | 207 | PASS |

## 3. Language-Specific Code Change Policy Compliance

Reference: CLAUDE.md § C# Code Change Policy. C# is the only language with changed source files.

| Requirement | Verdict | Evidence |
|---|---|---|
| C#1.1 Formatting — `dotnet tool run csharpier check .` | PASS (evidence-verified) | `evidence/qa-gates/p6-t2-format-check.md` — exit 0, `Checked 1580 files in 5613ms.` `evidence/qa-gates/p6-t1-format.md` records `Formatted 1580 files in 2390ms.` with mechanically identical `git status --porcelain` spans immediately before and after, so the format step auto-fixed nothing. Not independently re-run: `dotnet` is outside this session's command allowlist. |
| C#1.1 `dotnet format` not used | PASS | No `.csproj` was rewritten. The three project-file diffs are single-line `<Compile Include=…>` additions in existing `ItemGroup`s. |
| C#1.2 Analyzers — `/t:Rebuild` with `EnableNETAnalyzers` + `EnforceCodeStyleInBuild` | PASS (independently verified) | `Build succeeded`, 0 Warning(s), 0 Error(s). Non-vacuity re-derived by this review from the 67,979-line detailed log: `Skipping target "CoreCompile"` = 0, `Task "Csc"` = 18. |
| C#1.2 `/t:Rebuild` used, not `/t:Build` | PASS | The zero-skip / 18-Csc pair is the positive proof that `CoreCompile` ran on every project. |
| C#1.3 Nullable — `/t:Rebuild` with `TreatWarningsAsErrors=true` | PASS (independently verified) | 68,986-line detailed log: `Build succeeded`, 0 Warning(s), 0 Error(s), 0 skips, 18 Csc. |
| C#1.3 `/p:Nullable=enable` **not** added | PASS | The recorded command omits it, matching `.github/workflows/ci.yml` and CLAUDE.md's explicit prohibition. |
| C#2.1 Strong contracts | PASS | `ResolveValidatedArchiveRootPath` documents its three delegate parameters, its return, and its single exception type in XML docs. |
| C#2.2 Null-safety | PASS | `logDiagnostic?.Invoke(...)` and `UserFaultNotifier?.Invoke(...)` guard the two injectable delegates. `ArchiveRoot?.FolderPath` retains the pre-existing null-conditional. |
| C#2.4 Async and resource safety | PASS | `RunKbdGuardedAsync` awaits; the notice form self-disposes via `FormClosed`. No `async void` introduced. |
| C#5 Public surface minimal | PASS | Every new member is `internal`, `private`, or `protected internal`. `KbdExecuteAsync` remains `public` (pre-existing). |
| C#6.2 XML documentation on non-obvious APIs | PASS | All seven new members carry XML docs. |
| C#7 Suppressions narrow and documented | PASS | Two `[ExcludeFromCodeCoverage]` attributes, each with a `<remarks>` block stating the reason and naming the precedent (`ResolveInboxForStore` in `AppOlObjects.StoreRehook.cs`). |

## 4. Language-Specific Unit Test Policy Compliance

Reference: CLAUDE.md § C# Unit Test Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| CUT1 MSTest framework | PASS | `using Microsoft.VisualStudio.TestTools.UnitTesting;` with `[TestClass]` / `[TestMethod]` in both new/changed test files. No xUnit or NUnit reference introduced. |
| CUT2 Moq for mocking | PASS | `new Mock<IOlObjects>(MockBehavior.Strict)`, `new Mock<IApplicationGlobals>(MockBehavior.Strict)`, `new Mock<IQfcKeyboardHandler>(MockBehavior.Strict)`. Strict behaviour in all three cases, which is stronger than the policy requires. |
| CUT2 FluentAssertions for assertions | PASS | Every assertion in the 18 new methods is FluentAssertions (`.Should()...`). The one MSTest-shaped construct — `olObjects.VerifyGet(..., Times.Once())` — is a Moq verification, not an assertion. |
| CUT3 Toolchain command selection | PASS | Format → analyzer → nullable → vstest with coverage, in order, in a single final pass after the P6-T13 restart. Log mtimes corroborate the ordering: analyzer 05:35:01Z, nullable 05:36:22Z, coverage run 05:38:33Z, delivery commit 05:48:31Z. |
| CUT3 `/EnableCodeCoverage` equivalent | PASS | `Invoke-DotnetCoverageCollection` with `coverage.config`, emitting the Cobertura document whose SHA-256 this review re-computed and matched. |

## 5. Test Coverage Detail

**Coverage artifact.** `coverage/p6-t6-postchange.cobertura.xml`, 12,731,976 bytes, SHA-256
`A462D34E34BCA57A8AFC77A861562C1CBD5674B27EAC062BFE3DBC729044A777`. The canonical reviewer path
`artifacts/csharp/coverage.xml` is **absent**; this document is the substitute and its provenance is
established rather than assumed: `coverage/p6-t6-run.log` names it as the output of the 7013/7013
run, and its SHA-256 matches the value transcribed into `evidence/qa-gates/p6-t6-coverage.md`. The
document is gitignored, so it is present for this review but will not survive the merge — recorded
as finding **F-5**.

**Repo-wide, C#:** line coverage **85.459% PASS** (floor 85%); branch coverage **79.5242% PASS**
(floor 75%). Root attributes: `lines-covered` 55321, `lines-valid` 64734, `branches-covered` 13236,
`branches-valid` 16644. Baseline (`coverage/p0-t6-baseline.cobertura.xml`, same session): line
85.4332%, branch 79.5348%.

**Per-file, base vs post** (union of per-line `hits` across every `<class>` carrying the filename):

| File | Status | Base | Post | Verdict vs 85% |
|---|---|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | new | n/a | 18/21 = **85.71%** | PASS vs 85%; **FAIL vs the 90% new-file rule** (F-1) |
| `QuickFiler/Controllers/EfcFormController.cs` | modified | 204/794 = 25.69% | 251/821 = **30.57%** | **FAIL** (pre-existing debt; improved by 4.88 points) |
| `QuickFiler/Controllers/EfcDataModel.cs` | modified | 188/284 = 66.20% | 189/286 = **66.08%** | **FAIL** (pre-existing debt; +1 covered line, +2 coverable) |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | modified | 71/240 = 29.58% | 71/236 = **30.08%** | **FAIL** (pre-existing debt; improved by 0.50 points) |

**Changed-line detail, re-derived by this review.** The changed-line set was rebuilt from
`git diff -U0 origin/main...HEAD` and joined to the Cobertura per-line `hits`:

| File | Changed lines | Coverable | Covered | Uncovered |
|---|---|---|---|---|
| `EfcFormController.cs` | 111 | 33 | 33 | — |
| `AppOlObjects.ArchiveRoot.cs` | 95 | 21 | 18 | 89, 90, 91 |
| `EfcDataModel.cs` | 16 | 4 | 1 | 359, 360, 361 |
| `AppOlObjects.cs` | 4 | 1 | 0 | 266 |
| **Total** | **226** | **59** | **52** | **7** |

Strict aggregate **52/59 = 88.1356%**. The uncovered set is **exactly** the seven-member set `U`
declared in advance by the plan's D2, with **zero** members outside it. This confirms the escape's
stated precondition. The characterization of all seven as "unreachable" is qualified in
`feature-audit.2026-09-04T02-11.md` § AC12.

**Language coverage verdicts** (explicit for every language, per the audit scope invariant):

- C#: changed files present (8 source + 3 project files). Repo-wide line 85.459% and branch 79.5242% — **PASS**. Per-file new/modified thresholds — **FAIL**, non-blocking, findings F-1/F-2.
- PowerShell: **zero changed files** in the branch diff. No `.ps1`/`.psm1`/`.psd1` path appears in the 98-path diff. Pester coverage — N/A, not applicable, zero changed files.
- Python: **zero changed files**. No `.py` path in the diff. Coverage — N/A, zero changed files.
- TypeScript: **zero changed files**. No `.ts`/`.tsx` path in the diff. Coverage — N/A, zero changed files.

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Total tests | 7013 | `coverage/p6-t6-run.log`, re-read by this review |
| Passed | 7013 | same |
| Failed | 0 | same |
| Skipped | 0 | same |
| Wall time | 35.1547 s | same |
| Baseline total | 6995 | `evidence/baseline/p0-t6-coverage.md` |
| Delta | +18 | 6 in `AppOlObjectsArchiveRootComGuardTests.cs` + 12 in `EfcFormControllerTests.Part2.cs`; both counts re-derived by counting `[TestMethod]` occurrences (6 and 12) |
| Test assemblies | 9 | leaf-name set matches the required nine |
| Parallelization | `Workers=0`, `Scope=ClassLevel` | `scripts/vscode/TaskMaster.cli.runsettings` |

Regression-first evidence exists for all five in-scope findings. Recorded red/green pairs:
finding 1 `p1-t7` (6 total, 2 passed, 4 failed) → `p1-t9` (6/6/0); finding 2 `p2-t8` (6/0/6) →
`p2-t10` (6/6/0); finding 4 `p4-t4` (3/2/1) → `p4-t6` (3/3/0); finding 5 `p3-t2` → `p3-t4` (2/2/0);
finding 6 `p5-t2` (1/0/1, failure naming `NullReferenceException`) → `p5-t5` (11/11/0). The three
success-path tests from P6-T13 have green-only records, which is structurally correct: that task
changed no production code, so a fail-before run is impossible, and the artifact states this in a
`WhyFailingRunImpossible:` field. No acceptance criterion is discharged by a fail-before observation
on those three.

## 7. Code Quality Checks

| Check | Result |
|---|---|
| `dotnet tool run csharpier check .` | exit 0, 1580 files, no diff (evidence-verified) |
| Analyzer rebuild | exit 0, 0 warnings, 0 errors, non-vacuous (independently verified) |
| Nullable rebuild | exit 0, 0 warnings, 0 errors, non-vacuous (independently verified) |
| vstest + coverage | 7013/7013, exit 0 (independently verified) |
| Toolchain restart discipline | Honoured. P6-T13 added tests mid-gate; the entire four-step pass was re-run afterwards, and `p6-t6-coverage.md` explicitly records itself as the **second** execution, superseding a now-stale first document. |

### Evidence Location Compliance

`validate_evidence_locations.py` is not present in this repository, so the scan was performed
directly against the branch diff. All 71 evidence paths sit under
`docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/<kind>/` with
`<kind>` in {`baseline`, `issue-updates`, `other`, `qa-gates`, `regression-testing`}.

Files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/`: **none**. No `artifacts/` path of any kind appears in the branch diff.
**Verdict: PASS.**

### Host-Identity and Path Hygiene

| Sweep | Result |
|---|---|
| Unredacted account/host tokens (`danmoisan`, `megalodon`, `dmoisan`, `realgoodfoods`, `danmoi~1`, case-insensitive) across all 9,605 added content lines | **0** |
| Same tokens in any changed **path name** | **0** |
| Same tokens in the feature-folder tree of **every one of the 8 branch commits** (`git grep` per commit) | **0** — sanitization was performed in-task, not after commit, so no pre-sanitization blob is reachable in branch history |
| Absolute `C:\Users\…` paths in added content | 407 lines, all of the form `C:\Users\REDACTED\repos\TaskMaster\.claude\worktrees\agent-<id>\…` |

**Verdict: PASS.** The account name is replaced by the literal `REDACTED` in all 407 occurrences,
which satisfies the account/host-name prohibition. The residual absolute-path *shape* — drive,
repo layout, and worktree id — is retained in the four `min.log.txt` extracts and the fourteen TRX
files; it discloses no identity. Noted as an observation, not a finding, in
`code-review.2026-09-04T02-11.md`.

## 8. Gaps and Exceptions

### F-1 — New file below the 90% new-code threshold (non-blocking)

- **File:** `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`, lines 89–91.
- **Rule:** CLAUDE.md UT2, "Any new modules, classes, or methods added must target >= 90% coverage"; reviewer contract, "For each new file: if line coverage is below 90%, flag as FAIL."
- **Measurement:** 18/21 = **85.71% FAIL**. Adjusted figure with the three lines removed: 18/18 = 100.00%.
- **Cause, verified by inspection:** lines 89–91 are the three lambda arguments
  `() => Path.Combine(Root.FolderPath, "Archive")`, `() => ArchiveRoot?.FolderPath`, and
  `message => logger.Error(message)`, passed to the core from inside the `[ExcludeFromCodeCoverage]`
  wrapper `ResolveValidatedArchiveRootPath()` at line 86. Each captures `this`, so the compiler lifts
  it into an instance member of `AppOlObjects` rather than emitting it inside the attributed method.
  The attribute therefore does not reach them. Confirmed against the Cobertura: the wrapper's own
  lines 87, 88, 92, 93 carry no `<line>` node at all (the attribute removed them), while 89, 90, 91
  do and read `hits="0"`.
- **Irreducible within this item's scope:** all three lines are pure Outlook COM crossings
  (`Root.FolderPath`, `ArchiveRoot.FolderPath`) or a logger delegate literal. They cannot execute
  without a live Outlook process, which is precisely CLAUDE.md's ratified COM/VSTO exemption (a)/(c).
- **Disposition: FAIL row, non-blocking.** No remediation cycle recommended.

### F-2 — New method `InvokeFilerAsync` at 0% coverage (non-blocking)

- **File:** `QuickFiler/Controllers/EfcDataModel.cs`, lines 359–361.
- **Rule:** CLAUDE.md UT2, ">= 90% target" for any new method added.
- **Measurement:** 0/3 = **0.00% FAIL**.
- **Cause:** the seam's production body is `return new EmailFiler(config).SortAsync(mailHelpers);`.
  `TestableEfcDataModel` overrides it to return a completed `true`, so no test executes the base body.
- **This is the AC7-mandated design, not an oversight.** The spec requires the override; the spec's
  own Risk 2 anticipates a coverage consequence at this seam.
- **Honest accounting, which the executor's artifact does not state:** the pre-change equivalents
  `EfcDataModel.cs:343` (`var sorter = new EmailFiler(config);`) and `:344`
  (`var result = await sorter.SortAsync(mailHelpers);`) both carry **`hits="1"`** in the P0-T6
  baseline. Production code that a test did execute is now executed by no test. That prior execution
  was, however, the incidental `NullReferenceException` crash that finding 6 exists to eliminate —
  the call never completed and nothing asserted its behaviour. The same edit newly covered two lines
  the crash used to prevent reaching (`SortEmail.Cleanup_Files();` and `return result;`, both
  `hits=0` → `hits=1`), and the file's covered-line count rose 188 → 189.
- **Disposition: FAIL row, non-blocking.** The body is a zero-branch, zero-logic delegation to a
  collaborator with no test fixture, which is the "thinnest possible wiring" shape
  `.claude/rules/general-unit-test.md` prescribes for host-bound code.

### F-3 — Default-sink test depends on `Application.OpenForms`, app-domain-wide mutable global state (non-blocking)

- **Files:** `QuickFiler/Controllers/EfcFormController.cs` `ShowModelessFaultNotice` (the guard);
  `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:316`
  `BoundaryErrorSink_DefaultDelegate_ReturnsWithoutBlockingTheCallingThread` (the dependent test);
  and pre-existing `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:283`
  `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing`.
- **Rule:** `.claude/rules/general-unit-test.md` UT1 Determinism and UT4 "Tests must not rely on
  mutable global state."
- **Mechanism:** both tests invoke the default `BoundaryErrorSink` **without** installing a
  `UserFaultNotifier`, so control reaches `ShowModelessFaultNotice`. Its only protection against
  constructing a WinForms window on an MSTest worker thread is
  `if (System.Windows.Forms.Application.OpenForms.Count == 0) return;`. In .NET Framework,
  `Application.OpenForms` is a single static `FormCollection` for the whole app domain, populated by
  `Form.OnHandleCreated`. The test's outcome is therefore a function of whether any other test in the
  same host currently has a `Form` handle alive.
- **Measured exposure, stated precisely rather than inflated:** `ClassLevel` parallelization with
  `Workers=0` is configured. Within `QuickFiler.Test` the exposure is nil — a search of the whole
  assembly for `new Form(`, `: Form`, and `OpenForms` returned **zero** matches, and
  `WinFormsPumpHost` uses a bare `new ApplicationContext()` with no main form. The exposure is
  cross-assembly: `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73` calls `viewer.Show()` on a
  real `Form`. Whether that can overlap in time with `QuickFiler.Test` depends on whether the nine
  assemblies share one testhost process, which the `Invoke-DotnetCoverageCollection` wrapper
  determines and which this review did not open. The condition did not fire in the 7013/7013 run.
- **Consequence if it fires:** `ShowModelessFaultNotice` would construct a `Form` and a `TextBox` on
  a worker thread with no message pump. `NotThrow` is asserted, so an exception there fails the test.
- **What closes it:** install a capturing `UserFaultNotifier` in both default-sink tests, restoring
  it in a `finally` exactly as `BoundaryErrorSink_DefaultDelegate_RoutesThroughTheUserFaultNotifier`
  already does, so no test outcome depends on `Application.OpenForms` at all.
- **Disposition: PARTIAL, non-blocking.** Latent and order-dependent; not observed.

### F-4 — `EfcFormController.cs` at 1320 lines, 820 over the ceiling (non-blocking, pre-existing)

- **Rule:** `.claude/rules/general-code-change.md`, "No production code, test code, or reusable
  script file may exceed 500 lines."
- **Measurement:** 1320 lines at HEAD, 1216 at base. This change adds 104 lines to a file already
  2.4× over the ceiling.
- **Ratified:** the spec's Scope & Non-Goals places the split out of scope, and the plan's D7 sets a
  budgeted ceiling of 1330, which is respected with 10 lines to spare.
- **Disposition: FAIL row, non-blocking.** Correctly excluded from this item. Recorded because the
  policy is violated and the violation grew; the exclusion is a scoping decision, not compliance.

### F-5 — The substantiating coverage document does not survive the merge (non-blocking)

- **Rule:** AC13's final conjunct, "with the logs retained as evidence."
- **What is retained in git:** `evidence/qa-gates/p6-t4-analyzer.min.log.txt` and
  `p6-t5-nullable.min.log.txt`, each 19 lines listing project→DLL mappings.
- **What those files do not contain:** this review read both. Neither contains a single occurrence of
  `Skipping target "CoreCompile"` or `Task "Csc"`. They are derived extracts and cannot corroborate
  the 0/18 non-vacuity counts that AC13 turns on. The files that can — `coverage/p6-t4-analyzer.detailed.log`
  (10.6 MB) and `coverage/p6-t5-nullable.detailed.log` (10.6 MB) — are gitignored, as is
  `coverage/p6-t6-postchange.cobertura.xml`.
- **Assessment:** the counts are true; this review re-derived all of them from the gitignored logs
  while they still exist. The gap is one of *retention*, not accuracy. After merge, no artifact in
  the repository will substantiate AC13's non-vacuity conjunct or AC12's coverage arithmetic.
- **Countervailing consideration, and why this is not a recommendation to commit them:** committing
  a 12.7 MB Cobertura document leaves a permanent blob in history, and this repository has been
  bitten by exactly that twice (a 21 MB reachable blob, and a Cobertura whose `filename=` attributes
  leaked an account name 2007 times). The right resolution is a small committed *summary* carrying
  the two literal counts and the coverage figures with the source document's SHA-256 — which
  `p6-t6-coverage.md` already does for coverage, and which the two `min.log.txt` extracts do **not**
  do for the msbuild counts.
- **Disposition: PARTIAL, non-blocking.** Recorded against AC13 in the feature audit.

### Exceptions accepted

| Exception | Basis |
|---|---|
| `[ExcludeFromCodeCoverage]` on `AppOlObjects.ResolveValidatedArchiveRootPath()` | CLAUDE.md COM/VSTO exemption (a)/(c). Every expression is an Outlook COM crossing. Precedent: `ResolveInboxForStore` in `AppOlObjects.StoreRehook.cs`. |
| `[ExcludeFromCodeCoverage]` on `EfcFormController.ShowModelessFaultNotice` | CLAUDE.md WinForms exemption (b). Every statement past the early return constructs WinForms controls. |
| `FormatterServices.GetUninitializedObject(typeof(EfcHomeController))` in the new test helper | Documented in-place; matches the technique the sibling test file already uses for viewer seams. The constructor requires a live Outlook context. |
| Absence of `artifacts/csharp/coverage.xml` | Substituted by `coverage/p6-t6-postchange.cobertura.xml` with provenance established by SHA-256 match against the recorded value and by the run log naming it as output. |

## 9. Summary of Changes

98 changed paths, in three groups with nothing outside them:

| Group | Count | Notes |
|---|---|---|
| Ratified eleven-path Write Set | 11 | 8 source + 3 legacy `.csproj` registrations |
| Feature-folder documents and evidence | 71 | `issue.md`, `spec.md`, plan, research, and 67 evidence artifacts |
| `.claude/agent-memory/**` | 16 | Written by this run's task-researcher, prd-feature, atomic-planner, and orchestrator; committed before Phase 0 |

Production changes:

1. **Finding 1** — new partial `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` (95 lines) with a
   COM-guarded delegate-driven core plus an 8-line COM wrapper; `AppOlObjects.ArchiveRootPath`'s
   getter reduced from a five-line guarded call to one line delegating to the seam.
2. **Finding 2** — `RunKbdGuardedAsync` added as the single containment point; both `KbdExecuteAsync`
   overloads rewritten to route through it; `OperationCanceledException` classified as cancellation.
3. **Finding 4** — `BoundaryErrorSink`'s default replaced by a static `DefaultBoundaryErrorSink` that
   logs and then reports through a new `AsyncLocal`-backed injectable `UserFaultNotifier`, whose
   default is a modeless self-disposing notice.
4. **Finding 5** — `BindBreadcrumbRowsAsync`'s general catch rerouted from a bare `logger.Error` to
   `TryReportBoundaryFault`; the `catch (OperationCanceledException)` arm above it left unchanged.
5. **Finding 6** — `EfcDataModel.InvokeFilerAsync` extracted as a `protected internal virtual` seam;
   `TestableEfcDataModel` overrides it; the `ThrowAsync<NullReferenceException>` barrier removed.

Finding 3 (`ActionOkAsync` / disposal ordering) is untouched — verified below.

## 10. Compliance Verdict

**PASS — 0 blocking findings.**

| Section | Verdict |
|---|---|
| 1. General Unit Test Policy | PASS with 2 PARTIAL rows (UT1 Determinism, UT4 global state) and 1 FAIL row (UT2 new-code >= 90%) |
| 2. General Code Change Policy | PASS with 1 FAIL row (file size) |
| 3. C# Code Change Policy | PASS |
| 4. C# Unit Test Policy | PASS |
| 5. Test Coverage Detail | Repo-wide PASS; per-file new/modified FAIL, non-blocking |
| 6. Test Execution Metrics | PASS |
| 7. Code Quality Checks | PASS |
| Evidence Location Compliance | PASS |
| Host-Identity and Path Hygiene | PASS |

Findings F-1 through F-5 are all non-blocking. F-1, F-2, and F-4 are irreducible within this item's
ratified scope. F-3 and F-5 are latent and are recommended as follow-up issues rather than a
remediation cycle. **No `remediation-inputs` artifact is produced**; the rationale is stated in
`code-review.2026-09-04T02-11.md` § Remediation Decision.

## Appendix A: Test Inventory

**New — `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs` (6 methods, 207 lines)**

| # | Method | Scenario |
|---|---|---|
| 1 | `ResolveValidatedArchiveRootPath_WhenComposedReadThrowsComException_NormalizesToInvalidOperation` | COM fault on read 1 → `InvalidOperationException`, `InnerException` same instance |
| 2 | `ResolveValidatedArchiveRootPath_WhenResolvedReadThrowsComException_NormalizesToInvalidOperation` | COM fault on read 2 → same normalization |
| 3 | `ResolveValidatedArchiveRootPath_WhenComReadFails_MessageWithholdsPathAndMailboxAddress` | #602 redaction on both the exception message and the diagnostic |
| 4 | `ResolveValidatedArchiveRootPath_WhenBothReadsResolve_ReturnsPathAndEmitsNoDiagnostic` | Positive flow |
| 5 | `ResolveValidatedArchiveRootPath_WhenResolvedFolderIsNull_ThrowsUnresolvableWithNoInnerException` | Frozen guard's own branch still reaches the caller |
| 6 | `ResolveValidatedArchiveRootPath_WhenComReadFailsTwice_ReReadsTheComposedPathOnTheSecondCall` | Failed resolution not cached |

**New — `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs` (12 methods, 490 lines)**

| # | Method | Scenario |
|---|---|---|
| 1 | `KbdExecuteAsync_FuncTaskOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow` | Fault path, overload 1 |
| 2 | `KbdExecuteAsync_ActionOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow` | Fault path, overload 2 |
| 3 | `KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow` | `TryReportBoundaryFault` null-sink branch |
| 4 | `KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow` | `TryReportBoundaryFault` throwing-sink branch |
| 5 | `RunKbdGuardedAsync_WhenBodyThrowsOperationCanceled_DoesNotReportAsFault` | Cancellation classification |
| 6 | `RunKbdGuardedAsync_WhenBodyThrowsInvalidOperation_ReportsExactlyOnce` | Fault classification |
| 7 | `BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow` | Finding 5 boundary |
| 8 | `BoundaryErrorSink_DefaultDelegate_RoutesThroughTheUserFaultNotifier` | Finding 4 user surface |
| 9 | `BoundaryErrorSink_DefaultDelegate_ReturnsWithoutBlockingTheCallingThread` | Finding 4 non-blocking constraint — see F-3 |
| 10 | `RunKbdGuardedAsync_WhenBodyCompletes_InvokesBodyAndReportsNothing` | **Success path**, guard |
| 11 | `KbdExecuteAsync_FuncTaskOverload_WhenToggleSucceeds_AwaitsTheAction` | **Success path**, overload 1 |
| 12 | `KbdExecuteAsync_ActionOverload_WhenToggleSucceeds_InvokesTheAction` | **Success path**, overload 2 |

**Modified — `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` (11 methods, 399 lines)**
One method rewritten (`MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`); one override
added to `TestableEfcDataModel`. All 11 pass.

## Appendix B: Toolchain Commands Reference

Commands recorded as executed by the delivering agent, in order, in a single final pass:

1. `dotnet tool run csharpier format .` — exit 0, `Formatted 1580 files in 2390ms.`, no file changed
2. `dotnet tool run csharpier check .` — exit 0, `Checked 1580 files in 5613ms.`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — exit 0, 0 warnings, 0 errors
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — exit 0, 0 warnings, 0 errors
5. `vstest.console.exe <9 assemblies>` via `Invoke-DotnetCoverageCollection` with `coverage.config` and `TaskMaster.cli.runsettings` — 7013/7013

Commands run by **this review** (all check-only, no mutation):

- `git merge-base origin/main HEAD`; `git log --oneline origin/main..HEAD`
- `git diff --numstat origin/main...HEAD`; `git diff -U0 origin/main...HEAD -- <4 production files>`
- `git grep -i -l -E "danmoisan|megalodon|dmoisan" <each of 8 commits> -- <feature folder>`
- `git status --porcelain=v1 --ignored --untracked-files=all -- coverage artifacts`
- PowerShell XML joins over `coverage/p0-t6-baseline.cobertura.xml` and
  `coverage/p6-t6-postchange.cobertura.xml`; regex counts over the two detailed msbuild logs;
  `Get-FileHash -Algorithm SHA256`
