# Policy Compliance Audit — Issue #614 (efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary)

- Artifact timestamp: 2026-08-26T22-12 (UTC, matching the convention used by this feature's cycle-1 evidence tree)
- Review cycle: remediation cycle 1 **exit** re-audit. The prior-cycle record is the `2026-08-26T16-55` artifact set, which is left in place unmodified.
- Reviewer: feature-review agent
- Component: `UtilitiesCS`, `QuickFiler`, `TaskMaster` (VSTO add-in, .NET Framework 4.8.1)
- Branch: `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
- Base branch (resolved): `main`
- Merge-base SHA: `c279d40bddacdba00c29a9724d1b5b17f9ebbc90` (recomputed by this reviewer with `git merge-base main HEAD`; matches the caller-supplied value)
- Head SHA: `b45e2a2d5b7f4d4219aa0caea4e63e24777feab1` (25 commits ahead; working tree clean)
- Work Mode: `full-bug` (marker `- Work Mode: full-bug` read from `issue.md:15`) — acceptance-criteria source is `spec.md` only
- Scope: full branch diff against the resolved base branch — 111 files, +10581 / -243

## Template Provenance

`policy-audit-template-usage` and `feature-review-workflow` require resolving the artifact template
through the MCP tool `mcp__drm-copilot__resolve_policy_audit_template_asset` with the `template`
selector. No MCP tool is exposed to this agent session, and no policy-audit template asset file
exists anywhere in this checkout (`find . -iname "*policy-audit*template*"` returns only skill
directories, no asset). The canonical major section set named by the skill is therefore reproduced
from the skill text and from this feature's own validated `2026-08-26T16-55` artifact. This
substitution is recorded here rather than marking the artifact BLOCKED, because every canonical
section below is populated with independently measured evidence.

## Executive Summary

This is the exit re-audit of remediation cycle 1. The cycle was opened by the orchestrator, which
overrode the prior review's non-blocking disposition of findings CR-1 and CR-2 and promoted both to
blocking. Four commits were added since the prior review head `02092504`; one of them, `cbad2da2`,
changes production code, touching exactly two files (`QuickFiler/Controllers/EfcSelectionGuard.cs`,
`QuickFiler/Controllers/EfcFormController.cs`).

The full four-step toolchain was re-executed independently by this reviewer at head `b45e2a2d` and
passed in the mandated order. All 6111 tests in the three test assemblies this change touches pass.

**CR-1 is resolved.** The minimum-length rule is removed from the filing predicate and confined to a
new folder-creation predicate; filing to a one- or two-character archive folder works again, pinned
by two new regression tests.

**CR-2 is not substantively resolved, and the chosen remedy introduces a new, more severe failure
mode.** The remedy widened `IsValidFilingSelection` to accept a rooted value that resolves against
the archive root, but did not normalize that value anywhere on the path between the guard and the
filing boundary. `EmailFilerConfig.ResolvePaths` still calls
`ArchiveStemContract.RequireArchiveRelativeStem`, which throws `ArgumentException` for any rooted
value. The archive-root-exact case is production-reachable through
`FolderPredictor.ProjectSuggestionPath`. The user-visible outcome for that value class therefore
moved from a benign "Please select a valid folder." dialog (prior head) to an unhandled
`ArgumentException` raised after `_formViewer.Hide()` and rethrown out of the `async void`
`ButtonOK_Click` handler. That is a new crash on the exact path-representation chain issue #614 was
filed to close. It is recorded as **Blocking** finding RC-1.

One coverage FAIL is recorded: repository-wide line coverage measured from the canonical artifact is
84.8790%, below the 85% floor. The shortfall is pre-existing and this branch improves it by
+0.0993 points against the merge base. That FAIL is dispositioned non-blocking and is not the reason
remediation is triggered.

Three PARTIAL results are recorded: AC16 no longer matches its criterion text; the file-size ceiling
under a reinterpreted reading; manual validation not executed.

**Overall verdict: FAIL — 1 blocking finding (RC-1). Remediation cycle 2 is required.**

## Rejected Scope Narrowing

The caller prompt supplied a base branch, a merge-base SHA, an active feature folder, an AC source,
a coverage-artifact pointer, and a summary of what changed since the prior review. It explicitly
stated "Scope determination is yours" and "determine for yourself whether CR-1 and CR-2 are actually
resolved."

Two caller statements were examined for scope-narrowing effect and are recorded verbatim:

> "CR-3, CR-4, and every Minor finding from your prior review were explicitly held OUT of that
> cycle's scope and are unchanged. The repo-wide coverage shortfall and the unexecutable
> live-Outlook AC26 steps were likewise held out."

This describes what the *previous* cycle's remediation plan chose to act on. It is a statement about
that plan's scope, not an instruction narrowing this audit. This reviewer nevertheless re-verified
every carried-forward finding at the current head rather than accepting it as unchanged, and
re-measured every coverage figure from the artifacts rather than accepting the summary. No language,
no file, and no check was excluded from this audit on the basis of that statement.

> "`artifacts/csharp/coverage.xml` has been regenerated for THIS head and reports LINE 54000/63620
> and BRANCH 12752/16172. Measure and interpret it yourself."

The figures were verified independently and are accurate (see § 5). The caller stated no verdict;
the verdict derived below is this reviewer's.

**No scope narrowing was detected and none was applied. The audit covers the full feature-vs-base
diff at head `b45e2a2d`, not the remediation plan's two-file subset.**

## PR Context Artifacts

Both artifacts existed but were **stale**: `artifacts/pr_context.summary.txt` recorded
`Head ref (resolved): 02092504e50ede2527ae35f14629f0bc4c4c94ff`, four commits behind the current
head. Per `pr-context-artifacts`, both were regenerated against the resolved base branch before the
audit proceeded.

The drm-copilot `collect_pr_context` MCP tool is not exposed to this session, so both were
hand-authored from git plumbing (`git diff --numstat c279d40b..b45e2a2d`, `git diff --shortstat`,
`git diff`), using the canonical `- <path> (+N/-N)` bullet shape for every one of the 111 changed
paths. That shape avoids the recurring collector defect in which `.cs` files are dropped from the
"Changed files overview" bucket and the coverage validator then detects zero changed languages.

Verified by dot-sourcing `.claude/hooks/validate-feature-review-coverage.ps1` and calling
`Get-ChangedLanguageSet` against the regenerated summary: the enumerated changed-language set is
exactly `CSharp`. `Get-LanguageRepoCoverage -Language CSharp` returns `84.88` and
`Get-LanguageBranchCoverage -Language CSharp` returns `78.85`.

One correction was made to the prior summary while regenerating: it reported "22 changed (10
production, 12 test)" `.cs` files. Direct enumeration returns **21** (10 production, 11 test). The
corrected figure is used throughout this audit.

## Evidence Location Compliance

`validate_evidence_locations.py` does not exist in this repository. The equivalent scan was run
directly against the full branch diff, filtering the changed-path list for the four prohibited
prefixes `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` and `artifacts/coverage/`.

Result: **no matches**. All 47 evidence artifacts on this branch are under the canonical
`docs/features/active/<feature>/evidence/{baseline,remediation-baseline,regression-testing,qa-gates,issue-updates,other}/`
tree. The remediation cycle added `evidence/remediation-baseline/` and further
`evidence/regression-testing/` and `evidence/qa-gates/` files, all canonical.

Verdict: **PASS**. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record was required.

This reviewer wrote no evidence artifact of its own; all measurements are recorded inline in this
audit and in the companion code-review and feature-audit artifacts.

## modified-workflow-needs-green-run

The changed-path list contains zero paths matching `.github/workflows/**`, `.github/actions/**`, or
`scripts/benchmarks/**`. The rule does not fire, and no Blocking finding is emitted under it.
(`scripts/feature-review/Test-ModifiedWorkflowNeedsGreenRun.ps1` does not exist in this repository;
the trigger-path check was performed directly.)

## 1. General Unit Test Policy Compliance

| Rule | Verdict | Evidence |
| --- | --- | --- |
| Independence | PASS | The 23 tests in `EfcSelectionGuardTests` share no mutable state; the two resolver tests build their delegates inline. Full-assembly run in a single vstest process, 0 failures. |
| Isolation | PASS | Each test targets exactly one predicate call or one resolver call. |
| Fast execution | PASS | 6111 tests in 38.9 s wall clock across three assemblies. |
| Determinism | PASS | Reviewer grep across all new and modified test files for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random`, `Path.GetTempPath`, `Path.GetTempFileName`, `Environment.SetEnvironmentVariable`, `File.WriteAllText`: zero hits. |
| Readability | PASS | Every new test carries an explicit Arrange / Act / Assert comment; the CR regression tests name the finding they pin. |
| No temporary files | PASS | Zero temp-file API hits in the diff. |
| Test file location | PASS | All new test files are in the mirrored `*.Test` project trees; none is colocated with production source. |
| Coverage exclusion policy | PASS | No production source path is excluded from measurement. `TaskMaster.runsettings` was inspected and carries no production-path exclusion. |
| Scenario completeness | PARTIAL | Positive, negative, boundary, and error scenarios are present for both predicates and the resolver. The gap is at the composition level: no test exercises a value that the filing predicate **accepts** through to `EmailFilerConfig.ResolvePaths`, which is exactly where RC-1 hides. See § 8 G1. |

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
| --- | --- | --- |
| Simplicity first | PASS | The remediation splits one predicate into two named predicates plus a four-line resolver. No indirection was added beyond the delegate seam. |
| Reusability | PASS | Both predicates and the resolver live in one `internal static` guard class consumed from three call sites. |
| Extensibility | PASS | No public API surface changed. `EfcSelectionGuard` remains `internal`. |
| Separation of concerns | PASS | The predicates are pure; `ResolveArchiveRootOrEmpty` takes `Func<string>` and `Action<string>` seams so no COM or logging type crosses into the guard. |
| Fail fast and explicitly | PARTIAL | `ResolveArchiveRootOrEmpty` narrowly catches `InvalidOperationException` and degrades to an empty root. The catch is narrow, documented, and the diagnostic is logged before returning. However it guards only one of the nine `ArchiveRootPath` reads in `EfcFormController`; two of the unguarded reads (`:777`, `:787`) execute after `_formViewer.Hide()`. See code-review RC-3. |
| Error handling not silent | PASS | The degrade path logs `RootUnavailableDiagnostic` through the injected sink before returning empty. |
| Logging pattern | PASS | `logger.Error(message)` via the existing log4net logger; no ad-hoc console output. |
| Naming | PASS | `IsValidFilingSelection`, `IsValidCreationSelection`, `ResolveArchiveRootOrEmpty`, `MinimumCreationLength` are descriptive and behaviour-named. |
| Comment *why*, not *what* | PASS | Both predicates carry XML documentation naming the finding (CR-1, CR-2) and the reason the rule lives where it does. |
| File size limit (500 lines) | PARTIAL | Three files exceed the ceiling: `EfcFormController.cs` 1079, `BreadcrumbBridgeRouter.cs` 596, `BreadcrumbBridgeRouterIssue439Tests.cs` 694. All three are pre-existing; measured against the merge base (1084 / 596 / 694) none has grown. `EfcFormController.cs` grew 7 lines during this cycle (1072 to 1079) and remains 5 lines below its merge-base value and below the 1084 gate the remediation inputs set. |
| Dependencies | PASS | No new package. The `log4net 3.3.2` reference added to `QuickFiler.Test` matches the version already used by `QuickFiler` and `UtilitiesCS`, and `QuickFiler.Test/app.config` carries the matching `0.0.0.0-3.3.2.0` to `3.3.2.0` redirect. |
| I/O boundaries | PASS | No new filesystem, network, or COM access in the changed production code. |
| Bugfix workflow (failing test first) | PASS | `evidence/regression-testing/cr1-expect-fail.2026-08-26T21-46.md` and `cr2-expect-fail.2026-08-26T21-56.md` both record EXIT_CODE 1 with `ExpectedExitCode: 1` and quote the exact failing assertions; `cr1-pass-after` and `cr2-pass-after` record the green runs. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Rule | Verdict | Evidence |
| --- | --- | --- |
| CSharpier formatting | PASS | `dotnet tool restore` (csharpier 1.2.6 pinned by `dotnet-tools.json`) then `dotnet tool run csharpier check .` returned `Checked 1530 files in 4013ms`, exit 0. |
| .NET analyzer diagnostics | PASS | Analyzer rebuild exit 0. The only warnings emitted are the pre-existing `System.Reactive.PackagesConfigCheck` notices on five projects, present identically at the merge base. |
| Nullable / type checking | PASS | Nullable rebuild exit 0, zero `error` lines. `/p:Nullable=enable` was **not** added and `/t:Build` was **not** substituted for `/t:Rebuild`. |
| Per-file `#nullable enable` | PASS | `EfcSelectionGuard.cs:1` carries `#nullable enable`; both predicates annotate their parameters `string?`. |
| Strong contracts / explicit APIs | PASS | Both predicates take explicit `string?` and return `bool`; the resolver's two delegate parameters are explicitly typed. |
| Composition over inheritance | PASS | Static guard class; no inheritance introduced. |
| Broad catch avoidance | PASS | The single new `catch` names `InvalidOperationException` specifically; every other exception propagates. The analyzer run raises no CA1031 diagnostic. |
| XML documentation on non-obvious behaviour | PASS | All three new members carry XML documentation stating the rule, the rationale, and the finding reference. |
| Minimal public surface | PASS | All new members are `internal`. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Rule | Verdict | Evidence |
| --- | --- | --- |
| MSTest framework | PASS | `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` throughout `EfcSelectionGuardTests`. |
| FluentAssertions | PASS | Every assertion uses `.Should()`; most carry a `because` reason string. |
| Moq for mocking | PASS (documented deviation) | `EfcSelectionGuardTests` deliberately uses no Moq and states why in its class-level documentation: the predicates are pure and the resolver takes inline delegate seams, so no collaborator requires mocking. Moq is used where collaborators exist (`AppOlObjectsArchiveRootValidationTests` uses `Mock<IOlObjects>`). |
| Arrange-Act-Assert | PASS | Explicit in every new test. |
| Descriptive intent | PASS | Names encode member, input class, and expected outcome; the regression tests name CR-1 / CR-2 in their comments. |

## 5. Test Coverage Detail

The coverage artifact was inspected, not regenerated, per the review contract.

- Artifact: `artifacts/csharp/coverage.xml`. Root element `<report name="TaskMaster">` — a JaCoCo-shaped summary of the post-change filtered Cobertura output, not raw Cobertura.
- Reviewer re-summed the nine `<package>` LINE counters: covered **54000**, missed **9620**, valid **63620**.
- Reviewer read the single BRANCH counter: covered **12752**, missed **3420**, valid **16172**.
- The caller-supplied figures (LINE 54000/63620, BRANCH 12752/16172) are confirmed exactly.
- Cross-check: `Get-JacocoRepoCoverage` and `Get-JacocoBranchCoverage` from `.claude/hooks/validate-feature-review-coverage.ps1`, dot-sourced and invoked directly, return `84.88` and `78.85`.
- Merge-base baseline measured independently by this reviewer from `coverage/coverage.cobertura.filtered.p0-t9.xml` (the delivery cycle's pre-change run): line-rate `0.847797` (53769 / 63422), branch-rate `0.786938` (12676 / 16108).

### Language coverage verdicts

C# repo-wide line coverage verdict: **FAIL** — 84.8790% (54000 / 63620) is 0.1210 points below the 85% floor.

C# repo-wide branch coverage verdict: **PASS** — 78.8523% (12752 / 16172) clears the 75% floor by 3.8523 points.

C# new-code line coverage verdict: **PASS** — all three files introduced by this branch measure 100.0000% line.

C# new-code branch coverage verdict: **PASS** — 100.0000%, 100.0000% and 90.0000%, all clear of the 75% floor.

C# changed-line coverage verdict: **PASS** — no changed line lost coverage relative to the merge-base baseline; per-file covered counts are unchanged or higher in every modified production file.

C# modified-file line coverage verdict: **FAIL** — five of seven modified production files sit below the 85% line floor. All five were below it at the merge base as well, four of the five improve, and the fifth loses no covered line.

C# modified-file branch coverage verdict: **FAIL** — `EmailFilerConfig.cs` branch coverage moves 70.0000% to 60.0000% because the new `GetStem` ternary's out-of-ancestor arm has no test. This is the only measurable branch decrease attributable to added code.

| Language | Changed files on branch | Artifact state | Repo-wide line | Repo-wide branch | Verdict |
| --- | ---: | --- | ---: | ---: | --- |
| C# | 21 `.cs` | present and parsed | 84.8790% | 78.8523% | FAIL on repo-wide line, PASS on repo-wide branch |
| TypeScript | 0 | no artifact expected | — | — | zero changed files, nothing to evaluate |
| Python | 0 | no artifact expected | — | — | zero changed files, nothing to evaluate |
| PowerShell | 0 | no artifact expected | — | — | zero changed files, nothing to evaluate |

#### Disposition of the repository-wide line FAIL

Recorded honestly and dispositioned **non-blocking**, on these grounds:

1. The shortfall is **pre-existing**: the merge-base figure, measured by this reviewer on the same machine with the same counting method, is 84.7797%.
2. This branch **improves** the figure by **+0.0993** points and improves the branch figure by **+0.1585** points. It moves the repository toward the floor, not away from it.
3. The remediation cycle alone contributed +0.0078 points (84.8712% to 84.8790% on the executor's same-session pair), so the cycle did not erode the delivery cycle's gain.
4. Closing a 0.12-point gap requires covering roughly 77 additional lines in code this defect fix does not touch. Requiring that inside a bug fix would be an unrelated refactor.
5. A standing repository contradiction is noted: `CLAUDE.md` § UT2 states a `>= 80%` repo-wide floor while `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state `>= 85%`. This audit applies the stricter 85% figure, which is also what the local validator enforces.

This FAIL is **not** the reason remediation is triggered. Remediation cycle 2 is triggered by
blocking code-review finding RC-1.

#### Disposition of the modified-file FAILs

Also dispositioned **non-blocking**, with one exception noted below. Per-file measurements were
computed by this reviewer directly from the Cobertura artifacts, deduplicating `<class>` nodes by
`filename`, taking the maximum `hits` per line number and the maximum condition-coverage numerator
per line number, so a type appearing in more than one `<class>` node is not double-counted:

| Modified production file | Line (merge base to head) | Branch (merge base to head) | Movement |
| --- | ---: | ---: | --- |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 97.8723% to 97.8552% | 93.5484% to 94.4444% | 3 covered lines deleted alongside 3 valid lines; uncovered count unchanged at 8 |
| `QuickFiler/Controllers/EfcDataModel.cs` | 49.6000% to 52.7132% | 39.1304% to 43.4783% | improved |
| `QuickFiler/Controllers/EfcFormController.cs` | 11.2344% to 11.2971% | 4.9020% to 5.2083% | improved |
| `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | 58.8235% to 69.0355% | 66.6667% to 83.9286% | improved substantially |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 33.3333% to 32.7189% | 22.4138% to 20.9677% | covered count unchanged at 71; 4 instrumented lines added in the COM-bound getter |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` | 93.7500% to 94.3548% | 70.0000% to 60.0000% | line improved; branch decreased by the new `GetStem` ternary |
| `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | 98.4375% to 99.0654% | 97.3684% to 96.4286% | line improved; branch dilution from 18 added branch points, 16 of them covered |

`EfcFormController.cs` and `AppOlObjects.cs` are COM/WinForms-bound classes covered by the ratified
`CLAUDE.md` § UT2 exemption for VSTO lifecycle and Outlook Interop event-handler classes without an
injectable seam. `EfcDataModel.cs` is partially seam-bound. `AppFileSystemFolderPaths.cs` is
environment-bound. None of the five sub-floor files regressed a covered line.

The one item worth acting on is the `EmailFilerConfig.GetStem` branch decrease, which is a genuine
untested new code path rather than a denominator artefact. It is recorded as Minor finding RC-4 in
the code review and is not blocking.

### New-code coverage (gate `>= 85%` line, `>= 75%` branch; reviewer-measured)

| New file | Line | Branch | Verdict |
| --- | ---: | ---: | --- |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` | 100.0000% (51/51) | 100.0000% (28/28) | PASS |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | 100.0000% (31/31) | 100.0000% (14/14) | PASS |
| `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` | 100.0000% (20/20) | 90.0000% (9/10) | PASS |

`EfcSelectionGuard.cs` grew from 9 instrumented lines to 31 during the remediation cycle and remains
fully covered, including both arms of `ResolveArchiveRootOrEmpty`. Placing the resolver in the guard
rather than inline in `ActionOkAsync` is what makes its `catch` arm unit-reachable; that was a sound
design choice.

### Changed-line measurement and no-regression

- The remediation added 7 executable lines at `EfcFormController.cs:708-712` and 1 at `:1045`. All 8 are uncovered. Every line they replaced was uncovered at the merge base as well, so no changed line lost coverage.
- The uncovered lines are WinForms UI event glue: `ActionOkAsync` reads `SynchronizationContext.Current`, drives `_formViewer`, and calls `MessageBox.Show`, none of which is reachable headless. The logic they wire is at 100% through the extracted predicates.
- **Caveat recorded:** the "the logic they wire is fully tested" argument holds for each predicate in isolation. It does not hold for the *composition* of the predicate with the downstream filing boundary, which is where RC-1 lives. 100% line coverage on `EfcSelectionGuard.cs` is therefore not evidence that the OK path is correct.

## 6. Test Execution Metrics

Independently re-executed by this reviewer against head `b45e2a2d`, after the analyzer and nullable
rebuilds, using the `Extensions\TestPlatform` vstest binary (which preserves the Moq binding
redirect) with `/InIsolation` and `/Settings:TaskMaster.runsettings`:

| Metric | Value |
| --- | ---: |
| Total tests | 6111 |
| Passed | 6111 |
| Failed | 0 |
| Skipped | 0 |
| Exit code | 0 |
| Duration | 38.9 s |

The count moved from 6093 at the prior review head to 6111, matching the 18 net new tests added to
`EfcSelectionGuardTests` by the remediation. No pre-existing flake (#594 / #592 / #586 / #584) was
observed in this run.

The executor's own final gate reports 6587/6587/0 across the whole solution
(`evidence/qa-gates/final-test-coverage.2026-08-26T22-30.md`); this reviewer measured the three
assemblies the change touches, which is the blast-radius-relevant subset.

## 7. Code Quality Checks

| Check | Exit | Verdict |
| --- | ---: | --- |
| Formatting (`csharpier check`) | 0 | PASS — 1530 files checked, 0 unformatted |
| Analyzers (`/t:Rebuild` with `EnableNETAnalyzers` and `EnforceCodeStyleInBuild`) | 0 | PASS — 0 errors; only pre-existing System.Reactive packages.config warnings |
| Type check / nullable (`/t:Rebuild` with `TreatWarningsAsErrors`) | 0 | PASS — 0 errors, 0 `CS86xx` |
| Tests (3 assemblies) | 0 | PASS — 6111/6111 |
| Non-vacuity | — | PASS — both MSBuild steps used `/t:Rebuild` and each produced 18 assembly outputs, so `CoreCompile` was not skipped |

Exact command text is in Appendix B.

## 8. Gaps and Exceptions

### G1 — Blocking. The CR-2 remedy widens the guard without normalizing the value (code-review RC-1)

`EfcSelectionGuard.IsValidFilingSelection(selection, archiveRoot)` now returns `true` for a rooted
value that resolves against the archive root (`EfcSelectionGuard.cs:75-81`), pinned by two new
tests: `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` and
`IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted` (`EfcSelectionGuardTests.cs:120-141`).
The value itself is then passed on unchanged:

- Sort path: `ActionOkAsync:722` to `EfcHomeController.ExecuteMovesCoreAsync` (reads `_formController.SelectedFolder` verbatim) to `EfcDataModel.MoveToFolderAsync(string, ...)` to `EmailFilerConfig.DestinationOlStem = folderpath` (`EfcDataModel.cs:286`) to `EmailFiler.SortAsync` to `EmailFilerConfig.ResolvePaths(Folder)` to `ArchiveStemContract.RequireArchiveRelativeStem` — which **throws `ArgumentException`** for any rooted value (`ArchiveStemContract.cs:79-86`).
- The throw is not caught anywhere between there and `ButtonOK_Click` (`EfcFormController.cs:429-443`), which logs and **rethrows** from an `async void` handler. On a WinForms synchronization context that is an unhandled exception.
- It occurs **after** `_formViewer.Hide()` (`:718`), so the form has already disappeared.
- Find path: `ActionOkAsync:726` to `EfcDataModel.OpenOlFolderAsync` to `EmailFiler.TryOpenOlFolder`, which does catch, producing "Error opening folder" plus the contract message instead of the previous "Please select a valid folder."

Reachability was traced rather than assumed. `BreadcrumbRow.FilingTarget` is always the presented row
text (`BreadcrumbRowBuilder.cs:104-142` passes `presentedText` in all three branches). Presented rows
come from `FolderPredictor.FolderArray` / `FindFolder`. `ProjectSuggestionPath`
(`FolderPredictor.cs:845-858`) strips the archive prefix only when the suggestion is *strictly
under* it (`folderPath.Length > archivePrefix.Length`); a suggestion whose folder **is** the archive
root is returned as a full rooted path verbatim. `BreadcrumbBridgeRouter.SelectRow` admits it
(`TryMakeArchiveRelative` returns `true` with an empty stem), and the OK guard now admits it too.

Before this cycle that value was rejected at the OK guard with a dialog. After it, the value reaches
the filing boundary and throws. This is a new crash on the same path-representation chain issue #614
was filed to close, and it is codified by a test documented as a "CR-2 recorded consequence".

Note also that the two guards still disagree, only at a different pair of points:
`BreadcrumbBridgeRouter.SelectHierarchyPath` treats the archive root itself as a deterministic
non-selection (`stem.Length == 0` rejects, `:487-494`), while `IsValidFilingSelection` accepts it.

**Remediation is required.** The fix the prior review recommended — normalize in `SelectRow` via
`CommitSelection(row, stem)` — resolves the disagreement in the direction that also makes the value
filable, and was not taken.

### G2 — PARTIAL. AC16 no longer matches its criterion text and was not amended

spec AC16 requires that `ActionOkAsync` and `IsValidSelection` "share one predicate" and that "OK
rejects … a non-relative selection". After the remediation they delegate to two different predicates
in one shared class, and OK accepts a class of non-relative selection. The remediation inputs
acknowledged the AC16 tension for the length rule only. `spec.md` was not amended and AC16 remains
checked `[x]`. See the feature audit for the evaluation.

### G3 — PARTIAL. File-size ceiling

Three files remain above 500 lines (`EfcFormController.cs` 1079, `BreadcrumbBridgeRouter.cs` 596,
`BreadcrumbBridgeRouterIssue439Tests.cs` 694), all pre-existing and none grown against the merge
base. The applied reading is net non-growth, ratified in the **gitignored**
`artifacts/orchestration/orchestrator-state.json` under `orchestrator_adjudications`. That
ratification is by the orchestrator agent, not the human maintainer, and will not survive merge; it
should be transcribed into `issue.md` or the PR body.

### G4 — PARTIAL. Manual validation not executed, and the remediation added new behaviour without it

All five AC26 live-Outlook steps remain recorded as NOT EXECUTED. The remediation cycle added new
production behaviour on the OK path (the archive-root read, the degrade, the widened predicate)
without any additional manual validation. G1 is precisely the class of defect a live-profile OK-path
walkthrough would surface.

### G5 — Non-blocking. Repository-wide line coverage below the 85% floor

Recorded and dispositioned in § 5.

### G6 — Non-blocking, carried forward unchanged and re-verified at this head

- `FolderConverter`'s alternative-folder-name cluster is unreachable dead code (prior CR-3). Re-verified: a repository-wide grep for `AlternativeFolderPrompt`, `AskUserForAlternatives` and `BuildAlternativesDictionary` still returns only intra-cycle and test hits.
- `AppOlObjects.ArchiveRootPath` and `AppFileSystemFolderPaths.LoadFolders` now throw where they previously degraded (prior CR-4). Intended by AC13/AC14.
- The `internal AppFileSystemFolderPaths(Func<string,string>)` seam is still called by nothing; the only two construction sites are `ApplicationGlobals.cs:109` and the private `(bool async)` call at `AppFileSystemFolderPaths.cs:51`.
- `SortEmail.ResolvePaths` (both overloads, still `[ExcludeFromCodeCoverage]`) is still unmigrated.
- `FolderConverter.cs:265` still passes `nameof(fsPath)` for a local variable rather than a parameter.

## 9. Summary of Changes

| Category | Count | Notes |
| --- | ---: | --- |
| Total changed files | 111 | +10581 / -243 against `c279d40b` |
| C# production source | 10 | 3 added, 7 modified |
| C# test source | 11 | 8 added, 3 modified |
| Project / package wiring | 6 | `<Compile Include>` registrations plus one version-aligned `log4net 3.3.2` reference |
| Repo configuration | 1 | `.gitignore` adds `.claude/state/` |
| Markdown documentation, evidence, agent memory | 83 | feature docs, 47 evidence artifacts, promotion documents, agent memory |
| Changed by the remediation cycle only | 33 | of which 2 are production source (`EfcSelectionGuard.cs`, `EfcFormController.cs`) |

## 10. Compliance Verdict

| Area | Verdict |
| --- | --- |
| General Unit Test Policy | PARTIAL (scenario completeness at the composition level) |
| General Code Change Policy | PARTIAL (file-size ceiling; fail-fast asymmetry) |
| C# Code Change Policy | PASS |
| C# Unit Test Policy | PASS |
| Toolchain (4 steps, re-executed) | PASS |
| Repository-wide line coverage | FAIL (non-blocking; pre-existing and improved) |
| Repository-wide branch coverage | PASS |
| New-code coverage | PASS |
| Changed-line coverage | PASS |
| Evidence location compliance | PASS |
| modified-workflow-needs-green-run | Not triggered |
| Redaction (#602) | PASS |
| Code review | FAIL — 1 blocking finding (RC-1) |
| Acceptance criteria | PARTIAL — 23 PASS, 3 PARTIAL, 0 FAIL |

**Overall: FAIL. 1 blocking finding. Remediation cycle 2 is required.**

### Remediation Determination

Remediation is triggered under the `feature-review-workflow` § 8 rule "the code review contains
blockers". Blocking finding RC-1 is recorded in `code-review.2026-08-26T22-12.md` and carried into
`remediation-inputs.2026-08-26T22-12.md`.

The repository-wide coverage FAIL, the three PARTIAL results, and every carried-forward Major and
Minor finding are explicitly **not** remediation triggers and are dispositioned non-blocking in the
sections above.

## Appendix A: Test Inventory (remediation cycle 1 additions)

`QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` — 23 tests, 316 lines:

| Region | Tests | Scenarios covered |
| --- | ---: | --- |
| Filing predicate | 14 | null; empty; whitespace; banner sentinel; store-rooted; single-separator with null root; drive-rooted; valid relative stem; two-character stem (5 values); single-character stem; rooted under root; archive root exact; rooted above root; cross-store rooted; separator-boundary near miss; rooted with empty root |
| Folder-creation predicate | 7 | null; empty; whitespace; banner sentinel; two-character; single-character; minimum length; rooted; valid relative stem |
| Archive-root resolver | 2 | accessor succeeds (returns root, logs nothing); accessor throws `InvalidOperationException` (degrades to empty, logs exactly `RootUnavailableDiagnostic`) |

Composition-level tests, in which a value the filing predicate accepts is carried through to
`EmailFilerConfig.ResolvePaths`: **none**. That is the gap RC-1 occupies.

## Appendix B: Toolchain Commands Reference

Commands executed by this reviewer at head `b45e2a2d`, in the mandated order:

```
# 1. Format (check-only)
dotnet tool restore
dotnet tool run csharpier check .
#    -> Checked 1530 files in 4013ms; exit 0

# 2. Analyzers
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
#    -> exit 0

# 3. Type check / nullable
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
#    -> exit 0

# 4. Tests
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Settings:TaskMaster.runsettings
#    -> Total 6111, Passed 6111, Failed 0; exit 0
```

Both MSBuild steps were invoked through a `.cmd` wrapper so that `"/p:Platform=Any CPU"` survives
argument splitting; `msbuild.exe` resolved to the Visual Studio 18 Community `MSBuild\Current\Bin`
copy.

Read-only verification commands:

```
git merge-base main HEAD
git diff --numstat c279d40b..HEAD
git diff --name-status c279d40b..HEAD -- '*.cs'
git diff --name-only c279d40b..HEAD          # scanned for the four prohibited artifacts/ evidence prefixes
awk 'END{print NR}' <each changed .cs file>  # file-size audit at head and at the merge base
pwsh -c '. ./.claude/hooks/validate-feature-review-coverage.ps1; Get-ChangedLanguageSet -Lines (Get-Content artifacts/pr_context.summary.txt)'
pwsh -c '. ./.claude/hooks/validate-feature-review-coverage.ps1; Get-LanguageRepoCoverage -Language CSharp; Get-LanguageBranchCoverage -Language CSharp'
python <per-file Cobertura reducer> coverage/coverage.cobertura.filtered.p0-t9.xml   # merge-base per-file figures
python <per-file Cobertura reducer> coverage/coverage.cobertura.filtered.p5-t4.xml   # head per-file figures
```
