# Feature Audit — uithread-dispatcher-null-race-progresstrackerasync (#584)

- **Date:** 2026-09-04
- **Timestamp:** 2026-09-04T04-05
- **Work Mode:** `full-bug` (marker at `issue.md:3`)
- **AC Source (sole, per work mode):** `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md`, Version 0.5
- **Baseline:** `87cb4df338322844abfa580abea14df77e738e5c`
- **Branch:** `bug/uithread-dispatcher-null-race-progresstrackerasync-584`

**BLOCKING FINDINGS COUNT (this artifact): 0**

No `user-story.md` exists and none is expected: under `full-bug`, `spec.md` is the sole AC source.
Verified — the feature folder contains `issue.md`, `spec.md`, `plan.2026-09-02T09-02.md`,
`research/`, and `evidence/`, with no `user-story.md`.

---

## 1. Acceptance Criteria Evaluation

All seven criteria were already marked `[x]` in `spec.md` before this review. Each was re-evaluated
against its cited evidence, and the head-state facts underlying each were independently re-derived
where that was possible without running commands. **No criterion was found unsupported.** This
review checked off nothing new (nothing remained unchecked) and edited no AC text.

### AC1 — Named `InvalidOperationException` instead of `NullReferenceException`, verified by a deterministic test — **PASS**

| Clause | Verification |
|---|---|
| Throws a named `InvalidOperationException` | `UiThread.cs:139-144` — `if (_dispatcher is null) { throw new InvalidOperationException(...); }`. Read directly. |
| Message names the missing `Initialize()` | `UiThread.cs:142` — "The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher." Names both the entry point and the initialiser. |
| Verified by a deterministic regression test | `UiThread_Tests.cs:133-158`. Asserts `Throw<InvalidOperationException>().WithMessage("*UiThread.Initialize()*")` at line 150-152. |
| Test does not rely on timing, sleeps, retries | Full read of the 179-line file: none of `Thread.Sleep`, `Task.Delay`, `SpinWait`, retry, `Timeout(`, `PushFrame`. The arrangement is a reflective field write. |
| Test does not rely on full-suite execution order | `[DoNotParallelize]` at `:122`; prior field value captured and restored in `finally` at `:155-157`. Passes in a 2-test isolated run (`p3-t2`). |

**Fail-before / pass-after is genuine.** `p1-t4-expect-fail.md`: `Total tests: 2, Passed: 1, Failed: 1`,
with the verbatim message "Expected a `<System.InvalidOperationException>` to be thrown, but no
exception was thrown" at `UiThread_Tests.cs:150`. The sibling build (`p1-t3-build-before-fix.md`)
recorded a clean `0 Error(s)` result over the same tree state; the two artifacts' recorded
`Timestamp:` values do not establish their relative execution order, and the conclusion does not
depend on the order because the sibling positive test passed in the same run. The failure is
therefore an assertion-level RED rather than a compile error, and not a harness failure.
`p3-t2-regression-green.md`: `Total tests: 2, Passed: 2`, TRX `failed="0"`.

Cited evidence: `evidence/regression-testing/p1-t4-expect-fail.md`,
`evidence/regression-testing/p3-t2-regression-green.md`. Both exist and say what AC1 claims.

### AC2 — `null!` removed, field declared `Dispatcher?`, nullable analyser verifies the guard — **PASS**

| Clause | Verification |
|---|---|
| `null!` suppression removed | `p2-t2-nullforgiving-removed.md` records zero `null!` matches in the file (git grep exit 1). **Independently confirmed** by reading all 172 lines of `UiThread.cs`: no `null!` appears. |
| Field type is `Dispatcher?` | `UiThread.cs:149` — `private static Dispatcher? _dispatcher;`. Read directly. The stale trailing comment `// set in Initialize() before any access` is correctly removed with it. |
| Nullable analyser can verify the guard | `p4-t4-nullable-build.md`: exit 0, `0 Warning(s)`, `0 Error(s)` under `/p:TreatWarningsAsErrors=true` with `/t:Rebuild`. |

**The gate is non-vacuous, and this was checked rather than assumed.** Two conditions had to hold for
the nullable build to constitute real evidence, and both do:

1. `UiThread.cs:1` carries `#nullable enable` — verified by direct read. Nullable enforcement in this
   repository is per-file opt-in, so without this directive the build would prove nothing about this
   file.
2. The command uses `/t:Rebuild`, not `/t:Build`. A warm `/t:Build` skips `CoreCompile` when only
   `/p:` values change and would return exit 0 having run no analysis. `p4-t4-nullable-build.md`
   quotes the command line; `/t:Rebuild` is present and no `/p:Nullable=enable` substring appears,
   matching CLAUDE.md and `.github/workflows/ci.yml`.

With both true, a getter returning a `Dispatcher?` field as a non-nullable `Dispatcher` without
narrowing would raise `CS8603` and fail the build. `0 Error(s)` therefore proves the guard narrows
the field. AC2's substantive claim holds.

### AC3 — `ProgressTrackerAsync.cs` left unmodified, with the verification recorded — **PASS**

| Clause | Verification |
|---|---|
| File unmodified | `p3-t4-progresstrackerasync-unmodified.md`: empty `git status --porcelain` for that path *and* empty `git diff --name-status --cached 87cb4df3` for that path. The two spans are complementary — the porcelain catches an unstaged edit that `--cached` would miss, and vice versa. Both empty. |
| Complete staged footprint is only the owned files | Same artifact records exactly five `M` entries under `UtilitiesCS`/`UtilitiesCS.Test` and nothing else. |
| Consumer call site located | `git grep` returns exactly one hit at line 33: `UiDispatcher = UiThread.Dispatcher;`, matching the line number `p0-t3` recorded. |
| Verification paragraph recorded | Present in that artifact, §"Why the fix in UiThread.cs alone converts this consumer's failure mode". |

The recorded reasoning is sound and I verified its premise: `InitializeAsync()` assigns from the
property at line 33 and does not dereference the resulting field until line 35, so with the guard in
place the exception is raised at the property access and control never reaches `InvokeAsync`. The
consumer receives a self-diagnosing failure with no code change in that file.

Note for completeness: `p2-t2-nullforgiving-removed.md` records that three `null!` occurrences remain
in `ProgressTrackerAsync.cs`. That file is deliberately outside the write set and AC3 requires it to
be untouched, so leaving them is correct here, not an omission.

### AC4 — No regression in the five named test files; assertions unmodified — **PASS**

| Named file | Status | Evidence |
|---|---|---|
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | Attribute-only change; all pass | `p1-t5-donotparallelize.md`; `p3-t3-at-risk-tests.md` (41/41) |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | Unmodified; all pass | `p3-t3-at-risk-tests.md` |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs` | Unmodified; all pass | `p3-t3-at-risk-tests.md` |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | Attribute-only change; all pass | `p1-t5-donotparallelize.md`; `p3-t3-at-risk-tests.md` |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | Reflection target retargeted; 8/8 pass | `p2-t4-*.md`; `p4-t6-first-pass-failure.md` (8 failing); `p4-t6-quickfiler-tests.md` (1312/1312) |

**The binding "unmodified assertions" clause — verified against the complete file, not the diff
summary.** Read all 320 lines of `EmailMoveMonitorTests.cs`:

- 8 `[TestMethod]` declarations, unchanged from the base count of 8. No test method added, removed,
  or renamed.
- The only assertion inside the changed region is `current.Should().BeSameAs(_capturedDispatcher);`
  at line 65. It is byte-identical to its pre-change form; only the expression producing `current`
  changed, from `DispatcherProperty?.GetValue(null)` to `DispatcherField?.GetValue(null)`.
- No `Mock` setup, no `VerifyAdd`/`VerifyRemove`, no `Times` argument, and no helper
  (`CountingPassThrough`, `CreateMail`, `CreateFolder`) is altered.
- No `using` directive added — the retarget reuses the fully-qualified `System.Reflection` spelling
  the file already used.

The change is exactly what the criterion permits: a retarget of one reflection lookup, plus six
explanatory comment lines. The clause holds.

The two at-risk "dispatcher unavailable" tests are the substantive no-regression proof, and both
pass against the throwing accessor:
`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` (asserts only that nothing
escapes the broad catch, with no type assertion) and `YieldAsync_WithoutDispatcher_RemainsStrict`.

The amendment note on AC4 (round 15) is accurate: the criterion previously named four files
and carried a scope note asserting a regression in an unnamed fifth. Naming the fifth file and
returning the criterion to unchecked until the pass-after evidence existed keeps the criterion
binding — the alternative would have left the repair with no criterion binding it and would have
made the old scope note literally false once the repair landed.

### AC5 — No retry, sleep, or timing tolerance anywhere in the diff — **PASS**

Two artifacts are required to make "anywhere in the diff" true, and both exist:

| Artifact | Coverage | Result |
|---|---|---|
| `p3-t5-no-timing-tokens.md` | pathspec `UtilitiesCS UtilitiesCS.Test` — 5 of 6 owned files; 5626-byte BASE-anchored diff, 94 `+` lines | case-insensitive 7-token filter printed nothing, exit 1 |
| `p2-t4-emailmovemonitor-reflection-target.md` | the sixth file `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`; 2570-byte BASE-anchored diff | identical filter over the whole diff (not just added lines) printed nothing, exit 1 |

The union of the two pathspecs is exactly the six-file write set, so the criterion's "anywhere in the
diff" wording is now evidenced across the whole diff. The round-16 amendment note documenting *why*
this criterion was returned to unchecked — the round-15 write-set widening put a sixth file outside
`p3-t5`'s pathspec — is accurate and is the correct handling of an evidence gap.

**Independently confirmed** for the file carrying the substantive new code: a full read of
`UiThread_Tests.cs` found none of `Thread.Sleep`, `Task.Delay`, `SpinWait`, retry/retries,
`Timeout(`, or `PushFrame`, in code or comment. The regression strategy is structural — force the
backing field to its pre-`Initialize()` state and assert on the accessor contract — which is why no
timing construct is needed. The pre-existing `new Thread(...)`/`Join()` at
`EmailMoveMonitorTests.cs:281-291` is a marshal-target simulation, untouched by this change, and is
not a wall-clock wait.

### AC6 — Full C# toolchain passes in order in a single final pass, with per-step evidence — **PASS**

| Step | Command | Result | Artifact |
|---|---|---|---|
| 1. Format | `dotnet tool run csharpier format UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"` | exit 0, `Formatted 6 files`, identical before/after unscoped porcelain | `p4-t1-format.md` |
| 2. Format check | `dotnet tool run csharpier check .` | exit 0, `Checked 1576 files`, empty reported set | `p4-t2-format-check.md` |
| 3. Analyze | `msbuild ... /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, `0 Warning(s)`, `0 Error(s)` | `p4-t3-analyzer-build.md` |
| 4. Type-check | `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` | exit 0, `0 Warning(s)`, `0 Error(s)` | `p4-t4-nullable-build.md` |
| 5. Test (`UtilitiesCS.Test`) | `dotnet-coverage collect ... -- vstest.console.exe ...` | exit 0, 4787/4787 | `p4-t5-utilitiescs-tests.md` |
| 6. Test (`QuickFiler.Test`) | `vstest.console.exe ...` | exit 0, 1312/1312 | `p4-t6-quickfiler-tests.md` |
| 7. Loop closure | — | all seven steps in order; both passes recorded | `p4-t8-loop-closure.md` |

**"Single final pass" is satisfied correctly, not by omission.** Phase 4 ran twice, and both passes
are recorded chronologically. Pass 1 was green through P4-T5 and failed at P4-T6 (1304 of 1312). The
response was the policy-mandated one: P2-T4 rewrote a tracked source file, so the loop restarted from
step 1 and every step re-ran in order against the new tree. `p4-t8-loop-closure.md` then affirmatively
establishes that no step after P4-T1 rewrote a tracked file in pass 2 — the formatter's before/after
unscoped porcelain outputs are byte-identical, `csharpier check` is read-only, the msbuild steps write
only to gitignored `bin/` and `obj/`, and the test steps write only to gitignored `coverage/` and
`TestResults/`. Pass 2 is therefore a genuine single clean pass. The pass-1 failure artifact was
preserved under a distinct name before pass 2 overwrote the qa-gates copy, so the fail-before record
was not destroyed.

Test-count arithmetic is consistent: baseline `UtilitiesCS.Test` 4785 -> 4787, exactly the two added
tests; `QuickFiler.Test` 1312 -> 1312, consistent with an attribute/reflection-only change.

`p4-t7-coverage-delta.md` is correctly *not* cited under AC6; it is AC7's evidence.

### AC7 — No repo-wide line-coverage regression; changed lines meet the `>= 90%` new-code target — **PASS**

| Clause | Figure | Verdict |
|---|---|---|
| No regression vs baseline `line-rate` | 0.7073317347831605 -> 0.7073603942281368 (+0.0000286594) | PASS — an increase; the 0.005 tolerance is not consumed |
| Denominator comparability | `lines-valid` 149719 -> 149761 = **+42**, inside the 0-200 band | PASS — no `COVERAGE DENOMINATOR MISMATCH`, so the comparison stands rather than being VOID |
| Changed-line coverage >= 90% | 100.0% — 8 of 8 coverable added lines (138-143, 145, 146), each `hits >= 1` | PASS |

The changed-line derivation is methodologically sound and I checked its two weak points:

- **Non-coverable added lines are excluded correctly, and the exclusion is justified per line.**
  Added lines 137, 144, and 149 carry no `<line>` element (the `get` accessor header, the `if`
  block's closing brace, and the field declaration). None is an emitted sequence point, so excluding
  them from the denominator is correct rather than convenient.
- **The throw path is actually executed, not merely compiled.** Lines 141, 142, and 143 are the
  `throw new InvalidOperationException(`, its message argument, and the closing `);`, and all three
  carry `hits = 1`. This is the coverage-side corroboration of the AC1 test result.
- Context line 147 (`private set => _dispatcher = value;`) has a `<line>` element with `hits = 1` but
  is correctly excluded as a context line, not an added line.
- The other two class nodes for this file (`SynchronizationContextAwaiter` and its closure class)
  contribute only lines 87 and 92-105, outside the added-line set, so they do not perturb the
  intersection. This is the class-node double-count trap, and the artifact avoided it.

**Recorded qualification, which this review endorses.** Both `line-rate` figures are raw, unstripped
`dotnet-coverage` rates for the whole `UtilitiesCS.Test` host process, and `p4-t7-coverage-delta.md`
labels them as such on every occurrence rather than presenting them as the repository figure. The
70.74% post-change rate is below CLAUDE.md's 80% and the uniform 85% floor, and the artifact records
this as an explicit `PRE-EXISTING FLOOR SHORTFALL` — the baseline at BASE was 70.73%, already below
it. AC7 does not claim the floor is met; it claims no regression and `>= 90%` on changed lines. Both
of those claims are true. The floor shortfall is recorded as a separate non-blocking FAIL row in
`policy-audit.2026-09-04T04-05.md` §5.2, where the denominator non-comparability is set out in full.

---

## 2. Acceptance Criteria Status

### Acceptance Criteria Status

- Source: `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md`
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

Newly checked off by this review: **none** — all seven were already `[x]` and all seven are supported
by their cited evidence. No AC text was edited and no criterion was added or removed.

| AC | Verdict | Cited evidence exists | Evidence supports the claim |
|---|---|---|---|
| AC1 | PASS | Yes | Yes — verified, plus head-state re-derivation |
| AC2 | PASS | Yes | Yes — verified, plus gate non-vacuity confirmed |
| AC3 | PASS | Yes | Yes — verified |
| AC4 | PASS | Yes | Yes — verified, plus full-file assertion audit |
| AC5 | PASS | Yes | Yes — verified, plus full-file token audit |
| AC6 | PASS | Yes | Yes — verified, including single-clean-pass semantics |
| AC7 | PASS | Yes | Yes — verified, including the exclusion and double-count checks |

## 3. Baseline Comparison

| Dimension | BASE `87cb4df3` | HEAD | Delta |
|---|---|---|---|
| `UiThread.Dispatcher` on unset field | returns `null` silently | throws `InvalidOperationException` naming `Init()`/`Initialize()` | defect removed at source |
| `_dispatcher` declaration | `private static Dispatcher _dispatcher = null!;` | `private static Dispatcher? _dispatcher;` | suppression removed; analyser re-engaged |
| `UtilitiesCS.Test` tests | 4785 passing | 4787 passing | +2, both new |
| `QuickFiler.Test` tests | 1312 passing | 1312 passing | 0 |
| Classes mutating `UiThread._dispatcher` in the parallel bucket | 3 | 0 | 4 classes serialised |
| Reflective reads of the `Dispatcher` *property* | 1 (`EmailMoveMonitorTests`) | 0 | retargeted to the field |
| Raw process-wide line rate | 0.70733 | 0.70736 | +0.0000287 |
| Files > 500 lines among changed files | 1 (`ProgressTracker_Tests.cs`, 514) | 1 (same file, 514) | 0 |

## 4. Plan Delivery

`plan.2026-09-02T09-02.md`, Version 2.1: **50 of 50 tasks complete, 0 unchecked** — verified by
checkbox count against the plan file. The write set declared in `spec.md` (six files) matches the
committed diff exactly, with no file added or omitted.

## 5. Findings

**BLOCKING FINDINGS: 0**

No acceptance criterion is unsupported. No remediation is required and no
`remediation-inputs.<timestamp>.md` artifact is produced.

Non-blocking items carried in the companion artifacts:

| ID | Severity | Artifact | Summary |
|---|---|---|---|
| F1 | FAIL (non-blocking) | policy-audit §5.2, §8 | Repo-wide raw coverage 70.736% line / 46.79% branch below the 85%/75% floors; non-comparable denominator, pre-existing, improved by this change |
| F2 | FAIL (non-blocking) | policy-audit §5.1, §8 | Canonical `artifacts/csharp/coverage.xml` absent; equivalent Cobertura documents present and parsed |
| F3 / CR-1 | PARTIAL (non-blocking) | policy-audit §2.9, code-review §3 | `ProgressTracker_Tests.cs` 514 lines > 500; pre-existing at BASE, branch delta 0 |
| F4 | PARTIAL (non-blocking) | policy-audit §8 | `artifacts/pr_context.*` stale — describe issue #565 on another branch; scope derived from the verified `87cb4df3..HEAD` range |
| F5 / CR-2 | Minor (non-blocking) | policy-audit §2.17, code-review §3 | Deferred follow-up recorded only as feature-folder prose; promote to a GitHub issue before merge |
| F8 | Low (non-blocking) | policy-audit §8, code-review §5 | `[DoNotParallelize]` census covered reflective writers only; indirect production writer path verified closed by this review |
| CR-3..CR-7 | Low (non-blocking) | code-review §3 | Test-code polish: asymmetric null guard, `?.` vacuous-assertion risk, `CurrentDispatcher` thread residue, attribute-form inconsistency, three null policies in one class |

## 6. Verdict

**ACCEPT.** All seven acceptance criteria are delivered and verified against evidence. The fix
addresses the reported defect at its structural root rather than its timing-dependent symptom, the
regression test is deterministic by construction with an assertion-level fail-before, and the
public-API behaviour change is accompanied by a blast-radius census that this review independently
reproduced and extended by one route (`using static`) the census had not enumerated.

**Total blocking findings across all three artifacts: 0.**
