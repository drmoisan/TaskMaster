# Issue #873 — Local Update Mirror

Timestamp: 2026-09-13T07-26
Task: [P7-T15]

PostedAs: unknown

This mirror is authored locally. The executor was directed not to publish: committing is the
executor's, publishing is the caller's. No `gh` invocation was made and no comment or body update was
posted, so the posting mode is recorded as unknown rather than claimed as `comment` or `body`.

---

## Exact text intended

### #873 — Test evidence projection convention and identity-leak tooling: Phase 7 complete, Phase 6 outstanding

Phases 0 through 5 and Phase 7 are complete. Phase 6 is partial: P6-T1 is done, and P6-T2 through
P6-T5 remain outstanding on a pre-existing defect that this delivery did not introduce and was not
authorised to repair.

**Acceptance criteria: 22 of 23 satisfied.** AC23 is OUTSTANDING because it depends on the two
end-to-end runs that Phase 6 has not made. Its checkbox in `spec.md` is not marked.

**What the delivery adds.** Two new pure part files under the editor script directory: a package-level
JaCoCo projection built from the post-processed Cobertura document with an exact reconciliation
assertion against the source root totals, and a test-result summary reader that resolves the default
TeamTest namespace explicitly and derives skipped as total minus executed without discarding any
figure the test platform reported. Both entry points now pass an explicit results directory and an
explicit trx log file name to their argument builders, so no test-result document is written under the
platform's default account-and-host file name. The raw collector document is retained when the
resolved output directory is the repository coverage directory and discarded for any other directory,
sequenced after the threshold assertion, the projection write and the reconciliation assertion.

**Convention.** `CLAUDE.md` carries a new `## Committed Test Evidence Format` section stating the
permitted committed test-evidence formats, and both of its test-console toolchain steps now name the
explicit `/ResultsDirectory:` and `/Logger:trx;LogFileName=` forms.

**Identity leaks.** The publish-destination element in the TaskMaster project file, the Power Query
symbols path in the editor settings file, and five agent-memory documents no longer carry the account
or host token. Each absence check is paired with a parse or structural check, because a zero match
count alone does not distinguish a correct edit from one that broke the markup.

**Phase 7 gate results.**

| Gate | Result | Phase 0 baseline |
|---|---|---|
| PowerShell analyzer | 16 diagnostics, 0 absent from baseline | 16 |
| PowerShell tests | 133 passed, 0 failed, 0 skipped | 103 passed |
| C# format check | exit 0 over 1626 files | exit 0 |
| C# analyzer rebuild | exit 0, 0 warnings, 0 errors | exit 0, 0 errors |
| C# nullable rebuild | exit 0, 0 warnings, 0 errors | exit 0, 0 errors |
| New-code coverage, summary part file | 92.86 percent line | floor 90 |
| New-code coverage, projection part file | 92.50 percent line | floor 90 |
| Largest PowerShell file | 498 lines | ceiling 500 |

The coverage gate failed on its first measurement at 82.50 percent for the projection part file. Two
tests were added to the projection test file, which is inside the declared Write Set, and the
toolchain loop was restarted from the format step; every preceding gate was re-run and passed. Both
the failing first measurement and the remediation are recorded in the evidence artifact rather than
replaced by the passing figure.

**Follow-up sequencing.**

1. **Issue #602's historical sweep runs after this item merges.** The sweep removes the leaked
   default-name prefix from the more than one hundred raw test-evidence documents already tracked
   from earlier features. It must run after this item rather than before it, because this item is what
   stops a fresh test run from writing a new document under the default account-and-host name. A sweep
   that ran first would be undone by the next coverage or test run, so the ordering is load-bearing
   rather than a convenience.
2. **Citing the new convention section from the atomic-plan contract's evidence tasks is an upstream
   change.** That contract file is push-down owned: it is overwritten from the customization
   repository with no templating, so an edit made in this repository is reverted on the next
   push-down. The citation must therefore be added upstream. It is recorded here in prose rather than
   applied, and this delivery edits no push-down-owned governance document.

**Not certified by this update.** AC23, and therefore the end-to-end behaviour of the two coverage
entry-point runs. Phase 6 must run before that criterion can be judged.

---

## Required fields

Timestamp: 2026-09-13T07-26
PostedAs: unknown
Follow-ups named: issue #602's historical sweep sequenced after this item merges; the atomic-plan
contract citation recorded as an upstream change because the contract file is push-down owned.

## POSTING STATUS

Not posted. The delegation that authorised this Phase 7 pass reserved publishing to the caller and
directed the executor not to open a pull request and not to push. Posting this text to issue #873 is
the caller's action.
