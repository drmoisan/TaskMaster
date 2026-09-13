# P1-T4 — Scoped CSharpier format and check over the three Phase 1 paths

Timestamp: 2026-09-13T15-17
Command: dotnet tool run csharpier format QuickFiler/Controllers/QfcQueue.cs QuickFiler/Controllers/QfcQueue.Tlp.cs QuickFiler/Controllers/QfcQueue.UiIdle.cs
EXIT_CODE: 0
Output Summary: The formatter reported `Formatted 3 files in 2931ms.` and rewrote nothing: the
porcelain captures taken immediately before and immediately after the command are identical line for
line. The scoped check then reported `Checked 3 files in 858ms.` and exited 0.

The scoped form is used here rather than the repo-wide form because the repo-wide pass belongs to the
final QC loop in Phase 5; an interim repo-wide format would rewrite files outside the Write Set and
break the Phase 6 scope lock.

## Porcelain status immediately BEFORE the format command

```
 M .claude/agent-memory/orchestrator/MEMORY.md
 M QuickFiler/Controllers/QfcQueue.cs
 M QuickFiler/QuickFiler.csproj
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? .claude/agent-memory/orchestrator/cobertura-class-helper-property-names-differ-from-package.md
?? .claude/agent-memory/orchestrator/delegation-brief-silently-overrides-recorded-deviation.md
?? QuickFiler/Controllers/QfcQueue.Tlp.cs
?? QuickFiler/Controllers/QfcQueue.UiIdle.cs
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t1-split-tlp.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t2-split-uiidle.2026-09-12T10-25.md
```

## Format command output

```
Formatted 3 files in 2931ms.
EXIT_CODE: 0
```

## Porcelain status immediately AFTER the format command

```
 M .claude/agent-memory/orchestrator/MEMORY.md
 M QuickFiler/Controllers/QfcQueue.cs
 M QuickFiler/QuickFiler.csproj
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? .claude/agent-memory/orchestrator/cobertura-class-helper-property-names-differ-from-package.md
?? .claude/agent-memory/orchestrator/delegation-brief-silently-overrides-recorded-deviation.md
?? QuickFiler/Controllers/QfcQueue.Tlp.cs
?? QuickFiler/Controllers/QfcQueue.UiIdle.cs
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t1-split-tlp.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t2-split-uiidle.2026-09-12T10-25.md
```

The two captures are identical, so this run rewrote no file. The three agent-memory paths and the
untracked raw Cobertura baseline document are admitted by the Scope-lock rule: the first three lie
under the tracked agent-memory directory of this worktree and the fourth lies under a Write Set
evidence directory.

## Scoped check

```
Command: dotnet tool run csharpier check QuickFiler/Controllers/QfcQueue.cs QuickFiler/Controllers/QfcQueue.Tlp.cs QuickFiler/Controllers/QfcQueue.UiIdle.cs
Checked 3 files in 858ms.
EXIT_CODE: 0
```
