# tesseract-engine-initialization-failure (Issue #209)

- Date captured: 2026-06-19
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tesseract-engine-initialization-failure/ (Issue #209)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #209
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/209
- Last Updated: 2026-06-19
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

- [ ] Unit coverage areas: tessdata path resolution as a pure helper; engine initialization behind a seam mockable with Moq (no live Tesseract engine in tests).
- [ ] Integration scenario to retest: process an image-bearing item and confirm OCR initializes once without exception, or skips cleanly when OCR is unavailable.
- [ ] Manual verification notes: confirm the resolved `tessdata` path exists in the deployed working directory and contains the expected language files.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch