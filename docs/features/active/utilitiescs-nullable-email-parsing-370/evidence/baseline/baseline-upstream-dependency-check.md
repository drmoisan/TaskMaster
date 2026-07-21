# Baseline Upstream Dependency Check (Wave-0 `utilitiescs-nullable-extensions`, issue #363)

Timestamp: 2026-07-19T00-10

Command: `ls UtilitiesCS/Extensions/NullExtensions.cs UtilitiesCS/Extensions/StringExtensions.cs UtilitiesCS/Extensions/IEnumerableExtensions.cs` and `grep -n "#nullable" <file>` per file.

## Result

- `UtilitiesCS/Extensions/NullExtensions.cs` — present; line 12: `#nullable enable`.
- `UtilitiesCS/Extensions/StringExtensions.cs` — present; line 11: `#nullable enable`.
- `UtilitiesCS/Extensions/IEnumerableExtensions.cs` — present; line 16: `#nullable enable`.

## Confirmation

All three Wave-0 files exist and already carry `#nullable enable`. This satisfies the plan's
stated precondition ("Before starting Phase 1, confirm the Wave-0 `utilitiescs-nullable-extensions`
(issue #363) child has merged its verify-only file, Batch B, and Batch C"). Git log on this
branch confirms the Wave-0 merge commit (`11d47612 docs(363): record post-merge nullable pragma
gate verification`, merged via PR into `epic/utilitiescs-nullable-remediation-integration` at
`df2235bc`, which is this branch's fork point). Phase 1 may proceed.
