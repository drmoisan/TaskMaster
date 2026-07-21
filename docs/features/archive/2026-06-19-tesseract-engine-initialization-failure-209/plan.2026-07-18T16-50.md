# tesseract-engine-initialization-failure (Plan)

- **Issue:** #209
- **Branch:** bug/tesseract-engine-initialization-failure-209 (base: main)
- **Mode:** minor-audit
- **Requirements source:** `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/issue.md` (`## Acceptance Criteria` section, AC1-AC5) — sole requirements source. No `spec.md`/`user-story.md` required or expected in this folder.
- **Last Updated:** 2026-07-18T17-27
- **Status:** Complete — all 38 tasks (Phase 0/1/2) checked off; AC1-AC5 verified and checked off in `issue.md`

**Evidence location (non-overridable):** All evidence artifacts for this plan MUST be written under `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/<kind>/` (canonical sub-kinds used below: `baseline/`, `qa-gates/`). No artifact in this plan may be written to any `artifacts/` path.

**Fail-closed rule:** Every evidence-producing task below must produce its artifact with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` fields (baseline/final-QC command artifacts) before its checkbox may be marked complete. Do not mark a task complete without its artifact on disk.

**Timestamp convention:** `<TIMESTAMP>` below denotes the ISO-8601 timestamp (`yyyy-MM-ddTHH-mm`) captured at the moment each artifact is written; each artifact file name and its internal `Timestamp:` field must use the same value.

**Full first-party MSTest assembly set** (used identically in Phase 0 baseline and Phase 2 final-QC for a true before/after comparison):
- `ToDoModel.Test\bin\Debug\ToDoModel.Test.dll`
- `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
- `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
- `TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll`
- `Tags.Test\bin\Debug\Tags.Test.dll`
- `TaskTree.Test\bin\Debug\TaskTree.Test.dll`
- `VBFunctions.Test\bin\Debug\VBFunctions.Test.dll`
- `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`

The full-suite MSTest+coverage command used in both Phase 0 and Phase 2 is the repo-standard wrapper `scripts\vscode\Invoke-MSTestWithCoverage.ps1`, which discovers all `*.Test.dll` under `-SearchRoot` and drives `vstest.console.exe` (via `dotnet-coverage collect`, satisfying the CUT3 `vstest.console.exe ... /EnableCodeCoverage` requirement with a Cobertura-format coverage artifact) with `/InIsolation` — the same single-process invocation mode in which the ~60-test cascading failure cluster was observed.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read `CLAUDE.md` in full (policy reading order position 1) before any other action in this session.
- [x] [P0-T2] Read `.claude/rules/general-code-change.md` in full (policy reading order position 2).
- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` in full (policy reading order position 3).
- [x] [P0-T4] Read `.claude/rules/csharp.md` in full (policy reading order position 4, C#-specific toolchain and testing standard for this change).
- [x] [P0-T5] Read `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/issue.md` and confirm it contains an explicit `## Acceptance Criteria` heading with items AC1 through AC5; this section is the sole AC source for this plan.
- [x] [P0-T6] Confirm that no `spec.md` and no `user-story.md` file exists under `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/`; record the confirmation (minor-audit fail-closed check — presence of either file is a blocking finding requiring escalation before Phase 1 proceeds).
- [x] [P0-T7] Write the Phase 0 policy-read evidence artifact at `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/baseline/phase0-instructions-read.<TIMESTAMP>.md` containing `Timestamp:`, `Policy Order:` (the four-item order from P0-T1–P0-T4), and the explicit file list read in P0-T1–P0-T6.
- [x] [P0-T8] On branch `bug/tesseract-engine-initialization-failure-209`, before making any fix changes, run the baseline build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`; capture the console output and `EXIT_CODE`.
- [x] [P0-T9] Write the baseline build evidence artifact at `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/baseline/baseline-build.<TIMESTAMP>.md` with `Timestamp:`, `Command:` (from P0-T8), `EXIT_CODE:`, and `Output Summary:` (build succeeded/failed, error count, warning count).
- [x] [P0-T10] Run the baseline full-suite MSTest-with-coverage pass across all eight first-party `*.Test.dll` assemblies listed above: `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\baseline\coverage-baseline.cobertura.xml'`, redirecting the full console output via `Tee-Object` to `docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\baseline\mstest-baseline-console.<TIMESTAMP>.log`; capture `EXIT_CODE`.
- [x] [P0-T11] From `mstest-baseline-console.<TIMESTAMP>.log`, record the exact Total/Passed/Failed/Skipped test counts, AND explicitly record whether the strings `Failed loading language 'eng'` or `Error opening data file` occur anywhere in the log and the exact occurrence count (record `0` if absent — do not assume either outcome).
- [x] [P0-T12] From `coverage-baseline.cobertura.xml`, extract the root `<coverage>` element's `line-rate` and `branch-rate` attributes and record them as baseline line-coverage % and branch-coverage % (multiply each by 100).
- [x] [P0-T13] Write the baseline MSTest/coverage evidence artifact at `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/baseline/baseline-mstest.<TIMESTAMP>.md` with `Timestamp:`, `Command:` (from P0-T10), `EXIT_CODE:`, and `Output Summary:` containing: Total/Passed/Failed/Skipped counts (from P0-T11), the tessdata-error occurrence count (from P0-T11), and baseline line/branch coverage % (from P0-T12).

---

### Phase 1 — Constrained Implementation (Small-Path Delegation)

- [x] [P1-T1] In a new file `UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs`, add a public interface `IOcrTextExtractor` in namespace `UtilitiesCS.EmailIntelligence` (matching the namespace already declared in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs`) with a single method `string ExtractText(Bitmap bitmap)`.
- [x] [P1-T2] In the same file, add `internal sealed class TesseractOcrTextExtractor : IOcrTextExtractor` whose `ExtractText(Bitmap bitmap)` method body is the exact current body of `ImageStripper.extract_text` (today at `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs` lines 350–381): identical tessdata path resolution (`%LOCALAPPDATA%\TaskMaster\tessdata`), identical `new TesseractEngine(tessdataPath, "eng", EngineMode.Default)` construction inside a `using` block, and identical `engine.Process(bitmap)` / `page.GetText()` return — no behavior change from the current production path.
- [x] [P1-T3] In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs`, add a private field `private readonly IOcrTextExtractor _ocrTextExtractor;` and two new public constructor overloads, `ImageStripper(IOcrTextExtractor ocrTextExtractor)` and `ImageStripper(string cachefile, IOcrTextExtractor ocrTextExtractor)`, where the two-parameter constructor is the single implementation body: it sets `_cachefile = cachefile;` and `_ocrTextExtractor = ocrTextExtractor ?? new TesseractOcrTextExtractor();`.
- [x] [P1-T4] In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs`, change the two existing public constructors `ImageStripper()` and `ImageStripper(string cachefile)` to chain into the new two-parameter constructor via `: this(cachefile: null, ocrTextExtractor: null)` and `: this(cachefile, ocrTextExtractor: null)` respectively, with empty bodies; their public signatures and observable behavior for all existing callers remain unchanged (AC1's "default/production behavior is unchanged" requirement).
- [x] [P1-T5] In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs`, replace the body of `public string extract_text(Bitmap bitmap)` with a single delegation: `return _ocrTextExtractor.ExtractText(bitmap);`, removing the direct `TesseractEngine` construction from this method (moved to `TesseractOcrTextExtractor` in P1-T2).
- [x] [P1-T6] Add `<Compile Include="EmailIntelligence\EmailParsingSorting\TesseractOcrTextExtractor.cs" />` to `UtilitiesCS/UtilitiesCS.csproj` immediately adjacent to the existing `<Compile Include="EmailIntelligence\EmailParsingSorting\ImageStripper.cs" />` entry (this is a legacy `packages.config`, non-SDK project with explicit `<Compile Include>` wiring and no glob-based inclusion; the new file will not compile into the assembly without this entry).
- [x] [P1-T7] In `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs`, modify the test method `Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken` (currently at line 255): replace the `var stripper = new ImageStripper();` Arrange line with a `Mock<IOcrTextExtractor>` whose `ExtractText(It.IsAny<Bitmap>())` is set up to return `string.Empty`, and construct `stripper` via `new ImageStripper(mockExtractor.Object)`; preserve the existing Act line (`stripper.analyze("Tesseract", ...)`) and both existing Assert lines (`text.Should().NotBeNull();`, `tokens.Should().Contain("image-text:no text found");`) unchanged.
- [x] [P1-T8] In `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs`, modify the test method `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken` (currently at line 272) the same way as P1-T7: replace `new ImageStripper();` with a `Mock<IOcrTextExtractor>` returning `string.Empty` from `ExtractText`, construct via `new ImageStripper(mockExtractor.Object)`, and preserve the existing Act/Assert lines unchanged.
- [x] [P1-T9] [AC1 verification] Read the modified `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs` and confirm: (a) `extract_text` contains no reference to `Tesseract.TesseractEngine` and instead calls `_ocrTextExtractor.ExtractText(bitmap)`; (b) the parameterless `ImageStripper()` constructor still results in `_ocrTextExtractor` being a live `TesseractOcrTextExtractor` when no fake is supplied (i.e., default/production behavior is unchanged).
- [x] [P1-T10] [AC2 verification] Run `grep -n "TesseractEngine" UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` and confirm zero matches, and confirm both `Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken` and `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken` construct `ImageStripper` via the `Mock<IOcrTextExtractor>`-accepting constructor.

---

### Phase 2 — Final QC (Full Toolchain + AC3/AC4/AC5 Verification)

- [x] [P2-T1] Run CSharpier format: `dotnet tool run csharpier .` (or `csharpier .`); capture `EXIT_CODE` and the list of any files it reformatted.
- [x] [P2-T2] If P2-T1 reformatted any file, re-run `dotnet tool run csharpier .` until one pass completes with `EXIT_CODE 0` and zero files changed, before proceeding to P2-T3.
- [x] [P2-T3] Write the final-QC CSharpier evidence artifact at `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/final-csharpier.<TIMESTAMP>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P2-T4] Run the analyzer build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; capture `EXIT_CODE` and error/warning counts.
- [x] [P2-T5] Write the final-QC analyzer-build evidence artifact at `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/final-analyzer-build.<TIMESTAMP>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P2-T6] Run the nullable/type-check build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; capture `EXIT_CODE`.
- [x] [P2-T7] Write the final-QC nullable-build evidence artifact at `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/final-nullable-build.<TIMESTAMP>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P2-T8] Run the post-fix full-suite MSTest-with-coverage pass across the same eight first-party `*.Test.dll` assemblies: `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\qa-gates\coverage-final.cobertura.xml'`, redirecting console output via `Tee-Object` to `docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\qa-gates\mstest-final-console.<TIMESTAMP>.log`; capture `EXIT_CODE`. This command must execute unconditionally; `EXIT_CODE: SKIPPED` is not a valid outcome for this task.
- [x] [P2-T9] From `mstest-final-console.<TIMESTAMP>.log`, record the exact post-change Total/Passed/Failed/Skipped test counts.
- [x] [P2-T10] [AC3 verification] Grep `mstest-final-console.<TIMESTAMP>.log` for `Failed loading language 'eng'` and `Error opening data file`; confirm zero occurrences. If any occurrence is found, record AC3 as FAILED and do not report an overall PASS for this plan.
- [x] [P2-T11] [AC4 verification] Compare the P2-T9 post-change Total/Passed/Failed/Skipped counts against the P0-T11/P0-T13 baseline counts for the identical eight-assembly set; confirm no test that passed at baseline now fails, and record any test that changed status (e.g., the two named OCR tests moving from a live-engine-dependent outcome to a deterministic mock-backed pass) as an explicit, itemized delta rather than an unexplained aggregate change. If any unexplained new failure exists, record AC4 as FAILED.
- [x] [P2-T12] From `coverage-final.cobertura.xml`, extract the root `<coverage>` element's `line-rate` and `branch-rate` attributes, record post-change line/branch coverage %, and compute the delta against the P0-T12 baseline values.
- [x] [P2-T13] Write the final-QC MSTest/coverage evidence artifact at `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/final-mstest.<TIMESTAMP>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` containing: post-change counts (P2-T9), the AC3 occurrence count (P2-T10), the AC4 itemized delta (P2-T11), and baseline-vs-final line/branch coverage % (P2-T12).
- [x] [P2-T14] [AC5 verification] Confirm P2-T4 and P2-T6 both recorded `EXIT_CODE: 0` with zero analyzer errors and zero nullable-warnings-as-errors; write the confirmation to `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/final-ac5-verification.<TIMESTAMP>.md` with `Timestamp:`, and references to the P2-T5/P2-T7 artifacts. If either build recorded a non-zero `EXIT_CODE`, record AC5 as FAILED.
- [x] [P2-T15] If any of P2-T1, P2-T4, P2-T6, or P2-T8 failed or changed files on its most recent run, restart the full loop from P2-T1; do not report this plan as complete until a single pass completes P2-T1 through P2-T14 with no failing or file-changing step.
