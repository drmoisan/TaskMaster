# Code Review — issue 662 (efcselectionguard-banner-prefix-arity-and-stale-comment)

Timestamp: 2026-09-01T17-17
Reviewer: feature-review agent
Base: `origin/main` @ `43dcc800e5c75ab1d1033f0eac0e4b61ac919b59`
Head: `8a40a587970f9143e15969e3e233be7dd6b62114`

Every finding below carries an explicit **reachability** statement. Reachability is stated
separately from severity because the underlying defect this branch addresses is itself latent:
no producer emits a three-character banner row today, so no user-visible misbehaviour was
reachable before the change either.

## Files Reviewed

| File | Delta | Nature |
|---|---|---|
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | +27/-4 | constant rename, value unchanged; XML doc rewritten |
| `QuickFiler/Controllers/EfcFormController.cs` | +3/-3 | comment replacement only, no executable change |
| `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` | +5/-2 | delete duplicate constant, qualify its single reader |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | +22/-0 | one added `[TestMethod]` |

Total: 4 files, 57 insertions, 9 deletions. No `.csproj`, `.props` or `.targets` touched. No new
file created; `QuickFiler.Test.csproj:63` already carried a `<Compile Include>` for the test file.

## Overall Assessment

The change is well-executed for what it is: a consistency and comment-accuracy fix that
deliberately does not alter behaviour. The design decision at its centre — keeping the guard's
three-character value and renaming the constant so its name stops claiming to be the producers'
banner prefix — is the correct one, and the reviewer reached that conclusion by tracing the
dispatch path independently rather than by accepting the issue's framing (trace reproduced in
`policy-audit.2026-09-01T17-17.md` under "Independent dispatch trace").

The change eliminates the divergence condition it set out to eliminate: three declarations under
one name with two different values become two declarations with distinct names, one per role.

**No blocking code-quality finding.**

## What the Change Does Well

### 1. It picks the right side of a genuinely dangerous ambiguity

The naive reading of the issue title ("prefix arity") suggests unifying the three-character value
up to four. That edit would relax the only rejection mechanism for a three-equals row at both EFC
classification sites, because `MinimumCreationLength` is 3 and the length rule therefore accepts a
three-character input. The change instead renames the constant so the name no longer implies the
values should match, and documents the asymmetry as intentional.

### 2. The XML doc states the constraint, the reason, and the enforcement

`EfcSelectionGuard.cs:14-37` is three `<para>` blocks that separately state (a) that the value is
deliberately a proper prefix of `BreadcrumbRowBuilder.BannerPrefix`, (b) the consequence — the
guard rejects a strict superset — and (c) the prohibition, with the reason (`MinimumCreationLength`
is 3) and the name of the test that catches the edit. This is "comment why, not what" applied
correctly: a future contributor reading it learns why the values differ before they can decide the
difference is a bug.

The `<see cref="BreadcrumbRowBuilder.BannerPrefix"/>` resolves — `EfcSelectionGuard.cs:2` carries
`using UtilitiesCS.OutlookObjects.Folder;`, and `BreadcrumbRowBuilder` is a `public sealed class`
with a `public const string BannerPrefix`, so the reference is legal across the assembly boundary.
The `<see cref="MinimumCreationLength"/>`, `<see cref="IsValidFilingSelection"/>` and
`<see cref="IsValidCreationSelection"/>` references are same-type and resolve trivially.

The test name at `:34` is deliberately written as plain text rather than a `cref`. That is
correct: a `cref` to a method in `QuickFiler.Test` from `QuickFiler` would not resolve, since the
dependency runs the other way.

### 3. The producers now share exactly one declaration

`FolderSuggestionTree.cs:16` previously declared its own `private const string BannerPrefix = "===="`,
duplicating `BreadcrumbRowBuilder.cs:19`. Deleting it rather than aliasing it is the right call for
two reasons, one of which is subtle: an alias
(`private const string BannerPrefix = BreadcrumbRowBuilder.BannerPrefix;`) would still match the
declaration-inventory regex the issue's AC2 uses, so the count would read two rather than one and
the AC would be unsatisfiable. The plan spotted that and said so.

Verified post-change inventory:

- `git grep -nE 'const +string +[A-Za-z_]*BannerPrefix' -- '*.cs'` -> 1 (`BreadcrumbRowBuilder.cs:19`)
- `git grep -n -F -- '= "====";' -- '*.cs'` -> 1 (`BreadcrumbRowBuilder.cs:19`)
- `git grep -n -F -- '= "===";' -- '*.cs'` -> 1 (`EfcSelectionGuard.cs:38`)

### 4. The replacement comment is accurate against the code it describes

Old text asserted `IsValidSelection` "keeps its `\"====\"` rejection as a second guard" — false;
the second guard is a three-character rejection. New text at `EfcFormController.cs:318-320`:

> `IsValidSelection` routes to `IsSelectableFolder`, which composes `IsBannerRow`, matching the
> producers' `"===="` prefix, with the guard's deliberately broader three-character rejection.

Checked term by term against `EfcFormController.cs:1143-1155`: `IsValidSelection` is
`IsSelectableFolder(SelectedFolder)`; `IsSelectableFolder` is
`!IsBannerRow(f) && EfcSelectionGuard.IsValidCreationSelection(f)`; `IsBannerRow` matches
`BreadcrumbRowBuilder.BannerPrefix` = `"===="`; the guard's prefix is three characters and is
broader. Every clause is accurate. It also correctly drops the old claim that "the router never
selects `\"====\"`-banner rows", which was an assertion about a collaborator the comment cannot
verify.

### 5. The added test pins the relationship rather than restating it

`EfcSelectionGuardTests.cs:294-313`:

- Four assertions: both predicates against both `"==="` and `"===="`.
- A single shared `because` constant, so all four failure messages name the prohibited direction
  and its reason.
- Explicit `// Arrange` and `// Act / Assert` markers.
- A five-line intent comment citing #662 and stating what a contributor who widens the guard will
  see.
- No mock, no clock, no RNG, no `Thread.Sleep`, no I/O, no temporary file. Measured duration
  30.3 ms.

The test is not trivially all-red under the prohibited edit: the two `"===="` assertions still
pass, and only the two `"==="` assertions go red. That is the desirable shape — it localises the
failure to the property actually being relaxed.

This matters because the pre-existing merged test in `EfcFormControllerTests.cs:453` has a blind
spot the new test does not. Under the widening edit, at `:462`
`creationPath.Should().Be(filingPath, …)` — the assertion that *reads* like the consistency guard
— still passes, because both sides flip to `true` together. Only `:463`
`creationPath.Should().BeFalse(…)` fails. The reviewer re-derived this rather than taking it from
the plan; the derivation is in the policy audit. The new test asserts `BeFalse` directly on all
four inputs and therefore does not depend on that one assertion surviving a future edit.

## Findings

All findings are **non-blocking**. None requires remediation before merge.

### CR-1 — `IsBanner` doc comment still hard-codes the producers' arity (Minor)

`UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:193`:

```csharp
/// <summary>True when the row is a section/banner header (begins with <c>"===="</c>).</summary>
private static bool IsBanner(string row)
```

The body below now reads `BreadcrumbRowBuilder.BannerPrefix`, so this doc line is the only place
left in the file that restates the four-character arity as a hard-coded literal. It is accurate
today.

**Reachability: documentation only, latent.** It becomes wrong the moment
`BreadcrumbRowBuilder.BannerPrefix` changes value — precisely the drift class this issue exists to
close, reintroduced one layer up in prose. Not merge-method-dependent, and no runtime path reads
the comment.

**Suggested (post-merge, do not widen this branch):** replace the literal with
`<see cref="BreadcrumbRowBuilder.BannerPrefix"/>`. This edit is deliberately **not** recommended
now: `FolderSuggestionTree.cs` is inside the four-file scope boundary, but the AC set for this
item makes no allowance for it and the branch is otherwise complete.

### CR-2 — Partially-qualified type reference (Informational, no action)

`FolderSuggestionTree.cs:196-200`:

```csharp
return row != null
    && row.StartsWith(
        OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix,
        StringComparison.Ordinal
    );
```

The reference is relative, not fully qualified and not covered by a `using`. It resolves because
the file's namespace is `UtilitiesCS` and relative lookup finds `UtilitiesCS.OutlookObjects.Folder`.
It is consistent with the fully-qualified style already at `EfcFormController.cs:1146`.

**Reachability: none — it compiles and is exercised (hits = 1 on all five lines in the
post-change Cobertura).** Recorded only so a later reader does not "simplify" it into an
ambiguity, and so it is on record that the reviewer checked resolution rather than assuming it.

### CR-3 — `EfcFormController.cs` remains at 1189 lines (Minor, pre-existing)

`.claude/rules/general-code-change.md` caps production files at 500 lines. This file is 1189 at
both the base and the head; the change is +3/-3 comment-only, so it neither causes nor worsens
the violation.

**Reachability: maintainability only.** The file is COM- and WinForms-bound
(`using Microsoft.Office.Interop.Outlook;` at `:13`, `using System.Windows.Forms;` at `:12`), so
splitting it is a seam-extraction refactor far outside a `minor-audit` comment fix. Recorded so it
is not lost; it belongs to whichever item eventually decomposes that controller.

### CR-4 — `System.StringComparison` is written long-form in `EfcSelectionGuard` (Informational, no action)

`EfcSelectionGuard.cs:72` and `:98` use `System.StringComparison.Ordinal` while
`FolderSuggestionTree.cs:199` uses the imported `StringComparison.Ordinal`. The long form in
`EfcSelectionGuard.cs` is pre-existing at both call sites and the change preserved it rather than
opportunistically normalising it. That restraint is correct under the "minimal, targeted fix" rule.

**Reachability: none.** No action.

## Design and Correctness Review

| Aspect | Assessment |
|---|---|
| Ordinal comparison | Both predicates and both `IsBanner` implementations use `StringComparison.Ordinal`. Correct for a sentinel-prefix test; culture-sensitive comparison would be a defect here. |
| Null handling | `IsValidFilingSelection` / `IsValidCreationSelection` guard with `string.IsNullOrWhiteSpace` before `StartsWith`; `FolderSuggestionTree.IsBanner` and `EfcFormController.IsBannerRow` both null-check first. No new null path introduced. |
| Accessibility | `BannerRejectionPrefix` stays `private const`. No public surface widened. `QuickFiler/Properties/AssemblyInfo.cs:5` carries `InternalsVisibleTo("QuickFiler.Test")`, which is how the test reaches the `internal static` predicates — pre-existing, unchanged. |
| Cross-assembly coupling | `FolderSuggestionTree` (namespace `UtilitiesCS`) and `BreadcrumbRowBuilder` (namespace `UtilitiesCS.OutlookObjects.Folder`) are in the same assembly, so no project reference was needed and none was added. Verified. |
| Behaviour preservation | Confirmed by value identity: the guard's constant is byte-identical (`"==="`), and `BreadcrumbRowBuilder.BannerPrefix` is `"===="`, identical to the deleted local constant. No predicate's result changes for any input. |
| Regression risk from the rename | Zero call sites outside the file: `BannerRejectionPrefix` is `private`, and both `StartsWith` uses are in the same file. Verified: `git grep -n -F -- 'StartsWith(BannerRejectionPrefix' -- QuickFiler/Controllers/EfcSelectionGuard.cs` returns exactly 2 lines, and `git grep 'BannerRejectionPrefix' -- '*.cs'` finds it only in that file and in the test's method name. |

## Test Suite Impact

Read from the primary TRX documents, not from prose summaries. All six committed TRX files parse
as well-formed XML after the repair in commit `8a40a587`.

| Assembly | Baseline | Post-change | Failed | Delta |
|---|---|---|---|---|
| `QuickFiler.Test` | 1286 passed | 1287 passed | 0 | +1 |
| `UtilitiesCS.Test` | 4783 passed | 4783 passed | 0 | 0 |

The reviewer diffed the two QuickFiler TRX test-name multisets. The symmetric difference is a
single element in one direction — `BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates`
gained, nothing lost. The `+1` is therefore exactly the added test and nothing else moved. The
UtilitiesCS name multisets are identical in both directions.

Scoped runs, both reporting `total=1 passed=1 failed=0`:

- `evidence/regression-testing/p2-t5/ac6-scoped.trx` — the new test
- `evidence/regression-testing/p2-t6/ac7-scoped.trx` — the pre-existing merged test from AC7

Every `failed`, `error`, `timeout`, `aborted` and `notExecuted` attribute across all six TRX
`<Counters>` elements is `0`.

## Coverage Impact (summary; full detail in the policy audit)

| File | Baseline line | Post line | Baseline branch | Post branch |
|---|---|---|---|---|
| `EfcSelectionGuard.cs` | 100.0000% | 100.0000% | 100.0000% | 100.0000% |
| `FolderSuggestionTree.cs` | 98.4496% | 98.4962% | 96.4286% | 96.4286% |
| `EfcFormController.cs` | 25.5008% | 25.5008% | 31.5126% | 31.5126% |

Every changed executable statement records `hits >= 1` in the post-change Cobertura: 3 of 3
statements covered. `EfcFormController.cs` contributes no changed executable statement (its diff
is comment-only) and its counters are byte-identical across the two captures, so there is no
regression on any changed line.

## Recommendation

**Approve.** The change is minimal, correct, well-documented, fully covered on every line it
touches, and accompanied by a regression test that pins the relationship it establishes. The two
substantive items surfaced during review (CR-1 and CR-3) are documentation and file-size concerns
that predate or sit adjacent to this change and should be handled by separate items rather than by
widening a four-file scope boundary that two acceptance criteria explicitly measure.
