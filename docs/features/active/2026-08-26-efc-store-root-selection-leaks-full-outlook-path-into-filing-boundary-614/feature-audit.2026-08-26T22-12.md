# Feature Audit — Issue #614 (efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary)

- Artifact timestamp: 2026-08-26T22-12
- Review cycle: remediation cycle 1 **exit** re-audit. The prior-cycle record is `feature-audit.2026-08-26T16-55.md`, left in place unmodified.
- Reviewer: feature-review agent

## Scope and Baseline

| Item | Value |
| --- | --- |
| Branch | `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614` |
| Base branch (resolved) | `main` |
| Merge-base SHA | `c279d40bddacdba00c29a9724d1b5b17f9ebbc90` |
| Merge-base resolution | recomputed by this reviewer with `git merge-base main HEAD`; matches the caller-supplied SHA |
| Head SHA | `b45e2a2d5b7f4d4219aa0caea4e63e24777feab1` |
| Commits ahead of base | 25 |
| Working tree | clean |
| Branch diff | 111 files, +10581 / -243 |
| Work Mode marker | `- Work Mode: full-bug` (`issue.md:15`) |
| Authoritative AC source | `spec.md`, section `## Acceptance Criteria` (AC1-AC26) — `full-bug` resolves to `spec.md` only |
| Prior-cycle head | `02092504e50ede2527ae35f14629f0bc4c4c94ff` |
| Commits added since prior review | `6bbb18e7` (cycle inputs), `0fb0efec` (cycle plan), `cbad2da2` (the CR-1/CR-2 remediation), `b45e2a2d` (commit gate and plan close-out) |
| Production files changed by the remediation cycle | `QuickFiler/Controllers/EfcSelectionGuard.cs`, `QuickFiler/Controllers/EfcFormController.cs` |

The audit scope is the full branch diff against the resolved base branch, not the remediation plan's
two-file subset. Every criterion below was re-evaluated at head `b45e2a2d`; carried-forward evidence
was re-verified rather than inherited.

Independent verification performed for this audit: merge base recomputed; both PR context artifacts
regenerated from git plumbing (they were four commits stale); the full four-step toolchain
re-executed; 6111 tests re-run across the three changed test assemblies; the coverage artifact's
counters re-summed by hand and cross-checked against the repository validator's own parsers;
per-file line and branch coverage recomputed from both the merge-base and head Cobertura artifacts;
every changed file's line count measured at both revisions; a redaction sweep run over the whole
diff; and the RC-1 call chain traced from the breadcrumb row builder through to the filing boundary.

## Acceptance Criteria Inventory

| AC | Short title | Source |
| --- | --- | --- |
| AC1 | Contract type `ArchiveStemContract` exists, pure, registered, under 500 lines | `spec.md` |
| AC2 | D1 — `SelectHierarchyPath` no longer stores a verbatim out-of-root value | `spec.md` |
| AC3 | D2 — segment-activation scenarios covered | `spec.md` |
| AC4 | D3 — `SelectRow` guarded; `ToHierarchyPath` no longer fabricates | `spec.md` |
| AC5 | D4 — both `ResolvePaths` overloads enforce the stem contract before concatenation | `spec.md` |
| AC6 | D5a — `ToFsFolderpath` validates only derived segments | `spec.md` |
| AC7 | D5b — per-segment Windows name validation replaces the character blacklist | `spec.md` |
| AC8 | D5c — the `Substring(3)` drive-prefix assumption removed | `spec.md` |
| AC9 | D5d — ancestor strip is prefix-anchored, separator-aware, case-insensitive | `spec.md` |
| AC10 | D5e — exception message no longer embeds the path | `spec.md` |
| AC11 | D5f — "Remove illegal characters" removes only illegal characters | `spec.md` |
| AC12 | D5g — `ResolveOlRoot` separator-terminated; dead `ask` parameter resolved | `spec.md` |
| AC13 | D6 — `ArchiveRootPath` validated once, redacted diagnostic, no per-item COM round trip | `spec.md` |
| AC14 | D7 — `LoadFolders` OneDrive fallbacks removed; injectable seam; `MatchBestSpecialFolder` untouched | `spec.md` |
| AC15 | D8 — `MoveToFolderAsync(MAPIFolder, ...)` derives its stem through the contract | `spec.md` |
| AC16 | D9 — OK path and `IsValidSelection` share one predicate; stated rejection set | `spec.md` |
| AC17 | Primary regression test, fails before and passes after | `spec.md` |
| AC18 | Producer-side companion test, fails before | `spec.md` |
| AC19 | No regression of the #609 / #439 scenarios | `spec.md` |
| AC20 | Non-absorbing interaction with open issue #499 | `spec.md` |
| AC21 | Redaction — no real identifiers anywhere in the change | `spec.md` |
| AC22 | Test-policy compliance (MSTest / Moq / FluentAssertions, deterministic, mirrored trees) | `spec.md` |
| AC23 | Coverage — new and changed methods `>= 90%`, no changed line loses coverage, artifacts captured | `spec.md` |
| AC24 | Full four-step toolchain, one clean pass, exact commands and exit codes, non-vacuity | `spec.md` |
| AC25 | Scope isolation and the 500-line file-size limit | `spec.md` |
| AC26 | Manual validation against a live Outlook profile | `spec.md` |

Total: **26** criteria, all in `- [x]` / `- [ ]` checkbox form under the exact heading
`## Acceptance Criteria`.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence and reviewer verification |
| --- | --- | --- |
| AC1 | **PASS** | `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` exists, 147 lines. Registered in `UtilitiesCS.csproj` as an explicit `<Compile Include>`. Exposes all three named members. Reviewer read the file end to end at this head: no filesystem, network, COM, or environment access; no `init`, `record`, or `record struct` (net481-safe). The drive-rooted decision is recorded in the XML documentation at `:32-37`. Unchanged by the remediation cycle. |
| AC2 | **PASS** | `SelectHierarchyPath` (`BreadcrumbBridgeRouter.cs:485-497`) returns early when `TryMakeArchiveRelative` fails or yields an empty stem, logging a diagnostic; the former verbatim-return helper is deleted. Unchanged by the remediation cycle; the covering tests are green in the reviewer's run. |
| AC3 | **PASS** | All six scenarios present in `BreadcrumbBridgeRouterIssue614Tests`, all green in the reviewer's 6111-test run. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` appears in no diff output. |
| AC4 | **PASS (with a reviewer caveat)** | The `SelectRow` guard is present at `BreadcrumbBridgeRouter.cs:481-491` and `ToHierarchyPath` returns `null` for an out-of-root full path instead of prefixing it. **Caveat:** the guard is deliberately scoped to *out-of-root* full paths, so a rooted at-or-under-root target — including the archive root itself — still passes through verbatim. At the prior head that produced a dead-end selection; at this head it produces the RC-1 crash path. The criterion as written concerns out-of-root targets only and remains satisfied; the consequence is recorded as code-review RC-1. |
| AC5 | **PASS** | `EmailFilerConfig.cs:196-200` and `:210-215` both call `ArchiveStemContract.RequireArchiveRelativeStem(DestinationOlStem, nameof(DestinationOlStem))` immediately before the concatenation. `GetStem` (`:250-258`) and `IsDeleteRelevant` (`:167-180`) both route through `TryMakeArchiveRelative`; the unanchored `Replace` and `Contains` forms are gone. Unchanged by the remediation cycle. Reviewer note: this is the boundary RC-1 collides with — the criterion is met, and it is the guard *upstream* of it that now admits values this boundary rejects. |
| AC6 | **PASS** | `FindInvalidSegmentRule` is invoked only on the derived relative portion (`FolderConverter.cs:262`); `fsAncestorEquivalent` is never validated. `ToFsFolderpath_DottedAndHyphenatedFilesystemRoot_Succeeds` is green. |
| AC7 | **PASS** | `IllegalFolderCharacters` is now `Path.GetInvalidFileNameChars()`; `FindInvalidSegmentRule` adds the trailing-dot, trailing-space and reserved-device-name rules. Each of the four rules has a positive and a negative test in `FolderConverterIssue614Tests`, all green. |
| AC8 | **PASS** | No `Substring(3)` remains in `ToFsFolderpath`. `ToFsFolderpath_UncAncestor_NeitherThrowsNorManglesThePath` and `ToFsFolderpath_AncestorShorterThanThreeCharacters_DoesNotThrowOutOfRange` are green. |
| AC9 | **PASS** | The strip is `TryMakeArchiveRelative` (`ArchiveStemContract.cs:106-145`), which is prefix-anchored, separator-terminated, and ordinal case-insensitive. Both covering tests are green. |
| AC10 | **PASS** | The message at `FolderConverter.cs:248-252` contains the violated rule only; the value is explicitly withheld. `ToFsFolderpath_InvalidSegment_MessageLeaksNeitherMailboxNorFsAncestor` asserts the absence of both. |
| AC11 | **PASS (with a reviewer caveat)** | `RemoveIllegalCharacters` removes only the illegal characters and the dictionary entry calls it; `FolderConverterTests.cs:329` was corrected from an empty-result assertion to the specific corrected value. **Caveat unchanged from the prior cycle:** the whole alternative-folder-name cluster has no production entry point, so the D5f fix repairs a dialog option that cannot be reached. See code-review carried finding (prior CR-3). |
| AC12 | **PASS** | `ResolveOlRoot` uses `TryMakeArchiveRelative` for both roots; the `Archive2` near-miss test is green. The `bool ask = true` parameter is removed, the choice is stated in the change description, and the whole-solution rebuild exits 0. |
| AC13 | **PASS (with a reviewer caveat)** | `AppOlObjects.ArchiveRootPath` (`:241-266`) delegates to `ArchiveRootPathGuard.RequireResolvedArchiveRoot` inside the `_archiveRootPath is null` lazy-initialisation branch, so no per-filed-item COM round trip is added on the success path. Six tests in `AppOlObjectsArchiveRootValidationTests` use `Mock<IOlObjects>`; no live Outlook is required. **Caveat:** the getter throws where it previously always returned a string. The remediation cycle added the only catch of that exception anywhere in the codebase, and it covers one of nine reads (code-review RC-3). Also noted: on the failure path the cache is not populated, so every subsequent read retries the COM resolution and logs again. |
| AC14 | **PASS (with a reviewer caveat)** | The `AppData` and `SpecialFolders.First().Value` fallbacks are deleted; `ResolveOneDriveRoot` fails with the redacted diagnostic. Seven tests in `AppFileSystemFolderPathsOneDriveResolutionTests`, none mutating process environment state. `MatchBestSpecialFolder` is unmodified. **Caveat unchanged:** the injectable seam constructor is called by nothing; the real testability comes from `ResolveOneDriveRoot` being `internal static`. |
| AC15 | **PASS** | `EfcDataModel.ToArchiveRelativeStem` (`:373-385`) is the extracted pure helper and `MoveToFolderAsync(MAPIFolder, ...)` calls it at `:345`. Eight tests in `EfcDataModelIssue614Tests` cover under-root, store-root, archive-root-itself, cross-store, case-differing-ancestor, separator-boundary near-miss, and repeated-ancestor-substring inputs. Both live callers are unchanged and the rebuild exits 0. |
| AC16 | **PARTIAL** | *What holds.* Both call sites delegate to one shared guard type, `EfcSelectionGuard`. Tests prove the OK predicate rejects `null`, `string.Empty`, whitespace, the `"==== SUGGESTIONS ===="` sentinel, a store-rooted path, a drive-rooted path, an above-root rooted path, a cross-store rooted path, a separator-boundary near miss, and any rooted value when no archive root is available; and accepts a valid relative stem. The prior-cycle CR-1 defect this criterion inherited — the OK path rejecting short names — is **fixed**, and two regression tests pin it. *What does not hold.* The criterion's literal text requires the two call sites to "share one predicate"; after the remediation they delegate to two different predicates (`IsValidFilingSelection` and `IsValidCreationSelection`). The criterion also requires that "OK rejects … a non-relative selection"; OK now **accepts** a rooted value that resolves against the archive root, including the archive root itself, asserted by `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` and `IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted`. That acceptance is the mechanism of blocking finding RC-1, because the D4 boundary AC5 governs still throws on the same value. `spec.md` was not amended and AC16 remains checked `[x]`. See code-review RC-1 and RC-2. |
| AC17 | **PASS** | `EmailFilerConfig_Tests.cs:292` contains `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers` with exactly the specified inputs, asserting both the contract exception and the absence of host identifiers. Fail-before evidence `evidence/regression-testing/p1-t2-primary-regression-fail-before.2026-08-26T11-45.md` (EXIT_CODE 1, `ExpectedExitCode: 1`); pass-after verified in the reviewer's own run. |
| AC18 | **PASS** | `BreadcrumbBridgeRouterTests.cs:442` contains `Issue614_SegmentActivate_StoreRootSegment_DoesNotStoreFullOutlookPath`. Fail-before evidence `evidence/regression-testing/p1-t4-producer-companion-fail-before.2026-08-26T16-05.md` (EXIT_CODE 1, `ExpectedExitCode: 1`); pass-after verified in the reviewer's own run. |
| AC19 | **PASS (documented carve-out, unchanged)** | `FolderPredictor.cs` appears in no diff output. All `Issue439*` and `Issue609*` tests are green in the reviewer's 6111-test run, including `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` (`BreadcrumbBridgeRouterIssue439Tests.cs:165`), which the remediation cycle did not touch. The one #439 test whose assertions were rewritten during the delivery cycle (`Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic`) covers scenarios outside the six AC19 enumerates and the correction was pre-registered in the plan. Reviewer note: RC-1's recommended fix will require changing the asserted value in `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` from the rooted form to its stem; that is a further documented spec correction, not a regression. |
| AC20 | **PASS** | Selection clearing is a single `_selectedRowId = null;` in `BindRowsAsync`, present identically at baseline and head; no `SelectedFolderPath = null` assignment exists anywhere in the file at either revision. Both rejection paths `return` without touching `SelectedFolderPath`. The change description carries the required single paragraph stating #499 remains open and unregressed. Unchanged by the remediation cycle. |
| AC21 | **PASS** | Reviewer-run sweep over the full branch diff, including all 33 files the remediation cycle added: the only identifiers present are the fabricated placeholders (`mailbox@example.com`, `other-mailbox@example.com`, `C:\Users\testuser\OneDrive - Contoso`). The new `RootUnavailableDiagnostic` constant (`EfcSelectionGuard.cs:30-31`) names no path, mailbox, host, or account, and `ResolveArchiveRootOrEmpty_AccessorThrowsInvalidOperation_DegradesToEmpty` asserts the sink receives exactly that constant. Corroborated by `evidence/qa-gates/redaction-sweep.2026-08-26T22-44.md`. The pre-existing `<PublishUrl>` leak in `TaskMaster.csproj` is untouched and already promoted separately. |
| AC22 | **PASS** | All 23 tests in `EfcSelectionGuardTests` use MSTest attributes and FluentAssertions in explicit Arrange-Act-Assert form. Moq is deliberately not used there and the class documentation states why (pure predicates; inline delegate seams); Moq is used where collaborators exist. Reviewer grep across all new and modified test files for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random`, `Path.GetTempPath`, `Path.GetTempFileName`, `Environment.SetEnvironmentVariable`, `File.WriteAllText`: zero hits. All test files are in the mirrored `*.Test` trees. |
| AC23 | **PASS (with a path note and a scope note)** | Reviewer-measured new-code coverage: `ArchiveStemContract.cs` 100.0000% line / 100.0000% branch, `EfcSelectionGuard.cs` 100.0000% / 100.0000% (31 instrumented lines, up from 9), `ArchiveRootPathGuard.cs` 100.0000% / 90.0000%. All clear the `>= 90%` line gate. No changed line lost coverage: every line the remediation replaced was already uncovered at the merge base. Merge-base and post-change artifacts are both captured, and the repository figure rises from 84.7797% to 84.8790%. **Path note:** the AC names `<FEATURE>/evidence/coverage/`, which is not a canonical sub-path; the executor correctly used `evidence/qa-gates/`. No artifact was written to any prohibited `artifacts/` evidence prefix. **Scope note:** 100% coverage on the guard is coverage of each predicate in isolation and is not evidence that the composed OK path is correct — RC-1 sits in the composition, which no test exercises. |
| AC24 | **PASS (with a path note)** | `evidence/qa-gates/toolchain-clean-pass.2026-08-26T22-40.md` records the remediation cycle's clean pass with all four commands and exit codes. **Independently re-executed by this reviewer at head `b45e2a2d`:** csharpier check exit 0 (1530 files); analyzer rebuild exit 0; nullable rebuild exit 0 with zero `CS86xx`; tests exit 0 with 6111 passed / 0 failed. `/p:Nullable=enable` was not added and `/t:Rebuild` was not replaced by `/t:Build`; both MSBuild steps produced 18 assembly outputs, so `CoreCompile` was not skipped. **Path note:** the AC names `evidence/qa/`; the executor correctly used the canonical `evidence/qa-gates/`. |
| AC25 | **PARTIAL** | *Scope half — PASS.* `FolderPredictor.cs`, `UtilitiesCS/EmailIntelligence/FolderConverter.cs`, and `MatchBestSpecialFolder` appear in no diff output. The remediation cycle touched exactly the two production files its plan named, confirmed by `git show cbad2da2 --stat`. *Size half — PARTIAL.* Reviewer-measured with `awk 'END{print NR}'` at both revisions: `EfcFormController.cs` 1079 (merge base 1084), `BreadcrumbBridgeRouter.cs` 596 (596), `BreadcrumbBridgeRouterIssue439Tests.cs` 694 (694). The criterion's literal text — "No production, test, or script file exceeds 500 lines after the change" — is not satisfied. The applied reading is net non-growth, which holds: no over-limit file grew against the merge base, and `EfcFormController.cs` is 5 lines smaller despite the remediation adding 7. That reading is ratified only in the **gitignored** `artifacts/orchestration/orchestrator-state.json` under `orchestrator_adjudications`, by the orchestrator agent rather than the human maintainer, and will not survive merge. |
| AC26 | **PARTIAL** | The criterion's escape clause ("Any step that cannot be executed is recorded as not executed, with the reason") is satisfied: all five steps are recorded as NOT EXECUTED with a concrete reason and a named passing headless counterpart in `evidence/qa-gates/manual-validation.2026-08-26T18-55.md`. **Downgraded from the prior cycle's PASS** for one reason: the remediation cycle added new production behaviour on the OK path — an archive-root read, a swallowed exception, and a widened predicate — and produced no additional manual-validation record for it. The prior cycle's artifact predates that behaviour by four hours and does not cover it. Blocking finding RC-1 is precisely the class of defect a live-profile OK-path walkthrough would surface, and no live validation of the OK path has occurred at any point in this feature's history. |

## Summary

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/spec.md`, section `## Acceptance Criteria`
- Total AC items: 26
- Checked off (delivered): 26
- Remaining (unchecked): 0
- Items remaining: none

Reviewer note: the checkbox state and the reviewer verdict diverge on three criteria. See
`## Acceptance Criteria Check-off`.

### Reviewer verdict distribution

| Verdict | Count | AC numbers |
| --- | ---: | --- |
| PASS | 23 | AC1-AC15, AC17-AC24 |
| PARTIAL | 3 | AC16, AC25, AC26 |
| FAIL | 0 | — |
| UNVERIFIED | 0 | — |

Movement against the prior cycle: AC16 moves PASS (with caveat) to PARTIAL, because the remediation
changed the implementation away from the criterion's text without amending it. AC26 moves PASS to
PARTIAL, because new production behaviour was added on the OK path with no corresponding validation
record. AC25 is unchanged. No criterion improved from PARTIAL to PASS.

Seven of the 23 PASS verdicts carry a recorded caveat that does not defeat the criterion: AC4 (the
`SelectRow` scope pinning, now the mechanism of RC-1), AC5 (the boundary RC-1 collides with), AC11
(the D5f fix repairs unreachable code), AC13 and AC14 (hard-failure behaviour changes and the inert
seam), AC19 (a pre-registered spec correction to one #439 test), and AC23 and AC24 (the AC text
names non-canonical evidence sub-paths; the executor used the canonical ones).

### Feature completeness

All nine confirmed defects (D1-D9) remain addressed and the delivery cycle's defence-in-depth
structure is intact: the reported leak is stopped independently at the producer
(`BreadcrumbBridgeRouter`), the filing boundary (`EmailFilerConfig.ResolvePaths`), and the converter
(`FolderConverter.ToFsFolderpath`).

The remediation cycle closed prior finding CR-1 cleanly. It did not close prior finding CR-2; the
chosen remedy reconciled the filing guard with one of the two other guards on the same value and
left it in conflict with the other, converting a benign dialog rejection into an unhandled exception
raised after the form is hidden. That is blocking finding RC-1.

The feature is therefore **not complete**. The remaining work is narrow and well understood: move the
normalization to the producer, restore the filing predicate's rootedness rejection, update one
`Issue439` assertion, add one composition test, and amend AC16 to match.

### Verification independence

This audit did not rely on executor- or orchestrator-authored evidence for any verdict. The reviewer
independently recomputed the merge base; regenerated both stale PR context artifacts; re-ran all
four toolchain gates; ran 6111 tests; re-summed the coverage artifact by hand and cross-checked it
against the repository validator's own parsers; recomputed per-file line and branch coverage from
both the merge-base and head Cobertura artifacts; measured every changed file's line count at both
revisions; ran a redaction sweep over the whole diff; and traced the RC-1 call chain from
`BreadcrumbRowBuilder` through `FolderPredictor.ProjectSuggestionPath`, `SelectRow`,
`ActionOkAsync`, `ExecuteMovesCoreAsync`, `EfcDataModel.MoveToFolderAsync` and `EmailFiler.SortAsync`
to `EmailFilerConfig.ResolvePaths`.

Every executor-claimed figure that was re-measured matched, including the caller-supplied coverage
counters. One documentation figure was corrected: the prior PR context summary reported 22 changed
`.cs` files; direct enumeration returns 21.

### Go / no-go

**NO-GO.** One blocking finding (RC-1) and three PARTIAL acceptance criteria.

Required before merge:

1. Close RC-1 by normalizing the selection in `BreadcrumbBridgeRouter.SelectRow` and restoring the filing predicate's rejection of rootedness, with the two supporting changes RC-1 names.
2. Amend AC16 in `spec.md` to describe the delivered design, and clear its checkbox until the amended criterion is met.

Recommended in the same cycle: RC-3 and RC-4, both cheap and adjacent.

Recommended before release, unchanged from the prior cycle: execute the five live-Outlook validation
steps, prioritising the OK path and the two `AppGlobals` hard-failure changes; transcribe the
gitignored file-size adjudication into `issue.md` or the PR body; and promote the `FolderConverter`
dead cluster and the un-migrated `SortEmail.ResolvePaths` pair to their own issues rather than
absorbing them into #614.

## Acceptance Criteria Check-off

Per `acceptance-criteria-tracking`, the reviewer checks off passing criteria in the authoritative
source file and leaves non-passing criteria unchecked.

- **Newly checked off by this review: none.** All 26 checkboxes in `spec.md` were already `[x]` when this review began, having been checked off by the executor during the delivery cycle (commit `ff04bf0a`) and left unchanged by the remediation cycle.
- **AC1-AC15 and AC17-AC24:** reviewer verdict PASS. The existing `[x]` marks are confirmed correct and are left in place.
- **AC16:** reviewer verdict **PARTIAL**. The `[x]` mark is left in place but **should not be**. The criterion's text is now contradicted by the implementation on two clauses, and the contradiction is the mechanism of blocking finding RC-1. This reviewer did not clear the checkbox because doing so would alter an AC source file mid-audit and because AC16 has to be rewritten during remediation cycle 2 in any case. **Required action for cycle 2:** amend the AC16 text to describe the delivered design, clear the checkbox, and re-check it only when the amended criterion is met.
- **AC25:** reviewer verdict PARTIAL, unchanged from the prior cycle. The `[x]` mark rests on the net-non-growth reinterpretation ratified in the gitignored `artifacts/orchestration/orchestrator-state.json`, not on the criterion's literal text. Because that ratification is gitignored and will not survive merge, it should be transcribed into `issue.md` or the PR body, and a follow-up issue should be opened to split the three over-limit files. The mark is left in place: the deviation was disclosed rather than concealed, and the criterion is met in substance.
- **AC26:** reviewer verdict PARTIAL, downgraded from the prior cycle's PASS. The `[x]` mark rests on the criterion's own escape clause, which is satisfied for the delivery cycle's behaviour but not for the behaviour the remediation cycle added. The mark is left in place; the substantive gap is recorded above and in the policy audit § 8 G4.
- No criterion was added, removed, or reworded by this review, and no criterion text was altered.
