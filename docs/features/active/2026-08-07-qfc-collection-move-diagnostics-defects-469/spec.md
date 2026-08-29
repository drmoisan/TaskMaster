# qfc-collection-move-diagnostics-defects (Spec)

- **Issue:** #469
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug — this file is the sole acceptance-criteria source. No `user-story.md` is produced for this feature.

> **Scope statement.** This is a comment-and-documentation-accuracy change, not a defect fix. Three of
> issue #469's four defects are already remediated and merged. The fourth defect's only remaining
> action is tracked as a separate open issue. This change delivers no behavior change and does not
> deliver issue #469's Expected Behavior item 4.

## Context

Issue #469 filed four defects in the move and move-diagnostics path of
`QuickFiler/Controllers/QfcCollectionController.cs`. The state of those four defects was verified
against `origin/main` at commit `ecdb1c84`. The verification is recorded in
docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/research/2026-08-29T12-31-qfc-collection-move-diagnostics-defects-469.md
and every fact below is cited there.

| #469 defect (issue numbering) | State on `origin/main` | Evidence |
|---|---|---|
| 1 — unreachable null guard in `GetMoveDiagnostics` | Remediated and merged (landing commit `137ee307`) | Guard dominates every dereference; regression test `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` |
| 2 — trailing null element in the returned array | Remediated and merged (landing commit `137ee307`) | Allocation is `new string[_itemGroupsToMove.Count]`; regression tests `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine` and `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` |
| 3 — positional access into an unordered `ConcurrentDictionary` | Remediated and merged (landing commit `d512fcfe`) | Field is now `IReadOnlyList<QfcItemGroup>`; regression tests `ItemGroupsToMoveFieldDeclaresAnOrderedContract` and `TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation` |
| 4 — `MoveEmailsAsync` ignores `stackMovedItems` | **Not** satisfied against the issue's literal Expected Behavior (deferral recorded by commit `613e88c3`) | Parameter retained, explicitly discarded, and documented as a deliberate deferral |

All defect-1/2/3 regression tests live in
`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`.

Defect 4's Expected Behavior text is "Either `MoveEmailsAsync` populates the undo stack it is handed,
or the parameter is removed from the contract." Neither disjunct is met. The delivered route was a
third one: document the true mechanism (undo records reach the stack through the email filer, not
through this argument) and defer removal. **Parameter removal is already tracked as open GitHub issue
#629, "Refactor: Remove the stackMovedItems parameter from MoveEmailsAsync."** Issue #629 is out of
scope here and must not be duplicated.

The residual work genuinely attributable to issue #469 is therefore documentation accuracy only:
two stale comments left behind by the defect-2 fix, and a defect-numbering inversion between the
published issue text and the shipped source comments.

Environment:
- OS/version: n/a (comment and documentation edits only)
- Command/flags used: n/a
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Severity is Low. The stale comments misstate the reason a defensive filter is retained; a maintainer
acting on the stale text could delete the filter, which is the only concrete harm and which the
existing test suite already blocks.

## Repro & Evidence

This section records the verification that establishes the change footprint. There is no runtime
repro, because there is no defective runtime behavior remaining in the #469 surface.

### E-1 — Stale comment in production source

`QuickFiler/Controllers/QfcHomeController.Metrics.cs:171-173` currently reads:

```
// GetMoveDiagnostics returns an array one element longer than it fills, so its trailing
// element is null; dropping null and whitespace-only entries keeps blank rows out of
// the CSV.
```

The statement is false as of the defect-2 fix. `QuickFiler/Controllers/QfcCollectionController.cs:2366`
allocates exactly `_itemGroupsToMove.Count`, the loop bound at `:2367` is read from the same
expression, and every index is assigned on both branches of the loop body.

### E-2 — Same stale sentence duplicated in test source

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:397-400` repeats the same false
sentence as the XML doc comment of `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`. The
test body is correct; only its stated justification is stale.

### E-3 — The filter is still load-bearing and must not be deleted

Measured against the production implementation, the `.Where(line => !string.IsNullOrWhiteSpace(line))`
filter at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:174` is vacuous. Measured against the
interface contract it is not. The call is made through `IQfcCollectionController.GetMoveDiagnostics`,
declared at QuickFiler/Interfaces/IQfcCollectionController.cs:122-129 with no XML documentation and
therefore no non-null element guarantee. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:403`
feeds `new[] { "line-one", "   ", null, "line-two" }` through a `Mock<IQfcCollectionController>` and
asserts only `"line-one"` and `"line-two"` reach the writer. Deleting the filter fails that test.

### E-4 — Defect-numbering inversion

| Source | "defect 1" means | "defect 2" means |
|---|---|---|
| docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/issue.md:30 and :41 | unreachable null guard | trailing null element |
| docs/features/active/qfc-collection-controller-defects-468/spec.md:92-93 | unreachable null guard | trailing null element |
| `QuickFiler/Controllers/QfcCollectionController.cs:2362` and `:2372` | trailing null element | unreachable null guard |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:275`, `:306`, `:313`, `:340`, `:352`, `:387` | trailing null element | unreachable null guard |

The published issue and the #468 spec agree with each other. The shipped source and test comments
agree with each other and disagree with both. The published GitHub issue text is authoritative; the
code comments are corrected to match it.

### E-5 — Resolved cross-feature note

docs/features/active/quickfiler-home-controller-metrics-442/spec.md:869-876 (note CFN-2) asserts that
the trailing null "becomes a blank CSV line the moment #442 lands." That hazard no longer exists
after the defect-2 fix.

## Scope & Non-Goals

### In scope

| Item | File | Change |
|---|---|---|
| A | `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (lines 171-173) | Replace the false trailing-null justification with the real reason the filter is retained: the call is made through `IQfcCollectionController.GetMoveDiagnostics`, which carries no XML documentation and therefore no non-null element guarantee, so the filter defends the interface contract rather than a known producer defect. The `.Where(` filter expression itself is retained unchanged. |
| B | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (lines 397-400) | Correct the same false sentence in the XML doc comment of `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`. The test body does not change. |
| C1 | `QuickFiler/Controllers/QfcCollectionController.cs` (lines 2362, 2372) | Swap the defect numbers so the comment adjacent to the diagnostics-array allocation cites defect 2 and the comment adjacent to the `if (qf is null)` guard cites defect 1, matching the published issue. |
| C2 | `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` (lines 275, 306, 313, 340, 352, 387) | Same renumbering in the test doc comments and `because:` strings. Comment and string-literal text only. |
| D | `docs/features/active/quickfiler-home-controller-metrics-442/spec.md` (lines 869-876) | Mark cross-feature note CFN-2 resolved, citing the landed defect-2 fix. |

### Out of scope / non-goals

1. **Removing the `stackMovedItems` parameter from `MoveEmailsAsync`.** That is GitHub issue #629,
   which is open and separately tracked. It must not be duplicated, partially absorbed, or
   pre-empted by this change.
2. **Any change to QuickFiler/Controllers/QfcFormController.EventHandlers.cs.** That file was
   deliberately protected by decision D11 of the #468 plan and is issue #629's file. It must not
   appear in this change's diff.
3. **Re-fixing #469 defects 1, 2 or 3.** They are delivered on `origin/main` with regression tests.
   No behavior change is made to `GetMoveDiagnostics` or `TryGetItemGroupByIndex`.
4. **QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1272**, which still carries the pre-fix
   `new string[EmailsLoaded + 1]` shape. That file is not listed in QuickFiler/QuickFiler.csproj and
   is not compiled. It belongs to whatever issue owns legacy-file deletion.
5. **Deleting the whitespace filter in `QuickFiler/Controllers/QfcHomeController.Metrics.cs`.** The
   filter is retained. Deleting it fails `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`.
6. **Splitting `QuickFiler/Controllers/QfcCollectionController.cs`.** The file is 2,437 lines, over
   the 500-line cap, under an explicit no-split constraint with decomposition delegated to open
   issue #623.
7. **The separately discovered defect in TaskMaster/AppGlobals/AppAutoFileObjects.cs:43-50**, where
   `Initialized<T>` takes the backing field by value and never memoizes, so a property whose loader
   returns null re-invokes the loader on every read. Different assembly; not one of #469's four
   defects. Recorded here as a follow-up candidate only.

### Explicitly excluded systems, integrations, or datasets

- No interface, DTO, config schema, or serialized format is touched.
- No new test file, test method, or `Compile Include` entry is added to any csproj.
- No coverage-configuration file is touched.

## Root Cause Analysis

The stale comments are a documentation-drift consequence of a correct fix. The defect-2 remediation
changed the allocation from `Count + 1` to `Count` in
`QuickFiler/Controllers/QfcCollectionController.cs`, but the two consumer-side comments that
justified the defensive filter in `QuickFiler/Controllers/QfcHomeController.Metrics.cs` and its test
were written against the pre-fix producer and were not revisited. The filter survived the fix for a
different and still-valid reason — the interface contract offers no non-null guarantee — but that
reason was never written down, so the surviving justification is now the only one on record and it
is false.

The numbering inversion arose because the #468 implementation work labelled the defects in source
order (the allocation appears above the guard in the file) rather than in the order the published
issue enumerates them. Both labels are internally consistent; they disagree across the
issue/code boundary.

## Proposed Fix

### Design summary (what changes where)

Five files receive edits. Four are comment-only or XML-doc-only edits in C# sources; one is a
Markdown status update in an unrelated feature's spec. No executable statement, expression,
signature, attribute, or `using` directive changes anywhere.

### Boundaries and invariants to preserve

- The `.Where(line => !string.IsNullOrWhiteSpace(line))` expression in
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs` is preserved verbatim.
- The body of `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` in
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` is preserved verbatim.
- `QuickFiler/Controllers/QfcCollectionController.cs` must not grow past 2,437 lines. The file is
  already over the 500-line cap under the #623 no-split constraint, so this change must be
  net-neutral or net-negative in lines for that file.
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` is 498 lines with one
  line of headroom. Renumbering must not add lines. Rewrapping a `because:` string is permitted only
  if the total line count does not increase.
- The passing-test count of the `QuickFiler.Test` assembly is unchanged.

### Dependencies or blocked work

- Issue #629 (parameter removal) is independent of this change and is not blocked by it. If #629
  lands first, item C's line numbers in `QuickFiler/Controllers/QfcCollectionController.cs` shift but
  the comment text targeted is unchanged.
- Issue #623 (file decomposition) is unaffected.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

- `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
- `QuickFiler/Controllers/QfcCollectionController.cs`
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`
- `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`

#### Functions/classes/CLI commands impacted

No function or class behavior is impacted. The comments edited are adjacent to
`QfcHomeController.WriteMetricsAsync`, `QfcCollectionController.GetMoveDiagnostics`, and the test
methods listed in the in-scope table. No CLI command is affected.

#### Data flow and validation changes

None. The diagnostics array, the whitespace filter, and the writer seam are unchanged.

#### Error handling and logging updates

None.

#### Rollback/feature-flag considerations (if applicable)

Not applicable. A comment-only change carries no runtime rollback surface; reverting the commit is
sufficient.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

Unchanged. `GetMoveDiagnostics` continues to return `string[]` of length
`_itemGroupsToMove.Count`; `MetricsFileWriter` continues to receive the filtered array.

#### Required configuration keys and defaults

None.

#### Backward-compatibility expectations

Full source and binary compatibility. No public or internal signature changes.

#### Performance constraints (latency/throughput/memory)

None. No executable line changes, so no measurable performance delta is possible.

## Assumptions, Constraints, Dependencies

- **Assumptions:** `origin/main` remains at or after `ecdb1c84` when this change is prepared. If the
  #469 surface changes before the work lands, the cited line numbers must be re-verified before the
  edits are applied.
- **Constraints:**
  - `QuickFiler/Controllers/QfcCollectionController.cs` is 2,437 lines and cannot grow (see the
    invariants above).
  - QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs is exactly at the 500-line cap and
    must not receive additions. This change adds nothing to it and that file is out of scope.
  - Markdown files are exempt from the 500-line cap, so the `docs/` edit is unconstrained.
- **External dependencies:** none. No package, service, or release is involved.

## Data / API / Config Impact

- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates: none. The CSV metrics output is byte-identical before and after.
- Compatibility notes: none. No CLI flag, config schema, or version is affected.

## Test Strategy

The repository's Bugfix Workflow requires a failing regression test before a fix. That requirement
does not apply here: comment text has no observable behavior, so no deterministic red state exists
and no new test can be authored that would fail before the change and pass after it. This is stated
explicitly as a policy exception rather than silently skipped.

- **Regression tests to add or update:** none. No test method is added, removed, or renamed. The
  only test-file edits are XML doc comments and `because:` string literals.
- **Existing tests that act as the guard:**
  - `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` in
    `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` pins that the whitespace filter is
    not deleted.
  - `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine`,
    `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls`, and
    `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` in
    `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` pin the three landed
    fixes and must continue to pass with the renumbered doc comments.
- **Edge cases and negative scenarios:** none applicable; there is no new input surface.
- **Error handling and logging verification:** none applicable.
- **Coverage impact and targets for changed lines/modules:** no coverage delta is expected or
  required. `QuickFiler/Controllers/QfcCollectionController.cs` carries `[ExcludeFromCodeCoverage]`
  at line 21, so no coverage criterion is attributable to it. No coverage-increase criterion is
  authored anywhere in this spec.
- **Toolchain commands to run (format, then analyzers, then nullable, then test):**
  1. `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- **Manual validation steps:** capture the `QuickFiler.Test` passing-test count before any edit and
  again after, and compare. Both figures and the full test-run output belong under
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/regression-testing/`
  per the repository evidence-location conventions.

## Acceptance Criteria

- [ ] AC1 — `QuickFiler/Controllers/QfcHomeController.Metrics.cs` contains zero occurrences of the token `one element longer`. Scoped to that named file only; the token legitimately remains in this feature folder's issue.md, spec.md, and research document, so a repository-wide gate is not used.
- [ ] AC2 — The replacement comment in `QuickFiler/Controllers/QfcHomeController.Metrics.cs` states the interface-contract reason: the file contains the token `IQfcCollectionController` within the comment block immediately preceding the filter, and the file still contains the token `.Where(`.
- [ ] AC3 — `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` contains zero occurrences of the token `one element longer`. Scoped to that named file only.
- [ ] AC4 — The existing test `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` passes.
- [ ] AC5 — In `QuickFiler/Controllers/QfcCollectionController.cs`, the comment immediately preceding the diagnostics-array allocation contains the token `Issue #469 defect 2`, and the comment immediately preceding the `if (qf is null)` guard contains the token `Issue #469 defect 1`. This matches the numbering published in issue.md.
- [ ] AC6 — In `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`, the doc comments and `because:` strings of the three array-length tests cite defect 2 and those of the null-guard test cite defect 1, matching issue.md. The three test method bodies are unchanged.
- [ ] AC7 — Zero executable lines change. The diff against `origin/main` restricted to `QuickFiler/` and `QuickFiler.Test/` touches only comment lines, XML doc lines, and `because:` string literals.
- [ ] AC8 — `QuickFiler/Controllers/QfcCollectionController.cs` line count does not increase above 2437.
- [ ] AC9 — The full `QuickFiler.Test` assembly passes with the same passing-test count as the pre-change baseline, and no test method is added or removed.
- [ ] AC10 — The full C# toolchain passes in order: `dotnet tool run csharpier check .`, then msbuild with `EnableNETAnalyzers` and `EnforceCodeStyleInBuild`, then msbuild with `TreatWarningsAsErrors`, then `vstest.console.exe` with `/EnableCodeCoverage`.
- [ ] AC11 — Cross-feature note CFN-2 in `docs/features/active/quickfiler-home-controller-metrics-442/spec.md` is marked resolved.
- [ ] AC12 — Scope boundary holds: `git diff origin/main --name-only` does not list QuickFiler/Controllers/QfcFormController.EventHandlers.cs, and the token `StackMovedItems` is still present in QuickFiler/Interfaces/IQfcCollectionController.cs, proving issue #629 was not absorbed. Casing note: the issue text and the implementation use the camelCase form `stackMovedItems`, but the interface declares the parameter as `StackMovedItems`; the asserted token uses the interface's casing so the assertion is satisfiable as written.
- [ ] AC13 — The pre-change and post-change `QuickFiler.Test` passing-test counts are recorded as evidence under `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/regression-testing/`, per the repository evidence-location conventions.

## Risks & Mitigations

**Technical or operational risks**

1. *A reader interprets the corrected comment as license to delete the now-explained filter.* The
   replacement text states that the filter defends the interface contract and is exercised by an
   existing test, which makes the dependency explicit. AC2 and AC4 together pin both the text and
   the behavior.
2. *The renumbering edit accidentally alters a `because:` string in a way that changes assertion
   semantics.* `because:` strings are diagnostic text only and do not affect pass/fail. AC7 and AC9
   bound the risk: no executable line changes and the passing count is unchanged.
3. *Rewrapping comment text pushes `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`
   past 500 lines (one line of headroom) or grows `QuickFiler/Controllers/QfcCollectionController.cs`.*
   AC8 gates the production file directly; the test-file constraint is stated as an invariant and
   the renumbering is a token-for-token swap that needs no additional lines.
4. *Scope creep into issue #629.* AC12 fails if QfcFormController.EventHandlers.cs appears in the
   diff or if the interface parameter is removed.
5. *CSharpier reformats a rewrapped comment block, producing an unexpected diff.* The toolchain is
   run format-first and AC10 requires `csharpier check .` to pass, so any reformat is surfaced before
   review rather than after.

**Mitigations and rollbacks**

Revert the single commit. No runtime state, data, or configuration is affected, so no forward
migration or cleanup is needed.

## Rollout & Follow-up

**Release/rollout steps**

Normal branch, PR, and merge. No staged rollout, feature flag, or coordination with another team is
required.

**Post-fix monitoring or clean-up tasks**

- None at runtime. There is no behavior to monitor.
- After merge, issue #469 can be closed with a comment recording: (i) the defect 1/2/3 evidence and
  landing commits `d512fcfe`, `137ee307`, `613e88c3`; (ii) the defect-4 triage conclusion, namely
  that the undo record is not dropped in the shipped configuration and the sole remaining action is
  parameter removal; and (iii) the pointer to issue #629 as the owner of that action.

**Follow-up candidates (not opened by this change)**

- TaskMaster/AppGlobals/AppAutoFileObjects.cs:43-50 — `Initialized<T>` accepts the backing field by
  value and never assigns it, so a property whose loader returns null re-invokes the loader on every
  read and can hand out distinct instances. Affects `MovedMails`, `Encoder`, and `SubjectMap`.
  Recorded as a candidate only; promotion is a separate decision.
- The unfiltered `GetMoveDiagnostics` call site in `QfcHomeController.QuickFileMetrics_WRITE` has no
  found production caller while the filtered async path is the live one. This asymmetry is harmless
  today and is noted, not addressed.

**Links**

- Issue: https://github.com/drmoisan/TaskMaster/issues/469
- Related open issues: #629 (remove the `stackMovedItems` parameter), #623 (decompose
  `QfcCollectionController.cs`)
- Verification research: docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/research/2026-08-29T12-31-qfc-collection-move-diagnostics-defects-469.md
- Prior feature that landed defects 1-3: docs/features/active/qfc-collection-controller-defects-468/
