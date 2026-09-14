# Final QA step 3 of 4 — nullable type-check — issue #839

Timestamp: 2026-09-13T06-09
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/839-final-nullable.detailed.log"; $code = $LASTEXITCODE; "MSBUILD_EXIT=$code"; exit $code'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $log = Get-Content -LiteralPath coverage/839-final-nullable.detailed.log; $q = [string][char]34; $cscPat = "Task " + $q + "Csc" + $q; "CSC_TASK_LINES=$(@($log | Select-String -SimpleMatch -CaseSensitive $cscPat).Count)"; "ZERO_ERROR_SUMMARY_LINES=$(@($log | Select-String -Pattern "^\s+0 Error\(s\)\s*$").Count)"; "ERROR_SUMMARY_LINE=$((@($log | Select-String -Pattern "^\s+\d+ Error\(s\)\s*$") | Select-Object -Last 1).Line.Trim())"; "CS86_ERROR_LINES=$(@($log | Select-String -Pattern ": error CS86\d\d:").Count)"'
EXIT_CODE: 0

## Output Summary

MSBUILD_EXIT=0
CSC_TASK_LINES=18
ZERO_ERROR_SUMMARY_LINES=1
ERROR_SUMMARY_LINE=0 Error(s)
CS86_ERROR_LINES=0

`CSC_TASK_LINES=18` proves the compiler ran on 18 projects rather than being skipped, which is what makes this gate capable of failing. `CS86_ERROR_LINES=0` is the nullable-specific result: no `CS86xx` diagnostic was promoted to an error anywhere in the solution by `/p:TreatWarningsAsErrors=true`.

This is consistent with Decision D13 and with the change this item makes. QuickFiler/Controllers/QfcHomeController.cs carries no `#nullable` directive before or after the diff, so the file does not participate in nullable flow analysis and the inserted statement gains no `CS86xx` obligation. The continued absence of the directive is gated separately by [P3-T9].

The command is character-for-character the CLAUDE.md nullable command apart from the two logging-only switches the plan adds and the transport prefix described below. In particular no `/p:Nullable=enable` property is passed, which the C# policy forbids because no project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property would conscript every file that has never adopted the pragma. The Rebuild target is used rather than Build for the reason recorded under [P3-T2].

The Outlook precondition was re-checked immediately before this rebuild and printed `OUTLOOK_STATE=CLOSED`. Outlook was not running and was not killed.

This run's detailed log at coverage/839-final-nullable.detailed.log is also the positive control that [P3-T28] reads: it records absolute project paths, so the sanitisation instrument must find the account-name token in it.

Raw disposition: the msbuild detailed file log lives under the gitignored coverage directory at the worktree root and is not committed.

## Two command-transport adaptations, both semantics-preserving

1. `Set-Location -LiteralPath "REPO-ROOT";` prepended to both spans. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one.
2. In the summary span, the compiler-task pattern was built as `$q = [string][char]34; $cscPat = "Task " + $q + "Csc" + $q` and passed by variable, because the plan's nested doubled-quote literal does not bind when parsed inside a `$()` subexpression within an expandable string. The full diagnosis is recorded in evidence/qa-gates/final-analyzers.md; `-SimpleMatch` and `-CaseSensitive` are retained and the pattern matched is the same four-word literal.
