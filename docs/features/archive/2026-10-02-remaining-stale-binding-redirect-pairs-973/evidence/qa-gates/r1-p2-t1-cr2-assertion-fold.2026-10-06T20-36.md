# Remediation cycle 1, P2-T1: CR-2 fold of the known-debt literal into the count assertion

Timestamp: 2026-10-06T20-36
Command: Grep tool census over tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 before and after one Edit-tool replacement (two lines replaced by one); CMD-PARSE pwsh -NoProfile -Command '$t = $null; $e = $null; [System.Management.Automation.Language.Parser]::ParseFile("<execution-worktree-root>/tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1", [ref]$t, [ref]$e) | Out-Null; "PARSE-ERRORS: " + @($e).Count'; git -C <execution-worktree-root> diff --numstat HEAD -- tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1; git -C <execution-worktree-root> diff -U0 HEAD -- tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1
EXIT_CODE: 0

Before census:
- `@\(\$expectedDebt\)\.Count \| Should -Be 0` count 1 (expected 1)
- `Should -Be \$expectedDebt\.Count` count 0 (expected 0)
- `\$expectedDebt` count 2 (expected 2)
- `^\s+It '` count 16 (expected 16)
- CMD-LINECOUNT 402 (expected 402)
- CMD-CRCOUNT 402 (expected 402)

After census:
- `@\(\$expectedDebt\)\.Count \| Should -Be 0` count 0 (expected 0)
- `Should -Be \$expectedDebt\.Count` count 1, line 315 (expected 1)
- `\$expectedDebt` count 2, lines 293 (the literal) and 315 (the assertion) (expected 2)
- `^\s+It '` count 16 (expected 16)
- CMD-LINECOUNT 401 (expected 401)
- CMD-CRCOUNT 401 (expected 401; CRLF preserved)
- PARSE-ERRORS: 0
- `\$examined \| Should -Be \$redirectElement` count 1 (examined-count guard untouched)
- `Fizzler\|\*` count 1 (Fizzler/Unsafe exclusion untouched)

NUMSTAT: 1	2	tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1
HUNK @@ -315,2 +315 @@ Describe 'Repository binding redirects (issue 953)' {

Diff (-U0, verbatim body):
-        @($expectedDebt).Count | Should -Be 0 -Because 'issue 973 emptied the recorded known-debt set; a new stale pair is fixed, not recorded'
-        $actualDebt.Count | Should -Be 0 -Because ('every bindingRedirect newVersion must equal a csproj Reference version; observed: ' + ($actualDebt -join '; '))
+        $actualDebt.Count | Should -Be $expectedDebt.Count -Because ('every bindingRedirect newVersion must equal a csproj Reference version (issue 973 emptied the recorded known-debt set; a new stale pair is fixed, not recorded); observed: ' + ($actualDebt -join '; '))

Output Summary:
- The tautological assertion on the empty literal is removed and the literal is read by the real count assertion; expected value unchanged (0).
- One hunk, one line in place of two; file 402 to 401 lines, CRLF preserved, parses with zero errors.
- 16 It blocks unchanged; examined-count guard and Fizzler/Unsafe exclusion unchanged.
- Fail-before exception dossier: docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/fail-before-exception.2026-10-06T20-36.md.
