# AC14 Amendment Verification, Read-Only — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-28-36
- Task: [P0-T4]
- Finding: R7, decision D2
- EXIT_CODE: 0
- `AC14-AMENDMENT: present`

This task is **read-only**. Per **gate rule 19** the executor does not edit criterion text. The
coordinator applied and committed the amendment before execution began; this artifact verifies it.

## The AC14 Bullet, Verbatim

Read from `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`,
lines 473 through 483:

```
- [x] **AC14 - Binding redirects are reconciled to the resolved assembly version.**
      `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` asserts that for an `app.config`
      fixture whose redirect names an older assembly version than the one resolved from the manifest,
      the reconciled text names the resolved version in both the upper bound of `oldVersion` and in
      `newVersion`; and that an `app.config` with no redirect for that assembly is returned
      unchanged. Evidence: Pester output under evidence/qa.
      The class is exercised by unit assertion only and is not reachable from the `workflow_run`
      trigger: the workflow invokes the repair entry point with no `-CandidateUpgrade`, so the
      applied-upgrade set is always empty and the `app.config` reconciliation pass does not execute
      in the configured trigger path. Making it reachable is out of scope for issue #911 and is
      recorded in the code review dated 2026-09-20.
```

The appended sentences sit after the existing `Evidence: Pester output under evidence/qa.` sentence
and match the verbatim block in the `Gate-Quality Rules Added by This Cycle` section of
`remediation-plan.2026-09-20T01-37.md` word for word.

## Fragment Checks

| Fragment | Occurrences | Verdict |
|---|---|---|
| `is not reachable from the` | 1 | present |
| ``no `-CandidateUpgrade` `` — as the amendment spells it, with the code-span backticks | 1 | present |
| `no -CandidateUpgrade` — the same words with no backticks | 0 | absent |

**Reading recorded so a later reader is not misled.** The plan's acceptance clause quotes the second
fragment as `no -CandidateUpgrade` inside a markdown code span. Markdown cannot nest a backtick
inside a single-backtick span, so the clause could not reproduce the two backticks the amendment
actually carries. The authoritative text is the plan's own verbatim amendment block, which spells it
`with no \`-CandidateUpgrade\`,` and which the committed `spec.md` reproduces exactly. The
bare-words spelling has 0 occurrences and is recorded here as the reason the two counts differ. The
substance of the acceptance — that the amendment names the absent `-CandidateUpgrade` argument as
the reason the class is unreachable — holds.

## Criterion Ledger

```
Select-String -Path <spec.md> -Pattern '^- \[[ xX]\] \*\*AC\d+ '
```

| Measurement | Value | Required |
|---|---|---|
| Total criteria matching `^- \[[ xX]\] \*\*AC\d+ ` | **26** | exactly 26 |
| Ticked `[x]` | 23 | — |
| Unticked `[ ]` | 3 | — |
| AC14 state | `[x]`, ticked | remains ticked |

The three unticked criteria are AC18, AC19 and AC20, which are carried by issue #914. The count is
unchanged by the amendment: it appended prose to an existing bullet and added no criterion.

## Provenance

```
git log -1 --format=%H -- docs/features/active/2026-09-19-.../spec.md
```

`ffd53955ba67ccd25922df9c8b3e73afdea087a6`

Forty hexadecimal characters, non-empty. This value is empty for a never-committed path, which is
what makes the check falsifiable: an amendment applied to the working tree but not committed would
have produced the commit that last touched `spec.md` before the amendment, and an uncommitted file
would have produced nothing.

## Output Summary

`AC14-AMENDMENT: present`. The committed `spec.md` carries the amendment verbatim as the plan
specified it. Criterion count is exactly 26, AC14 remains ticked, 23 ticked and 3 unticked. The
last commit touching `spec.md` is a non-empty 40-character hash. No edit was made by this task.
