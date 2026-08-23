Timestamp: 2026-08-22T14-17

Command: pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-test-form1-live-form-491\evidence\qa-gates\coverage-postchange-remediation.cobertura.xml

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 6438, Passed: 6438.
- Cobertura file exists at
  `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/coverage-postchange-remediation.cobertura.xml`.
- Root `<coverage>` attributes: `lines-covered=53392`, `lines-valid=62401`, `line-rate=0.855627`.
- `Assert-CoberturaLineCoverageThreshold` did NOT throw (85.5627% is comfortably above the 80% floor).
  This is not the documented pre-existing HALT condition; the capture and Koverage post-processing
  both completed normally.
- Koverage post-processing confirmed to have run: 0 `//class` nodes have a `filename` attribute
  containing `:` (all rewritten to workspace-relative paths).

Diagnostic note (recorded here because it directly affects the P3-T3 comparison): this reading
(53392 lines-covered) is 10 lines below the primary plan's baseline (53402 lines-covered, same
62401 lines-valid, captured at `evidence/baseline/coverage-baseline.cobertura.xml`,
2026-08-22T13-13). A per-class diff against the baseline shows the shortfall in this capture comes
from `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs|UtilitiesCS.OlTableExtensions`
(base=271, post=267, diff=-4) and `UtilitiesCS\HelperClasses\SegmentStopWatch.cs|UtilitiesCS.HelperClasses.SegmentStopWatch`
(base=109, post=103, diff=-6). Neither file is touched by this change, and no `QuickFiler.Test`
package or class appears anywhere in the diff (consistent with spec.md's documented harness
exclusion of `.Test`-suffixed assemblies from both instrumentation and the Koverage allowlist).

Three additional capture attempts were run as a diagnostic (not as the official artifact) to
determine whether this shortfall was one-off measurement noise or a stable environmental condition:
- Attempt 2: lines-covered=53389 (diff -13 from baseline), unrelated classes affected:
  `SegmentStopWatch.cs` (-6, same as attempt 1), `QuickFiler\Controllers\EfcHomeController.cs` (-1),
  `UtilitiesCS\Interfaces\IWinForm\PropertyStore.cs` (-6).
- Attempt 3: aborted — `dotnet-coverage`/vstest returned a non-zero exit from one unrelated flaky
  test failure before Koverage post-processing ran; the resulting file was discarded per the
  same-spirit "discard an invalid/incomplete capture" guidance used for the documented threshold-throw
  HALT case, and was not used for any comparison.
- Attempt 4: lines-covered=53392 (identical to attempt 1), unrelated classes affected:
  `SegmentStopWatch.cs` (-6, same as attempts 1 and 2), `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs` (-4).

`SegmentStopWatch.cs` shows the identical -6 shortfall in all three valid attempts (1, 2, 4), while
the second contributing file differs each time. This is consistent with a reproducible,
environment-dependent coverage gap in an unrelated timing-sensitive production class, present at the
time of this remediation cycle's session but not at the time the baseline was captured
(a different session, ~65 minutes earlier), rather than with anything caused by deleting the dead
`QfcFormViewerDerived` nested class. The diagnostic attempts' raw coverage XML files were deleted
after their headline numbers were recorded above, so only the canonical
`coverage-postchange-remediation.cobertura.xml` (this task's required artifact) remains on disk.

This capture (EXIT_CODE 0, valid Cobertura file, no threshold throw) satisfies this task's own
acceptance condition. The below-baseline reading it reports is carried forward honestly into P3-T3.
