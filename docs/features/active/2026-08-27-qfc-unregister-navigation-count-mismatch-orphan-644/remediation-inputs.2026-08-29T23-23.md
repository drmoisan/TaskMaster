# Remediation Inputs — Issue #644, Cycle 1

- Timestamp: 2026-08-29T23-23
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head at cycle entry: `a2c69aead286ad0ec6c7087f1bd8c46d39d0d472`
- Pre-change anchor: `e968a1a8804b7641380d4489c496662824d45767`
- Source audits: `code-review.2026-08-29T23-06.md`, `policy-audit.2026-08-29T23-06.md`, `feature-audit.2026-08-29T23-06.md`

## Why this cycle exists

The review returned **0 blocking findings**, so the exit gate was already met and no cycle was
mandated. This cycle is opened by orchestrator election over two non-blocking findings that the
reviewer expressly recommended fixing before merge. Both are defects this branch itself introduces
into `main`, and both are cheap and fully contained. Recording the distinction matters: the entry
condition here is an elective quality pass, not a blocking-gate failure.

The remaining non-blocking findings are deliberately NOT in scope for this cycle. They are recorded
in the committed audit artifacts and are carried forward as reported findings.

## Item 1 — CR-1: stale XML documentation describing a deleted mechanism

**File:** `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`

**Location:** the `<summary>` block on the test
`UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys`.

**Text at fault:** the block ends "After the fix it replays the recorded width 1 and, because the
loop bound has grown to ten, removes every registered key."

**The violated rule:** `CLAUDE.md` C#6.3 requires comments to stay synchronized with behavior.

**Why it is wrong after this change.** Both mechanisms the sentence names were deleted by this
change. There is no longer a recorded *width*: `_registeredDigits`, its assignment, and the format
expression derived from it were removed together. There is no longer a *loop bound* at all:
`UnregisterNavigation` replays a ledger of recorded key strings verbatim instead of looping to
`_itemGroups.Count`. The sentence therefore describes the superseded #472 implementation as though
it were current behavior.

**Why it is in scope rather than a follow-up.** This is the same defect class the approved plan
already treated as in scope at `[P3-T5]`, which corrected exactly this kind of behavior-desynchronized
comment in `QfcCollectionControllerDefects468Tests.cs` for the same reason. This change modified this
same file at `[P3-T4]`; the sibling documentation block was updated there and this one was missed.
It also sits on the test that carries the #472-supersession proof, which is the claim the commit
message makes, so a false mechanism description here is worse placed than it would be elsewhere.

**Constraint.** Comment and documentation text only. Change no assertion, no test name, no attribute,
and no executable line. The sibling block earlier in the same file (the `Issue #472` summary that
already names the #644 ledger) is correct and must not be disturbed.

## Item 2 — PA-7: absolute host paths committed into repository documents

**Instances, both of which name the account and an agent-worktree identifier:**

1. `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/research/research.2026-08-29T07-55.md`,
   the `- Worktree:` line. Tracked and already committed on this branch, so it enters `main` with this
   pull request.
2. `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/policy-audit.2026-08-29T23-06.md`,
   which quotes the same path when citing finding PA-7. Untracked at cycle entry.

**The violated rule:** repository artifact hygiene prohibits embedding absolute host paths, bare
account names, or machine names in committed artifacts.

**Why it is in scope.** The repository is actively remediating pre-existing host-identity leaks under
a separate tracked issue. Admitting a *new* instance through this pull request adds to the very set
being cleaned up, which is a worse outcome than an old instance left in place. Both instances are
authored by this run.

**Constraint.** Redact to a repository-relative or clearly generic form that preserves the meaning of
each line. Do not delete the surrounding context and do not alter any finding, verdict, or measured
figure in the policy audit. Sweep the whole feature folder afterward and confirm zero remaining
instances.

## Explicitly out of scope for this cycle

- **AC-16.** Its adjudication stands as recorded: PARTIAL, left unchecked, referred and reported. It
  is not re-opened, no coverage comparison is re-run, and no attempt is made to obtain a passing
  figure. A comment-only edit in a test file cannot change instrumented production line counts, and
  re-running the instrument whose measured noise exceeds the disputed delta would produce a third
  number without resolving anything.
- **PA-1, PA-2** — pre-existing conditions not authored by this change.
- **PA-3** — procedural, dispositioned in the audit.
- **PA-4, PA-5** — the overclaim and the superseded sentence both sit in a recorded evidence artifact
  that must not be rewritten. Correcting forward is the disposition the reviewer endorsed.
- **PA-6** — a bookkeeping inconsistency in `[P4-T8]`'s checkbox. Unchecking a completed task at this
  stage would leave the plan showing an unchecked task with no remaining work, which is a worse
  record than the disclosed deviation already written into that task's evidence artifact. Carried as
  a reported finding instead.
- **CR-2, CR-3, CR-4, CR-5** — recorded in the code review. CR-3 and CR-5 are promotion candidates and
  are reported to the parent, because this branch is barred from committing anything under
  `docs/features/potential/`.

## Exit condition for this cycle

A reaudit producing `code-review`, `feature-audit`, and `policy-audit` artifacts at a new timestamp,
with a total blocking count of zero and both items above confirmed remediated.
