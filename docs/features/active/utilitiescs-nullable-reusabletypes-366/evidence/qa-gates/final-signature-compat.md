# Final QC — AC5 Signature Compatibility (P9-T10)

Timestamp: 2026-07-19T22-03

## Method

Reviewed the git diff of the #366-authored changes to the remediated cluster (ReusableTypeClasses
batch commits 12a709c8, ddbe93b9, 7f605e31, 20f13a75 plus this session's four-file waiver
working-tree edits). Attribution excludes sibling child #367 (NewtonsoftHelpers `#nullable enable`,
commit c9284b30).

## Change decomposition (#366-authored added lines in ReusableTypeClasses)

- `?` nullable annotations on existing declarations/returns: 179
- Justified `!` null-forgiving operators (each with a `// why` rationale where non-obvious): 42
- `= null!` reflection-populated-field initializers (`// set by deserialization`): 14
- `= default!` initializer: 1
- `where TKey : notnull` constraint clause: 2 in the committed batches + 2 more this session
  (`WrapperScDictionary`, `ScDictionaryConverter`) + `ScDictionary` base = the ratified additive
  constraint on the three generic bases and four waiver consumers.
- `#nullable enable` pragmas: one per opted-in file (non-executable directive).

## Control-flow lines flagged in the diff (4) — all annotation-only

The four added lines matching an `if (` pattern are pre-existing conditionals whose entire line was
re-emitted by the diff because an inline `!` annotation was added; no new branch logic was
introduced:
- `if (ms.mid!.next != null)` / `if (ms.mid!.prev != null)` (linked-list node null-terminus)
- `if (isUpdated && !value!.Equals(oldValue))` (x2)

## Conclusion

Every public-signature change is limited to additive nullability annotations, the ratified
additive `where TKey : notnull` constraint, and justified `!`, all reflecting actual null behavior.
No parameter was added/removed/reordered, no return type semantics changed, and no type was
converted to `record`/`record struct`/`init`. The four one-line `where TKey : notnull` additions on
`WrapperScoDictionary`, `ScoDictionaryConverter`, `WrapperScDictionary`, and `ScDictionaryConverter`
are the epic-authorized Option-A'' four-file waiver changes (additive constraint, no behavior
change) and are expected in the diff. AC5 satisfied.
