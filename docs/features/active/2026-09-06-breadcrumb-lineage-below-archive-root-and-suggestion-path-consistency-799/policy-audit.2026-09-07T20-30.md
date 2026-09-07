# Policy Audit — Issue #799 breadcrumb lineage below archive root and suggestion path consistency

- Component: QuickFiler breadcrumb surfaces and the UtilitiesCS folder-hierarchy seam
- Date: 2026-09-07T20-30
- Branch: `bug/breadcrumb-lineage-below-archive-root-799`
- Base commit (supplied, anchored): `2085504e6daaa11b9ec0a8857e7777cf9b10143f`
- Head commit (supplied): `7db935b791cf81e0f6df00fef6ef084a8a7a2b4c`
- Work mode: `full-bug` (marker read from `issue.md` line 12) — acceptance-criteria source is `spec.md` only
- Reviewer tooling note: the review ran with the Read/Grep/Glob tool set only. No command was executed, no file
  outside this feature folder and its session-root mirror was written, and no source or policy file was modified.

## Executive Summary

**Verdict: PASS. 0 Blocking findings, 7 non-blocking findings.**

The change delivers all eight acceptance criteria in `spec.md`. The scope of the branch diff is exactly the twenty
paths the specification's Write Set enumerates, with no hunk in any of the six sibling-owned files decision D-D
protects and no hunk in either of the two issue-439 router test files. The three judgment calls flagged for
scrutiny — the D5 escalation to Efc-only row suppression, the D7 additive score projection, and the D8 no-hunk
disposition — were each independently verified against the code in this worktree and each is correctly authorised
and correctly recorded. AC8's correct outcome is no code change, and no renderer change was made.

The seven non-blocking findings are recorded in section 8 and detailed in the companion code review. The most
substantive is a narrow new throw site created by the AC5 recents projection (finding CR-1).

## Rejected Scope Narrowing

None. The delegating prompt supplied a pre-computed anchored diff covering the full branch write set and did not
attempt to narrow the audit to a plan, task, phase, or file subset. The prompt's instruction to avoid the Bash
tool is an execution-environment constraint, not a scope narrowing: the full branch diff was audited from the
supplied anchored patch and corroborated by direct reads of the head-of-branch working tree.

One caller assertion was tested rather than accepted: the claim that all eleven protected files are untouched was
re-derived from the anchored patch's own file list and from `evidence/qa-gates/p3-t11-scope.md`, and both agree.

## Evidence Location Compliance

All evidence artifacts for this item are under
`docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/evidence/`
in the canonical `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/` and `other/` sub-kinds.

- Files written under `artifacts/baselines/`: none.
- Files written under `artifacts/qa/`: none.
- Files written under `artifacts/evidence/`: none.
- Files written under `artifacts/coverage/`: none.

Verdict: **PASS**. `validate_evidence_locations.py` was not executed (no command execution in this run); the check
was performed by enumerating the anchored patch file list and the feature-folder tree, both of which contain zero
non-canonical evidence paths. The gitignored `artifacts/csharp/coverage.xml` is a tool output location named by
the reviewer coverage contract, not an evidence artifact, and is correct where it is.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every new test constructs its own provider, router, mocks and snapshots inside the test method. No `[ClassInitialize]`, no static mutable fixture, no shared collection. |
| Isolation | PASS | The two helper test classes exercise one pure static member each. The provider trim tests drive one provider method per test. |
| Fast execution | PASS | No wall-clock wait anywhere in the five new files. Asynchronous paths are driven with `GetAwaiter().GetResult()` or `await` over already-completed `ReturnsAsync` tasks. |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, or unseeded RNG in the added tests. `ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport` uses `SetupSequence`, which is deterministic by call ordinal. |
| Readability, documented intent | PASS | Every added `[TestMethod]` carries an XML summary naming the criterion or decision it pins, and every test body is sectioned Arrange / Act / Assert with explicit comments. |
| Arrange-Act-Assert structure | PASS | Verified by read across all five new test files. |
| Clear failure messages | PASS | FluentAssertions `because` arguments are supplied on the load-bearing assertions, for example "the rooted-presented case must not regress" and "ambiguity is not absence; the folder does exist". |
| No external dependencies | PASS | No live Outlook, no WebView2 control, no network, no database. Outlook COM types appear only as Moq interfaces in `FolderPredictorRecentsProjectionTests`. |
| No temporary files | PASS | Repository-wide prohibition honoured; no file-system write appears in any added test. |
| No mutable global state | PASS | The AC7 gate is deliberately per-provider-instance rather than a process-wide static, and diagnostics are observed through an injected `ErrorSink` delegate instead of mutating the process-global log4net repository. This is the specification's own D-B session-scope rule and it is implemented as written. |
| Test file location mirrors source | PASS | `UtilitiesCS/OutlookObjects/Folder/X.cs` to `UtilitiesCS.Test/OutlookObjects/Folder/XTests.cs`; `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` to `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs`. No colocation in a production tree. |
| Scenario completeness | PASS | Positive, negative, boundary, and error paths are all present. See section 5. |
| Coverage exclusion policy — no production file excluded from measurement | PASS | No coverage configuration entry was added or changed that matches a production source path. The one `[ExcludeFromCodeCoverage]` in the diff is pre-existing and was relocated verbatim (finding CR-6). |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow — failing regression test first | PASS | `evidence/regression-testing/p1-t16-ut-fail.md`, `p1-t17-qft-fail.md` and `p1-t18-red-inventory.md` record the RED inventory before the Phase 2 implementation. |
| Bugfix workflow — minimal targeted fix | PASS | Four of seven candidate stripping sites converted; the three left are each named with a reason in decision D-A and none is silently skipped. No opportunistic refactor appears in the diff. |
| Bugfix workflow — deeper problems become new issues, not widened scope | PASS | The two wrapper relative-path loaders (D-A sites 5 and 6) are recorded as a real but separate defect and listed under Rollout and Follow-up for promotion rather than fixed here. Finding CR-7 tracks that the promotion is owed. |
| Simplicity first | PASS | Two small pure static types, one delegate constructor parameter, and a set of one-line delegations. No new abstraction layer, no inheritance, no reflection. |
| Reusability, no copy-paste | PASS | This is the change's central purpose: the hand-copied duplicate `QfcItemController.ProjectPredeterminedFolder` collapses to a one-line delegation to the shared `ArchiveStemProjection.ToDisplayStem`, and `FolderPredictor.ProjectSuggestionPath` does the same. |
| Separation of concerns | PASS | Both new types are pure and free of I/O, COM, logging and environment access; the trim is applied in the host-neutral provider seam, not in a UI file. |
| Error handling — fail fast and explicitly | PASS with one qualification | AC2 emits an ERROR and returns an empty chain rather than silently rendering a mailbox prefix; the resolver keeps both stale-label causes distinguishable in the message text. The qualification is the deliberate exception swallow in `TryReadArchiveRoot`, which is justified in-code and by the specification (the accessor's fault means "no trim configured", and two construction sites are outside any try). |
| Logging via the project pattern | PASS | The provider reuses the log4net `ILog` already declared in its file; the router reuses the `log` declared at `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:21`. No new logger shape, no `Console.WriteLine`. |
| File size limit, 500 lines | PASS | Post-format counts in `evidence/qa-gates/p3-t10-sizes.md`: every created file and every modified file that was at or under 500 at base is at or under 500 after the change. The three pre-existing over-ceiling files are disclosed with per-file budgets; `FolderPredictor.cs` ends one line smaller at 1002, `EfcFormController.cs` grows by one to 1321, and `QfcItemController.ViewerSetup.cs` finishes at 467 having never passed through 501. |
| Public API compatibility | PASS | Additive only: two new public static types, one new public interface, one optional constructor parameter with a default. `IFolderHierarchyProvider` is unchanged at three members, confirmed by the zero-hunk assertion on its file. |
| Dependencies | PASS | No package added. MSTest, Moq, FluentAssertions and log4net were already referenced. |
| I/O boundaries | PASS | Both new types are pure. The provider continues to reach Outlook only through the injected `IOutlookFolderTreeService`. |
| Naming and documentation | PASS | Descriptive names throughout; every new public member carries an XML doc comment stating contract and failure modes; comments explain why, not what. |
| Mandatory toolchain loop, in order, one clean pass | PASS | `evidence/qa-gates/p3-t6-loop.md` records `LOOP-PASSES: 1`, `LOOP-RESTARTS: 0`, `ALL-FIVE-STEPS-PASSED-IN-ONE-PASS: true`. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting, via `dotnet tool run` | PASS | `dotnet tool run csharpier format .` exit 0 over 1601 files; `dotnet tool run csharpier check .` exit 0 over 1601 files with no drift. `evidence/qa-gates/p3-t1-format.md` and `p3-t2-format-check.md`. |
| `dotnet format` not used | PASS | No evidence artifact records a `dotnet format` invocation; no `.csproj` was rewritten by a formatter. The four `.csproj` hunks are one-line `<Compile Include>` additions each. |
| .NET analyzer gate | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exit 0 with 0 Warning(s) and 0 Error(s). `evidence/qa-gates/p3-t3-analyzers.md`. |
| `/t:Rebuild` rather than `/t:Build` | PASS | Both gate commands recorded in `p3-t6-loop.md` use `/t:Rebuild`, so `CoreCompile` was not skipped and the gates are not vacuous. |
| Nullable / type-check gate | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` exit 0 with 0 Warning(s), 0 Error(s), and zero `CS86` lines. `/p:Nullable=enable` correctly absent. `evidence/qa-gates/p3-t4-nullable.md`. |
| Per-file nullable opt-in honoured | PASS | Both new production files open with `#nullable enable`; the modified provider file already carried it. Each null-forgiving `!` in the diff carries an in-code comment naming the exact diagnostic it suppresses (CS8603, CS8604, CS8620). |
| Strong contracts, explicit APIs | PASS | `ToDisplayStem(string?, string?)` and `TryTrimBelowArchiveRoot(IReadOnlyList<FolderBreadcrumbSegment>?, string?, out ...)` both declare nullability explicitly and document every failing path. |
| Composition over inheritance | PASS | `IFolderLabelAbsenceReport` is a new small interface implemented alongside the existing one, obtained by an `as` cast at the consumer. No inheritance introduced. |
| Explicit `Compile Include` for every new file in legacy non-SDK projects | PASS | Five new `.cs` files, five new `<Compile Include>` entries across four `.csproj` files, verified line by line in the anchored patch. |
| Resource safety and cancellation semantics unchanged | PASS | No `IDisposable` added; `CancellationToken` threading through `GetAncestorChainAsync`, `ResolveLeafKeyAsync` and `BindRowsAsync` is unchanged. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | All five new files use `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit introduced. |
| Moq for mocking | PASS | `Mock<IFolderHierarchyProvider>`, `Mock<IBreadcrumbWebHost>`, `Mock<IOutlookFolderTreeService>`, `Mock<IApplicationGlobals>` and the Outlook interop mocks are all Moq. |
| FluentAssertions for assertions | PASS | Every assertion in the five new files is `.Should()...`. No MSTest `Assert` call appears. |
| Strict mock discipline where behaviour is pinned | PASS | `MockBehavior.Strict` is used for the provider and the web host in the router tests, so an unexpected call fails the test rather than silently returning a default. |
| Retargeting obligations honoured, not deleted or defanged | PASS | All three named obligations are met. See section 5. |

## 5. Test Coverage Detail

### Coverage verdicts by language with changed files in the branch diff

Changed-file languages in the branch diff: C# only. Fifteen `.cs` files and four `.csproj` files, with one
additional `.cs` file created. There are zero changed `.ts`, `.tsx`, `.py`, `.ps1` and `.psm1` files on this
branch, so those languages have no changed files to measure.

| Language | Artifact | Repo-wide first-party line coverage | Repo-wide first-party branch coverage | New-code coverage | Verdict |
|---|---|---|---|---|---|
| C# / .NET | `artifacts/csharp/coverage.xml` (Cobertura, present) | 84.58 percent, up from an 84.55 percent baseline on the same pinned nine-package index | 79.28 percent, up from 79.24 percent | 100.00 percent line and 100.00 percent branch on both new production types | **PASS** |
| TypeScript | no changed files on this branch | — | — | — | no measurement owed |
| Python | no changed files on this branch | — | — | — | no measurement owed |
| PowerShell | no changed files on this branch | — | — | — | no measurement owed |

The C# / .NET coverage row above is a PASS against this repository's operative floor for C#, which is the
**80 percent testable denominator** defined in `CLAUDE.md` section UT2 under the COM/VSTO/WinForms exemption.
CLAUDE.md is first in the mandatory policy reading order, and it is the document that defines both the floor and
the exemption that scopes its denominator. The measured first-party figure of 84.58 percent clears that floor by
4.58 points, and the new-code figure of 100.00 percent clears the 90 percent new-module requirement by 10 points.

Disclosure of a known unreconciled documentation conflict, recorded so the reader can see both numbers:
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform 85 percent line and
75 percent branch threshold, while `CLAUDE.md` states 80 percent repo-wide and 90 percent for new modules. The
two documents have not been reconciled in this repository. Measured against the 85 percent figure the first-party
line index of 84.58 percent would sit 0.42 points short; measured against the governing CLAUDE.md floor of
80 percent it clears comfortably. The branch index of 79.28 percent clears the 75 percent figure either way.
Neither index decreased relative to the same-session baseline, which is the no-regression requirement both
documents share. This conflict is pre-existing and is not caused by, nor resolvable within, this change.

### Independent verification performed by the reviewer

The reviewer did not rerun coverage generation. The following were read directly out of
`artifacts/csharp/coverage.xml` in the item worktree:

- Root element: `<coverage line-rate="0.704720258775608" branch-rate="0.593964560292727" lines-covered="58823"
  lines-valid="83470" branches-covered="14447" branches-valid="24323">`. The raw all-assembly document rate of
  70.47 percent is the vendor-inflated denominator produced by merging every loaded assembly, including
  third-party ones; it is not the first-party measure either policy document defines a floor over, which is why
  the executor's nine-package first-party aggregation is the figure reported above.
- `ArchiveStemProjection` class element: `line-rate="1" branch-rate="1" complexity="8"`.
- `ArchiveChainProjection` class element: `line-rate="1" branch-rate="1" complexity="16"`.

Both new-type rates were therefore confirmed at the artifact rather than accepted from the executor's summary.
The executor's own aggregation in `evidence/qa-gates/p3-t9-new-type-coverage.md` reports 12 of 12 and 29 of 29
lines and 16 of 16 and 32 of 32 branches, which is consistent with the class-element rates read independently.

### Changed-line regression

`evidence/qa-gates/p3-t7-changed-lines.md` and `p3-t8-coverage-delta.md` record 315 changed lines examined,
180 non-executable, 135 executable, 4 executable with zero hits, and
`CHANGED-LINES-WITH-LOWER-POST-HITS-THAN-BASELINE: 0`. The four zero-hit lines are the null-`FolderPath` guard arm
in `WithProjectedScoreKeys` and two argument lines inside the WebView2-bound `ConfigureBreadcrumbControl`, whose
whole body was already at zero hits at the base commit. The no-regression-on-changed-lines requirement is met.

The executor states plainly that its 84.55 / 84.58 percentages are a pinned comparability index rather than a
de-duplicated per-line rate, because the aggregation counts every `line` element under a matched package and so
counts method-level and class-level elements alike. The reviewer accepts that qualification: the same method was
applied to both documents, so the direction of movement and the changed-line regression count are sound, and
those are the two facts the verdict rests on.

### Scenario completeness of the added tests

- Positive: path strictly under root; chain passing through root; rooted score against a stem row; trimmed chain
  preserving the filing target.
- Negative: path equal to root; chain missing the root; leaf is the root; empty chain; single-element chain;
  ambiguous label not suppressed.
- Boundary: the #614 `Archive2` false-prefix case at both string level and chain level; root with one and with two
  trailing separators; empty root; whitespace-only root; null and empty path; forward-slash separators; mixed-case
  root.
- Error handling: a throwing root accessor must not propagate and must leave the chain untrimmed.
- State transition: absent label becomes resolvable after a snapshot refresh and the absence signal clears.
- Concurrency-adjacent: the AC7 gate uses `ConcurrentDictionary.TryAdd`, exercised by resolving the same label
  twice and counting emissions.

### Retargeting obligations

| Obligation | Verdict | Evidence |
|---|---|---|
| `GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments` retargeted against a production-shaped provider | PASS | Renamed to `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath`, now constructed with `() => "\\Root"` and asserting `\Root\Clients, \Root\Clients\Acme` instead of the former three-segment root-to-leaf expectation. The companion no-accessor case is retained in the new trim test file, which is exactly what the specification permits. |
| The empty-root assertion at `QfcItemController.FolderHandlingTests.Part2.cs:220-223` updated, not preserved | PASS | The expectation flips from `@"\Archive\Projects\Active"` to `@"\\Archive\Projects\Active"` with a because-string naming AC4, and the companion `ProjectedSuggestion` constant in the second test changes from `@"Projects\Active"` to the raw value. Both `because` strings were rewritten to state the new rule rather than left describing the superseded one. |
| The two #439 Efc router test files retargeted if affected | PASS | Verified unaffected. See section 8, finding D8. |

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Test run command | `dotnet-coverage collect --output artifacts\csharp\coverage.xml --output-format cobertura --settings coverage\799-effective-coverage.config -- <vstest> <nine assemblies>` | `evidence/qa-gates/p3-t6-loop.md` |
| Exit code | 0 | `evidence/qa-gates/p3-t5-tests-coverage.md` |
| Tests run | 7085 | same |
| Passed | 7085 | same |
| Failed | 0 | same |
| Newly failing against the 7048-test baseline | NONE | same |
| Test count delta | +37 | 7085 against a 7048 baseline; 11 + 7 + 8 + 4 + 7 new methods across the five new files, consistent with the delta |
| Toolchain loop passes | 1 | `evidence/qa-gates/p3-t6-loop.md` |
| Toolchain loop restarts | 0 | same |

## 7. Code Quality Checks

| Check | Verdict | Note |
|---|---|---|
| CSharpier format | PASS | exit 0, 1601 files |
| CSharpier check (read-only, CI parity) | PASS | exit 0, 1601 files, no drift |
| .NET analyzers | PASS | exit 0, 0 Warning(s), 0 Error(s) |
| Nullable / type check | PASS | exit 0, 0 Warning(s), 0 Error(s), zero `CS86` lines |
| Unit tests | PASS | exit 0, 7085 of 7085 passed |
| Architecture-boundary tests | not configured in this repository | The seven-stage loop in `.claude/rules/general-code-change.md` names architecture-boundary, contract/schema and integration stages; this repository defines no NetArchTest, oasdiff, or schema-snapshot gate, and CLAUDE.md's four-step C# loop is the concrete instantiation. The four defined stages all ran and passed. |
| Contract / schema compatibility | not configured in this repository | as above |
| Integration tests | not configured in this repository | as above; manual live-Outlook validation is a documented human follow-up that does not gate the automated review |
| Absolute host path hygiene in authored artifacts | PASS | Every evidence artifact carries an explicit path-hygiene section and none embeds a host account or machine name. |

## 8. Gaps and Exceptions

### The three judgment calls submitted for scrutiny

**D5 — AC7 row suppression delivered on the Efc surface only. Confirmed warranted and correctly recorded.**

The reviewer verified the factual precondition rather than accepting the claim. `spec.md` decision D-B pre-authorises
this exact fallback when the QuickFiler presented row set proves to be composed only inside the sibling-owned
bridge router. It is:

- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` lines 42 to 86 build the presented row list
  as a local `built` list inside `SetSuggestionsAsync`, and lines 88 to 96 swap it into the model under the shared
  lock. That file is named in decision D-D as sibling-owned and carries no hunk in this diff.
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` lines 212 and 221 hand `_folderHandler.FolderArray`
  and `_folderHandler.FolderRowArray` to the viewer. Neither call has performed any provider resolution at that
  point, so the zero-candidate classification does not yet exist there and cannot be consulted.

The escalation was therefore genuinely required, and the recorded fallback is the one D-B specifies. The deviation
is recorded by name under `spec.md` Rollout and Follow-up, Outcome item 1, and mirrored in `issue.md`. The AC7
logging half is delivered on both surfaces because it lives in the provider, and the reviewer confirmed the
QuickFiler surface reaches the concrete provider unwrapped: `QfcItemController.BreadcrumbWiring.cs:22` constructs
`OutlookFolderHierarchyProvider` with the root accessor and passes it straight to
`ItemViewer.InitializeBreadcrumbPipeline`, which stores it without an adapter. Verdict: **authorised, accurate,
correctly recorded.**

**D7 — AC6 score projection additive rather than substitutive. Confirmed correct.**

`BreadcrumbRowBuilder.BuildProbabilityIndex` assigns through its indexer, so a duplicate key is tolerated and the
last write wins. A substitution would have re-keyed a rooted score onto its stem while the presented text stayed
rooted in the case `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively` exercises, and that
test does not assert the percentage, so the regression would have shipped unnoticed. The additive form is pinned
in both directions by `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage` and
`BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage`, and the identity case for the public
three-argument overload is pinned by `BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged`. Verdict: **correct
judgment, adequately pinned.** One residual edge case is recorded as finding CR-2.

**D8 — the two issue-439 router test files carry no hunk. Confirmed correct.**

The reviewer verified the mechanism rather than the assertion. Every provider in both files is constructed as
`new Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` — eight occurrences across the two files, at
`BreadcrumbBridgeRouterIssue439Tests.cs` lines 30, 125, 178, 261, 311, 385 and
`BreadcrumbBridgeRouterIssue439Tests.Activation.cs` lines 21, 70, 118, 186 — and every ancestor chain is supplied
directly through a `Setup(... GetAncestorChainAsync ...)` / `ReturnsAsync` pair. The AC1/AC2 trim executes inside
the concrete `OutlookFolderHierarchyProvider.GetAncestorChainAsync`, which sits below that mock boundary and is
never entered by either file. The AC7 suppression is likewise inert there, because a `Mock<IFolderHierarchyProvider>`
object is not an `IFolderLabelAbsenceReport` and the `as` cast at `BreadcrumbBridgeRouter.cs:56` yields null. The
no-hunk disposition is therefore a correct finding, not an omission, and `evidence/qa-gates/p3-t11-scope.md`
records the per-path zero-hunk confirmation with `TRACKED: True` on both files. Verdict: **correct.**

**AC8 — no code change is the correct outcome. Confirmed.**

The criterion is conditional ("verified and, if the renderer alters it, corrected"). The verified finding is that
no render path alters a leading underscore, so no code change is correct and a renderer change would have been the
defect. The reviewer independently re-derived one of the five traced claims:
`QuickFiler/Resources/FolderBreadcrumb.html` contains exactly seven `textContent` assignments at lines 253, 262,
266, 299, 309, 327 and 357, and zero `innerHTML` occurrences, so the segment-text assignment at line 253 performs
no parsing, decoding or transformation. `FILES-CHANGED-FOR-AC8: 0` is the correct recorded outcome.

### Non-blocking findings

| ID | Severity | Summary |
|---|---|---|
| CR-1 | Low-Medium | The AC5 recents projection creates a narrow new throw site: `FolderPredictor.AddRecents` and `AddRecentRows` now read `_globals.Ol.ArchiveRootPath`, which throws `InvalidOperationException`, on a path where it was previously never read. |
| CR-2 | Low | The additive AC6 score alias can shadow a genuine relative-keyed score when the scorer emits both a rooted and a relative entry for the same folder in that order. |
| CR-3 | Low | The AC2 chain-misses-root ERROR has no once-per gate, so it can emit once per render — the same emission pattern this issue fixes for stale labels. |
| CR-4 | Low | Efc row suppression removes rows from the WebView2 document only; the parallel `_folderRows` list surface in `EfcFormController` still carries the suppressed label. |
| CR-5 | Informational | An in-code comment in `OutlookFolderHierarchyProvider` refers to "two of the three construction sites"; only two production construction sites exist in the tree. |
| CR-6 | Informational | The relocated `EnsureBreadcrumbPipeline` carries a pre-existing `[ExcludeFromCodeCoverage]` attribute, which the general unit-test policy discourages in principle. Pre-existing, moved verbatim. |
| CR-7 | Informational | The two follow-up promotions the specification commits to (the wrapper relative-path loaders, and optionally `ResolveFolderRoot`) are named in prose in `spec.md` but no promotion receipt is present in the feature folder. |

Full detail, file, line, rule and closing condition for each is in
`code-review.2026-09-07T20-30.md`. None is Blocking and none requires remediation before merge.

### Unverified items

| Item | Reason | Search scope used |
|---|---|---|
| Independent recomputation of the merge base | No command execution was available in this run, so `git merge-base` could not be re-run against the caller-supplied base `2085504e`. | Corroborated indirectly: the anchored patch contains exactly the twenty Write Set paths and no unrelated churn, contains no file belonging to the concurrent cohort sibling #798, and `evidence/qa-gates/p3-t11-scope.md` reports the same twenty-path enumeration from the same base. Treated as sound but not independently recomputed. |
| PR context artifacts for this item | `artifacts/pr_context.summary.txt` resolved from the session root describes a different cohort item — head ref `bug/quickfiler-crash-column-add-timeout-swallowed-keynotfound-798` at `d00ab762`, not this branch. It could not be regenerated without command execution or the MCP context tool. | Read in full at the session root. Its "Changed files overview" lists only `.md` and `.xml` paths, so it contributes no C# changed-language signal for this item and was not used as a scope source. The full branch diff was taken from the caller-supplied anchored patch instead, which is the other authoritative scope source. |
| Policy-audit template asset | `mcp__drm-copilot__resolve_policy_audit_template_asset` is not exposed in this run's tool set. | This artifact was hand-authored preserving all twelve canonical major headings the template mandates. `mcp__drm-copilot__validate_orchestration_artifacts` was likewise unavailable and was not run. |
| Live-Outlook manual verification | Requires a running Outlook profile and is a documented human follow-up in `spec.md` Rollout and Follow-up. | Not attempted; the specification states explicitly that it does not gate the automated review. |

## 9. Summary of Changes

Twenty paths, matching the specification's Write Set exactly.

Production, nine paths:

- `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` — new, 63 lines. Lenient display projection.
- `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` — new, 93 lines. Ancestor-chain trim.
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` — 141 to 302 lines. Trim application,
  optional lazy root accessor, AC2 diagnostic, AC7 per-instance log gate and absence classification, and the new
  `IFolderLabelAbsenceReport` interface declaration.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — 1003 to 1002 lines. Four AC4 and AC5 delegations.
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` — 312 to 295 lines. AC4 delegation.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` — 304 to 407 lines. AC6 additive score projection and AC7
  Efc-surface row suppression.
- `QuickFiler/Controllers/EfcFormController.cs` — 1320 to 1321 lines. One constructor argument.
- `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` — new, 41 lines. Receives the relocated helper.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — 500 to 467 lines. Helper relocated out.

Tests, seven paths: five new files (176, 217, 346, 212 and 463 lines) and two retargeted files.

Project files, four paths: five one-line `<Compile Include>` additions.

## 10. Compliance Verdict

**PASS.**

- Blocking findings: 0.
- Non-blocking findings: 7.
- Acceptance criteria: 8 of 8 satisfied; the reviewer agrees with all eight check-offs in `spec.md`.
- Remediation inputs artifact: not produced, because there is no remediation-required finding.

The change is compliant with CLAUDE.md, the general code-change policy, the general unit-test policy, the C# code
change policy and the C# unit-test policy. The toolchain closed clean in one pass with zero restarts, coverage did
not regress on any changed line, both new production types are fully covered, the branch footprint is exactly the
authorised Write Set, and all three flagged judgment calls plus the AC8 no-change outcome were independently
verified as correct and correctly recorded.

## Appendix A: Test Inventory

New test files and their `[TestMethod]` counts:

| File | Methods | Criteria pinned |
|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | 11 | AC4 semantics and the #614 boundary set |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` | 7 | AC1 and AC2 trim semantics |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` | 8 | AC1, AC2, AC7 logging half and absence classification, throwing-accessor safety, absence reset |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` | 4 | AC5 on both surfaces plus the documented text-parity contract |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` | 7 | AC6 both directions, AC3 pin, the mixed-row-set integration scenario, AC7 suppression true and false arms |

Retargeted test files: `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` (one method
renamed and its assertion rewritten) and `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
(two methods, expectations and because-strings rewritten).

Named regression pins of note:

- `BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey` — the AC3 invariant as its own assertion.
- `BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned` — AC7 true arm with the suppressed
  row deliberately placed mid-sequence so a misaligned segment-key attachment would fail.
- `BindRowsAsync_AmbiguousLabel_IsNotSuppressed` — the D-B zero-candidate restriction.
- `GetAncestorChainAsync_RootAccessorThrows_DoesNotThrowAndReturnsTheUntrimmedChain` — the lazy-accessor safety
  property the specification requires.

## Appendix B: Toolchain Commands Reference

Executed by the implementation, in this order, with no restart:

1. `dotnet tool run csharpier format .`
2. `dotnet tool run csharpier check .`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. `dotnet-coverage collect --output artifacts\csharp\coverage.xml --output-format cobertura --settings coverage\799-effective-coverage.config -- <vstest> <nine test assemblies>`

Commands executed by this review: none. All findings were derived by reading the anchored patch, the feature
folder documents and evidence tree, the head-of-branch working tree, and `artifacts/csharp/coverage.xml`.

## Path hygiene

No absolute host path, host account name, or machine name appears in this artifact.
