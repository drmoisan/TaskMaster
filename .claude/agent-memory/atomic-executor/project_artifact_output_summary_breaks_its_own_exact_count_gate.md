---
name: artifact-output-summary-breaks-its-own-exact-count-gate
description: Restating an asserted literal inside the artifact that documents the gate makes an "exactly 1" git grep -c gate report 2 — the evidence file breaks the gate it is evidence for.
metadata:
  type: project
---

When a plan's acceptance is `git grep -F -c "<literal>" -- <file>` **reports exactly 1**, do not
restate that literal anywhere else in the same file — including in the artifact's own
`Output Summary:` or in a quoted-original passage. Refer to it in prose instead
("the machine-checkable field recorded above").

**Why:** On 2026-08-28 (issue 489, remediation cycle 1, P3-T1) the addendum appended to a handoff
record carried `ObligationDischargedInBranch: true` once as the field and once inside its own
Output Summary sentence. `git grep -c` counts **matching lines**, not files, so it returned 2 and
the "exactly 1" gate failed on an artifact that was otherwise completely correct. The same trap
applies to a dated-marker gate like `Amendment (2026-08-28).` where the amendment note naturally
wants to name itself, and to any spec amendment that quotes the original wording of a criterion the
gate also searches.

**How to apply:** Before writing an artifact whose acceptance is an exact-count grep, count the
literal's occurrences in the file you are about to write, not just in the file you are asserting
against — they are often the same file. Where the plan requires a criterion to be quoted verbatim
*and* an exact-count gate over a token, keep the token out of the quotation. When the gate does
fail this way, record the failed first attempt in the artifact rather than silently fixing it: it
is the cheapest available proof that the gate is capable of failing, which
[[preflight-selfderived-gate-thresholds-are-blind]] and the plan-acceptance-gate rules both care
about.
