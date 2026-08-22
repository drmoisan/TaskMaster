# Phase 2 — Latch-Contract XML Doc Comment Verification (Issue #445, AC2)

Timestamp: 2026-08-22T09-42

Command:
```powershell
(git grep -n -F 'latch' -- 'QuickFiler/Controllers/KaStringAsync.cs' | Measure-Object -Line).Lines
(git grep -n -F '///' -- 'QuickFiler/Controllers/KaStringAsync.cs' | Measure-Object -Line).Lines
sed -n '57,105p' QuickFiler/Controllers/KaStringAsync.cs   # read the comment back
```
Run from `WS`.

EXIT_CODE: 0

## Numeric gates

| Token | Baseline (P0-T19) | Now | Required | Pass |
|---|---|---|---|---|
| `latch` | 0 | **6** | at least 1 | yes |
| `///` | 0 | **49** | at least 15 | yes |

## Mandated wording — read back and confirmed present

The plan's "Latch Contract to Record Verbatim in the XML Doc Comment" section fixes the wording. The comment records it with code identifiers wrapped in `<c>` and emphasis in `<b>`, which is the C# XML-doc convention and changes no word of the sentence:

```
/// <b>Latch contract.</b> <c>Activated</c> is a per-keystroke latch that gates every
/// observable side effect of <c>KeyEquals</c> — both <c>Update</c> and <c>ToggleControl</c>.
/// A <b>matching</b> probe (branch 1) deliberately does not clear the latch and returns
/// early, so a matching element's <c>Update</c> continues to fire on each pass
/// <c>KeyboardHandler</c> makes within one keystroke; that repetition is intentional and is
/// what advances the item-number label. A <b>non-matching</b> probe (branches 2 and 3)
/// clears the latch, so a non-matching element's side effects fire at most once per
/// keystroke regardless of how many times a LINQ predicate is re-enumerated.
```

Clause-by-clause confirmation against the mandated text:

1. "`Activated` is a per-keystroke latch that gates every observable side effect of `KeyEquals` — both `Update` and `ToggleControl`." — present.
2. "A **matching** probe (branch 1) deliberately does not clear the latch and returns early, so a matching element's `Update` continues to fire on each pass `KeyboardHandler` makes within one keystroke; that repetition is intentional and is what advances the item-number label." — present.
3. "A **non-matching** probe (branches 2 and 3) clears the latch, so a non-matching element's side effects fire at most once per keystroke regardless of how many times a LINQ predicate is re-enumerated." — present.

## Prohibited claim is absent

The comment does **not** contain the unqualified claim that each element's side effects fire at most once per keystroke. The "at most once per keystroke" limit appears exactly once, and it is scoped by the qualifier "a non-matching element's". That qualification is required because the claim is false for branch 1, whose matching element's idempotent `Update` may legitimately re-fire across the three passes of one keystroke.

## Other required elements, all present

- `<param name="other">` — one element, documenting the non-null and non-empty precondition.
- `<returns>` — one element, describing the substring-match semantics.
- `<exception cref="ArgumentNullException">` — documents the P2-T1 null contract.
- `<exception cref="ArgumentException">` — documents the P2-T1 empty contract, including the reason (`string.Contains(string.Empty)` is true for every receiver).
- A `<para>` recording that `KbdActions` methods with a `string` key type (`ContainsKey`, `FilterKeys`, `Find`, `FindIndex`, and the indexer) inherit the new precondition, so an empty key argument now surfaces an `ArgumentException` from the predicate rather than matching every element. This is the sentence the task requires.
- An additional `<para>` restating the anti-regression rationale for branch 1's early return, so a future reader encounters the constraint at the code rather than only in this plan.

## Deliberate wording deviation, and why

The first draft of the argument-contract paragraph quoted the offset expression literally as `<c>Key.Substring(other.Length - 1, 1)</c>`. That raised the repository count of the literal `Key.Substring(other.Length - 1, 1)` in this file from 1 to 2, which would have **failed** the P4-T4 retention gate that pins it at exactly 1. The paragraph was reworded to "branch 1's substring offset expression is never evaluated with a negative start index", which preserves the meaning and restores the count to 1. This is recorded because the retention gate did its job: it caught a documentation change that would otherwise have silently broken an out-of-scope-verification assertion.

Verified after the reword: `Key.Substring(other.Length - 1, 1)` count is **1**, equal to its P0-T19 baseline.

## Documentation-generation safety check

`QuickFiler/QuickFiler.csproj` sets neither `DocumentationFile` nor `GenerateDocumentationFile`, confirmed by search. XML documentation generation is therefore off, so adding the first XML comments to this file cannot trigger a CS1591 ("missing XML comment for publicly visible type or member") cascade across the file's other undocumented public members under `/p:TreatWarningsAsErrors=true`. This was checked before the comment was written rather than discovered by a failing build.

Output Summary: The XML documentation comment was added immediately above `KaStringAsync.KeyEquals` and read back for confirmation. Both numeric gates pass: `latch` occurs 6 times (baseline 0, required at least 1) and `///` occurs 49 times (baseline 0, required at least 15). All three clauses of the plan's mandated verbatim latch-contract wording are present, with code identifiers wrapped in `<c>` and emphasis in `<b>` per XML-doc convention and no word altered. The prohibited unqualified claim is absent: "at most once per keystroke" appears once and is scoped to "a non-matching element's". The comment also carries one `<param>`, one `<returns>`, an `<exception>` element for each of `ArgumentNullException` and `ArgumentException`, the mandated sentence about `KbdActions` string-keyed methods inheriting the precondition, and a paragraph restating the branch-1 anti-regression rationale. One deviation is recorded: the argument-contract paragraph was reworded to avoid literally reproducing `Key.Substring(other.Length - 1, 1)`, which had raised that token's count from 1 to 2 and would have failed the P4-T4 retention gate; after the reword the count is back to its baseline of 1.
