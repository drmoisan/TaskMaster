# Feature Audit — itemviewer-surface-defects (Issue #489) — remediation cycle 1 exit-gate reaudit

- Timestamp: 2026-08-28T04-35 (UTC)
- Branch: `bug/itemviewer-surface-defects-489` at `923cd3ce`; base `epic/quickfiler-bug-family-integration` (merge base `69e83171`)
- Work mode: `full-bug` (from `issue.md`) — the acceptance-criteria source is `spec.md` only.
- Prior cycle: `feature-audit.2026-08-28T03-13.md` evaluated all 62 criteria PASS at `74d02ad2`; the NO-GO came from the policy finding RC-1, which sat outside the AC set because AC11 bound only the handoff record.

## Verification method

The prior cycle's 62 PASS verdicts are carried forward and re-validated against the remediation delta `d77ac212..923cd3ce`: for each criterion the question asked was whether anything in the delta could invalidate it. The delta consists of one production line, one test, the spec amendment, the handoff addendum, plan checkbox flips, and evidence — so the only criteria requiring fresh evaluation were those touching `EventWiring.cs`, the `QfcItemController_EventWiringTests` class, the handoff record, and the toolchain/coverage gates. Each of those was re-verified directly; the rest were spot-checked as unaffected.

## The spec amendment (2026-08-28, remediation cycle 1) — audited and accepted

- **Criterion count:** re-counted at HEAD — 62 `- [x]`, 0 `- [ ]`. Unchanged at 62.
- **No criterion weakened:** the diff modifies exactly one checkbox line — the issue #486 handoff criterion — and only by adding a further requirement (the dated addendum with `ObligationDischargedInBranch: true`). The original requirement text is retained verbatim inside the criterion. This is a strengthening.
- **Disposition-table amendment:** § Sibling-collision resolution row 1 now covers the wire **and** the single matching detachment. This keeps the scope-discipline criterion (spec.md:874, "each diff is confined to the members named in § Sibling-collision resolution") true by reference: the full-branch `EventWiring.cs` diff is exactly two lines, one in `WireIntentEvents` and one in `UnwireIntentEvents`, both now named.
- **Superseded risk row:** retained in place as history and marked superseded; honest and consistent with the repository's dated-amendment precedent (spec.md:751, accepted in the prior cycle).
- **Precedent:** identical mechanism to the 2026-08-27 in-flight amendment accepted in the prior audit — dated, in place, original wording quoted, reason recorded.

## Criteria re-verified this cycle (all PASS)

| Criterion | Re-verification |
|---|---|
| Issue #486 handoff criterion (amended, spec.md:833) | Handoff record states the 16 -> 17 count change and names 484 (original halves, unchanged); the dated addendum exists with `Timestamp: 2026-08-28T03-52` and exactly one occurrence of `ObligationDischargedInBranch: true` (re-grepped: 1, at line 124); recorded in `evidence/other/`. PASS on both the original and the added requirement. |
| Scope-discipline criterion (spec.md:874) | `EventWiring.cs` full-branch diff is +2/-0, confined to the two members named in the amended disposition row. Other sibling-owned production diffs untouched by the remediation. PASS. |
| `WireIntentEvents_SubscribesToPicturesChanged`, `PicturesChanged_WhenRaised_RefreshesOptionsPictures` | Both in the full-suite committed TRX as Passed (1122/0/0); the class gained one test without disturbing them. PASS. |
| vstest gate criterion (spec.md:890) | Failed 0 (not greater than baseline 0), skipped 0 (equal), passed 1122 (not less than 1121); zero failures in the two feature-created classes; per-class failed counts all 0. PASS. |
| Toolchain criteria (csharpier, analyzer, nullable) | Re-verified from committed rem1 evidence and by this reviewer's independent rechecks (format exit 0 on the changed files; both rebuild logs 5 warnings / 0 errors, 0 CoreCompile skips, 0 CS86xx). PASS. |
| Coverage criteria | Post-processed shape: line rate 0.851617 at lines-valid 63902 (baseline 0.851567 at 63901, +1 exactly); added line 481 covered (`hits="1"`); re-parsed independently from the runner's canonical output document. PASS. |
| File-size criteria | `EventWiring.cs` 484/500, `EventWiringTests.Part2.cs` 105/500, re-measured; `EventWiringTests.cs` untouched at 499. PASS. |

No criterion transitions from PASS to any other state as a result of the remediation. No criterion was checked off or altered by this reviewer.

## Verdicts on the four disclosed deviations

1. **P3-T1 gate failed first (grep line-count 2 vs 1):** the fix was to stop restating the token in prose, not to weaken the gate; the failed attempt is retained in the artifact. Accepted; demonstrates the gate is falsifiable.
2. **Backslash-eaten coverage argument:** caught from the runner's echoed path, run killed, stray root file deleted before any recorded run; no stray file exists now. Accepted as a disclosed operational false start.
3. **P4-T7 two runs:** the detached first run had no observable exit code, which the acceptance clause requires; the foreground re-run is the judged run, both are recorded, both clear the floor, jitter allowance unconsumed. Accepted.
4. **P4-T11 eight commits vs "a single commit":** the sole literal plan-text miss. The intent — all four required paths committed and traceable, nothing outside scope — is met and was independently verified path by path and commit by commit. The caller's standing per-phase-commit instruction took precedence and the divergence is disclosed inside the artifact rather than hidden; the per-phase series additionally made the RED-before-fix ordering provable from the history. Accepted; recommend future remediation plans encode the caller's commit-granularity convention up front.

Additionally judged: the **deliberate non-rename** of `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions`. Acceptable (recorded as Info finding RCV-1 in the code review): every assertion the test makes remains true, it passes unmodified at HEAD in this reviewer's live rerun, the staleness is documented in two places, and renaming a merged sibling's stable node ID has no behavioural gain.

## Acceptance Criteria Status

- Source: `docs/features/active/itemviewer-surface-defects-489/spec.md` (work mode `full-bug`)
- Total AC items: 62
- Checked off (delivered): 62
- Remaining (unchecked): 0
- Items remaining: none

## Overall

**GO.** RC-1, the single Blocking finding that produced the prior NO-GO, is cured and verified from source, commit history, runtime, and coverage evidence. All 62 acceptance criteria hold at HEAD, the amended criterion is strictly stronger, and the remediation introduced no new finding above Info level. Remediation cycle 1 closes with `blocking_count` 0. Outstanding non-blocking residuals (promotions O1–O8, E1–E4, reframed O3, #489 D4 residual, #490 D5, deferred #490 D1 clear-insertion; the 500/500 test file; PA-1 threshold-wording conflict) transfer to the fan-in/post-merge obligations recorded in the prior cycle and remain owed there.
