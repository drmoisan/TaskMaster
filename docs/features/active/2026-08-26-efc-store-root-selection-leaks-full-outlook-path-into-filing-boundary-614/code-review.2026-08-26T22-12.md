# Code Review — Issue #614 (efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary)

- Artifact timestamp: 2026-08-26T22-12
- Review cycle: remediation cycle 1 **exit** re-audit. The prior-cycle record is `code-review.2026-08-26T16-55.md`, left in place unmodified.
- Reviewer: feature-review agent
- Branch: `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
- Base branch: `main` — merge-base `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`
- Head: `b45e2a2d5b7f4d4219aa0caea4e63e24777feab1`
- Review scope: full branch diff (111 files, +10581 / -243). Line references are to the head revision.
- Finding prefix: `RC-` (remediation cycle exit). Prior-cycle findings are referenced by their original `CR-` numbers.

## Executive Summary

The remediation cycle changed two production files. `EfcSelectionGuard.cs` grew from 38 to 147
lines, splitting one predicate into two and adding a throw-tolerant archive-root resolver;
`EfcFormController.cs` rewired its two call sites accordingly.

**CR-1 is resolved.** `IsValidFilingSelection` no longer carries `value.Length >= 3`. The rule moved
to `IsValidCreationSelection` (`EfcSelectionGuard.cs:97-108`), which is now the sole delegate of the
`IsValidSelection` property (`EfcFormController.cs:1044-1045`), and that property gates only the two
folder-creation call sites (`:468`, `:758`). Filing to `HR`, `IT`, `PR`, `QA`, `Q1` and to a
single-character name is pinned by `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` and
`..._SingleCharacterRelativeStem_IsAccepted`. This reviewer read both predicates and all three call
sites and confirms the separation is complete and correct.

**CR-2 is not substantively resolved.** The remedy widened the filing predicate to accept a rooted
value that resolves against the archive root, satisfying the literal instruction in the remediation
inputs ("the two guards must agree"). It did not normalize the value. `EmailFilerConfig.ResolvePaths`
— the D4 filing boundary this same feature added — still calls `RequireArchiveRelativeStem`, which
throws for any rooted value. The accepted value class is therefore still unfilable; it now fails by
unhandled exception after the form is hidden rather than by a dialog before it. That is finding
**RC-1, Blocking**.

Three further findings are new to this cycle: RC-2 (the spec's AC16 text is now contradicted and was
not amended), RC-3 (the new degrade guards one of nine archive-root reads), RC-4 (an untested new
branch in `EmailFilerConfig.GetStem`, the only measurable branch-coverage decrease on the branch).
Six findings carry forward from the prior cycle unchanged; each was re-verified at this head.

Four gates were re-executed independently by this reviewer and all passed: CSharpier check, analyzer
rebuild, nullable rebuild, and the 6111 tests in the three changed test assemblies.

**One blocking finding. Recommendation: NO-GO pending remediation cycle 2.**

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| **Blocking** (RC-1) | `QuickFiler/Controllers/EfcSelectionGuard.cs`, `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | Guard `:75-81`; router `:474-495`; boundary `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs:196-200` | The CR-2 remedy makes `IsValidFilingSelection` accept a rooted value that resolves against the archive root, but nothing between the guard and the filing boundary converts it to a stem. `EfcHomeController.ExecuteMovesCoreAsync` reads `SelectedFolder` verbatim, `EfcDataModel.MoveToFolderAsync` assigns it verbatim to `EmailFilerConfig.DestinationOlStem` (`:286`), and `ResolvePaths` then calls `ArchiveStemContract.RequireArchiveRelativeStem`, which throws `ArgumentException` for any rooted value. The throw escapes `ExecuteMovesAsync` (try/finally, no catch) and `ActionOkAsync`, and `ButtonOK_Click` (`EfcFormController.cs:429-443`) logs and **rethrows** from an `async void` handler — an unhandled exception on the WinForms context. It occurs after `_formViewer.Hide()` (`:718`), so the form is already gone. Before this cycle the same value produced "Please select a valid folder." and nothing else. | Normalize at the producer, as the prior review recommended: in `BreadcrumbBridgeRouter.SelectRow`, when `ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out var stem)` succeeds, call `CommitSelection(row, stem)` rather than committing the rooted value. Reject the empty-stem case (the archive root itself), matching `SelectHierarchyPath`'s existing `stem.Length == 0` rule. Then restore `IsValidFilingSelection` to rejecting rootedness as such, and update `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` (`BreadcrumbBridgeRouterIssue439Tests.cs:165`) from the rooted value to its stem. Add one composition test: a value the filing predicate accepts must not throw at `EmailFilerConfig.ResolvePaths`. | Issue #614 is a filing-correctness defect whose reported symptom is a crash caused by a rooted path reaching the filing boundary. This change re-opens exactly that symptom for a narrower input class, and codifies it with a test. Making the guard agree with the router by *widening* the guard leaves the invariant ("`SelectedFolderPath` is always an archive-relative stem") unenforced at the producer, which is the only place it can be enforced once. | Guard accepts: `EfcSelectionGuard.cs:75-81`; asserted by `EfcSelectionGuardTests.cs:120-128` and `:134-141` (the latter commented "CR-2 recorded consequence"). Boundary throws: `ArchiveStemContract.cs:79-86` (`IsFullOutlookPath` is true for any leading separator, `:48-53`). Verbatim carry: `EfcHomeController.ExecuteMoves.cs:64-66`, `EfcDataModel.cs:286`. Rethrow: `EfcFormController.cs:436-442`. Reachability: `BreadcrumbRowBuilder.cs:104-142` sets `FilingTarget = presentedText` in all branches; `FolderPredictor.ProjectSuggestionPath` (`:845-858`) returns a full rooted path verbatim when the suggestion is not strictly under the archive prefix, which is the case when the suggestion **is** the archive root. |
| Minor (RC-2) | `docs/features/.../spec.md` (AC16) vs `QuickFiler/Controllers/EfcSelectionGuard.cs` | spec AC16; guard `:62`, `:97` | AC16 states that `ActionOkAsync` and `IsValidSelection` "share one predicate" and that tests prove "OK rejects … a non-relative selection". Post-remediation they delegate to two different predicates, and OK accepts a class of non-relative selection. `spec.md` was not amended and AC16 is still checked `[x]`. The remediation inputs acknowledged the AC16 tension for the length rule only, not for the rootedness rule. | Amend AC16 in `spec.md` to describe the delivered design (two scope-specific predicates in one guard type; OK rejects null, empty, whitespace, banner sentinels, and any rooted value not resolvable against the archive root), and record the amendment in the change description. Do this as part of remediation cycle 2, since RC-1 will change the rootedness half again. | An acceptance criterion that contradicts the code it governs stops functioning as a spec. A future reviewer reading AC16 would conclude the shared-predicate design was reverted without authorisation. | AC16 text in `spec.md`; two predicates at `EfcSelectionGuard.cs:62` and `:97`; call sites at `EfcFormController.cs:712` and `:1044`. |
| Minor (RC-3) | `QuickFiler/Controllers/EfcFormController.cs` | `:708-711` vs `:492`, `:502`, `:777`, `:787`, `:897`, and `EfcDataModel.cs:289`, `:310`, `:328` | The new `ResolveArchiveRootOrEmpty` degrade is documented as existing so "the OK button must degrade to rejecting rooted selections rather than tearing the form down". It guards exactly one of the nine `ArchiveRootPath` reads reachable from this controller. Two of the unguarded reads (`:777`, `:787`, the folder-creation path) execute after `_formViewer.Hide()`, and three more are inside `EfcDataModel`'s `EmailFilerConfig` initialisers on the same OK path the degrade protects. If `ArchiveRootPath` is unresolvable, the OK path degrades cleanly only because `BindBreadcrumbRowsAsync` (`:890-905`) swallows the earlier failure and leaves the row list empty, so no selection exists to file. | Either lift the degrade to a single cached read shared by the controller and the data model, or drop it and let the documented fail-fast behaviour of AC13 stand uniformly. A partial degrade that is effective only because an unrelated `catch` upstream masks the same failure is harder to reason about than either alternative. | The comment states a protection the code provides on one path out of several, which will mislead the next maintainer. | Read `EfcFormController.cs:708-711`, `:492`, `:502`, `:777`, `:787`, `:897`; `EfcDataModel.cs:289`, `:310`, `:328`; `AppOlObjects.cs:241-263` (the cache is populated only on success, so a failing read retries the COM resolution on every call). |
| Minor (RC-4) | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` | `:250-258` (`GetStem`) | The new ternary's out-of-ancestor arm (`folderPath.TrimStart('\\')`) has no test. The file's branch coverage moves 70.0000% to 60.0000% (7/10 to 6/10 covered conditions) between the merge base and this head, and this is the only measurable branch-coverage decrease on the branch attributable to added code. The single `GetStem` test (`EmailFilerConfig_Tests.cs:186-193`) exercises only the success arm. The XML documentation asserts the fallback "preserv[es] the pre-existing out-of-ancestor behaviour", and that assertion is unverified. | Add one test: `GetStem` with a `folderPath` outside `olAncestor` returns the input with leading separators trimmed and does not throw. Two lines of test code close the branch. | The claim in the documentation is exactly the kind of behavioural assertion the repository's own test policy requires to be pinned, and `GetStem` feeds `OriginOlStem`, which drives un-training. | Reviewer-measured per-line condition coverage from `coverage/coverage.cobertura.filtered.p0-t9.xml` (baseline: partial branches at `:190`, `:216`) and `coverage/coverage.cobertura.filtered.p5-t4.xml` (head: partial branches at `:196`, `:227`, **`:252`**). Line `:252` is the new ternary. |
| Major, non-blocking (carried, prior CR-3) | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | `:17-24`, `:118-215` | The alternative-folder-name recovery cluster still has no production entry point: `IsLegalFolderName(string, bool)` is called only from `AskUserForAlternatives`, which is reachable only from `AlternativeFolderPrompt`, which is called only from `IsLegalFolderName(string, bool)` — a closed cycle. The AC11 / D5f `RemoveIllegalCharacters` fix therefore repairs a dialog option that cannot appear. Unchanged by the remediation cycle. | Do not fix in #614. Promote a follow-up issue to delete the cluster together with the roughly nine tests that exercise it, cross-referencing the already-promoted `2026-08-26-orphaned-duplicate-folderconverter-dead-file-with-always-false-guards.md`. | Dead code with a full test suite reads as live behaviour and inflates both the file line count and the apparent coverage of this change. | Re-verified at this head by grepping the repository for `AlternativeFolderPrompt`, `AskUserForAlternatives` and `BuildAlternativesDictionary`: the only non-test hits are inside the cycle itself. |
| Major, non-blocking, intended by spec (carried, prior CR-4) | `TaskMaster/AppGlobals/AppOlObjects.cs`, `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | `AppOlObjects.cs:253-266`; `AppFileSystemFolderPaths.cs:268-271` | `ArchiveRootPath` (a property getter) and `LoadFolders` (run during add-in startup) now raise `InvalidOperationException` where they previously returned a wrong value or fell back. No consumer catches it, other than the new narrow catch in `ResolveArchiveRootOrEmpty`. | Do not revert; this is AC13 / AC14 behaviour. Catch `InvalidOperationException` at the add-in-startup and EFC UI boundaries so the user sees the redacted diagnostic in a dialog, and treat this as the highest-priority item for live-profile validation. | Throwing from a property getter is the CA1065 pattern; combined with an unguarded startup path, an environment that previously degraded silently now fails hard. Correct in intent, but the failure mode moved from data corruption to a crash and no test exercises the crash path end to end. | `AppOlObjects.cs:259-263` calls `ArchiveRootPathGuard.RequireResolvedArchiveRoot`; the guard throws at `ArchiveRootPathGuard.cs:44` and `:56`. `AppFileSystemFolderPaths.ResolveOneDriveRoot` throws at `:209`. |
| Minor (carried) | `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | `:20`, `:28-39` | The injectable environment seam is inert: `internal AppFileSystemFolderPaths(Func<string, string>)` is called by no production code and no test, so `_readEnvironmentVariable` can only ever hold `Environment.GetEnvironmentVariable`. The real testability AC14 credits to the seam comes from `ResolveOneDriveRoot` being `internal static`. | Delete the seam constructor and the field; have `LoadFolders` call `ResolveOneDriveRoot(Environment.GetEnvironmentVariable)` directly. That removes 9 uncovered lines and one misleading affordance. | An injection seam nothing injects through is a maintenance cost with no benefit and overstates the testability of `LoadFolders`. | Re-verified: `grep -rn "new AppFileSystemFolderPaths" --include=*.cs .` returns exactly two hits, `ApplicationGlobals.cs:109` and the private `(bool async)` call at `AppFileSystemFolderPaths.cs:51`. |
| Minor (carried) | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | `:57-104` vs `:118-127` | The two folder-name validators apply different rule sets. `FindInvalidSegmentRule` rejects invalid characters, trailing dot, trailing space, and reserved device names; `IsLegalFolderName` rejects only invalid characters. `CON` or `Report.` passes one and fails the other. | Route `IsLegalFolderName` through `FindInvalidSegmentRule`. Lower priority than the dead-cluster finding, and it disappears if that cluster is deleted. | Two validators for one concept is the duplication this change set out to eliminate elsewhere. | Read both methods at the cited lines. |
| Minor (carried) | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | `:1000-1032` and `:1035-1069` | The D4 filing-boundary guard was added to both `EmailFilerConfig.ResolvePaths` overloads but not to the structurally identical `SortEmail.ResolvePaths` pair, which has three live call sites. Those overloads still concatenate the ancestor and stem with no `RequireArchiveRelativeStem` call. Both carry `[ExcludeFromCodeCoverage]`, so no test observes them. | Promote a follow-up issue rather than widening #614. The leak itself is already stopped downstream by the `TryMakeArchiveRelative` gate in `ToFsFolderpath`. | Leaving one of two structurally identical filing paths on the old semantics reintroduces the divergence the contract exists to remove. | Re-verified at this head: `SortEmail.cs:999-1008` still carries `[ExcludeFromCodeCoverage]` and no contract call. |
| Minor (carried) | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | `:265` | The invalid-segment `ArgumentException` passes `nameof(fsPath)`, but `fsPath` is a local variable (`:254`), not a parameter of `ToFsFolderpath`. A caller reading `ArgumentException.ParamName` receives a name absent from the signature. | Use `nameof(olBranchPath)`, matching the sibling throw at `:248-252`. | `ParamName` is part of the exception contract and is read by diagnostics and tests. | Read `FolderConverter.cs:261-267`; compare with `:248-252`. |
| Info (carried) | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | `:142-157` (`ToHierarchyPath`) | The relative branch dropped the pre-change `TrimStart('\\', '/')`. Any leading-separator value is diverted to the full-path branch by `IsFullOutlookPath` before the concatenation is reached, so the trim is genuinely dead. | No action. Recorded so the behaviour is not mistaken for an oversight. | The plan's stated reasoning was verified against the head revision and is correct. | `BreadcrumbBridgeRouter.cs:142-157`; `ArchiveStemContract.cs:48-53`. |
| Info | `QuickFiler/Controllers/EfcFormController.cs` | `:1044-1045` | `IsValidSelection` gained two behaviours against the merge base that no acceptance criterion asked for: it now rejects whitespace-only selections (the old form rejected only `""`) and rejects rooted values (the old form had no such rule). Both are defensible tightenings of a folder-creation guard and both are pinned by tests. | No action. Recorded because the criterion text does not cover them. | The folder-creation path concatenates the selection beneath the archive root, so a rooted value was never a valid creation stem; the tightening removes a latent defect. | `git show c279d40b:QuickFiler/Controllers/EfcFormController.cs` lines 1038-1050 versus head `:1044-1045` and `EfcSelectionGuard.cs:97-108`. Tests: `IsValidCreationSelection_WhitespaceSelection_IsRejected`, `IsValidCreationSelection_RootedSelection_IsRejected`. |

## Design and Structure Assessment

**What the remediation got right.** Splitting `IsValidFilingSelection` and `IsValidCreationSelection`
is the correct structural response to CR-1. The two paths genuinely have different rule sets, the
shared class keeps them adjacent and comparable, and `MinimumCreationLength` is now a named constant
with a documented rationale instead of a bare `3`. The XML documentation on both predicates states
which rule lives where and why, which is precisely the "comment *why*" the policy asks for.

Extracting `ResolveArchiveRootOrEmpty` into the guard rather than writing the `try`/`catch` inline in
`ActionOkAsync` is also a good call: it is what makes the `catch` arm unit-reachable, and it is why
`EfcSelectionGuard.cs` measures 100% line and 100% branch at 31 instrumented lines rather than
leaving an untestable arm in a WinForms method. The catch is narrow (`InvalidOperationException`
only), logs before degrading, and uses a fixed value-free message that satisfies the #602 redaction
requirement.

**What the remediation got wrong.** The instruction it was given — "the two guards must agree" — has
two solutions, and it chose the one that preserves the disagreement further downstream. Widening
`IsValidFilingSelection` reconciles it with `SelectRow`, but `SelectRow` is not the only other guard
on that value: `EmailFilerConfig.ResolvePaths` is, and it is the guard this same feature added as the
D4 filing boundary. The consumer contract that `ArchiveStemContract` exists to establish is
"`DestinationOlStem` is archive-relative", and the remedy admits a value that violates it into the
pipe rather than converting it. The router remains the only place where the conversion can happen
once, before the value fans out to the OK path, the folder-creation path, the Find path, and the
recents list.

The archive-root-exact case makes this concrete and is not hypothetical: `SelectHierarchyPath`
already treats the archive root as a non-selection (`stem.Length == 0` rejects), `SelectRow` admits
it, and the OK guard now admits it too — while the boundary throws on it. Three guards, three
different answers for one value.

**Test-design consequence.** `EfcSelectionGuardTests` is a good unit-test suite for two pure
predicates and one resolver, and it is at 100% coverage. It cannot detect RC-1, because RC-1 is a
property of the composition of the predicate with a boundary in a different assembly. The suite's
completeness at the unit level is what made the defect easy to miss. A single composition test —
"any value `IsValidFilingSelection` accepts must survive `RequireArchiveRelativeStem`" — would have
failed on the first run.

**Redaction.** Unchanged and still thorough. `RootUnavailableDiagnostic` is a fixed constant naming
no path, mailbox, host, or account, and the resolver test asserts the sink receives exactly that
constant. A reviewer sweep of the whole branch diff for real account, host and organization names
returns only the fabricated placeholders.

**Dependency hygiene.** The `log4net 3.3.2` reference added to `QuickFiler.Test` matches the version
already referenced by `QuickFiler` and `UtilitiesCS`, and `QuickFiler.Test/app.config` carries the
matching binding redirect. No version skew was introduced.

## Toolchain Verification

Re-executed independently by this reviewer at head `b45e2a2d`:

| Step | Command | Exit | Result |
| --- | --- | ---: | --- |
| 1. Format | `dotnet tool run csharpier check .` | 0 | Checked 1530 files in 4013ms; 0 unformatted |
| 2. Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | 0 errors; only pre-existing System.Reactive packages.config warnings |
| 3. Type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | 0 errors, 0 `CS86xx` |
| 4. Tests | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Settings:TaskMaster.runsettings` | 0 | 6111 total, 6111 passed, 0 failed, 38.9 s |

Coverage figures and per-file measurements are in `policy-audit.2026-08-26T22-12.md` § 5.

## Severity Rollup

| Severity | Count | Findings |
| --- | ---: | --- |
| Blocking | 1 | RC-1 |
| Major (non-blocking) | 2 | carried prior CR-3 (dead cluster), carried prior CR-4 (fail-fast throws) |
| Minor | 6 | RC-2, RC-3, RC-4, and three carried (inert seam, validator rule-set asymmetry, `SortEmail.ResolvePaths` unmigrated, `nameof(fsPath)`) |
| Info | 2 | `ToHierarchyPath` dead trim (carried), `IsValidSelection` tightenings |

Prior-cycle CR-1 is **closed**. Prior-cycle CR-2 is **not closed**; it is superseded by RC-1, which
describes the same value class with a worse failure mode.

## Recommendation

**NO-GO.** One blocking finding. Remediation cycle 2 should address RC-1 by normalizing the
selection at `BreadcrumbBridgeRouter.SelectRow` and restoring the filing predicate's rejection of
rootedness, together with the two supporting changes RC-1 names (the `Issue439` assertion update and
one composition test). RC-2 should be closed in the same cycle, because the AC16 text has to be
rewritten either way once RC-1 lands.

RC-3 and RC-4 are cheap and adjacent; including them is reasonable but not required.

Everything carried forward from the prior cycle — the `FolderConverter` dead cluster, the fail-fast
behaviour changes, the inert seam, the un-migrated `SortEmail.ResolvePaths` pair, the `nameof`
defect, and the repository-wide line coverage shortfall — retains its non-blocking disposition and
should not be absorbed into #614. The first two warrant their own promoted issues.

The live-Outlook validation steps (AC26) remain unexecuted and are now more important than they were
at the prior review, because the remediation added new behaviour on the OK path and RC-1 is exactly
the class of defect an OK-path walkthrough on a real profile would surface immediately.
