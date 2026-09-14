# Conformance assessment against `## Committed Test Evidence Format` (CLAUDE.md line 412)

Timestamp: 2026-09-13T18-40
Command: `git ls-tree -r --name-only HEAD` and the same against `e6d86049e`, filtered for `.trx` and `.cobertura.xml`; plus a read of the policy text at CLAUDE.md line 412 on `origin/main`.
EXIT_CODE: 0
Output Summary: PARTIAL CONFORMANCE, reported plainly rather than quietly conformed to or quietly ignored. The branch fully satisfies the policy's prohibition — it adds zero raw documents — and satisfies the test-run summary form. It does NOT satisfy the coverage-run form: no package-level JaCoCo projection and no one-line first-party coverage summary is committed. The policy postdates this branch's merge base.

## Provenance of the policy

`## Committed Test Evidence Format` was added to `CLAUDE.md` on `main` and is at line 412 of the file at `e6d86049e31096914eaf6dcbc7222e5b9f435258`. It postdates this branch's merge base and arrived in this branch only through the merge recorded at `f8f4a15d3`. Every acceptance artifact this item committed was authored before the policy existed.

Whether a branch is judged against the policy at its merge base or against the policy at review time is an operator question. It applies to several items in flight, not only this one, and it is not resolved here.

## Requirement-by-requirement assessment

### Prohibition: no raw collector document and no raw test-platform document, anywhere, including under a feature folder's evidence tree

**CONFORMS.** Measured against the merged tree:

- Tracked `.trx` and `.cobertura.xml` paths at this branch's HEAD: **572**.
- The same at `e6d86049e` (`main`): **572**.
- Set difference, paths present at HEAD and absent on `main`: **0**.
- Untracked or modified raw paths in the working tree: **0**.

The branch therefore adds no raw document of either kind. Every tool run in this item wrote its raw output to the gitignored repository-root `coverage` directory, and the post-change Cobertura documents were discarded at plan task P6-T18. This was not retrofitted to satisfy the new policy: the item adopted a projections-only convention from the outset, under the maintainer decision on issue #671 dated 2026-09-11, which the earlier audit `evidence/qa-gates/final-projections-only-audit.2026-09-12T19-30.md` records.

Repository-level observation, outside this item's scope and reported rather than acted on: `main` itself already carries **572** tracked `.trx` and `.cobertura.xml` paths, inherited from items that merged before the policy existed. The new prohibition is written in the present tense ("Neither may be added to git in any form"), so those pre-existing paths are not added by anyone now; but if the intent is that the repository should hold none, a separate cleanup item is needed. This branch neither adds to that set nor reduces it.

### Permitted form: a test-result summary derived from the trx document, for a test run

**CONFORMS.** Committed trx-derived summaries, each recording the transcribed `ResultSummary/Counters` rather than the document:

- `evidence/baseline/phase0-serial-test-baseline.2026-09-12T16-30.md`
- `evidence/qa-gates/final-serial-test-run.2026-09-12T19-30.md` (`total=1400 passed=1400 failed=0 timeout=0`)
- `evidence/qa-gates/postmerge-toolchain.2026-09-13T18-30.md` (`Total tests: 1401 / Passed: 1401`)
- the AC1 and AC2 measurement and regression artifacts, which transcribe counters and per-test outcomes

### Permitted form: a package-level JaCoCo projection of the post-processed Cobertura document, for a coverage run

**DOES NOT CONFORM.** No JaCoCo projection is committed, in any form, anywhere on this branch.

What is committed instead is a **per-file Cobertura extraction table** in `evidence/qa-gates/ac4-coverage-comparison.2026-09-12T19-30.md`, giving `classNodes`, `linesValid`, `linesCovered` and `rate` for each of the two subject files, together with the root `line-rate`, `lines-valid` and `lines-covered` attributes. That is a projection of the post-processed document and not the document itself, so it complies with the prohibition and with the intent the policy states — it carries the figures a reviewer needs, in a form a reviewer can read in a diff. It is not, however, either of the two forms the policy enumerates for a coverage run.

### Permitted form: the one-line first-party coverage summary, committed alongside that projection

**DOES NOT CONFORM.** No one-line first-party coverage summary is committed on this branch.

## Why this was not repaired in this run, stated as a decision rather than an omission

Both non-conformances could only be repaired by producing a JaCoCo projection, and that is not available from what is on disk:

1. The post-processed Cobertura documents this item produced were deliberately discarded at P6-T18 under the projections-only convention. A directory scan of `coverage/**` for `*.cobertura.xml` returns nothing. There is no document left to project.
2. Producing one would require a fresh instrumented coverage run. That run would be a post-change measurement taken in a different session from the P0-T9 pre-change baseline, and AC4's own wording requires the pre-change and post-change figures to be taken "in the same session with the same command". A fresh run would not invalidate the existing AC4 pairing, since it would be an additional artifact rather than a replacement, but it would cost a full instrumented run under a shared machine build lock with two sibling items in flight, to satisfy a policy that postdates this branch's merge base and whose applicability to this branch is the operator's question.

The decision is therefore to report the gap rather than to close it unilaterally, and to leave the applicability question with the operator. If the operator rules that the policy binds at review time, the repair is one coverage run plus two artifacts and can be done before merge.
