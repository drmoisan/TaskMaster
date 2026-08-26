---
name: 614-review-residuals
description: "#614 EFC store-root leak review: PASS/0 blocking; residuals CR-1 (OK-path Length>=3 regression), CR-2 (router/guard rooted-value disagreement), FolderConverter's alternative-folder-name cluster is unreachable dead code, SortEmail.ResolvePaths not migrated"
metadata:
  type: project
---

Cycle-1 review of `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
(head `02092504`, base `main` @ `c279d40b`, 2026-08-26). Verdict **PASS, 0 blocking**, 26/26 AC
(25 PASS, AC25 PARTIAL). Artifacts at
`docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/{policy-audit,code-review,feature-audit}.2026-08-26T16-55.md`.

**Residuals that a later reviewer or maintainer should re-check:**

1. **CR-1 — real functional regression, unaddressed.** `EfcSelectionGuard.IsValidFilingSelection`
   carries `value.Length >= 3`, and `EfcFormController.ActionOkAsync:706` now delegates to it. The
   old OK guard was only `selectedFolder is null || StartsWith("====")`; the `Length < 3` rule came
   from `IsValidSelection`, which gated ONLY folder creation (`:468`, `:752`). So filing to an
   archive subfolder named `HR`/`IT`/`PR`/`Q1` now fails with "Please select a valid folder."
   `EfcSelectionGuardTests.IsValidFilingSelection_TwoCharacterSelection_IsRejected` locks the
   regression in. spec AC16 never asked for a length rule.
2. **CR-2 — guard disagreement.** `BreadcrumbBridgeRouter.SelectRow` deliberately passes a
   rooted-AT-OR-UNDER-root `FilingTarget` through verbatim (preserving the #439 contract asserted by
   the untouched `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`,
   `:165`, `@"\aRcHiVe\Clients\North"`), but `IsValidFilingSelection` rejects EVERY rooted value via
   `IsFullOutlookPath`. That value class is therefore selectable but unfilable. Fix is to normalize
   in `SelectRow` (`CommitSelection(row, stem)`), which requires updating the #439 assertion.
3. **`FolderConverter`'s alternative-folder-name cluster is unreachable dead code.**
   `AlternativeFolderPrompt` -> `AskUserForAlternatives` -> `IsLegalFolderName(string,bool)` ->
   `AlternativeFolderPrompt` is a closed cycle with no production entry point; it was already closed
   at the merge base, and #614 removed the last external caller of the one-arg `IsLegalFolderName`
   (baseline `:161` inside `ToFsFolderpath`). Consequence: AC11 / D5f (`RemoveIllegalCharacters`)
   repairs a dialog option that cannot appear, and the corrected `FolderConverterTests.cs:329`
   assertion tests unreachable behaviour. ~9 tests exercise the cluster. The defect census did NOT
   find this. Adjacent to the promoted
   `2026-08-26-orphaned-duplicate-folderconverter-dead-file-with-always-false-guards.md`.
4. **`SortEmail.ResolvePaths` (both overloads, `:1000` and `:1035`, 3 live call sites) was not
   migrated.** It still concatenates `olAncestor + stem` with no `RequireArchiveRelativeStem`, and
   still uses the unanchored case-sensitive `FolderPath.Contains(olAncestor)` that
   `EmailFilerConfig.IsDeleteRelevant` was changed away from. Both carry `[ExcludeFromCodeCoverage]`.
   Out of the D1–D9 census; the leak itself is still stopped downstream by the new
   `TryMakeArchiveRelative` gate inside `ToFsFolderpath`.
5. **Dead test seam.** `internal AppFileSystemFolderPaths(Func<string,string>)` is called by nothing;
   `_readEnvironmentVariable` can only ever hold the default. Real testability comes from
   `ResolveOneDriveRoot` being `internal static`. Contributes 9 of the 18 uncovered changed lines.
6. **Two intended hard-failure changes need live validation.** `AppOlObjects.ArchiveRootPath` (a
   property getter) and `AppFileSystemFolderPaths.LoadFolders` (runs during add-in startup) now
   throw `InvalidOperationException` where they previously returned a wrong value / fell back. No
   consumer catches it. All five AC26 manual steps are recorded NOT EXECUTED.

**Coverage:** `artifacts/csharp/coverage.xml` is the `<report>` JaCoCo-summary shape. Repo-wide line
**84.8696%** (53972/63594) -> below the 85% hook floor -> policy audit MUST carry an explicit C#
coverage FAIL row. Branch 78.8331% (12741/16162) clears 75%. Baseline was 84.7797%, so the change
improves by +0.0899 — record the FAIL, disposition it non-blocking, and do NOT open remediation
(the workflow's own numeric remediation trigger is <80%, which is cleared).

**AC25 note:** three files stay over the 500-line ceiling (`EfcFormController.cs` 1072,
`BreadcrumbBridgeRouter.cs` 596, `BreadcrumbBridgeRouterIssue439Tests.cs` 694), all pre-existing,
none grown. Ratified as "net non-growth" in the **gitignored**
`artifacts/orchestration/orchestrator-state.json` -> `orchestrator_adjudications`
(2026-08-26T10:38:00Z and 2026-08-26T15:25:00Z). Ratification is by the orchestrator agent, not the
human maintainer, and will not survive merge — it needs transcribing into `issue.md` or the PR body.

**Both PR context artifacts were entirely ABSENT** (not merely misclassified — see
[[pr-context-summary-misclassifies-cs]]); hand-authored from `git diff --numstat` in the
`- <path> (+N/-N)` shape, after which `Get-ChangedLanguageSet` enumerated `CSharp` and the
end-to-end `Invoke-FeatureReviewCoverageValidation` simulation returned Ok.
