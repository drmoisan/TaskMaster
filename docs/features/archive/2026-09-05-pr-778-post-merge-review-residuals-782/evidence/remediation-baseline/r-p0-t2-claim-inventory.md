# [P0-T2] Claim inventory — the eight sites this remediation changes

Timestamp: 2026-09-06T01-27

Every line range below was re-derived in this task against the worktree at `HEAD`, and every quoted
block is the current text as it stands before Phase 2 and Phase 3 run. Nothing here is copied from
the remediation plan.

---

## 1. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md:643-654` — AC10

```text
- [x] AC10: `UtilitiesCS/Threading/UiThread.cs` declares exactly one `internal const string` message
      constant whose value is the text stated in the Behavioral Contract section; both throw sites —
      the one in that file and the one in `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` —
      reference it, and no `InvalidOperationException` message literal for this precondition remains
      anywhere in `UtilitiesCS`. The `WpfDispatcherYield` message's former "before yielding folder
      tree work" tail is intentionally gone; that loss is recorded in this delivery's code-review
      artifact as an accepted, reviewed change rather than a regression, and is pinned by the C20
      `WithMessage` assertion in
      `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`. **Evidence:** a grep for
      "before yielding folder tree work" returning zero hits in `UtilitiesCS`; a grep for
      `UiThread.Initialize()` returning zero hits in any message literal or assertion; the passing
      `WithMessage` assertion.
```

## 2. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md:655-662` — AC11

```text
- [x] AC11: The test method `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`
      in `UtilitiesCS.Test/Threading/UiThread_Tests.cs` retains that exact name while its assertion
      changes to `*UiThread.Init()*`, and this delivery's code-review artifact records the residual
      naming inaccuracy and the reason the name is retained: the fully-qualified name is quoted inside
      a TestCaseFilter expression in a committed #584 regression-testing evidence artifact, and renaming would
      make that recorded command resolve to zero tests (SD4). **Evidence:** a grep confirming the
      method name is unchanged and the asserted wildcard is `*UiThread.Init()*`; the code-review
      artifact entry.
```

## 3. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md:166-171` — Behavioral Contract `WpfDispatcherYield` bullet

```text
- **The domain-specific tail "before yielding folder tree work" is removed.** This loss is intended
  (scope decision SD5) and is pinned by an acceptance criterion and by the C20 `WithMessage`
  assertion, so a reviewer does not read it as a regression. Two facts bound the impact: the guard is
  unreachable on the production path, because the production fallback provider throws from
  `UiThread.Dispatcher` first with the same message; and the guard therefore covers only injected
  providers, which are typed `Func<Dispatcher?>` and exist only in tests.
```

## 4. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md:193` — Write Set test-file table row for `UtilitiesCS.Test/Threading/UiThread_Tests.cs`

```text
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 179 lines measured. Host the populated-branch sentinel on a dedicated STA thread with shutdown; move the field null guard into the helper and use expression-bodied throw lambdas; assert `*UiThread.Init()*`; migrate to the install scope; refresh the stale XML-doc prose at line 113. | C06, C10, C11, C12, C13 |
```

## 5. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/code-review.2026-09-05T23-00.md:128-135` — entry (b)

```text
## (b) SD5 — the removed message tail

The `WpfDispatcherYield` message's tail "before yielding folder tree work" is intentionally gone
under SD5. Both throw sites now share the single `UiThread.DispatcherNotInitializedMessage`
constant, whose text is domain-neutral and names no caller-specific operation. This is an accepted
and reviewed change rather than a regression. It is pinned by the `WithMessage("*UiThread.Init()*")`
assertion that P4-T3 added to `YieldAsync_WithoutDispatcher_RemainsStrict`, so a future edit that
changed the constant's text would fail that test.
```

## 6. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/ac-status-summary.2026-09-05T23-15.md:170` — AC10 row

```text
| AC10 | `[x]` | `UtilitiesCS/Threading/UiThread.cs` declares exactly one `internal const string DispatcherNotInitializedMessage` and references it on two lines, one the declaration and one the throw; `WpfDispatcherYield.cs` references it once; the `UtilitiesCS` tree carries zero `before yielding folder tree work` and zero `UiThread.Initialize()`; `YieldAsync_WithoutDispatcher_RemainsStrict` recorded `Passed` |
```

## 7. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/ac-status-summary.2026-09-05T23-15.md:171` — AC11 row

```text
| AC11 | `[x]` | The test method retains its exact name and asserts `WithMessage("*UiThread.Init()*")`; `evidence/other/code-review.2026-09-05T23-00.md` records the SD4 residual naming inaccuracy and the reason the name is retained |
```

## 8. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/baseline/p0-t7-coverage.md:137-150` — superseded-figures section

```text
### Superseded first-party figures, retained for audit and not current

| Figure | Superseded value | Re-measured value |
|---|---|---|
| `lines-covered` | 112359 | 112355 |
| `lines-valid` | 132967 | 132967 |
| line percentage | 84.50% | 84.50% |
| `branches-covered` | 26496 | 26500 |
| `branches-valid` | 33480 | 33480 |
| branch percentage | 79.14% | 79.15% |

Those superseded figures were measured at the orphaned base `b95a5252` and are superseded for the
reason stated at the head of this artifact. A Phase 7 comparison that reads either 112359 or 26496
as its baseline side is invalid.
```

---

## Labelled before-counts

The three `spec.md` counts were taken over
`docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\spec.md` alone, and the
fourth over
`docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\evidence\other\code-review.2026-09-05T23-00.md`
alone. Each was run with `Select-String -SimpleMatch`, so no character in the searched literal is
read as a regular-expression metacharacter.

### Count A — `is pinned by` in `spec.md`

`Select-String -SimpleMatch 'is pinned by' -Path 'docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\spec.md'`

COUNT_IS_PINNED_BY_SPEC: 2

Matching line numbers: 167, 649.

- 167 — the Behavioral Contract `WpfDispatcherYield` bullet, rewritten by [P2-T3].
- 649 — the AC10 pinning clause, rewritten by [P2-T1].

Both occurrences sit inside text Phase 2 replaces, which is why [P2-T4]'s zero-hit assertion is
reachable. The SD5 scope-decision row's `pinned by AC10` wording is a different token and is not
counted by this search.

### Count B — `*UiThread.Init()*` in `spec.md`

`Select-String -SimpleMatch '*UiThread.Init()*' -Path 'docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\spec.md'`

COUNT_WILDCARD_SPEC: 3

Matching line numbers: 193, 657, 661.

- 193 — the Write Set test-file table row, rewritten by [P2-T8].
- 657 and 661 — the two AC11 clauses, rewritten by [P2-T2].

### Count C — `WithMessage(UiThread.DispatcherNotInitializedMessage)` in `spec.md`

`Select-String -SimpleMatch 'WithMessage(UiThread.DispatcherNotInitializedMessage)' -Path 'docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\spec.md'`

COUNT_CONSTANT_TOKEN_SPEC: 0

Matching line numbers: none. The token is absent from `spec.md` before Phase 2, so the positive
counts [P2-T4] and [P2-T8] assert cannot be satisfied by pre-existing text.

### Count D — `would fail that test` in `code-review.2026-09-05T23-00.md`

`Select-String -SimpleMatch 'would fail that test' -Path 'docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\evidence\other\code-review.2026-09-05T23-00.md'`

COUNT_WOULD_FAIL_THAT_TEST_CODEREVIEW: 1

Matching line number: 135, inside the sentence [P2-T5] replaces.

## Consumers

[P2-T4] reads Count A, [P2-T8] reads Count B, and [P2-T5] reads Count D from this artifact. Count C
is the zero before-state that makes the positive assertions in [P2-T4] and [P2-T8] discriminating.
