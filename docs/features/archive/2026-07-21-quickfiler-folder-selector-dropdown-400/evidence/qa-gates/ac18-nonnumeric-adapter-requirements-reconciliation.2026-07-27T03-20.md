# AC-18 Nonnumeric Adapter Requirements Reconciliation

Timestamp: 2026-07-27T03-20

## Scope

P9-T9 changed only the toolchain wording at `spec.md:231` and the AC-18 wording at `spec.md:256`. The AC-18 checkbox remains unchecked. No source, test, project, coverage, filter, threshold, or checkpoint file was changed by this task.

## Changed Lines

### `spec.md:231`

Old:

```markdown
  1. Derive the exact issue-#400 C# path set from the live merge base plus untracked `QuickFiler/**/*.cs`, `QuickFiler.Test/**/*.cs`, `UtilitiesCS/**/*.cs`, and `UtilitiesCS.Test/**/*.cs` files, including `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` and `UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs`; sort the exact set with `StringComparer.OrdinalIgnoreCase`; require exactly 65 paths and LF-joined SHA-256 `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`; then run `csharpier format @authorized` followed by `csharpier check @authorized`.
```

New:

```markdown
  1. Derive the exact issue-#400 C# path set from the live merge base plus untracked `QuickFiler/**/*.cs`, `QuickFiler.Test/**/*.cs`, `UtilitiesCS/**/*.cs`, and `UtilitiesCS.Test/**/*.cs` files, including `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`, `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs`, and `UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs`; sort the exact set with `StringComparer.OrdinalIgnoreCase`; require exactly 68 paths and LF-joined SHA-256 `2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9`; then run `csharpier format @authorized` followed by `csharpier check @authorized`.
```

### `spec.md:256`

Old:

```markdown
- [ ] AC-18: One final uninterrupted C# toolchain pass succeeds in this exact order: derive and verify the exact 65-path issue-#400 C# scope, including `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` and `UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs`, sorted with `StringComparer.OrdinalIgnoreCase` and having LF-joined SHA-256 `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`; run `csharpier format @authorized` and `csharpier check @authorized` while preserving the recorded hashes of `coverage.config` and `.csharpierignore`; run analyzer-enabled `msbuild`; run nullable warnings-as-errors `msbuild`; and run coverage-enabled `vstest.console.exe` for `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`. Repository-wide line coverage is at least 80%, every measurable new or changed selector type and member reaches at least 90%, and changed-line coverage does not regress, with numeric baseline/post-change/delta evidence. Only direct WebView2/WinForms adapter calls and unavoidable navigation-readiness coordination and cleanup may be classified as bounded nonnumeric surfaces, and every such surface must be enumerated and verified through deterministic injected seams; no numeric threshold, filter, or exclusion is waived or widened.
```

New:

```markdown
- [ ] AC-18: One final uninterrupted C# toolchain pass succeeds in this exact order: derive and verify the exact 68-path issue-#400 C# scope, including `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`, `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs`, and `UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs`, sorted with `StringComparer.OrdinalIgnoreCase` and having LF-joined SHA-256 `2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9`; run `csharpier format @authorized` and `csharpier check @authorized` while preserving the recorded hashes of `coverage.config` and `.csharpierignore`; run analyzer-enabled `msbuild`; run nullable warnings-as-errors `msbuild`; and run coverage-enabled `vstest.console.exe` for `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`. Repository-wide line coverage is at least 80%, every measurable new or changed selector type and member reaches at least 90%, and changed-line coverage does not regress, with numeric baseline/post-change/delta evidence. Only direct WebView2/WinForms adapter calls and unavoidable navigation-readiness coordination and cleanup may be classified as bounded nonnumeric surfaces, and every such surface must be enumerated and verified through deterministic injected seams; no numeric threshold, filter, or exclusion is waived or widened.
```

## Ledger Reconciliation

| Field | Previous value | Reconciled value |
| --- | --- | --- |
| Ordered path count | 65 | 68 |
| Sort rule | `StringComparer.OrdinalIgnoreCase` | `StringComparer.OrdinalIgnoreCase` |
| LF-joined SHA-256 | `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7` | `2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9` |

Added planned paths:

- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`
- `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs`

Retained paths:

- `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs`
- `UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs`

Protected-hash references retained: `coverage.config`, `.csharpierignore`.

## Wording Hashes

SHA-256 uses UTF-8 bytes of the complete physical line without its trailing newline.

| Wording | Previous SHA-256 | Reconciled SHA-256 |
| --- | --- | --- |
| `spec.md:231` toolchain wording | `9DA73935F40B5AA722FF93FD4C28F3246DED9CE70A10C1A8C7E8DA51F40A6CA8` | `509E9DCF2CEBF8319474B311F1CDEFD3616A84BD9B527D0BC5BCBCCDD60F491B` |
| `spec.md:256` AC-18 wording | `77BB15326CC5B539556F10FCC003277C89CE7F6BCE0EE2622F0BAF73972014AB` | `48B14FE23D433E3B12BCA84D0BAAD615C751D5F1426C836062CC55CE8C868F43` |

## Validation

Command: `git diff --check`

EXIT_CODE: 0

Output Summary: No whitespace errors were reported. Existing working-tree line-ending conversion warnings were emitted by Git.

Command: `pwsh -NoProfile -Command "& { $s='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md'; $l=Get-Content -LiteralPath $s; $t=$l[230]; $a=$l[255]; $n=@('2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9','StringComparer.OrdinalIgnoreCase','QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs','QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs','UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs','UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs'); $ok=$t.Contains('exactly 68 paths') -and $a.Contains('exact 68-path') -and @($n | Where-Object { -not ($t.Contains($_) -and $a.Contains($_)) }).Count -eq 0 -and -not $t.Contains('ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7') -and -not $a.Contains('ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7') -and $a.StartsWith('- [ ] AC-18:') -and $a.Contains('coverage.config') -and $a.Contains('.csharpierignore'); if(-not $ok){ exit 1 }; 'INVARIANT_RESULT: PASS' }"`

EXIT_CODE: 0

Output Summary: `INVARIANT_RESULT: PASS`; both locations contain the 68-path ledger and reconciled hash; AC-18 remains unchecked; protected-hash references are limited to `coverage.config` and `.csharpierignore`.

Spec SHA-256: `BB416F8729990EEFDC336407EA945762FC79045A80239B78CB395B1DCA74DBBE`

P9-T9 Result: PASS
