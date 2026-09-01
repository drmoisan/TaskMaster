# efcselectionguard-banner-prefix-arity-and-stale-comment

- Work Mode: minor-audit
- Issue: 662
- Type: bug
- Source issue: https://github.com/drmoisan/TaskMaster/issues/662

## Problem / Why

`QuickFiler/Controllers/EfcSelectionGuard.cs:15` declares `private const string BannerPrefix = "===";`
(three `=` characters). Both row producers declare four:

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19` — `public const string BannerPrefix = "====";`
- `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16` — `private const string BannerPrefix = "====";`

Those are the only three `BannerPrefix` declarations in the repository. Three independent
declarations under one name, carrying two different values, is the divergence condition that
produced issue #465 D.

Separately, the comment on the `SelectedFolder` property at `QuickFiler/Controllers/EfcFormController.cs:318-320`
reads:

```
// Derived from the bridge router's selection tracking. The router never selects
// "===="-banner rows, and IsValidSelection keeps its "====" rejection as a second
// guard, so banner rows remain invalid filing targets.
```

`IsValidSelection` routes to `IsSelectableFolder`, which combines `IsBannerRow` (four-character,
via `BreadcrumbRowBuilder.BannerPrefix`) with `EfcSelectionGuard.IsValidCreationSelection`
(three-character). The second guard the comment describes is therefore a three-character
rejection, not the four-character one the comment claims. The comment describes behaviour the
code does not implement.

The defect is latent, not live. No producer emits a three-character row today, so no
user-visible misbehaviour is currently reachable. The cost is to the next maintainer.

## Implementation Intent

This is a consistency and comment-accuracy fix. It is explicitly **not** a behavioural repair
and **not** a redesign of banner-row classification.

### The direction the fix must not take

The guard's three-character prefix is **load-bearing**, and unifying it upward to four
characters would be a behavioural regression:

- `EfcFormController.IsBannerRow` classifies by the four-character producer constant, so
  `IsBannerRow("===")` is `false`.
- The only mechanism that rejects a three-equals row at either EFC classification site is
  `EfcSelectionGuard`'s three-character prefix.
- The merged test `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:453`
  (`IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically`) asserts that
  both `"==="` and `"===="` are rejected on both the creation path and the filing path.

Widening `EfcSelectionGuard`'s constant to `"===="` would make `IsValidFilingSelection("===")`
and `IsSelectableFolder("===")` both return `true` and would break that merged test. The issue
body names this exact hazard: the inaccurate comment "invites a future contributor to 'correct'
the guard in the dangerous direction."

### The direction the fix must take

1. Keep the guard's rejection breadth exactly as it is. Its value stays `"==="`.
2. Rename the guard's constant from `BannerPrefix` to `BannerRejectionPrefix`, so its name no
   longer claims to be the producers' banner prefix, and document in its XML doc that it is
   deliberately a proper prefix of `BreadcrumbRowBuilder.BannerPrefix` and must not be widened.
3. Correct the `SelectedFolder` comment so it describes the rejection the code implements.
4. Remove the duplicated four-character literal in `FolderSuggestionTree` by referencing
   `BreadcrumbRowBuilder.BannerPrefix`, so the producers share one declaration.
5. Add an MSTest regression test that pins the intended relationship, so a future contributor
   who widens the guard breaks a test with an explanatory message rather than silently
   relaxing a guard.

After the change there are two banner-prefix declarations, not three: one producer constant
shared by both producers, and one deliberately broader classifier constant that is documented
and test-pinned as broader.

## Dependencies / Risks

- **Risk: a naive "unify the arity" edit relaxes a merged guard.** Mitigated by AC1, AC6, and AC7.
- **Risk: a stale-literal sweep miscounts, in two distinct ways.**
  1. `"==="` is a substring of `"===="`, so a bare grep for the three-character literal also
     matches every four-character declaration. Every occurrence assertion must anchor on the
     closing quote and semicolon (`= "===";` versus `= "====";`), which cannot cross-match.
  2. The anchored form still over-counts if it is run across the whole tree. Beyond the two
     production declarations, the literal text `= "====";` also appears in historical audit
     records under `docs/features/active/efc-controller-surface-defects-464/`
     (`evidence/qa-gates/sibling-ownership.md:103` and
     `research/2026-08-25T12-20-efc-controller-surface-defects.md:1134`), which are closed-feature
     records and must not be edited, and in this document itself. The unscoped file count is
     therefore scope-dependent and grows as this feature's own documents are authored; no absolute
     unscoped figure is asserted anywhere in this issue. Every occurrence assertion is scoped with
     the pathspec `-- '*.cs'`, and only the scoped counts are operative.

  Pre-change baseline, derived by two independent methods (anchored fixed-string search and a
  `const string` declaration regex), both scoped to `-- '*.cs'`:

  | Pattern (scoped to `*.cs`) | Pre-change | Post-change target |
  |---|---|---|
  | `= "===";` | 1 (`EfcSelectionGuard.cs:15`) | 1 (`EfcSelectionGuard.cs`) |
  | `= "====";` | 2 (`BreadcrumbRowBuilder.cs:19`, `FolderSuggestionTree.cs:16`) | 1 (`BreadcrumbRowBuilder.cs`) |
  | `const +string +[A-Za-z_]*BannerPrefix` | 3 | 1 (`BreadcrumbRowBuilder.cs`) |

  The third row falls to 1 rather than 2 because the guard's constant is renamed to
  `BannerRejectionPrefix`, which does not end in `BannerPrefix`. Zero matches of any of the three
  patterns occur in a `*.Test` assembly, so no test project is in the sweep scope.
- **Dependency:** `FolderSuggestionTree` (namespace `UtilitiesCS`) and `BreadcrumbRowBuilder`
  (namespace `UtilitiesCS.OutlookObjects.Folder`) are in the same assembly, so the constant
  reference is available without a project reference change.
- **No promoted potential record exists.** Neither `docs/features/potential/` nor
  `docs/features/potential/promoted/` carries a record for this issue on `origin/main`, so the
  lifecycle retention check has no source to verify against. Recorded as an observed condition,
  not a blocker. The GitHub issue body is the durable source.

## Acceptance Criteria

Every occurrence assertion below is scoped with the pathspec `-- '*.cs'` for the reason recorded
under Dependencies / Risks. An unscoped search returns historical audit records and cannot reach
the asserted count.

- [ ] AC1 — The guard's rejection breadth is unchanged: its constant still holds the
  three-character value. Verified by `git grep -n -F -- '= "===";' -- '*.cs'` returning exactly
  one line, located in `QuickFiler/Controllers/EfcSelectionGuard.cs`.
- [ ] AC2 — The guard's constant is renamed from `BannerPrefix` to `BannerRejectionPrefix`, and
  the new name is used at both `StartsWith` call sites in that file (currently `:49` in
  `IsValidFilingSelection` and `:75` in `IsValidCreationSelection`). Verified by two commands:
  `git grep -nE 'const +string +[A-Za-z_]*BannerPrefix' -- '*.cs'` returning exactly one line,
  located in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`; and
  `git grep -n -F -- 'StartsWith(BannerRejectionPrefix' -- QuickFiler/Controllers/EfcSelectionGuard.cs`
  returning exactly two lines. The second command counts call sites only and is deliberately
  insensitive to how many times the new name appears in doc-comment prose, so AC3's wording cannot
  perturb AC2's count.
- [ ] AC3 — `BannerRejectionPrefix` carries an XML doc comment that states three things: that it
  is deliberately a proper prefix of `BreadcrumbRowBuilder.BannerPrefix`; that it therefore
  rejects a strict superset of the producers' banner rows; and that it must not be widened to the
  producers' four-character value, naming the test from AC6 as the guard against that edit.
- [ ] AC4 — The `SelectedFolder` comment in `QuickFiler/Controllers/EfcFormController.cs`
  (currently `:318-320`) no longer asserts that `IsValidSelection` keeps a four-character
  rejection. The replacement text describes the composition the code implements: `IsBannerRow`
  matching the producers' four-character prefix, combined with the guard's deliberately broader
  three-character rejection. Verified by reading the replacement text and by AC9's clean
  toolchain pass; no occurrence-count assertion is made against comment prose.
- [ ] AC5 — `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` declares no banner-prefix
  constant at all. Its `BannerPrefix` declaration at `:16` is DELETED, not re-aliased, and its
  single reader `IsBanner` (`:195-198`) references `BreadcrumbRowBuilder.BannerPrefix` directly.
  Deletion rather than aliasing is required because an aliasing declaration
  (`private const string BannerPrefix = BreadcrumbRowBuilder.BannerPrefix;`) still matches AC2's
  declaration regex and would make AC2's count two instead of one. Verified by
  `git grep -n -F -- '= "====";' -- '*.cs'` returning exactly one line, located in
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`, and by
  `git grep -n 'BannerPrefix' -- UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`
  returning exactly one line, which is the qualified reference inside `IsBanner`.
- [ ] AC5b — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` is NOT modified. Feature
  #498's acceptance criteria assert that file is unmodified, so this work may only add a reader to
  its existing public constant. Verified by
  `git diff 2b85134b42872e405602e6064e02dc9cda6c319b --stat -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`
  reporting no change to that file.
- [ ] AC6 — A new MSTest test method named
  `BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` is added to
  `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`. It asserts that
  `IsValidFilingSelection` and `IsValidCreationSelection` each return false for `"==="` and for
  `"===="`, and its FluentAssertions `because` message states that widening the guard to the
  producers' four-character prefix is the prohibited direction. Verified by a scoped
  `vstest.console.exe` run with
  `/Tests:BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` reporting
  `Passed: 1` and `Failed: 0`.
- [ ] AC7 — The pre-existing test
  `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` in
  `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` is unmodified and still passes.
  Verified by `git diff 2b85134b42872e405602e6064e02dc9cda6c319b --stat -- QuickFiler.Test/Controllers/EfcFormControllerTests.cs`
  reporting no change to that file, and by a scoped `vstest.console.exe` run with
  `/Tests:IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` reporting
  `Passed: 1` and `Failed: 0`.
- [ ] AC8 — No behavioural change reaches `FolderSuggestionTree.IsBanner`, `BreadcrumbRowBuilder`,
  or `EfcFormController.IsBannerRow`. Verified by full-assembly `vstest.console.exe` runs of
  `QuickFiler.Test` and `UtilitiesCS.Test` reporting `Failed: 0` for each, with each assembly's
  `Passed:` count no lower than the count recorded for that same assembly in the Phase 0 baseline
  artifact.
- [ ] AC9 — The full C# toolchain passes in one clean pass in the order format, analyze,
  type-check, test, using the exact commands in CLAUDE.md. Each step records an evidence artifact
  carrying `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. The format step's
  artifact records the CSharpier summary line printed on a no-change run, not the exit code alone.

## Verification Steps

1. Capture the Phase 0 baseline: `dotnet tool run csharpier check .`, the two `msbuild` gates, and
   a `vstest.console.exe` run over `QuickFiler.Test` and `UtilitiesCS.Test` with per-assembly
   passed/failed counts.
2. Record the pre-change anchored search results for `= "===";` and `= "====";` across the
   repository, and the pre-change `BannerPrefix` declaration inventory (expected: three).
3. Apply the changes in the four files named in the Implementation Intent.
4. Re-run the anchored searches and confirm the post-change counts asserted by AC1 and AC5.
5. Run the scoped test command naming the new test and the pre-existing test from AC7; confirm
   both are reported passed.
6. Run the full toolchain loop in order and record each step's evidence artifact.

## Evidence Checklist

- [ ] baseline
- [ ] targeted verification
- [ ] end-state
