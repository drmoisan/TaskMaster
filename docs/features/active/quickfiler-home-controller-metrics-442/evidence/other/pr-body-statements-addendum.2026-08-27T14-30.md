# PR Body Statements — Addendum

Timestamp: 2026-08-27T14-30
Task: [P7-T1] follow-up; the base artifact is `pr-body-statements.2026-08-26T11-31.md`
Command: not applicable; this artifact records additional required PR body content
EXIT_CODE: 0

The base artifact `evidence/other/pr-body-statements.2026-08-26T11-31.md` remains authoritative for
its six required statements and its four test dispositions. This addendum supersedes only its final
section, "Outstanding item the PR body must also disclose", and adds three further disclosures.

## 1. Supersedes: the outstanding failing test is resolved

The base artifact closed by requiring the PR body to disclose one failing test:

`QuickFiler.Controllers.Tests.EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`

**That test now passes.** Commit `889fa298` changed
`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:64` from
`SetField(controller, "_isExecuting", true)` to `SetField(controller, "_isExecuting", 1)`. The
coverage-enabled suite at 2026-08-27T14:19:36Z records 6701 tests, 6701 passed, 0 failed, and the
run log records that test as `Passed`. Evidence:
`evidence/qa-gates/mstest-coverage.2026-08-27T14-19.md`.

The PR body must therefore **not** carry the base artifact's "one failing test" disclosure. It must
instead carry the disclosure in section 2 below.

## 2. New disclosure: one plan-forbidden file was written, under parent ratification

The PR body must state that `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` is on this
plan's forbidden-to-write list and that this change writes one line of it, so the forbidden-file
ownership gate [P7-T6] is **not** clean and is recorded as a documented deviation rather than
checked off.

The reasoning the PR body must summarise:

- AC-14 and [P3-T5] require `_isExecuting` to be `private int`, consumed via
  `Interlocked.CompareExchange(ref _isExecuting, 1, 0)`.
- `FieldInfo.SetValue` rejects a boxed `System.Boolean` for a `System.Int32` field, so the sibling
  test threw `ArgumentException` and no production-side change could make the suite green.
- The parent epic-orchestrator verified that the integration history holds exactly four commits
  touching that file (`23935185`, `ceadcd8a`, `44bfdf20` for coverage seams #236, and `88366ad4`
  for store-disable-service #261) and that **none** belongs to epic sibling 446, 468, 498 or 484.
  The ban's fan-in rationale does not apply to this file, so the write cannot break fan-in.
- This branch alone made the field an `int`, so it is both the sole cause and the sole legitimate
  fixer, and the repository's breaking-change rule requires updating in-repo callers.
- The ratification covers this one file on this one feature only.

Full record: `evidence/qa-gates/ownership-gate.2026-08-27T14-03.md`.

Consequence for acceptance criteria: **AC-19 is left unchecked.** Its first sentence requires the
diff to list only the five owned production files, the two owned test files and paths under the
feature folder, and this diff carries an eighth source path. The criterion is not claimed.

## 3. New disclosure: CFN-4 is promoted to issue #645

The PR body must state that cross-feature note CFN-4 — the 12-hour `"hh:mm"` time format with no
AM/PM designator at `QfcHomeController.Metrics.cs:31` and `:110` and
`EfcHomeController.Metrics.cs:68` — is promoted to its own issue,
https://github.com/drmoisan/TaskMaster/issues/645, verified `OPEN`, and that the number is written
back into the CFN-4 section of `spec.md`. It is **not** fixed here. Evidence:
`evidence/issue-updates/cfn4-promotion-complete.2026-08-27T13-59.md`.

This supersedes the `PROMOTION BLOCKED` disposition recorded on 2026-08-26, which reflected a
session that lacked the promotion MCP tools.

## 4. New disclosure: the coverage exception, stated rather than absorbed

The PR body must state the coverage outcome including its one shortfall:

- repository-wide line coverage moved **up**, 84.8433% to 85.1255%; branch coverage moved up,
  78.8181% to 79.2096%;
- changed-line coverage is **39 of 39, 100.00%** — no line this change touched lost coverage;
- five of the six members named in the spec's Test Strategy are at 100.00%;
- `QuickFileMetrics_WRITE` aggregates **88.37%** against a 90% target. The entire shortfall is ten
  lines of inline Outlook `AppointmentItem` creation in the QFC overload, unreachable unless
  `UtilitiesCS.Calendar.GetCalendar("Email Time", ...)` returns a live MAPI calendar. That overload's
  figure is unchanged from baseline at 39/49 on both sides. Excluding those ten Interop lines the
  member is 76/76 = 100.00%.

Evidence: `evidence/qa-gates/coverage-delta.2026-08-27T14-19.md`.

## 5. Closing references

The PR targets `epic/quickfiler-bug-family-integration`, not the default branch. GitHub registers
closing references only for pull requests targeting the default branch, so this merge does **not**
close #442, #443 or #451. The PR body may reference them but must not assert that the merge closes
them, and no acceptance criterion worded "closed by the merge" may be checked on that basis.

All three issues were verified `OPEN` with `gh issue view` immediately before PR creation:
#442 `Bug: qfc-home-controller-metrics-never-flushed`,
#443 `Bug: qfc-home-controller-metrics-duration-misread`,
#451 `Bug: efc-home-controller-metrics-inert-duration`.
