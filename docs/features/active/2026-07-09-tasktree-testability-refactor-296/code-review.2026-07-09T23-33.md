# Code Review — #296 tasktree-testability-refactor (remediation re-audit)

- Timestamp: 2026-07-09T23-33
- Branch: feature/tasktree-testability-refactor-296 @ c19f77ec
- Base: epic/winforms-testability-refactor-integration (merge-base 3f04d50f)

## Executive Summary

The remediation commit replaces the three previously-exempted testable seams with mockable,
unit-tested designs while keeping the residual host-bound wrappers thin and individually justified.
The changes align with the repository General and C# Code Change policies: strong typing at the seam
boundary (`object` + typed `switch` dispatch instead of `dynamic`), separation of the host-neutral
routing decision from control-bound marshalling, and small, single-responsibility methods. Tests
follow the C# Unit Test policy (MSTest, Moq, FluentAssertions, Arrange-Act-Assert, no live
control/popup, no temp files, deterministic). No Blocking or Non-Blocking code-quality finding was
identified.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | TaskTree/TaskTreeController.cs | 67-108 | `ActivateOlItem`/`ActivateOlItemAsync` now take `object` and dispatch display via typed `DisplayOutlookItem` switch; late-bound `dynamic` removed. | None. | Static binding against mockable interop interfaces satisfies C#2 (strong contracts) and enables coverage. | Lines 67, 88, 117-128 |
| Info | TaskTree/TaskTreeController.MoveLogic.cs | 78-159 | Drop routing extracted to `RouteDrop` (returns bool) and `ApplyPostDropView` over `ITreeVisual`/`ITaskTreeForm` seams; exemption confined to the thin `HandleModelDropped` wrapper. | None. | Separation of pure routing from host-bound marshalling satisfies General §1.4 and §6.2; wrapper is irreducible. | Lines 77-96, 107-159 |
| Info | TaskTree/TaskTreeController.cs | 148-149 | `ResolveRowStyle` keeps the strikeout decision host-neutral and covered while `FormatRow` (E3) stays a thin wrapper. | None. | Consistent with the seam-extraction pattern; decision logic is testable. | Lines 134-149 |
| Info | TaskTree.Test/TaskTreeControllerRouteDropTests.cs | 48-63 | `ModelDropEventArgs` built via reflection on private fields (`targetModel`, `dragModels`) because the third-party type has no public constructor for these. | Accept as-is; documented and localized to a test helper. | Reflection is confined to test setup for a non-constructible third-party arg type; keeps tests off live controls per spec risk mitigation. | Lines 48-63 |

## Detailed Observations

### Strengths

- Typed dispatch (`DisplayOutlookItem`) replaces late binding, so the display branch binds against the
  Moq-mockable `Outlook.MailItem`/`TaskItem` interfaces and is directly asserted in tests.
- `RouteDrop` returns a boolean routing result, letting the caller cleanly skip the post-drop refresh
  on unhandled locations; this made the default branch independently testable.
- The four residual exemptions are each accompanied by an in-code comment identifying the site (E1/E2/
  E3/E6) and the specific irreducible host dependency, aiding future review.
- Test files remain within the 500-line limit and use consistent Arrange-Act-Assert structure with
  descriptive scenario names.

### Policy alignment

- General Code Change §1 (simplicity/separation): host-neutral logic is separated from COM/WinForms
  marshalling. Met.
- General Code Change §4 (file size): all files < 500 lines. Met.
- C# Code Change C#2 (strong contracts, null-safety): `object` seam with typed switch; nullable/TWAE
  build clean. Met.
- General Unit Test UT1/UT4: tests are independent, isolated, deterministic; no external dependencies,
  no temp files, no popups. Met.
- C# Unit Test CUT1/CUT2: MSTest + Moq + FluentAssertions used throughout. Met.

### Risks / follow-ups

None Blocking. The reflection-based `ModelDropEventArgs` construction is an acceptable, localized
accommodation for a non-constructible third-party type and is the standard way to exercise this
BrightIdeasSoftware event contract without a live control.

## Verdict

PASS. No Blocking or Non-Blocking code-quality finding. The remediation is a clean, policy-conformant
seam extraction with adequate test coverage of the newly-exposed branches.
