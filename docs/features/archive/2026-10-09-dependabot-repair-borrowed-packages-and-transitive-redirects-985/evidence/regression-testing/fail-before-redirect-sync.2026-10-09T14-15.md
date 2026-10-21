# Fail-before: Composition-root Redirect Sync (P1-T4) [expect-fail]

Timestamp: 2026-10-09T14-15
Command: pwsh -NoProfile -File CMDDIR\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- TEST-PATH-COUNT: 1; COVERAGE-PATH-COUNT: 0
- PESTER Passed=2 Failed=4 Skipped=0 NotRun=0 Total=6
- Failed set: exactly R1 (rewrites the transitive redirect in both positions), R2 (reports the rewritten application configuration in the written paths), R3 (carries the synchronised-redirect block in the body), R4 (exposes the synchronisation in the RedirectSync field and not in the per-project repair records)
- R5 (writes nothing on a second run over the repaired store) and R6 (leaves the stale redirect unchanged and writes nothing) passed
- R1 FAILED-MESSAGE contains `newVersion` (the store still carries newVersion="3.4.0.0")
- Right reason: the pre-change script has no unconditional sync pass and no RedirectSync result field.
- P1-T2 static checks: Grep `^\s+It '` count 6; temporary-file pattern count 0; `RedirectSync` appears only inside It blocks (lines 161-170), 0 inside the BeforeAll blocks.

## FAILED-TEST / FAILED-MESSAGE lines

```
FAILED-TEST: Repair-PackageManifestConsistency redirect synchronisation (issue 985).A transitive redirect left stale by an upgrade elsewhere, with no candidate upgrade supplied.rewrites the transitive redirect in both positions
FAILED-MESSAGE: Expected regular expression 'oldVersion="0\.0\.0\.0-3\.5\.0\.0"' to match '... <bindingRedirect oldVersion="0.0.0.0-3.4.0.0" newVersion="3.4.0.0" /> ... <bindingRedirect oldVersion="0.0.0.0-1.0.0.0" newVersion="1.0.0.0" /> ...', but it did not match.
FAILED-TEST: Repair-PackageManifestConsistency redirect synchronisation (issue 985).A transitive redirect left stale by an upgrade elsewhere, with no candidate upgrade supplied.reports the rewritten application configuration in the written paths
FAILED-MESSAGE: Expected 'X:\fixture\Test\app.config' to be found in collection $null, but it was not found.
FAILED-TEST: Repair-PackageManifestConsistency redirect synchronisation (issue 985).A transitive redirect left stale by an upgrade elsewhere, with no candidate upgrade supplied.carries the synchronised-redirect block in the body
FAILED-MESSAGE: Expected regular expression '## Binding redirects synchronised' to match '## Repairs applied No repairs were applied.', but it did not match.
FAILED-TEST: Repair-PackageManifestConsistency redirect synchronisation (issue 985).A transitive redirect left stale by an upgrade elsewhere, with no candidate upgrade supplied.exposes the synchronisation in the RedirectSync field and not in the per-project repair records
FAILED-MESSAGE: The property 'RedirectSync' cannot be found on this object. Verify that the property exists.
```
