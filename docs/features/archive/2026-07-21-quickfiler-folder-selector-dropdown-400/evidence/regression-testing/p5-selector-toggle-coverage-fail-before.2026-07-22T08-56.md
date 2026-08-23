# P5 selector-toggle coverage fail-before reconciliation

Timestamp: `2026-07-22T08-56`

Command: `$evidence='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/popup-ui-boundary-composition-coverage.2026-07-22T08-44.md'; $xml='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-popup-ui-boundary-composition.2026-07-22T08-44.cobertura.xml'; Get-Content -Raw $evidence; Get-FileHash -Algorithm SHA256 $xml; Get-FileHash -Algorithm SHA256 'coverage.config'`

EXIT_CODE: `0`

Output Summary: `PASS for the expect-fail reconciliation. The read-only command confirmed that the underlying 08-44 filtered coverage command completed naturally with exit code 1 after discovering exactly 70 cases: 69 passed, one failed, and zero skipped. The generated XML is structurally complete but non-authoritative because the test command failed.`

## Reconciled failure

- Underlying coverage command exit code: `1`.
- Discovered: `70`.
- Passed: `69`.
- Failed: `1`.
- Skipped: `0`.
- Failure: `BreadcrumbSelectorToggleUiBoundaryTests.WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`.
- Observation: `context.PostCount == 1`.
- Retained expectation: `context.PostCount` must be greater than `postsBeforeToggle`, whose observed value was `1`.
- Non-instrumented comparison: P5-T86 passed the same case without coverage instrumentation.

## Artifact classification and integrity

- `coverage-popup-ui-boundary-composition.2026-07-22T08-44.cobertura.xml` is structurally complete but non-authoritative because the underlying command failed.
- Cobertura SHA-256: `7D19A7AFB1BA278EA1BD8A80AE20BABB603220BD8FEB8CB7548EF11DA0495AAB`.
- Pre-command `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- Post-command `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.

P5-T87 through P5-T89, P5-T67, P5-T68, and P5-T73 through P5-T78 remain unchecked. This task made no production or test correction.
