# Phase 9 Formatter Ordering-Contract Correction

- Timestamp: `2026-07-23T12:33:18Z`
- Command: `$base=(git merge-base HEAD origin/main).Trim(); $patterns=@('QuickFiler/**/*.cs','QuickFiler.Test/**/*.cs','UtilitiesCS/**/*.cs','UtilitiesCS.Test/**/*.cs'); $spam='UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs'; [string[]]$authorized=@(@(git diff --name-only --diff-filter=ACMR $base -- $patterns)+@(git ls-files --others --exclude-standard -- $patterns)|Where-Object {$_ -and $_ -ne $spam}|Select-Object -Unique); [Array]::Sort($authorized,[StringComparer]::OrdinalIgnoreCase); hash the LF-joined paths; clone and sort with [StringComparer]::Ordinal; hash the strict-ordinal paths; verify protected hashes; hash spec.md and remediation-plan.2026-07-21T21-37.md; git diff --check -- <spec, plan>`
- EXIT_CODE: `0`
- Output Summary: `ORDER_CONTRACT_CORRECTED authorized=62 ordinal_ignore_case_hash=E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD strict_ordinal_hash=4FC9A8CACF4E93CA4D8D5F7AC90C82922C7FEF09A4B266A5B70486A189D7D618 protected_changes=0 spec_hash=C139DC5186CF005E8B75CC2D2F1B2AD05B7D5B562F13830ACA57365179ED7C9E plan_hash=4CE2459C211575C6C184C39CDF8EE010346123A08C012A62F9DBEF9695A852FC diff_check=clean`

## Independent Finding

The first P8-T26 independent review by `/root/p8_t26_independent_review` returned:

| Severity | Count |
|---|---:|
| Blocker | 0 |
| Major | 0 |
| Medium | 0 |
| Low | 1 |

The sole Low finding was a wording/algorithm mismatch. The existing `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` path hash was produced by PowerShell default ordering, which matches `StringComparer.OrdinalIgnoreCase` for the current 62 paths, while the specification and plan called that order strict ordinal. Strict `StringComparer.Ordinal` instead produces `4FC9A8CACF4E93CA4D8D5F7AC90C82922C7FEF09A4B266A5B70486A189D7D618` because each `.Part2.cs` name sorts before its paired `.cs` name.

All other independent checks passed, including the semantic one-test-file change, four formatter-only files, preserved 13-case and assertion inventories, 62-file CSharpier check, analyzer and nullable gates, 13/13 independent VSTest rerun, protected hashes, line limits, no masking, no project/configuration/filter/threshold/exclusion change, runner cleanup, and `git diff --check`.

## Corrected Contract

The authorized set and its membership did not change. The in-place correction:

1. Explicitly sorts the 62 paths with `StringComparer.OrdinalIgnoreCase`.
2. Retains the authorized LF-joined hash `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD`.
3. Updates only the specification and existing remediation plan wording.
4. Requires a new atomic-executor preflight, canonical plan validation, and fresh independent P8-T26 review before Phase 9.

## Protected Hashes

| Path | SHA-256 |
|---|---|
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |
| `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` |

No protected path changed.
