# Final Remediation CSharpier Gate

- Timestamp: `2026-07-23T12:49:49Z`
- Run identity: `2026-07-23T12-49`
- Command: `$base=(git merge-base HEAD origin/main).Trim(); derive changed/untracked QuickFiler/**/*.cs, QuickFiler.Test/**/*.cs, UtilitiesCS/**/*.cs, and UtilitiesCS.Test/**/*.cs; exclude UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs; deduplicate; [Array]::Sort($authorized,[StringComparer]::OrdinalIgnoreCase); require count 62 and LF-joined SHA-256 E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD; verify protected hashes; hash the full authorized file inventory; csharpier format @authorized; require unchanged inventory; csharpier check @authorized; require unchanged inventory and protected hashes; require every authorized file <=500 physical lines; git diff --check`
- EXIT_CODE: `0`
- Output Summary: `P9_T1_OK run_id=2026-07-23T12-49 base=df5ad49c909f6b739edef45d0336151f44e827a6 authorized=62 path_hash=E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD inventory_hash=6B38F11CF7E064BD583358E6DAB6393CBCF7216A12C9B8F2023FA17C1513827E format_exit=0 check_exit=0 protected_changes=0 over500=0 diff_check=0`

## Results

```text
Formatted 62 files in 15092ms.
Checked 62 files in 10318ms.
```

The format command produced no authorized-file delta. The check command also preserved the same authorized-file inventory.

| Invariant | Result |
|---|---|
| Merge base | `df5ad49c909f6b739edef45d0336151f44e827a6` |
| Comparer | `StringComparer.OrdinalIgnoreCase` |
| Authorized paths | 62 |
| LF-joined path hash | `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` |
| Pre/post authorized inventory hash | `6B38F11CF7E064BD583358E6DAB6393CBCF7216A12C9B8F2023FA17C1513827E` |
| Files over 500 physical lines | 0 |
| `git diff --check` | Exit 0 |

## Protected Hashes

| Path | Required and observed SHA-256 |
|---|---|
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |
| `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` |

This is the formatter step of final-pass run identity `2026-07-23T12-49`. No later Phase 9 evidence may cite this artifact if a final-QA command changes source or fails.
