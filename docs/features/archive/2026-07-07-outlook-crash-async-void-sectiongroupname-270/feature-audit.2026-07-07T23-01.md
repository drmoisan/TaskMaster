# Feature Audit — Issue #270 (outlook-crash-async-void-sectiongroupname)

- Timestamp: 2026-07-07T23-01
- Reviewer: feature-reviewer
- Work mode: minor-audit
- AC source: `issue.md` `## Acceptance Criteria` section (minor-audit -> `issue.md` only)

## Scope and Baseline

- Base branch: `main`
- Merge-base SHA: `82f89f2bd90b6456eb2fd2639eb2d5bc05eec999` (recomputed via `git merge-base HEAD origin/main`)
- Head SHA: `d3ed469f1e72d37f61ba7089a759e6bcbdd7c337`
- Diff range: `82f89f2b..d3ed469f`

Production scope (from git diff, not the misclassified summary):
- `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` — the only production file changed.

Test / build scope:
- `TaskMaster.Test/AppGlobals/AppEventsTests.cs` — helper extraction + 2 new regression tests.
- `TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs` — new (byte-equivalent helper move).
- `TaskMaster.Test/AppGlobals/AppEventsCoverageExpansionTests.cs` — corrected pre-existing test.
- `TaskMaster.Test/TaskMaster.Test.csproj` — compile-include for the new helper file.

Documentation/evidence scope: feature folder `issue.md`, `plan.md`, research doc, and
`evidence/**` artifacts.

## Acceptance Criteria Inventory

Source: `issue.md` `## Acceptance Criteria` (6 checkbox items, all pre-marked `[x]` by the executor):

- AC1: `OlToDoItems_ItemChange` no longer contains `catch (Exception) { throw; }`; fault logged (full exception, existing `logger`) and contained; nothing escapes the async-void method.
- AC2: `OlInboxItems_ItemAdd` no longer contains `catch (Exception) { throw; }`; fault logged and contained; nothing escapes.
- AC3: Logged output preserves the original exception object (message + stack), making a previously-lost `sectionGroupName` `ArgumentException` observable.
- AC4: A deterministic MSTest regression test (Moq + FluentAssertions, no COM/network/temp) drives each handler with an injected throwing collaborator and asserts contain+log (no throw); fails pre-fix, passes post-fix.
- AC5: Full C# toolchain passes in order (CSharpier -> analyzers -> nullable/type-check -> MSTest) with no new warnings; coverage on changed lines does not regress.
- AC6: No scope creep — only `AppEvents.ReadinessHookup.cs` (production) and `AppEventsTests.cs` (test) changed for the fix; config trigger and RibbonViewer handlers remain documented follow-ups.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | Diff of `AppEvents.ReadinessHookup.cs`: `OlToDoItems_ItemChange` is now a one-line async-void delegating to `HandleToDoItemChangeAsync`, whose `catch (System.Exception ex)` calls `logger.Error("OlToDoItems_ItemChange handler faulted; contained...", ex)` with no rethrow. No `catch { throw; }` remains. |
| AC2 | PASS | Same diff: `OlInboxItems_ItemAdd` delegates to `HandleInboxItemAddAsync`, whose `catch (System.Exception ex)` logs via `logger.Error(...)` and does not rethrow. |
| AC3 | PASS | `logger.Error(message, ex)` passes the original exception object; the new tests assert `ReferenceEquals(loggingEvent.ExceptionObject, injected)` on the injected `ArgumentException("...sectionGroupName...")`. Verified in `test-final.2026-07-07T22-50.md`. |
| AC4 | PASS | Two new `[TestMethod] async Task` tests use Moq (`Mock<IApplicationGlobals>` Strict), FluentAssertions (`NotThrowAsync`, `ContainSingle`), in-memory `MemoryAppender`; no COM/network/temp (grep-verified). `evidence/regression-testing/fail-before.2026-07-07T22-18.md` (EXIT 1 pre-fix) and `pass-after.2026-07-07T22-20.md` (EXIT 0 post-fix) confirm the fail-then-pass property. |
| AC5 | PASS | Ordered gates green in committed evidence: format EXIT 0; analyzer EXIT 0 with zero new warnings on touched files; nullable touched-project build EXIT 0 (solution EXIT 1 is pre-existing vendored SVGControl/UtilitiesSwordfish debt, baseline-identical, zero new diagnostics on touched files); MSTest 202/202 EXIT 0. Changed-line coverage: `HandleInboxItemAddAsync` 100%, `HandleToDoItemChangeAsync` 92.86%; package +0.43 pt — no regression. |
| AC6 | PASS | Production scope is strictly the single handler file (`git diff --name-only ... -- '*.cs' | grep -v TaskMaster.Test` returns only `AppEvents.ReadinessHookup.cs`). The proximate config trigger and the ~40 RibbonViewer async-void handlers remain documented follow-ups in `issue.md`. See note below on the additional test-side files. |

### Note on AC6 (literal enumeration vs. substance)

AC6's literal text enumerates only `AppEvents.ReadinessHookup.cs` and `AppEventsTests.cs`.
The branch also changed three additional test/build files:
`AppEventsTests.Helpers.cs` (new), `AppEventsCoverageExpansionTests.cs` (corrected), and
`TaskMaster.Test.csproj` (compile-include). These are non-discretionary, consequential
changes, not scope creep:

- The helper extraction was required by the 500-line file-size policy (baseline
  `AppEventsTests.cs` was exactly 500 lines; adding two tests would exceed the ceiling).
- The `AppEventsCoverageExpansionTests.cs` edit corrected a pre-existing test that asserted
  the old rethrow behavior; leaving it would fail the suite against the corrected contract.
- The csproj `<Compile Include>` is required for the legacy `packages.config` project to
  compile the new helper file.

Substantive scope creep — new production behavior, RibbonViewer changes, or config-trigger
work — is absent. AC6 is therefore evaluated PASS on substance; the literal file list is
narrower than the necessary mechanical/test-maintenance surface, which is documented here
for transparency.

## Summary

All six acceptance criteria are PASS. The defect fix is contained to a single production
file, is policy-compliant, has deterministic regression coverage that fails pre-fix and
passes post-fix, and the full C# toolchain is green in committed evidence with no new
warnings and no changed-line coverage regression. No PARTIAL, FAIL, or UNVERIFIED criteria.
No blocking findings. Recommendation: go for PR.

## Acceptance Criteria Check-off

All six AC items were already marked `[x]` in `issue.md` by the executor, and this review
confirms each as PASS. No changes to the check-off state are required; all remain `[x]`.

### Acceptance Criteria Status
- Source: docs/features/active/2026-07-07-outlook-crash-async-void-sectiongroupname-270/issue.md
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none
