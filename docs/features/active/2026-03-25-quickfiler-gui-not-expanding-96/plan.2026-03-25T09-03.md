# 2026-03-25-quickfiler-gui-not-expanding (Plan)

- **Issue:** #96
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-25T11:10:50-04:00
- **Status:** Completed
- **Version:** 0.1
- **Work Mode:** minor-audit

Requirements source: `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/issue.md`

## Root Cause Summary

`QfcItemController.RegisterFocusAsyncActions()` does not add `Keys.Right` to
`_kbdHandler.KeyActionsAsync`. The async migration left the Right-key handler
commented out. When the Right arrow is pressed, `KeyDownTaskAsync` finds no
match, does not suppress the key press, and WinForms routes it to focused controls
displaying the sender's mailto: address.

Fix: add `Keys.Right → ToggleExpansionAsync(On)` to `RegisterFocusAsyncActions()`
and remove it in `UnregisterFocusAsyncActions()`.

---

### Phase 0 — Policy Read + Baseline Capture

- [x] [P0-T1] Read mandatory policy files (general-code-change-policy, csharp-code-change-policy, general-unit-test-policy, csharp-unit-test-policy) in policy-compliance order and save a policy-read evidence artifact.
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/phase0-instructions-read.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Policy Order:` listing all four policy files read in order
    - Explicit list of filenames read

- [x] [P0-T2] Run the formatter to establish a format baseline and save the artifact.
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-format.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: dotnet tool run csharpier format .`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming no files were changed

- [x] [P0-T3] Run the lint/analyzer build to establish a lint baseline and save the artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-lint.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming build succeeded with 0 errors

- [x] [P0-T4] Run the nullable/type-check build to establish a nullable baseline and save the artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-nullable.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming build succeeded with 0 errors

- [x] [P0-T5] Run the targeted test filter to establish a test baseline and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_KeyboardRegistration"`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-test.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_KeyboardRegistration"`
    - `EXIT_CODE: <recorded integer>`
    - `Output Summary:` noting that `QfcItemController_KeyboardRegistration` tests do not yet exist at baseline (0 tests found is the expected baseline state)

- [x] [P0-T6] Run the full QuickFiler.Test suite with coverage enabled to establish a numeric coverage baseline and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-coverage.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
    - `EXIT_CODE: 0`
    - `Output Summary:` including the numeric QuickFiler.Test line-coverage percentage reported by vstest (e.g., `Lines covered: XX%`)

---

### Phase 1 — Regression Tests + Implementation Fix

- [x] [P1-T1] [expect-fail] Add test method `RegisterFocusAsyncActions_RightArrowKey_RegisteredInKeyActionsAsync` to `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` — asserts `Keys.Right` is present in `_kbdHandler.KeyActionsAsync` after calling `RegisterFocusAsyncActions()`. Run it before the fix and confirm it fails.
  - Precondition: Phase 0 all tasks complete; `QfcItemControllerTests.cs` exists at `QuickFiler.Test/Controllers/QfcItemControllerTests.cs`.
  - Acceptance:
    1. Method `RegisterFocusAsyncActions_RightArrowKey_RegisteredInKeyActionsAsync` is present in `QfcItemControllerTests.cs`.
    2. Run targeted command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~RegisterFocusAsyncActions_RightArrowKey_RegisteredInKeyActionsAsync"` exits with nonzero `EXIT_CODE`.
    3. Output contains a failing assertion excerpt referencing `Keys.Right` or `KeyActionsAsync`.
    4. Evidence artifact `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/regression-testing/regression-fail-before.md` exists and contains:
       - `Timestamp: <ISO-8601>`
       - `Command: <exact command from step 2>`
       - `EXIT_CODE: <nonzero integer>`
       - `Output Summary:` including a verbatim excerpt of the failing assertion

- [x] [P1-T2] [expect-fail] Add test method `UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowKey` to `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` — asserts `Keys.Right` is absent from `_kbdHandler.KeyActionsAsync` after calling `UnregisterFocusAsyncActions()`. Run it before the fix and confirm it fails.
  - Precondition: P1-T1 complete.
  - Acceptance:
    1. Method `UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowKey` is present in `QfcItemControllerTests.cs`.
    2. Run targeted command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowKey"` exits with nonzero `EXIT_CODE`.
    3. Output contains a failing assertion excerpt referencing `Keys.Right` or `KeyActionsAsync`.
    4. Append run record to `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/regression-testing/regression-fail-before.md` with:
       - `Timestamp: <ISO-8601>`
       - `Command: <exact command from step 2>`
       - `EXIT_CODE: <nonzero integer>`
       - `Output Summary:` including a verbatim excerpt of the failing assertion

- [x] [P1-T3] In `QuickFiler/Controllers/QfcItemController.cs`, add the `Keys.Right` registration to `RegisterFocusAsyncActions()`.
  - Precondition: P1-T2 complete.
  - Change: Inside `RegisterFocusAsyncActions()` (near line 1335), add:
    ```
    _kbdHandler.KeyActionsAsync.Add(ItemHelper.EntryId, Keys.Right, (x) => this.ToggleExpansionAsync(Enums.ToggleState.On));
    ```
  - Acceptance: `RegisterFocusAsyncActions()` in `QfcItemController.cs` contains a line adding `Keys.Right` to `_kbdHandler.KeyActionsAsync` with a lambda calling `ToggleExpansionAsync`.

- [x] [P1-T4] In `QuickFiler/Controllers/QfcItemController.cs`, add (or uncomment) the `Keys.Right` removal in `UnregisterFocusAsyncActions()`.
  - Precondition: P1-T3 complete.
  - Change: Inside `UnregisterFocusAsyncActions()` (near line 1465), add or uncomment:
    ```
    _kbdHandler.KeyActionsAsync.Remove(ItemHelper.EntryId, Keys.Right);
    ```
  - Acceptance: `UnregisterFocusAsyncActions()` in `QfcItemController.cs` contains a line removing `Keys.Right` from `_kbdHandler.KeyActionsAsync`.

---

### Phase 2 — Final QA Loop

- [x] [P2-T1] Run the formatter as the first QA gate and save the artifact.
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/qa-format.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: dotnet tool run csharpier format .`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming no files were changed; if files were changed, fix and restart the QA loop from P2-T1.

- [x] [P2-T2] Run the lint/analyzer build and save the QA lint artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/qa-lint.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming build succeeded with 0 errors; if errors present, fix and restart QA loop from P2-T1.

- [x] [P2-T3] Run the nullable/type-check build and save the QA nullable artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/qa-nullable.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming build succeeded with 0 errors; if errors present, fix and restart QA loop from P2-T1.

- [x] [P2-T4] Run the full QuickFiler.Test suite with coverage enabled as the final QA test gate and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/qa-test.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming all tests passed; explicitly listing that both `RegisterFocusAsyncActions_RightArrowKey_RegisteredInKeyActionsAsync` and `UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowKey` are in the passed set; and including the numeric post-change QuickFiler.Test line-coverage percentage (e.g., `Lines covered: XX%`) for comparison against the P0-T6 baseline; if any test fails, fix and restart QA loop from P2-T1.
