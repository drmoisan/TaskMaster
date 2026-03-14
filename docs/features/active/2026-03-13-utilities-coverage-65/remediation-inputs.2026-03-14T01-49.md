# Remediation Inputs — utilities-coverage (2026-03-14T01-49)

These remediation inputs are authoritative for follow-up work required to bring the feature into compliance with its documented acceptance criteria and repo policy.

## Required fixes

1. **Close the documented coverage gap for non-excluded `UtilitiesCS` files**
   - **Primary evidence:** `evidence/qa-gates/final-per-file-coverage.2026-03-14T01-42.md`
   - **Current state:** Only **72 / 256** non-excluded files meet the ≥80% threshold; **184** remain below target.
   - **Expected behavior:** Every non-excluded file counted by the feature scope must either:
     - reach **≥80%** line coverage through added/expanded tests, or
     - be moved to an explicitly justified exclusion category that is consistent with the existing scoping docs.
   - **Acceptance criteria affected:**
     - Per-file line coverage ≥80% for each testable UtilitiesCS source file
     - Directory-wide “every public class/method” criteria for Extensions / HelperClasses / Threading / ReusableTypeClasses / NewtonsoftHelpers
   - **Verification:**
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
     - Recompute and review `evidence/qa-gates/final-per-file-coverage.<timestamp>.md`

2. **Split oversized test files to satisfy the 500-line repo limit**
   - **Files:**
     - `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` (**1529** lines)
     - `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` (**580** lines)
     - `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs` (**559** lines)
     - `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` (**527** lines)
   - **Expected behavior:** Each test file must be ≤500 lines and organized by focused behavior area.
   - **Acceptance criteria affected:** Tests are readable and maintainable; repo file-structure policy compliance.
   - **Verification:** Line-count check over `UtilitiesCS.Test/**/*.cs`; solution build and full MSTest pass.

3. **Remove environment-coupled filesystem and timing patterns from unit tests**
   - **Files / locations:**
     - `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs:90-92`
     - `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs:83`
     - `UtilitiesCS.Test/HelperClasses/MyFileSystemInfoTests.cs:28-29`
     - `UtilitiesCS.Test/ReusableTypeClasses/TimedBatchAction_Tests.cs:84`
     - Also review similar timing patterns such as `SegmentStopWatch_Tests.cs` and any other raw sleep / wall-clock usage.
   - **Expected behavior:** Tests should not depend on the live repository filesystem, wall-clock timestamps, or `Thread.Sleep` for correctness.
   - **Acceptance criteria affected:** Tests are independent, isolated, fast, and deterministic; no external dependencies/file I/O in tests.
   - **Verification:** Static inspection for `Thread.Sleep`, `DateTime.Now`, repo-root discovery, and direct filesystem traversal; full MSTest rerun.

4. **Normalize changed tests to the repo’s preferred assertion style**
   - **Primary file:** `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs`
   - **Locations:** `66,274,297,319,465,486,514,532`
   - **Expected behavior:** New/updated tests should prefer FluentAssertions and explicit throw assertions over MSTest `Assert` / `[ExpectedException]` where practical.
   - **Acceptance criteria affected:** Test readability and policy alignment.
   - **Verification:** Static inspection; build + MSTest rerun.

5. **Reconcile plan/evidence artifacts with the actual final state**
   - **Files:**
     - `docs/features/active/2026-03-13-utilities-coverage-65/plan.2026-03-13T22-21.md`
     - `evidence/qa-gates/final-format.2026-03-14T01-36.md`
     - `evidence/qa-gates/final-per-file-coverage.2026-03-14T01-42.md`
     - `evidence/qa-gates/final-coverage-delta.2026-03-14T01-42.md`
   - **Expected behavior:** The plan’s Phase 8 status must reflect the real outcome: current review confirms toolchain pass, but coverage gate failure must remain explicit until fixed.
   - **Acceptance criteria affected:** Toolchain audit fidelity and feature readiness reporting.
   - **Verification:** Review updated plan/evidence text for consistency with rerun outputs.

## Do not do

- Do **not** weaken repo policy or silently redefine success to package-level coverage only.
- Do **not** mark the feature complete while `final-per-file-coverage` still reports failing thresholds.
- Do **not** keep oversized files and argue that “tests are exempt” — they are not.
- Do **not** replace deterministic synchronization with longer sleeps.
- Do **not** add temp-file-based tests or new external service dependencies.

## Acceptance criteria not yet met

1. **Per-file line coverage ≥80% for each testable UtilitiesCS source file**
   - **Minimum change required:** Raise all non-excluded below-threshold files to ≥80%, or explicitly and credibly reclassify exclusions consistent with the feature’s allowed exclusions.

2. **Every public class/method in the targeted UtilitiesCS directories has corresponding tests**
   - **Minimum change required:** Close the remaining gaps in `Extensions/`, `HelperClasses/`, `Threading/`, `ReusableTypeClasses/`, and `NewtonsoftHelpers/`, as evidenced by the low/zero coverage files already enumerated in `final-per-file-coverage.2026-03-14T01-42.md`.

3. **Tests are independent, isolated, fast, deterministic, and avoid external dependencies**
   - **Minimum change required:** Replace repo-filesystem discovery, wall-clock values, and sleep-based waits with mocks, fixed values, and synchronization primitives.
