# [P3-T10] Scope boundary (AC5)

Timestamp: 2026-09-06T15-12

Commands, all in one block with the R10 `$BaseSha` binding, which resolved to
`51b557dfe35702090fec778febfd4049e0e0fed4`:

```
git add --intent-to-add -- '*.cs' '*.csproj'
git diff --name-only $BaseSha -- '*.cs' '*.csproj'
git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'
```

EXIT_CODE: 0

## Why both outputs are listed

Neither output alone is correct in both states. An anchored `git diff --name-only` enumerates
tracked changes only, so a path this plan creates is invisible to it until it is staged — hence the
`git add --intent-to-add` companion. Conversely, `git status --porcelain` goes empty once the change
is committed. The two are therefore recorded side by side, and they agree exactly here: seventeen
paths in each, the same seventeen.

## Anchored diff — `git diff --name-only $BaseSha -- '*.cs' '*.csproj'`

```
QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs
QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
QuickFiler/Controllers/QfcDatamodel.cs
QuickFiler/Controllers/QfcFormController.Deactivate.cs
QuickFiler/Controllers/QfcFormController.EventHandlers.cs
QuickFiler/Controllers/QfcHomeController.cs
QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs
QuickFiler/Interfaces/IQfcDatamodel.cs
```

## Porcelain status — `git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'`

```
 A QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
 A QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs
 A QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
 M QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs
 M QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs
 M QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs
 M QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs
 A QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs
 M QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
 M QuickFiler/Controllers/QfcDatamodel.cs
 M QuickFiler/Controllers/QfcFormController.Deactivate.cs
 M QuickFiler/Controllers/QfcFormController.EventHandlers.cs
 M QuickFiler/Controllers/QfcHomeController.cs
 M QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs
 M QuickFiler/Interfaces/IQfcDatamodel.cs
```

The four `A` entries are the four new test files, visible to the anchored diff only because of the
`git add --intent-to-add` companion.

## The set against the Write Set

CHANGED-SOURCE-PATH-COUNT: 17

**Seven Write Set production paths (all present, none missing, none extra):**

1. `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`
2. `QuickFiler/Interfaces/IQfcDatamodel.cs`
3. `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
4. `QuickFiler/Controllers/QfcDatamodel.cs`
5. `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`
6. `QuickFiler/Controllers/QfcFormController.Deactivate.cs`
7. `QuickFiler/Controllers/QfcHomeController.cs`

**Four new test paths under `QuickFiler.Test/Controllers`:**

8. `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`
9. `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs`
10. `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`
11. `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`

**Five modified test paths under `QuickFiler.Test/Controllers`:**

12. `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`
13. `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs`
14. `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs`
15. `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`
16. `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`

**One project file:**

17. `QuickFiler.Test/QuickFiler.Test.csproj` — four `<Compile Include>` entries only.

`QuickFiler/QuickFiler.csproj` is **not** in the set. The Write Set says an entry there is required
only if implementation introduces a new production file, and it introduces none: every production
change is an edit to an existing file, including the relocated
`TryQueueRemainingMailItemAsync` and the new synchronous
`TryCreateRemainingQueueAdmission`, both of which live in an existing partial.

## The five named exclusions

None of the five files AC5 names appears in either output:

| Path named by AC5 | In anchored diff? | In porcelain? |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | No | No |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | No | No |
| `TaskMaster/Ribbon/RibbonController.cs` | No | No |
| `TaskMaster/Properties/Settings.Designer.cs` | No | No |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | No | No |

`QuickFiler/Controllers/QfcHomeController.Iteration.cs` was additionally verified unmodified by
[P2-T3] with its own path-scoped anchored diff and porcelain pair, both of which returned empty.
That is the #446 AC-6 preservation evidence AC6 cites.

## R7 reading

AC5 says the branch diff "touches no file outside the Write Set". Read literally over the whole tree
that is unsatisfiable, because this plan is required to write evidence artifacts under
`<FEATURE>/evidence/` and to check AC boxes in `spec.md`. R7 therefore evaluates AC5 over the source
pathspec `'*.cs' '*.csproj'` only, which is the footprint the Write Set actually describes. The
narrower evaluation is recorded here and in the AC5 check-off note so a reviewer does not read it as
an unstated relaxation. Outside that pathspec the branch also changes this plan file, this feature
folder's `spec.md` and `issue.md`, and the evidence artifacts under
`<FEATURE>/evidence/`, all of which are the plan's own required outputs.

## Determination

AC5 holds under the R7 pathspec: the enumerated set contains only the seven Write Set production
paths, the four new and five modified test paths under `QuickFiler.Test/Controllers`, and
`QuickFiler.Test/QuickFiler.Test.csproj`, and none of the five named exclusions appears in either
output.
