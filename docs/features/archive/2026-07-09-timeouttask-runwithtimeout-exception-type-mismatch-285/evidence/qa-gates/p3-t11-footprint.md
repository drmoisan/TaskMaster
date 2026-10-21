# P3-T11 — Change Footprint Verification

Timestamp: 2026-09-01T08-29

MERGE_BASE, read from `evidence/baseline/p0-t2-branch-and-merge-base.md`:
`2b85134b42872e405602e6064e02dc9cda6c319b`

EXIT_CODE: 0 (both invocations)

## Invocation 1 — `git diff --name-only 2b85134b42872e405602e6064e02dc9cda6c319b`

Output, verbatim:

```text
.claude/agent-memory/orchestrator/MEMORY.md
.claude/agent-memory/orchestrator/check-ignore-false-negative-on-directory-glob.md
.claude/agent-memory/orchestrator/feature-folder-order-hook-is-workmode-blind.md
UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs
UtilitiesCS/Threading/TimeOutTask.cs
docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/issue.md
docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/plan.2026-09-01T00-30.md
docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/research/2026-08-31T21-30-timeouttask-taskcanceled-retry-research.md
docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/spec.md
```

## Invocation 2 — `git status --porcelain`

Paired with the diff because a name-listing diff enumerates tracked changes only and cannot see the
untracked evidence files this plan creates.

Output, verbatim:

```text
 M UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs
 M UtilitiesCS/Threading/TimeOutTask.cs
 M docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/plan.2026-09-01T00-30.md
?? docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/evidence/
```

The `?? .../evidence/` entry is precisely the untracked directory the diff is blind to, which is why
the two observations are complementary and both are required.

## The Exclusion Set

**The exclusion set is exactly `.claude/agent-memory/` plus the P0-T6 unformatted-file list, and
nothing else.**

**Cardinality of the P0-T6 unformatted-file list: 0.** P0-T6 recorded the tree as already fully
formatted, with an empty unformatted-file list, so that source contributes no exclusion. The
exclusion set therefore reduces in practice to `.claude/agent-memory/` alone.

### Every excluded entry, enumerated by full path with its source

| # | Full path | Excluded by | Appeared in |
| --- | --- | --- | --- |
| 1 | `.claude/agent-memory/orchestrator/MEMORY.md` | `.claude/agent-memory/` | diff only |
| 2 | `.claude/agent-memory/orchestrator/check-ignore-false-negative-on-directory-glob.md` | `.claude/agent-memory/` | diff only |
| 3 | `.claude/agent-memory/orchestrator/feature-folder-order-hook-is-workmode-blind.md` | `.claude/agent-memory/` | diff only |

**Total excluded: 3, all from the `.claude/agent-memory/` source. Zero excluded from the P0-T6
unformatted-file list source, because that list is empty.**

These three files are tracked in git and were changed by commits already on this branch between the
merge base and HEAD; they do not appear in `git status --porcelain`, which confirms they are
committed rather than pending. They are agent memory, not part of this item's code change.

## Union Evaluation

The union of the two outputs, after applying the exclusion set above, is:

| Path | Classification |
| --- | --- |
| `UtilitiesCS/Threading/TimeOutTask.cs` | In-scope path 1 |
| `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` | In-scope path 2 |
| `docs/features/active/2026-07-09-.../issue.md` | Under in-scope path 3 |
| `docs/features/active/2026-07-09-.../plan.2026-09-01T00-30.md` | Under in-scope path 3 |
| `docs/features/active/2026-07-09-.../research/2026-08-31T21-30-timeouttask-taskcanceled-retry-research.md` | Under in-scope path 3 |
| `docs/features/active/2026-07-09-.../spec.md` | Under in-scope path 3 |
| `docs/features/active/2026-07-09-.../evidence/` | Under in-scope path 3 |

**Every remaining entry is one of the two named source files or a path under
`docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/`.**

**No other path appears. There is no `REMEDIATION-REQUIRED` entry for this task.**

Note on `.dotnet-sdk`: the repo-local SDK directory created by the P0-T3 bootstrap is matched by the
directory-only glob at `.gitignore` line 350 (`.dotnet*/`), so it appears in neither output and
required no exclusion-set entry. This was verified and recorded at P0-T3. Likewise `TestResults/`
and `coverage/` working output are ignored by `.gitignore` line 39 and lines 144-145 respectively.

Output Summary: The name-only diff against the merge base lists 9 paths and the porcelain status
lists 4. After excluding the 3 `.claude/agent-memory/` entries — the only exclusions, since the
P0-T6 list is empty with cardinality 0 — the union contains only `UtilitiesCS/Threading/TimeOutTask.cs`,
`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, and paths under the feature folder.

Acceptance: met. The union of the two outputs, after excluding entries under `.claude/agent-memory/`
and entries whose path appears in the (empty) P0-T6 unformatted-file list, contains only the two
in-scope source files and paths under the feature folder. Every excluded entry is enumerated by full
path with the source it came from, and the cardinality of the P0-T6 list is stated as 0.
