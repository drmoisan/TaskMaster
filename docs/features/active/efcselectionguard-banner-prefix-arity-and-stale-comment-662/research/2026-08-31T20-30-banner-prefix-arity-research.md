# Research — EfcSelectionGuard banner-prefix arity and stale comment (issue #662)

- Timestamp: 2026-08-31T20-30
- Issue: 662
- Work mode: minor-audit (bug)
- Measurement base: worktree on branch `bug/efcselectionguard-banner-prefix-arity-and-stale-comment-662`,
  HEAD `2b85134b42872e405602e6064e02dc9cda6c319b`. All paths below are relative to `<repo-root>`,
  which is that worktree root.
- Scope: read-only. No source file, configuration, or file under
  `docs/features/active/efc-controller-surface-defects-464/` was modified.

## Search-tool note (applies to every count in this document)

All searches were executed with the repository's ripgrep-backed content search from the worktree
root. `git grep` was not used, because no shell was invoked for this research; the ripgrep `--glob`
filter `*.cs` is the operational equivalent of the `-- '*.cs'` pathspec the issue's acceptance
criteria name, and the two are stated separately wherever the distinction matters.

The substring trap is real and was controlled for: `"==="` is a prefix of `"===="`, so the
fixed-string form `= "===";` is safe (the closing quote cannot cross-match) while a bare `"===`
is not. Every count below carries a second, structurally different query as a cross-check.

---

## Summary of verdicts on the eight orchestrator hypotheses

| # | Hypothesis | Verdict |
|---|---|---|
| 1 | Three `BannerPrefix` declarations, repo-wide | **CONFIRMED** |
| 2 | `IsBannerRow` classifies on the four-character producer constant; `IsBannerRow("===")` is `false` | **CONFIRMED** |
| 3 | `IsSelectableFolder` = `!IsBannerRow(x) && EfcSelectionGuard.IsValidCreationSelection(x)` | **CONFIRMED** |
| 4 | `ArchiveStemContract.IsFullOutlookPath("===")` is `false` | **CONFIRMED** |
| 5 | The guard's three-character prefix is the only mechanism rejecting a three-equals row at either EFC site | **CONFIRMED**, with the added detail that `MinimumCreationLength` does not assist (3 >= 3) |
| 6 | The merged test at `EfcFormControllerTests.cs:453` rejects both arities on both paths | **CONFIRMED** |
| 7 | Widening the guard to `"===="` breaks that test | **CONFIRMED**, and the failing assertion is identified precisely (see Q2) |
| 8 | `EfcSelectionGuardTests.cs` contains only four-equals literals, so widening passes it silently | **CONFIRMED** (`:43`, `:183`, `:245`) |

No hypothesis was refuted. Two corrections to peripheral claims in `issue.md` are recorded under
"Corrections to the issue document" below; neither changes the intended remedy.

---

## Q1 — Completeness of the classification-site census

### Q1.1 `BannerPrefix` declarations

Exactly three, all in `*.cs`, and no fourth declaration or fourth arity exists anywhere in the tree.

| Site | Declaration | Assembly | Role |
|---|---|---|---|
| `QuickFiler/Controllers/EfcSelectionGuard.cs:15` | `private const string BannerPrefix = "===";` | QuickFiler | rejection prefix (consumer) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19` | `public const string BannerPrefix = "====";` | UtilitiesCS | producer/classifier |
| `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16` | `private const string BannerPrefix = "====";` | UtilitiesCS | producer/classifier |

### Q1.2 Every reference to those constants

All twelve `.cs` lines carrying the identifier `BannerPrefix` (production and test):

| Line | Kind |
|---|---|
| `QuickFiler/Controllers/EfcSelectionGuard.cs:15` | declaration |
| `QuickFiler/Controllers/EfcSelectionGuard.cs:49` | reference (`IsValidFilingSelection`) |
| `QuickFiler/Controllers/EfcSelectionGuard.cs:75` | reference (`IsValidCreationSelection`) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19` | declaration |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:159` | reference (`Classify`) |
| `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16` | declaration |
| `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:197` | reference (`IsBanner`) — the **only** reader of that private constant |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs:369` | cross-type reference to `BreadcrumbRowBuilder.BannerPrefix` (`BreadcrumbStateRow.IsBanner`) |
| `QuickFiler/Controllers/EfcFormController.cs:1146` | cross-assembly reference to `BreadcrumbRowBuilder.BannerPrefix` (`IsBannerRow`) |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:419` | comment |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:433` | assertion that the producer constant is `"===="` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs:20` | comment |

### Q1.3 Prefix-based banner classifiers (the complete family)

Four production predicates classify a row as a banner by string prefix. Three consume
`BreadcrumbRowBuilder.BannerPrefix` (`"===="`), one consumes its own four-equals literal, and one
(the guard) uses three:

| Predicate | Location | Prefix source | Effective arity |
|---|---|---|---|
| `BreadcrumbRowBuilder.Classify` | `BreadcrumbRowBuilder.cs:152-170`, test at `:159` | own `public const` | 4 |
| `BreadcrumbStateRow.IsBanner` | `BreadcrumbStateModel.Row.cs:368-370` | `BreadcrumbRowBuilder.BannerPrefix` | 4 |
| `EfcFormController.IsBannerRow` | `EfcFormController.cs:1143-1148` | `BreadcrumbRowBuilder.BannerPrefix` | 4 |
| `FolderSuggestionTree.IsBanner` | `FolderSuggestionTree.cs:195-198` | own `private const` (duplicate literal) | 4 |
| `EfcSelectionGuard.IsValidFilingSelection` / `IsValidCreationSelection` | `EfcSelectionGuard.cs:49`, `:75` | own `private const` | **3** |

Downstream consumers of `BreadcrumbStateRow.IsBanner` (no independent prefix logic of their own):
`BreadcrumbStateModel.Row.cs:104`, `BreadcrumbStateModel.cs:102`,
`FolderBreadcrumbBridgeRouter.cs:165`, `FolderBreadcrumbBridgeRouter.SearchPresentation.cs:90`.

Downstream consumers of `BreadcrumbRowKind.Banner` / `FolderSuggestionNodeKind.Banner` (kind
already assigned upstream, no prefix logic): `BreadcrumbRowBuilder.cs:101`, `:106`;
`BreadcrumbHtmlRenderer.cs:104`; `BreadcrumbBridgeRouter.Selection.cs:85`, `:212`;
`FolderSuggestionTree.cs:117`, `:134`, `:151`, `:168`; `FolderProbabilityAdapter.cs:41`.

### Q1.4 Non-prefix banner mechanisms found (reported for completeness; not in 662's scope)

1. **Exact-string sentinel list, QFC filing readiness.** `QuickFiler/Controllers/QfcCollectionController.cs:217-226`
   builds a three-element `string[] headers` of the exact producer banner texts and rejects a group
   whose `SelectedFolder` is `null` or `headers.Contains(...)`. This is exact equality, not a prefix,
   so it is arity-insensitive and is unaffected by any change contemplated by 662. A dead twin exists
   at `QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1207-1209` (Legacy tree, uncompiled orphan class
   family per the #464 audit).
2. **Explicit `FolderRowKind.Separator` construction.** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:819`,
   `:835`, `:869` construct separator rows by passing the kind, never by re-deriving it from a prefix.
   `FolderPredictor.cs:789`, `:799`, `:806` add the banner strings to the legacy `string[]` path.
3. **A non-equals banner sentinel in a test.** `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionHighlightTests.cs:283`
   declares `private const string BannerText = "-- Suggested folders --";`, used at `:202` via
   `CreateRow("banner", BannerText, false)` — selectability is passed explicitly, not derived from a
   prefix. This is **not** a fourth arity and does not participate in prefix classification.
4. **`CtfIncidenceList.cs:140`** writes `"==============================="` (31 equals) into
   `EmailFolders[i]` specifically so the value is not accepted from a selection list. Prefix-length
   insensitive.

### Q1.5 Answer to the "is there a fourth arity" question

**No.** Every banner string actually emitted by a production producer carries **seven or more**
equals characters:

- `FolderPredictor.cs:789` — `"======= RECENT SELECTIONS ========"` (7 leading)
- `FolderPredictor.cs:799` — `"======= SEARCH RESULTS ======="` (7 leading)
- `FolderPredictor.cs:806` — `"========= SUGGESTIONS ========="` (9 leading)
- `CtfIncidenceList.cs:140` — 31 equals

Every one of those starts with both `"==="` and `"===="`, which is the mechanical reason the defect
is latent: no producer emits a string that the two arities classify differently. There is no `.cs`
string literal in the repository whose value is exactly `"====="` or longer as a bare constant
declaration; the only two constant-declaration arities present are 3 and 4, enumerated in Q1.1.

---

## Numeric Derivation Evidence

The three numeric assertions below back `issue.md` AC1, AC2, and AC5. Each is derived twice by
structurally distinct queries over an identical, exhaustive scope, and the member sets are compared.

### N1 — Banner-prefix constant declarations in C# source

- **Complete family:** every `const string` declaration in the repository whose simple name ends in
  `BannerPrefix`, in any assembly, any accessibility, production or test.
- **Exhaustive search scope:** the entire worktree tree, filtered to `*.cs`. No directory exclusion,
  no assembly restriction. (Unscoped runs additionally return markdown under `docs/`; those are
  excluded by the `*.cs` filter for the reason stated in `issue.md` Dependencies / Risks.)
- **Inclusion rules:** a line that both declares a `const string` and binds a name ending in
  `BannerPrefix`.
- **Exclusion rules:** references, XML-doc mentions, comment prose, markdown, and test-local banner
  constants whose names do not end in `BannerPrefix` (for example `BannerText`, `SuggestionsBanner`,
  `SearchBanner`, `Banner`).
- **Primary search strategy / query expression:** identifier-occurrence census then manual
  declaration/reference partition — regex `BannerPrefix`, glob `*.cs`, content mode, unlimited head.
- **Primary member set:**
  1. `QuickFiler/Controllers/EfcSelectionGuard.cs:15`
  2. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`
  3. `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16`
  (The same query returned nine further lines, all classified as references or comments in Q1.2.)
- **Primary count:** 3
- **Cross-check search strategy / query expression:** declaration-shape regex over a *wider* name
  family, deliberately broadened so that any differently-named banner constant would surface —
  regex `const\s+string\s+\w*[Bb]anner\w*`, glob `*.cs`, content mode, unlimited head.
- **Cross-check member set:** nine lines returned. Partitioned by the inclusion rule:
  - Members (name ends in `BannerPrefix`): `EfcSelectionGuard.cs:15`,
    `BreadcrumbRowBuilder.cs:19`, `FolderSuggestionTree.cs:16`.
  - Non-members (test-local, different names, excluded by rule):
    `UtilitiesCS.Test/.../FolderSuggestionTreeStateTests.cs:18`,
    `FolderSuggestionTreeHierarchyTests.cs:19` and `:20`, `FolderProbabilityAdapterTests.cs:19`,
    `FolderBreadcrumbBridgeRouterReplaceItemsTests.cs:22`,
    `BreadcrumbSelectionSessionHighlightTests.cs:283`.
- **Cross-check count:** 3
- **Member-set comparison:** the two normalized member sets are identical, element for element and
  line for line. The cross-check's broader name family surfaced six additional constants, none of
  which is a `BannerPrefix` declaration; this establishes that the family is exhaustively covered
  rather than that a narrow pattern happened to agree with itself. **Assertion accepted: 3.**

### N2 — Occurrences of the three-character literal declaration in C# source

- **Complete family:** every `.cs` line assigning a string literal consisting of exactly three `=`
  characters and nothing else.
- **Exhaustive search scope:** entire worktree, `*.cs` glob, unlimited head.
- **Inclusion rules:** the assignment's literal value is exactly `"==="`.
- **Exclusion rules:** any literal of four or more equals; any literal containing further text;
  markdown records under `docs/`.
- **Primary search strategy / query expression:** fixed-string anchored on the closing quote and
  semicolon — pattern `= "===";` (the closing `";` prevents cross-match against `"===="`).
- **Primary member set:** `QuickFiler/Controllers/EfcSelectionGuard.cs:15`
- **Primary count:** 1
- **Cross-check search strategy / query expression:** bounded-repetition regex that constrains the
  run length from both sides rather than by fixed text — pattern `"={3}";`. Against `"====";` the
  engine matches three equals then requires a `"` and finds `=`, so the four-character form cannot
  satisfy it; this is a different mechanism from fixed-string matching.
- **Cross-check member set:** `QuickFiler/Controllers/EfcSelectionGuard.cs:15`
- **Cross-check count:** 1
- **Member-set comparison:** identical single-element sets. **Assertion accepted: 1.**

### N3 — Occurrences of the four-character literal declaration in C# source

- **Complete family:** every `.cs` line assigning a string literal consisting of exactly four `=`
  characters and nothing else.
- **Exhaustive search scope:** entire worktree, `*.cs` glob, unlimited head.
- **Inclusion rules / exclusion rules:** as N2, with the run length four.
- **Primary search strategy / query expression:** fixed-string `= "====";`, glob `*.cs`.
- **Primary member set:**
  1. `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16`
  2. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`
- **Primary count:** 2
- **Cross-check search strategy / query expression:** bounded-repetition regex `"={4}";`, glob `*.cs`.
- **Cross-check member set:** `FolderSuggestionTree.cs:16`, `BreadcrumbRowBuilder.cs:19`
- **Cross-check count:** 2
- **Member-set comparison:** identical two-element sets. **Assertion accepted: 2.**

### N3-supplement — the unscoped file count has drifted since `issue.md` was written

`issue.md:85-89` states that four files contain the literal text `= "====";`. Re-measured **without**
the `*.cs` filter, the same fixed-string query now returns **five** files:

1. `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`
2. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`
3. `docs/features/active/efc-controller-surface-defects-464/research/2026-08-25T12-20-efc-controller-surface-defects.md:1134`
4. `docs/features/active/efc-controller-surface-defects-464/evidence/qa-gates/sibling-ownership.md:103`
5. `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/issue.md` (multiple lines)

The fifth is `issue.md` itself, which did not exist when its own baseline was taken. This does not
affect any acceptance criterion, because every AC is already scoped with `-- '*.cs'`. It is recorded
so a later auditor who re-runs the unscoped form is not surprised. **Recommendation: keep the
`*.cs` scoping in every AC verification command exactly as written; do not "correct" the four to
five in `issue.md`, because the count is scope-dependent and the scoped counts are the operative ones.**

---

## Q2 — Is the directional constraint correct?

**Yes. Confirmed by trace, and the failing assertion is identified exactly.**

### The trace

Source facts, read in full:

- `EfcSelectionGuard.IsValidFilingSelection` (`EfcSelectionGuard.cs:41-51`) returns
  `!value.StartsWith(BannerPrefix, Ordinal) && !ArchiveStemContract.IsFullOutlookPath(value)`.
- `EfcSelectionGuard.IsValidCreationSelection` (`:66-77`) returns
  `value.Length >= MinimumCreationLength && !value.StartsWith(BannerPrefix, Ordinal) && !IsFullOutlookPath(value)`,
  with `MinimumCreationLength = 3` (`:22`).
- `ArchiveStemContract.IsFullOutlookPath` (`ArchiveStemContract.cs:41-56`): for `"==="`, `value[0]`
  is `'='` (neither `'\\'` nor `'/'`), and `value.Length > 1 && value[1] == ':'` is false because
  `value[1]` is `'='`. **Returns `false`.** Hypothesis 4 confirmed.
- `EfcFormController.IsBannerRow` (`EfcFormController.cs:1143-1148`) uses
  `BreadcrumbRowBuilder.BannerPrefix` = `"===="`. `"===".StartsWith("====")` is `false`, so
  `IsBannerRow("===")` is `false`. Hypothesis 2 confirmed.
- `EfcFormController.IsSelectableFolder` (`:1151-1153`) is
  `!IsBannerRow(selectedFolder) && EfcSelectionGuard.IsValidCreationSelection(selectedFolder)`.
  Hypothesis 3 confirmed.
- `IsValidSelection` (`:1155`) is `IsSelectableFolder(SelectedFolder)`.
- The filing site is `ActionOkAsync` (`:738-753`), whose guard is
  `selectedFolder is null || IsBannerRow(selectedFolder) || !EfcSelectionGuard.IsValidFilingSelection(selectedFolder)`.

Evaluating `"==="` **today** (guard prefix `"==="`):

| Term | Value |
|---|---|
| `IsBannerRow("===")` | false |
| `IsValidCreationSelection("===")` | `3 >= 3` true, then `!"===".StartsWith("===")` → **false** ⇒ false |
| `IsSelectableFolder("===")` | `true && false` = **false** (rejected) |
| `IsValidFilingSelection("===")` | `!"===".StartsWith("===")` → **false** ⇒ false |
| filing-site guard | `false \|\| false \|\| !false` = **true** ⇒ rejected |

Evaluating `"==="` **after widening the guard constant to `"===="`**:

| Term | Value |
|---|---|
| `IsBannerRow("===")` | false (unchanged; it reads the producer constant) |
| `IsValidCreationSelection("===")` | `3 >= 3` true, `!"===".StartsWith("====")` true, `!IsFullOutlookPath("===")` true ⇒ **true** |
| `IsSelectableFolder("===")` | `true && true` = **true** (accepted) |
| `IsValidFilingSelection("===")` | `true && true` = **true** (accepted) |
| filing-site guard | `false \|\| false \|\| !true` = **false** ⇒ **passes to filing** |

**Hypothesis 5 confirmed, with an added detail the orchestrator did not state:** the creation
predicate's `MinimumCreationLength = 3` provides no backstop, because `"===".Length` is exactly 3
and the comparison is `>=`. The three-character prefix is the sole rejecting mechanism at both sites.

### The precise failure

The merged test is
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs:452-465`,
`IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically`. Its body:

```csharp
foreach (var row in new[] { "===", "====" })
{
    bool creationPath = EfcFormController.IsSelectableFolder(row);
    bool filingPath =
        !EfcFormController.IsBannerRow(row)
        && EfcSelectionGuard.IsValidFilingSelection(row);
    creationPath.Should().Be(filingPath, $"both sites must classify {row} alike");   // :462
    creationPath.Should().BeFalse($"{row} is rejected at both sites");               // :463
}
```

On the first loop iteration (`row = "==="`) after a widening edit:

- **Line 462 still passes.** `creationPath` is `true` and `filingPath` is `true`, so the
  agreement assertion is satisfied. This is the dangerous part: the assertion that *looks* like the
  consistency guard does not catch the relaxation.
- **Line 463 is the assertion that fails.** `creationPath.Should().BeFalse(...)`.
  - Expected: `false`
  - Actual: `true`
  - FluentAssertions message: `Expected creationPath to be false because === is rejected at both sites, but found True.`

The second iteration (`row = "===="`) would still pass at both lines, because `IsBannerRow("====")`
is `true` and short-circuits both paths to `false`.

**Hypotheses 6 and 7 confirmed.** Exactly one existing test method fails under the widening edit.

### Confirmation that nothing else catches the relaxation

`QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` contains three equals-run literals, all four
characters plus text: `:43` and `:245` (`"==== SUGGESTIONS ===="` on the filing and creation
predicates respectively) and `:183` (the same value inside the `Issue614_...` candidate matrix).
`"==== SUGGESTIONS ====".StartsWith("====")` remains true after widening, so all three still assert
correctly. **Hypothesis 8 confirmed:** every assertion in that file passes under the prohibited edit.

`IsBannerRow_ClassifiesByTheFourCharacterPrefix` (`EfcFormControllerTests.cs:420-435`) also still
passes, because it exercises only `IsBannerRow` and `BreadcrumbRowBuilder.BannerPrefix`, neither of
which the widening edit touches. No `UtilitiesCS.Test` test is affected: `EfcSelectionGuard` is
`internal` to `QuickFiler` and has no consumer in that assembly.

---

## Q3 — Feasibility of the `FolderSuggestionTree` dedupe

All five sub-questions answer in favour of the change. It is a one-line-value edit plus, optionally,
one `using` directive.

### Q3.1 Same assembly?

**Yes.** Both files are compiled into `UtilitiesCS.dll`:

- `UtilitiesCS/UtilitiesCS.csproj:625` — `<Compile Include="OutlookObjects\Folder\BreadcrumbRowBuilder.cs" />`
- `UtilitiesCS/UtilitiesCS.csproj:815` — `<Compile Include="OutlookObjects\Folder\FolderSuggestionTree.cs" />`
- `UtilitiesCS/UtilitiesCS.csproj:15` — `<AssemblyName>UtilitiesCS</AssemblyName>`

No project reference, no `<Compile Include>` addition, and no `.csproj` edit of any kind is required.
This matters: the project uses the legacy non-SDK csproj format with explicit `Compile Include`
items, so a *new file* would require a csproj edit — but the intended remedy adds no file.

### Q3.2 Namespace reachability

`FolderSuggestionTree` is declared in `namespace UtilitiesCS` (`FolderSuggestionTree.cs:5`);
`BreadcrumbRowBuilder` is in `namespace UtilitiesCS.OutlookObjects.Folder`
(`BreadcrumbRowBuilder.cs:5`). `FolderSuggestionTree.cs`'s only using directives are
`using System;` (`:2`) and `using System.Collections.Generic;` (`:3`).

Two compiling forms are available:

1. Relative qualification with no new using, because the enclosing namespace is `UtilitiesCS`:
   `OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix`.
2. Add `using UtilitiesCS.OutlookObjects.Folder;` and reference `BreadcrumbRowBuilder.BannerPrefix`.

Form 2 matches how `EfcSelectionGuard.cs:2` handles the same dependency (`using UtilitiesCS.OutlookObjects.Folder;`)
and is the more readable option; form 1 avoids touching the using block. Either is acceptable. Note
that `FolderSuggestionNode` is itself in `namespace UtilitiesCS` (`FolderSuggestionNode.cs:4`), which
is why `FolderSuggestionTree.cs` compiles today without any `OutlookObjects.Folder` using.

### Q3.3 Accessibility

**Accessible.** `BreadcrumbRowBuilder.BannerPrefix` is `public const string`
(`BreadcrumbRowBuilder.cs:19`) on a `public sealed class` (`:13`).

### Q3.4 Does `const string X = OtherType.PublicConstString;` compile here?

**Yes.** A `const` field initializer must be a compile-time constant expression; a reference to
another accessible `const string` is exactly that, and the compiler folds it at the reference site.
This is a C# 1.0 language feature with no dependency on language version or target framework, and
there is no circular-constant hazard (`BreadcrumbRowBuilder` does not reference `FolderSuggestionTree`).

Toolchain context confirming there is no exotic constraint in play:
`UtilitiesCS.csproj:10` — `<LangVersion>12.0</LangVersion>`; `:16` — `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`.
Neither bears on constant folding. (For contrast, the known net48 restrictions recorded elsewhere in
this repository concern `init` accessors and `record`/`record struct`, none of which is involved.)

### Q3.5 Other readers of `FolderSuggestionTree`'s private constant

**None beyond `IsBanner` at `:197`.** The identifier census in Q1.2 shows exactly two lines in that
file: the declaration at `:16` and the single use at `:197`:

```csharp
private static bool IsBanner(string row)
{
    return row != null && row.StartsWith(BannerPrefix, StringComparison.Ordinal);
}
```

`IsBanner` is called from one place, `BuildFromRows` at `FolderSuggestionTree.cs:69`. Every other
`FolderSuggestionNodeKind.Banner` check in that file (`:117`, `:134`, `:151`, `:168`) reads the
already-assigned node kind, not the prefix.

**Consequence:** the dedupe is behaviour-preserving by construction — the constant's *value* is
unchanged (`"===="` before and after), only its *declaration site* moves. No test in
`UtilitiesCS.Test` can observe the change. Existing coverage of that path:
`FolderSuggestionTreeHierarchyTests.cs:93`, `:100`;
`FolderSuggestionTreeStateTests.cs:212`; `FolderProbabilityAdapterTests.cs:55`.

### Q3.6 Recommended form

Replace the literal, keep the named constant:

```csharp
// Shares the producers' single declaration so the two banner producers cannot drift (#662).
private const string BannerPrefix = BreadcrumbRowBuilder.BannerPrefix;
```

Rationale over the alternative of deleting the constant and inlining
`BreadcrumbRowBuilder.BannerPrefix` at `:197`: it keeps the call site at `:197` and the XML-doc at
`:194` unchanged, so the diff is one line; and it preserves the local name for future readers.
Both forms satisfy AC5's verification query (`= "====";` scoped to `*.cs` returning exactly one line
in `BreadcrumbRowBuilder.cs`), because neither leaves a four-equals literal in the file.

One caution for AC2's verification: AC2 asserts
`git grep -nE 'const +string +[A-Za-z_]*BannerPrefix' -- '*.cs'` returns **exactly one** line. Under
the recommended form above, `FolderSuggestionTree.cs:16` **still matches that regex** (it is still a
`const string` named `BannerPrefix`), so the count would be **two**, not one. This is a real conflict
between AC2 as written and the recommended dedupe form. Two resolutions:

- **Preferred:** delete `FolderSuggestionTree`'s constant entirely and reference
  `BreadcrumbRowBuilder.BannerPrefix` directly at `:197`. AC2 and AC5 then both hold as written, and
  the doc comment at `:194` needs no change. Cost: the `:197` line becomes longer and, if form 1 of
  Q3.2 is used, fully qualified.
- **Alternative:** keep the aliasing constant and amend AC2's expected count from one to two, with
  the second line explicitly identified as an alias carrying no independent literal.

**Recommendation: take the preferred resolution.** It satisfies both criteria without amending an
approved acceptance criterion, and it also removes the only remaining independent banner-prefix
*name* in the producer assembly, which is the stated goal ("the producers share one declaration").

---

## Q4 — Test reachability

### Q4.1 `InternalsVisibleTo`

`QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]`.

This is the mechanism by which `QuickFiler.Test` reaches `internal static class EfcSelectionGuard`
(`EfcSelectionGuard.cs:12`) and `EfcFormController.IsBannerRow` / `IsSelectableFolder`
(`internal static`, `:1143`, `:1151`). No reflection is needed and none is used for these members.

### Q4.2 Does `EfcSelectionGuardTests` already exercise both predicates directly?

**Yes, both, by direct static call.** The class is
`QuickFiler.Test.Controllers.EfcSelectionGuardTests` (`EfcSelectionGuardTests.cs:6`, `:14`), and it
is registered in the legacy csproj at `QuickFiler.Test/QuickFiler.Test.csproj:63`
(`<Compile Include="Controllers\EfcSelectionGuardTests.cs" />`), so **adding a test method to that
existing file requires no csproj edit**. Its using block is `FluentAssertions`,
`Microsoft.VisualStudio.TestTools.UnitTesting`, `QuickFiler.Controllers`,
`UtilitiesCS.EmailIntelligence.EmailParsingSorting` (`:1-4`) — sufficient for a new test that calls
only the two predicates.

`EfcFormControllerTests.cs` is likewise registered at `QuickFiler.Test/QuickFiler.Test.csproj:119`;
AC7 requires it be left unmodified.

### Q4.3 Existing test method names in `EfcSelectionGuardTests.cs` (collision list)

Region "Filing predicate" (`:16`):

1. `IsValidFilingSelection_NullSelection_IsRejected` (`:19`)
2. `IsValidFilingSelection_EmptySelection_IsRejected` (`:26`)
3. `IsValidFilingSelection_WhitespaceSelection_IsRejected` (`:33`)
4. `IsValidFilingSelection_BannerSentinel_IsRejected` (`:40`)
5. `IsValidFilingSelection_StoreRootedSelection_IsRejected` (`:47`)
6. `IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected` (`:57`)
7. `IsValidFilingSelection_DriveRootedSelection_IsRejected` (`:68`)
8. `IsValidFilingSelection_ValidRelativeStem_IsAccepted` (`:78`)
9. `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` (`:85`)
10. `IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted` (`:100`)
11. `IsValidFilingSelection_RootedTargetAboveArchiveRoot_IsRejected` (`:111`)
12. `IsValidFilingSelection_CrossStoreRootedTarget_IsRejected` (`:122`)
13. `IsValidFilingSelection_SeparatorBoundaryNearMiss_IsRejected` (`:133`)
14. `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsRejected` (`:144`)
15. `IsValidFilingSelection_ArchiveRootExactTarget_IsRejected` (`:157`)
16. `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` (`:168`)

Region "Folder-creation predicate" (`:218`):

17. `IsValidCreationSelection_NullSelection_IsRejected` (`:221`)
18. `IsValidCreationSelection_EmptySelection_IsRejected` (`:228`)
19. `IsValidCreationSelection_WhitespaceSelection_IsRejected` (`:235`)
20. `IsValidCreationSelection_BannerSentinel_IsRejected` (`:242`)
21. `IsValidCreationSelection_TwoCharacterSelection_IsRejected` (`:249`)
22. `IsValidCreationSelection_SingleCharacterSelection_IsRejected` (`:260`)
23. `IsValidCreationSelection_MinimumLengthSelection_IsAccepted` (`:267`)
24. `IsValidCreationSelection_RootedSelection_IsRejected` (`:277`)
25. `IsValidCreationSelection_ValidRelativeStem_IsAccepted` (`:288`)

The name AC6 specifies, `BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates`,
**does not collide** with any of the 25. Repo-wide, the identifier `BannerRejectionPrefix` currently
appears only inside this feature's own `issue.md` (7 lines) and in no `.cs` file at all.

### Q4.4 Note on the `Issue614_...` candidate matrix

`EfcSelectionGuardTests.cs:172-184` enumerates ten candidates, of which the only banner is
`"==== SUGGESTIONS ===="` (`:183`). If a `"==="`-prefixed candidate were ever added there it would,
under today's guard, simply be skipped by the `continue` at `:191`, so that test is not a suitable
place to pin the arity relationship. AC6's dedicated new test is the right vehicle.

---

## Q5 — Scoped test execution (documentation gathering only; nothing was built or run)

### Q5.1 Built test-assembly output paths under `Debug` / `Any CPU`

| Project | `AssemblyName` | Debug\|AnyCPU `OutputPath` | Resulting assembly path |
|---|---|---|---|
| `QuickFiler.Test` | `QuickFiler.Test` (`QuickFiler.Test.csproj:17`) | `bin\Debug\` (`:36`, under `Condition=" '$(Configuration)\|$(Platform)' == 'Debug\|AnyCPU' "` at `:32`) | `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` |
| `UtilitiesCS.Test` | `UtilitiesCS.Test` (`UtilitiesCS.Test.csproj:16`) | `bin\Debug\` (`:51`) | `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` |

Both target `v4.8.1` (`QuickFiler.Test.csproj:18`, `UtilitiesCS.Test.csproj:17`). Each project also
declares `Debug|x86` (`bin\x86\Debug\`) and `Release` variants; the CLAUDE.md toolchain uses
`"/p:Platform=Any CPU"`, which resolves to the `AnyCPU` property group, so `bin\Debug\` is correct.

**Neither assembly is present in this worktree at the time of this research** — a glob for
`{QuickFiler.Test,UtilitiesCS.Test}/bin/Debug/*.Test.dll` returned no files. This is a fresh
worktree; Phase 0 must build before any test run.

### Q5.2 The invocation shape this repository actually uses

The most recent and most directly comparable precedent is the #464 evidence (same EFC surface, same
worktree pattern), for example
`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/464c-fail.md:5`:

```
& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings `
  /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~<TestName>" `
  "/Logger:trx;LogFileName=<name>.trx" `
  /ResultsDirectory:<feature>/evidence/regression-testing/<task>
```

run under `pwsh -NoProfile` from the worktree root. `vstest.console.exe` is resolved via `vswhere`
(`docs/features/active/efc-controller-surface-defects-464/upstream-constraints-briefing.2026-08-27T23-12.md:156`);
it is not on `PATH` in this environment
(`docs/features/active/efc-controller-surface-defects-464/plan.2026-08-25T07-01.md:244`).

`scripts/vscode/TaskMaster.cli.runsettings` exists and contains only an MSTest parallelization block
(`<Workers>0</Workers>`, `<Scope>ClassLevel</Scope>`).

### Q5.3 `/Tests:` versus `/TestCaseFilter:` — both are in use, and they are not interchangeable

Both forms appear in committed evidence:

- `/Tests:` — comma-separated **method-name substring** list. Used in the older archived features,
  for example `docs/features/archive/2026-03-14-dark-mode-detection-71/evidence/regression-testing/qfcformcontroller.expect-fail.md:2`
  and `docs/features/archive/2026-03-20-triage-null-classifier-group-88/policy-audit.2026-03-20T10-04.md:65`.
- `/TestCaseFilter:` — expression form over `FullyQualifiedName`, `TestCategory`, etc. Used
  throughout the recent #464 and #511 evidence.

`issue.md` AC6 and AC7 name the `/Tests:` form. That form is valid and has repository precedent, so
it can be used as written. **Note for the executor:** `/Tests:` and `/TestCaseFilter:` cannot be
combined in one invocation — vstest rejects the pair. If the `TestCategory!=LiveOutlook` filter
(Q5.4) is wanted in the same run, the `/TestCaseFilter:` form must be used for both conditions, for
example `/TestCaseFilter:"FullyQualifiedName~BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates"`.
For the two AC6/AC7 single-test runs this is moot (see Q5.4).

### Q5.4 The live-Outlook exclusion filter

The repository convention, established by the QuickFiler suite-determinism epic, is:

```
vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"
```

Cited at `docs/features/epics/quickfiler-suite-determinism-foundation/epic.md:170` and `:219`,
`docs/features/active/winformspumphost-suite-determinism-511/spec.md:583`, and
`.../policy-audit.2026-08-24T00-01.md:212`.

Two operationally important riders recorded with it:

1. **`/InIsolation` is mandatory.** Omitting it causes each assembly's `app.config` binding
   redirects to be ignored, producing roughly 1,695 phantom failures with empty messages and
   sub-millisecond durations, surfacing as a Moq `TypeInitializationException`
   (`docs/features/epics/quickfiler-suite-determinism-foundation/epic-kickoff.md:32`,
   `epic-status.md:225`). That is a load failure, not a regression.
2. **Exclude `\.claude\` from recursive `*.Test.dll` discovery**, or the worktree copies of the
   assemblies are picked up (same citations).

**Scope note specific to #662:** `[TestCategory("LiveOutlook")]` occurs three times in two files,
both in `TaskMaster.Test` (`TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs` ×2,
`TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunnerTests.cs` ×1). **Neither `QuickFiler.Test` nor
`UtilitiesCS.Test` contains any `LiveOutlook`-categorised test**, so for AC8's two full-assembly runs
the filter is a no-op. Applying it anyway costs nothing and keeps the command identical to the
repository convention; that is the recommended course.

---

## Q6 — Naming and analyzer risk of `BannerRejectionPrefix`

**No analyzer risk, and no identifier collision.**

### Q6.1 Configuration surface

There is **no `.globalconfig`** anywhere in the repository (glob `**/.globalconfig` returned no
files). The only analyzer configuration is `.editorconfig` at the repository root.

`.editorconfig:27` sets the catch-all `dotnet_analyzer_diagnostic.severity = suggestion`. A search of
`.editorconfig` for `severity = error` returned **zero** matches, and `IDE1006` carries no explicit
per-rule severity entry.

### Q6.2 Naming rules that could match a private const field

Two rule families would nominally apply to a private field, both at `severity = suggestion`:

- `.editorconfig:566-575` — `cs_private_field_camelcase`: `applicable_kinds = field`,
  `applicable_accessibilities = internal, private, private_protected`, style `_camelCase`.
- `.editorconfig:595-610` — `private_or_internal_field_should_be__camelcase`: same shape,
  `applicable_accessibilities = friend, private, private_protected`.

In editorconfig naming terms a `const` field is a `field`, so these rules technically apply. **The
existing code already sits in this state**: `EfcSelectionGuard.BannerPrefix` and
`MinimumCreationLength`, `BreadcrumbRowBuilder.TrashRowText`, `FolderSuggestionTree.BannerPrefix`
and `PathSeparator` are all PascalCase private/public consts today, and the repository builds clean
under `/p:EnforceCodeStyleInBuild=true /p:TreatWarningsAsErrors=true`. Renaming
`BannerPrefix` to `BannerRejectionPrefix` preserves the shape exactly — same kind, same
accessibility, same casing convention — so it introduces **no new diagnostic of any severity**, and
`suggestion`-severity diagnostics are not promoted to errors by `TreatWarningsAsErrors`.

No rule in `.editorconfig` constrains identifier length, required prefixes for non-interface types,
or the token `Rejection`.

### Q6.3 Identifier collision

`BannerRejectionPrefix` appears in **zero** `.cs` files. Its only occurrences repository-wide are
seven lines inside this feature's own
`docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/issue.md`
(`:64`, `:103`, `:122`, `:127`, `:129`, `:144`, `:150`).

---

## Q7 — Prior art in `docs/features/active/efc-controller-surface-defects-464/` (read only)

### Q7.1 What the closed-feature records already concluded

Four artifacts in that folder discuss this exact residual. Their conclusions are consistent with
each other and with findings 1-8.

**`evidence/qa-gates/465-source-structure.md:152-160` — "Recorded residual — reported, not fixed here".**
States that `EfcSelectionGuard.BannerPrefix` is `"==="` while both producers use `"===="`; calls it
"a third arity variant"; notes the comment still describes a `"===="` rejection the guard does not
implement; and records explicitly that `spec.md` criterion 979 asserts only that **`IsBannerRow`'s**
prefix agrees with `BreadcrumbRowBuilder.BannerPrefix`, making no claim that every
banner-classification site shares one arity. `:142-150` records a zero-line diff for both
`BreadcrumbRowBuilder.cs` and `EfcSelectionGuard.cs`.

**`feature-audit.2026-08-28T02-29.md:46-53` — "Judgment: the RC7 non-edit decision was correct".**
The independent reviewer re-verified the underlying facts and endorsed the non-edit for three
reasons, of which the first two are the substantive ones:

- `:51` — "Widening `BannerPrefix` to `"===="` would relax a merged filing guard — a three-`=` row
  would begin passing `IsValidFilingSelection` — in a file this feature does not own, while
  `EfcSelectionGuardTests.cs` asserts only on a four-`=` banner, so the relaxation would pass every
  existing test silently. That is precisely the widen-the-strict-guard failure mode recorded against
  #614's own remediation history."
- `:52` — the delivered composition is "strictly narrowing"; both sites agree on both arities and
  no previously rejected input becomes accepted.
- `:53` — the residual "is a latent inconsistency with no user-observable effect today (no producer
  emits a three-`=` row), which is the correct severity for a promoted follow-up rather than an
  in-scope fix."

**`evidence/other/followup-promotions.md:65-72`** repeats the direction independently: widening
"would *relax* a filing guard #614 deliberately tightened — a three-`=` row would become filable —
in a file this feature does not own, on a merged sibling's behaviour, to gain nothing a user can
observe". It splits the work into two homes: **item 3** (`:50`) is the
`FolderSuggestionTree`/`BreadcrumbRowBuilder` consolidation, and **item 7** (`:86`, `:93-94`) is the
guard residual plus the stale comment. Issue #662 unifies both, which is consistent with — not
contrary to — that split.

**`research/2026-08-25T12-20-efc-controller-surface-defects.md:1132-1135`** ("A fourth, unowned copy
of the banner constant") identified `FolderSuggestionTree.cs:16` as outside #464's owned set and
directed "Record as a downstream note; do not consolidate it." `:1128-1131` recorded that
`BreadcrumbRowBuilder.cs` is read-only to #464 because #498's acceptance criteria assert it is not
modified. `:1168` classifies RC7 as CONFIRMED.

### Q7.2 Does any prior conclusion contradict findings 1-8?

**No.** Every substantive conclusion corroborates them. Three discrepancies exist, all of them
citation or terminology drift rather than a conflicting judgment:

1. **Comment line citation drift.** `465-source-structure.md:156` and
   `followup-promotions.md:94` place the stale comment at `EfcFormController.cs:325`. The
   later `feature-audit.2026-08-28T02-29.md:48` corrects this to `:318-320`, which is what the
   current tree holds (verified directly). Use `:318-320`.
2. **"Fourth"/"fifth" copy terminology.** The research artifact calls `FolderSuggestionTree.cs:16`
   "a fourth, unowned copy" in its heading (`:1132`) and "a fifth classification site" in its body
   (`:1134`), and `followup-promotions.md:50` calls it "the fifth banner-prefix constant". These
   count *classification sites* (which today number five: `BreadcrumbRowBuilder.Classify`,
   `BreadcrumbStateRow.IsBanner`, `EfcFormController.IsBannerRow`, `FolderSuggestionTree.IsBanner`,
   and the two `EfcSelectionGuard` predicates), **not** constant declarations, of which there are
   exactly three (see N1). The two figures are not in conflict; they measure different families.
   `issue.md`'s "three declarations" framing is the correct one for this feature's ACs.
3. **A stale claim in `followup-promotions.md:40-42`.** It records that a `gh issue list` duplicate
   search returned empty for all seven follow-ups as of 2026-08-28. Issue #662 now exists and is
   this feature. That artifact is a point-in-time record and was not re-verified here; it is noted
   only so a reader does not treat it as current.

Nothing in the closed folder was modified.

---

## Corrections to the issue document

Both are recorded for accuracy; neither blocks the plan.

1. **`issue.md:85-89` (unscoped file count).** The claim "Four files contain the literal text
   `= "====";`" is now **five**, because `issue.md` itself contains the literal. See
   N3-supplement. The `*.cs`-scoped counts on which every AC depends are unaffected and were
   independently re-derived as 1 and 2 (N2, N3).
2. **`issue.md:125-128` (AC2's expected count) versus `issue.md:139-142` (AC5's remedy).** AC2
   asserts `const +string +[A-Za-z_]*BannerPrefix` scoped to `*.cs` returns exactly one line. If AC5
   is implemented by *aliasing* (`private const string BannerPrefix = BreadcrumbRowBuilder.BannerPrefix;`),
   AC2's query returns **two** lines and AC2 fails while AC5 passes. Resolve by implementing AC5 as a
   **deletion** of `FolderSuggestionTree`'s constant with a direct reference at `:197`. See Q3.6.

---

## Behaviour semantics of the intended change

| Aspect | Before | After | Observable difference |
|---|---|---|---|
| `EfcSelectionGuard` rejection prefix value | `"==="` | `"==="` (unchanged, renamed constant) | none |
| `FolderSuggestionTree.IsBanner` prefix value | `"===="` (own literal) | `"===="` (producer's constant) | none |
| `EfcFormController.IsBannerRow` | `"===="` via producer constant | unchanged | none |
| `SelectedFolder` comment | claims a `"===="` second rejection | describes the actual four-plus-three composition | none (comment) |
| Declaration count named `BannerPrefix` (`*.cs`) | 3 | 1 | metric only |
| Four-equals literals (`*.cs`) | 2 | 1 | metric only |

**The change is behaviour-preserving at every call site.** No predicate's return value changes for
any input. This is the property that makes the merged test at `EfcFormControllerTests.cs:453` an
adequate regression gate: it must continue to pass unmodified (AC7), and the new AC6 test adds an
explicit, named pin so that a future widening fails with a message stating the prohibited direction
rather than failing on an unrelated-looking assertion.

### Edge cases the new AC6 test should cover

- `"==="` on `IsValidFilingSelection` → false (the sole rejecting mechanism; would flip to true on a widening edit)
- `"==="` on `IsValidCreationSelection` → false (note that the length rule does **not** reject it: `3 >= 3`)
- `"===="` on both → false (still rejected after any widening; included so the test states the full relationship)
- The `because` message must name the widening as prohibited, per AC6.

Optionally, and stronger: assert the structural relationship rather than only the two values, for
example that `BreadcrumbRowBuilder.BannerPrefix.StartsWith(<guard prefix>)` holds while the two are
not equal. That would fail on a widening edit even if someone also changed the sampled row values.
This is a suggestion for the planner, not a requirement of AC6 as approved; AC6 as written is
satisfied by the four value assertions above.

---

## Testing implications (strategy only; no test code written)

1. **Framework and libraries.** MSTest attributes, FluentAssertions for every assertion, no Moq
   (both predicates are pure static methods with no collaborators — `EfcSelectionGuardTests.cs:8-11`
   already records that rationale for the existing file).
2. **Location.** Add the single new method to the existing
   `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`, inside a new region or at the end of the
   creation-predicate region. No new file, therefore no `QuickFiler.Test.csproj` edit
   (`QuickFiler.Test.csproj:63` already includes the file).
3. **Determinism.** No clock, no RNG, no async, no sleeps, no temp files, no external dependency.
   Pure string inputs, pure boolean outputs. Independent and order-insensitive.
4. **Arrange-Act-Assert.** With pure predicates, "Act / Assert" combined (the existing file's
   convention throughout) is appropriate and consistent.
5. **Coverage.** No new production member is added, so the >= 90% new-code target has no new
   denominator. The changed lines (`EfcSelectionGuard.cs:15`, `:49`, `:75`;
   `FolderSuggestionTree.cs:16` or `:197`; `EfcFormController.cs:318-320`) are all either comments
   or already covered by the existing suites listed in Q3.5 and Q4.3, so no coverage regression on
   changed lines is expected.
6. **Regression protection for AC7.** `EfcFormControllerTests.cs` must show a zero-line diff. That
   is verifiable by `git diff origin/main --stat -- QuickFiler.Test/Controllers/EfcFormControllerTests.cs`
   as AC7 states.
7. **Full-assembly runs (AC8).** Both `QuickFiler.Test` and `UtilitiesCS.Test` must run in full,
   because the `FolderSuggestionTree` edit lives in `UtilitiesCS` and the guard edit lives in
   `QuickFiler`. Apply `/InIsolation` and exclude `\.claude\` from discovery per Q5.4.

---

## Files that will be touched by the intended remedy

| File | Change | Constraint |
|---|---|---|
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | rename constant at `:15`, update `:49` and `:75`, add XML doc (AC2, AC3) | value must stay `"==="` (AC1) |
| `QuickFiler/Controllers/EfcFormController.cs` | replace comment at `:318-320` (AC4) | comment only; no code change; must not disturb `:834-837` per the #476 cross-feature note in `465`'s prior art |
| `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` | remove the duplicate literal at `:16`, reference the producer constant (AC5) | see Q3.6 on the AC2/AC5 interaction |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | add one `[TestMethod]` (AC6) | file already in csproj; no new file |

`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` must **not** be modified. Prior art
(`research/2026-08-25T12-20-efc-controller-surface-defects.md:1128-1131`) records that feature #498's
acceptance criteria assert that file is unmodified. It is read-only for this work; the remedy only
adds a reader.

`QuickFiler.Test/Controllers/EfcFormControllerTests.cs` must **not** be modified (AC7).
