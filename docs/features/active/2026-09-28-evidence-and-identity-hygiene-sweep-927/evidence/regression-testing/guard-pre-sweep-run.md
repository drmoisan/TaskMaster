# P1-T12 Guard over the pre-sweep tracked tree (expect-fail, AC1 observation)

Timestamp: 2026-09-29T19-34
Command: pwsh -NoProfile -Command '& ./scripts/hygiene/Test-RepositoryHygiene.ps1 2>&1 | Tee-Object -FilePath coverage/logs/927-guard-run.log; exit $LASTEXITCODE'
ExpectedExitCode: 1
EXIT_CODE: 1
Output Summary:
- GATE6 exited 1 (expected 1) after 1190 seconds of wall clock over the pre-sweep tracked tree.
- Count payload (P1-T12, labelled lines): RAW=625, PROFILE=1214, UNREADABLE=0, HYGIENE Findings=1839.
- HYGIENE Findings=1839 equals RAW plus PROFILE (625 + 1214).
- RAW=625 equals RAW-POPULATION: 625 from the P0-T17 artifact (equality holds).
- PROFILE=1214 equals PROFILE-PATH-FILES-NOW=1214 from the re-measurement below (equality holds).
- Re-measurement payload (labelled lines): RX-LENGTH=40, RX-CANONICAL=False, GATE4-NOW=1213, UTF16-FILES-NOW=1, UTF16-PROFILE-FILES-NOW=1, PROFILE-PATH-FILES-NOW=1214.
- REMOVAL-LIST-LINES: 625 (SCRATCH\raw-documents.txt written per D4 at the expression Join-Path $env:TEMP "hygiene-927"; its value is not recorded).
- No finding line is transcribed; counts and the one UTF-16 path only.
- AC1 arithmetic: RAW-POPULATION (625) plus PROFILE-PATH-FILES-NOW (1214) = 1839 = HYGIENE Findings. No stop condition applies.

MEASUREMENT-CORRECTION:
- BASELINE-PROFILE-PATH-FILES: 1213 (transcribed from the P0-T17 artifact, which is not edited)
- BASELINE-UTF16-PROFILE-FILES: 0 (transcribed from the P0-T17 artifact, which is not edited)
- Re-measurement command (verbatim, pattern intact):

```text
pwsh -NoProfile -Command '[System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); $b = [char]92; $sep = "[" + $b + $b + "/]+"; $rx = "(?im)[a-z]:" + $sep + "users" + $sep + "[a-z0-9_.~-]"; "RX-LENGTH=" + $rx.Length; "RX-CANONICAL=" + ($rx -ceq "(?im)[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]"); $g = @(git grep -I -l -i -E -e "[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]" -- . ":!.claude/"); "GATE4-NOW=" + $g.Count; $u = @(git ls-files --eol -- . ":!.claude/" | Where-Object { $_ -like "i/-text*" } | ForEach-Object { $_.Substring($_.IndexOf([char]9) + 1) } | Where-Object { $bytes = [System.IO.File]::ReadAllBytes($_); $bytes.Length -ge 2 -and (($bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) -or ($bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF)) }); "UTF16-FILES-NOW=" + $u.Count; $p = @($u | Where-Object { [System.IO.File]::ReadAllText($_) -match $rx }); "UTF16-PROFILE-FILES-NOW=" + $p.Count; foreach ($f in $p) { "UTF16-PROFILE-PATH| " + $f }; "PROFILE-PATH-FILES-NOW=" + ($g.Count + $p.Count)'
```

- Printed: RX-LENGTH=40 and RX-CANONICAL=False.
- Pattern integrity: the executor confirmed the measuring pattern reached pwsh intact by the printed length 40 of the character-code-92 construction, which carries the two-backslash separator class; RX-CANONICAL=False records only that the typed literal in the same payload was collapsed by the Bash-to-pwsh channel, as expected on a Bash launch, and it has no effect on the measurement.
- UTF16-PROFILE-PATH| docs/features/archive/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/2026-05-14T12-41-05Z/msbuild-analyzers.txt
- GATE4-NOW: 1213
- UTF16-PROFILE-FILES-NOW: 1
- PROFILE-PATH-FILES-NOW: 1214 (GATE4-NOW: 1213 plus UTF16-PROFILE-FILES-NOW: 1)
- UNDERCOUNT-CAUSE: the P0-T17 UTF-16 census typed its .NET separator class with a doubled backslash that the Bash-to-pwsh channel collapsed to a single backslash, so the class matched only a forward slash, no backslash-separated path in the UTF-16 file matched, the census recorded UTF16-PROFILE-FILES: 0 and the baseline therefore recorded PROFILE-PATH-FILES: 1213, while GATE4 was unaffected because a POSIX bracket expression treats a backslash as a literal and the class names the same two characters whether one or two backslashes arrive.

Execution notes:
- This artifact is rewritten in full by this run and supersedes the earlier stop record at the same path.
- Every pwsh payload ran with a prefix that sets the PowerShell location and the .NET process directory to <repo-root> (the item worktree), because pwsh launched from Bash starts in the session checkout.
- The GATE6 console copy was discarded with Out-Null after Tee-Object and the elapsed seconds were measured with a stopwatch around the invocation; the full output is in coverage/logs/927-guard-run.log under the ignored coverage directory and is never committed.
