# Coverage baseline — QfcHomeController.cs — issue #839

Timestamp: 2026-09-13T05-42
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; [xml]$x = Get-Content -LiteralPath coverage/839-baseline.cobertura.xml -Raw; $classes = @($x.SelectNodes("//class") | Where-Object { ($_.GetAttribute("filename") -replace [regex]::Escape([string][char]92), "/") -match "/QfcHomeController[.]cs$" }); $lines = @{}; foreach ($c in $classes) { foreach ($l in $c.SelectNodes("./lines/line | ./methods/method/lines/line")) { $n = [int]$l.GetAttribute("number"); $h = [int]$l.GetAttribute("hits"); if (-not $lines.ContainsKey($n) -or $h -gt $lines[$n]) { $lines[$n] = $h } } }; $valid = $lines.Count; $covered = @($lines.Values | Where-Object { $_ -gt 0 }).Count; $pkgs = @($x.SelectNodes("//package") | Where-Object { $_.GetAttribute("name") -match "^quickfiler([.]dll)?$" }); "DOC_LINE_RATE=$($x.coverage.GetAttribute("line-rate"))"; "PKG_COUNT=$($pkgs.Count)"; "PKG_NAME=$(if ($pkgs.Count -gt 0) { $pkgs[0].GetAttribute("name") } else { "none" })"; "PKG_LINE_RATE=$(if ($pkgs.Count -gt 0) { $pkgs[0].GetAttribute("line-rate") } else { "none" })"; "QFC_CLASS_NODES=$($classes.Count)"; "QFC_LINES_VALID=$valid"; "QFC_LINES_COVERED=$covered"; "QFC_LINE_PCT=$([math]::Round(100.0 * $covered / [math]::Max($valid, 1), 2))"; "LINE88_HITS=$(if ($lines.ContainsKey(88)) { $lines[88] } else { "absent" })"'
EXIT_CODE: 0

## Output Summary

DOC_LINE_RATE=0.20083593262632565
PKG_COUNT=1
PKG_NAME=QuickFiler
PKG_LINE_RATE=0.8056730919918508
QFC_CLASS_NODES=8
QFC_LINES_VALID=258
QFC_LINES_COVERED=201
QFC_LINE_PCT=77.91
LINE88_HITS=1

Scope: QuickFiler.Test assembly only; the repository-wide 80 percent floor is not measured by this plan (Decision D5).

Raw disposition: raw Cobertura discarded, not committed.

DOC_LINE_RATE and PKG_LINE_RATE are read from raw dotnet-coverage output that this plan does not post-process, so they include instrumented third-party modules and are not comparable to any repository-wide figure; they are recorded for before-and-after comparison under one identical method only.

The planned diff will add one statement that two tests execute and delete one comment line, which is not a coverable line, so it is not expected to lower the repository-wide line rate; that floor is unmeasured here, not waived, and the post-change figures are recorded in the coverage-comparison artifact rather than projected in this baseline.

## The figure AC11 compares against

BASELINE-QFC-LINE-PCT: 77.91, over 258 valid lines of which 201 are covered, de-duplicated by line number across the 8 `class` elements that share the QfcHomeController.cs file name (the async state-machine classes), taking the maximum hits per line number across both the class-level rollup and the method-level view, per Decision D5.

LINE88_HITS=1 is recorded for context only and is not yet the changed line. At the baseline, line 88 is the datamodel-loader statement `_datamodel = QfcDataModelLoader(Globals, this.Token);`, which the existing `Init_InitializesCorrectly` test executes. After the fix, line 88 becomes the inserted `CreateCancellationToken();` and the loader statement moves to line 89.

## Two command-transport adaptations, both semantics-preserving

1. `Set-Location -LiteralPath "REPO-ROOT";` was prepended. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one; every path in the span is worktree-relative and the plan assumes the current directory is the worktree root.
2. The plan's filename predicate `-match "[\\/]QfcHomeController[.]cs$"` was run as `($_.GetAttribute("filename") -replace [regex]::Escape([string][char]92), "/") -match "/QfcHomeController[.]cs$"`. Forced, not preferred, and the reason is a transport defect rather than a plan defect: the Bash-to-native-executable transport this executor is required to use de-doubles a doubled backslash, so PowerShell received the 4-character class `[\/]` instead of the 5-character `[\\/]`. Probe, run to settle it rather than infer it: `$s = "[\\/]"` printed `LEN=4` and `CHARS=91,92,47,93`, which is `[`, backslash, `/`, `]`. In .NET regex that class is an escaped forward slash and matches `/` only, so it cannot match a Windows separator. Raw dotnet-coverage output records absolute host paths with backslash separators, so the plan's predicate as transported matched nothing: the first run of this command printed `QFC_CLASS_NODES=0`, `QFC_LINES_VALID=0` and `LINE88_HITS=absent`, which would have failed this task's acceptance. The adaptation normalises both separator forms to `/` before matching and then requires a separator immediately before the file name, anchored at end of string, which is exactly the plan's intent. The `[char]92` spelling is used so the command text contains no literal backslash at all and is therefore transport-invariant. Single backslashes are unaffected by the transport and were left as written: the `\s` and `\b` classes in the companion test-log summary command matched correctly.

The same adaptation is applied identically wherever this plan cites CMD-COVERAGE-PARSE, so the baseline and the post-change measurement are taken under one identical method and the AC11 comparison is not skewed.

## Instrument-integrity note

This artifact does not rest on an artifact-exists check. The document was parsed and four independent structural values were read from it: `PKG_COUNT=1` locates the single QuickFiler package, `QFC_CLASS_NODES=8` locates the class elements for the file under change, `QFC_LINES_VALID=258` proves those elements carry line data, and `LINE88_HITS=1` reads a specific line's hit count. A run in which `dotnet-coverage` had written a document but the measurement had not happened would fail those reads rather than pass them.
