# Changed-file set relative to `BASE_SHA`

Timestamp: 2026-08-26T11-05
Task: [P6-T1]

`<BASE_SHA>` is the `[P0-T3]` value `61edc19befcf6c4e95b5acd32542f2dcdab41b78`.

## Command 1 — changed-file set

```
git diff --name-only 61edc19befcf6c4e95b5acd32542f2dcdab41b78 -- . ':(exclude).claude/agent-memory'
```

EXIT_CODE: 0

Output (verbatim, 53 paths):

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs
QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs
QuickFiler/Controllers/QfcItemController.EventWiring.cs
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs
QuickFiler/Controllers/QfcItemController.MailActions.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/analyzer-backfill.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/coverage.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/csharpier-check.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/dotnet-sdk.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/dotnet-tool-restore.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/exemption-and-viewer-counts.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/file-sizes.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/msbuild-analyzers.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/msbuild-nullable.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/named-tests.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/nuget-restore.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/phase0-feature-documents-read.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/phase0-instructions-read.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/quickfiler-test.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/repo-state.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/test-capacity-budget.md
docs/features/active/qfc-item-controller-defects-484/evidence/baseline/toolchain-paths.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/480-overload-retained.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/481-unwire-call-order.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/483-seam-build.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/483-signature-and-caller.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/484-navigation-untouched.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/485-extraction-build.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/485-test-isolation.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/file-sizes-after-480.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/file-sizes-after-481.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/file-sizes-after-483.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/file-sizes-after-484.md
docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/file-sizes-after-485.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/480-async-fail.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/480-pass.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/480-sync-tightened-fail.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/481-empty-bodies-fail.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/481-pass.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/481-unguarded-fail.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/483-fail.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/483-pass.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/484-fail.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/484-pass.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/485-fail.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/485-pass.md
docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/fail-before-exception.webresourcerequested-detach.md
docs/features/active/qfc-item-controller-defects-484/plan.2026-08-24T09-36.md
docs/features/active/qfc-item-controller-defects-484/spec.md
```

### Classification

| Class | Count |
|---|---|
| One of the nine owned files in constraint C1 | 9 |
| Under `docs/features/active/qfc-item-controller-defects-484/` | 44 |
| Any other path | 0 |

The nine `QuickFiler*` paths above are exactly the four owned production partials and the five owned
test files listed in constraint C1. No other path appears.

## Command 2 — working-tree cleanliness

```
git status --porcelain
```

EXIT_CODE: 0

Output (verbatim):

```
```

The command produced zero output lines: the working tree is clean, with no path under
`.claude/agent-memory/` dirty in this worktree either.

Output Summary: 53 changed paths, of which 9 are owned source files and 44 are feature-folder
paths. Zero paths fall outside those two classes. `git status --porcelain` is empty.
