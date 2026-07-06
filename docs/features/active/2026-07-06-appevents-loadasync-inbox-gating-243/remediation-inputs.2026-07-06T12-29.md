# Remediation Inputs: appevents-loadasync-inbox-gating (Issue #243)

Timestamp: 2026-07-06T12-29
Feature Folder: docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243
Policy Audit: docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/policy-audit.2026-07-06T12-29.md
Code Review: docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/code-review.2026-07-06T12-29.md
Feature Audit: docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/feature-audit.2026-07-06T12-29.md

## Remediation Required Findings

1. **C# coverage gate failure**
   - Finding: repository-wide C# coverage fell from 79.9234% baseline to 8.9566% final coverage. The policy threshold is 80%, and modified-file coverage must not regress relative to baseline.
   - Required behavior: produce baseline-comparable final C# coverage evidence that reports repository-wide line coverage at or above 80% and not below baseline, while preserving 100% changed executable line coverage for issue #243.
   - Files/artifacts:
     - `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/csharp-coverage-delta.2026-07-06T11-02.md`
     - `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/final-csharp-coverage.2026-07-06T11-02.md`
     - `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/post-refinement-verification.2026-07-06T12-26.md`
   - Verification commands:
     - `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot TaskMaster.Test -Configuration Debug -CoverageOutput docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/remediation-csharp-coverage.2026-07-06T12-29.cobertura.xml`
     - Parse baseline and final Cobertura XML and write a replacement coverage delta artifact under `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/`.

2. **Required C# coverage artifact path absent**
   - Finding: `artifacts/csharp/coverage.xml` was absent during feature review.
   - Required behavior: after a valid final C# coverage run, provide the required language coverage artifact path for review tooling and preserve the canonical feature-folder evidence copy.
   - Files/artifacts:
     - `artifacts/csharp/coverage.xml`
     - `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/remediation-csharp-coverage.2026-07-06T12-29.cobertura.xml`
   - Verification commands:
     - `Test-Path artifacts/csharp/coverage.xml`
     - XML parse of `artifacts/csharp/coverage.xml` confirming numeric line-rate values.

3. **Changed C# files exceed the 500-line limit**
   - Finding: `TaskMaster/AppGlobals/AppEvents.cs` and `TaskMaster.Test/AppGlobals/AppEventsTests.cs` are each 507 lines after the change.
   - Required behavior: reduce every changed production and test file to 500 lines or fewer without weakening issue #243 behavior or tests.
   - Files:
     - `TaskMaster/AppGlobals/AppEvents.cs`
     - `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs`
     - `TaskMaster.Test/AppGlobals/AppEventsTests.cs`
     - Optional new focused test file under `TaskMaster.Test/AppGlobals/` if needed to keep files cohesive and under limit.
   - Verification commands:
     - `(Get-Content TaskMaster/AppGlobals/AppEvents.cs).Count`
     - `(Get-Content TaskMaster.Test/AppGlobals/AppEventsTests.cs).Count`
     - Count any new or modified C# files and confirm each is `<= 500`.

4. **Evidence-location validator unavailable**
   - Finding: `python scripts/dev_tools/validate_evidence_locations.py --root .` failed because the script does not exist in this checkout; recursive search found no `validate_evidence_locations.py`.
   - Required behavior: restore, locate, or document the repository-approved evidence-location validator path, then run it successfully. If the correct path differs from review instructions, update the evidence artifact with the resolved command and result.
   - Files/artifacts:
     - Repository validator script location if restored.
     - `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/remediation-evidence-location-validation.2026-07-06T12-29.md`
   - Verification command:
     - `python scripts/dev_tools/validate_evidence_locations.py --root .` or the repository-approved equivalent.

## Do Not Do

- Do not move feature evidence out of `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/`.
- Do not weaken the acceptance criteria or change `issue.md` acceptance criteria text.
- Do not remove or weaken the issue #243 focused tests.
- Do not restore pre-readiness startup inbox processing in hooked-event `LoadAsync()`.
- Do not report PASS based only on changed-line coverage while repository-wide C# coverage remains below threshold or below baseline.
- Do not ignore the changed-file 500-line limit.
- Do not modify unrelated user or orchestrator changes.

## Required Final Verification

Run the C# toolchain in order after remediation:

```powershell
dotnet tool run csharpier format .
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors
vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:TaskMaster.Test.AppGlobals.AppEventsTests,TaskMaster.Test.AppGlobals.HookReadinessCoordinatorTests /InIsolation
vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot TaskMaster.Test -Configuration Debug -CoverageOutput docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/remediation-csharp-coverage.2026-07-06T12-29.cobertura.xml
git diff --check
```

Final remediation evidence must be written under `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/`.
