# Phase 0 — Instructions Read (P0-T1)

Timestamp: 2026-09-17T02-08

Policy Order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/csharp.md`
6. `.claude/rules/tonality.md`
7. `.claude/skills/atomic-plan-contract/SKILL.md`
8. `.claude/skills/acceptance-criteria-tracking/SKILL.md`
9. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
10. `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/spec.md`
11. `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/research/2026-09-16T23-50-breadcrumb-thread-affinity-tests-taskrun-distinct-thread-research.md`
12. `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/issue.md`

CHANNEL: NONE

## Files Read (repository-relative path and line count)

| # | Path | Lines |
| --- | --- | --- |
| 1 | `CLAUDE.md` | 463 |
| 2 | `.claude/rules/general-code-change.md` | 80 |
| 3 | `.claude/rules/general-unit-test.md` | 105 |
| 4 | `.claude/rules/quality-tiers.md` | 51 |
| 5 | `.claude/rules/csharp.md` | 96 |
| 6 | `.claude/rules/tonality.md` | 80 |
| 7 | `.claude/skills/atomic-plan-contract/SKILL.md` | 245 |
| 8 | `.claude/skills/acceptance-criteria-tracking/SKILL.md` | 104 |
| 9 | `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` | 175 |
| 10 | `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/spec.md` | 325 |
| 11 | `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/research/2026-09-16T23-50-breadcrumb-thread-affinity-tests-taskrun-distinct-thread-research.md` | 225 |
| 12 | `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/issue.md` | 63 |

Twelve files listed, each with a line count.

## Line-count derivation

Command: `git grep -c "" HEAD -- <the twelve paths>`

That form reports the exact newline-terminated line count for each tracked path in a single
invocation. It is used in preference to reading the final numbered row of a file viewer, which
renders one additional empty row for the terminating newline and therefore overcounts by one.

## Output Summary

All twelve files were read in the order listed above, in the assigned worktree, before any other
plan task ran. The reads are the authority for the constraints this execution applies:

- `CLAUDE.md` fixes the four-step C# toolchain order and the committed-test-evidence format, which
  prohibits committing a raw coverage-collector document or a raw test-platform document in any
  form, including under a feature folder's evidence tree. This plan keeps the Cobertura documents
  under `coverage/` and the TRX documents under `TestResults/`, both git-ignored, and commits only
  derived figures in artifacts.
- `.claude/rules/general-code-change.md` fixes the 500-line file-size limit that gates P2-T2 and
  P5-T7.
- `.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay` and real wall-clock waits
  in test code; the replacement helper uses an untimed `Join()`, a completion wait rather than a
  wall-clock wait, and the token census gates `Thread.Sleep`, `Task.Delay` and `[Timeout` at 0.
- `.claude/rules/csharp.md` fixes the formatter, analyzer and nullable command shapes, and lists
  "adding sleeps, retries, or timing hacks to mask flaky behavior" and "weakening assertions or
  relaxing test expectations" among prohibited behaviors.
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` fixes the canonical evidence scheme
  `<FEATURE>/evidence/<kind>/` and the `ExpectedExitCode:` semantics used by P0-T9, P0-T10, P3-T1,
  P3-T3 and P5-T5.
- `spec.md` is the sole acceptance-criteria source for this item: `issue.md` line 4 carries
  `- Work Mode: full-bug`, and `user-story.md` is absent.
