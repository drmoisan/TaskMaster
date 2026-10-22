# P1-T4 alias durability guard It (b) inserted (issue #973; section 9 T4)

Timestamp: 2026-10-03T10-58
Command: Edit tool on tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 (old_string It (a)'s last assertion line followed by `    }` and `}`; new_string with It (b) and a blank separator inserted); Grep tool counts below; CMD-LINECOUNT; CMD-CRCOUNT; CMD-PARSE; CMD-HUNKS (git -C <execution-worktree-root> diff -U0 a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1)
EXIT_CODE: 0
Output Summary: It (b) inserted (30 lines plus one blank separator); 16 It blocks; 402 lines (371 + 31, under 500) with 402 carriage returns; 0 parse errors; every diff hunk starts at old line 293 or later, so lines 1-292 (fixtures and the Fizzler It) are untouched. No normalisation was needed.

`^\s*It 'carries an Aliases child on every System.Linq.AsyncEnumerable project Reference' \{`: 1
`QuickFiler,TaskMaster,ToDoModel,UtilitiesCS,UtilitiesCS.Test`: 1
`^\s*It '`: 16
LINECOUNT: 402 (LINES-AFTER-T3 371 plus 31)
CRCOUNT: 402
PARSE-ERRORS: 0
HUNK @@ -293,18 +293,2 @@ Describe 'Repository binding redirects (issue 953)' {
HUNK @@ -331,2 +315,3 @@ Describe 'Repository binding redirects (issue 953)' {
HUNK @@ -334,0 +320,82 @@ Describe 'Repository binding redirects (issue 953)' {
HUNK-OLD-START-MINIMUM: 293
EOL-NORMALISED: no
