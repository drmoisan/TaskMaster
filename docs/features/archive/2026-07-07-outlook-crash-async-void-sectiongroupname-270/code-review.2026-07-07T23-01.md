# Code Review — Issue #270 (outlook-crash-async-void-sectiongroupname)

- Timestamp: 2026-07-07T23-01
- Reviewer: feature-reviewer
- Base: `main` @ merge-base `82f89f2bd90b6456eb2fd2639eb2d5bc05eec999`
- Head: `d3ed469f1e72d37f61ba7089a759e6bcbdd7c337`

## Executive Summary

The change is a focused, well-structured defect fix. It converts two `async void` Outlook
COM event handlers from a process-terminating `catch (Exception) { throw; }` to a
log-and-contain boundary catch, and it introduces the smallest seam (an injectable delegate
with a safe production default) needed to make the fault-containment path deterministically
testable. The refactor separates host-neutral logic (`HandleInboxItemAddAsync`,
`HandleToDoItemChangeAsync`, which hold the try/catch) from the thin, host-bound async-void
wrappers, which is the correct pattern for async-void event handlers. Test helpers were
extracted into a partial-class file to respect the 500-line ceiling, and a pre-existing test
that encoded the old (defective) rethrow contract was corrected.

Code quality is good. XML documentation explains the "why" (ThreadPool rescheduling
terminating `outlook.exe`). Naming is descriptive and follows repo conventions. The new
tests are deterministic, use MSTest + Moq + FluentAssertions, and assert on the original
exception object via reference identity. No blocking or high-severity findings. One
low-severity observation and one informational note are recorded below.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | (build/tooling) | `artifacts/csharp/coverage.xml` (absent) | Canonical machine-readable C# coverage artifact is not emitted at the SKILL/hook path; coverage was verified from committed markdown evidence and the local uncommitted `.coverage` file | Emit the merged Cobertura to `artifacts/csharp/coverage.xml` during the coverage run so the numeric C# gate has an artifact to parse | The `validate-feature-review-coverage.ps1` hook reads this path for the numeric C# repo-wide/branch gate; absence means the gate cannot self-verify from an artifact | `evidence/qa-gates/test-final.2026-07-07T22-50.md` (references `TestResults/.../*.coverage`); `ls artifacts/csharp/coverage.xml` -> not found |
| Info | `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` | `HandleToDoItemChangeAsync` default-collaborator lambda | The production default-collaborator lambda (COM path) is the single uncovered line (92.86% method coverage); it cannot be exercised without a live Outlook process | Accept as-is; the COM path is host-bound. Optionally note it as covered by manual verification | Consistent with the ratified COM/VSTO coverage exemption; no unit-testable seam exists for the live Outlook call | `evidence/qa-gates/coverage-delta.2026-07-07T22-50.md` |
| Info | `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` | seam property declarations (lines ~64-90) | `#nullable enable annotations` / `#nullable restore annotations` scoping is used to keep the `?` seam annotations CS8632-clean under the analyzer build while remaining correct under the nullable build | None — this is the correct narrow approach; a whole-file `#nullable enable` would surface unrelated CS8625/CS8618 on pre-existing members | Documented and confirmed clean under both build gates | `evidence/qa-gates/analyzer-final.2026-07-07T22-50.md`; `evidence/qa-gates/typecheck-final.2026-07-07T22-50.md` |

## Detailed Observations

### Error handling and containment (production)

The replacement `catch (System.Exception ex) { logger.Error("...contained to prevent
process termination.", ex); }` is a correct boundary catch: it sits at the top of an
async-void COM event handler (a defined boundary), preserves and logs the full exception
object, and prevents the ThreadPool rethrow that terminated `outlook.exe`. This satisfies
the general-code-change and csharp.md error-handling standards (broad catch permitted at a
boundary with added context). The original exception object is passed to `logger.Error`, so
the previously-lost `sectionGroupName` `ArgumentException` becomes observable (AC3).

### Seam design (production)

The injectable-delegate seam (`internal Func<object, Task>?` properties, null-coalesced to
the production call) is minimal and correct: default behavior in production is unchanged
(the null-coalesced production lambda runs), and tests assign a throwing delegate to drive
the containment path. This matches csharp.md DI-seam option 2 and the "smallest seam"
guidance. Members are `internal` (via existing IVT to the test assembly), keeping the public
surface unchanged.

### Async-void wrapper pattern (production)

`OlToDoItems_ItemChange` and `OlInboxItems_ItemAdd` remain `async void` (required for the
COM event delegate signature) but are reduced to one-line delegations to `async Task` core
methods. The try/catch lives in the awaited `Task`-returning core, so any fault is contained
before the async-void machinery can reschedule it. This is the recommended structure for
async-void handlers.

### Test refactor and additions (test)

- Helper extraction into `AppEventsTests.Helpers.cs` (partial class, same namespace,
  required usings) is a byte-equivalent move that was necessary because the baseline
  `AppEventsTests.cs` was exactly at the 500-line ceiling; adding two tests would have
  exceeded it. Post-split: 329 + 255 lines, both under 500.
- The two new tests (`HandleInboxItemAddAsync_WhenCollaboratorThrows_...`,
  `HandleToDoItemChangeAsync_WhenCollaboratorThrows_...`) inject a throwing delegate, assert
  `NotThrowAsync`, and assert the in-memory appender captured a single event whose
  `ExceptionObject` is reference-equal to the injected exception. Deterministic and isolated.
- `AppEventsCoverageExpansionTests.cs` correction: the pre-existing test asserted
  `CapturedException.Should().BeSameAs(expected)` (old rethrow contract). It was renamed to
  `..._ContainsAndDoesNotRethrow` and now asserts `CapturedException.Should().BeNull()`,
  encoding the corrected contract. This is correct maintenance of a test-as-spec, not a
  weakening of assertions.
- `TaskMaster.Test.csproj` adds one `<Compile Include="AppGlobals\AppEventsTests.Helpers.cs" />`
  entry — required for the legacy `packages.config` project to compile the new file.

## Verdict

No blocking findings. Recommend proceeding to PR. Address the Low-severity coverage-artifact
recommendation opportunistically (emit `artifacts/csharp/coverage.xml`).
