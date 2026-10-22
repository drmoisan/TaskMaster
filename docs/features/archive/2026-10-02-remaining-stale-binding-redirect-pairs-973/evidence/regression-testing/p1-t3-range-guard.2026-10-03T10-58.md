# P1-T3 range guard It (a) inserted (issue #973; section 9 T3)

Timestamp: 2026-10-03T10-58
Command: Edit tool on tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 (old_string the main It's last three lines: the Fizzler/Unsafe exclusion line, `    }`, `}`; new_string those lines with a blank separator and It (a) inserted between `    }` and `}`); Grep tool counts below; CMD-LINECOUNT; CMD-CRCOUNT; CMD-PARSE (pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; $t = $null; $e = $null; [System.Management.Automation.Language.Parser]::ParseFile("<execution-worktree-root>/tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1", [ref]$t, [ref]$e) | Out-Null; "PARSE-ERRORS: " + @($e).Count')
EXIT_CODE: 0
Output Summary: It (a) inserted (50 lines plus one blank separator); file 371 lines with 371 carriage returns; parses with 0 errors.

`^\s*It 'bounds every corrected redirect range at its newVersion across the repository app.config files' \{`: 1
`'System.Linq.AsyncEnumerable'\r?$`: 1
`every corrected redirect must bound its range at a csproj Reference version`: 1
LINES-AFTER-T3: 371
CRCOUNT: 371
PARSE-ERRORS: 0
