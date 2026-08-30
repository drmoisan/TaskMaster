# Remediation Inputs — Issue #644, Cycle 2

- Timestamp: 2026-08-30T02-08
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head at cycle entry: `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- Cycle-1 exit audits: `code-review.2026-08-30T01-46.md`, `feature-audit.2026-08-30T01-46.md`,
  `policy-audit.2026-08-30T01-46.md`
- Cycle-1 blocking count: **0**

## Why this cycle exists

The cycle-1 exit reaudit returned zero blocking findings, so the exit gate is met and no cycle is
mandated. This cycle, like cycle 1, is opened by orchestrator election.

The reason is consistency and closure, not severity. Cycle 1 was opened to correct a stale comment
describing a mechanism this change deleted. The cycle-1 reaudit then ran the first **class-scoped**
sweep of the repository for that defect class and found two further instances of the identical
defect in the identical file. Shipping them would make cycle 1's premise arbitrary: the same defect
was judged unacceptable in one location and acceptable two lines' walk away.

The decisive difference from an open-ended regress is that the defect set is now **enumerated and
closed**. Every prior pass anchored on specific literal fragments and so could only find what it was
already looking for. The reaudit's sweep asked the class question instead:

```
grep -rn --include=*.cs -iE "recorded (registration )?width|loop bound"
```

Re-run independently by the orchestrator at cycle entry, that sweep returns exactly four hits
repository-wide. Two are correct past-tense descriptions of the superseded behavior and are not
defects:

- `QuickFiler/Controllers/QfcCollectionController.cs` line 2372 — describes the original bug in the
  past tense inside the fix's own explanatory comment.
- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` line 305 — "before
  the ledger the loop bound dereferenced that null field", explicitly past tense.

The remaining two are this cycle's scope. There is no third candidate, so this cycle closes the
class rather than opening the next round of it.

## Item 1 — CR-6: an assertion message asserting a deleted mechanism as the reason

**File:** `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`, line 179.

**Current text:** `"the recorded registration width is replayed, so the '0'-prefixed keys go"`

**Why it is wrong.** "The recorded registration width" is `_registeredDigits`, which this change
deleted; `git grep` confirms zero occurrences of that field repository-wide. The message states, as
the reason the assertion holds, a mechanism that no longer exists. After this change
`UnregisterNavigation` replays a ledger of the recorded key strings verbatim, so every registered
key is removed and no `'0'`-prefixed key survives.

**Why it was missed until now.** It is the first of the two assertion messages in that test; cycle 1
corrected the second one at line 222 and the `<summary>` block above, both located by literal
fragment rather than by class.

## Item 2 — CR-2: an unqualified "After the fix" that now reads as current behavior

**File:** the same file, line 145, inside the `<summary>` block of the two-digit test.

**Current text:** `/// fix it replays the recorded width and removes "01".."09".`

**Why it is wrong.** The sentence is a correct historical statement about the #472 fix, but it says
"After the fix" without naming which fix, inside a documentation block that now also describes #644.
A reader reaches it before the paragraph that explains #644 replaced the bound with a ledger, so the
unqualified phrasing reads as current behavior. This is weaker than item 1 — ambiguous rather than
plainly false — and the correction is correspondingly small: qualify which fix is meant and place
the clause in the past tense so the following paragraph reads as the update it is.

**Do not rewrite the second paragraph of that block.** It already describes #644 correctly and is
out of scope.

## Constraints

- Comment and string-literal text only. Change no assertion, no `.Should()` chain, no test name, no
  attribute, and no executable line. The second assertion message at line 222 and the `<summary>`
  block at lines 189-196 were corrected in cycle 1 and are correct; do not disturb them.
- Keep each replacement close in length to the text it replaces so the formatter's line-breaking
  decision for the enclosing call is unchanged.
- The file is 226 lines with 3 `[TestMethod]` attributes and must remain so.

## Explicitly out of scope

- **AC-16.** Its disposition is final and unchanged: PARTIAL, left unchecked, referred and reported.
  No coverage comparison is run and no acceptance criterion changes state.
- **The analyzer HintPath skew** in `UtilitiesCS.csproj` and `VBFunctions.csproj`. Pre-existing,
  byte-identical at the merge base, touched by neither this branch nor this cycle, and not
  reproduced by CI, which is green on `main` at the merge base and the five runs before it.
  Reported to the parent for promotion.
- **PA-1 through PA-6, CR-3, CR-4, CR-5.** Recorded in the committed audits and carried forward.
- The prior audit artifacts at both earlier timestamps. They are the cycle-entry records and are not
  rewritten.

## Exit condition

A reaudit at a new timestamp with a total blocking count of zero, both items above confirmed
remediated, and the class-scoped sweep returning only the two legitimate past-tense hits.
