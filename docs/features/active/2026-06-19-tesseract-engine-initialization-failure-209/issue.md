# tesseract-engine-initialization-failure (Issue #209)

- Date captured: 2026-06-19
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tesseract-engine-initialization-failure/ (Issue #209)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #209
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/209
- Last Updated: 2026-06-19
- Work Mode: minor-audit

## Summary

`UtilitiesCS.EmailIntelligence.ImageStripper` fails to initialize the Tesseract OCR engine on every image-bearing mail item it processes, logging `ERROR ... Failed to initialise tesseract engine.` and throwing `Tesseract.TesseractException`. Image OCR is non-functional, and each failed attempt adds latency and exception noise to inbox-item processing.

## Environment

- OS/version: Windows; Outlook desktop (`outlook.exe` host)
- Runtime: .NET Framework Outlook VSTO add-in (TaskMaster), STA `VSTA_Main` thread
- Command/flags used: Normal add-in startup; inbox catch-up processing of unprocessed mail items
- Data source or fixture: Live inbox mail items containing images; bundled `Tesseract.dll` and tessdata language files

## Steps to Reproduce

1. Launch Outlook with the TaskMaster add-in loaded.
2. Allow `AppEvents.ProcessNewInboxItemsAsync` to process unprocessed inbox items that contain images.
3. Observe a `TesseractException` and the `Failed to initialise tesseract engine.` error for each such item.

## Expected Behavior

The Tesseract engine initializes once from a valid tessdata path, OCR runs on image content, and no per-item initialization failure is raised. If OCR is intentionally unavailable, the code should skip OCR cleanly without throwing.

## Actual Behavior

For each image-bearing item, the following is logged and thrown:

```
Exception thrown: 'Tesseract.TesseractException' in Tesseract.dll
ERROR UtilitiesCS.EmailIntelligence.ImageStripper - Failed to initialise tesseract engine.. See https://github.com/charlesw/tesseract/wiki/Error-1 for details.
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: the `TesseractException` and `ImageStripper` error lines above, observed repeatedly during the 2026-06-19 startup capture (issue #207 diagnostic run). The referenced wiki page (Error-1) documents a missing/incorrect `tessdata` path or missing language data.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Image OCR within the email-intelligence pipeline is non-functional. Each failed initialization adds processing latency and exception noise during inbox catch-up, which also complicates diagnosis of the unrelated startup stall.

## Suspected Cause / Notes

Per the Tesseract "Error-1" guidance, the engine cannot locate a valid `tessdata` directory or required language data at the deployed runtime location, or the engine is re-initialized per item rather than once. Candidate remedies: resolve the `tessdata` path relative to the deployed assembly location, verify language files are included in the VSTO/ClickOnce payload, and initialize the engine once behind an injectable seam.

Files to inspect:
- `UtilitiesCS/EmailIntelligence/ImageStripper.cs`
- Tesseract/tessdata deployment configuration

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: tessdata path resolution as a pure helper; engine initialization behind a seam mockable with Moq (no live Tesseract engine in tests).
- [ ] Integration scenario to retest: process an image-bearing item and confirm OCR initializes once without exception, or skips cleanly when OCR is unavailable. (Deferred — see Scope Note below.)
- [ ] Manual verification notes: confirm the resolved `tessdata` path exists in the deployed working directory and contains the expected language files. (Deferred — see Scope Note below.)

## Scope Note (2026-07-18)

This cycle scopes the fix to the **test-isolation gap** this bug causes, not the full live-Outlook OCR runtime fix. Root cause confirmed via CI log analysis and codebase research: `ImageStripper.extract_text` (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs`) constructs a real, unseamed `Tesseract.TesseractEngine` on every call, with no injectable interface. Two tests in `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` (`Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken`, `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken`) exercise this live engine directly. Because `%LOCALAPPDATA%\TaskMaster\tessdata\eng.traineddata` is never provisioned by build/restore/first-run code (confirmed: the checked-in `UtilitiesCS/Resources/eng.traineddata` is wired as plain `<None Include=...>` with no `CopyToOutputDirectory`), these two tests fail to initialize Tesseract on every machine/CI runner that lacks that manual prerequisite — and the repeated failed native engine construction is the most evidence-consistent explanation for the ~60-test cascading failure cluster observed in CI (all test assemblies share one `vstest.console.exe .../InIsolation` process; native/process-wide resources are not isolated per-AppDomain the way managed statics are).

This cycle's fix: introduce an injectable seam for the OCR engine, wire `ImageStripper` to use it (default implementation preserves current production behavior), and update the two named tests to use a mock instead of a live engine — eliminating the external-dependency policy violation (`.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md` Deterministic Test Rules) and removing the CI flakiness trigger. The live-Outlook OCR initialization/tessdata-deployment problem itself (this issue's original `Summary`/`Steps to Reproduce`) remains open as a separate follow-up; it requires a live Outlook/VSTO environment to verify and is out of scope for this test-isolation-focused cycle.

## Acceptance Criteria

- [x] AC1: `ImageStripper` obtains OCR text extraction through an injectable seam (interface), not a directly-constructed `Tesseract.TesseractEngine`. Default/production behavior is unchanged (still uses the real Tesseract engine when no fake is injected).
- [x] AC2: `Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken` and `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken` in `ImageStripper_Tests.cs` no longer construct or depend on a live `Tesseract.TesseractEngine`; they inject a Moq-based fake through the new seam instead.
- [x] AC3: Full MSTest suite run (`vstest.console.exe` across all first-party `*.Test.dll` assemblies) shows zero occurrences of `Failed loading language 'eng'` / `Error opening data file ... tessdata` in the output.
- [x] AC4: No other test in the suite newly fails or newly passes as a result of this change (no regression, no incidental masking).
- [x] AC5: CSharpier format, .NET analyzer build, and nullable build all pass with zero errors.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch