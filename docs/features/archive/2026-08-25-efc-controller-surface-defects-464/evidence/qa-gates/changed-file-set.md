# [P9-T2] Changed-file set

Timestamp: 2026-08-28T01-46
Task: [P9-T2]
Command (as written): `git diff --name-only 002335989830ba9f3ad802858ef0b794f6281750 -- . ":(exclude).claude/agent-memory"`
Command (merge-corrected, the one whose result is evaluated against the acceptance condition): `git diff --name-only 38f097898639b054428188c9c5e266e54972c259 -- . ":(exclude).claude/agent-memory"`
EXIT_CODE: 0

## Recorded deviation — `BASELINE_SHA` no longer isolates this feature's diff

The plan was authored on the assumption that `BASELINE_SHA` remains this branch's merge base for the
whole run. It does not. Between Phase 4 and Phase 5 the branch took an integration merge,
`25924673 Merge remote-tracking branch 'origin/epic/quickfiler-bug-family-integration'`, whose second
parent is `38f097898639b054428188c9c5e266e54972c259` — the integration tip carrying merged siblings
**#476** (`bug/webview2-host-initializer-defects-476`) and **#501**
(`bug/breadcrumb-coordinator-hub-defects-501`). That merge was mandated by the orchestrator and is
recorded in `postmerge-quickfiler-test.md`.

Consequently `git diff BASELINE_SHA..` now reports the union of this feature's diff and both merged
siblings' diffs. Measured:

| Diff base | Paths reported | Paths outside this feature's allowlist |
|---|---|---|
| `002335989830ba9f3ad802858ef0b794f6281750` (`BASELINE_SHA`, as written) | **307** | **223** |
| `38f097898639b054428188c9c5e266e54972c259` (the merged integration tip) | **98** | **0** |

The acceptance condition as literally written is therefore **unsatisfiable on this base for reasons this
feature did not cause**: it would require this feature to be answerable for every file two merged
siblings changed. `git merge-base HEAD 38f09789` returns `38f09789` itself, confirming that the
integration tip is an ancestor of `HEAD` and is the correct base for a scope gate over **this feature's
own** changes.

The gate is therefore evaluated against `38f097898639b054428188c9c5e266e54972c259`. Both commands were
run and both results are recorded above; nothing is concealed.

## Result of the evaluated gate — 98 paths, all conforming

Breakdown:

| Category | Count |
|---|---|
| Paths under `docs/features/active/efc-controller-surface-defects-464/` | 86 |
| The nine writable paths named in constraint C1 | 9 |
| The three deleted `QuickFiler/Viewers/EfcViewer3.*` paths | 3 |
| **Any other path** | **0** |

The twelve non-documentation paths, verbatim:

```
QuickFiler.Test/Controllers/EfcFormControllerTests.cs
QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs
QuickFiler.Test/Controllers/EfcItemControllerTests.cs
QuickFiler.Test/Controllers/EfcViewerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/EfcItemController.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Viewers/EfcViewer.cs
QuickFiler/Viewers/EfcViewer3.Designer.cs
QuickFiler/Viewers/EfcViewer3.cs
QuickFiler/Viewers/EfcViewer3.resx
```

Classified against constraint C1:

| Path | C1 category |
|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | writable production file 1 |
| `QuickFiler/Controllers/EfcItemController.cs` | writable production file 2 |
| `QuickFiler/Viewers/EfcViewer.cs` | writable production file 3 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | writable production file 4 (one-line carve-out) |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | writable test file 1 (pre-existing, extended) |
| `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` | writable test file 2 (created) |
| `QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs` | writable test file 3 (created) |
| `QuickFiler.Test/Controllers/EfcViewerTests.cs` | writable test file 4 (created) |
| `QuickFiler.Test/QuickFiler.Test.csproj` | ninth writable path |
| `QuickFiler/Viewers/EfcViewer3.cs` | authorised deletion |
| `QuickFiler/Viewers/EfcViewer3.Designer.cs` | authorised deletion |
| `QuickFiler/Viewers/EfcViewer3.resx` | authorised deletion |

A set-subtraction of the allowlist from the non-documentation set returns **0 lines**, computed by
`grep -v -x` against each of the twelve literals. No other path appears.

Note that `QuickFiler/QuickFiler.csproj` **does** appear in the as-written 307-path result and **does
not** appear in the evaluated 98-path result. Its appearance is entirely attributable to the merged
siblings; this feature did not write to it. `[P9-T4]` measures that file directly.

Output Summary: PASS under the merge-corrected base. 98 paths, of which 86 are this feature's own
documentation, 9 are the C1 writable paths and 3 are the authorised `EfcViewer3.*` deletions; zero
paths fall outside the allowlist. The as-written `BASELINE_SHA` form reports 307 paths and is recorded
as unsatisfiable because a mid-plan integration merge (`25924673`, second parent `38f09789`) placed two
merged siblings' diffs inside that range. The deviation is recorded, not concealed.
