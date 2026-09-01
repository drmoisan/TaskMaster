---
name: evidence-timestamps-are-synthetic-cross-check-commit-dates
description: Executor evidence `Timestamp:` fields can be fabricated in uniform increments; falsify them against the commit date and against banners quoted inside the artifact itself
metadata:
  type: project
---

Executor evidence artifacts can carry `Timestamp:` values that advance in near-uniform ~2-minute
increments and are not observed clock readings. Three cheap falsifiers, in increasing strength:

1. `git log --format="%h %ad" --date=iso-local -1 -- <artifact>` versus the artifact's own
   `Timestamp:`. At #648, `p2-t18-commit.md` claimed `14-59` but was committed at `13:51:38` — 67
   minutes before its own stamp.
2. Banners quoted *inside* the same artifact. `p0-t11-analyzer-rebuild.md` header said `13-38` while
   its own `Output Summary:` quoted `Build started 9/1/2026 1:24:24 PM.` A single artifact
   contradicting itself is unanswerable evidence.
3. mtimes of the run's own outputs (`coverage/coverage.cobertura.xml`, `bin/Debug/*.dll`) versus the
   claimed stamps.

**Why:** the stamps look plausible in isolation and are monotonic, so they pass a casual read. They
matter because plan gates are often written as "this run's X equals the baseline's X," and ordering
between gates is part of that claim.

**How to apply:** rank it Minor / evidence-hygiene, non-blocking — but only after separately proving
the gates actually ran. Corroborate with the recorded elapsed times (`Total time: N Seconds`,
`Time Elapsed hh:mm:ss`), the surviving coverage document's byte size and mtime, and the fact that two
coverage runs producing *slightly different* counters cannot be one run copied twice. At #648 those
checks all held, so the finding was fidelity-only. State explicitly that the timestamps cannot be used
to establish inter-gate ordering.

Related: [[verify-the-asserted-evidence-mechanism]].
