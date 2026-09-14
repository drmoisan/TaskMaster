# Phase 0 — JaCoCo document shape (P0-T9)

Timestamp: 2026-09-14T18-08

Source document: `coverage/pester-coverage.xml`, written by the P0-T8 run.

Command: `pwsh -NoProfile -Command '<worktree prologue>; [xml]$j = Get-Content coverage/pester-coverage.xml -Raw; $n = @($j.report.counter) | Where-Object { $_.type -eq "LINE" }; "LINE covered=" + $n.covered + " missed=" + $n.missed'`
EXIT_CODE: 0

## Report-level LINE counter

Selector expression used, and the single definition every later measurement task and the workflow gate body use:

```
@($j.report.counter) | Where-Object { $_.type -eq "LINE" }
```

Number of counter elements that matched: exactly 1.

Printed line, verbatim:

```
LINE covered=662 missed=177
```

- covered = 662
- missed = 177
- total = 839

Because exactly one element matched, the corrective branch of this task — recording the actual root element name, the actual location of the report-level LINE counter, and a corrected selector expression — does not apply. The selector above stands unchanged for every later task.

Corroborating context, recorded so the selector's scope is unambiguous. The document root element is `report`, and the four report-level counters it carries are, verbatim from the same run:

```
INSTRUCTION:792/210 LINE:662/177 METHOD:48/9 CLASS:12/2
```

The `INSTRUCTION` counter is the command (instruction) figure Pester reports as `CoveragePercent`; 792 of 1002 is 79.0419 percent, which reproduces the `PESTER COMMANDPERCENT=79.0419161676647` line recorded in P0-T8 to every printed digit. That identity confirms the two counters are distinct and that the gate reads `LINE`, not `INSTRUCTION`. The `CLASS:12/2` counter shows two of the fourteen production scripts have no covered class entry.

## Method-entry shape

Command: `pwsh -NoProfile -Command '<worktree prologue>; [xml]$j = Get-Content coverage/pester-coverage.xml -Raw; $cls = @($j.SelectNodes("//class")) | Where-Object { $_.GetAttribute("name") -like "*Threshold*" }; foreach ($c in $cls) { ...; foreach ($m in @($c.SelectNodes("method"))) { $lc = @($m.counter) | Where-Object { $_.type -eq "LINE" }; ... } }'`
EXIT_CODE: 0

Output, verbatim:

```
CLASS=vscode/Invoke-MSTestWithCoverage.Threshold sourcefilename=Invoke-MSTestWithCoverage.Threshold.ps1
  METHOD=<script> line=1 LINEcovered=1 LINEmissed=0
  METHOD=Assert-CoberturaLineCoverageThreshold line=31 LINEcovered=13 LINEmissed=1
```

Findings:

1. The document does carry `method` elements, each with a `name` attribute holding the PowerShell function name, a `line` attribute holding the function's declaration line, and child `counter` elements including a `LINE` counter with `covered` and `missed` attributes. The corrective branch of this task therefore does not apply and P6-T5 uses the selector recorded below rather than a corrected per-function expression.
2. The name the document uses for statements that sit outside any function is the literal `<script>`. That entry carries the file's top-level statements. In a file behind an invocation guard, the guard-body invocation statement falls inside that `<script>` entry and is uncovered by construction, because it never runs while the file is dot-sourced.

## Selector that reaches a named method entry

The selector P6-T5 uses to read the LINE counter of a named method, stated once here and used unchanged there:

```
@($j.SelectNodes("//method")) | Where-Object { $_.GetAttribute("name") -eq "<FunctionName>" }
```

and then, on the matched element,

```
@($matched.counter) | Where-Object { $_.type -eq "LINE" }
```

reading its `covered` and `missed` attributes. The enclosing `class` element's `sourcefilename` attribute identifies the production file the method belongs to, which disambiguates any two same-named functions across files.

Output Summary: exactly one report-level `LINE` counter matched the recorded selector, reading covered 662 and missed 177 over a total of 839. The document offers named `method` elements, so per-function LINE counters are directly readable, and statements outside any function are grouped under the method name `<script>`. No corrected expression is required by either half of this task.
