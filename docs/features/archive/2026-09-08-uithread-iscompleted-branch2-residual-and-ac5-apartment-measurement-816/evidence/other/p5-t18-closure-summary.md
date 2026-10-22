# P5-T18 — Closure summary for issue #816

Timestamp: 2026-09-13T23-52

## The fourteen acceptance criteria

Source: the specification document in this feature folder, section `## Acceptance Criteria`.

| AC | Subject | State | Artifact(s) cited |
|---|---|---|---|
| AC1 | The captured-UI-context exit is hardened and nothing else in the accessor changes | **checked** | `evidence/other/p2-t7-ac01-diff-confinement.md`, `evidence/qa-gates/p4-t15-ac01-postformat-confinement.md` |
| AC2 | The recycled-id negative regression test is red before and green after | **checked** | `evidence/regression-testing/p1-t9-ac02-ac03-fail-before.md`, `evidence/regression-testing/p2-t6-pass-after-projection.md` |
| AC3 | The null-dispatcher fail-closed negative test passes | **checked** | `evidence/regression-testing/p2-t6-pass-after-projection.md` |
| AC4 | The positive twin passes both before and after | **checked** | `evidence/regression-testing/p1-t8-fail-before-run.md`, `evidence/regression-testing/p2-t5-pass-after-run.md` |
| AC5 | The weak retry assertion is tightened and discriminates | **checked** | `evidence/regression-testing/p2-t6-pass-after-projection.md` |
| AC6 | The runtime apartment measurement is taken | **checked** | `evidence/other/ac05-mta-initialize-measurement.md` |
| AC7 | The measurement leaves nothing behind and needs no host | **checked** | `evidence/other/p4-t14-ac07-measurement-residue.md` |
| AC8 | Both issue #782 findings are recorded, and neither changes production | **checked** | `evidence/other/p3-t3-issue782-findings.md`, `evidence/other/p2-t7-ac01-diff-confinement.md` |
| AC9 | Clause (ii) of issue #809's AC5 is discharged | **checked** | `evidence/regression-testing/p4-t9-ac09-repetitions.md` |
| AC10 | Coverage of the hardened exit and the repository floor | **checked** | `evidence/qa-gates/p4-t12-ac10-coverage.md` |
| AC11 | Issue #809's AC5 is checked off only on a complete discharge | **checked** | the P5-T1 task outcome, `evidence/other/p3-t2` mirror at the issue 809 feature folder, `evidence/other/p3-t4-mirror-parity.md` |
| AC12 | The new file is registered and its tests actually ran | **checked** | the P1-T4 acceptance, `evidence/regression-testing/p2-t5-pass-after-run.md` |
| AC13 | File-size limit respected | **checked** | `evidence/qa-gates/p4-t13-ac13-file-sizes.md` |
| AC14 | Full four-step toolchain passes in one final pass | **checked** | `evidence/qa-gates/p4-t10-ac14-toolchain.md` |

All fourteen are checked. None is left unchecked.

## Disposition of issue #809's AC5

**Checked.** Both clauses were discharged, so the strict condition in AC11 was met and the checkbox
at line 464 of the issue 809 specification was changed from the unchecked to the checked form, with
no other character of that line modified.

- Clause (i), the measurement: the guard value read on the executing thread was **MTA** and the
  settling line was **`MTA_INITIALIZE_OUTCOME: COMPLETED`**. Recorded in
  `evidence/other/ac05-mta-initialize-measurement.md` under this feature folder and mirrored, byte
  for byte, into the issue 809 feature folder's own `evidence/other/` directory, which is the
  location that criterion names. The two copies were verified identical by an anchored no-index diff
  that printed zero output lines and by equal SHA-256 hashes.
- Clause (ii), the three-repetition record: three repetitions of the full two-assembly run were
  recorded, each with exit code 0, and
  `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`
  was recorded **Passed** in every one. No failure of that test occurred, so no attribution was
  required.

No partial discharge was committed and no outstanding-clause note was needed.

## The two residuals carried forward and explicitly NOT closed here

1. **The eleven production await sites issue #809 enumerated still have no ordering assertions.** No
   test asserts ordering at any of them today, and this delivery adds none. The hardening does not
   change behaviour on the leg those sites take today: a caller genuinely standing on the captured
   UI thread inside a WPF dispatcher operation still completes inline, because on that leg
   `Dispatcher.FromThread(Thread.CurrentThread)` is the captured dispatcher and the added conjunct
   evaluates true. This residual is restated here rather than silently inherited.

2. **Whether the pinned test framework's STA test-class attribute forces STA for plain test methods
   remains unsettled by the tree.** The in-repository record is explicitly contradictory on the
   mechanism, and nothing in this delivery settles it. What this delivery did instead was convert
   the one test that depended on that assumption into one that measures it: the retry test now
   asserts `Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA)` as its first
   Arrange step. That is the extent of the claim; the open question is recorded for a future issue
   rather than resolved here. The surviving operational rule, which every new test in this delivery
   follows, is that a test needing a caller of a known apartment must create a dedicated thread and
   set the apartment explicitly.

## Host-path hygiene

This summary contains no absolute host path. Where one would otherwise appear it is written with the
tokens `<repo-root>`, `<user>` or `<host>` substituted; no such substitution was required in the
text above.
