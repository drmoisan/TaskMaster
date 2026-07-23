# Phase 9 Formatter-Stabilization Scope Review

- Timestamp: `2026-07-23T12:18:57Z`
- Command: `compare HEAD and worktree for the five CSharpier-processed files by whitespace-normalized source; normalize only the authorized ExceptionRecorder-to-ConcurrentQueue and OperationEntry alias transformation for BreadcrumbPopupControlDispatchTests.cs; recompute test-name, assertion-line, and Breadcrumb seam-token hashes; inspect C# and project worktree paths; git diff --check`
- EXIT_CODE: `0`
- Output Summary: `P8_T26_SCOPE_AUDIT_OK formatter_processed=62 formatter_byte_changes=5 tracked_csharp_deltas=4 formatter_only_semantic_matches=4 target_semantic_match=true tests=11 cases=13 assertion_lines=44 assertion_hash=0FA3A31B15FE6825B716DEB28E0CFAE58CE8014891AA6BA901FDD0ABD2034BEC seam_hash=48DA4538877099D3B0D59D7CD26BE2E9CAC24F905D3D6E34E4F06BD79DA34D82 project_changes=0 diff_check=0`

## Scope Result

The stabilization remains within its authorized boundary.

| Category | Result |
|---|---|
| Authorized issue path set | 62 paths; SHA-256 `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` |
| CSharpier processed | 62 paths |
| CSharpier byte changes | Five authorized files |
| Tracked C# deltas after Git normalization | Four test files |
| Production semantic delta from stabilization | None |
| Test semantic delta | Only the authorized empty queue-wrapper and tuple-alias representation change |
| Project/config/filter/threshold/exclusion delta | None |
| Protected hash changes | None |
| Maximum authorized C# lines | 500, with all retained lower headroom bounds passing |
| `git diff --check` | Exit 0 |

`BreadcrumbMessengerHub.cs` was one of the five byte-level CSharpier writes, but its formatter result normalizes to the committed Git content and therefore does not remain as a tracked worktree delta.

## Formatter-Only Semantic Proof

Removing whitespace from HEAD and worktree source produced exact equality for these four CSharpier-only files:

| Path | Normalized SHA-256 |
|---|---|
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | `1D0DFC30D829163C0503A279A3910710809FD1EBEB428A00A331129FC20AFECC` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | `8AD75C9388D95DEABEF04EA77994279699858956ADA139FA9C4ABB78E2A57599` |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `E1914656F4DDB659C5DD90E0292563D4FEAA2D706A5B5B46A56DCBC46753C5A4` |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | `AB304EE9672949B35C042065C2D69D3BE4C1A9604B5E0E5D6120008AF89123BE` |

For `BreadcrumbPopupControlDispatchTests.cs`, the verifier removed the old empty `ExceptionRecorder` declaration, normalized remaining old wrapper references to `ConcurrentQueue<Exception>`, removed the new compilation-unit alias, expanded `OperationEntry` back to `Tuple<string, SynchronizationContext>`, and removed whitespace. The normalized HEAD and worktree sources were then byte-identical with SHA-256 `0C6FABC624963AECD18BF89533A7B244120BA5FCADE20953A9BDCB05666DDB83`.

## Test and Assertion Preservation

| Inventory | Pre-edit | Post-format | Result |
|---|---:|---:|---|
| `[TestMethod]` methods | 10 | 10 | Match |
| `[DataTestMethod]` methods | 1 | 1 | Match |
| `[DataRow]` rows | 3 | 3 | Match |
| Discovered cases | 13 expected | 13 passed | Match |
| Ordered test-name hash | `DFCD8BB714DB88473F702E9E8122F15BCF4EB8B749F5A0CE9F36321DD2266981` | Same | Match |
| `.Should()` occurrences | 52 | 52 | Match |
| Assertion-bearing lines | 44 | 44 | Match |
| Assertion-line hash | `0FA3A31B15FE6825B716DEB28E0CFAE58CE8014891AA6BA901FDD0ABD2034BEC` | Same | Match |
| `Breadcrumb*` seam-token hash | `48DA4538877099D3B0D59D7CD26BE2E9CAC24F905D3D6E34E4F06BD79DA34D82` | Same | Match |

No test, assertion, exception expectation, synchronization action, production seam reference, data row, filter, or runsettings changed.

## Ordered Gate Evidence

| Task | Artifact | SHA-256 | Result |
|---|---|---|---|
| P8-T20 | `evidence/regression-testing/p9-formatter-stabilization-ledger.2026-07-23T11-58.md` | `43D624D625B8950A064EE9305E1BEEE10687ABCF3081A663FB37E80912F2FDB2` | Pass |
| P8-T22 | `evidence/qa-gates/p9-formatter-stabilization-csharpier.2026-07-23T12-03.md` | `D97DE0FC93AE7F4510D8B71D45FEFCD47A2E633E98BD251C2AE328CAAAFED79A` | 62-file format/check pass |
| P8-T23 | `evidence/qa-gates/p9-formatter-stabilization-analyzers.2026-07-23T12-05.md` | `D77A0A94738D8B14A3EC62E068FC2F5136ECA68503757F923D7D421E91F215D1` | Analyzer build pass |
| P8-T24 | `evidence/qa-gates/p9-formatter-stabilization-nullable.2026-07-23T12-05.md` | `8623F66A4BF9C2104F74B265FD9DFD246652E574545298FD71F01D9AAC89560E` | Nullable build pass |
| P8-T25 | `evidence/regression-testing/p9-formatter-stabilization-tests.2026-07-23T12-16.md` | `A389F5FAA734D28182CAF8039C100ACD243322994914F6604D75BAC008E30EEA` | 13/13 pass |

The initial VSTest resolution failure and bounded residual-runner stalls are retained as nonpassing diagnostics in the P8-T25 artifact. The final detailed and normal whole-class commands both passed all 13 cases and left no workspace-owned runner process.

## Independent Review

The first fresh delegated read-only review by `/root/p8_t26_independent_review` returned `REMEDIATION_REQUIRED`:

| Severity | Count |
|---|---:|
| Blocker | 0 |
| Major | 0 |
| Medium | 0 |
| Low | 1 |

The sole Low finding was a path-order contract mismatch: the required `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` hash uses `Sort-Object`/`StringComparer.OrdinalIgnoreCase` ordering, while strict `StringComparer.Ordinal` ordering produces `4FC9A8CACF4E93CA4D8D5F7AC90C82922C7FEF09A4B266A5B70486A189D7D618`. Every implementation, semantic-preservation, formatter, test, analyzer, nullable, protected-file, line-limit, process-cleanup, no-masking, and diff-integrity check otherwise passed.

The specification and plan now explicitly require `StringComparer.OrdinalIgnoreCase`, preserving the same exact 62 paths and authorized hash. The correction is recorded in `evidence/regression-testing/p9-formatter-ordering-contract-correction.2026-07-23T12-33.md`.

The corrected plan then returned `PREFLIGHT: ALL CLEAR` and passed canonical plan validation. At `2026-07-23T12:46:41Z`, the fresh independent re-review by `/root/p8_t26_corrected_re_review` returned:

| Severity | Count |
|---|---:|
| Blocker | 0 |
| Major | 0 |
| Medium | 0 |
| Low | 0 |

The reviewer independently reproduced:

- Merge base `df5ad49c909f6b739edef45d0336151f44e827a6`.
- Exactly 62 paths under explicit `StringComparer.OrdinalIgnoreCase` ordering.
- Authorized hash `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD`.
- Strict-ordinal control hash `4FC9A8CACF4E93CA4D8D5F7AC90C82922C7FEF09A4B266A5B70486A189D7D618`.
- All protected hashes, semantic-normalization hashes, test names, attributes, three data rows, 13 cases, 52 assertions across 44 lines, and retained headroom limits.
- A clean 62-file CSharpier check, 13/13 independent VSTest rerun, zero workspace-owned VSTest/testhost processes, no masking or project/configuration/filter/threshold/exclusion change, and `git diff --check` exit 0.

P8-T26 passes. Phase 9 may begin from a new final-pass run identity.
