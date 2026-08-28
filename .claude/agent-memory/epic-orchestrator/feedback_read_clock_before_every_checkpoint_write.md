---
name: read-clock-before-every-checkpoint-write
description: Run date -u immediately before every checkpoint write — carrying a timestamp convention forward silently skewed my whole audit trail by 15.5 hours, and the wave-barrier validator reads those timestamps
metadata:
  type: feedback
---

Run `date -u` immediately before **every** checkpoint or evidence write. Never carry a timestamp forward from
an earlier write in the same session, and never infer one from the session context banner.

**Why:** On the quickfiler-bug-family epic I stamped a run of checkpoint records `2026-08-26T21-52Z` through
`2026-08-26T22-40Z` by reusing the convention from earlier writes. The real clock was `2026-08-27T13:51:26Z`
— every value was ~15.5 hours early. Sessions that span a day make this trivially easy, and nothing warns you.
It is not cosmetic: the retrospective wave-barrier invariant in `validate_epic_orchestrator_state_text`
compares per-feature lifecycle timestamps to decide whether a feature started before its dependency merged, so
skew can both mask a real ordering violation and manufacture a false one.

**How to apply:** Read the clock per write. If you discover skew after the fact, do **not** rewrite the bad
values into invented precise instants — that fabricates precision. Record a correction entry that names the
affected fields, bounds them ("all within the two hours before <true instant>"), and cites filesystem
corroboration, which is authoritative: gitdir `index`/`ORIG_HEAD` mtimes gave me instants that postdated every
skewed value while describing work I had already recorded as finished. Also warn each child to do the same;
they inherit the habit from the prompt. Related: [[fan-in-hook-paths-resolve-to-session-cwd]].
