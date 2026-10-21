# Feature Audit — Issue #826 (console-out aggressors and banned-symbol promotion)

- Date: 2026-09-09
- Work Mode: `full-bug` (marker at `issue.md` line 12)
- Acceptance-criteria source: `spec.md` **only**, section `## Acceptance Criteria`, criteria AC1–AC16
- Branch: `bug/console-out-aggressors-and-banned-symbol-promotion-826-exec`
- Head: `077856c915cccf81d89898d4b3e2537a44b30f3e`
- Base: `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`
- Verdict: **PASS** — 16 of 16 acceptance criteria PASS, 0 blocking findings

## AC Source Resolution

Work Mode is `full-bug`, so per the `acceptance-criteria-tracking` skill the sole authoritative
acceptance-criteria source is `spec.md`. No `user-story.md` exists for this feature. **That absence is
correct for `full-bug` and is not recorded as a gap.** `spec.md` states explicitly that no `user-story.md`
may be created, because a second file carrying `- [ ]` items would split the criteria and break the
check-off protocol.

`issue.md` is not an AC source in this mode, and correctly does not appear in the change footprint.

## Verification Method

Every criterion was re-verified by the reviewer directly against the delivered worktree rather than accepted
from the executor's evidence. Where a criterion depends on a git-derived measurement that cannot be produced
without a shell — this review was conducted without the Bash tool at the caller's direction — the caller's
pre-measured figure is used and is labelled `(caller-supplied)`.

## Acceptance Criteria Evaluation

### AC1 — Console writer installs are gone — **PASS**

Repository-wide search of `*.cs` for `Console.SetOut(` returns exactly two hits, reproduced by the reviewer:

- `TaskMaster/ThisAddIn.cs:103` — `Console.SetOut(tw);`, production, out of scope.
- `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs:31` —
  `//Console.SetOut(new DebugTextWriter());`, a commented-out call that installs nothing.

Neither is a test file modified by this feature. Before the change the same search returned 38 occurrences
across 35 files (caller-supplied), so the criterion was capable of failing.

### AC2 — No residual writer references in the changed test files — **PASS**

A repository-wide `DebugTextWriter` search returns six files. Cross-checking against the 33-file write set,
**none of the 33 appears among them**, so the per-file count is zero for every file in scope.

The six retaining files are `UtilitiesCS/HelperClasses/Logging/DebugTextWriter.cs`,
`UtilitiesCS.Test/HelperClasses/DebugTextLogger_Tests.cs`, `UtilitiesCS.Test/DeedleTests.cs`,
`UtilitiesCS.Test/Extensions/DeedleTests.cs`, `TaskMaster/ThisAddIn.cs` and
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs`.

Note: AC2's own allowlist enumerates only the first five. The sixth is the commented-out call the same spec
names under AC1. This is a spec-internal enumeration slip, not a delivery defect — AC2 is scoped to the 33
write-set files and all 33 are clean. Recorded as non-blocking finding NB-5 in the policy audit.

### AC3 — CS0169/CS0414 hazard in the two `TreeNode` files discharged — **PASS**

Both halves verified.

- Whole-word `tw` search across `ToDoModel.Test/Data Model/Tree/` returns **zero matches**, confirming the
  field declaration, the assignment, the `Console.SetOut(tw);` call and the orphaned commented-out
  `[ClassInitialize]` block were all removed together from `TreeNodeTests.cs` and
  `TreeNodeTests_UnfinishedStubs.cs`.
- Toolchain step 3, `msbuild ... /p:TreatWarningsAsErrors=true`, exits 0 with zero `CS0169` and zero `CS0414`
  in its log. The gate is non-vacuous: the log carries 0 occurrences of `Skipping target "CoreCompile"` paired
  with 18 occurrences of `Task "Csc"`.

The criterion deliberately pins this to the type-check step rather than to inspection, and the type-check
step is the evidence relied on.

### AC4 — Empty initializers deleted with their statement — **PASS**

A repository-wide `TestInitialize` census returns 80 files. The reviewer checked each of the ten AC4 files
against that list; **all ten are absent**, so each returns zero hits as required:

`VBFunctions.Test/ComputerInfo_Test.cs`, `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs`,
`UtilitiesCS.Test/Extensions/Frexp_Test.cs`, `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs`,
`UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs`,
`TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs`,
`UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs`.

The removed-line distribution for these ten (6, 7, 7, 12, 6, 6, 6, 6, 7, 6) is consistent with whole-method
deletion rather than single-line deletion, and the three comment-only cases show the larger counts expected
when an orphaned comment is removed alongside the method (caller-supplied distribution).

### AC5 — No console output remains in the table-access file — **PASS**

In `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, reviewer-verified:

- `Console.WriteLine` — **0 occurrences.**
- `Console.` — **1 occurrence**, at line 404: `var target = writer ?? Console.Out;`, the issue #811
  `EnumerateTable` seam, which is out of scope and correctly remains.

### AC6 — Both diagnostics routed through the logger, confined to two statements — **PASS**

Two `logger.Warn` calls that did not exist before are present, and the reviewer confirmed each is in the
required clause by reading the surrounding control flow:

- Line 96, inside the `else` branch of `catch (TaskCanceledException)` (the branch taken when
  `token.IsCancellationRequested` is false).
- Line 115, inside `catch (TimeoutException)`.

The file's diff is exactly 2 added and 2 removed lines (caller-supplied), which bounds the change to the two
substitutions. Reviewer inspection confirms no change to the deadline window, the retry counter, the
`timeoutSourceFactory` seam, the `TimeProvider` resolution, the exception types caught, the control flow
after each diagnostic, any other catch clause, or any `using` directive. The file now holds five
`logger.Warn` calls: the three pre-existing ones at lines 237, 258 and 331, plus these two.

The implementation matches the four-step trace in "Proposed Fix": the diagnostic that previously reached an
unreadable `DebugTextWriter` now reaches the log4net `logger`.

### AC7 — A named regression test covers the previously uncovered branch and passes — **PASS**

`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs` contains two methods:

| Method | Branch | Outcome |
|---|---|---|
| `GetTableInViewAsync_TimeoutSourceThrowsTimeout_EntersTimeoutCatchAndRetriesOnce` | `catch (TimeoutException)` | Passed |
| `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce` | `else` of `catch (TaskCanceledException)` | Passed |

Entry into the catch clause is genuine rather than asserted at the seam. The injected factory throws on its
first invocation, before `RunWithTimeout` opens its own `try`, so the exception escapes carrying its type
and selects the matching clause. The three assertions are jointly satisfiable only if the catch body ran:
`factoryInvocations == 2` requires the retry inside the catch to construct a second source,
`getTableCalls == 1` shows the first attempt never reached `GetTable`, and the method returning the mocked
table shows the exception was caught rather than propagated.

The bounded retry is asserted. Both methods passed under the coverage run and under the CI-verbatim
`vstest.console.exe ... /EnableCodeCoverage` form. The file contains no `Thread.Sleep`, no `Task.Delay`, no
wall-clock wait and no temporary file (reviewer-verified by full read).

The criterion's final requirement — that reverting the item-2 production edit alone must not be what makes
the test pass or fail — is satisfied. The assertions address exception routing and retry behaviour, neither
of which depends on whether the statement inside the catch body is `Console.WriteLine` or `logger.Warn`. The
test pins the branch; its purpose is to bring the changed lines under coverage. The executor's fail-before
dossier reaches the same conclusion and is correct: a red-then-green run is structurally impossible here
because AC7 itself forbids the property that would make one possible.

### AC8 — The project file gains exactly one line — **PASS**

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` shows exactly one added line and zero removed lines:

```xml
<Compile Include="OutlookObjects\Table\OlTableExtensionsTimeoutDiagnosticsTests.cs" />
```

It is inserted among the sibling `OutlookObjects\Table\` entries, with no existing entry reordered or
reformatted. No other project file in the solution is modified: the 38-path write set contains exactly one
`.csproj` (caller-supplied).

### AC9 — The eight DocID lines are present, in the file's existing format — **PASS**

`BannedSymbols.txt` holds 15 lines. The reviewer read the whole file and confirms all eight required DocIDs
are present exactly once, each prefixed `M:System.Threading.` and each carrying a `;` message naming
`TimeProvider`: `CancellationTokenSource.CancelAfter(System.Int32)`,
`CancellationTokenSource.CancelAfter(System.TimeSpan)`, `CancellationTokenSource.#ctor(System.Int32)`,
`CancellationTokenSource.#ctor(System.TimeSpan)`, `WaitHandle.WaitOne(System.Int32)`,
`WaitHandle.WaitOne(System.TimeSpan)`, `WaitHandle.WaitOne(System.Int32,System.Boolean)`,
`WaitHandle.WaitOne(System.TimeSpan,System.Boolean)`.

The seven pre-existing lines are unchanged, proven by an anchored numstat of 8 added and 0 removed and
confirmed by reading lines 1-7 against the base content.

### AC10 — The DocIDs are proven to resolve, by positive observation, with a working control — **PASS**

This was the criterion most exposed to a false pass and it survives scrutiny.

**Channel.** The Roslyn SARIF error log (`/p:ErrorLog=`), filtering `runs[0].results` on
`ruleId -eq "RS0030"`. This is a positive-observation channel, not an absence check.

**Sites.** All fifteen enumerated sites produced an RS0030 diagnostic, and the reviewer independently
confirmed every site exists at the exact line reported:

| File | Reported RS0030 lines | Reviewer-confirmed |
|---|---|---|
| `UtilitiesCS/Threading/TimeOutTask.cs` | 53, 119, 200, 274, 358, 436, 506, 588, 670, 752 | all ten confirmed |
| `QuickFiler/Controllers/QfcQueue.cs` | 50, 101 | both confirmed |
| `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs` | 295 | confirmed |
| `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | 169 | confirmed (`tokenSource.CancelAfter(25);`) |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 57 | confirmed (`staleToken.WaitHandle.WaitOne(0)`) |

The set difference "re-derived minus observed" is empty for all five files, which is the per-site pass
condition rather than a weaker per-file one.

**Control, in the same run — satisfied.** Three already-banned symbols produced diagnostics in the same
SARIF documents: `ApplicationIdleTimer.cs` (9), `EfcHomeControllerDependencies.cs` (1),
`MailItemInfoTests.cs` (1). The reviewer corroborated each independently against a repository-wide
`DateTime.Now` census, which returns 3 hits in `ApplicationIdleTimer.cs`, 1 in
`EfcHomeControllerDependencies.cs` and 1 in `MailItemInfoTests.cs` — matching the control counts at the
reported files. Because the controls fired in the same run that produced the observations, the channel
demonstrably reports info-level diagnostics and the observation is **not void**.

A further confirmation that the amended ban list was loaded for that run: the fifteen sites are reachable
only through the eight DocIDs this change adds, so their appearance is itself proof.

Observed total was 45 RS0030 diagnostics across the three projects, of which 15 are at the sites under test.
The equality with the predicted 15 is recorded as an observation, correctly not treated as the pass
condition.

### AC11 — RS0030 severity is unchanged, deliberately — **PASS**

`.editorconfig` still contains the exact line `dotnet_diagnostic.RS0030.severity = suggestion`, exactly once
(reviewer-verified at line 555). The anchored diff of `.editorconfig` contains 11 changed lines, all comment
lines in the `BannedApiAnalyzers` block; zero changed lines carry `dotnet_diagnostic.` and zero carry
`.severity`.

**No compensating gate was lowered.** The reviewer ran an exhaustive negative scan across `*.cs`, `*.csproj`,
`*.props`, `*.targets`, `*.globalconfig` and `.editorconfig`:

| Compensating mechanism | Occurrences repository-wide |
|---|---|
| `WarningsNotAsErrors` | 0 |
| `NoWarn` | 0 |
| `#pragma warning disable RS0030` | 0 |
| Second banned-symbols file | 0 (`BannedSymbols.txt` is the only such file in the tree) |
| Path-scoped `.editorconfig` section for RS0030 | 0 |
| `[ExcludeFromCodeCoverage]` added | 0 |
| Coverage threshold or exclusion change | 0 (`coverage.config` and all `.claude/rules/` files absent from the diff) |

Holding the severity at `suggestion` while shipping the reachable subset is the correct resolution of the
epic's authorization, which permitted changing the severity but not breaking the build to obtain the
promotion.

### AC12 — The two documented exclusions hold — **PASS**

Reviewer read the delivered `BannedSymbols.txt` in full:

- `TimeoutAfter` — **0 occurrences.**
- `WaitHandle.WaitOne;` (the parameterless-overload DocID form, carrying no parentheses) — **0
  occurrences.** Every delivered `WaitOne` DocID carries a parenthesised parameter list.

Neither exclusion was added silently and both remain justified in "Proposed Fix".

### AC13 — The tracking comment no longer points at closed work — **PASS**

The BannedApiAnalyzers comment block immediately above `dotnet_diagnostic.RS0030.severity` (lines 545-554)
contains **zero occurrences of `#181`**. It states the promotion precondition inline — that the pre-existing
call sites must be cleared first because toolchain step 3 promotes every warning to a build error — and
correctly names the nullable gate rather than the analyzer gate as the constraint.

The verified textual surface is recorded as `DateTime.Now 53, DateTime.UtcNow 20, Random.Shared 5,
Thread.Sleep 15, Task.Delay 60 = 153`. The arithmetic is correct, and the reviewer independently confirmed
the two largest terms at head: `DateTime.Now` returns 53 occurrences across 26 files and `Task.Delay`
returns 60 across 34 files.

Two `#181` references survive elsewhere in `.editorconfig`, in the third-party-analyzer-severities header
and the naming-preferences header. Neither is the tracking reference AC13 targets and neither is in the
block the criterion scopes.

Non-blocking: the comment is labelled "Verified textual surface, 2026-09-08" while carrying figures
re-measured on 2026-09-09. The parenthetical "(re-measured at implementation time)" discloses this, and
AC13's requirement is that the surface be verified and stated, which it is. Recorded as NB-7.

### AC14 — Full toolchain pass, non-vacuous — **PASS**

All four steps ran in order in a single final pass with **0 restarts**, each exiting 0 and each backed by a
named artifact.

| # | Step | Exit |
|---|---|---|
| 1 | `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .` | 0 / 0 |
| 2 | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 |
| 3 | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 |
| 4 | `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation` (plus a measured `dotnet-coverage` form) | 0 / 0 |

**Non-vacuity is proven, which is the criterion's substance.** Both msbuild steps used `/t:Rebuild`, and
each log carries **0** occurrences of `Skipping target "CoreCompile"` paired with **18** occurrences of
`Task "Csc"`, one per project in the solution. The pairing is what makes the zero meaningful: a zero alone
would also be produced by an empty log. A warm `/t:Build` would have returned exit 0 with `CoreCompile`
skipped on every project, and neither gate could have failed.

The step-1 no-rewrite claim likewise does not rest on the exit code, which is 0 whether or not the formatter
changed anything. It rests on a SHA-256 comparison of 1658 files before and after, index-aligned, with 0
differences.

### AC15 — Coverage obligations met and recorded — **PASS**

**Changed lines.** Neither changed production line lost coverage and both carry a non-zero post-change hit
count:

| Changed line | Baseline | Post-change | Decrease |
|---|---|---|---|
| 96 — `else` branch of `catch (TaskCanceledException)` | 0 | 2 | no |
| 115 — `catch (TimeoutException)` | 2 | 2 | no |

The values are pooled figures that count each line once from `class/lines` and once from `method/lines`, so
one execution reads as 2; the rule was applied identically to both runs. The reviewer corroborated the gain
independently: the `<GetTableInViewAsync>d__32` state-machine class rose from 0.6533 to 0.88 line-rate
(+17 covered lines on a 75-line class), and the whole-file covered count rose 238 → 255, also +17. The two
deltas agree exactly.

**Repository figure.** Read by the reviewer directly from the Cobertura root elements rather than accepted
from the evidence. Every executor-recorded figure matched to seven decimal places.

| Denominator | Line | Branch | Verdict |
|---|---|---|---|
| First-party production packages only (post-commit verification at head) | 85.70% | 79.87% | clears >= 85% and >= 75% |
| All instrumented modules (executor's measured run) | 86.13% | 66.49% | line clears; branch depressed by five vendor packages |

The first-party figure is the policy-conformant one, because `.claude/rules/general-unit-test.md` requires
test files to be excluded from the metric and the executor's run includes nine `*.Test` packages plus
`log4net`, `Mono.Reflection`, `Microsoft.IO.RecyclableMemoryStream`, `System.Linq.Async` and
`System.Interactive`. All nine first-party packages remain in the first-party denominator, including the low
scorers `SVGControl` (47.3%) and `ToDoModel` (58.2%), so nothing first-party is excluded. On both
denominators the post-change line figure is **not lower** than the baseline, which is the criterion's
comparison requirement.

**No weakening.** No coverage exclusion, no threshold change and no `[ExcludeFromCodeCoverage]` attribute was
added anywhere; see the AC11 negative-scan table.

**Item 1 moved no figure,** as predicted: all 33 files compile into `*.Test.dll` assemblies excluded from
instrumentation.

**On AC15's partly false premise.** The criterion asserts both changed lines are uncovered before the change.
That is false for line 115, which feature 825's live test
`GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000` already covered at the base commit. The
plan records this as decision D2 and discharges the criterion with a no-decrease comparison plus a non-zero
post-change requirement. **The reviewer judges that discharge adequate.** AC15's enforceable content is that
the changed lines must not be left newly-touched-but-uncovered; both lines carry non-zero post-change
coverage and neither decreased. The executor correctly did not amend the criterion text — a feature may check
its spec's boxes but never rewrite them — and correctly recorded the divergence in evidence rather than
concealing it.

Non-blocking: no feature artifact reports a branch figure at all, though C# is branch-capable and the rules
set a uniform >= 75% branch floor. The reviewer closed that gap. AC15 itself speaks only of line coverage, so
the criterion is not failed. Recorded as NB-2.

### AC16 — No out-of-scope file is touched — **PASS**

`git diff --name-only` against the merge base lists 38 paths outside the feature folder, matching the spec's
declared write set **exactly in both directions** — nothing missing, nothing extra (caller-supplied,
corroborated by the executor's independent committed-footprint gate which reports 85 total paths decomposing
as 38 write-set paths plus `spec.md`, the plan and 45 evidence artifacts).

Confirmed absent:

- `CLAUDE.md`
- any path under `.claude/` or `.github/`
- `docs/features/epics/review-residuals-2026-09-08/`
- this feature's `issue.md` and its `research/` directory
- the sibling-owned `UtilitiesCS/Threading/TimeOutTask.cs`
- the sibling-owned `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`

The write-set arithmetic reconciles: 33 item-1 test files + `OlTableExtensions.TableAccess.cs` +
`OlTableExtensionsTimeoutDiagnosticsTests.cs` + `UtilitiesCS.Test.csproj` + `BannedSymbols.txt` +
`.editorconfig` = 38.

The naming trap was navigated correctly: `QfcHomeControllerCleanupTests.cs` is absent while the four
`QfcHomeController*` files and two `QfcFormController*` files belonging to the population are present.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/spec.md
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none
```

| AC | Verdict | AC | Verdict |
|---|---|---|---|
| AC1 | PASS | AC9 | PASS |
| AC2 | PASS | AC10 | PASS |
| AC3 | PASS | AC11 | PASS |
| AC4 | PASS | AC12 | PASS |
| AC5 | PASS | AC13 | PASS |
| AC6 | PASS | AC14 | PASS |
| AC7 | PASS | AC15 | PASS |
| AC8 | PASS | AC16 | PASS |

Totals: **16 PASS, 0 PARTIAL, 0 FAIL, 0 unverified.**

## Check-Off Actions Taken

All 16 criteria were already checked `- [x]` in `spec.md` by the executor. The reviewer's evaluation agrees
with every one of them, so **no criterion was unchecked and no edit to `spec.md` was required**. No criterion
text was altered, added or removed by this review.

## Baseline Comparison

| Dimension | Baseline (`dea7b49d`) | Head (`077856c9`) |
|---|---|---|
| Tests passed / failed | 7190 / 0 | 7192 / 0 |
| `Console.SetOut(` occurrences | 38 across 35 files | 2 across 2 files |
| `Console.WriteLine` in the table-access file | 2 | 0 |
| `logger.Warn` in the table-access file | 3 | 5 |
| `BannedSymbols.txt` lines | 7 | 15 |
| RS0030 severity | `suggestion` | `suggestion` (deliberately unchanged) |
| Line coverage, first-party | — | 85.70% |
| Branch coverage, first-party | — | 79.87% |
| Line coverage, all modules | 86.1154% | 86.1329% |
| Table-access file line coverage | 84.70% | 90.75% |

The test-count delta of exactly 2 is the two added methods, which is the regression signal for the 33-file
deletion sweep: removing 34 statements and 11 initializer methods broke no existing test.

## Residual Items

None blocking. Nine non-blocking findings (NB-1 through NB-9) and six carried-forward follow-ups are recorded
in `policy-audit.2026-09-09T20-15.md`; five code-quality observations (CR-1 through CR-5) are recorded in
`code-review.2026-09-09T20-15.md`.

The follow-ups most worth filing as issues at epic close are the RS0030 promotion after the call-site
cleanup, the stale `Directory.Build.props` claim in `CLAUDE.md` §C#1.3, and the two `SVGControl` projects
that do not reference `BannedSymbols.txt`.

## Verdict

**PASS. 0 blocking findings. 16 of 16 acceptance criteria delivered and verified.**

No remediation is required and no `remediation-inputs` artifact is produced.
