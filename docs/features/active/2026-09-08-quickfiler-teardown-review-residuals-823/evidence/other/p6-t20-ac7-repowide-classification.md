# Phase 6 — AC7 repo-wide token classification

Timestamp: 2026-09-09T14-59

Task: [P6-T20]

AC7 words its `_userEmailRetryAttempted;` search as repo-wide, and repo-wide it cannot return no
match: the token is quoted as prose in several living documents, three of which D22 requires to
stay byte-identical. The criterion's intent is that no C# source declares the field, and the
source-scoped form is what tests that intent. Both forms were run and both are recorded, so the
divergence from AC7's literal wording is auditable rather than silent.

## The source-scoped form

Command: `git grep -c -F "_userEmailRetryAttempted;" -- UtilitiesCS UtilitiesCS.Test QuickFiler QuickFiler.Test TaskMaster`

EXIT_CODE: 1

It printed nothing and exited 1, so no file under any of the five source trees carries the token.
This is the form that tests AC7's intent, and it passes.

## The repo-wide form

Command: `git grep -c -F "_userEmailRetryAttempted;"`

EXIT_CODE: 0

Eight paths matched. Every one is a Markdown document and none is a `.cs` file, so none is a
declaration.

| Path | Count | Classification |
| --- | --- | --- |
| docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/code-review.2026-09-08T17-00.md | 1 | Documentation quotation. The issue-812 code review quotes the field it reviewed. D22 requires this file byte-identical and it is unchanged. |
| docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/plan.2026-09-07T22-12.md | 2 | Documentation quotation. The issue-812 plan quotes the field it introduced. D22 requires this file byte-identical and it is unchanged. |
| docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/policy-audit.2026-09-08T17-00.md | 2 | Documentation quotation. The issue-812 policy audit quotes the field. D22 requires this file byte-identical and it is unchanged. |
| docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md | 1 | Documentation quotation inside the dated correction block [P2-T14] appended, which names the replaced field so a reader can identify it. This is the one file in that folder this plan modifies, and R1.9 authorises the modification. |
| docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/p0-t14-token-baseline.md | 1 | Documentation quotation. The [P0-T14] token baseline names the token whose disappearance it baselines. |
| docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/plan.2026-09-08T23-50.md | 5 | Documentation quotation. This plan's Literals section and four task acceptances quote the token. |
| docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/research/research.2026-09-08T23-50.md | 1 | Documentation quotation. The research record quotes the field it found. |
| docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/spec.md | 2 | Documentation quotation. The specification's R1.1 current-state table and its AC7 text quote the token. |

The plan predicted six of these eight as a lower bound rather than a closed enumeration. The two it
did not name are `evidence/baseline/p0-t14-token-baseline.md`, which this execution created, and the
issue-812 `spec.md` occurrence, which [P2-T14] created. No match lies under `.claude/agent-memory/`,
because the executing agent made no persistent-memory write during this run.

## Disposition

AC7 is checked off: the source-scoped form exits 1 with no output, and every repo-wide match is
classified as a documentation quotation rather than a declaration in a `.cs` file. The retry state
in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` is a `HashSet<StoreWrapper>`
instance field whose declaration carries no `static` keyword, recorded with its line numbers and
`FIELD-IS-STATIC: NO` in `evidence/other/p2-t13-r1-ordering-read.md`.

Output Summary: Source-scoped search exits 1 with no output. Repo-wide search returns eight
documentation quotations across seven Markdown files, none a `.cs` declaration. AC7 met.
