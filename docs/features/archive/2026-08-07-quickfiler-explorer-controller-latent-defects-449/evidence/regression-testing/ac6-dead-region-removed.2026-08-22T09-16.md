# AC-6 — Dead-Region Identifiers Removed (Issue #449, [P4-T6])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
git grep -n -E "SanitizeArrayLineTSV|StripTabsCrLf|WriteCSV_StartNewFileIfDoesNotExist|SanitizeArray|SaveMessageAsMSG|GetCurrentExplorerFolder" -- QuickFiler QuickFiler.Test
```
EXIT_CODE: 1
Output: (empty — no output)

`git grep` returns exit code 1 when there is no match. The output is empty.

## Result

**ZERO matching lines across BOTH path scopes.** The search covers `QuickFiler` and `QuickFiler.Test`
and finds none of the six identifiers deleted with the `#region Email Sorting To Rewrite` block:
`SanitizeArrayLineTSV`, `StripTabsCrLf`, `WriteCSV_StartNewFileIfDoesNotExist`, `SanitizeArray`,
`SaveMessageAsMSG`, `GetCurrentExplorerFolder`. No identifier survived the deletion, so AC-6 is
satisfied and no follow-up removal is required.

## Untracked-file confirmation

Command:
```
git grep -n --untracked -E "SanitizeArrayLineTSV|StripTabsCrLf|WriteCSV_StartNewFileIfDoesNotExist|SanitizeArray|SaveMessageAsMSG|GetCurrentExplorerFolder" -- QuickFiler QuickFiler.Test
```
EXIT_CODE: 1
Output: (empty)

Repeated with `--untracked` so the newly created and not-yet-committed
`QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` is in scope. Still zero matches: the new
test file references none of the six identifiers.

## Non-vacuity — the same search DID match before the deletion

A zero-match result only gates something if the same search would have matched beforehand. Verified
against the merge base:

Command:
```
git grep -c -E "SanitizeArrayLineTSV|StripTabsCrLf|WriteCSV_StartNewFileIfDoesNotExist|SanitizeArray|SaveMessageAsMSG|GetCurrentExplorerFolder" c551eabab0aa0a6b1a284252811a2e1de819634e -- QuickFiler QuickFiler.Test
```
EXIT_CODE: 0
Output:
```
c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Controllers/QfcExplorerController.cs:12
```

At the merge base the identical pattern and path scope matched **12 lines**, all inside
`QuickFiler/Controllers/QfcExplorerController.cs` (the dead region at merge-base lines 183-321). The
count fell from **12 to 0**. The search is therefore discriminating, not vacuous, and the zero is a
real observation about the deletion.

Note that no `QuickFiler/Legacy/` or `QuickFiler/Notes/` file matched even at merge base, so the zero
result is not an artifact of those uncompiled files happening to be clean.

## Scope note — the six names still exist ELSEWHERE, by design

The path scope is deliberately `QuickFiler QuickFiler.Test`. The six identifiers remain present in
other projects, where they name INDEPENDENT copies that this change does not touch:
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`,
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs`, and
`ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`, plus their own tests in `UtilitiesCS.Test`.
Those are the surviving maintained copies and they are explicitly out of scope; consolidating the
three copies is a separate, larger change. See `fail-before-exception.defect3.2026-08-22T09-16.md`
for the full enumeration.

## Output Summary

The AC-6 search returns **zero matching lines** across both `QuickFiler` and `QuickFiler.Test`
(EXIT_CODE 1, empty output), and still zero when repeated with `--untracked` to include the new test
file. The same search matched **12 lines** at the merge base, so the result is discriminating rather
than vacuous. All six identifiers from the deleted `#region Email Sorting To Rewrite` block are gone
from both path scopes. AC-6 is satisfied.
