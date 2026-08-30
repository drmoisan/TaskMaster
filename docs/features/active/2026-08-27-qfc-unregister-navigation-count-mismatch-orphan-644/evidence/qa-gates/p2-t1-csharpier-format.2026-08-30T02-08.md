# [P2-T1] — CSharpier format (write mode)

- Timestamp: 2026-08-30T02-08
- Task: `[P2-T1]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Working directory: repository root of the branch worktree (recorded as a generic
  placeholder; no absolute host path is written to this artifact).
- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0

## Prerequisite

`dotnet tool restore` was run once for this worktree before the first CSharpier
invocation, as `.claude/rules/csharp.md` requires. It reported
`Tool 'csharpier' (version '1.2.6') was restored.` and `Restore was successful.`,
`EXIT_CODE: 0`. This confirms the manifest-pinned version 1.2.6 is the one that ran.

## Why the exit code alone is not the observation

`csharpier format` is a write-mode command: it exits 0 both when it rewrites files and
when it changes nothing. Three additional observations are therefore recorded.

## Observation 1 — the literal console line printed on this run

```
Formatted 1562 files in 2027ms.
```

## Observation 2 — repository-wide porcelain status, before and after

- Command: `git status --porcelain -- . ':!.claude/agent-memory'`
- EXIT_CODE: 0 (both invocations)

The porcelain scope is repository-wide, not scoped to `QuickFiler.Test`, because
`csharpier format .` is file-based and processes `*.cs`, `*.xml` and `packages.config`
across the whole tree; a rewrite outside the test project would be invisible to a
narrower observation. The `.claude/agent-memory` exclusion is the only exclusion. It
excludes no C# source and is required because that path carries unrelated
modifications authored by other agents that would otherwise make the comparison
non-deterministic.

Listing **before** the command:

```
 M QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/code-review.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t1-cr6-edit.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t2-cr2-edit.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t3-class-sweep-final.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t1-instructions-read.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t2-target-lines-baseline.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t3-class-sweep-baseline.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t4-invariance-baseline.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/feature-audit.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/policy-audit.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-inputs.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-plan.2026-08-30T02-08.md
```

Listing **after** the command: byte-for-byte identical to the listing above — the same
thirteen entries in the same order, one ` M` entry for the digits test file edited by
`[P1-T1]` and `[P1-T2]`, and twelve `??` entries for the cycle artifacts. The listings
are identical and not empty, which is the required outcome: the two Phase 1 edits and
the untracked cycle artifacts are present in both.

## Observation 3 — SHA-256 of the edited file, before and after

- Command: `(Get-FileHash -Algorithm SHA256 -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Hash`
- EXIT_CODE: 0 (both invocations)

| Point | SHA-256 |
|---|---|
| Before `csharpier format .` | `972BCD8F142E50099783C4F92BDA624639E13DFE5A2767ED4AC189E2679D3DAB` |
| After `csharpier format .` | `972BCD8F142E50099783C4F92BDA624639E13DFE5A2767ED4AC189E2679D3DAB` |

The two hashes are identical. This check is necessary in addition to the porcelain
comparison because a further rewrite of a file that `[P1-T1]`/`[P1-T2]` already left in
` M` status would not change that status letter, so the porcelain comparison alone
cannot detect it. The identical hash confirms CSharpier left the file byte-identical,
including the 100-column line 179 produced by `[P1-T1]`.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE:` | 0 | **0** | PASS |
| Before/after repository-wide porcelain listings identical, and not empty | identical, non-empty | **identical, 13 entries each** | PASS |
| Before/after SHA-256 of the digits test file identical | identical | **identical** | PASS |

No file was rewritten and neither hash differs, so no toolchain-loop restart is
required and execution proceeds to `[P2-T2]`.

## Output Summary

`dotnet tool run csharpier format .` exited 0 and printed
`Formatted 1562 files in 2027ms.`. The repository-wide porcelain listing is identical
before and after the run (13 entries: one ` M`, twelve `??`), and the SHA-256 of
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` is
identical before and after. CSharpier rewrote nothing.
