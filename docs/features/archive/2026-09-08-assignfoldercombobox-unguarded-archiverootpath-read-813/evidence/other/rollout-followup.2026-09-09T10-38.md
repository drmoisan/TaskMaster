Timestamp: 2026-09-09T10-38

Verbatim reproduction of spec.md's "## Rollout & Follow-up" section (lines 244-251):

## Rollout & Follow-up
- Release/rollout steps:
  - Standard PR review and merge; no phased rollout, feature flag, or migration is required for a
    fix this narrow.
- Post-fix monitoring or clean-up tasks:
  - None beyond normal post-merge verification that the regression test passes in CI.
- Links: issue #813; related issue #812 (archive-root getter hardening, out of scope here); related
  issue #797 (originating report of the unset-archive-root state).
