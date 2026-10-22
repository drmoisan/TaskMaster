# Issue update mirror — issue 743 (AC1 ratification outcome)

Timestamp: 2026-09-13T18-10
PostedAs: comment
URL: https://github.com/drmoisan/TaskMaster/issues/743#issuecomment-5656400054
Command: `gh issue comment 743 --repo drmoisan/TaskMaster --body-file <gitignored scratch path>`
EXIT_CODE: 0
Output Summary: one comment posted to issue 743 recording the maintainer's ratification of the AC1 negative result, the withdrawal of both superseded verdict claims, and the cross-reference to the new issue 882. The body file was written outside the repository tree so no untracked artifact entered it.

## Exact text posted

## AC1: maintainer ratification of the recorded negative result (2026-09-13)

The maintainer has ruled on the AC1 escalation. Recording the outcome here so it is auditable outside the feature folder.

**What was measured.** The instrumented runs produced no expiry. Both regimes recorded `timeout=0` (SERIAL `GATECOUNTERS acquisitions=11 releases=10 contended=0` over 1394 tests; PARALLEL `19/18/14` over 1395). AC1's final sentence provides that an instrumented run producing no expiry is "a recorded negative result, not a pass", so AC1 is not satisfied and its checkbox in `spec.md` stays `- [ ]`.

**What was ratified.** The maintainer accepts the item for merge despite that negative result, subject to four conditions. The ratification is the maintainer accepting the item despite a negative result; it is not a finding that the result was positive, and it does not convert AC1 into a pass. The unchecked box and the recorded ratification state two different things and both are kept in the record.

**Correction to the earlier verdict.** The verdict artifact previously claimed H-LEAK was "REJECTED by direct observation", and a first amendment then claimed that rejection "rests entirely on the counter observable, which is a legitimate basis". Both claims were wrong and both are now withdrawn, with the superseded texts retained for the audit trail. With `timeout=0` no test was abandoned, so under the spec's own definition of H-LEAK as a cascade conditional on a prior expiry, no leak could have occurred under either hypothesis; the `contended=0` reading was predetermined and carries no information about the hypothesis. **Neither H-COST nor H-LEAK was discriminated.** A hypothesis cannot be rejected by the absence of observations.

**Condition 4 — the open question is carried forward.** H-LEAK was never excluded, only never observed, and this item's UI-marshalling seam routes the affected tests around the question rather than answering it. That open question is now filed separately as **#882**, so it is not retired when this item merges. Correction C2 of this item's spec is its load-bearing evidence: `TransactionGate` remains a `SemaphoreSlim(1,1)`, still awaited without timeout or cancellation token, still held from acquisition to disposal — #493 changed the owner of the serialization, not its shape.

Full record: `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/evidence/other/maintainer-ratification-ac1.2026-09-13T18-00.md`.
