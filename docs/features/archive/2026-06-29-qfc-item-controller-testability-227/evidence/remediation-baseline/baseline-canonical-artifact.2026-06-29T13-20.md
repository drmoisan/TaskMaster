# Baseline — Canonical Coverage Artifact Presence (P0-T2)

Timestamp: 2026-06-29T13-20

Command: Test-Path artifacts/csharp/coverage.xml
(executed equivalently as `ls -la artifacts/csharp/coverage.xml`)

EXIT_CODE: 2

## Output Summary

`artifacts/csharp/coverage.xml` = ABSENT at cycle entry.

`ls: cannot access 'artifacts/csharp/coverage.xml': No such file or directory`

This is the defect baseline for R1: the workflow-mandated canonical Cobertura C# coverage artifact
does not exist. Coverage was recorded only in feature-folder evidence files. The remainder of this
cycle generates this artifact at the canonical path.
