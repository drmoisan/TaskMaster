# Final Diff Integrity

Timestamp: 2026-07-21T21-21Z
Command: `git diff --check`; scoped `git diff`, `git status --short`, and `git ls-files --others --exclude-standard` review; exact include/exclusion searches; line counts; P4-T5 whole-file SHA-256 comparison; P0-T9 assertion-line SHA-256 comparison; and `git diff --exit-code -- coverage.config`
EXIT_CODE: 0
Output Summary: The issue #400 remediation diff matches the authorized production, test, project, specification, and canonical evidence scope. No whitespace error, unrelated edit, unexplained configuration change, temporary file, public-signature change, protected-test mutation, or file-size violation was found.

## Whitespace and Worktree Scope

- `git diff --check`: exit 0.
- Tracked remediation changes: the two authorized project files, host, two authorized semantic test files, and AC-18/checkbox work in `spec.md`.
- Authorized untracked source: helper, readiness tests, lifecycle-concurrency tests, and coverage-threshold tests.
- All untracked documentation is under `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/`.
- Untracked file outside authorized source or canonical feature evidence folders: None.
- Temporary source, results, log, or scratch file in version-control status: None.
- Package, policy, workflow, build-props/targets, or persisted-setting edit: None.

`coverage.config` was temporarily adjusted only while running coverage and was restored immediately. Its working and `HEAD` Git blobs are both `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`; `git diff --exit-code -- coverage.config` exits 0, and it is absent from final version-control status.

## Phase 2 Production and Project Boundary

The P0-T9 production rebaseline authorized only:

1. Refactoring `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` to the 475-485-line target.
2. Adding internal static `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs`.
3. Adding exactly one immediately adjacent helper Compile include in `QuickFiler/QuickFiler.csproj`.

Final result:

| File/check | Result |
|---|---|
| `BreadcrumbDropDownHost.cs` | 484 lines; SHA-256 `b219d3f2d6dd05805adfda95932d3a7d29f2b59bdcb3b674722d804e46719acc` |
| `BreadcrumbWebViewSurfaceFactory.cs` | 118 lines; SHA-256 `4f840dfb2ea96c462e57c5f93d6d88ec9a156251751ce2459c71696b27f767a3` |
| Host include | Exactly one at `QuickFiler.csproj:394` |
| Helper include | Exactly one immediately adjacent at `QuickFiler.csproj:395` |
| Helper visibility | Internal static type; internal factory; private adapter |
| Public host surface | Unchanged; the refactor adds no public signature |

No other production or production-project edit belongs to the remediation batch.

## Post-Blocker Corrective Boundary

The P4-T5 baseline authorized exactly one new test and one include after the 20:38 coverage blocker.

| File/check | Result |
|---|---|
| `BreadcrumbDropDownCoverageThresholdTests.cs` | 395 lines; 7 tests; SHA-256 `2627b112a53efc3fa358af1cdee0d60dc7cbee350b84e015c5124df4cdcffd91` |
| Existing integration include | `QuickFiler.Test.csproj:70` |
| New threshold-test include | Exactly one adjacent include at `QuickFiler.Test.csproj:71` |
| Next existing include | `QuickFiler.Test.csproj:72` |

No production file, existing test, package, setting, filter, exclusion, or public signature changed in the corrective batch.

## P4-T5 Whole-File Hash Recheck

| File | Lines | Baseline/current SHA-256 | Result |
|---|---:|---|---|
| `BreadcrumbDropDownHostTests.cs` | 499 | `8d02e8b9e8c68c9d197e22787c2f82e724e8fc7b7e07d0ffb354af9dd1928d5c` | PASS |
| `BreadcrumbDropDownIntegrationTests.cs` | 500 | `455a0b76ac2606fda73fb0cf715fc370194cbce5d5760a3da99fb305538affdb` | PASS |
| `BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 379 | `386126ef040d87e72091322d000c5a3e607911d71a53215050bebda14ae0e0ab` | PASS |
| `BreadcrumbDropDownLifecycleTests.cs` | 277 | `d35570def5bb0aec362aff5e8a977414119c9eee490ca812aa76f261d9fffd72` | PASS |
| `BreadcrumbDropDownReadinessTests.cs` | 305 | `69e8b09fc4cd7f656bc39d594b8079e071af3b26cf4c114c014d4b33420b9610` | PASS |
| `BreadcrumbSelectorCoordinatorTests.cs` | 369 | `fd9475a1ca8bfc9c002c9f2882802ee555dfa13f71b3f59e93cf78968a22a2fe` | PASS |
| `BreadcrumbDropDownHost.cs` | 484 | `b219d3f2d6dd05805adfda95932d3a7d29f2b59bdcb3b674722d804e46719acc` | PASS |
| `BreadcrumbWebViewSurfaceFactory.cs` | 118 | `4f840dfb2ea96c462e57c5f93d6d88ec9a156251751ce2459c71696b27f767a3` | PASS |

`BreadcrumbDropDownIntegrationTests.cs` remains exactly 500 lines.

## P0-T9 Protected Assertion Recheck

| Protected file | Assertions | Baseline/current SHA-256 | Result |
|---|---:|---|---|
| `BreadcrumbDropDownReadinessTests.cs` | 51 | `58cff79fb67b5a6d95f60e961adedba7492691fdd9ffe16036ea467417bfda6d` | PASS |
| `BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 81 | `a38135b5a39844c4a4f1a420773d54dac6cff6c87c0dcc979a8edd4ebce3e84a` | PASS |
| `BreadcrumbDropDownHostTests.cs` | 52 | `8d9b16ed5d5e2ca21217e4e4c6653415f7fb7c13c105119f7a2182cac418f3dc` | PASS |
| `BreadcrumbDropDownLifecycleTests.cs` | 34 | `fc9370c70b339dd99251e43385d82e7c04c2ac779a17546c3ae64e0a7c4fd5ce` | PASS |

## Exclusion Integrity

- `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:30`: the former host direct-WebView2 method-level exclusion moved here exactly once.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:477`: `ShowOwnedPopup` retains its original method-level exclusion.
- Total remediation host/helper exclusions: exactly two method-level exclusions.
- Class-level exclusion: None.
- Coverage threshold, filter, or exclusion widening: None.

P5-T8 result: PASS. The scoped implementation, tests, project wiring, specification update, and canonical evidence are ready for acceptance reconciliation.
