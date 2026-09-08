# [P0-T2] Requirements documents read and acceptance-criteria inventory

Timestamp: 2026-09-08T00-14

## Documents read in full

- `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md` (501 lines)
- `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/issue.md` (81 lines)
- `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/research/research.2026-09-07T20-20.md` (806 lines)

## Acceptance-criteria source

Work mode is `full-bug`, persisted at `issue.md:12` as `- Work Mode: full-bug` and restated at `spec.md:9`. Under `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `full-bug` resolves to `spec.md` **only**. `spec.md` is therefore the sole acceptance-criteria source for this delivery. `user-story.md` is intentionally absent.

`issue.md` carries its own `## Acceptance Criteria` section holding AC1 through AC4, reproduced verbatim from it into `spec.md`. That section is a mirror, not a second source; [P6-T11] keeps it consistent and adds no criterion.

## Inventory — six identifiers from the `## Acceptance Criteria` section of `spec.md` (lines 458 through 465)

- `AC1` — "`Init()` throws a named `InvalidOperationException` when called from a non-STA thread, before"
- `AC2` — "A failed `Initialize()` does not consume the latch; a subsequent `Init()` retries"
- `AC3` — "`SynchronizationContextAwaiter.IsCompleted` returns true on the owning UI thread regardless of ambient context"
- `AC4` — "Unit tests cover STA/MTA rejection, latch re-arm after throw, and awaiter inline-vs-post"
- `AC5` — "An evidence artifact under this feature folder's `evidence/other/` directory records a measurement"
- `AC6` — "A coverage report produced by `vstest.console.exe ... /EnableCodeCoverage` and stored under this"

Each quotation is the first twelve words of that criterion's clause as written in `spec.md`. All six lines are currently unchecked (`- [ ] AC<n>:`).

`spec.md:467` records that AC1 through AC4 are reproduced verbatim from `issue.md` and that AC5 and AC6 are added by the specification for the decision-D5 measurement and the coverage uplift.
