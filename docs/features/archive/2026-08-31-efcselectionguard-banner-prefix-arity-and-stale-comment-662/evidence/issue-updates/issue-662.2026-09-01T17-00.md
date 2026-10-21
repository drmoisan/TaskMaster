# Issue 662 — Acceptance Criteria Update Mirror (P2-T21)

Timestamp: 2026-09-01T17-00

PostedAs: unknown

## POSTING BLOCKED

No GitHub post was attempted at this stage. This is a local-only execution run:
the executor was directed not to open a pull request and not to merge anything,
and no issue-body update or comment was posted to
https://github.com/drmoisan/TaskMaster/issues/662. No issue URL and no
`IssueUpdatedAt:` value can therefore be recorded.

The mirror below is the local record of the acceptance-criteria state as it
stands in `issue.md` at the end of Phase 2.

## Exact final text of the `## Acceptance Criteria` section in `issue.md`

```markdown
## Acceptance Criteria

Every occurrence assertion below is scoped with the pathspec `-- '*.cs'` for the reason recorded
under Dependencies / Risks. An unscoped search returns historical audit records and cannot reach
the asserted count.

- [x] AC1 — The guard's rejection breadth is unchanged: its constant still holds the
  three-character value. Verified by `git grep -n -F -- '= "===";' -- '*.cs'` returning exactly
  one line, located in `QuickFiler/Controllers/EfcSelectionGuard.cs`.
- [x] AC2 — The guard's constant is renamed from `BannerPrefix` to `BannerRejectionPrefix`, and
  the new name is used at both `StartsWith` call sites in that file (currently `:49` in
  `IsValidFilingSelection` and `:75` in `IsValidCreationSelection`). Verified by two commands:
  `git grep -nE 'const +string +[A-Za-z_]*BannerPrefix' -- '*.cs'` returning exactly one line,
  located in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`; and
  `git grep -n -F -- 'StartsWith(BannerRejectionPrefix' -- QuickFiler/Controllers/EfcSelectionGuard.cs`
  returning exactly two lines. The second command counts call sites only and is deliberately
  insensitive to how many times the new name appears in doc-comment prose, so AC3's wording cannot
  perturb AC2's count.
- [x] AC3 — `BannerRejectionPrefix` carries an XML doc comment that states three things: that it
  is deliberately a proper prefix of `BreadcrumbRowBuilder.BannerPrefix`; that it therefore
  rejects a strict superset of the producers' banner rows; and that it must not be widened to the
  producers' four-character value, naming the test from AC6 as the guard against that edit.
- [x] AC4 — The `SelectedFolder` comment in `QuickFiler/Controllers/EfcFormController.cs`
  (currently `:318-320`) no longer asserts that `IsValidSelection` keeps a four-character
  rejection. The replacement text describes the composition the code implements: `IsBannerRow`
  matching the producers' four-character prefix, combined with the guard's deliberately broader
  three-character rejection. Verified by reading the replacement text and by AC9's clean
  toolchain pass; no occurrence-count assertion is made against comment prose.
- [x] AC5 — `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` declares no banner-prefix
  constant at all. Its `BannerPrefix` declaration at `:16` is DELETED, not re-aliased, and its
  single reader `IsBanner` (`:195-198`) references `BreadcrumbRowBuilder.BannerPrefix` directly.
  Deletion rather than aliasing is required because an aliasing declaration
  (`private const string BannerPrefix = BreadcrumbRowBuilder.BannerPrefix;`) still matches AC2's
  declaration regex and would make AC2's count two instead of one. Verified by
  `git grep -n -F -- '= "====";' -- '*.cs'` returning exactly one line, located in
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`, and by
  `git grep -n 'BannerPrefix' -- UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`
  returning exactly one line, which is the qualified reference inside `IsBanner`.
- [x] AC5b — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` is NOT modified. Feature
  #498's acceptance criteria assert that file is unmodified, so this work may only add a reader to
  its existing public constant. Verified by
  `git diff 2b85134b42872e405602e6064e02dc9cda6c319b --stat -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`
  reporting no change to that file.
- [x] AC6 — A new MSTest test method named
  `BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` is added to
  `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`. It asserts that
  `IsValidFilingSelection` and `IsValidCreationSelection` each return false for `"==="` and for
  `"===="`, and its FluentAssertions `because` message states that widening the guard to the
  producers' four-character prefix is the prohibited direction. Verified by a scoped
  `vstest.console.exe` run with
  `/Tests:BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` reporting
  `Passed: 1` and `Failed: 0`.
- [x] AC7 — The pre-existing test
  `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` in
  `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` is unmodified and still passes.
  Verified by `git diff 2b85134b42872e405602e6064e02dc9cda6c319b --stat -- QuickFiler.Test/Controllers/EfcFormControllerTests.cs`
  reporting no change to that file, and by a scoped `vstest.console.exe` run with
  `/Tests:IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` reporting
  `Passed: 1` and `Failed: 0`.
- [x] AC8 — No behavioural change reaches `FolderSuggestionTree.IsBanner`, `BreadcrumbRowBuilder`,
  or `EfcFormController.IsBannerRow`. Verified by full-assembly `vstest.console.exe` runs of
  `QuickFiler.Test` and `UtilitiesCS.Test` reporting `Failed: 0` for each, with each assembly's
  `Passed:` count no lower than the count recorded for that same assembly in the Phase 0 baseline
  artifact.
- [x] AC9 — The full C# toolchain passes in one clean pass in the order format, analyze,
  type-check, test, using the exact commands in CLAUDE.md. Each step records an evidence artifact
  carrying `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. The format step's
  artifact records the CSharpier summary line printed on a no-change run, not the exit code alone.
```

All ten criteria are checked. Only the `- [ ]` to `- [x]` transition was made in
each case; no criterion text was edited.

## One note on the reproduced text, recorded outside it

AC5b and AC7 quote the diff anchor `2b85134b42872e405602e6064e02dc9cda6c319b`.
That text is criterion text and was therefore left byte-identical. The plan's
"Execution Amendment — corrected diff anchor (orchestrator, 2026-09-01)"
substituted the run-time-resolved `git merge-base origin/main HEAD` for that
anchor in P2-T16, P2-T18 and P2-T23, because the pinned anchor is an ancestor of
both HEAD and `origin/main` and the two-dot diff form would therefore report
everything `origin/main` accumulated since it. Both gates were verified against
the corrected anchor `43dcc800e5c75ab1d1033f0eac0e4b61ac919b59` and both returned
empty output. The substitution is recorded in
`evidence/baseline/base-commit-resolution.md`,
`evidence/qa-gates/ac5b-verification.md` and
`evidence/qa-gates/ac7-verification.md`.
