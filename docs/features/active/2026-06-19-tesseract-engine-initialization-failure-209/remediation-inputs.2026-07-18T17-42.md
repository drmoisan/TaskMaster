# Remediation Inputs — tesseract-engine-initialization-failure (Issue #209)

- Branch: `bug/tesseract-engine-initialization-failure-209`
- Base: `main` @ `a4977216467c6a275648e6ce134adf847693fc6a`
- Timestamp: 2026-07-18T17-42
- Referenced artifacts: `policy-audit.2026-07-18T17-42.md` (`## 3. Coverage Verification` / `## 3.2`), `code-review.2026-07-18T17-42.md` (Findings Table, Medium-severity row)

## Remediation-Required Finding

**Severity: Blocking.**

`UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs` (new file, added by this branch) has 0% line coverage (0 of 13 executable lines hit) in the post-change Cobertura evidence at `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/coverage-final.cobertura.xml`. This fails:

- `.claude/rules/general-unit-test.md` / `.claude/rules/quality-tiers.md`: uniform 85% line / 75% branch floor applied to new code.
- `CLAUDE.md` UT2: "Any new modules, classes, or methods added must target >= 90% coverage."

The class is not covered by any of the three enumerated exemption categories in CLAUDE.md UT2 (VSTO add-in lifecycle classes; WinForms Designer code; Outlook-Interop event handlers in `TaskVisualization`/`QuickFiler`/`TaskMaster`/`ToDoModel`/`Tags`). It depends on the third-party native `Tesseract.TesseractEngine`, not on Outlook COM/Interop types, so no existing exemption applies as written.

## Root Cause

`TesseractOcrTextExtractor.ExtractText(Bitmap bitmap)` interleaves two kinds of logic in one method body:

1. **Testable, pure logic:** formatting the `tessdataPath` string from `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)`.
2. **Genuinely untestable logic (without a live environment):** constructing a native `TesseractEngine`, calling `Process(bitmap)`, and calling `GetText()`.

Because both are in the same method, the entire method — including the testable path-formatting portion — is reported as 0% covered. The issue's own "Proposed Fix / Validation Ideas" section in `issue.md` explicitly named "tessdata path resolution as a pure helper" as a target unit-coverage area; this extraction was not performed during Phase 1 of the plan.

## Recommended Remediation (either path, or both)

### Option A — Extract the testable seam (preferred; smaller, no new exemption needed)

1. Add a separate, directly-testable member to `TesseractOcrTextExtractor` (or a small static helper), e.g.:
   ```csharp
   internal static string ResolveTessdataPath() =>
       $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{Path.DirectorySeparatorChar}TaskMaster{Path.DirectorySeparatorChar}tessdata";
   ```
2. Call it from `ExtractText`: `string tessdataPath = ResolveTessdataPath();`
3. Add an MSTest test (`internal` visibility is testable via `InternalsVisibleTo` if not already configured for `UtilitiesCS.Test`, or make it `public static` if that attribute is absent — verify which applies in this project before choosing) asserting `ResolveTessdataPath()` returns the expected `%LOCALAPPDATA%\TaskMaster\tessdata` path, using Moq/FluentAssertions conventions consistent with CUT1-CUT2.
4. Re-run the full-suite coverage capture (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`) and confirm the new file's line coverage rises above the applicable floor (85% general-unit-test.md / 90% CLAUDE.md new-module floor) for at least the extracted, testable portion; the remaining native-engine-call lines will still show 0% but will now be a smaller fraction of the file.
5. Re-verify no regression: baseline vs. final test counts, CSharpier, analyzer build, and nullable build must all remain green per the existing toolchain-loop discipline.

### Option B — Formal, ratified exemption (if Option A is judged insufficient or the maintainer prefers to treat this class as untestable in full)

Per CLAUDE.md UT2's existing exemption mechanism ("Authority: This exemption must be ratified by the project maintainer"), obtain explicit maintainer sign-off to add `TesseractOcrTextExtractor` (or just its `ExtractText` method) to the coverage-exemption list, applied via an `[ExcludeFromCodeCoverage]` attribute with an in-code comment documenting the rationale (native, unmockable third-party engine dependency with no available seam below this class), or via a `coverage.config` assembly-level exclude scoped as narrowly as possible. This path does not remove the underlying testability gap but makes the exemption explicit and reviewable rather than silent, consistent with the general-unit-test.md Coverage Exclusion Policy's requirement that exemptions be visible and intentional rather than a byproduct of an unreviewed 0% figure.

## Non-Blocking, Informational Item (no remediation required)

Repo-wide C# line coverage (83.7806% post-change vs. 83.7981% baseline) sits marginally below the stricter 85% uniform floor in `.claude/rules/general-unit-test.md`/`quality-tiers.md`, but this is pre-existing debt (baseline was already below 85%) and this change does not introduce a measurable regression (delta -0.0175 percentage points, within the noise band the executor documented for `dotnet-coverage`). Under the alternative CLAUDE.md 80% repo-wide floor, this passes outright. No remediation is requested for this item specifically; it is surfaced here only because it co-occurs with the Blocking new-code finding above and the two thresholds are known to conflict (see `.claude/agent-memory/atomic-planner/project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md`).

## Handoff

This remediation should route to the standard remediation cycle (atomic-planner / atomic-executor) scoped to Option A (or A+B) above. No other Blocking or High-severity findings exist in this review; AC1-AC5 are already satisfied and do not require rework.
