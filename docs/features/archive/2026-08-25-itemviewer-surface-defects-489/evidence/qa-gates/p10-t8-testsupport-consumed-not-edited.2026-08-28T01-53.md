# P10-T8 — `QfcItemController.TestSupport.cs` is absent from the P10-T2 diff

Timestamp: 2026-08-28T01-53
Command: git diff --name-only cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Result

The command produces **zero output lines**. The path does not appear in the diff, and it is not among
the 25 paths in the P10-T2 scope-lock list recorded in
`FEATURE/evidence/qa-gates/p10-t2-scope-lock-diff.2026-08-28T01-49.md`.

The file is tracked and present on this branch, so this is a genuine no-change observation rather than
a vacuous assertion about a path that does not exist. Acceptance met.

## Consumed, not edited

`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` is **493-owned**. The disposition in
`FEATURE/spec.md` § Sibling-collision resolution is: consume `BuildSyncDispatcher`; do not edit.

That is exactly what happened. `BuildSyncDispatcher` was **consumed** by
`QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs`, the test file this plan
created for the #489 D2 theme-marshalling regression:

```
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs:39:            Mock<IUiDispatcher> dispatcher = QfcItemControllerTestSupport.BuildSyncDispatcher();
```

The helper builds a `Mock<IUiDispatcher>` whose `Invoke`, both `InvokeAsync` overloads and
`BeginInvoke` all run the supplied delegate synchronously, mirroring the ambient
`_itemViewer.Invoke` pattern. Members routed through the Phase 6 dispatch seam therefore become
deterministically unit-testable without a message pump, which is why the new theme-marshalling test
could reuse it instead of introducing a competing dispatcher double.

Consuming a `internal static` helper requires no change to the file that declares it, which is why
the 493-owned file could be reused and still show a zero-line diff.

## A note on the printed line range

The plan cites `BuildSyncDispatcher` at `:102-137`. The member is declared at **`:105`** and its body
ends at **`:139`**, immediately before the next member `InjectThemes` at `:146`; `:102` is inside the
XML documentation block above the declaration. Sibling 493 grew the file after this plan was authored,
so the printed range is a pre-growth number. No acceptance condition in P10-T8 asserts a line number —
the acceptance is the path's absence from the diff — so the difference is recorded here for the audit
trail only, and the helper was located by member name as the plan's anchoring rule requires.

Output Summary: `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` is **absent** from the
P10-T2 diff; `git diff --name-only <BASELINE_SHA>` over that path produces zero output lines with
`EXIT_CODE: 0`. The 493-owned file was consumed and not edited:
`QfcItemControllerTestSupport.BuildSyncDispatcher()` — declared at `:105`, body ending at `:139`; the
plan's printed `:102-137` is a pre-growth number — is called at
`QfcItemController.ThemeMarshallingTests.cs:39` to supply the synchronous `Mock<IUiDispatcher>` the
#489 D2 theme-marshalling test needs.
