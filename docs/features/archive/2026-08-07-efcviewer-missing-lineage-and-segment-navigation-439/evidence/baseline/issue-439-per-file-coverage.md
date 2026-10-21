Timestamp: 2026-08-24T18:24:20.1245884-04:00 Command: & { [xml]$c=Get-Content -Raw -LiteralPath 'docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/baseline/issue-439-baseline.cobertura.xml'; $f=@('QuickFiler/Controllers/EfcFormController.cs','QuickFiler/Controllers/BreadcrumbBridgeRouter.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs'); $r=@('Timestamp: '+(Get-Date -Format 'o'),'Command: '+$MyInvocation.Line,'EXIT_CODE: 0','Output Summary: per-file baseline coverage'); foreach($p in $f){$x=@($c.coverage.packages.package.classes.class|Where-Object {$_.filename.Replace('\\','/') -eq $p}); $lines=@($x|ForEach-Object {$_.lines.line}); if($x.Count -eq 0 -or $lines.Count -eq 0){$r+=($p+': REMEDIATION_REQUIRED missing source entry or line data')}else{$covered=@($lines|Where-Object {[int]$_.hits -gt 0}).Count; $percentage=100.0*$covered/$lines.Count; $r+=($p+': '+[math]::Round($percentage,2)+'%')}}; Set-Content -LiteralPath 'docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/baseline/issue-439-per-file-coverage.md' -Value $r } EXIT_CODE: 0 Output Summary: per-file baseline coverage
QuickFiler/Controllers/EfcFormController.cs: REMEDIATION_REQUIRED missing source entry or line data
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs: REMEDIATION_REQUIRED missing source entry or line data
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs: REMEDIATION_REQUIRED missing source entry or line data
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs: REMEDIATION_REQUIRED missing source entry or line data
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: REMEDIATION_REQUIRED missing source entry or line data
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs: REMEDIATION_REQUIRED missing source entry or line data
UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: REMEDIATION_REQUIRED missing source entry or line data
UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs: REMEDIATION_REQUIRED missing source entry or line data

Supplemental extraction (post-plan command): normalizes Cobertura single-backslash filenames with -replace '\\', '/'; the literal plan-command result above is retained unchanged.
Timestamp: 2026-08-24T18:28:17.6487476-04:00
Command: pwsh -NoProfile -Command "[xml]$coverage=Get-Content -Raw issue-439-baseline.cobertura.xml; normalize $_.filename with -replace '\\', '/'; append per-file baseline coverage"
EXIT_CODE: 0
Output Summary: supplemental normalized per-file baseline coverage
QuickFiler/Controllers/EfcFormController.cs: REMEDIATION_REQUIRED missing source entry or line data after single-backslash normalization
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs: 97.87% (276/282 lines)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs: 98.02% (99/101 lines)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs: 100% (114/114 lines)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: 100% (23/23 lines)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs: 95.65% (88/92 lines)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: REMEDIATION_REQUIRED missing source entry or line data after single-backslash normalization
UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs: 96.9% (125/129 lines)

Baseline-absent source diagnostic (read-only):
Timestamp: 2026-08-24T18:29:44.0023960-04:00
Command: Select-String issue-439-baseline.cobertura.xml for EfcFormController and BreadcrumbDocumentAssets; inspect coverage.config, TaskMaster.runsettings, Invoke-MSTestWithCoverage.ps1, and source attributes.
EXIT_CODE: 0
Output Summary: no filename variant exists for either target after inspecting Cobertura class filename/name entries and collector configuration.
QuickFiler/Controllers/EfcFormController.cs: REMEDIATION_REQUIRED numeric baseline unavailable. Source line 27 declares [ExcludeFromCodeCoverage]; the collector honors this instrumentation exclusion, so no compliant nontracked command/filter can surface a numeric entry without changing tracked production coverage policy/source.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: REMEDIATION_REQUIRED numeric baseline unavailable. The source is a static class containing const CSS/JS fields only; its values are compile-time inlined and the Cobertura XML has no class or line entry. coverage.config and TaskMaster.runsettings exclude only third-party/test modules, and Invoke-MSTestWithCoverage post-processing only removes third-party packages; no repository-supported filter can create executable coverage lines for this source without changing production code.
Collector behavior: Invoke-MSTestWithCoverage uses dotnet-coverage collect with coverage.config and the full discovered test-assembly list, then only normalizes absolute paths, injects sources, and strips third-party packages. The absence is not caused by a filename separator or filter mismatch.
Baseline/delta limitation: numeric comparison is available for the six emitted target sources only. The two entries above must remain REMEDIATION_REQUIRED until a separately authorized production/policy change makes them instrumentable.
