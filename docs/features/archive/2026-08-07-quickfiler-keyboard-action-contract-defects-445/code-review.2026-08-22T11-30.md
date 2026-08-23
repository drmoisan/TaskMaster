# Code Review — quickfiler-keyboard-action-contract-defects (#445)

- **Artifact:** `code-review.2026-08-22T11-30.md`
- **Reviewer:** feature-review agent
- **Date:** 2026-08-22T11-30
- **Branch:** `bug/quickfiler-keyboard-action-contract-defects-445-exec` @ `1292b4c3` vs `origin/epic/quickfiler-suite-determinism-foundation-integration` (merge-base `c551eaba`)

## Findings Table

| ID | Severity | File / Location | Finding |
|---|---|---|---|
| CR-A1 | Advisory | `evidence/qa-gates/coverage-postchange.2026-08-22T10-38.md` | The +6 vs +11 `lines-covered` discrepancy was disclosed and attributed to dotnet-coverage run-to-run nondeterminism, but the 5 flipped lines were not localized to specific files. Adjudicated as adequate here (see policy audit section 5: in-scope files are at 100% per-file, magnitude is 0.006 pp, inner test run identical). Recommendation for future coverage-delta artifacts: diff the two Cobertura documents per-file to name the flipped files when the aggregate does not reconcile. No action required for this change. |
| CR-A2 | Advisory | `spec.md` AC18 vs `.claude/agent-memory/**` | AC18's literal text reads "No file under `.claude/**` ... is modified", while the branch modifies three files under `.claude/agent-memory/atomic-executor/`. The approved plan (P4-T3) explicitly carves out `agent-memory` with the rationale that agents legitimately persist memory there and an unscoped gate would be unsatisfiable by construction; the review directive permits the same carve-out. Evaluated as satisfying AC18's intent (no rule, hook, skill, or policy file touched). Recommendation: future specs should write the carve-out into the AC text so the criterion and the gate coincide literally. |
| CR-A3 | Advisory | `QuickFiler/Controllers/KaChar.cs:2-5`, `QuickFiler/Interfaces/IKbdAction.cs:2-5` | Pre-existing unused `using` directives remain (`System.Collections.Generic`, `System.Linq`, `System.Text` in both; `System.Threading.Tasks` unused in `IKbdAction.cs`). Pre-existing, analyzer-clean under the repository configuration, and removing them would have widened a minimal bugfix. No action required; candidate for an opportunistic cleanup in a future change that owns these files. |
| CR-A4 | Advisory | repository-wide | C# repo-wide coverage (70.60% line / 58.75% branch) sits below both documented floors, and the 80-vs-85 threshold divergence between CLAUDE.md UT2 and `.claude/rules/quality-tiers.md` remains unadjudicated. Both are pre-existing repository state, tracked in the spec's Rollout & Follow-up, and are not findings against this change (which moved both rates upward). |

**Blocking findings: 0. PARTIAL findings: 0. Advisory findings: 4.**

## Substantive Correctness Review (caller questions)

### Q1 — `IKbdAction` implementer completeness

Verified by full-file reads of the current tree:

- Implementers are exactly `KaChar`, `KaCharAsync` (`KaChar.cs`), `KaKey`, `KaKeyAsync` (`KaKey.cs`), and `KaStringAsync`. Each declares only `SourceId`, `Key`, `Delegate` (of its own stored delegate type), and `KeyEquals` — no implementer reports a delegate type it does not store, because `DelegateType` no longer exists anywhere (repo-wide grep over `*.cs`: 0 hits). The defect-3 misreport (`KaChar` claiming `typeof(Action<Keys>)` for a stored `Action<char>`) is resolved by removal.
- `IKbdAction.cs` (16 lines) contains no commented-out members; the four live members (`SourceId`, `Key`, `Delegate`, `KeyEquals`) are byte-identical to the base version (the diff removes only the two comment lines).
- `KaCharAsync` and `KaKeyAsync` are intact: both retain their constructors, three properties, and `KeyEquals`; both compiled under the analyzer and nullable rebuilds (exit 0, 0 errors, `CoreCompile` skip count 0), which proves interface conformance for all five implementers.
- `Update` is **retained** on `KaStringAsync` (AC9): property with backing field `_update` present, assigned by the five-argument constructor, read at the two guard sites in `KeyEquals`. `Update` is **removed** from the other four implementers (AC8): confirmed absent in the current files.

### Q2 — Blast radius of the new `KeyEquals` precondition (highest-value question)

`KeyEquals` now throws `ArgumentNullException` on a null probe and `ArgumentException` on an empty probe, where an empty probe previously returned `true` for every action. The string-keyed `KbdActions` members that evaluate `x.KeyEquals(key)` — `ContainsKey`, `FilterKeys`, `Find`, `FindIndex`, and the indexer (via `Find`) — inherit the precondition. This reviewer traced every live production path:

1. **The only production consumer of string-keyed `KbdActions` is `KeyboardHandler.StringActionsAsync`** (`KeyboardHandler.cs:83-84`; interface surface at `IQfcKeyboardHandler.cs:26`). No other production type constructs or probes a `KbdActions<string, KaStringAsync, ...>`.
2. **Filter-driven probes cannot be empty.** `KeyboardHandler.cs:180` appends the incoming character to `_filterBuilder` *before* the probes at `:181` (`ContainsKey`) and `:188` (`FilterKeys`), so every filter probe has length >= 1. The decrement at `:200` (`_filterBuilder.Length--`) runs only after an append, so the builder never goes negative, and the next keystroke appends again before the next probe.
3. **Indexer probes use registered keys, which are never empty.** `KeyboardHandler.cs:194` probes with `actions[0].Key`, a key registered via `QfcCollectionController.GenerateStringKbdAction` (`:1363-1385`). `digits` is 1 or 2 on every path (`items.Count >= 10 ? 2 : 1` at `:373`, `:460`, `:595`, `:643`; the `Digits` property at `:114-128` yields only 1 or 2), so the generated key is `"1"`..`"99"`-style and non-empty. The `key = ""` initialization at `:1366` is unreachable as a final value.
4. **`Remove` does not inherit the precondition.** `KbdActions.Remove` (and both `Add` overloads) match via `StoredKeyEquals` (`EqualityComparer<TKey>.Default.Equals`), not `KeyEquals`, so the removal calls at `QfcCollectionController.cs:1349/:1353` are unaffected.
5. **Null probes are impossible** at every site: `_filterBuilder.ToString()` is never null, and `actions[0].Key` is a stored non-null string (constructor lower-cases it).

**Conclusion: no production path can reach the new throw.** The behavior change is observable only to tests and to future callers that pass an empty or null probe — which is exactly the fail-fast contract the spec's Decision 2 records. The XML doc comment on `KeyEquals` explicitly documents the consequence for the string-keyed `KbdActions` members, so the inherited precondition is discoverable at the call-site surface. Additionally, the previous *silent* behavior was itself defective (`FilterKeys("")` returned every action; `Find("")` threw `InvalidOperationException` with >= 2 registrations), so the throw replaces an incoherent semantics rather than a useful one.

### Q3 — Branch 1's early return (latch retention)

Verified in the current `KaStringAsync.cs`:

- Branch 1 still returns `true` immediately without falling through to the trailing `Activated = false` (method body, `return true;` inside the `Key.Contains(other)` block).
- The XML doc `<remarks>` explains **why** in two dedicated paragraphs: the latch contract paragraph states a matching probe "deliberately does not clear the latch and returns early" so the matching element's `Update` continues firing across `KeyboardHandler`'s three passes within one keystroke (that repetition advances the item-number label), and a second paragraph states the early return "is therefore load-bearing and must not be 'completed' into a fall-through ... for symmetry", naming the concrete failure (the `ContainsKey` pass would consume the activation and the label would stop advancing).
- Two tests pin it: the pre-existing `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` (asserts `Activated` remains true after a match; AC3's named witness, passing unmodified) and the new `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar`, whose intent comment states exactly what breaks if the early return is removed and whose assertions (`updates.Equal({"b","a"})`, `toggled == true`, `Activated == false` at the end) would fail under a symmetric fall-through.

### Q4 — Banned-API check

Reviewer grep of all five changed files for `DateTime.Now`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` (plus `Sleep(`, `retry`): **zero hits**. No banned API is present in any file this change touched.

## Code Quality Assessment

- **Guard-clause implementation** is correct and ordered correctly: the null test precedes the length test (the in-code comment explains why), and both throws name `nameof(other)`. The `ArgumentException` message is specific and explains the rationale (empty probe would match every receiver).
- **XML documentation** is unusually thorough for this codebase and is accurate against the implementation: latch contract, argument contract, return semantics, both exceptions, and the caller-consequence paragraph all match the code as written.
- **Test quality** is high: AAA with intent comments; `ThrowExactly` used deliberately (and correctly justified in-comment — `ArgumentOutOfRangeException` derives from `ArgumentException`, so a plain `Throw` would not gate variant 2); `WithParameterName("other")` distinguishes the guard from the library-internal throw (whose parameter is `value`); the dual-variant empty-probe test closes the AOORE path across instance states; the latch-transition test uses a single collection assertion with a comment explaining the AC19 retention-gate interaction.
- **Deletions** (`DelegateType`, dead `Update` x4, two comment lines, one unused `using`) are exactly the spec's Decision 3 and are proven safe by zero read sites plus two full rebuilds.
- **No logging added** — appropriate: the changed types have no logger and `KbdActions` retains its own log4net diagnostics unchanged.
- **Formatting** conforms to the pinned CSharpier (repo-wide check clean at 1517 files).

## Verdict

Ready to merge from a code-quality standpoint: 0 blocking findings, 4 advisory findings, none requiring action on this branch.
