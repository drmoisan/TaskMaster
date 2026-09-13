# Coverage comparison — QfcHomeController.cs — issue #839

Timestamp: 2026-09-13T06-14
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; [xml]$x = Get-Content -LiteralPath coverage/839-final.cobertura.xml -Raw; $classes = @($x.SelectNodes("//class") | Where-Object { ($_.GetAttribute("filename") -replace [regex]::Escape([string][char]92), "/") -match "/QfcHomeController[.]cs$" }); $lines = @{}; foreach ($c in $classes) { foreach ($l in $c.SelectNodes("./lines/line | ./methods/method/lines/line")) { $n = [int]$l.GetAttribute("number"); $h = [int]$l.GetAttribute("hits"); if (-not $lines.ContainsKey($n) -or $h -gt $lines[$n]) { $lines[$n] = $h } } }; $valid = $lines.Count; $covered = @($lines.Values | Where-Object { $_ -gt 0 }).Count; $pkgs = @($x.SelectNodes("//package") | Where-Object { $_.GetAttribute("name") -match "^quickfiler([.]dll)?$" }); "DOC_LINE_RATE=$($x.coverage.GetAttribute("line-rate"))"; "PKG_COUNT=$($pkgs.Count)"; "PKG_NAME=$(if ($pkgs.Count -gt 0) { $pkgs[0].GetAttribute("name") } else { "none" })"; "PKG_LINE_RATE=$(if ($pkgs.Count -gt 0) { $pkgs[0].GetAttribute("line-rate") } else { "none" })"; "QFC_CLASS_NODES=$($classes.Count)"; "QFC_LINES_VALID=$valid"; "QFC_LINES_COVERED=$covered"; "QFC_LINE_PCT=$([math]::Round(100.0 * $covered / [math]::Max($valid, 1), 2))"; "LINE88_HITS=$(if ($lines.ContainsKey(88)) { $lines[88] } else { "absent" })"'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; (Get-Content -LiteralPath QuickFiler/Controllers/QfcHomeController.cs)[87]'
EXIT_CODE: 0

## Output Summary

| Metric | Before ([P0-T19]) | After ([P3-T5]) |
|---|---|---|
| QFC_LINE_PCT | 77.91 | 77.99 |
| QFC_LINES_VALID | 258 | 259 |
| QFC_LINES_COVERED | 201 | 202 |
| PKG_LINE_RATE | 0.8056730919918508 | 0.8056883177936222 |
| DOC_LINE_RATE | 0.20083593262632565 | 0.20084590335741287 |

NEW_STATEMENT_LINE=88
LINE88_HITS=1
PKG_COUNT=1
PKG_NAME=QuickFiler
QFC_CLASS_NODES=8

Line 88 of the production file reads, trimmed: `CreateCancellationToken();`

AC11 no-regression result: after `QFC_LINE_PCT` of 77.99 is greater than before `QFC_LINE_PCT` of 77.91, so per-file line coverage for the changed file did not regress. The movement is explained entirely by the diff: the valid-line denominator rose by exactly one, from 258 to 259, because the inserted statement is a new coverable line, and the covered-line numerator rose by exactly one, from 201 to 202, because that line is executed. The removed dead comment is not a coverable line and appears in neither figure.

`LINE88_HITS=1` is the changed line's own coverage and is the reason the numerator moved. Both `Init_InitializesCorrectly` and the new `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` call `Init()`, so the inserted statement is reached by two tests in this run.

Scope: QuickFiler.Test assembly only; the repository-wide 80 percent floor is not measured by this plan (Decision D5).

Raw disposition: raw Cobertura discarded, not committed.

DOC_LINE_RATE and PKG_LINE_RATE are read from raw dotnet-coverage output that this plan does not post-process, so they include instrumented third-party modules and are not comparable to any repository-wide figure; they are recorded for before-and-after comparison under one identical method only.

This diff adds one statement that two tests execute and deletes one comment line, which is not a coverable line, so it cannot lower the repository-wide line rate; that floor is unmeasured here, not waived.

## Method parity between the two sides

Both figures were produced by the same command against documents produced by the same command, with the same assembly, the same derived exclusion settings, the same isolation switch and the same test-case filter. Two adaptations were applied and both were applied identically to both sides, so no method skew is introduced:

1. `Set-Location -LiteralPath "REPO-ROOT";` prepended, forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one.
2. The filename predicate was run as a separator normalisation followed by `-match "/QfcHomeController[.]cs$"` instead of the plan's `-match "[\\/]QfcHomeController[.]cs$"`, because the Bash-to-native transport de-doubles a doubled backslash and the class as transported matched forward slashes only. The probe that settles this, and the zero-match result the unadapted form produced, are recorded in evidence/baseline/coverage-baseline.md. The adapted predicate expresses the plan's intent exactly: a separator of either form immediately before the file name, anchored at end of string.

Per Decision D5 the per-file figure de-duplicates `line` elements by number across all 8 `class` elements sharing this file name, enumerates both the class-level rollup and the method-level view, and retains the maximum hits per line number, which is the rule the repository's own Cobertura helper applies.
