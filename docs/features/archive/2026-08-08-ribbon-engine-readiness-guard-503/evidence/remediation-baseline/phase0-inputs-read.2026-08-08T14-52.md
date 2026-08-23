# Phase 0 — Remediation Inputs and Supporting Context Read (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T2]
Command: Read tool invocations against each absolute path listed below (read-only inspection; no command executed)
EXIT_CODE: 0

## Files read (six absolute paths)

1. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\remediation-inputs.2026-08-08T14-26.md`
2. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\spec.md`
3. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\plan.2026-08-08T11-59.md`
4. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\code-review.2026-08-08T14-15.md`
5. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\policy-audit.2026-08-08T14-15.md`
6. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\feature-audit.2026-08-08T14-15.md`

## Resolved work mode and acceptance-criteria source

- Work mode: **`full-bug`**, read from the persisted marker `- Work Mode: full-bug` in `spec.md` (line 9) and corroborated by `issue.md:12` as recorded in `feature-audit.2026-08-08T14-15.md`.
- Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `full-bug` resolves the authoritative acceptance-criteria source to **`spec.md` only**. No `user-story.md` exists for this issue and none is to be created.
- `spec.md` holds 30 acceptance criteria (AC1-AC30). 27 are `- [x]`; AC19, AC20, and AC21 are `- [ ]` and are MANUAL-ONLY.

## In-scope findings for this cycle — exactly two

**F1 — Vacuous assertion in the AC5 ribbon-XML test (Medium, in-scope).**
Location: `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`, approximately lines 197-206, inside `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback`. The assertion is written `...Attribute("getEnabled")?.Value.Should().Be("EngineCommand_GetEnabled")`. The null-conditional `?.` short-circuits the whole expression including the `.Should()` call, so when the attribute is absent — the exact regression the test exists to catch — no assertion executes and the test passes silently. Required outcome: the assertion must fail when the attribute is missing, when it is present with the wrong value, and when it is present but empty. Acceptance requires a deliberate temporary removal of one `getEnabled` attribute from the embedded ribbon resource, confirmation that this specific test fails, then restoration, with the fail-then-restore proof recorded as regression-testing evidence. The permanent test suite must not contain the temporary mutation.

**F2 — `RibbonExplorer.xml` grew more than the change required (Medium, in-scope).**
The file went from 519 to 539 lines while already above the 500-line cap, which `spec.md` AC25 records as an accepted pre-existing exception rather than something this change may worsen. Only 8 of the added lines are functionally required (one `getEnabled` attribute per engine-backed button); approximately 12 came from reformatting three previously single-line `<button>` elements (`TriageSetA`, `TriageSetB`, `TriageSetC`) into multi-line form. Required outcome: restore those three elements to single-line form while retaining their `getEnabled` attribute. Acceptance: line count at or below 527, all eight `getEnabled` attributes present and correct, AC5/AC6/AC7 ribbon-XML tests still passing, and the embedded resource still parsing as valid CustomUI.

No third finding is in scope. The `feature-review` pass returned PASS with zero Blocking findings, so this cycle is discretionary; its exit gate is the same reaudit standard.

## Out-of-scope list, verbatim from remediation-plan section 6

Do not touch, fix, re-promote, or re-litigate any of the following. Each is already routed to its own issue or was assessed and dismissed by the review.

- Issue **#512** — pre-existing repository-wide nullable debt and the vacuous type-check gate.
- Issue **#510** — the `CS2002` duplicate `<Compile Include>` entry in `UtilitiesCS.Test.csproj`.
- Issue **#508** — the `YieldAsync_WithoutDispatcher_RemainsStrict` order-dependent flake.
- The residual `engine as SpamBayes` / `.Engine` dereference window (Low; the click guard makes the reported defect unreachable, and narrowing the readiness contract is a design change, not a fix).
- The `??=` lazy-initialiser thread-safety observation in `RibbonController.EngineCommands.cs` (Low; the runner is immutable and the reviewer assessed it benign).
- Any change to `TaskMaster\AppGlobals\AppItemEngines.cs` or `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` — AC15 requires a zero-line diff on both.
- AC19, AC20, AC21 — MANUAL-ONLY, must remain unchecked.
- Issues **#504**, **#505**, **#506**, **#507**, **#509**, **#511** — already promoted out-of-scope findings.

## Output Summary

Six supporting documents read. Work mode resolved to `full-bug` with `spec.md` as the sole acceptance-criteria source. Exactly two in-scope findings for this cycle: F1 (vacuous AC5 ribbon-XML assertion) and F2 (`RibbonExplorer.xml` line growth). Eight out-of-scope categories recorded verbatim and must not be touched. Blocking findings entering the cycle: 0. `plan.2026-08-08T11-59.md` sections 4 (scope lock) and 5 (contract specifications) remain authoritative and are not re-litigated here.
