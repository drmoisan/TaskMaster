Timestamp: 2026-09-09T10-10
Task: P3-T1

Change: replaced the unguarded expression at (former) lines 231-234 of
QuickFiler/Controllers/QfcItemController.FolderHandling.cs with a narrow
try/catch (InvalidOperationException) around the Ol.ArchiveRootPath read, substituting
string.Empty on catch, exactly as specified by the plan. No other line in the file changed;
the `_globals is null ? null : ...` branch is preserved unchanged inside the try.

Verification: `grep -n "catch (InvalidOperationException)"` against the file returns exactly one
match (line 238, inside AssignFolderComboBox).

Acceptance (AC4 evidence, not yet sign-off): met.
