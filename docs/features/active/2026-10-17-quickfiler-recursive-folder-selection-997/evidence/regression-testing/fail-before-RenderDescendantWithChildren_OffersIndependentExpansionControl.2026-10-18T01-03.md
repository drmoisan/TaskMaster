Timestamp: 2026-10-18T01-03
Command: vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /TestCaseFilter:Name=RenderDescendantWithChildren_OffersIndependentExpansionControl /ResultsDirectory:SCRATCH/issue-997-fail-before-results
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:   Failed RenderDescendantWithChildren_OffersIndependentExpansionControl [144 ms]
  Error Message:
   Expected html to match regex "(?s)if\s*\(\s*child\.hasChildren\s*\).*?selectorDescendantToggle", but "<!DOCTYPE html>
Total tests: 1
     Failed: 1

