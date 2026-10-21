# P0-T9 — The Bundled PowerShell Coverage Artifact Is Non-Probative For This Feature

Timestamp: 2026-09-09T10-48
Task: [P0-T9]
Command: `pwsh -NoProfile -Command 'Test-Path -LiteralPath "artifacts/pester/powershell-coverage.xml"'`
EXIT_CODE: 0

```
True
```

## Branch applied: A — the file exists

The plan authorizes exactly two branches. Branch A applies: `artifacts/pester/powershell-coverage.xml`
is present in this worktree. It was produced by the P0-T7 PoshQC test run, whose bundled coverage
configuration is independent of the `scan_folders` argument that run supplied.

The plan's executor note predicted Branch B on the grounds that the file was absent while the plan
was authored. That prediction did not hold in this worktree, and the deviation is recorded here. It
is not a failure: the plan states that either branch establishes the same conclusion, and Branch A
establishes it with a stronger, directly measured observation.

## Report-level LINE totals

Command: `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "artifacts/pester/powershell-coverage.xml"; $root = $j.DocumentElement; foreach ($c in @($root.SelectNodes("./counter"))) { Write-Output ("ROOTCOUNTER type=" + $c.type + " covered=" + $c.covered + " missed=" + $c.missed) }; foreach ($p in @($j.SelectNodes("//package"))) { Write-Output ("PACKAGE=" + $p.name) }'`
EXIT_CODE: 0

Root element name: `Pester (09/09/2026 10:42:36)`

| Counter type | covered | missed |
| --- | --- | --- |
| INSTRUCTION | 0 | 9095 |
| LINE | **0** | **6583** |
| METHOD | 0 | 590 |
| CLASS | 0 | 77 |

## Every `<package>` name in the document (9)

The tool writes absolute host paths into the `name` attribute. They are recorded here as
repository-relative paths, because a committed artifact must not carry an absolute host path. No
package was omitted and none was renamed.

1. `.claude/hooks`
2. `.claude/lib/blast-radius`
3. `.claude/lib/codex-routing`
4. `.claude/lib/discovery-validation`
5. `.claude/lib/hook-payload`
6. `.claude/lib/mermaid`
7. `.claude/lib/model-routing`
8. `.claude/lib/orchestrator-state`
9. `.codex/hooks`

Output Summary: The bundled artifact exists and its report-level LINE counter is covered 0 / missed
6583. All nine of its `<package>` elements name paths under `.claude/` or `.codex/`. **Not one names
any path under the scripts tree**, so the artifact measures no file this feature adds or modifies,
and its zero covered count is a property of the folders it instruments rather than a coverage
regression. The bundled artifact therefore cannot supply changed-file coverage for this feature.
AC10 is measured instead by the direct `Invoke-Pester` runs of P0-T8 (baseline) and P5-T7
(post-change), whose `CodeCoverage.Path` is explicitly set to `scripts/vscode`.
