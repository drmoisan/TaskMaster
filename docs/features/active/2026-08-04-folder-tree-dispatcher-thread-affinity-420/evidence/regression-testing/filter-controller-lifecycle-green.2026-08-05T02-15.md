# P5-T25 through P5-T27 green evidence

Timestamp: 2026-08-05T02:15:00-04:00 (derived from the artifact filename)
Command: N/A — historical source-summary artifact; it records prior gate and serialized-suite results but does not preserve an executable command string.
EXIT_CODE: N/A — no standalone command is preserved in this summary artifact.
Output Summary: The recorded controller regression suite passed 26/26 and documents candidate-view ownership, synchronous-failure identity, terminal rechecks, and no global dispatcher mutation or production fallback.

- CSharpier, analyzer, nullable production build, `git diff --check`, compile-entry, forbidden-pattern, and residual-runner checks passed.
- The serialized controller regression suite passed 26/26.
- P5-T9 and P5-T15 through P5-T17 verify candidate-view ownership, exact-once disposal with original synchronous failure identity, exact null-globals parameter names, service getter before FormClosed subscription add, ArchiveRoot terminal rechecks, and candidate/subscription dispose interleavings.
- Candidate and subscription races prove no committed view, stored SnapshotChanged handler, post-dispose refresh, or application notification after disposal.
- Initial and refresh composition use an instance-local test dispatcher; production has no fallback dispatcher, global test hook, or fire-and-forget dispatch.
- Source-line results: controller lifecycle 191; controller tests 481, 489, 492, 497, and 234. Each planned source retains one compile entry.
