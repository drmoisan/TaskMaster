# Issue #439 Pre-Remediation Coverage Record

Timestamp: 2026-08-24T19:44:30-04:00
Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.cobertura.xml`
Status: `PRE_REMEDIATION_NOT_FINAL`
Output Summary: The coverage collector produced a parseable Cobertura XML with repository line rate `70.12358090380796%`. A duplicate retry finalized the XML, while the initial collector remained orphaned after five minutes and was terminated with its known child `vstest.console.exe`; it did not produce a reliable command-finalization result. This record is not P4-T5 acceptance evidence and must not be used for coverage thresholds because the router accessibility remediation follows.
