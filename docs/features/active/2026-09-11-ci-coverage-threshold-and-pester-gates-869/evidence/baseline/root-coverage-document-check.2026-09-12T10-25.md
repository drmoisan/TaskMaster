# Phase 0 — Repository-root coverage document check (P0-T16)

Timestamp: 2026-09-14T18-18

Purpose: confirm no stray `coverage.xml` sits at the repository root. Such a file is matched by neither the repository ignore file nor the formatter ignore file, and CSharpier processes `*.xml`, so a stray root document is a format-check liability.

## Glob listing

Tool: Glob
Pattern: `<repo-root>/coverage.xml`

Returned list, verbatim, with the item worktree root shown as `<repo-root>`:

```
<repo-root>\docs\features\archive\2026-06-08-hierarchical-lcppn-folder-prediction-177\evidence\baseline\2026-06-10T12-31\coverage.xml
<repo-root>\docs\features\archive\2026-07-03-quickfiler-navigation-key-collision-232\evidence\coverage\2026-07-03T16-58\coverage.xml
<repo-root>\docs\features\archive\2026-06-08-hierarchical-lcppn-folder-prediction-177\evidence\qa-gates\2026-06-12T15-26\coverage.xml
```

The Glob tool matched on the file name rather than on the exact path, so it returned three pre-existing documents that live under `docs/features/archive/`. **None of the three is at the repository root**, and all three belong to archived feature folders that this delivery does not touch. They sit under `docs/features/archive/**/evidence/**`, which the formatter ignore file excludes by its `**/evidence/**` entry, so they are not a format-check liability either.

## Direct existence check

Command: `pwsh -NoProfile -Command '<worktree prologue>; "RootCoverageXmlExists=" + (Test-Path -LiteralPath "<repo-root>/coverage.xml")'`
EXIT_CODE: 0
Output: `RootCoverageXmlExists=False`

## Verdict

**No repository-root `coverage.xml` exists.** No deletion was required and the deletion branch of this task did not fire.

## Explicit output path used by every Pester invocation in this plan

Every Pester invocation in this plan sets `$c.CodeCoverage.OutputPath = "coverage/pester-coverage.xml"`, so the Pester default of `coverage.xml` relative to the working directory is never relied on. That explicit path lies inside the `coverage/` directory, which `.gitignore` ignores at its line 144 with a single negated exception for the directory keep-file at line 145, so the document cannot be committed and cannot become a stray root artifact.

Output Summary: no repository-root `coverage.xml` exists, confirmed by a direct `Test-Path` returning `False`. The three documents the name-based Glob returned all live under archived feature evidence folders and are excluded by the formatter ignore file. Every Pester invocation in this plan writes to the explicit path `coverage/pester-coverage.xml`.
