# Remediation Inputs — ribbon-controller-engines-null-unsafe (#507)

Timestamp: 2026-08-08T17-45
Source audits: `policy-audit.2026-08-08T17-45.md`, `code-review.2026-08-08T17-45.md`,
`feature-audit.2026-08-08T17-45.md` (all in this feature folder).

Total Blocking findings: 2.

## Fix 1 — Test file exceeds the 500-line limit (Blocking)

- **File**: `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`
- **Current state**: 513 lines (merge-base baseline was 452 lines; the two new `[TestMethod]`s for
  #507 added 61 lines).
- **Expected behavior**: File must be `<= 500` lines, per `CLAUDE.md` § 4.1 and
  `.claude/rules/general-code-change.md` § File Size Limit.
- **Suggested approach**: Extract a cohesive subset of existing tests (for example, all
  `Engines`/`SB`-focused tests, or another naturally cohesive group already in the file) into a new
  sibling test file (e.g. `TaskMaster.Test/Ribbon/RibbonControllerEnginesTests.cs`) so both files
  stay under 500 lines. Do not delete or weaken any existing test to make room. Do not move tests
  out of the `tests/`-mirroring structure this repo already uses.
- **Verification command**: `wc -l TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (and the new
  sibling file, if created) must each report `<= 500`.

## Fix 2 — `Engines` null-return does not resolve the reachable NRE for real callers (Blocking)

- **Files**: `TaskMaster/Ribbon/RibbonController.Intelligence.cs` (property, already fixed at the
  boundary) and `TaskMaster/Ribbon/RibbonViewer.cs` (11 unguarded call sites: `TestSpam_Click`,
  `SpamBayesEnabled_Click`, `SpamBayesEnabled_GetPressed`, `SpamSaveNetwork_Click`,
  `SpamSaveLocal_Click`, `GetSaveLocation_Click`, `TriageEnabled_Click`, `TriageEnabled_GetPressed`,
  `TriageSaveNetwork_Click`, `TriageSaveLocal_Click`, `TriageGetSaveLocation_Click`).
- **Current state**: `Controller.Engines` now returns `null` instead of throwing when `Globals` is
  unassigned, but every one of the 11 call sites above immediately dereferences the result with no
  null check, so the same click still throws an unhandled `NullReferenceException` — it originates
  one or more frames later, inside `RibbonViewer.cs`, instead of inside
  `RibbonController.get_Engines()`.
- **Expected behavior — pick one of two remediation paths, and state which was chosen in the
  updated `issue.md`**:
  1. **Scope-clarification path (no code change to `RibbonViewer.cs`)**: Amend `issue.md`'s
     "Problem / Why" and AC1 wording (or add an explicit note) to state plainly that this fix
     resolves the `RibbonController.Engines` property contract only, matching the `SB` sibling
     precedent, and does **not** by itself resolve the end-to-end reachable-crash scenario for any
     current `RibbonViewer.cs` caller — that remains explicitly deferred to #503/#505/#506. This
     path requires no source change beyond documentation and is consistent with the plan's existing
     Hard Scope Boundary (`RibbonViewer.cs` must not be modified).
  2. **Caller-hardening path (requires a scope amendment)**: If the intent is to actually close the
     reachable crash, add null-guards at the 11 call sites in `RibbonViewer.cs` (e.g. an early
     return or a user-facing "not ready" message when `Controller.Engines` is `null`), consistent
     with how a caller-readiness guard is expected to work. This requires explicitly amending the
     plan's Hard Scope Boundary (which currently forbids touching `RibbonViewer.cs` and defers this
     exact class of fix to the unmerged `bug/ribbon-engine-readiness-guard-503` branch) — do not
     silently expand scope without that amendment being recorded.
- **Do not do**:
  - Do not widen or remove the `[ExcludeFromCodeCoverage]` exemption on `RibbonController`.
  - Do not modify `RibbonViewer.cs` unless remediation path 2 is explicitly chosen and the scope
    amendment is recorded in `issue.md`.
  - Do not resolve this by adding a null-forgiving `!` or changing `Engines`'s declared return type
    to `IAppItemEngines?` — the policy-audit's evidence shows both were already considered and
    rejected for the CS8603/CS8632 tradeoffs they introduce under the CLAUDE.md nullable command.
  - Do not touch `TaskMaster/Ribbon/RibbonViewer.cs` behavior for #505 (`ribbon-async-getpressed-signature`)
    or #506 (`ribbon-toggle-engine-fire-and-forget`) — those remain out of scope regardless of which
    path is chosen here.
- **Verification command**: if path 2 is chosen, re-run the full four-stage C# toolchain
  (`csharpier check .`; `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`;
  `msbuild ... /t:Rebuild /p:TreatWarningsAsErrors=true` matching `.github/workflows/ci.yml`;
  `vstest.console.exe <9 assemblies> /EnableCodeCoverage`) and add regression tests for the new
  guard behavior at the affected `RibbonViewer.cs` call sites, following the same MSTest/Moq/FluentAssertions/AAA
  pattern used by the two existing #507 tests.

## Pointer to audit artifacts

- `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/policy-audit.2026-08-08T17-45.md`
- `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/code-review.2026-08-08T17-45.md`
- `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/feature-audit.2026-08-08T17-45.md`
