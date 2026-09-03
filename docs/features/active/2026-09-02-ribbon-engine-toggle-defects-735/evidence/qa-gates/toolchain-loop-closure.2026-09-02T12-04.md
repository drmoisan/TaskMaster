# Phase 4 — Toolchain Loop Closure (P4-T11)

Timestamp: 2026-09-03T03-27
Task: [P4-T11]
Command: this task performs no command of its own; it reconciles the exit codes of P4-T1 through P4-T10, each recorded in its own artifact.
EXIT_CODE: 0

## Pass number: 1 — and it is the only pass

No step failed, and no tracked file was rewritten after the format step in a way that required
restarting the loop. The loop therefore completed in a single pass and no failed pass exists to
record.

## The ten steps with their exit codes

| Step | Task | Command | EXIT_CODE | Result |
|---|---|---|---|---|
| 1 | P4-T1 | `dotnet tool run csharpier format <in-scope paths>` | 0 | 4 of 8 rewritten on pass 1; 2 of 10 on the branch B re-run |
| 2 | P4-T2 | post-format line-count audit | 0 | 515 on pass 1, above the ceiling; 415 after branch B |
| 3 | P4-T3 | coordinator size contingency | 0 | branch B taken; final count 415, at or below 500 |
| 4 | P4-T4 | `dotnet tool run csharpier check .` | 0 | 1576 files checked, no unformatted path |
| 5 | P4-T5 | `msbuild ... /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | 5 warnings, 0 errors — equal to baseline |
| 6 | P4-T6 | `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` | 0 | 5 warnings, 0 errors — equal to baseline |
| 7 | P4-T7 | `Invoke-MSTestWithCoverage.ps1` over the whole first-party suite | 0 | 6982 tests, 6982 passed, 0 failed |
| 8 | P4-T8 | coverage delta and new-code figures | 0 | every required row numeric or explicitly ABSENT |
| 9 | P4-T9 | anchored diff for added or removed exemption attributes | 0 | 0 added, 0 removed |
| 10 | P4-T10 | anchored footprint diff plus porcelain status | 0 | 12 source paths, all authorized; prohibited paths absent |

Every step exited 0.

## Why the format-step rewrites did not force a restart

The restart obligation is triggered by a later FAILING step, not by the format step having rewritten
a file. P4-T1's own acceptance says so explicitly: if the rewritten count is greater than zero,
execution continues to P4-T2.

The ordering of this pass matters and is worth stating, because the branch B extraction happened
inside the loop rather than before it:

1. The format step ran and rewrote 4 files.
2. The line-count audit found the coordinator at 515, above the ceiling.
3. P4-T3 resolved that on branch B by extracting the versioned cache, which changed tracked source.
4. Branch B's own terms require P4-T1 and P4-T2 to be re-run, and they were. The re-run format pass
   rewrote only the two newly authored branch B files; every pre-existing path was byte-identical.
5. Only then did steps 4 through 10 run, so every gate from the repository-wide format check onward
   observed the FINAL tree, after the extraction. No gate measured a superseded state.

That is why this counts as one clean pass rather than a failed pass followed by a clean one: no gate
failed at any point, and the mid-loop source change was mandated by an in-loop task whose own terms
required the two earlier steps to be repeated, which they were.

## Toolchain order observed

Format (P4-T1, P4-T4) then lint (P4-T5) then type-check (P4-T6) then test with coverage (P4-T7),
followed by the three scope and coverage verification gates (P4-T8, P4-T9, P4-T10). This is the
order the C# code-change policy requires.

## No file rewritten after the format step

After the branch B re-run of P4-T1, the repository-wide read-only check in P4-T4 reported zero
unformatted files across 1576 files. Since that check ran after every source edit in this plan and
found nothing to format, no tracked file was left in an unformatted state by any later step, and no
later step rewrote a tracked source file.

Output Summary: The full toolchain loop completed in a single clean pass. All ten steps P4-T1 through
P4-T10 exited 0, in the required order of format, lint, type-check, then test with coverage. No step
failed and no restart was required. The mid-loop branch B extraction was followed by the re-run of
P4-T1 and P4-T2 that branch B mandates, so every subsequent gate observed the final tree.
