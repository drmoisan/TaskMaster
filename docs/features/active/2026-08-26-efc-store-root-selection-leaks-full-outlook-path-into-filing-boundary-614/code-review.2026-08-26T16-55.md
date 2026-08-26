# Code Review — Issue #614 (efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary)

- Artifact timestamp: 2026-08-26T16-55
- Reviewer: feature-review agent
- Branch: `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
- Base branch: `main` — merge-base `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`
- Head: `02092504e50ede2527ae35f14629f0bc4c4c94ff`
- Review scope: full branch diff (80 files, +7599 / -158). Line references are to the head revision.

## Executive Summary

This is a well-engineered defect-chain fix. The central design decision — collapsing four divergent
ad-hoc `Replace` / `Contains` / `Substring` path manipulations onto one 147-line pure static
contract (`ArchiveStemContract`) with prefix-anchored, separator-terminated, `OrdinalIgnoreCase`
semantics — is the correct structural response to the reported defect, and it is applied
consistently across the router, the filing boundary, the data model, and the converter. Redaction
discipline in the new diagnostics is thorough: every rejection message names the violated rule and
withholds the value. Test quality is high (80 new methods, all deterministic, no temporary files, no
wall-clock dependency, 100% line coverage on every new type).

Four gates were re-executed independently by this reviewer and all passed: CSharpier check,
analyzer rebuild, nullable rebuild, and the 6093 tests in the three changed test assemblies.

**No blocking finding was identified.** Ten findings are recorded: four Major (all non-blocking,
two of which are intended-by-spec behaviour changes that warrant maintainer awareness), four Minor,
and two Informational. The two Major findings that are *not* intended by spec — CR-1 and CR-2 —
concern the new `EfcSelectionGuard` predicate and are worth addressing before or shortly after
merge, because both can make a legitimate filing destination unusable.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Major | `QuickFiler/Controllers/EfcSelectionGuard.cs` | `:33` (`value.Length >= 3`), consumed at `QuickFiler/Controllers/EfcFormController.cs:706` | The OK/filing path now rejects any selection shorter than three characters. Before this change `ActionOkAsync` guarded only `selectedFolder is null \|\| selectedFolder.StartsWith("====")`. The `Length < 3` rule came from `IsValidSelection`, which gated only folder *creation* (`EfcFormController.cs:468`, `:752`), never filing. Filing to an archive subfolder named `HR`, `IT`, `PR`, or `Q1` now fails with "Please select a valid folder." | Remove `value.Length >= 3` from `IsValidFilingSelection` and rely on the `===` banner-prefix check, which is what the length rule was a crude proxy for. If the strictness must be retained for folder creation, keep it in a separate `IsValidParentFolderSelection` predicate. Update `EfcSelectionGuardTests.IsValidFilingSelection_TwoCharacterSelection_IsRejected` accordingly. | Unifying two guards is correct, but unifying onto the *stricter* of the two silently narrows the accepted input set of the filing path. spec AC16 requires OK to reject `null`, `string.Empty`, a `"===="`-prefixed sentinel, and a non-relative selection; it does not ask for a length rule. | Old guard: `git show c279d40b:QuickFiler/Controllers/EfcFormController.cs` line 706. New predicate: `EfcSelectionGuard.cs:31-36`. Test asserting the new rejection: `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:71-77` (`IsValidFilingSelection("AB")` -> false). Pre-existing call sites of `IsValidSelection` read at `EfcFormController.cs:468` and `:752`, both folder-creation paths. |
| Major | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` and `QuickFiler/Controllers/EfcSelectionGuard.cs` | Router `:477-491` (`SelectRow` guard) vs guard `:35` (`!ArchiveStemContract.IsFullOutlookPath(value)`) | The two new guards disagree on rooted-at-or-under-root values. `SelectRow` deliberately passes a rooted target through verbatim when it is at or under the bound root, preserving the #439 contract; `IsValidFilingSelection` then rejects every rooted value at OK. A selection the router accepts and renders as selected can therefore never be filed, and the user sees only the generic "Please select a valid folder." | Normalize inside `SelectRow`: when `ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out var stem)` succeeds, `CommitSelection(row, stem)` instead of the verbatim rooted value. This removes the disagreement without weakening any guard and makes `SelectedFolderPath` uniformly archive-relative. `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` would need its asserted value changed from `@"\aRcHiVe\Clients\North"` to `@"Clients\North"`, which is the same class of documented spec correction already applied to that file. | The stated purpose of `ArchiveStemContract` is that `SelectedFolderPath` is always an archive-relative stem. Leaving one producer path that emits a rooted value, and then rejecting that value downstream, keeps the invariant unenforced at the producer and converts a formerly-working (if unsafe) selection into a dead end. `EmailFilerConfig.RequireArchiveRelativeStem` would reject the same value, so the class is unfilable through every route. | Router guard scope is explicitly limited to out-of-root full paths: `BreadcrumbBridgeRouter.cs:479-489` requires `IsFullOutlookPath(selection) && !TryMakeArchiveRelative(...)` before rejecting. `IsFullOutlookPath(@"\aRcHiVe\Clients\North")` returns true on the leading-separator rule (`ArchiveStemContract.cs:47-51`). Untouched assertion: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:165`. |
| Major | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | `:17-24`, `:118-215` (the alternative-folder-name cluster) | The entire "illegal folder name" recovery cluster has no production entry point, and this change extended it rather than removing it. `IsLegalFolderName(string, bool)` is called only from `AskUserForAlternatives` (`:162`); `AskUserForAlternatives` is reachable only through `AlternativeFolderPrompt` (`:139`), which is called only from `IsLegalFolderName(string, bool)` — a closed cycle. The single-argument `IsLegalFolderName` lost its last external caller when `ToFsFolderpath` stopped calling it. The new `RemoveIllegalCharacters` helper (`:207-213`), which is the AC11 / D5f fix, therefore repairs a dialog option that cannot appear, and the corrected assertion at `FolderConverterTests.cs:329` verifies unreachable behaviour. | Do not fix this in #614. Promote a follow-up issue to delete the whole cluster (`AlternativeFolderPrompt`, `AlternativeFolderSelectionDialog`, `AlternativeFolderInputDialog`, both `IsLegalFolderName` overloads, `AskUserForAlternatives`, `BuildAlternativesDictionary`, `RemoveIllegalCharacters`, `GetIllegalFolderChars`) together with the ~9 tests that exercise it, which would return `FolderConverter.cs` to roughly its 244-line baseline. Cross-reference the already-promoted `2026-08-26-orphaned-duplicate-folderconverter-dead-file-with-always-false-guards.md`, which is the adjacent defect. | Dead code with a full test suite reads as live behaviour to the next maintainer and inflates both the file line count and the apparent coverage of this change. It also means the D5f "defect" is unobservable, which the defect census did not establish. | Reachability traced by `grep -rn "AlternativeFolderPrompt\|AskUserForAlternatives\|BuildAlternativesDictionary" --include=*.cs .`: the only non-test hits are inside the cycle itself. Baseline confirmation that the cycle was already closed before this change: `git show c279d40b:UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` shows the two-argument overload called only at `:88`. The change removed the last external call to the one-argument overload (baseline `:161`). |
| Major (intended by spec; awareness item) | `TaskMaster/AppGlobals/AppOlObjects.cs`, `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | `AppOlObjects.cs:253-266`; `AppFileSystemFolderPaths.cs:268-271` | Two hot paths changed from "silently return a wrong value" to "throw". `ArchiveRootPath` is a property getter that now raises `InvalidOperationException` when the `Archive` folder does not resolve in the default store or resolves cross-store. `LoadFolders`, invoked from the `AppFileSystemFolderPaths` parameterless constructor during `ApplicationGlobals` construction, now raises `InvalidOperationException` when none of `OneDriveCommercial` / `OneDrive` / `OneDrivePersonal` is set, where it previously fell back to `AppData` or to `SpecialFolders.First().Value`. No consumer of either catches `InvalidOperationException`, and both wiring sites are uncovered COM/environment-bound lines. | This is the specified D6 and D7 behaviour (AC13, AC14) and should not be reverted. Two mitigations are worth taking: (a) catch `InvalidOperationException` at the EFC and add-in-startup UI boundaries so the user sees the redacted diagnostic in a dialog rather than an unhandled exception; (b) treat this as the highest-priority item for the AC26 live-profile validation, since it is the change most likely to alter startup behaviour on a machine unlike the developer's. | Throwing from a property getter is the CA1065 "do not raise exceptions in unexpected locations" pattern; combined with an unguarded startup path, an environment that previously degraded silently now fails hard. The change is correct in intent — a wrong archive root produces misfiled mail — but its failure mode moved from data corruption to a crash, and no test exercises the crash path end to end. | `AppOlObjects.cs:259-263` calls `ArchiveRootPathGuard.RequireResolvedArchiveRoot(...)`; the guard throws at `ArchiveRootPathGuard.cs:47` and `:59`. `AppFileSystemFolderPaths.ResolveOneDriveRoot` throws at `:209`. Removed fallback chain visible in `git diff c279d40b..HEAD -- TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`. Both wiring sites listed as uncovered in `evidence/qa-gates/coverage-delta.2026-08-26T19-50.md` § (d). |
| Minor | `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | `:20`, `:28-39` | The injectable environment seam is inert. `internal AppFileSystemFolderPaths(Func<string, string> readEnvironmentVariable)` is called by no production code and by no test (`grep -rn "new AppFileSystemFolderPaths"` finds only the parameterless call in `ApplicationGlobals.cs:109` and the private `(bool async)` call at `:51`). `_readEnvironmentVariable` can therefore only ever hold `Environment.GetEnvironmentVariable`. The testability that AC14 credits to the seam actually comes from `ResolveOneDriveRoot(Func<string,string>)` being `internal static`. The dead constructor contributes 9 of the 18 uncovered changed lines. | Delete the seam constructor and the `_readEnvironmentVariable` field, and have `LoadFolders` call `ResolveOneDriveRoot(Environment.GetEnvironmentVariable)` and `Environment.GetEnvironmentVariable("OneDriveConsumer" / "OneDrivePersonal")` directly. That removes 9 uncovered lines and one misleading affordance. | An injection seam nothing injects through is a maintenance cost with no benefit, and its presence overstates the testability of `LoadFolders`. The executor's own coverage note concedes the constructor cannot be exercised because `LoadFolders` reads machine-specific `Environment.GetFolderPath` values. | `grep -rn "new AppFileSystemFolderPaths" --include=*.cs .` returns 2 hits, neither the seam constructor. `evidence/qa-gates/coverage-delta.2026-08-26T19-50.md` § (d) lists `AppFileSystemFolderPaths.cs:30-35, 37-39` as uncovered with the reason "the internal test-seam constructor body". |
| Minor | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | `:57-104` (`FindInvalidSegmentRule`) vs `:118-127` (`IsLegalFolderName`) | The two folder-name validators now apply different rule sets. `FindInvalidSegmentRule` rejects invalid characters, trailing dot, trailing space, and reserved device names; `IsLegalFolderName` rejects only invalid characters. A name such as `CON` or `Report.` passes one and fails the other. | Route `IsLegalFolderName` through `FindInvalidSegmentRule` so a single rule set governs both. Lower priority than the previous finding, since the cluster containing `IsLegalFolderName` is currently unreachable; if that cluster is deleted, this finding disappears with it. | Two validators for one concept is exactly the duplication this change set out to eliminate elsewhere. Should the cluster ever be revived, the divergence becomes a user-visible inconsistency: the dialog accepts a name the projection later rejects. | Read both methods at `FolderConverter.cs:57-104` and `:118-127`. |
| Minor | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | `:1000-1032` and `:1035-1069` | The D4 filing-boundary guard was added to both `EmailFilerConfig.ResolvePaths` overloads but not to the structurally identical `SortEmail.ResolvePaths` pair, which has three live call sites (`:136`, `:326`, `:475`). Those overloads still build `destinationOlPath = $"{olAncestor}\\{destinationOlStem}"` with no `RequireArchiveRelativeStem` call, and still use the unanchored, case-sensitive `currentFolder.FolderPath.Contains(olAncestor)` guard that `EmailFilerConfig.IsDeleteRelevant` was changed away from. Both overloads carry `[ExcludeFromCodeCoverage]`, so no test observes them. | Promote a follow-up issue to route `SortEmail.ResolvePaths` through `ArchiveStemContract` for both the stem contract and the delete-relevance predicate. Do not widen #614 to cover it: it is outside the confirmed D1–D9 census and the leak itself is already stopped downstream by the new `TryMakeArchiveRelative` gate in `ToFsFolderpath`. | Leaving one of two structurally identical filing paths on the old semantics reintroduces the divergence the contract exists to remove, and the surviving `Contains` guard's precondition no longer matches the tightened contract of the method it protects. The residual risk is low (a nested recurrence of the ancestor string inside a folder path is not realistic), but the maintenance hazard is real. | Read `SortEmail.cs:1000-1069`. `EmailFilerConfig.IsDeleteRelevant` post-change form at `EmailFilerConfig.cs:167-180` shows the anchored replacement the sibling did not receive. |
| Minor | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | `:263` | The invalid-segment `ArgumentException` passes `nameof(fsPath)` as its `paramName`, but `fsPath` is a local variable (`:250`), not a parameter of `ToFsFolderpath`. A caller inspecting `ArgumentException.ParamName` receives a name that does not exist in the signature. The pattern is carried over from the pre-change code. | Use `nameof(olBranchPath)`, which is the parameter whose value actually produced the invalid segment, matching the sibling throw at `:255`. | `ParamName` is part of the exception contract and is read by diagnostics and by tests; a name absent from the signature makes it useless. Cheap to fix. | `FolderConverter.cs:259-266`; compare with the correctly-named throw at `:252-257`. |
| Info | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | `:142-157` (`ToHierarchyPath`) | The relative branch dropped the pre-change `TrimStart('\\', '/')`. A presented target with a single leading separator now takes the full-path branch (`IsFullOutlookPath` is true for a leading `\`), and if it is not at or under the bound root the method returns `null`, leaving the row on single-segment fallback rendering instead of root-prefixing it. | No action. Recorded so the behaviour is not mistaken for an oversight during a future edit. | The plan's stated reasoning ("once full paths are diverted, a relative target cannot lead with a separator, so the `TrimStart` is dead") was verified against the head revision and is correct: any leading-separator value is diverted by `IsFullOutlookPath` before the concatenation is reached. | `BreadcrumbBridgeRouter.cs:142-157`; `ArchiveStemContract.IsFullOutlookPath` leading-separator rule at `ArchiveStemContract.cs:47-51`; reasoning recorded in `plan.2026-08-26T09-59.md` task P3-T2. |
| Info | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` | `:243-258` (`GetStem`) | The out-of-ancestor fallback returns `folderPath.TrimStart('\\')`, so a path outside the archive root still yields an unrooted, non-relative value (for example `mailbox@example.com\Other`) rather than a failure. The XML documentation states this explicitly as deliberate preservation of pre-existing behaviour. | No action for #614. If a future change tightens `OriginOlStem` semantics, this is the site to revisit. | The value feeds `OriginOlStem`, which is used for un-training rather than for filing, so a non-relative value cannot reach the filing boundary. Documented and intentional. | `EmailFilerConfig.cs:243-258`; consumer at `:201`. |

## Design and Structure Assessment

**Contract placement.** `ArchiveStemContract` is correctly located in `UtilitiesCS`, the lowest
assembly in the dependency chain, so `QuickFiler` and `TaskMaster` can both consume it without a new
reference. `EmailFilerConfig` imports it through a `using` alias rather than a namespace import,
which avoids disturbing the file's existing name resolution — a deliberate and appropriate choice.

**Purity.** All three new types are free of I/O, COM, logging, and environment access.
`ArchiveRootPathGuard` in particular is a good pattern: it receives two already-resolved strings and
an `Action<string>` diagnostic sink, which keeps every COM property read in `AppOlObjects` and makes
the decision fully testable. This is the right way to extract logic from a COM-bound class.

**Consistency of the core comparison.** `TryMakeArchiveRelative` is the single implementation of
"at or under the root", and it handles the three cases that the ad-hoc predecessors got wrong:
trailing separators on the root, an exact root match, and the `Archive2`-against-`Archive`
separator-boundary near miss. The `root.Length == 0` guard after `TrimEnd` is reachable (a
separator-only root passes the whitespace check and then trims to empty) and is covered by a test
added specifically for it during Phase 9.

**Diagnostics and redaction.** Every new exception message names the violated rule and states that
the value is withheld. This is applied uniformly across `ArchiveStemContract`,
`ArchiveRootPathGuard`, `AppFileSystemFolderPaths`, `EfcDataModel`, and `FolderConverter`, and it is
verified by dedicated message-content assertions in five test classes. This directly addresses the
open host-identifier-leakage concern (#602).

**Error handling.** Fail-fast is applied consistently. `ArchiveRootPathGuard` and
`ResolveOneDriveRoot` log the redacted diagnostic *before* throwing, so the failure survives a
caller that swallows the exception — a small detail that is easy to omit and was not omitted here.

**Test design.** The tests are readable, independent, and deterministic. Strict-mode Moq is used
where interaction verification matters. The log4net `MemoryAppender` order-dependency defect found
and fixed during Phase 9 (an exact-count assertion against an appender shared per-type across
parallel test classes) was diagnosed correctly and fixed in the right place — the assertion, not the
production code.

## Toolchain Verification

| Gate | Command | Result |
| --- | --- | --- |
| Format | `dotnet tool run csharpier check .` | exit 0, `Checked 1530 files in 3954ms.` |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, 0 errors, 18 DLL outputs, 5 pre-existing System.Reactive advisories |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, 0 errors, 0 CS86xx, 18 DLL outputs |
| Tests | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:TaskMaster.runsettings /InIsolation` | exit 0, 6093 total, 6093 passed, 0 failed, 0 skipped |

## Severity Rollup

| Severity | Count | Blocking |
| --- | ---: | ---: |
| Blocking | 0 | 0 |
| Major | 4 | 0 |
| Minor | 4 | 0 |
| Info | 2 | 0 |
| **Total** | **10** | **0** |

## Recommendation

**Approve for PR.** No finding blocks merge. CR-1 and CR-2 are the two findings worth acting on
soonest, because each can render a legitimate filing destination unusable through the OK path; both
are small, localized changes with existing test coverage to update. CR-3 (the unreachable
alternative-folder-name cluster) and the `SortEmail.ResolvePaths` divergence should be promoted to
follow-up issues rather than absorbed into #614. The two intended-by-spec hard-failure changes in
`AppGlobals` should be the priority items in the outstanding live-Outlook validation.
