## Research: FolderConverter/FolderPredictor dead code and bugs (Issue #732)

- Date: 2026-09-02T13-15
- Feature folder: `docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732`
- All file/line citations below were read directly in this session against the current worktree
  state (branch `bug/folderconverter-folderpredictor-dead-code-and-bugs-732`, checked out from
  `origin/main`). Every citation is independently verified unless explicitly marked otherwise.

---

## 1. Current State Analysis

### 1.1 Two distinct `FolderConverter` classes in the same namespace

Two files each declare `public static class FolderConverter` inside `namespace UtilitiesCS`,
neither marked `partial`:

- **Dead file**: `UtilitiesCS/EmailIntelligence/FolderConverter.cs` (63 lines). Verified via
  `Grep` that `UtilitiesCS/UtilitiesCS.csproj` contains **zero** `<Compile Include>` entries for
  any path under `EmailIntelligence/FolderConverter.cs`. Declaration at line 12:
  `public static class FolderConverter` — no `partial` keyword present.
  - Line 30: `if (olBranchURI.Scheme != olBranchURI.Scheme)` — always `false`, self-comparison bug.
  - Line 40: `if (relativePath[0].Equals("."))` — `relativePath[0]` is `char`; `char.Equals(string)`
    resolves to `object.Equals(object)` overload, which is legal C# (always returns `false` because
    a boxed `char` never equals a `string`), **not a compile error** as the issue text implies. This
    corrects one sub-claim in issue #732's finding 1 text ("a type mismatch that would not compile
    as written"): it compiles, but the intended `.` prefix check silently never fires, which is a
    distinct, arguably worse, logic bug (dead guard) rather than a build error.
  - Extension methods exposed: `ToFsFolder(this string, string, string)` and
    `ToFsFolder(this Folder, string, string)`.

- **Live file**: `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` (358 lines). Verified
  compiled via `UtilitiesCS/UtilitiesCS.csproj:1054`:
  `<Compile Include="OutlookObjects\Folder\FolderConverter.cs" />`. Declaration at line 16:
  `public static class FolderConverter` — no `partial` keyword present.
  - Exposes `ToFsFolderpath` overloads for `string`, `Folder`, `MAPIFolder`, and two
    `IApplicationGlobals`-based overloads (lines 227-327), `SanitizeFilename` (line 219),
    `ResolveOlRoot` (line 329), plus the folder-name legality/prompt machinery
    (`IsLegalFolderName`, `AskUserForAlternatives`, `BuildAlternativesDictionary`, etc., lines
    18-217). Built on `ArchiveStemContract.TryMakeArchiveRelative` (verified in
    `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs:106-145`), a pure,
    separator-boundary-respecting, prefix-anchored, ordinal-case-insensitive stem extractor —
    materially more robust than the dead file's `Uri.MakeRelativeUri` approach, which has no
    segment-boundary awareness and no invalid-Windows-folder-name validation.

**Compile-collision determination (verified, not assumed):** Since both classes are `public
static class FolderConverter` in the identical namespace `UtilitiesCS`, and **neither is declared
`partial`**, adding a `<Compile Include>` for `EmailIntelligence/FolderConverter.cs` to the same
assembly (`UtilitiesCS.csproj`) that already compiles `OutlookObjects/Folder/FolderConverter.cs`
produces a hard **CS0101** ("The namespace 'UtilitiesCS' already contains a definition for
'FolderConverter'") compile error, independent of whether member signatures overlap. This is a
correctness fact about C#'s type-declaration rules (a non-`partial` type name can only be declared
once per namespace per assembly), not merely a risk.

### 1.2 `FolderConverter_Tests.cs` target method resolution

`UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs` (verified, full file read, 28
lines):
- Namespace: `UtilitiesCS.Test.OutlookExtensions`.
- `using` directives: only `System` and
  `Microsoft.VisualStudio.TestTools.UnitTesting` — **no `using UtilitiesCS;`**.
- Test body calls `olBranchPath.ToFsFolderpath(olAncestorPath, fsAncestorEquivalent)` — method
  name `ToFsFolderpath`, which exists **only** on the live
  `OutlookObjects/Folder/FolderConverter.cs` class (line 227-231); the dead
  `EmailIntelligence/FolderConverter.cs` class has no `ToFsFolderpath` member at all (only
  `ToFsFolder`).
- Verified via `Grep` that `UtilitiesCS.Test/UtilitiesCS.Test.csproj` has **zero**
  `<Compile Include>` entries referencing `OutlookExtensions\FolderConverter_Tests.cs`.

**(a) Namespace/using resolution, verified:** C# extension-method lookup searches the current
namespace and its enclosing namespaces, plus namespaces named in `using` directives. The
namespace `UtilitiesCS.Test.OutlookExtensions` is **not** nested inside `UtilitiesCS` (it is
nested inside the sibling root `UtilitiesCS.Test`), so `UtilitiesCS` is not an enclosing namespace
of the test's namespace. Without an added `using UtilitiesCS;` directive, the call to
`.ToFsFolderpath(...)` would fail to resolve (CS1061: "string does not contain a definition for
'ToFsFolderpath' and no accessible extension method ... could be found"), even if the file were
added to the csproj as-is. Adding `using UtilitiesCS;` would resolve it, and since the dead file's
class only defines `ToFsFolder` (never `ToFsFolderpath`) — and, per §1.1, the dead file cannot ever
be compiled into the same assembly regardless — the call would bind unambiguously to the live
`OutlookObjects/Folder/FolderConverter.ToFsFolderpath(string, string, string)` overload.

**(b) Expected-output trace, verified:** Inputs — `olBranchPath =
"first.last@company.com\Ol Level 1\Common Level A\Common Level B"`, `olAncestorPath =
"first.last@company.com\Ol Level 1"`, `fsAncestorEquivalent = "C:\Fs Level 1\Fs Level 2\Fs Level
3"`, expected `"C:\Fs Level 1\Fs Level 2\Fs Level 3\Common Level A\Common Level B"`.
Trace through the live implementation:
1. `ArchiveStemContract.TryMakeArchiveRelative(olBranchPath, olAncestorPath, out stem)`
   (`ArchiveStemContract.cs:106-145`): `root = olAncestorPath` (no trailing separator to trim);
   `fullPath.StartsWith(root, OrdinalIgnoreCase)` is true; `boundary = fullPath[root.Length]` is
   `\`, valid; `stem = "Common Level A\Common Level B"` (leading separator trimmed). Returns
   `true`.
2. `FolderConverter.cs:254-259`: `fsPathExDividers.Length != 0`, so
   `fsPath = fsAncestorEquivalent.TrimEnd(SegmentSeparators) + '\' + "Common Level A\Common Level
   B"` = `"C:\Fs Level 1\Fs Level 2\Fs Level 3\Common Level A\Common Level B"` — **matches
   expected exactly**.
3. `FindInvalidSegmentRule("Common Level A\Common Level B")`
   (`FolderConverter.cs:77-108`): splits into segments `["Common Level A", "Common Level B"]`.
   Each rule checked per segment: `IndexOfAny(IllegalFolderCharacters)` — none of `<>:"/\|?*` etc.
   present; `EndsWith(".")` — false (ends with `A`/`B`); `EndsWith(" ")` — false (ends with
   `A`/`B`, **not** a trailing space; the internal spaces inside "Common Level A" do not trigger
   this rule because the check is `segment.EndsWith(" ")`, a suffix test, not a
   contains-any-space test); `IsReservedDeviceName` — false. Returns `null` (no violation).

Conclusion: the test's arrange/act/assert values **do** produce the expected output when run
against the live implementation. This matches the corresponding assertion already confirmed
passing in the compiled sibling test below.

**(c) Redundancy against already-compiled tests, verified:**
`UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` (compiled — verified via
`Grep` at `UtilitiesCS.Test.csproj:351`) contains, at lines 18-35, a test
`ToFsFolderpath_WithStringInputs_MapsOutlookBranchIntoFilesystemBranch` using **the identical
three input strings and identical expected output string**, calling
`FolderConverter.ToFsFolderpath(olBranchPath, olAncestorPath, fsAncestorEquivalent)` directly
(static-call style rather than extension-method style, but the same method). This is a
byte-for-byte duplicate scenario, already compiled and (per the file being an accepted part of the
suite) already passing. `FolderConverterIssue614Tests.cs` was not found to contain this specific
parent/branch/ancestor scenario on inspection of `FolderConverterTests.cs`'s coverage above;
`FolderConverterTests.cs` alone already fully subsumes `FolderConverter_Tests.cs`'s single test
case. Wiring `FolderConverter_Tests.cs` into the csproj (even after adding `using UtilitiesCS;`)
would add **zero net-new coverage** — it duplicates an already-compiled, already-passing assertion
verbatim.

### 1.3 Dead-file reachability search (production and test code)

Ran a repo-wide `Grep` for `ToFsFolder\b` (word-boundary, to distinguish from `ToFsFolderpath`).
Two files matched:
- `UtilitiesCS/EmailIntelligence/FolderConverter.cs` — the dead file's own declaration (self-match,
  not a caller).
- `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs:94-97` — calls
  `folderCurrent.ToFsFolder(OlFolderRoot: _globals.Ol.ArchiveRootPath, FsFolderRoot:
  _globals.FS.FldrRoot)`, which matches the dead file's parameter names exactly (named-argument
  syntax `OlFolderBranch, OlFolderRoot, FsFolderRoot`).

Verified via `Grep` on `ToDoModel/ToDoModel.csproj` (a legacy, non-SDK-style project with explicit
`<Compile Include>` entries only, confirmed by reading the file header — no SDK-style implicit
globbing) that `SortItemsToExistingFolder` produces **zero matches**, i.e. this file, too, is
excluded from the `ToDoModel` build. This is a second, independent piece of dead code (not part of
issue #732's scope) that happens to reference the dead `FolderConverter.ToFsFolder` API, but since
neither file compiles, there is no live reference anywhere in the codebase to the dead file's
members.

The XML doc `<seealso cref="FolderConverter.ToFsFolderpath(string, string, string)">` at
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:667` was verified to reference the method
name `ToFsFolderpath` (three-arg string overload), which exists only on the live
`OutlookObjects/Folder/FolderConverter.cs:227-231`. This cref is not evidence of any reference to
the dead file.

**Conclusion for §1.3:** the dead `EmailIntelligence/FolderConverter.cs` file is genuinely
unreferenced by any compiled code anywhere in the repository, confirming the issue's "dead code"
characterization.

---

## 2. Finding 2 — `FolderPredictor.cs:691` bitwise-OR / unguarded index

Verified at `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:677-725`
(`public MAPIFolder? CreateFolder(string parentBranchPath, string olAncestor, string fsAncestor)`,
a public method):

```
691: if (olAncestor.EndsWith('\\'.ToString()) | parentBranchPath[0] == '\\')
```

- `olAncestor` is defaulted from `_globals.Ol.ArchiveRootPath` only when null/empty (line 684-687);
  `parentBranchPath` has **no** null/empty guard anywhere in the method before line 691.
- The `|` (bitwise-OR on two `bool` operands) forces evaluation of **both** operands regardless of
  the left operand's value — unlike `||`, which short-circuits. `parentBranchPath[0]` throws
  `IndexOutOfRangeException` when `parentBranchPath == ""`, and `NullReferenceException` when
  `parentBranchPath` is `null`. Because `CreateFolder` is a `public` instance method with no
  precondition validation on `parentBranchPath`, this is reachable from any caller that passes an
  empty or null branch path (e.g., creating a folder directly under `olAncestor` with no relative
  path segment).
- A correctly length-guarded sibling pattern exists at
  `FolderPredictor.cs:950-967` (`GetOlSubpath`), which only calls `olAncestor.EndsWith(...)` (no
  indexing into an unguarded string), and separately at line 954 uses `EndsWith` alone without
  needing to also inspect `parentBranchPath`. This confirms the issue's characterization that a
  correctly-guarded `EndsWith` pattern already exists elsewhere in the file, though it is not a
  literal drop-in replacement for line 691 since line 691 additionally needs a length guard on
  `parentBranchPath` specifically (a different variable than what `GetOlSubpath` guards).
- Recommended guard shape, consistent with existing repo conventions (e.g.
  `ArchiveStemContract.IsFullOutlookPath`, `ArchiveStemContract.cs:41-56`, which checks
  `value.Length > 1 && value[1] == ':'` before indexing):
  `(olAncestor.EndsWith("\\", StringComparison.Ordinal) || (parentBranchPath.Length > 0 &&
  parentBranchPath[0] == '\\'))`. `StringComparison.Ordinal` should be preferred over
  `'\\'.ToString()` allocation/culture-sensitive `EndsWith` per repo convention seen in
  `FolderConverter.cs:91,96` (`EndsWith(".", StringComparison.Ordinal)`).

**Existing test coverage of `CreateFolder`, verified:**
`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` contains at least four
`CreateFolder`-exercising tests (confirmed via `Grep`):
- Line 596-625: `CreateFolder_WhenParentBranchStartsWithSeparator_UsesCombinedPathWithoutDoubleSlash`
  — calls `predictor.CreateFolder("\Projects", "\\ArchiveRoot", "C:\OneDrive")`. `parentBranchPath`
  is non-empty here (starts with `\`), so this test does not currently exercise the empty-string
  crash path and would not need to change to accommodate a guard fix, but its assertion on the
  combined-path behavior (line 691's `if`/`else` branches) should be re-verified unaffected by the
  guard fix, since the guard only changes reachability for empty `parentBranchPath`, not the
  existing non-empty branches' logic.
- Line 791-825: `InjectedDirectory_CreateFolder_WhenPromptSuppliesName_CreatesFolderAndDirectoryPath`
  — `parentBranchPath = "Projects"` (non-empty, no leading `\`).
- Line 827-857: `CreateFolder_WhenAncestorIsNull_UsesArchiveRootAndCreatesFolder` —
  `parentBranchPath = "Projects"` (non-empty).
- No existing test in `FolderPredictorTests.cs` was found (via the above searches) that passes an
  **empty string** `parentBranchPath`, so a regression test for the crash (per the repo's
  Bugfix Workflow: failing test before fix, passing after) does not yet exist and would need to be
  added.

---

## 3. Finding 3 — `MatchBestSpecialFolder` substring matching

Verified at `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`:
- Line 77-83: public instance method `MatchBestSpecialFolder(string path)` delegates to the static
  helper, passing `SpecialFolders` unchanged.
- Line 85-96: XML doc comment on the `internal static` helper explicitly states: "Behavior is
  byte-for-byte identical to the original instance method body: ... matching uses ordinal
  `string.Contains`; ... Introduced as a pure seam to enable deterministic unit testing without
  changing runtime behavior."
- Line 97-111: `internal static string MatchBestSpecialFolder(IReadOnlyDictionary<string,string>
  specialFolders, string path)` — `specialFolders.Where(x => path.Contains(x.Value))
  .OrderByDescending(x => x.Value.Length).FirstOrDefault()`. Pure substring (`Contains`), not a
  segment/prefix-aware test.

**This finding traces to a pre-existing, independently authored root-cause artifact**, verified at
`docs/features/potential/promoted/2026-08-26-matchbestspecialfolder-substring-matching-codified-by-tests.md`
(dated 2026-08-26, already promoted to GitHub issue **#618**, cited in issue #732's spec.md as the
`(Source: #618.)` for finding 3). That document independently establishes, and this session's own
re-verification (below) confirms unchanged:

- **No production caller exists.** Verified via `Grep` across `TaskMaster/` and `UtilitiesCS/`:
  `MatchBestSpecialFolder` appears only in
  `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` (definition/delegation) and
  `UtilitiesCS/Interfaces/IGlobals/IFileSystemFolderPaths.cs` (interface declaration). No other
  production file references it.
- **Existing compiled tests pin substring semantics as correct.** Verified at
  `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs`: assertions
  explicitly document and test ordinal `Contains` behavior, including case-sensitivity (line 80-90:
  "ordinal Contains treats differing case as a non-match") and trailing-separator insensitivity
  (line 93-113: "the value is a substring of the path regardless of trailing separator"), and a
  null-path test at line 174-183 asserting `NullReferenceException` specifically because
  `path.Contains` dereferences a null path — i.e., the tests assume and would break under any
  semantic change from `Contains` to a segment-aware prefix test.

**Determination on issue #732's open decision ("decide the correct matching semantics ... and
update the XML doc comment to match whatever is implemented"):** Because (a) there is no
production caller today (confirmed independently in this session, matching #618's prior finding)
and (b) an entire dedicated test file already codifies substring semantics as the pinned contract,
changing the matching *logic* now is a larger, separately-scoped change (touching production logic
+ rewriting ~12 assertions in a dedicated test file) that #618 already tracks as its own issue with
its own proposed-fix checklist. Issue #732's own spec.md explicitly attributes finding 3 to
`(Source: #618.)` and frames the sub-task as "decide," not "must fix here." Recommendation below
treats #732's finding 3 scope as documentation-only (confirm/clarify), leaving the semantic change
to #618, to avoid #732 silently absorbing #618's separately-tracked, larger-blast-radius test
rewrite.

---

## 4. Numeric Derivation Evidence

No numeric count, enumeration, or population claim is proposed in this research (all four findings
are existence/reachability/behavior claims, not counts of members or files). This section is
intentionally omitted per the researcher's numeric-derivation-evidence gate, which applies only
when a numeric claim is proposed for an acceptance criterion.

---

## 5. Testing Implications

- Finding 1 (delete path, recommended — see below): no new tests required; removing an uncompiled,
  unreferenced file has no coverage impact since it was never in the coverage denominator (excluded
  from any `.csproj`, hence never measured).
- Finding 2: add one regression test to `FolderPredictorTests.cs` — `CreateFolder` invoked with an
  empty-string `parentBranchPath` and an `olAncestor` that does **not** end with `\`, asserting the
  method no longer throws `IndexOutOfRangeException`/`ArgumentOutOfRangeException` and instead
  falls through to the `else` branch's `$"{olAncestor}\\{parentBranchPath}"` concatenation (or
  whatever the fixed guard's chosen behavior is, per MSTest/Moq/FluentAssertions conventions
  already used throughout `FolderPredictorTests.cs`). Per the repo's Bugfix Workflow, this test
  must be written and observed failing before the guard fix, then passing after.
- Finding 3: if #732 is scoped to documentation-only, no test changes are needed beyond confirming
  `AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` still passes unmodified (it will, since
  no logic changes). If a future planner nonetheless chooses to fold #618's semantic fix into #732,
  the ~12 existing substring assertions in
  `AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` would all need rewriting to assert
  prefix/segment semantics instead — a materially larger change than #732's other three findings.
- Finding 4 (`FolderConverter_Tests.cs`): recommended disposition is deletion (see below), which
  requires no new test additions since `FolderConverterTests.cs` already covers the identical
  scenario.

---

## RECOMMENDATIONS

### Finding 1 — Dead `EmailIntelligence/FolderConverter.cs`

**Recommendation: Delete, do not resurrect.** Rationale:
1. A live, more mature, better-tested `FolderConverter` class already exists in the same
   namespace/assembly (`OutlookObjects/Folder/FolderConverter.cs`) with materially more capability
   (validated Windows-folder-name segment checks, reserved-device-name checks, `IApplicationGlobals`
   integration, `ResolveOlRoot`).
2. Resurrecting the dead file (adding a `<Compile Include>`) is a **guaranteed CS0101 hard compile
   error** because both classes are non-`partial` `public static class FolderConverter` in
   namespace `UtilitiesCS` (verified, §1.1) — resurrection is not merely risky, it is not buildable
   as literally written without first renaming one of the two classes, which would be a much larger
   and riskier change than the issue's stated intent.
3. No compiled or uncompiled-but-otherwise-live caller anywhere in the repository references the
   dead file's `ToFsFolder` members (verified, §1.3); the one apparent caller
   (`ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`) is itself excluded from its own
   project's build.
4. The two bugs cited in the issue (self-comparison at line 30; the dead `.` guard at line 40) are
   real but moot under a delete recommendation — they only matter if resurrection were chosen.
5. Deletion is the minimal, targeted fix consistent with "Simplicity first" and avoids introducing
   a second, weaker-featured, harder-to-maintain path-conversion implementation alongside the live
   one.

### Finding 2 — `FolderPredictor.cs:691`

**Recommendation: Minimal targeted fix, per repo Bugfix Workflow.**
1. Add a failing regression test to `FolderPredictorTests.cs`: `CreateFolder` with an empty-string
   `parentBranchPath` (and non-`\`-terminated `olAncestor`) currently throws
   `IndexOutOfRangeException`; assert the desired post-fix behavior (no throw; falls into the
   `else`-branch concatenation, or whatever behavior the planner selects for an empty branch path —
   note this choice affects the test's expected assertion and should be decided during planning,
   not left implicit).
2. Change line 691 from `|` to `||`, and add a length guard:
   `olAncestor.EndsWith("\\", StringComparison.Ordinal) || (parentBranchPath.Length > 0 &&
   parentBranchPath[0] == '\\')`. Prefer `StringComparison.Ordinal` over `'\\'.ToString()` to match
   the ordinal-comparison convention already used in `OutlookObjects/Folder/FolderConverter.cs:91,96`.
3. No change needed to `GetOlSubpath` (line 954) — it already guards correctly for its own
   variable (`olAncestor`) and does not index `parentBranchPath`, so it is not itself defective;
   it exists here only as an existing-pattern reference point, not a fix target.
4. Verify the three existing `CreateFolder` tests (lines 596, 791, 827 in
   `FolderPredictorTests.cs`) still pass unchanged, since all three use non-empty
   `parentBranchPath` values and the guard fix does not alter behavior for non-empty inputs.

### Finding 3 — `MatchBestSpecialFolder`

**Recommendation: Documentation/confirmation only within #732's scope; do not fold #618's semantic
fix into this change.**
1. Within #732, either (a) leave the implementation and its XML doc comment as-is, since the doc
   comment already accurately documents the substring behavior as intentional (verified,
   `AppFileSystemFolderPaths.cs:85-96`), and simply cross-reference #618 as the tracked follow-up
   for any semantic change; or (b) if the planner determines #732 must independently close finding
   3, treat it as a pointer/no-op: confirm no production caller exists (re-verified in this
   session, §3) and that the doc comment is already accurate, requiring no code or doc change.
2. Do **not** implement the segment-aware prefix rewrite as part of #732: it requires rewriting
   ~12 assertions in `AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` that currently pin
   substring semantics as correct (verified, §3), which is #618's separately-scoped, larger-blast
   -radius change and would conflate two issues' acceptance criteria.

### Finding 4 — `FolderConverter_Tests.cs`

**Recommendation: Delete, do not wire in.** Rationale (all verified, §1.2):
1. Its target method resolves (once `using UtilitiesCS;` were added) to the **live**
   `OutlookObjects/Folder/FolderConverter.ToFsFolderpath`, not the dead file — so "wiring it in to
   restore coverage for the resurrected type" (the issue's implied motivation) is moot once
   Finding 1 is resolved by deletion rather than resurrection.
2. Its single test case is a byte-for-byte duplicate of an already-compiled, already-passing test
   (`FolderConverterTests.cs:18-35`,
   `ToFsFolderpath_WithStringInputs_MapsOutlookBranchIntoFilesystemBranch`) — wiring it in would
   add a redundant `<Compile Include>` with zero net-new coverage.
3. As literally written (no `using UtilitiesCS;`), it would not even compile if merely added to the
   csproj (CS1061) — it cannot be wired in "as-is" regardless of the Finding 1 decision.
4. Deleting an uncompiled, redundant test file has no negative coverage impact (it was never
   measured) and removes stale/duplicate code from the tree, consistent with "Reusability — avoid
   copy-paste."

### Cross-cutting note

Findings 1 and 4 should be resolved together (both delete) in the same change, since #732's
spec.md root-cause analysis already links them ("Findings 1 and 4 share a root cause"). Findings 2
and 3 are independent logic fixes/confirmations in neighboring files, consolidated by module
proximity only, and can be planned as separate atomic-plan phases with no shared file-level
conflict (Finding 2 touches `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` +
`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`; Finding 3 touches at most
`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`'s doc comment, which this research recommends
leaving unchanged).
