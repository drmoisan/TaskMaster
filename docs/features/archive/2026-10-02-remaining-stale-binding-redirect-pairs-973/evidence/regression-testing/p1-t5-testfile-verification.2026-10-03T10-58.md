# P1-T5 whole-file re-verification (issue #973)

Timestamp: 2026-10-03T10-58
Command: git -C <execution-worktree-root> ls-files --eol -- tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 (CMD-EOL); Grep tool `Set-StrictMode -Version Latest` -n, `^Describe ` count, `Import-Module .*-Force` count; CMD-PARSE; CMD-HUNKS (git -C <execution-worktree-root> diff -U0 a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1)
EXIT_CODE: 0
Output Summary: the edited test file is w/crlf, keeps Set-StrictMode at line 1, three Describe blocks and two forced imports, parses with 0 errors, and every diff hunk starts at old line 293 or later (in-memory fixture Describes untouched).

CMD-EOL: i/lf    w/crlf  attr/text=auto (second field w/crlf)
`Set-StrictMode -Version Latest`: 1 at line 1
`^Describe `: 3
`Import-Module .*-Force`: 2
PARSE-ERRORS: 0
HUNK @@ -293,18 +293,2 @@ Describe 'Repository binding redirects (issue 953)' {
HUNK @@ -331,2 +315,3 @@ Describe 'Repository binding redirects (issue 953)' {
HUNK @@ -334,0 +320,82 @@ Describe 'Repository binding redirects (issue 953)' {
HUNK-OLD-START-MINIMUM: 293
