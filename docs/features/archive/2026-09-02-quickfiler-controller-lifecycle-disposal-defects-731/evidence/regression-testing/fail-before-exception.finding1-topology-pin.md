# Fail-before exception dossier — finding 1, three-owner monitor topology pin

Timestamp: 2026-09-03T14-10

Task: [P1-T8]
Issue: #731

Covers: `QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs`, both `[TestMethod]` members.

WhyFailingRunImpossible: Finding 1 changes no executable behaviour. The three per-owner `EmailMoveMonitor` instances already exist in the pre-change tree and are deliberately retained, so the topology the pin test asserts is already true before the change; the only production edits finding 1 makes are comments — one explanatory line above each of the three field initializers and a replacement of the stale class comment on `EmailMoveMonitor`. A test that asserted the post-change state would therefore pass on the pre-change tree as well, and there is no defect state to reproduce.

## Alternative proof

The pin test passes on the pre-change tree as well as on the post-change tree, and that is the intended and correct outcome for this finding.

- Pre-change state, established by the Phase 0 baseline: `EVIDENCE/baseline/mstest-coverage.md` records a full-suite run of 6985 tests with 6985 passed, 0 failed and 0 skipped, over a tree in which the three field initializers were already present at `QuickFiler/Controllers/QfcCollectionController.cs:83`, `QuickFiler/Controllers/QfcDatamodel.cs:103` and `QuickFiler/Controllers/QfcQueue.cs:40`.
- Post-change state: `EVIDENCE/regression-testing/finding1-topology-pin-pass.md` records both methods passing with `EXIT_CODE: 0`.

The test therefore functions as a **forward guard** against a future collapse of the deliberate three-owner topology into a shared singleton, rather than as a reproduction of a present defect. That collapse is the failure mode issue #731 finding 1 and issue #620 describe: `EmailMoveMonitor.BeforeItemMove` dispatches at most one action per MailItem via `FirstOrDefault` (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:212-218` at baseline), and `UnhookAll` is instance-scoped and clears the whole hook list (`:185-200` at baseline), so a shared instance would silently drop sibling owners' actions and unhook them all on any one owner's teardown.

The guard is discriminating rather than vacuous: `NoTypeDeclaresMoreThanOneEmailMoveMonitorField` asserts a total of exactly three declaring types, so both a collapse to fewer owners and an unintended addition of a fourth owner fail the test, and `EachOwnerDeclaresExactlyOneEmailMoveMonitorInitializer` fails if any one owner's initializer is removed or duplicated.

This exception is scoped to finding 1 alone. The plan requires a genuine failing run for finding 2, finding 3 and finding 4, recorded at [P2-T3], [P3-T2] and [P4-T4] respectively; no claim about the outcome of those runs is made here.
