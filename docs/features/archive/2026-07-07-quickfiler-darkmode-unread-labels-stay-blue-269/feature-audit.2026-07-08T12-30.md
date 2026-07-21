# Feature Audit — Issue #269 (quickfiler-darkmode-unread-labels-stay-blue), CORRECTED FIX

- Timestamp: 2026-07-08T12-30
- Work mode: `minor-audit`
- AC source: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`, `## Acceptance Criteria` section (AC1-AC5), and only that section, per `acceptance-criteria-tracking`.

**SUPERSEDES** `feature-audit.2026-07-08T11-30.md`, which evaluated a since-reverted change against an earlier, incorrect version of the AC source text. `issue.md` has since been rewritten for the corrected root cause; this audit evaluates the current AC1-AC5 text against the current (corrected) diff.

## Summary

All five acceptance criteria are satisfied by the branch diff and its accompanying evidence, independently re-verified by this reviewer. The fix is a four-argument positional rotation in `QfcThemeHelper.cs`'s `LightNormal`/`LightActive` theme definitions, confirmed by this reviewer to exactly match the pre-refactor (correct) values from `44bfdf204~1`. A new deterministic MSTest/FluentAssertions regression test asserts the exact corrected colors and confirms Dark themes remain unaffected. The full C# toolchain passed cleanly in this reviewer's own independent re-run.

## Scope and Baseline

- Base branch: `origin/main` (tip `954c78407f5b1fb0b163f982885826327e346b3d`); merge-base with `HEAD` = `254388adeb4e189e8c3781b7c2096a7b4b208980` = current `HEAD` (all feature work is uncommitted working-tree state on top of this merge-base; see `policy-audit.2026-07-08T12-30.md` §1).
- In-scope production file: `QuickFiler/Helper Classes/QfcThemeHelper.cs`.
- In-scope test file: `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`.
- Confirmed via `git diff HEAD --numstat` that no other production or test file changed on this branch, and confirmed via targeted `git diff HEAD -- <path>` that the previously-reviewed (reverted) `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` and `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs` show zero diff, and that the unrelated `ItemViewer.Designer.cs`/`ItemViewer.resx` DPI-rescale churn also shows zero diff.

## Acceptance Criteria Inventory

| ID | Criterion (verbatim from `issue.md`) |
|---|---|
| AC1 | In Light mode, `_lblSender`/`_lblSubject` on `ItemViewer` render dark text on a light background for read items and blue text on a light background for unread items. No Light-mode item shows light text on a black or blue background. |
| AC2 | Dark mode remains correct: `_lblSender`/`_lblSubject` render light text on a black background (goldenrod unread accent). No regression to the Dark themes. |
| AC3 | Root cause is corrected with a minimal, targeted change (no opportunistic refactor). The change is confined to the `LightNormal`/`LightActive` `CreateTheme(...)` mail-label arguments in `QuickFiler/Helper Classes/QfcThemeHelper.cs`, restoring the pre-refactor values. |
| AC4 | A deterministic regression test reproduces the defect (fails before the fix, passes after) using seams only — `QfcThemeHelper.SetupThemes` with a handle-less control set; no live Outlook, no COM, no temporary files. The test asserts the corrected Light colors and that Dark remains correct. |
| AC5 | The full C# toolchain (CSharpier -> analyzers -> nullable -> MSTest with coverage) passes; full impacted suite 4663/4663 with no regression. |

## Acceptance Criteria Evaluation

### AC1 — PASS

Evidence:
- `Theme.cs:356-374` (`SetMailRead()`) confirms a direct, unconditional mapping: `_lblSender.BackColor = _mailReadBackColor; _lblSender.ForeColor = _mailReadForeColor;` (and identically for `_lblSubject`), and `Theme.cs:394-411` (`SetMailUnread()`) confirms the equivalent mapping for `_mailUnreadBackColor`/`_mailUnreadForeColor`. This means the `MailReadForeColor`/`MailReadBackColor`/`MailUnreadForeColor`/`MailUnreadBackColor` dictionary values asserted by the new test are a direct, 1:1, deterministic proxy for the exact rendered `ForeColor`/`BackColor` AC1 describes — not an indirect or approximated signal.
- The new test `SetupThemes_LightThemes_MailLabelColorsAreDarkTextOnLightBackground` asserts, for `LightNormal`: `MailReadForeColor == SystemColors.ControlText` (dark text), `MailReadBackColor == SystemColors.Control` (light bg), `MailUnreadForeColor == Color.MediumBlue` (blue text), `MailUnreadBackColor == SystemColors.Control` (light bg) — i.e., no black or blue background for any Light-mode item, exactly as AC1 requires. The equivalent four assertions are made for `LightActive` against `Color.LightCyan` as the light background.
- This reviewer independently re-ran this test against a freshly built assembly: `Passed SetupThemes_LightThemes_MailLabelColorsAreDarkTextOnLightBackground [130 ms]` / `[< 1 ms]` on two separate runs (`Total tests: 1, Passed: 1` and, within the full `QfcThemeHelperTests` class run, `Total tests: 10, Passed: 10`).
- This reviewer independently confirmed, via `git show 44bfdf204~1:"QuickFiler/Helper Classes/QfcThemeHelper.cs"`, that these four values exactly match the last known-correct pre-refactor state for both `LightNormal` and `LightActive`.
- Not independently reproduced: a live-Outlook visual observation of the QuickFiler window. This repository's No-COM architecture direction does not require live-Outlook manual verification for this class of fix, and the deterministic dictionary-to-label-property mapping confirmed above closes the gap between "theme dictionary is correct" and "rendered label colors are correct" that would otherwise justify a PARTIAL grade. Graded PASS (not PARTIAL) because, unlike the superseded 11-30 review's AC1 (which depended on an async/probe-fault code path only reachable via a live render pipeline), this AC1's claim is fully and directly verified by the `SetMailRead()`/`SetMailUnread()` assignment logic plus the theme-dictionary test — there is no remaining code path between the tested value and the described visual outcome that this review has not inspected.

### AC2 — PASS

Evidence:
- The new test asserts, for `DarkNormal`: `MailReadForeColor == Color.WhiteSmoke`, `MailReadBackColor == Color.Black`, `MailUnreadForeColor == Color.Goldenrod`, `MailUnreadBackColor == Color.Black` — matching AC2's literal description exactly.
- `git diff HEAD -- "QuickFiler/Helper Classes/QfcThemeHelper.cs"` shows zero changes to the `DarkNormal`/`DarkActive` `CreateTheme(...)` blocks (lines 174-235) — confirmed by this reviewer via direct diff-hunk inspection, not merely by the executor's own claim.

### AC3 — PASS

Evidence:
- `git diff HEAD --numstat -- '*.cs'` confirms exactly one production file changed: `QuickFiler/Helper Classes/QfcThemeHelper.cs` (+10/-2).
- Diff-hunk inspection (this reviewer, independent of the executor's own scope evidence) confirms the change is confined to the mail-label argument positions within the `LightNormal` and `LightActive` `CreateTheme(...)` calls plus four comment lines per block — no other statement, method, or theme block was touched.
- This reviewer's `git show 44bfdf204~1` comparison (see AC1) directly confirms the restored values equal the pre-refactor values, satisfying the criterion's specific claim of "restoring the pre-refactor values."
- `code-review.2026-07-08T12-30.md` independently confirms no opportunistic refactor.

### AC4 — PASS

Evidence:
- Deterministic seams confirmed: `CreateControlSet()` constructs plain in-process WinForms controls (`new Label()`, `new TextBox()`, `new ComboBox()`, etc., none of which force `.Handle` creation) plus `FormatterServices.GetUninitializedObject<WebView2>()` (bypasses the WebView2 constructor entirely) and a `Mock<IUiDispatcher>`. No live Outlook `MailItem`, no COM object, and no `Path.GetTemp*`/`File.WriteAllText` appear in the new test or the reused helper.
- Fail-before/pass-after confirmed: `evidence/qa-gates/corrected-fix-verification.2026-07-08T12-15.md` records `FAILED, EXIT 1` against buggy-HEAD code and `PASSED, EXIT 0` against fixed code, both against real full-solution builds. This reviewer independently re-ran the passing test against a freshly built assembly (see AC1) and confirms the pass; the buggy-HEAD (pre-fix) state was not re-executed by this reviewer (it would require reverting the working tree), so the "fails before" half of this claim rests on the executor's own recorded evidence, cross-checked for plausibility against this reviewer's independent git-history value comparison (the pre-fix values are demonstrably the transposed values, so a test asserting the correct values would necessarily fail against them).
- Assertion content matches the AC's literal requirement: the test asserts the corrected Light colors (`LightNormal`/`LightActive`) and confirms Dark remains correct (`DarkNormal`), exactly as AC4 specifies.

### AC5 — PASS

Evidence:
- Full toolchain, independently re-run by this reviewer in order: CSharpier (`csharpier check`, clean, EXIT 0) -> analyzers (`MSBuild.exe ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, EXIT 0, 0 errors) -> nullable (`MSBuild.exe ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`, EXIT 0, 0 errors) -> MSTest with coverage (`vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`, `Total tests: 4663. Passed: 4663.`, EXIT 0) — see `policy-audit.2026-07-08T12-30.md` §4 for full command transcripts.
- No regression: a second independent test run (without `/EnableCodeCoverage`) showed one unrelated pre-existing flaky failure (`TryAddValuesAsync_UpdatesExistingValue`, an OneDrive/dictionary-write test unconnected to theme code); the authoritative clean run is the 4663/4663 result above, matching the executor's own recorded evidence.
- No coverage regression: this reviewer independently generated a fresh Cobertura coverage artifact from the passing run above and confirmed the changed class (`QuickFiler.QfcThemeHelper`) line-rate at 96.47%, first-party-aggregate coverage at 88.88%, both clearing the applicable CLAUDE.md 80% floor and the `general-unit-test.md` 85% floor; see `policy-audit.2026-07-08T12-30.md` §5 for full detail and the pre-existing policy-document-conflict note (which this fix does not need to resolve, since it clears both readings).

## Acceptance Criteria Check-off

- [x] AC1
- [x] AC2
- [x] AC3
- [x] AC4
- [x] AC5

All five items are already checked `[x]` in `issue.md` (verified by direct read of the file, lines 69-73). This review's own independent evaluation confirms all five as PASS; no check-off action was required.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

## Overall Verdict

**PASS.** All five acceptance criteria are satisfied and independently re-verified by this reviewer against the corrected fix. No blocking gaps. Outstanding, non-blocking follow-up items: a live-Outlook manual visual spot-check (recommended, not required, per AC1's evaluation above) and opening a PR so CI can run the required checks (per `policy-audit.2026-07-08T12-30.md` §7).
