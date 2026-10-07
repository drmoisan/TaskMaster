# Cycle 3 P1-T4 Preservation Verification

Timestamp: 2026-10-06T23-57
Command: Verify both backup refs with `git rev-parse`; compute SHA-256 for the snapshot blobs and restored working files under `2026-10-06T23-34-audit/` and `2026-10-06T23-37-remediation/`; compare the live plan with its snapshot version.
EXIT_CODE: 0
Output Summary: Both preservation refs remain at their recorded objects. The three audit files and remediation inputs exactly match their pre-snapshot SHA-256 values. The snapshot retains the exact pre-snapshot plan; the live plan differs only by the required P1-T3 execution check-off.

## Preservation Refs

- `refs/heads/backup/issue-979-pre-isolation-f09f2ae2` -> `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`
- `refs/backup/issue-979/uncommitted-artifacts-f09f2ae2` -> `71406da89795bf1b020a519f9f97046e13948505`
- Snapshot untracked parent: `01bb2e748c4911a6e3e3850e5f853bbceba37903`

## Restored File Inventory and SHA-256

| Path | Pre-snapshot / snapshot SHA-256 | Current SHA-256 | Result |
|---|---|---|---|
| `2026-10-06T23-34-audit/code-review.2026-10-06T23-34.md` | `e5ecc3e60ea0f8e306f8f0009cbfc8e4cb31c82f00bcdd34f93cfe32bbb46d62` | `e5ecc3e60ea0f8e306f8f0009cbfc8e4cb31c82f00bcdd34f93cfe32bbb46d62` | Exact match |
| `2026-10-06T23-34-audit/feature-audit.2026-10-06T23-34.md` | `9d302ac512430b3a9d6b524e2aefd8bdbcfbae4392b6d05021e067b58780e094` | `9d302ac512430b3a9d6b524e2aefd8bdbcfbae4392b6d05021e067b58780e094` | Exact match |
| `2026-10-06T23-34-audit/policy-audit.2026-10-06T23-34.md` | `085efe4c767c3803dc06bbbe3b1754be0409ab9874e2cc09fdac7d919bc20645` | `085efe4c767c3803dc06bbbe3b1754be0409ab9874e2cc09fdac7d919bc20645` | Exact match |
| `2026-10-06T23-37-remediation/remediation-inputs.2026-10-06T23-37.md` | `dac40517406d4e0f957995086a3ca4f453ef63c334de0d1a28e361bcaee0fc58` | `dac40517406d4e0f957995086a3ca4f453ef63c334de0d1a28e361bcaee0fc58` | Exact match |
| `2026-10-06T23-37-remediation/remediation-plan.2026-10-06T23-37.md` | `90146a7647d8550da9938cd52ee04cd0571e8eb105221a0edf054998dc72dc21` | `a732e912f0d82b07c0bd9d3e20c7a09752e6c57786a6286b3a723a94fc064bd6` | Snapshot exact; live plan advanced only at P1-T3 |

The initial stash application materialized CRLF worktree bytes because local `core.autocrlf=true`. The five files were re-read from the immutable snapshot tree with LF output before this verification. The four non-plan artifacts now match their pre-snapshot bytes exactly. A line comparison between the snapshot and live plan reports only:

- Snapshot: `- [ ] [P1-T3] Snapshot all uncommitted paths ...`
- Live plan: `- [x] [P1-T3] Snapshot all uncommitted paths ...`

The plan snapshot remains recoverable at its exact pre-snapshot hash. The live plan retains mandatory executor progress and no requirement or task wording changed.
