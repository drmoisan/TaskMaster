# P2-T14 — Reduced-audit handoff packet

Timestamp: 2026-09-02T00-38

## 1. Both check-off roles, so neither task is the sole owner

The `acceptance-criteria-tracking` skill assigns two distinct check-off roles, and both are stated
here so responsibility is not left ambiguous:

**The executor's role.** The executor checks off each criterion, **one criterion per edit**, as that
criterion's supporting evidence artifact verifies during execution. That is the state P2-T13 records.
Concretely, this executor checked off 22 of the 23 criteria and left AC20 unchecked; the only edit
made to the `## Acceptance Criteria` section of `issue.md` is the checkbox transition `- [ ]` to
`- [x]`, proved by a byte comparison in `evidence/issue-updates/ac-verdicts.md`.

**The reduced audit's role.** The reduced audit then **verifies those check-offs against the
evidence** rather than trusting them, and checks off any remaining criterion it evaluates as PASS.
Every criterion it evaluates as PARTIAL, FAIL or UNVERIFIED is left unchecked with the reason
recorded in the audit artifact. If the audit disagrees with an executor check-off, the audit's
verdict governs and the checkbox is reverted with the reason recorded.

The audit's specific attention is drawn to **AC20**, which the executor left unchecked. It is the one
criterion where the audit must reach its own verdict rather than confirm the executor's: three of its
four clauses hold and the fourth fails for two COM-bound members. The full argument, including what
was done to reduce the shortfall and why it could not be closed, is in
`evidence/qa-gates/coverage-delta.md` and summarised in `evidence/issue-updates/ac-verdicts.md`.

## 2. Every evidence artifact produced by Phase 0 and Phase 2, by path

All paths are relative to
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`.

### Phase 0 — baseline (13 artifacts)

- `evidence/baseline/phase0-instructions-read.md`
- `evidence/baseline/minor-audit-integrity.md`
- `evidence/baseline/base-ref-anchor.md`
- `evidence/baseline/dotnet-tool-restore.md`
- `evidence/baseline/csharpier-check.md`
- `evidence/baseline/analyzer-build.md`
- `evidence/baseline/nullable-build.md`
- `evidence/baseline/mstest-coverage-run.md`
- `evidence/baseline/coverage-baseline.md`
- `evidence/baseline/coverage-baseline.jacoco.xml`
- `evidence/baseline/coverage-per-file-baseline.md`
- `evidence/baseline/file-size-census.md`
- `evidence/baseline/carrier-construction-sites.md`

### Phase 2 — QA gates and issue updates (12 artifacts)

- `evidence/qa-gates/csharpier-format.md`
- `evidence/qa-gates/csharpier-check.md`
- `evidence/qa-gates/analyzer-build.md`
- `evidence/qa-gates/nullable-build.md`
- `evidence/qa-gates/mstest-coverage-run.md`
- `evidence/qa-gates/coverage-post-change.md`
- `evidence/qa-gates/coverage-delta.md`
- `evidence/qa-gates/exclude-attribute-invariant.md`
- `evidence/qa-gates/coverage-post-change.jacoco.xml`
- `evidence/qa-gates/file-size-audit.md`
- `evidence/qa-gates/scope-confinement.md`
- `evidence/qa-gates/final-toolchain-pass.md`
- `evidence/issue-updates/ac-verdicts.md`

### Phase 1 — produced by the implementation block, listed for completeness (9 artifacts)

- `evidence/other/implementation-handoff.md`
- `evidence/other/compile-seam.md`
- `evidence/regression-testing/ac16-red.md`
- `evidence/other/carrier-chain.md`
- `evidence/other/leg-a.md`
- `evidence/other/leg-b.md`
- `evidence/regression-testing/ac16-green.md`
- `evidence/regression-testing/ac9-negative-guard.md`
- `evidence/regression-testing/ac12-path-normalisation.md`
- `evidence/other/change-description.md`
- `evidence/other/out-of-scope-register.md`

Plus this packet, `evidence/other/reduced-audit-handoff.md`, and the terminal
`evidence/qa-gates/final-commit.md` written by P2-T15.

## 3. The P1-T12 out-of-scope register and its referral records

The full register is `evidence/other/out-of-scope-register.md`. Its verdicts, carried here so the
audit does not have to reconstruct them:

| # | Out-of-scope item | Verdict | Reachability |
|---:|---|---|---|
| 1 | Synchronous `QfcItemController.LoadFolderHandler` predictor-initialisation defect (`QfcItemController.FolderHandling.cs:27-55`) | **CONFIRMED-DEFECT** | **LIVE** — `PopulateFolderComboBox` is reachable from production UI code |
| 2 | De-exempting any `[ExcludeFromCodeCoverage]` class | **NOT-CONFIRMED** | n/a |
| 3 | Splitting oversized files | **CONFIRMED-DEFECT (pre-existing)** | **LATENT** — maintainability only |
| 4 | Adding `InitAsync` to `IFolderSearchHandler` | **NOT-CONFIRMED** | n/a |
| 5 | Deleting the dormant post-display filter | **CONFIRMED-DEFECT (dead code)** | **LATENT** — dormant by construction |
| 6 | Consolidating the duplicated `MailItemHelper.FromMailItemAsync` calls | **CONFIRMED-DEFECT** | **LIVE** — both call sites execute on the high-confidence path |

**Referral record, identical for all four confirmed items:**

```
Deferred to a single consolidated follow-up issue filed by the parallel orchestrator from a separate
branch after this PR merges.
```

That route names the owner (the parallel orchestrator), the branch condition (a separate branch) and
the timing (after this PR merges). **No promotion MCP tool was run, no potential entry was created,
and no GitHub issue was opened from this branch**, because doing so would put an out-of-scope artifact
into this change's footprint and break AC23.

## 4. The minor-audit fail-closed conditions

The reduced audit **fails closed** if any of the following holds:

1. **`spec.md` or `user-story.md` has appeared** in the feature folder. Neither existed at Phase 0;
   `evidence/baseline/minor-audit-integrity.md` records the search scope, patterns and a `none`
   result. For `minor-audit`, their presence is an integrity failure, not an enrichment.
2. **The explicit `## Acceptance Criteria` section is missing from `issue.md`.** It was present at
   Phase 0 with all 23 identifiers occurring exactly once each. No other checkbox section of
   `issue.md` may be treated as acceptance criteria.
3. **Any required artifact is absent**, or an artifact omits any of `Timestamp:`, `Command:`,
   `EXIT_CODE:` or `Output Summary:` where the plan requires them.
4. **Plan checklist state contradicts evidence on disk** — a task marked `[x]` whose artifact is
   missing, or whose acceptance conditions the artifact does not in fact establish.

A fourth-condition check the audit should make deliberately: **P2-T7 is checked off although AC20 is
not satisfied.** That is not a contradiction. P2-T7's acceptance conditions require the figures to be
*recorded*, including a pass-or-fail verdict per member; it does not require every member to pass.
The task is complete and the criterion is not, and both states are recorded.

## 5. The two artifacts recording the AC12 normalisation decision and the AC15 accepted delta

- **AC12 normalisation decision:** `evidence/other/change-description.md`, section "The AC12
  normalisation decision, and which side was normalised". It records that the **consumer** side was
  normalised, that the projection is duplicated in QuickFiler rather than reused from
  `FolderPredictor.ProjectSuggestionPath` because AC23 forbids modifying `UtilitiesCS/`, that the
  projection is the identity when the archive root is null or empty so pre-change selection behaviour
  is preserved, and why the producer side was rejected. The fail-before and pass-after evidence is in
  `evidence/regression-testing/ac12-path-normalisation.md`.
- **AC15 accepted behavioural delta:** `evidence/other/change-description.md`, section "The AC15
  accepted behavioural delta". It records that reusing the scan-time suggestion set freezes
  conversation-derived `CtfMap` suggestions at scan time rather than re-deriving them at display
  time, for both legs; that the scan-to-display interval is longer for leg B and unbounded; and that
  Bayesian suggestions and the recents list are unaffected because the folder array is still built
  lazily at display time.

## 6. Known limitations the audit should not have to rediscover

- **P1-T1 delegation was unavailable.** No Agent or delegation tool exists in this session, so the
  handoff packet was written in full and the executor performed the implementation directly. Recorded
  in `evidence/other/implementation-handoff.md`.
- **Six plan citations are stale or mis-scoped**, and two enumerations are incomplete. Each is
  recorded in the artifact of the task that hit it, with the cited and the true location. The
  executor acted on the true location and did not edit the plan.
- **The baseline per-line coverage map was not retained**, so the per-member baseline for the two
  relocated `QfcQueue` members cannot be read directly. The no-regression claim for them rests on two
  independent arguments given in `evidence/qa-gates/coverage-delta.md`.
- **The coverage suite hung twice** on the known load-flaky `WinFormsPumpHost` cluster, once at
  P0-T8 and once at P2-T5. Both hangs were diagnosed by CPU sampling, both were followed by exactly
  one re-run of the byte-identical command with no intervening file change, and both runs are
  recorded in the respective artifact. The suite passed 6938/6938 at baseline and 6946/6946
  post-change.
