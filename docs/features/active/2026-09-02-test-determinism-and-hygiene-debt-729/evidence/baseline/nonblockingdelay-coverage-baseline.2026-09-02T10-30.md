# NonBlockingDelay.cs baseline line coverage (P0-T12)

Timestamp: 2026-09-03T01-29

Command: aggregate, across every `<class>` element in
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/coverage-baseline.cobertura.xml`
whose `filename` attribute ends with `NonBlockingDelay.cs`, the count of `<line>` children with a
`hits` attribute greater than zero and the total count of `<line>` children.

EXIT_CODE: 0

BaselineCoveredLines: 17
BaselineTotalLines: 17

## Derivation

Aggregation is by `filename` rather than by class name, so any compiler-generated partitioning
would be summed. In this baseline exactly one `<class>` element matched:

```
CLASS: TaskMaster.NonBlockingDelay | filename=TaskMaster\AppGlobals\NonBlockingDelay.cs
  direct class/lines/line count: 17
MATCHED_CLASSES: 1
```

The `<line>` children counted are the direct `class/lines/line` children. The descendant form
`.//line` is deliberately not used, because it would additionally traverse each
`method/lines/line` set and double-count every line that appears in both places.

Output Summary: `TaskMaster/AppGlobals/NonBlockingDelay.cs` has 17 of 17 covered lines at the
merge base, a baseline ratio of 1.0. `BaselineTotalLines` is greater than 0, so the P6-T6 ratio
comparison has a non-zero denominator.
