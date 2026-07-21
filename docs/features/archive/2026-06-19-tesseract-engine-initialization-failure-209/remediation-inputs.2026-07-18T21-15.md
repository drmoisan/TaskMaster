# Remediation Inputs — tesseract-engine-initialization-failure (Issue #209) — R4 Re-Audit (remediation_pass 1)

- Branch: `bug/tesseract-engine-initialization-failure-209`
- Base: `main` @ `a4977216467c6a275648e6ce134adf847693fc6a`
- Timestamp: 2026-07-18T21-15
- Referenced artifacts: `policy-audit.2026-07-18T21-15.md` (`## 3.2`), `code-review.2026-07-18T21-15.md` (Findings Table, Low-severity row on the residual)
- Prior remediation cycle: `remediation-inputs.2026-07-18T17-42.md` (Option A directed), `remediation-plan.2026-07-18T17-42.md` (executed in commits `727ec8f5`, `1c8daf4f`).

## Remediation-Required Finding

**Severity: Blocking.**

`UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs` line coverage improved from 0% to **7.6923%** (1 of 13 executable lines hit) in `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/remediation1-coverage-final.cobertura.xml`, following the R1-cycle remediation (extraction of `ResolveTessdataPath()` as a directly-testable `internal static` helper, now covered by `TesseractOcrTextExtractor_Tests.cs`). This is a genuine, verified improvement — not a no-op — but it does not close the underlying policy gap:

- `.claude/rules/general-unit-test.md` / `.claude/rules/quality-tiers.md`: uniform 85% line / 75% branch floor applied to new code. 7.6923% line coverage fails this floor by a wide margin.
- `CLAUDE.md` UT2: "Any new modules, classes, or methods added must target >= 90% coverage." 7.6923% fails this floor as well.

No formal, maintainer-ratified exemption (`[ExcludeFromCodeCoverage]` attribute with documented rationale, or a `coverage.config` assembly/class-level exclude) has been added for the residual. The class does not fall within any of CLAUDE.md UT2's three enumerated automatic-exemption categories (VSTO add-in lifecycle classes; WinForms Designer code; Outlook-Interop event handlers in `TaskVisualization`/`QuickFiler`/`TaskMaster`/`ToDoModel`/`Tags`), since it depends on the third-party native `Tesseract.TesseractEngine`, not on Outlook COM/Interop or VSTO types.

## Why This Is Different From the R1 Finding

The R1 finding (0% coverage) had a clear, actionable, low-risk remediation: extract the one piece of genuinely pure logic (`tessdataPath` string formatting) and test it directly. That remediation was executed correctly and completely — this review independently confirmed the extraction is a verbatim, behavior-preserving move (see `code-review.2026-07-18T21-15.md`).

**The R4 residual is architecturally different in kind.** The remaining 12 uncovered lines in `ExtractText` are the literal construction of a third-party native `Tesseract.TesseractEngine`, the call to `engine.Process(bitmap)`, and `page.GetText()`. This class is the single concrete implementation of `IOcrTextExtractor` in the codebase; the rest of the codebase already mocks the interface, not this class, at the correct architectural boundary (`ImageStripper_Tests.cs` uses `Mock<IOcrTextExtractor>`). Introducing a further layer of indirection purely to make this class's own body "testable" would either (a) still require a live, provisioned `tessdata` directory to genuinely exercise the native call — the exact external dependency the R1 fix was designed to eliminate from unit tests — or (b) relocate the same untestable native call one level deeper into a new default-implementation class, which would itself then present the identical 0%-of-its-own-body problem. Neither path is a real fix; both would add complexity without adding real verification.

This means Option A (further code-level extraction) has effectively reached its ceiling. The remaining path to full policy compliance is not an engineering task — it is a **policy disposition decision**.

## Recommended Remediation (requires a maintainer/user decision; no further code change is recommended by this review)

### Option B — Formal, maintainer-ratified exemption (recommended primary path)

Per CLAUDE.md UT2's existing exemption mechanism ("Authority: This exemption must be ratified by the project maintainer"), obtain explicit maintainer sign-off to add `TesseractOcrTextExtractor.ExtractText`'s native-engine body to the coverage-exemption list, applied via:
- an `[ExcludeFromCodeCoverage]` attribute on the method (or the whole `internal sealed class TesseractOcrTextExtractor`, scoped as narrowly as reasonable) with an in-code comment documenting the rationale (native, unmockable third-party engine dependency; the class is the sole default implementation of an already-mocked interface seam; further indirection would not add real test coverage), or
- a `coverage.config` class-level exclude scoped only to this class.

This path makes the exemption explicit and reviewable rather than a byproduct of an unremediated low percentage, consistent with the general-unit-test.md Coverage Exclusion Policy's requirement that exemptions (where used) be visible and intentional. **Caveat, to be surfaced to the maintainer explicitly:** general-unit-test.md's Coverage Exclusion Policy also states unconditionally that "No production file may be excluded from coverage measurement," and its enumerated `Prohibited exclude entries` section does not carve out a "native third-party engine adapter" category the way CLAUDE.md UT2 does for COM/VSTO/WinForms. This is the same unresolved CLAUDE.md-vs-general-unit-test.md conflict already recorded in `.claude/agent-memory/atomic-planner/project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md`; ratifying an exemption here would be, in effect, also resolving that conflict in favor of CLAUDE.md's exemption mechanism for this specific class. The maintainer should make that choice knowingly rather than have it made silently by a future review cycle.

### Option C — Accept as documented residual, no exemption attribute (alternative path)

Per general-unit-test.md's Coverage Exclusion Policy philosophy ("leave only the thinnest possible wiring in the host-bound entry point... a real and visible cost in the coverage metric"), the maintainer could instead explicitly accept the 7.6923% figure as the intended, permanent state of this class — recording that decision (e.g., in `issue.md` or a project-memory entry) so a future review cycle does not re-open this as a fresh Blocking finding. Under this option, repo-wide C# coverage permanently carries this class's ~92%-uncovered body as a visible cost, consistent with the policy's stated design intent, and no `[ExcludeFromCodeCoverage]` attribute is added (avoiding the conflict with the "no production file may be excluded" prohibition).

### Non-viable path (documented for completeness, not recommended)

Further code-level seam decomposition (e.g., wrapping `TesseractEngine` construction behind a second injectable factory) was evaluated by this review and rejected as not materially improving verifiable coverage — see the "Why This Is Different" analysis above. A future planner should not schedule this as an engineering task without first securing the Option B or Option C decision from the maintainer.

## Non-Blocking, Informational Item (carried forward, unchanged in substance)

Repo-wide C# line coverage (83.7826% post-remediation vs. 83.7729% remediation-baseline) sits marginally below the stricter 85% uniform floor in `.claude/rules/general-unit-test.md`/`quality-tiers.md`, but this is pre-existing debt (baseline was already below 85% before this branch existed) and this change does not introduce a regression (delta +0.0097 percentage points, an improvement). Under the alternative CLAUDE.md 80% repo-wide floor, this passes outright. No remediation is requested for this item specifically.

## Handoff

This remediation-required finding should route to the standard remediation cycle, but the first action is a **maintainer/user decision** (Option B or Option C above), not an atomic-planner/atomic-executor engineering cycle. Once the maintainer's choice is recorded (either a ratified `[ExcludeFromCodeCoverage]` attribute with in-code rationale, or an explicit accepted-residual decision documented in the feature folder), a follow-up review cycle can close this finding without further code changes. No other Blocking or High-severity findings exist in this review; AC1-AC5 remain satisfied and do not require rework.
