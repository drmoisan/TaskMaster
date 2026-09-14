# netstandard 2.0.0.0 Child-Domain Observation — [P4-T10]

Timestamp: 2026-09-14T11-43

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$m = @(Get-ChildItem -LiteralPath "TestResults/p4-harness" -Filter "p4-harness.trx" -Recurse)
Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
$trx = $m[0]
$x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
foreach ($r in @($x.TestRun.Results.UnitTestResult)) {
if ($r.testName -eq "NegativeControl_Netstandard20Observation_IsRecorded") {
foreach ($line in @(($r.Output.StdOut -split "`r?`n"))) {
if ($line.StartsWith("NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=")) { Write-Output $line } } } }
'
```

The `Set-Location` prefix is mechanically necessary and measurement-neutral: this executor was launched
without worktree isolation, so its inherited working directory is a different checkout and every
repository-relative path in the plan would otherwise resolve into the wrong tree. It is disclosed here
rather than omitted.

This task carries no pre-run TRX removal span, because it reads the TRX that `[P4-T3]` wrote earlier in
this same phase. Adding a removal span here would delete the file this task must consume.

EXIT_CODE: 0

Output Summary:

```
TRX_MATCH_COUNT=1
NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=LOADED
```

Acceptance Condition: MET. `TRX_MATCH_COUNT=1`, and the artifact carries exactly one
`NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=` line with a non-empty value. The value itself is an
observation and not a gate; either value would have been recorded and neither blocks.

## What this narrows, and what it does not settle

The observation is taken in the INSTALLER-FREE second child domain, rooted at the `QuickFiler.Test` build
output directory. In that domain, with no resolve handler subscribed, a full display name at
`Version=2.0.0.0` loads. The sibling observation in the same domain and the same run,
`NegativeControl_WithoutInstall_Netstandard21Throws`, records a `FileNotFoundException` for the same
identity at `Version=2.1.0.0`.

This narrows the open risk that `## R2` records. The maintainer's reproduced production trace shows the
chain falling back to `netstandard, Version=2.0.0.0` and failing there as well. That did not reproduce
here: the `2.0.0.0` leg succeeded in a clean child domain on this machine. The difference is therefore
localised to the add-in `AppDomain` in the live Outlook host rather than to the machine's assembly cache.

This does not settle why the `2.0.0.0` frame appeared in the production trace. Nothing in this repository
accounts for it and this measurement does not explain it. The remedy does not depend on the answer: ladder
rung 3 loads the facade from the runtime directory by absolute path and bypasses cache lookup entirely.
Issue #879 must not be reported as having explained the `2.0.0.0` leg.
