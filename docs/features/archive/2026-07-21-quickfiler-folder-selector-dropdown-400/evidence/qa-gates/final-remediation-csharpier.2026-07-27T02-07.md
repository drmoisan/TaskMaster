# Final Remediation CSharpier Gate

- Timestamp: `2026-07-27T02-07Z`
- Run identity: `2026-07-27T02-07`
- Command: `$base=(git merge-base HEAD origin/main).Trim(); derive changed/untracked QuickFiler/**/*.cs, QuickFiler.Test/**/*.cs, UtilitiesCS/**/*.cs, and UtilitiesCS.Test/**/*.cs; exclude UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs; deduplicate; [Array]::Sort($authorized,[StringComparer]::OrdinalIgnoreCase); require count 62 and LF-joined SHA-256 E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD; verify protected hashes; hash the full authorized file inventory; csharpier format @authorized; require unchanged inventory; csharpier check @authorized; require unchanged inventory and protected hashes; require every authorized file <=500 physical lines`
- EXIT_CODE: `0`
- Output Summary: `P9_T1_OK run_id=2026-07-27T02-07 base=e63ddc7c18ca71e2c968b3329e42d965d45af1eb authorized=62 path_hash=E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD inventory_hash=20FA6CF44506AB1F8334475C6A9B9434EE7A9F866B39055E406FA8D1676CD7F2 format_exit=0 check_exit=0 authorized_delta=false protected_changes=0 over500=0`

## Results

`csharpier format` completed for 62 authorized paths in 18,743ms and `csharpier check` completed for the same paths in 11,575ms. Both commands exited zero. The pre-format, post-format, and post-check authorized inventory hash was `20FA6CF44506AB1F8334475C6A9B9434EE7A9F866B39055E406FA8D1676CD7F2`; formatting produced no authorized-file delta.

| Invariant | Result |
|---|---|
| Merge base | `e63ddc7c18ca71e2c968b3329e42d965d45af1eb` |
| Comparer | `StringComparer.OrdinalIgnoreCase` |
| Authorized paths | `62` |
| LF-joined path hash | `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` |
| Authorized inventory delta | `false` |
| Authorized files over 500 physical lines | `0` |

## Protected hashes

| Path | Required and observed SHA-256 before and after |
|---|---|
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |
| `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` |

This artifact is the current P9-T1 formatter evidence. It supersedes earlier P9 final-pass formatter artifacts without relying on them as current proof.
