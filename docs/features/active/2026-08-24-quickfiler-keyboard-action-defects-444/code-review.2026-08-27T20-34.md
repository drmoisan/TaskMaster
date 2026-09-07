# Code Review — quickfiler-keyboard-action-defects (Issue #444, closes #472, #482)

- Artifact: `code-review.2026-08-27T20-34.md`
- Branch: `bug/quickfiler-keyboard-action-defects-444` @ `833423ba`
- Diff base: `4f238289` (merge-base with `origin/epic/quickfiler-bug-family-integration`)
- Scope: 3 production files, 4 test files, 1 `.csproj` line (full branch diff)

## Summary

The three fixes are minimal, well-commented, and correctly targeted at the shared root cause (the
`KbdActions` registry admitting inconsistent state through entry points that disagree). No public
API changed, no forbidden file was touched, and every new behavior is pinned by a named test with a
recorded red state (or an explicitly justified pass-after-only pin). Zero blocking findings.

## Findings

| ID | Severity | File / location | Finding |
| --- | --- | --- | --- |
| CR-1 | Observation | `QuickFiler/Controllers/QfcCollectionController.cs` (UnregisterNavigation) | `_registeredDigits` is never reset after `UnregisterNavigation`, so a second unregister without an intervening register replays the previous width. This matches the current call discipline (register/unregister pairs) and the spec design; noted so a future caller does not assume the field self-clears. `KbdActions.Remove` returning silent `false` makes the replay harmless today. |
| CR-2 | Observation | `QuickFiler/Controllers/QfcItemController.Navigation.cs` (SyncExpandedRegistrations) | In `ToggleExpansionAsync`, `SyncExpandedRegistrations(_expanded)` runs on the continuation after `_uiDispatcher.InvokeAsync`, i.e., registry mutation occurs off the dispatcher exactly as `RegisterExpandedAsyncActions` did before this change. Not a regression; the registries remain single-UI-flow structures. Recorded because the new method now mutates both registries, widening the surface that assumption covers. |
| CR-3 | Observation | `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` (BuildExpansionHarness) | The spec clause "ItemHelper.UnRead is false, established explicitly rather than by relying on a default" is implemented as a guard assertion (`helper.UnRead.Should().BeFalse(...)`) rather than an assignment, because the setter writes through to `Item.Save()` on a null Outlook item. The assertion pins the arrangement and fails fast at Arrange if the default ever changes, which satisfies the criterion's intent (no silent reliance on a default). The rationale is documented in the harness doc comment. |
| CR-4 | Observation | `QuickFiler/Controllers/KbdActions.cs:44` | On a null seed, `ArgumentNullException` originates from the `List<UClass>` copy constructor with parameter name `collection`, not `list`. The test asserts only the exception type, so this is consistent; noted for anyone later tightening the assertion to `ParamName`. |
| CR-5 | Observation | `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` | File is at 498 of 500 lines (base 391, +107 here). The next addition must split the file (mirrors policy-audit OB-3). |

No Blocking and no Non-blocking code defects were found in the changed lines.

## Production Code Review

### `QuickFiler/Controllers/KbdActions.cs` (+36)

- The `IEnumerable<UClass>` constructor now materializes the seed once
  (`_list = new List<UClass>(list)`), preserving the `ArgumentNullException` contract and avoiding
  double enumeration of one-shot sequences, then runs an O(n^2) pairwise scan using `SourceId`
  equality plus `StoredKeyEquals`. Matching on `StoredKeyEquals` rather than the element-defined
  `KeyEquals` is correct: `KaStringAsync.KeyEquals` matches substrings and can carry side effects,
  and the choice is pinned by
  `KbdActionsTests.EnumerableConstructor_WhenStoredKeysDifferButKeyEqualsOverlaps_DoesNotThrow`.
- The exception message contains the literal `already exists` (consistent with `Add`), and
  `logger.Error(message)` precedes the throw, matching the file's established fail-fast + log
  pattern (`Find`/`FindIndex`/`Add`). Complies with the error-handling and logging policy.
- The O(n^2) choice is justified in a why-comment (seed lists hold at most eight entries; a hash set
  would require an `IEqualityComparer<TKey>`). Simplicity-first is the right call here.
- XML doc comments state the contract, the exception condition, and the comparison rationale —
  meets the public-contract documentation requirement.
- No change to `Remove`, `Add`, `Find`, or any public member; the upstream contract for #464/#489 is
  intact (verified against the diff; no `TryRemove`-style member exists anywhere in source).

### `QuickFiler/Controllers/QfcCollectionController.cs` (+8/-8)

- New private field `_registeredDigits` with an issue-tagged why-comment; assigned in
  `RegisterNavigation` from the already-captured `digits` local, so register and unregister now share
  one width source.
- `UnregisterNavigation` computes `var format = _registeredDigits == 2 ? "00" : ""` once and drops
  the per-iteration `Digits` reads entirely (verified: zero `Digits` occurrences in the body). The
  `== 2` form deliberately maps the uninitialized-object case (`0`) to single-digit, documented in a
  comment and required by the spec.
- The edit collapses a duplicated if/else into one loop body — a small net readability gain inside a
  file this feature is otherwise not allowed to restructure.

### `QuickFiler/Controllers/QfcItemController.Navigation.cs` (+28/-4)

- `SyncExpandedRegistrations(bool)` is the new single owner of the four expansion
  register/unregister calls: unconditional unregister of both registries, then conditional
  re-register of both. It is idempotent by construction because `KbdActions.Remove` returns `false`
  for absent pairs rather than throwing; the remarks block documents exactly that dependency, which
  is the load-bearing design fact.
- Both `ToggleExpansion` overloads now call `SyncExpandedRegistrations(_expanded)` after
  `ToggleExpansionOn()/Off()` writes `_expanded`, keying registration on actual state instead of on
  which code path ran. This closes the #482 divergence and deliberately widens behavior (sync toggle
  now maintains the async registry and vice versa) — the widening is disclosed in the spec and is a
  deferred PR-body item.
- The overloads retain their signatures, `virtual` modifiers, and `[ExcludeFromCodeCoverage]`
  attributes; the new method carries no coverage exclusion and measures 100% line and branch
  coverage. Verified against the diff and the raw Cobertura document.

### `QuickFiler.Test/QuickFiler.Test.csproj` (+1)

Exactly one `<Compile Include>` line for the new test file, inserted in the slot the spec reserved
between the `QfcCollectionControllerTests.cs` and `QfcCollectionControllerDarkModeTests.cs` entries.
No other line changed.

## Test Quality (general-unit-test.md, CUT1/CUT2)

- Framework and libraries: MSTest attributes, Moq mocks, FluentAssertions with `because:` messages
  throughout — compliant.
- Independence/isolation/determinism: no shared mutable state between tests; every controller is
  built per-test via `FormatterServices.GetUninitializedObject` plus reflection injection; no
  external services, no temporary files, no wall-clock waits, no `Thread.Sleep`/`Task.Delay`
  (verified by grep over all four files). The #482 harness keeps `UnRead` false specifically to keep
  the 4000 ms `System.Threading.Timer` out of the arrangement.
- Structure: the #444/#472 tests carry explicit Arrange/Act/Assert comments; the #482 tests are
  AAA-shaped without section comments but are short enough that intent stays clear, and every test
  carries an issue-tagged doc comment stating scenario and expected outcome.
- Scenario completeness for the new ctor guard: positive (duplicate-free seed), negative (exact
  duplicate throws with message), null input, boundary (same key under different `SourceId`;
  substring-overlapping stored keys), plus the decision pin for `RegisterAsyncKeyActions`
  cardinality. For #472: both directions across the 10-item digit-width boundary. For #482:
  interleaving, collapse, idempotence, and direct invocation of the private owner. Complete for the
  changed behavior.
- The #472 width-fidelity test asserts the residual `"10"` entry as an explicit exact bound with a
  doc comment attributing it to the separately promoted count-mismatch defect (#644) — this prevents
  the test from silently endorsing the out-of-scope defect, as the spec requires.
- Test file placement follows the repository's established convention
  (`QuickFiler.Test/Controllers/` mirroring `QuickFiler/Controllers/`); consistent with every
  sibling test in this solution.

## Executor Disclosure Assessments

1. **[P4-T7] TRX normalization (`&lt;repo-root&gt;` re-encoding)** — Sound. A raw `<` is illegal in
   XML text nodes and attribute values, so the literal placeholder had to be entity-encoded; the
   stored value decodes to the intended literal `<repo-root>`. The executor re-verified
   well-formedness and result counts after re-encoding. Reviewer independently re-parsed the TRX:
   well-formed, 6713 `UnitTestResult` elements all `Passed`, zero user-path prefixes, 6713 of 6713
   `computerName="host"`. The disclosure is recorded in the artifact itself, not papered over.
   Accepted.
2. **[P5-T31] structurally unsatisfiable acceptance clause** — Substantive condition met. The clause
   required the single dirty path at capture to be the [P5-T31] artifact itself, but [P5-T30] is the
   commit task, so its own checkbox edit necessarily post-dates its commit; no execution order can
   satisfy the literal clause. The executor recorded the true capture (the plan file, one checkbox
   line), stated the structural reason instead of rewording the clause, and demonstrated the
   substantive terminal condition via the [P5-T32] addendum (post-commit scoped `git status
   --porcelain` empty). Reviewer confirmation: repo-wide `git status --porcelain` is empty at review
   time. Accepted; this is honest evidence handling, not a defect.

## Verdict

Zero blocking findings. The change is approved from a code-quality standpoint; the three deferred
PR-body items remain with the orchestrator (see the feature audit).
