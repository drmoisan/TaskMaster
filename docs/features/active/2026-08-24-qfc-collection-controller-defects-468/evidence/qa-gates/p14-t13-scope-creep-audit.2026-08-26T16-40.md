# [P14-T13] Scope-creep audit (AC-25)

Timestamp: 2026-08-26T16-40

Command:

```
# 1 — partial-class split
grep -c 'partial class' QuickFiler/Controllers/QfcCollectionController.cs
grep -n  'class QfcCollectionController' QuickFiler/Controllers/QfcCollectionController.cs
ls QuickFiler/Controllers/QfcCollectionController*.cs

# 2 — coverage exclusion attribute
grep -n 'ExcludeFromCodeCoverage' QuickFiler/Controllers/QfcCollectionController.cs

# 3 — package additions
git diff --name-only 61edc19b 48c9ad8f | grep -i 'packages.config'
git diff --name-only 61edc19b..HEAD    | grep -i 'packages.config'
git diff 61edc19b 48c9ad8f -- '*.csproj' | grep -E '^[+-]' | grep -ci 'PackageReference'
git diff 61edc19b..HEAD    -- '*.csproj' | grep -E '^[+-]' | grep -ci 'PackageReference'

# 4 — undo-stack parameter on the interface member
grep -n 'MoveEmailsAsync' QuickFiler/Interfaces/IQfcCollectionController.cs
git show 61edc19b:QuickFiler/Interfaces/IQfcCollectionController.cs | grep -n 'MoveEmailsAsync'
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

All four checks are satisfied.

| # | Check | Result |
|---|---|---|
| 1 | `QfcCollectionController.cs` is not split into partial classes | **satisfied** — 0 occurrences of `partial class`; 1 source file |
| 2 | `ExcludeFromCodeCoverage` is still present in that file | **satisfied** — present at `:21` |
| 3 | No `packages.config` and no csproj `PackageReference` gained an entry | **satisfied** — 0 `packages.config` paths changed; 0 `PackageReference` lines added or removed |
| 4 | The undo-stack parameter is still present on the interface member | **satisfied** — `Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems);`, byte-identical to the base commit |

---

## Check 1 — no partial-class split

```
$ grep -c 'partial class' QuickFiler/Controllers/QfcCollectionController.cs
0

$ grep -n 'class QfcCollectionController' QuickFiler/Controllers/QfcCollectionController.cs
22:    public class QfcCollectionController : IQfcCollectionController

$ ls QuickFiler/Controllers/QfcCollectionController*.cs
QuickFiler/Controllers/QfcCollectionController.cs
```

The type is declared once, at line 22, without the `partial` modifier, and exactly one source file
matches the `QfcCollectionController*.cs` pattern in `QuickFiler/Controllers/`. There is no second
part anywhere in the tree, so the type cannot have been split.

This is the check AC-25 exists for. Splitting the file would have been an attractive way to bring it
under the 500-line cap, and it is explicitly out of scope: the spec's `## Follow-up Candidates`
entry 1 says "**Do not propose a file split in this feature**", because seven defect fixes plus a
type decomposition in one branch would make the diff unreviewable and would destroy the ability to
attribute a regression to a specific fix.

The file is 2,437 lines at HEAD against 2,349 at the base commit. That is recorded and analysed at
P15-T7; it is not a scope-creep finding, because the growth comes from the fixes themselves, their
XML doc comments, and the three seams — not from any new responsibility being added to the type.

## Check 2 — `ExcludeFromCodeCoverage` retained

```
$ grep -n 'ExcludeFromCodeCoverage' QuickFiler/Controllers/QfcCollectionController.cs
21:    [ExcludeFromCodeCoverage]
```

Present, once, at line 21, immediately above the type declaration at line 22 — the same position it
occupied at the base commit.

Removing it was out of scope. It is also the reason the `#468` coverage-denominator rationale must not
appear in the PR body: every line of the type is outside both the numerator and the denominator, so no
change to this file can move a coverage number. That constraint is recorded at
`evidence/other/pr-accuracy-constraints.2026-08-26T16-27.md`, constraint 1.

## Check 3 — no package additions

```
$ git diff --name-only 61edc19b 48c9ad8f | grep -i 'packages.config'
(no output — 0 paths)

$ git diff --name-only 61edc19b..HEAD | grep -i 'packages.config'
(no output — 0 paths)

$ git diff 61edc19b 48c9ad8f -- '*.csproj' | grep -E '^[+-]' | grep -ci 'PackageReference'
0

$ git diff 61edc19b..HEAD -- '*.csproj' | grep -E '^[+-]' | grep -ci 'PackageReference'
0
```

Both ranges are reported. `61edc19b 48c9ad8f` is this feature's own contribution; `61edc19b..HEAD` is
the full mandated range, which additionally contains the two merges of
`origin/epic/quickfiler-bug-family-integration`. **Neither range changes a single `packages.config`
file, and neither adds or removes a single `PackageReference` line in any csproj.**

Reporting both matters here: three csproj files changed in the full range
(`QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler/QuickFiler.csproj`,
`UtilitiesCS/UtilitiesCS.csproj`), of which only the first is this feature's. The zero above shows
that none of the three — this feature's or the siblings' — added a dependency.

This feature's own csproj change is five `Compile Include` lines and nothing else, recorded verbatim
at `evidence/qa-gates/p14-t11-test-file-constraints.2026-08-26T16-38.md`.

## Check 4 — the undo-stack parameter is retained on the interface member

Base commit:

```
$ git show 61edc19b:QuickFiler/Interfaces/IQfcCollectionController.cs | grep -n 'MoveEmailsAsync'
50:        Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems);
```

Current tree:

```
$ grep -n 'MoveEmailsAsync' QuickFiler/Interfaces/IQfcCollectionController.cs
63:        Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems);
```

The declaration text is **byte-identical**. The line number moved from 50 to 63 solely because a
13-line XML doc block was inserted above it; the member's return type, name, parameter type, and
parameter name are unchanged.

The full diff at this file, verbatim, is a pure addition:

```
@@ -47,6 +47,19 @@ namespace QuickFiler.Interfaces
         void EliminateSpaceForItems(int removalInex, int removalCount);
         void RemoveSpecificControlGroup(int intPosition);
         Task RemoveSpecificControlGroupAsync(int selection);
+
+        /// <summary>
+        /// Moves every cached item group's message to its assigned destination folder.
+        /// </summary>
+        /// <param name="StackMovedItems">
+        /// The undo stack. This parameter does not carry the undo records: the stack is populated
+        /// by the email filer's push-to-undo-stack path, which pushes onto
+        /// <c>Globals.AF.MovedMails</c>. That is the same instance the caller passes here, because
+        /// the caller reads it from the same globals object. Passing a different instance would not
+        /// redirect the undo records, and passing <c>null</c> does not suppress them. The parameter
+        /// is retained only for source compatibility with existing callers; removing it is a
+        /// follow-up candidate, not part of this change.
+        /// </param>
         Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems);
         void AddItemGroup(MailItem mailItem);
```

Nineteen lines added, four lines of context, zero lines removed. No other member of the interface was
touched, so the member set is unchanged and no implementer breaks.

Retaining the parameter is decision D11. Removing it would have forced an edit to
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225`, which is outside the owned file set;
the removal is filed as a follow-up at
`docs/features/potential/2026-08-26-qfc-remove-stackmoveditems-parameter.md`.

## Acceptance verification

- The artifact exists.
- All four checks are recorded as satisfied, each with its search output quoted verbatim above.
