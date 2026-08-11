---
name: powershell-gate-observables
description: Five PowerShell/Pester gate observables that make plan acceptance vacuous — no Invoke-Pester exit code, missing config/poshqc-scan.json, git status cannot count edits, outer-double-quote command templates get $-expanded, and Pester writes a repo-root coverage.xml
metadata:
  type: project
---

Three measurement seams that turn a PowerShell plan gate vacuous. Verified 2026-08-10 while revising the #457 plan.

**`Invoke-Pester` sets no process exit code.** `New-PesterConfiguration` defaults `Run.Exit` to `$false`, and `pwsh -Command` exits 0 unless the script calls `exit` or raises an unhandled terminating error. A direct-Pester command template that ends in count-emitting statements therefore always yields `EXIT_CODE: 0`, so an `EXIT_CODE: 0` acceptance is vacuous and a required non-zero `EXIT_CODE:` on an `[expect-fail]` task is unsatisfiable. Do NOT fix this with `Run.Exit = $true` — it calls `exit` before the count-emitting statements run and the numbers are lost. Append an explicit `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }` instead, and emit the counts as one interpolated string beforehand.

**`mcp__drm-copilot__run_poshqc_test` resolves its scan set from `config/poshqc-scan.json`, which does not exist in this repository.** Always pass `scan_folders` explicitly; the tool accepts individual file paths, not just folders. Also require `MCP Result: ok:true` in acceptance — an `ok:false` run with a green paired direct run lets a toolchain AC be checked off while the MCP test step errored. (Related known gap: `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` named by `.claude/rules/powershell.md` is a bundled extension resource, also absent here; the tool exposes no settings parameter.)

**`git status --porcelain` cannot measure "exactly N edits".** It reports only that a file is modified. Any AC of the form "exactly two edits in `<file>`" needs `git diff -- <file>` with recorded added-line/removed-line counts, plus an explicit exemption for hunks attributable to a Phase 0 in-place format run (`run_poshqc_format` rewrites files). Re-measure after the final format pass, since formatting can add hunks. A **file list cannot identify hunks** — have the Phase 0 format task record the verbatim `git diff -- <file>` taken immediately after the format run (tree is clean there, so the diff IS the baseline hunks), and phrase the exclusion as "any hunk present verbatim in that recorded diff".

**The direct-Pester command template must use OUTER single quotes and INNER bare double quotes.** Wrapping the inner script in double quotes (with or without `\"` escaping) lets both git-bash and a `pwsh` host expand `$` before `pwsh` parses it: measured output was `Passed= Failed=` with exit `1` on an all-passing run — inverting the very `EXIT_CODE: 0` gate the explicit `exit` was added to make load-bearing. Use `"Detailed"`, not `'Detailed'`, so no single quote appears inside the script. Verified working in both shells: `pwsh -NoProfile -Command '…; $c.Output.Verbosity = "Detailed"; …; "Passed=$($r.PassedCount) …"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`.

**Pester's `CodeCoverage.OutputPath` defaults to `coverage.xml` in the process working directory** and is always written when `CodeCoverage.Enabled = $true`. A repo-root `coverage.xml` is not matched by this repo's `.gitignore` (`*.coverage`, `*.coveragexml`, `coverage/*`), so it violates a no-temporary-files prohibition and breaks any whole-repo `git status --porcelain -uall` changed-file audit. Always set `OutputPath` explicitly under `<FEATURE>/evidence/<kind>/`, picking the kind per calling task, and add a clause to the changed-file audit acknowledging those artifacts as expected evidence.

**Why:** each of these passed a plan validator and a first preflight while measuring nothing; all three were caught only by reading the tool's actual default behavior rather than the plan's stated intent.

**How to apply:** when writing any PowerShell QA-gate or expect-fail task, ask what observable the acceptance actually reads and whether the command can produce the failing value at all. See [[project_457_closure_filter_plan_seams]] and [[diff-gates-need-a-commit-task]].
