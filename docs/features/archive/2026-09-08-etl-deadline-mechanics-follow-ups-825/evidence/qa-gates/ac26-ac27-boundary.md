# AC26 and AC27 — Ownership Boundary with Sibling Feature 826

Timestamp: 2026-09-09T16-58

Command: git diff $b -- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs, with $b re-derived from evidence/baseline/base-commit.md per D3, its output filtered for lines matching the regular expression `^[+-][^+-].*Console\.WriteLine`

EXIT_CODE: 0

ConsoleWriteLineDiffLines: 0
CatchTaskCanceledExceptionCount: 1
CatchTimeoutExceptionCount: 1
IntCounterParameterCount: 1
ConsoleWriteLineOccurrenceCount: 2

Output Summary: The anchored diff for the shared file produces zero added and zero removed lines
containing Console.WriteLine, so both diagnostics survive byte-identical including their leading
indentation. The diagnostic inside the TaskCanceledException catch carries 20 leading spaces and the
one inside the TimeoutException catch carries 16, both unchanged; that was measured directly on the
post-change file as well as through the diff.

The `^[+-][^+-]` anchor excludes the diff's own `---` and `+++` header lines, so the zero result is
a property of the content lines rather than an artefact of a header carve-out. The file's own
`+++ b/UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` header would otherwise
match a naive `^[+-]` filter.

The corollary constraint AC27 states also holds: the `counter` variable is still declared exactly
once as a parameter, and both catch blocks still exist exactly once each. Feature 826's change
depends on all three, and this feature has removed none of them. The two Console.WriteLine
statements remain out of this feature's ownership; a reviewer must not read them as an oversight or
an incomplete cleanup.
