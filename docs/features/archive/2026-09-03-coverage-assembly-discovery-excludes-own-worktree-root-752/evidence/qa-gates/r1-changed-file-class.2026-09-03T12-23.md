# Changed-File Classification and Toolchain Applicability — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-47
- Task: `[P2-T1]`

Command: `git -C <repo-root> status --porcelain -uall`

EXIT_CODE: `0`

The `-uall` flag is required and was not dropped. With the default `-unormal`, git collapses a newly
created untracked directory into a single entry naming the directory rather than its files.
`[P0-T1]` creates `evidence/remediation-baseline/`, which did not exist in the tree before this plan
ran, so a collapsed directory entry would not end in `.md` and would trip this task's own
stop-and-report clause even though every file inside it is markdown.

Output Summary:

## PREEXISTING: paths present in `PORCELAIN_BASELINE:` (recorded in `[P0-T2]`), excluded from classification

```
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/code-review.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/phase0-instructions-read.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/feature-audit.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/policy-audit.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md
```

All six are markdown files under `docs/features/`, so their exclusion does not affect the
classification either way.

## AGENT_MEMORY_WRITES:

`<none>`. No path under `.claude/agent-memory/` appears in this porcelain output. Had any appeared,
it would be the executing agent's standing, plan-independent memory-persistence write rather than a
change this remediation makes: it would be recorded here verbatim, excluded from the classification,
and never staged.

## Classified change set: paths absent from `PORCELAIN_BASELINE:`

| Porcelain status | Path (repo-relative) | Extension | Under `docs/` |
|---|---|---|---|
| ` M` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md` | `.md` | yes |
| ` M` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md` | `.md` | yes |
| ` M` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md` | `.md` | yes |
| ` M` | `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md` | `.md` | yes |
| `??` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-secondary-sanitisation.2026-09-03T12-23.md` | `.md` | yes |
| `??` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-squash-merge-note.2026-09-03T12-23.md` | `.md` | yes |
| `??` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-artifact-hygiene.2026-09-03T12-23.md` | `.md` | yes |
| `??` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-line5-baseline.2026-09-03T12-23.md` | `.md` | yes |
| `??` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-mergebase.2026-09-03T12-23.md` | `.md` | yes |
| `??` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-baseline.2026-09-03T12-23.md` | `.md` | yes |
| `??` | `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-helper-bootstrap.2026-09-03T12-23.md` | `.md` | yes |

Every listed path ends in `.md` and lies under `docs/`. No path is a `.ps1`, `.cs`, `.csproj`,
`.props`, or `.targets` file. No collapsed directory entry appears, confirming that `-uall` did its
work. The stop-and-report BLOCKED condition is therefore not triggered.

## HELPER_EXEMPTION:

The sweep helper at `coverage/r1-host-path-sweep.ps1` is a `.ps1` file and is not a counter-example
to the classification below. Two independent grounds are recorded, and the classification rests on
the second rather than on porcelain invisibility alone:

1. It is gitignored — `[P0-T2]` recorded `git check-ignore -q` exiting `0` against it — so it can
   never enter the change set, the staged index, or the branch diff.
2. It is a temporary throwaway script created and deleted within this agent session, which is the
   first named exception in the File Size Limit section of `.claude/rules/general-code-change.md`
   (lines 47-50, exception at line 50). `[P2-T5]` step 9 performs the deletion that makes that
   exemption true.

## Classification

`TOOLCHAIN: NOT APPLICABLE — no source file in any coverage-bearing language is modified, so the PowerShell (PoshQC format, PoshQC analyze, Pester with coverage) and C# (csharpier, msbuild analyzers, msbuild nullable, vstest) gates have an empty input set and the Coverage Evidence Contract is not triggered.`
