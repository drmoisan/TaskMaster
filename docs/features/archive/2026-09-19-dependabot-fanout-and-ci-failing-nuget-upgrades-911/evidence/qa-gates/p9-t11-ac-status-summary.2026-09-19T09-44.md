# P9-T11 — Acceptance criteria status summary

Timestamp: 2026-09-20T09-44

AC source: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`,
resolved from the `- Work Mode: full-bug` marker in `issue.md`, for which `spec.md` is the sole AC
source and `user-story.md` is absent by default.

## Summary

| | Count |
|---|---|
| Total AC items | **26** |
| Checked off, delivered and verified | **23** |
| Remaining, unchecked | **3** |

The three unchecked criteria are AC18, AC19 and AC20. Each names the P8-T5 follow-up issue,
**https://github.com/drmoisan/TaskMaster/issues/914**, in the row below.

## Every criterion, its discharging task and its evidence

Every path below is relative to
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/` and was
confirmed present on disk in a single existence sweep over all 26 rows; all 26 resolved.

| AC | State | Discharging task | Evidence artifact |
|---|---|---|---|
| AC1 — Dependabot configuration is consolidated | [x] | P3-T9 | `evidence/qa-gates/p3-t9-ac1-dependabot-consolidated.2026-09-19T09-44.md` |
| AC2 — Config manifests are outside the formatting gate, proven positively | [x] | P1-T3 | `evidence/qa-gates/p1-t3-ac2-format-scope-control.2026-09-19T09-44.md` |
| AC3 — All 18 manifests are normalised, and normalisation is idempotent | [x] | P1-T8 | `evidence/qa-gates/p1-t8-ac3-normaliser-idempotence.2026-09-19T09-44.md` |
| AC4 — The NuGet CLI version is pinned everywhere it is selected | [x] | P3-T10 | `evidence/qa-gates/p3-t10-ac4-nuget-pin.2026-09-19T09-44.md` |
| AC5 — Every analyzer item agrees with its manifest (#898) | [x] | P7-T4 | `evidence/qa-gates/p7-t4-ac5-analyzer-verifier.2026-09-19T09-44.md` |
| AC6 — The cold-cache failure is observed before the fix and absent after | [x] | P1-T14 | `evidence/qa-gates/p1-t14-ac6-cold-analyzer-build-green.2026-09-19T09-44.md` |
| AC7 — The incompatible framework is excluded, not ranked (#902) | [x] | P3-T6 | `evidence/qa-gates/p3-t6-ac7-framework-exclusion.2026-09-19T09-44.md` |
| AC8 — Orphaned hint paths are eliminated and detectable (#903) | [x] | P5-T17 | `evidence/qa-gates/p5-t17-ac8-orphan-hintpaths.2026-09-19T09-44.md` |
| AC9 — The compatibility gate is asset-level | [x] | P3-T3 | `evidence/qa-gates/p3-t3-ac9-asset-level-gate.2026-09-19T09-44.md` |
| AC10 — An incompatible package is skipped and the remaining upgrades proceed | [x] | P7-T3 | `evidence/qa-gates/p7-t3-ac10-skip-and-proceed.2026-09-19T09-44.md` |
| AC11 — Version reconciliation covers all four dependent element kinds (D1) | [x] | P5-T15 | `evidence/qa-gates/p5-t15-ac11-version-reconciliation.2026-09-19T09-44.md` |
| AC12 — Analyzer items are repaired by preserving the existing folder segment (D2) | [x] | P5-T13 | `evidence/qa-gates/p5-t13-ac12-analyzer-derivation.2026-09-19T09-44.md` |
| AC13 — Sibling elements in the analyzer item group survive regeneration | [x] | P5-T14 | `evidence/qa-gates/p5-t14-ac13-sibling-survival.2026-09-19T09-44.md` |
| AC14 — Binding redirects are reconciled to the resolved assembly version | [x] | P5-T16 | `evidence/qa-gates/p5-t16-ac14-binding-redirects.2026-09-19T09-44.md` |
| AC15 — The repair pass leaves a formatting-stable tree | [x] | P7-T5 | `evidence/qa-gates/p7-t5-ac15-repair-idempotence.2026-09-19T09-44.md` |
| AC16 — The verifier repairs freely and fails only on residual inconsistency | [x] | P5-T18 | `evidence/qa-gates/p5-t18-ac16-verifier-both-directions.2026-09-19T09-44.md` |
| AC17 — The repair workflow exists and is statically valid | [x] | P7-T7 | `evidence/qa-gates/p7-t7-ac17-workflow-static-validity.2026-09-19T09-44.md` |
| AC18 — The repair commit is pushed under the GitHub App identity | [ ] | P8-T2, deferred | `evidence/qa-gates/p8-t2-ac18-repair-identity.2026-09-19T09-44.md`; carried by issue **#914**, https://github.com/drmoisan/TaskMaster/issues/914 |
| AC19 — The required checks re-run and pass on the post-repair head SHA | [ ] | P8-T3, deferred | `evidence/qa-gates/p8-t3-ac19-required-checks.2026-09-19T09-44.md`; carried by issue **#914**, https://github.com/drmoisan/TaskMaster/issues/914 |
| AC20 — Disclosure is present and conditional | [ ] | P8-T4, deferred | `evidence/qa-gates/p8-t4-ac20-disclosure.2026-09-19T09-44.md`; carried by issue **#914**, https://github.com/drmoisan/TaskMaster/issues/914 |
| AC21 — The #908 three-way divergence is reproduced as a fixture and resolved | [x] | P5-T20 | `evidence/qa-gates/p5-t20-ac21-908-divergence-resolved.2026-09-19T09-44.md` |
| AC22 — The AC21 regression test is observed failing before the fix | [x] | P5-T21 | `evidence/regression-testing/p5-t21-ac22-fail-before-pass-after.2026-09-19T09-44.md` |
| AC23 — Reference completeness is asserted and demonstrably detectable | [x] | P5-T19 | `evidence/qa-gates/p5-t19-ac23-reference-completeness.2026-09-19T09-44.md` |
| AC24 — PowerShell toolchain and coverage | [x] | P9-T3 | `evidence/qa-gates/p9-t3-pester.iter1.2026-09-19T09-44.md` |
| AC25 — C# toolchain passes on the delivered tree | [x] | P9-T8 | `evidence/qa-gates/p9-t8-ac25-csharp-toolchain.2026-09-19T09-44.md` |
| AC26 — Documentation matches the delivered behaviour | [x] | P7-T9 | `evidence/qa-gates/p7-t9-ac26-documentation-pin.2026-09-19T09-44.md` |

## Why the three are unchecked

AC18, AC19 and AC20 each require a live GitHub App installation token and an open Dependabot pull
request as a fixture. P8-T1 measured both conditions and both failed independently, from successful
queries rather than forbidden ones:

- `CREDENTIAL-PRESENT: false` — the repository secrets query exited 0 and returned an empty name
  list, so no `DEPENDABOT_REPAIR_APP_ID` or `DEPENDABOT_REPAIR_APP_PRIVATE_KEY` exists.
- `DEPENDABOT-PR-COUNT: 0` — the open-pull-request query exited 0 and returned zero Dependabot pull
  requests.

The three criteria are therefore **unverifiable rather than failing**. They are left unchecked, and
each is carried by issue #914 together with the exact verification commands and the credential
provisioning runbook at
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`.

## Acceptance

| Clause | Required | Observed | Result |
|---|---|---|---|
| Criteria listed in the summary | exactly 26 | 26 | PASS |
| Every ticked criterion cites an artifact that exists on disk | yes | 23 of 23 resolved; all 26 rows resolved | PASS |
| Every unticked criterion names the P8-T5 follow-up issue | yes | AC18, AC19 and AC20 each name issue #914 with its URL | PASS |
| No criterion text reworded | yes | only the leading `- [ ]` to `- [x]` characters were changed in `spec.md` | PASS |

The only edits made to `spec.md` by this plan's Phase 9 were the two checkbox characters for AC24 and
AC25. No criterion text was altered, and no criterion was added or removed.
