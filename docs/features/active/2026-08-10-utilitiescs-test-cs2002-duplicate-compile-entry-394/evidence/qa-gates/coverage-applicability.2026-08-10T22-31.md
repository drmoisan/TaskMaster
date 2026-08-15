Timestamp: 2026-08-10T22-31

Determination: No `artifacts/csharp/coverage.xml` (or any other coverage-report) capture is
performed for this change.

Rationale: `evidence/qa-gates/diff-scope.2026-08-10T22-31.md` (P2-T6) confirms this change's diff
consists of exactly one deleted line in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — a `.csproj`
item-list entry, not `.cs` source — plus this feature folder's own documentation and evidence
files. Zero `.cs` source lines are modified, added, or removed; no module, class, or method is
added; and `PercentageFormatterTests.cs` itself (the file whose duplicate `<Compile>` item is
removed) is not modified in any way. A build-configuration item-list edit of this kind has no
changed-line coverage surface: coverage tooling (Cobertura/dotnet-coverage/vstest
`/EnableCodeCoverage`) instruments and reports on compiled IL mapped to source-line ranges in `.cs`
files, and no `.cs` file's line-range mapping changes as a result of this fix. The repository's
general-unit-test.md coverage-regression rule ("Code changes or refactors must not reduce coverage
for the lines that were changed") has no applicable changed-line set here, because there is no
changed line in any `.cs` file to regress.

A repository-wide `artifacts/csharp/coverage.xml` capture is not performed for this change because:
(a) it is not warranted for a non-code, single-line build-configuration edit with no coverage
surface to measure, and (b) running a full repository coverage instrumentation pass carries a risk
of tripping an unrelated coverage-floor hook against content this feature did not touch and is
explicitly out of scope to fix (see `spec.md` Scope & Non-Goals: no change to `CLAUDE.md`,
`.claude/rules/**`, or `scripts/**`, and no repository-level automated check is introduced by this
feature). P2-T2's before/after `PercentageFormatterTests` test-count parity (7/7, both passing,
`evidence/regression-testing/post-fix-test-count.2026-08-10T22-31.md`) is the applicable regression
evidence for this change: it confirms the fix does not alter which tests are compiled or executed.
