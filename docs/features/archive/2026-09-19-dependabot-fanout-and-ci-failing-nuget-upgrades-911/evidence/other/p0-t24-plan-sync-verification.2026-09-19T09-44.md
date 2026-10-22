# P0-T24 — Plan-File Identity Modulo Check-Off State

Timestamp: 2026-09-19T23-17

Commands:

```
[System.IO.File]::ReadAllLines(<path>)
  normalised by [regex]::Replace($line, "^- \[[ xX]\] \[(P\d+-T\d+)\]", "- [ ] [$1]")
Get-FileHash -Algorithm SHA256   (over the raw file, and over the normalised text)
git log -1 --format=%H -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
```

EXIT_CODE: 0

Both copies were read: the execution-worktree copy at the repository-relative path above inside the
execution worktree, and the session copy at the same repository-relative path inside the session
worktree `TaskMaster-wt\2026-09-12T10-15`.

## Hashes

| Copy | Raw SHA-256 | Normalised SHA-256 |
|---|---|---|
| Execution worktree | `C59AFA3A15B30D2C0BEE874507222990A24F8C7FDF2CFE06CE7566E10D7C55AA` | `BE9F32C69307644F518F9C97A2DF17513FEBCBB84E84BF4399D6AA4A0FA0D827` |
| Session worktree | `844EECD7E5B36BE6764B6E68D4592FA46B41A9440496649523AA792127828F59` | `BE9F32C69307644F518F9C97A2DF17513FEBCBB84E84BF4399D6AA4A0FA0D827` |

**The two normalised hashes are equal.** The plan text itself has not diverged between the copies.

**The two raw hashes differ, and are recorded but not compared.** That difference is expected and is
the intended state: 23 tasks are ticked in the execution copy and the session copy is left entirely
unticked for the whole run.

## Counts, execution-worktree copy

| Measurement | Value |
|---|---|
| Lines matching `^- \[[ xX]\] \[P\d+-T\d+\]` (either mark) | **128** |
| `**Task Count:**` figure in the plan header | **128** |
| Lines beginning `### Phase ` | **10** |
| Lines matching `^- \[[xX]\] \[P\d+-T\d+\]` (ticked only) | **23** |

The session copy reports the same 128 either-mark lines and the same 10 phase headings, and **0**
ticked.

### Why the two patterns are both required

The either-mark pattern `^- \[[ xX]\] \[P\d+-T\d+\]` matches ticked and unticked lines alike. It
therefore measures the plan's structure and is completely blind to a destroyed tick set: it would
read 128 whether 23 tasks were ticked or none were. The separate ticked-only pattern
`^- \[[xX]\] \[P\d+-T\d+\]` is what makes a destroyed tick set detectable, and its expected value at
this point is an assertion rather than an observation.

## Ticked-task assertion

**Required: exactly 23, being P0-T1 through P0-T23. Measured: 23.** PASS.

Recorded as an assertion deliberately. An observation would have recorded `0` after a sync destroyed
the tick set and the run would have continued to P9-T15 before anything noticed, which is 103 tasks
later.

## Plan-file commit value

```
git log -1 --format=%H -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
```

returned:

```
1ed87d668bd9680c03de16629a674cee48b54719
```

Written verbatim as measured. **No literal is asserted against it.** The whole of the commit
assertion is that the value is a non-empty 40-character hexadecimal string, and it is: length 40,
every character in `[0-9a-f]`. The reachable failure is the empty return —
`git log -1 --format=%H -- <path>` prints nothing for a path that is untracked or has never been
committed — so the assertion establishes that the plan file is tracked and committed, and fails when
it is not.

Pinning a literal here is prohibited: the coordinator re-commits the plan at every revision, so each
revision invalidates the previous revision's literal. That defect was reported and fixed once at
round 2 and re-introduced twice by revisions 8 and 9.

**An ancestor check is deliberately not asserted.** `git log -1 --format=%H -- <path>` walks HEAD's
own history restricted to that path, so every value it can return is reachable from HEAD by
construction and `git merge-base --is-ancestor <that value> HEAD` returns 0 unconditionally. That is
a constant-valued condition of the kind removed from P0-T3, arriving from the opposite direction.
The content guarantee does not rest on the commit value at all; it rests on the normalised-hash
equality above.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Normalised hashes equal | required | both `BE9F32C6…` | PASS |
| Either-mark line count, execution copy | 128 | 128 | PASS |
| Equals the `**Task Count:**` header figure | required | 128 = 128 | PASS |
| `### Phase ` line count | 10 | 10 | PASS |
| Commit value | non-empty 40-character hexadecimal, no literal asserted | `1ed87d668bd9680c03de16629a674cee48b54719`, 40 hex characters | PASS |
| Ticked-task count, execution copy | exactly 23 | 23 | PASS |
| Raw hashes | recorded, not compared, expected to differ | recorded, differ | PASS |

No normalised mismatch was found, so the stop-and-report path is not taken and neither copy is
overwritten.

Output Summary: the execution-worktree and session-worktree copies of the plan are identical once
every `^- \[[ xX]\] \[P\d+-T\d+\]` line is normalised to the unticked form — both normalise to
SHA-256 `BE9F32C69307644F518F9C97A2DF17513FEBCBB84E84BF4399D6AA4A0FA0D827`. Their raw hashes differ
as expected, because the execution copy carries **23** ticks (P0-T1 through P0-T23, the asserted
value) and the session copy carries 0. The execution copy contains **128** task lines, equal to the
`**Task Count:** 128` header figure, and **10** `### Phase ` headings. The plan file's last-touching
commit is `1ed87d668bd9680c03de16629a674cee48b54719`, a non-empty 40-character hexadecimal string,
recorded as measured with no literal asserted against it.
