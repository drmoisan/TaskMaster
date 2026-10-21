# Research: KaStringAsync.KeyEquals branch-1 offset correction (Issue #583)

- Timestamp: 2026-09-12T10-35
- Scope: preparation-only research. No production source edited, no build or test run performed.

## R1 — Current branch-1 expression and surrounding structure

File: `QuickFiler/Controllers/KaStringAsync.cs`.

`KeyEquals(string other)` spans lines 106-145. The guard clauses (null at 110-113, empty at
115-123) run first. The branch-1 block is lines 125-130:

```csharp
125  if (Key.Contains(other))
126  {
127      if (Activated && Update is not null)
128          Update(Key.Substring(other.Length - 1, 1));
129      return true;
130  }
```

Branches 2 and 3 follow as `else if (other.Length == 1)` (131-135, `ToggleControl`) and
`else if (other.Length > 1)` (136-142, `Update(Key.Substring(0, 1))` then `ToggleControl`), with
the shared `Activated = false; return false;` trailer at 143-144. Only the branch-1 offset
(line 128) is in scope per the maintainer decision; branch 3's `Key.Substring(0, 1)` (line 139)
is untouched by Option 2 and is a separate, already-correct expression (first character of the
key, unconditional on match position).

## R2 — Comparison semantics (Contains vs IndexOf) and analyzer exposure

**Target framework.** `QuickFiler/QuickFiler.csproj` line 13: `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` — legacy non-SDK-style .NET Framework 4.8.1 project (packages.config, explicit `<Compile>` items).

**Ordinal-vs-culture asymmetry.** `string.Contains(string)` performs an ordinal (culture-insensitive) search in both .NET Framework and .NET; `string.IndexOf(string)` with no `StringComparison` argument performs a culture-sensitive search using the current culture in .NET Framework. This is the exact asymmetry CA1307 ("Specify StringComparison for clarity") and CA1310 ("Specify StringComparison for correctness") exist to flag: CA1310 is the one Microsoft's guidance associates with `string.IndexOf(string)`, because that overload's default behavior is culture-dependent and can differ from an ordinal caller's expectation; CA1307 is the milder, clarity-only counterpart typically associated with methods such as `Contains`/`Equals` whose behavior does not actually vary by culture.

**Can they disagree for this input domain?** No evidence of disagreement was found or is expected for this domain. The registered keys are two-digit numeral strings (`"01"`–`"12"`, confirmed at `QfcCollectionController.cs:1109-1112`, `key = (i + 1).ToString("00")`) and the probes are the accumulated single/short ASCII-digit filter string built in `KeyboardHandler.cs:180` (`_filterBuilder.Append(char.ToLower((char)e.KeyValue))`). ASCII digit characters (`0`–`9`) have no culture-specific collation, casing, or normalization differences under any standard .NET culture, so an ordinal search and a current-culture search over this restricted alphabet locate the same substring at the same index. This is a characterization from documented framework behavior over the enumerated input alphabet, not a runtime observation (no code was executed for this research).

**Analyzer configuration search.** `.editorconfig` (repo root) was searched in full for `CA1307`/`CA1310`: no explicit entries exist. The file instead carries a single blanket default at line 27:

```
dotnet_analyzer_diagnostic.severity = suggestion
```

with the comment (lines 23-26) stating this default exists specifically "so [new analyzer diagnostics] cannot be promoted to errors under the nullable `/p:TreatWarningsAsErrors=true` build." Every explicit override in the file (Meziantou `MA####`, Roslynator `RCS####`, Sonar `S####`, `CRR0029`) is also pinned to `suggestion`; the only diagnostic pinned above suggestion is `MSTEST0032` at `warning` (line 29). No `.globalconfig` file exists anywhere in the repository (glob returned no matches). `QuickFiler.csproj`'s `<Analyzer Include>` items (lines 594-604) are Meziantou, Roslynator, AsyncFixer, BannedApiAnalyzers, and SonarAnalyzer — none of these ship CA1307/CA1310 (those are built-in Roslyn/.NET-analyzer IDs, gated by the project's `EnableNETAnalyzers=true` MSBuild property used in the approved analyzer-rebuild command, not by a NuGet `<Analyzer>` reference).

**Conclusion.** Because `dotnet_analyzer_diagnostic.severity = suggestion` is a blanket, source-agnostic default with no CA1307/CA1310-specific override anywhere in the repository, even if CA1310 fires on `Key.IndexOf(other)` (no `StringComparison`), it would report at `suggestion` severity. Suggestion-severity diagnostics are not warnings and are not promoted to errors by either the analyzer rebuild (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, no `TreatWarningsAsErrors`) or the nullable rebuild (`/p:TreatWarningsAsErrors=true`, which only escalates *warnings*). An explicit `StringComparison.Ordinal` argument on `IndexOf` is therefore **not required** for either rebuild to pass. It is **advisable**: it makes the offset derivation's comparison semantics textually consistent with the ordinal `Contains` guard it depends on, and it preempts the (currently dormant) CA1307/CA1310 diagnostics if their severities are ever tightened. Adding `StringComparison.Ordinal` would **not itself introduce a diagnostic** — it is precisely the fix pattern CA1307/CA1310 recommend.

## R3 — Callers of KeyEquals and producers of a non-null Update

**`KeyEquals` overloads/implementers** (`IKbdAction<T, VDelegate>.KeyEquals`, `IKbdAction.cs:14`): four sibling implementations exist — `KaChar.cs:42,77`, `KaKey.cs:43,78`, and the one under study, `KaStringAsync.cs:106`. Only the `string`-keyed `KaStringAsync` implementation contains the Contains/Substring branch-1 logic in question.

**Callers, production (`QuickFiler/Controllers/KbdActions.cs`):** every call to `x.KeyEquals(key)` is inside the generic `KbdActions<TKey, UClass, VDelegate>` class: `ContainsKey` (85), `FilterKeys` (87), `Find` (91), `FindIndex` (109, 116). `KeyboardHandler.cs:181,188` calls `StringActionsAsync.ContainsKey(...)` and `StringActionsAsync.FilterKeys(...)` on the live keyboard-dispatch path (`StringActionsAsync` typed as `KbdActions<string, KaStringAsync, Func<string, Task>>`, declared at `IQfcKeyboardHandler.cs:26` and `KeyboardHandler.cs:84`). This is the only production call chain that reaches `KaStringAsync.KeyEquals`.

**Production construction sites for `KaStringAsync`:** exactly one, `QfcCollectionController.GenerateStringKbdAction` (`QfcCollectionController.cs:1114-1121`):

```csharp
1114  var stringAsyncAction = new KaStringAsync(
1115      "Collection",
1116      key,
1117      (s) => ChangeByIndexAsync(int.Parse(s) - 1),
1118      //(s) => grp.ItemViewer.LblItemNumber.Text = s,
1119      null,
1120      null
1121  );
```

Both `update` (position 4) and `toggleControl` (position 5) are the literal `null` at 1119-1120; the intended `Update` callback is present only as a comment (line 1118), never wired. The only other production path that could construct a `KaStringAsync` is `KbdActions<...>.Add(string, TKey, VDelegate)` (`KbdActions.cs:126-140`), which builds its element with `new()` — the parameterless constructor (`KaStringAsync.cs:12`) — and assigns only `SourceId`, `Key`, `Delegate` (`KbdActions.cs:135-138`); it never touches `Update`/`ToggleControl`, so both remain their default `null`. No other production call site constructs a `KaStringAsync`.

**Conclusion:** the issue's claim is confirmed — `Update` is `null` on every `KaStringAsync` instance production code creates, via both the direct constructor call and the generic `Add(string, TKey, VDelegate)` path. Test code is the only place a non-null `Update` is ever supplied (`KaStringAsyncTests.cs`'s `NewKa` helper, line 26, and inline test-local lambdas).

## R4 — Existing tests constraining branch-1 behaviour

File: `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`.

| Test method | Inputs | Asserted value | Stays TRUE under Option 2? |
|---|---|---|---|
| `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` (78-97) | `Key="abc"`, `other="ab"`, `Activated=true` | `updateArg == "b"`, with because-string `"Update receives Key.Substring(other.Length - 1, 1) => index 1 => \"b\""` (line 90-92) | **Value stays TRUE** (see arithmetic below). **Because-string becomes stale** — it quotes the exact old expression, which is no longer what the code executes. |
| `KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate` (100-115) | `Key="abc"`, `other="ab"`, `Activated=false` | `updateCalled == false` | Unaffected — gated on `Activated`, never reaches the offset expression. |
| `KeyEquals_SingleCharNonMatchWhileActivated_...` (118-132) | `Key="abc"`, `other="z"` | branch 2 (`ToggleControl`) | Unaffected — branch 2, no offset expression involved. |
| `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse` (135-153) | `Key="abc"`, `other="zz"` | `updateArg == "a"` (branch 3, `Key.Substring(0,1)`) | Unaffected — branch 3's expression is untouched by Option 2. |
| `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` (156-167) | `Key="abc"`, `other="zz"`, null delegates | no throw | Unaffected — branch 3. |
| `KeyEquals_MultiCharNonMatchWhileNotActivated_...` (170-188) | `Key="abc"`, `other="zz"`, `Activated=false` | `updates` empty | Unaffected — branch 3, gated on `Activated`. |
| `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar` (191-214) — **the matching-then-non-matching probe test** | `Key="abc"`; `KeyEquals("ab")` then `KeyEquals("zz")` | `updates.Should().Equal(new[] { "b", "a" })` | **Stays TRUE.** First call is branch 1 with `other="ab"` (prefix match, arithmetic unchanged — see below); second call is branch 3, untouched. No stale literal in this test's prose. |
| `KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther` (217-256) | `""`, both an inactive and an active+non-null-Update instance | `ArgumentException` named `"other"` | Unaffected — the empty-string guard clause (lines 115-123) runs before any `Contains`/offset code in both the old and new implementation. |
| `KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther` (259-277) | `null` | `ArgumentNullException` named `"other"` | Unaffected — null guard runs first, same in both versions. |

**Arithmetic — the only test with a stale literal, `KeyEquals_ContainsMatchWhileActivated_...`:**
`Key="abc"`, `other="ab"`. `Key.Contains("ab")` is `true` under both the current and the corrected code (the guard is unchanged). `other` is a prefix of `Key` here (`IndexOf("ab") == 0`).
- Current: `Key.Substring(other.Length - 1, 1)` = `Substring(1, 1)` = `"b"`.
- Corrected: last character of the matched span = `Key.IndexOf(other) + other.Length - 1` = `0 + 2 - 1 = 1`; `Key.Substring(1, 1)` = `"b"`.
Both formulas agree because this probe happens to be a prefix match — this is exactly why AC4 (below) pins the prefix case as unaffected, and exactly why the because-string is stale prose rather than a wrong assertion: the *value* `"b"` is still correct, but the *reason given* (a literal quote of the soon-to-be-replaced expression) will no longer describe the code. Recommended reword: replace the quoted literal with a description, e.g. `"Update receives the last character of the matched span (IndexOf(\"ab\") + other.Length - 1 = 1) => \"b\""`.

No other test in the file quotes the literal expression `Key.Substring(other.Length - 1, 1)` in an assertion or a because-string (a targeted grep of the file for that exact literal found it only in the production source at line 128, not in any test assertion string; the test's because-string quotes it as descriptive prose, which is the one occurrence requiring rewording).

## R5 — The pinned test (`KbdActionsTests.cs`)

File: `QuickFiler.Test/Controllers/KbdActionsTests.cs` (not to be edited; read-only for this research).

- `EnumerableConstructor_WhenStoredKeysDifferButKeyEqualsOverlaps_DoesNotThrow` (57-84): seeds two `KaStringAsync` instances via object initializers (`Key="10"`, `Key="1"`), setting only `SourceId`, `Key`, `Delegate` — **`Update` and `ToggleControl` are never set, so they are `null`** (default field values, `KaStringAsync.cs:147-159`). It asserts only that construction does not throw; it never calls `KeyEquals` directly and never observes `Update`.
- `FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics` (87-123): builds its `KbdActions` via `actions.Add("Collection", "10", _ => Task.CompletedTask)` and `actions.Add("Collection", "1", _ => Task.CompletedTask)` — the `Add(string, TKey, VDelegate)` overload (`KbdActions.cs:126-140`), which (per R3) never assigns `Update`/`ToggleControl`, leaving them `null`. It asserts only on `KeyEquals`'s **return value**, indirectly through `FilterKeys`/`ContainsKey`/the indexer (`actions["10"].Should().NotBeNull()`), never on any `Update` side effect.

**Conclusion:** Option 2 changes only the *argument* passed to `Update` in branch 1 (and only when `Update` is non-null and `Activated`); it does not change `KeyEquals`'s boolean return value, because the guard (`Key.Contains(other)`) is explicitly unchanged per the maintainer decision. Both tests in this file supply a `null` `Update` and assert only on return-value-derived behavior (membership, filtering, indexing). Neither test can be affected by the Option 2 fix. This file does not need to be, and per the maintainer decision must not be, edited.

## R6 — Stale documentation

**`KaStringAsync.KeyEquals` XML doc comment** (`KaStringAsync.cs:57-105`): searched in full for any literal reproduction of `Key.Substring(other.Length - 1, 1)` or an equivalent offset formula — none exists. The only related sentence is the paraphrase at lines 80-83 (`<b>Argument contract.</b>` paragraph):

> "...The guard clause at the top of this method rejects both fail-fast, so branch 1's substring offset expression is never evaluated with a negative start index."

This sentence remains **true** after the fix (the empty-probe guard still runs before any offset arithmetic), but its implicit justification narrows: today "non-negative" follows solely from `other.Length >= 1` (guard) feeding `other.Length - 1 >= 0`. Under the corrected expression (`Key.IndexOf(other) + other.Length - 1`), non-negativity additionally depends on `Key.IndexOf(other) >= 0`, which holds only because branch 1 is entered exclusively when `Key.Contains(other)` is already `true`. The sentence is not factually wrong, but it should be reworded to name both preconditions so a future reader does not assume the old single-guard justification still fully explains it. Suggested replacement: "...so branch 1's derived offset (`Key.IndexOf(other)` plus the matched length) is never evaluated with a negative start index: `IndexOf` is non-negative because branch 1 only runs when `Contains` already matched, and `other.Length` is at least 1 because of the guard above." No other sentence in the doc comment (the `<remarks>` latch-contract paragraphs, the `<returns>`, or the `<exception>` blocks) references the offset arithmetic; all remain accurate because the `Contains` guard and the `KeyEquals` return contract are explicitly unchanged.

**`QfcCollectionController.cs`:** searched around `GenerateStringKbdAction` (1101-1123) and its caller `RegisterNavigationAsyncAction` (1091-1099) for any comment stating a prefix assumption for the two-digit keys — none exists. The commented-out line 1118 (`//(s) => grp.ItemViewer.LblItemNumber.Text = s,`) documents the intended (never-wired) `Update` callback, not a prefix assumption, and needs no change.

## R7 — Retention gates from Issue #445

Searched the full repository (via `git grep`-equivalent full-text search, not limited to a subdirectory) for the literals `Key.Substring(other.Length - 1, 1)` and `Key.Contains(other)`. Matches:

- `QuickFiler/Controllers/KaStringAsync.cs` — production source (the code under change).
- `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` — the because-string identified in R4.
- `docs/features/active/kastringasync-keyequals-contains-offset-583/issue.md` — this issue's own record.
- `docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/{spec.md, issue.md, plan.2026-08-21T18-09.md, code-review.2026-08-22T11-30.md, feature-audit.2026-08-22T11-30.md, research/keyboard-action-contract-defects.2026-08-21T18-20.md, evidence/qa-gates/*.md, evidence/baseline/structural-counts.2026-08-22T09-36.md, evidence/issue-updates/followup-substring-defect.2026-08-22T10-46.md, evidence/other/ac-status-summary.2026-08-22T10-50.md}` — all under `docs/features/archive/...-445/`, i.e., #445's now-closed, archived feature folder.
- `docs/features/potential/promoted/2026-08-07-quickfiler-keyboard-action-contract-defects.md` — the promoted-potential source document for #445.
- `docs/features/active/2026-08-27-.../spec.md` and two other unrelated active-feature research documents that merely cite the expression in prose while discussing #445's history.
- `.claude/agent-memory/atomic-executor/project_concurrent_dotnet_coverage_deadlock_and_doccomment_retention_gate.md` — an agent-memory note *describing* the #445 gate methodology (a lesson learned), not an executable gate.

`docs/features/archive/2026-08-07-.../evidence/baseline/structural-counts.2026-08-22T09-36.md` shows the gate's actual mechanism: a one-time PowerShell command (`git grep -n -F 'TOKEN' -- 'PATHSPEC' | Measure-Object -Line`) run manually during #445's Phase-0 baselining and recorded as a static table in that dated evidence file. It was never wired into a script, hook, or CI workflow.

Searched explicitly for a **live** enforcement point and found none:
- `.github/` workflows: no match for either literal (grep against `.github` returned no files).
- `.claude/hooks/**` (full listing enumerated): none of the 37 hook files reference either literal or a generic "literal register" / "structural count" gate mechanism by content (file listing inspected; none named or scoped to this feature).
- No `scripts/` directory content matches either literal (the repo-wide search's only non-doc, non-source hits were the two `.cs` files already listed).

**Conclusion (explicit negative result):** the #445 "structural-count gate" was a one-time, manually-run Phase-0 baseline captured as a dated Markdown evidence artifact inside #445's now-archived feature folder. It is **inert** — not a live script, hook, CI workflow, or test — and it exercised no ongoing enforcement mechanism that would break when `KaStringAsync.cs`'s branch-1 expression changes under #583. No live gate of any kind counts occurrences of `Key.Substring(other.Length - 1, 1)` or `Key.Contains(other)` in this repository today.

## R8 — Test project conventions

`QuickFiler.Test/Controllers/KaStringAsyncTests.cs` is the location AC3 targets; it lives at repo-relative path `QuickFiler.Test/Controllers/`, mirroring `QuickFiler/Controllers/`. Framework: MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`, `[TestClass]`/`[TestMethod]`). Assertion library: FluentAssertions (`.Should().Be(...)`, `.Should().ThrowExactly<T>().WithParameterName(...)`), consistently supplying a `because:`-style trailing string literal as the human-readable rationale for each assertion. Conventions observed in the existing file to match for the new AC3 test:
- A class-level `///` summary (lines 10-17) precedes `[TestClass]`.
- Each `[TestMethod]` uses `// Arrange`, `// Act`, `// Assert` section comments (e.g., lines 79-96).
- Regression-specific tests carry an `// Intent: ...` comment block above the AAA sections explaining the defect being pinned and the pre-fix behavior it replaces (e.g., lines 172-178, 191-197, 217-227, 258-266) — the AC3 test should follow this pattern, naming the issue/defect and stating both the pre-fix (`"0"`) and post-fix (`"1"`) result.
- The shared `NewKa(...)` factory helper (lines 21-26) is the established construction seam and should be reused for the new AC3 test rather than calling the constructor directly.

## R9 — Edge cases for the registered two-digit key width

Registered keys (per `QfcCollectionController.GenerateStringKbdAction`, `key = (i + 1).ToString("00")`): `"01"` through `"12"`. For a single-character probe, a key of this format matches (`Contains`) if and only if the probe equals one of its two digits. Enumerating the distinct **match-position** shapes reachable, with the corrected expression `Key.IndexOf(other) + other.Length - 1` (here `other.Length == 1`, so this reduces to `Key.IndexOf(other)`, i.e., the position of the single matched digit) against the current (wrong) `Key.Substring(other.Length - 1, 1)` = `Substring(0, 1)` (always the first character, regardless of match position):

| Key | Probe | Match position (`IndexOf`) | Corrected `Update` arg | Current (buggy) `Update` arg | Differ? |
|---|---|---|---|---|---|
| `"01"` | `"1"` | 1 (second char) | `"1"` | `"0"` | **Yes** — this is the issue's regression case. |
| `"10"` | `"1"` | 0 (first char) | `"1"` | `"1"` | No — prefix match, both formulas agree. |
| `"11"` | `"1"` | 0 (first occurrence; digit repeats) | `"1"` | `"1"` | No — `IndexOf` returns the first of the two occurrences (index 0), and the first-character formula happens to return the same character, so this key is not a distinguishing case even though the probe occurs twice. |
| `"12"` | `"1"` | 0 (first char) | `"1"` | `"1"` | No — prefix match. |

For `Key="11"`, `other="1"` specifically: `"1"` occurs at both index 0 and index 1. `string.IndexOf` returns the **first** match, index 0, so `Update` receives `Key[0] = "1"` — the same character branch 3's `Substring(0,1)` would also yield, and the same result the current (unfixed) code already produces for this key. This key does not exercise a behavior change; `"01"` is the only one of the four reachable two-digit keys where the corrected and current expressions diverge, which matches the issue's chosen regression case exactly.

## Recommendation

Implement Option 2 exactly as the maintainer recorded it:
1. Leave the branch-1 guard (`Key.Contains(other)`) and the empty/null argument guards unchanged.
2. Replace `Key.Substring(other.Length - 1, 1)` (line 128) with an offset derived from `Key.IndexOf(other)` — recommended form: `Key.Substring(Key.IndexOf(other) + other.Length - 1, 1)`, optionally with an explicit `StringComparison.Ordinal` on the `IndexOf` call for semantic consistency with `Contains` (advisable, not required by any active analyzer gate; see R2).
3. Add the AC3 regression test to `KaStringAsyncTests.cs` following the file's existing `NewKa`/AAA/`because:` conventions (R8), covering `Key="01"`, `other="1"`, non-null `Update`, asserting `"1"`.
4. Reword the one stale because-string identified in R4 (`KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue`) to stop quoting the replaced literal expression, while keeping its asserted value (`"b"`) unchanged, per AC4.
5. Reword the doc-comment sentence identified in R6 to name both preconditions for non-negativity.
6. Do not touch `KbdActionsTests.cs` (R5 confirms no test in that file is affected).
7. No retention-gate script, hook, or CI check needs updating (R7 — none exists live).

## Rejected alternatives

- **Option 1 (switch the guard to `StartsWith`)**: explicitly rejected by the maintainer decision; not evaluated further here.
- **Leaving `IndexOf` culture-sensitive vs forcing `StringComparison.Ordinal`**: both are behaviorally equivalent for the actual digit-only input domain (R2) and neither is blocked or required by any live analyzer gate; `StringComparison.Ordinal` is recommended for semantic clarity but is not a correctness requirement for this fix.

## Testing implications

- Add exactly one new `[TestMethod]` to `KaStringAsyncTests.cs` for the AC3 regression case (`Key="01"`, `other="1"`, non-null `Update`, asserting `"1"`), following the file's existing MSTest + FluentAssertions + AAA + `because:` conventions (R8) and reusing the `NewKa` helper.
- Reword (do not remove) the AC4 prefix-case assertion's because-string in the existing `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` test so it no longer quotes the replaced literal expression (R4); the asserted value `"b"` itself does not change.
- No other existing `KaStringAsyncTests.cs` test requires a value change (R4); `KbdActionsTests.cs` requires no change and must not be edited (R5, maintainer decision).
- No coverage, retention-gate, or CI artifact needs updating as a prerequisite (R7).
- This is unit-level, deterministic, no-I/O test work consistent with repository MSTest/FluentAssertions/AAA conventions; no new test infrastructure or seam is required.
