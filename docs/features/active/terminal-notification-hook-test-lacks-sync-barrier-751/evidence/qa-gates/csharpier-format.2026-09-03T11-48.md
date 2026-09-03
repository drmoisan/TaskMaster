# P4-T1 — Scoped CSharpier Format (Issue #751)

Timestamp: 2026-09-03T14-39

Command: `dotnet tool run csharpier format TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs`

EXIT_CODE: 0

Sanitization: applied. Placeholder tokens used: `<WORKTREE>` for the worktree root (matched
case-insensitively in both slash directions) and `<USER>` for the account name derived at run time from
`$env:USERPROFILE`. Neither token appears in the transcribed stdout, because the command named no path.

## Recorded deviation from `CLAUDE.md`

`CLAUDE.md` prescribes `dotnet tool run csharpier format .` in § "C# Toolchain (run in this exact order)"
step 1 (`CLAUDE.md:405`) and in § C#1.1 (`CLAUDE.md:192`), and `spec.md:315` repeats it. **This task
deliberately deviates from that prescription by scoping the write to the two files this plan owns.**

Rationale, recorded by name rather than left implicit: a repository-wide `format` is a **write-mode** command
over files this plan does not own. Any file it repaired would be rewritten and would enter the branch diff,
which would falsify the AC4 and AC7 footprint gates that P4-T8, P4-T10, and P4-T11 enforce.

The deviation is scoped to the **write** step only. The read-only verification
`dotnet tool run csharpier check .` is run **repository-wide and unmodified** by P4-T2, so `CLAUDE.md`'s
repository-wide formatting *standard* remains enforced in full. What is narrowed is only which files this
plan is permitted to rewrite.

## Three required observations

A formatter rewrites tracked source and still exits 0 after rewriting, so its exit code alone cannot
distinguish a clean run from a repairing one. All three of the following are therefore recorded.

### 1. Command stdout (sanitized)

```
Formatted 2 files in 1113ms.
```

### 2. `git status --porcelain -- TaskMaster.Test/AppGlobals`, taken immediately BEFORE the command

```
(no lines)
```

### 3. `git status --porcelain -- TaskMaster.Test/AppGlobals`, taken immediately AFTER the command

```
(no lines)
```

## Interpretation

The before-capture and the after-capture are **identical** (both empty; compared mechanically, result
`True`). The formatter therefore **did not rewrite either file**. This is the expected result: the two files
were already CSharpier-clean, because the P0-T11 repository-wide baseline check exited 0 over 1574 files and
the three lines added by Phase 2 were authored in already-formatted shape.

"Formatted 2 files" is CSharpier's count of files **processed**, not files **changed**; the porcelain pair is
what establishes that nothing was changed.

Because the after-capture does not differ from the before-capture, the Phase 4 pass is **not** void and the
phase does not restart at P4-T1.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |
| Carries the line `Sanitization: applied` | present above | PASS |
| Carries both porcelain-status captures | both recorded above | PASS |
| Carries a statement naming `CLAUDE.md` as the source of the `format .` prescription being deviated from, together with the footprint-gate rationale | recorded under "Recorded deviation from `CLAUDE.md`" | PASS |
