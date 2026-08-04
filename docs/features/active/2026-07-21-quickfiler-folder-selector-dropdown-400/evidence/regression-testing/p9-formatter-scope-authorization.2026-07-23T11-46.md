# Phase 9 Formatter-Scope Authorization

- Timestamp: `2026-07-23T11:46:08Z`
- Command: `Get-FileHash -Algorithm SHA256 <spec, plan, coverage.config, .csharpierignore, SpamBayes.Actions.cs>; git merge-base HEAD origin/main; derive the ordinally sorted issue-#400 C# path set from merge-base changes plus untracked QuickFiler/**/*.cs, QuickFiler.Test/**/*.cs, UtilitiesCS/**/*.cs, and UtilitiesCS.Test/**/*.cs paths while excluding SpamBayes.Actions.cs; hash the LF-joined path set; git diff --check -- <spec, plan>`
- EXIT_CODE: `0`
- Output Summary: `SCOPE_CHANGE_AUTHORIZED authorized_paths=62 path_hash=E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD protected_changes=0 spec_wording_changed=true plan_revised=true diff_check=clean`

## Decision

The user was presented with three explicit dispositions for the conflict between literal root `csharpier format .` and the protected/unrelated formatter targets. The user responded `continue` after option 1 was identified as recommended. This authorizes option 1 only:

- Preserve `coverage.config`, `.csharpierignore`, and the unrelated committed `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` byte-for-byte.
- Revise the specification toolchain section, AC-18, and P9-T1 to format and check the exact 62-path issue-#400 C# scope.
- Permit one bounded, behavior-preserving test-helper compaction in `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` so genuine CSharpier output remains within the retained 480-line limit.
- Retain repository-wide analyzer, nullable, complete test/coverage, numeric threshold, independent review, validator, commit, PR, and CI requirements.

This decision does not waive a threshold, test, review, or validation gate. AC-18 remains unchecked until current Phase 9 and Phase 10 evidence passes.

## Scope and Protected Hashes

| Item | SHA-256 |
|---|---|
| Live merge base `df5ad49c909f6b739edef45d0336151f44e827a6` plus untracked exact 62-path issue scope, ordinally sorted and LF-joined without trailing LF | `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` |
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |
| `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` |

All three protected hashes were unchanged when this artifact was written.

## Specification and Plan Revision Ledger

| Artifact | Before SHA-256 | After SHA-256 | Authorized change |
|---|---|---|---|
| `spec.md` | `42A7E11878CBED4F63F8DF6F7A83F538C7610247510576EF47496DCC5E9603E1` | `F8B337F60AC249231FF1FD2F3C5EA24EF0B9592E57CFD75F622EEA9EBF530D04` | Replace only the root formatter command in the toolchain description and AC-18 with the exact scoped formatter derivation, count/hash, check, and protected-hash requirements. The AC-18 checkbox remains unchecked. |
| `remediation-plan.2026-07-21T21-37.md` | `05E0F3FB7E2BD3FBA97DB215D2C61674442B26F080D06F3C0B375ADA02E631D3` | `EA1C4101C7B5D41AEA88EE4F8290FE0BED907711BF08D9FD3E9B61E5066043D4` | Reconcile the execution-head status to `a1fbb5b0ce7c058dd44debdf1510282050928687`, record the eighth revision input, distinguish the historical initial-planning receipt from authorized revisions, authorize P8-T20 through P8-T26 as one bounded test-only stabilization, distinguish the P8-T22 planned delta from the no-delta P9-T1 pass, make P9-T1 scope-specific, and update the mandatory preflight contract. |

`git diff --check` returned numeric exit code zero for both revised artifacts. No production, test, project, resource, configuration, filter, threshold, or exclusion file was changed by this decision-recording step.
