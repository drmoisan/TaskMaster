# Acceptance-Criteria Check-Off Register — Issue #553

- Timestamp: 2026-08-14T11-14 (local) / 2026-08-14T15:14:22Z (UTC)
- Tasks: [P5-T6] through [P5-T13]
- Work Mode: `full-feature` → authoritative AC sources are `spec.md` and
  `user-story.md`; `issue.md` is tracked as a third mirror because the plan's
  check-off tasks name it.

## Method note — why evidence paths are recorded here, not in the AC text

Each [P5-T6]…[P5-T13] task says the checkboxes should cite their evidence paths.
`.claude/skills/acceptance-criteria-tracking/SKILL.md` rule 3 is explicit that
check-off must change only `- [ ]` to `- [x]` and must **not** modify criterion
text, and rule 5 forbids adding content to AC source files. Those rules govern
the AC files. This register is therefore the citation surface: the AC files carry
the checkbox state, and this artifact carries the evidence pointer for each one.
No criterion text was altered in any of the three files.

## Check-offs applied by this plan's Phase 5

| Plan task | spec.md | user-story.md | issue.md | Evidence |
| --- | --- | --- | --- | --- |
| [P5-T6] | AC 1 | AC 1 | AC 1 | `.github/workflows/ci.yml` ([P2-T1] verification 3: zero `needs:` matches; verification 4: five `uses:` references) + `evidence/qa-gates/post-probe-green-run.2026-08-14T11-10.md` (five independent jobs, all started within 1s of each other) |
| [P5-T7] | AC 2 | AC 2 | AC 2 | The five `Test-CalleeContract` results in [P1-T1]–[P1-T5] (each asserts `workflow_call`, `workflow_dispatch`, own `permissions:`, right-sized `timeout-minutes`, no `concurrency:`, no `needs:`) + `evidence/qa-gates/actionlint-final.2026-08-14T11-14.md` |
| [P5-T8] | AC 3 | AC 3 | AC 3 | [P2-T1] verification outputs: `steps:` match count 0, `needs:` match count 0, callee `uses:` count 5, header byte-identical at offset 0 |
| [P5-T9] | AC 4 | AC 4 | AC 4 | `evidence/qa-gates/byte-identity.2026-08-14T09-54.md` (block `upload-step`, SHA-256 `894b0ce75a70c838…`) + `evidence/qa-gates/test-results-artifact.2026-08-14T11-10.md` (single `test-results` artifact, 8,246,282 bytes) |
| [P5-T10] | AC 5 | — | — | `evidence/qa-gates/byte-identity.2026-08-14T09-54.md` (6/6 blocks, matching SHA-256 digests, 12/12 critical fragments) + `evidence/qa-gates/lastexitcode-review.2026-08-14T11-14.md` |
| [P5-T11] | AC 7 | AC 6 | AC 6 | `.github/workflows/README.md` ([P2-T2] heading verification; both required headings present at L52 and L101) |
| [P5-T12] | AC 8 | AC 7 | AC 7 | `evidence/qa-gates/post-probe-green-run.2026-08-14T11-10.md` — run 31812508684 on head `ad28ea81`, all five jobs `success`. Superseded by [P5-T15]'s final-head confirmation if further commits land. |
| [P5-T13] | AC 10 | — | — | `evidence/qa-gates/ci-split-timing-comparison.2026-08-14T11-10.md` + sibling `post-split-timing.provenance.json` |

`spec.md` AC 5 and AC 10 have no issue/user-story mirror; those documents' AC 8
equivalents are handled by [P7-T3].

## Check-offs already applied by `feature-review`

The reviewer (artifacts dated 2026-08-14T10-21) had already checked spec ACs
1, 2, 3, 4, 5, 7 and user-story ACs 1, 2, 3, 4, 6 as PASS in its feature-audit,
which the AC-tracking skill authorises reviewers to do. Phase 5 verified those
states rather than re-applying them, and supplied the `issue.md` mirrors, which
the reviewer had not touched. The evidence table above is the citation for all of
them regardless of who set the checkbox.

## Deliberately still unchecked

| Criterion | Files | Why | Cleared by |
| --- | --- | --- | --- |
| Ruleset `required_status_checks` replaced in one atomic PUT | spec AC 6, user-story AC 5, issue AC 5 | The PUT has not been performed. It is orchestrator-gated ([P6-T3]) and requires explicit user confirmation that has not been given. | [P7-T2] after Phase 6 |
| Every gate still enforced; no check dropped, weakened, or made non-required | spec AC 9, user-story AC 8, issue AC 8 | The "still required" half of this criterion depends on the post-PUT ruleset state. The "not weakened" half is already evidenced by the byte-identity artifact and the three fault-isolation probes, but the criterion is not fully satisfied until the five contexts are actually required on `main`. | [P7-T3] after Phase 6 |

Both are correctly blocked on Phase 6, which is outside this execution segment.

## Current tally

| Source | Total | Checked | Remaining |
| --- | --- | --- | --- |
| `spec.md` | 10 | 8 | 2 (AC 6, AC 9) |
| `user-story.md` | 8 | 6 | 2 (AC 5, AC 8) |
| `issue.md` | 8 | 6 | 2 (AC 5, AC 8) |

Spec seeded test conditions: 8 total, 7 checked (1, 3, 4, 5, 6, 7, 8). Condition 2
("Each `_<name>.yml` is independently dispatchable via `workflow_dispatch` and
succeeds standalone") remains unchecked; it is [P7-T1]'s post-merge dispatch
smoke. Note that partial evidence already exists: every run in Phases 3–5 was
started via `gh workflow run ci.yml`, proving `workflow_dispatch` works on the
orchestrator, but the criterion is about each **callee** dispatched standalone,
which has not been exercised.

Spec Definition of Done: 5 items, 0 checked — all are [P7-T4]…[P7-T8], outside
this segment.
