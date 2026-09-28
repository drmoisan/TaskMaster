---
name: outcome-ac-when-mechanism-unverified
description: When the delivery mechanism rests on an unverified assumption (CI trigger, token identity, tool behaviour), write the acceptance criterion against the observable outcome rather than the mechanism, and handle an internally contradictory issue.md by pinning the resolution as a named design decision instead of silently choosing
metadata:
  type: feedback
---

Two authoring rules, both applied on #911 (2026-09-19, Dependabot repair pass).

**1. Unverified mechanism means an outcome-shaped criterion.** The design needed a workflow trigger
and credential that would make the required checks re-run on a bot branch. The supporting research
had no execution capability, so the trigger choice was documentation-derived. Writing an AC that
asserts the mechanism ("the workflow triggers on X") would pass on a wrong choice, because the file
would say X regardless. The criterion was instead written against the observable end state: for
every check named required by the ruleset, a check run exists on the post-repair head SHA, its
originating workflow run has event `pull_request`, its conclusion is success, and none is in an
approval-required state. A wrong trigger or a read-only token then fails visibly. State the
mechanism in Proposed Fix as an "assumption of record" and name the AC that falsifies it.

**Why:** this repository has a documented history of gates passing for reasons unrelated to the
property they assert. A mechanism assertion is a restatement of the diff; an outcome assertion is a
measurement.

**2. An internally contradictory authoritative issue is resolved in the spec, not deferred.** #911's
`issue.md` required both `.csharpierignore` coverage for `packages.config` / `app.config` **and**
"CSharpier formatting of `packages.config` and `app.config`" in the repair pass. Those neutralise
each other: an ignored path is not formatted. The research recommended the opposite of the issue on
this point. Resolution written into the spec: honour the authoritative issue (add the ignore
entries), then name the consequence explicitly (the formatter no longer defines a canonical form),
adopt a stated canonical form, and assign the normalisation to a named module with an idempotence
AC. Put the whole thing under a "Resolved tension" heading, label it a scope decision made by the
spec rather than by the issue, and repeat it in the final report to the caller.

**How to apply:** do not paper over the contradiction by dropping one clause, and do not leave it as
an open question for the planner. Related: [[ac-gates-verify-satisfiability]],
[[full-bug-spec-only]], [[backticked-paths-are-the-change-footprint]].
