# Merge-Time Instructions — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-29-00
- Task: [P6-T3]
- Findings: R4, R1
- EXIT_CODE: 0

Three instructions for whoever merges this pull request. Each carries its reason, because an
unreasoned instruction is the one a later reader drops.

---

## Instruction 1 — Squash-merge the pull request

**Do not use a merge commit and do not rebase-merge. Squash.**

**Reason.** [P4-T2] sanitised the **working tree**: 103 occurrences of an absolute host path
disclosing the account name were replaced across 33 committed documents, and [P4-T3] measured
the residual at zero. But the **pre-sanitisation blobs remain reachable in this branch's
history**. Every commit from the [P0-T2] anchor back through the branch's 25-commit history
still contains the original text, and a merge commit preserves every one of those parents on
the default branch.

A squash merge writes one commit containing the sanitised tree and nothing else. The
pre-sanitisation blobs then become unreachable from the default branch.

This leak class has recurred on issues **#645, #680, #730 and #752** despite being fixed each
time, and it recurred because the fix was applied as a follow-up commit rather than at the merge
strategy. The remedy is the merge strategy.

**Citing artifacts.**
`evidence/qa-gates/p4-t2-sanitisation.2026-09-20T01-37.md`,
`evidence/qa-gates/p4-t3-residual.2026-09-20T01-37.md`,
`evidence/remediation-baseline/p0-t3-hostpath-census.2026-09-20T01-37.md`.

---

## Instruction 2 — Strip the two detector false positives again if `pr_context` is regenerated

**Before the pull-request body is authored, confirm that
`artifacts/pr_context.summary.txt` carries neither `#MEZIANTOU-898` nor `#SHA-256`. If it does,
strip both from every close-candidate section.**

**Reason.** [P4-T4] removed both tokens from both close-candidate sections of the current file.
They are **false positives from the issue-number detector**, which re-derives them from the
words `Meziantou.Analyzer` and `SHA-256` in prose. Both phrases appear throughout this feature's
documents and neither is going away, so **a regenerated `pr_context.summary.txt` reintroduces
both, in both sections.**

Regeneration is likely: the current file was generated at `2026-09-20 05:30:49 UTC` against head
`794d34f02`, which is five commits behind `H1`.

A pull-request body carrying `#SHA-256` would reference an unrelated issue number and could
close it.

The correct autoclose list is exactly these 11, in this order: `#181`, `#563`, `#668`, `#895`,
`#898`, `#902`, `#903`, `#907`, `#908`, `#909`, `#911`.

**Citing artifact.** `evidence/qa-gates/p4-t4-autoclose-list.2026-09-20T01-37.md`.

---

## Instruction 3 — Do not represent issue #911 as closed until #914 discharges AC18, AC19 and AC20

**The pull request may merge. Issue #911 must stay open.**

**Reason.** Three acceptance criteria are unverified and stay unticked, and they are precisely
the three that would exercise the forward-prevention half of the change:

- **AC18** — The repair commit is pushed under the GitHub App identity.
- **AC19** — The required checks re-run and pass on the post-repair head SHA.
- **AC20** — Disclosure is present and conditional.

All three require a GitHub App credential and an open Dependabot pull request. [P0-T13] measured
both absent from an authorised query: **zero** repository Actions secrets and **zero** open pull
requests. `.github/workflows/dependabot-repair.yml` has therefore **never executed**, and four
of the nine review findings sat inside it for exactly that reason.

Issue **#914** carries all three criteria and the live fixture they need.

**Citing artifacts.**
`evidence/remediation-baseline/p0-t13-remote-probe.2026-09-20T01-37.md`,
`evidence/qa-gates/p5-t12-finding-status.2026-09-20T01-37.md`.

---

## The Four Residuals That Remain Unverifiable Until #914

Reproduced from [P5-T12] per **gate rule 20**. None is closed by the green CI run at [P6-T2],
because that run does not execute `dependabot-repair.yml`.

1. **That `actions/create-github-app-token@v3` publishes an `app-slug` output.** Decision D3
   records this as an assumption of record. [P3-T7]'s first guard turns a wrong assumption into
   a named step failure rather than a silent bad commit identity.
2. **That the resolved bot user id produces a commit whose `author.login` ends `[bot]`** and is
   not `github-actions[bot]`. This is AC18's stated acceptance.
3. **That the push causes the required checks to re-run** on the post-repair head SHA. This is
   AC19. The mechanism is sound in principle: the push carries an App installation token.
4. **That the disclosure edit produces exactly one block on a real pull-request body.** [P3-T5]
   exercises the strip-then-append expression against a body this repository constructs, not one
   GitHub returned. This is AC20.

Residual 1 underlies residual 2. R6, R7 and R8 should all be verified by the same live run that
settles these.

---

## R1's Authoritative Discharge

**R1 is discharged authoritatively by the PR-context `CI` run at the merge head**, not by the
dispatched run this cycle recorded.

[P6-T2] dispatched `CI` against the branch and recorded run **35513025198** at `headSha`
`de9a00106c951a073c1ac33a4cf5223e24563cd8`, `conclusion` **success**, with all six jobs green.
That run carries `event` `workflow_dispatch`. The rule the review cited —
`modified-workflow-needs-green-run` — demands a green run of the modified gate **at the exact
commit being merged**, and the merge head is not knowable from here.

The dispatched run is the evidence that the six gates pass at head today. The orchestrator
records the PR-context run at pull-request time and that is what closes R1.

**Citing artifact.** `evidence/qa-gates/p6-t2-ci-run.2026-09-20T01-37.md`.

## Output Summary

Three merge-time instructions, each with its reason and its citing artifact paths: squash-merge
because the pre-sanitisation blobs remain reachable in branch history; re-strip the two detector
false positives if `pr_context` is regenerated; and keep issue #911 open until #914 discharges
AC18, AC19 and AC20. The four #914 residuals are reproduced, and R1's authoritative discharge is
named as the PR-context CI run at the merge head.
