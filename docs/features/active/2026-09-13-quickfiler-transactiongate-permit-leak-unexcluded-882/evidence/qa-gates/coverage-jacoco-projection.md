Timestamp: 2026-09-29T09-15 Command: pwsh -NoProfile -File "coverage\plan882-helper.ps1" -EvidenceDirectory docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates -StepArtifactName qa-coverage-test-run -NewTestName "BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing" ; inner collection: dotnet-coverage collect --output coverage/coverage.cobertura.xml --output-format cobertura --settings coverage/coverage.cobertura.xml.effective-coverage.config -- vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /ResultsDirectory:coverage/test-results /Logger:trx;LogFileName=mstest-coverage-run.trx /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /logger:console;verbosity=normal EXIT_CODE: 0 Scope: QuickFiler.Test.dll only. QuickFiler.Test is a test project outside the first-party coverage denominator (Get-KoverageProjectAllowlist drops every assembly whose name ends in .Test), so the figures are the first-party lines the QuickFiler.Test suite alone exercises; the repository-wide 80 percent line floor and 75 percent branch floor are NOT measured by this run, and the floor outcomes below are observations of the single-assembly denominator, not gates. Package-level JaCoCo projection of the post-processed Cobertura document (ConvertTo-JacocoPackageProjection, scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1); reconciliation against the document root totals passed (Assert-JacocoProjectionReconciliation).  ```xml <report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2295" covered="10459" />
    <counter type="BRANCH" missed="700" covered="2517" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="38975" covered="4449" />
    <counter type="BRANCH" missed="10088" covered="1181" />
  </package>
  <package name="TaskVisualization">
    <counter type="LINE" missed="1569" covered="0" />
    <counter type="BRANCH" missed="400" covered="0" />
  </package>
  <package name="SVGControl">
    <counter type="LINE" missed="1580" covered="274" />
    <counter type="BRANCH" missed="573" covered="65" />
  </package>
  <package name="ToDoModel">
    <counter type="LINE" missed="1823" covered="0" />
    <counter type="BRANCH" missed="508" covered="0" />
  </package>
  <package name="Tags">
    <counter type="LINE" missed="758" covered="0" />
    <counter type="BRANCH" missed="190" covered="0" />
  </package>
</report> ```
