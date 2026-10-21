# Final QA step 2 of 4 — .NET analyzers — issue #839

Timestamp: 2026-09-13T06-03
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/839-final-analyzers.detailed.log"; $code = $LASTEXITCODE; "MSBUILD_EXIT=$code"; exit $code'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $log = Get-Content -LiteralPath coverage/839-final-analyzers.detailed.log; $q = [string][char]34; $cscPat = "Task " + $q + "Csc" + $q; "CSC_TASK_LINES=$(@($log | Select-String -SimpleMatch -CaseSensitive $cscPat).Count)"; "ZERO_ERROR_SUMMARY_LINES=$(@($log | Select-String -Pattern "^\s+0 Error\(s\)\s*$").Count)"; "ERROR_SUMMARY_LINE=$((@($log | Select-String -Pattern "^\s+\d+ Error\(s\)\s*$") | Select-Object -Last 1).Line.Trim())"; "CS86_ERROR_LINES=$(@($log | Select-String -Pattern ": error CS86\d\d:").Count)"'
EXIT_CODE: 0

## Output Summary

MSBUILD_EXIT=0
CSC_TASK_LINES=18
ZERO_ERROR_SUMMARY_LINES=1
ERROR_SUMMARY_LINE=0 Error(s)
CS86_ERROR_LINES=0

`CSC_TASK_LINES=18` is the load-bearing value here and not a decoration. It counts the `Task "Csc"` entries in the detailed log, so it proves the compiler actually ran on 18 projects rather than being skipped. This is the check that distinguishes a real analyzer gate from a vacuous one: MSBuild's up-to-date check does not invalidate on a command-line property change, so a warm `/t:Build` would have returned exit 0 with `CoreCompile` skipped on every project and no analyzer would have executed. The Rebuild target was used, per the C# policy and Decision D8, and the count confirms it took effect.

The Outlook precondition was re-checked immediately before this rebuild and printed `OUTLOOK_STATE=CLOSED`. Outlook was not running and was not killed.

The command is character-for-character the CLAUDE.md analyzer command apart from the two logging-only switches the plan adds, the minimal console verbosity switch and the detailed file-logger switch, and apart from the transport prefix described below. No solution-wide Nullable property is passed.

Raw disposition: the msbuild detailed file log lives under the gitignored coverage directory at the worktree root and is not committed.

## Two command-transport adaptations, both semantics-preserving

1. `Set-Location -LiteralPath "REPO-ROOT";` prepended to both spans. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one; the log path and the solution path in the spans are worktree-relative.
2. In the summary span, the plan writes the pattern for the compiler-task count as a nested doubled-quote literal inside a `$()` subexpression inside an expandable string. Run as written it does not bind: PowerShell reads the doubled quotes as escaped quotes in the enclosing string rather than as a quoted pattern argument, and `Select-String` reports for every input line that the input object cannot be bound to any parameter, producing no count at all. This is the same parse-boundary defect class recorded for CMD-TEST-SUMMARY in evidence/baseline/baseline-quickfiler-tests.md, in its other form. The pattern was therefore built as a string concatenation using an explicit quote character, `$q = [string][char]34; $cscPat = "Task " + $q + "Csc" + $q`, and passed by variable. The resulting pattern is the exact four-word literal the plan intends, `-SimpleMatch` and `-CaseSensitive` are retained unchanged, and the count is the count the plan asks for. The same adaptation is applied identically wherever this plan cites CMD-MSBUILD-SUMMARY.
