# Debt 2 — Batch: People — Baseline

Timestamp: 2026-07-20T00-40
Command: filtered from a fresh isolated rebuild (post-OlFolderTools batch), using the
authoritative per-`(file, line, col, code)` deduped extraction method.

Files under `UtilitiesCS/EmailIntelligence/People/**` (confirmed via
`find UtilitiesCS/EmailIntelligence/People -iname "*.cs"`): `PeopleScoDictionaryNew.cs` and
`PeopleScoDictionaryNewBackup.cs` (2 files total). `PeopleScoDictionaryNewBackup.cs` is the
already-documented dead, uncompiled duplicate (not in the csproj `<Compile Include>` set per the
epic's Maintainer Decision Summary) and is excluded from remediation scope (not part of the
build, cannot emit diagnostics).

Diagnostics (3 total, `PeopleScoDictionaryNew.cs` only):

| File | Diagnostics |
|---|---|
| `PeopleScoDictionaryNew.cs` | CS8604:2, CS8600:1 |

These 3 diagnostics are handled in P2-T13 (excluding the island lines 29/30/32). The island
decision itself is handled separately in P2-T14.
