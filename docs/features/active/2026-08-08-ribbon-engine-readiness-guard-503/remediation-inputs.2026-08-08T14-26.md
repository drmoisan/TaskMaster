# Remediation Inputs — Cycle 1 (Issue #503)

Timestamp: 2026-08-08T14-26
Entered by: orchestrator
Source review: `code-review.2026-08-08T14-15.md`, `policy-audit.2026-08-08T14-15.md`, `feature-audit.2026-08-08T14-15.md`

## Cycle basis

The `feature-review` pass returned **PASS with zero Blocking findings**, so this cycle is **discretionary**, not gate-forced. It is opened because two of the review's Medium findings are defects introduced by this change itself rather than pre-existing conditions, and shipping them would knowingly merge a test that cannot fail and a net-negative edit to a file already over the repository size cap.

Findings that are pre-existing, out of scope, or design questions are **not** in this cycle; they are routed to issues (#504, #505, #506, #507, #508, #509, #510, #511, #512) and must not be touched here.

## Findings to remediate

### F1 — Vacuous assertion in the AC5 ribbon-XML test (Medium, in-scope)

Location: `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`, approximately lines 197-206.

The assertion is written as:

```csharp
...Attribute("getEnabled")?.Value.Should().Be("EngineCommand_GetEnabled");
```

The null-conditional `?.` short-circuits the entire expression, including the `.Should()` call. When the `getEnabled` attribute is **absent** — precisely the regression this test exists to catch — no assertion executes and the test passes silently.

Required outcome: the assertion must fail when the attribute is missing, when it is present with the wrong value, and when it is present but empty. Do not rely on the sibling set-equality test at approximately line 224 to carry the criterion; that test independently enforces AC5 and must remain, but this test must also be non-vacuous.

Acceptance: introduce a deliberate temporary removal of one `getEnabled` attribute from the embedded ribbon resource, confirm this specific test fails, then restore. Record that fail-then-restore proof as regression-testing evidence. The permanent test suite must not contain the temporary mutation.

### F2 — `RibbonExplorer.xml` grew more than the change required (Medium, in-scope)

The file went from 519 to 539 lines. It was already above the 500-line cap before this change, and `spec.md` AC25 records that pre-existing overage as an accepted exception rather than something this change may worsen.

Only 8 of the 23 added lines are functionally required (one `getEnabled` attribute per engine-backed button). Approximately 12 lines came from reformatting three previously single-line `<button>` elements into multi-line form, which is incidental churn with no functional purpose.

Required outcome: restore the three reformatted `<button>` elements to their original single-line form while retaining their `getEnabled` attribute, so the net line growth reflects only the functional change. The eight `getEnabled` attributes must all remain present and correct.

Acceptance: `RibbonExplorer.xml` line count is at or below 527 (519 + 8). All AC5, AC6, and AC7 ribbon-XML tests still pass. The embedded resource still parses as valid CustomUI.

## Explicitly out of scope for this cycle

- The pre-existing nullable debt and the type-check gate defect (issue #512).
- The `CS2002` duplicate compile entry in `UtilitiesCS.Test.csproj` (issue #510).
- The `YieldAsync_WithoutDispatcher_RemainsStrict` flake (issue #508).
- The residual `engine as SpamBayes` / `.Engine` dereference window (Low; the click guard already makes the reported defect unreachable, and narrowing the readiness contract further is a design change, not a fix).
- The `??=` lazy initialiser thread-safety observation (Low; the runner is immutable and the reviewer assessed it benign).
- Any change to `AppItemEngines.cs` or `IAppItemEngines.cs` — AC15 requires these to remain a zero-line diff.
- AC19, AC20, AC21 — MANUAL-ONLY, must remain unchecked.

## Blocking finding count entering this cycle

0 blocking. This cycle is quality-discretionary; its exit gate is the same reaudit standard.
