# ci-nuget-cache-fallback-masks-stale-package-refs (Issue #569)

- Date captured: 2026-08-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ci-nuget-cache-fallback-masks-stale-package-refs/ (Issue #569)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #569
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/569
- Last Updated: 2026-08-15
## Summary

The three build and test workflows restore the `packages` directory from an
`actions/cache` entry whose `restore-keys` fallback is a bare
`nuget-${{ runner.os }}-` prefix. On a cache miss the fallback supplies package
folders from an unrelated earlier commit, so a project referencing a package
version that `packages.config` no longer declares still compiles. Two real
defects on PR #568 were invisible to CI for exactly this reason and only
appeared on a local cold build.

## Environment

- OS/version: `windows-latest` GitHub-hosted runner
- Python version: n/a (GitHub Actions YAML)
- Command/flags used: `nuget restore TaskMaster.sln`, then the workflow's msbuild or vstest step
- Data source or fixture: `actions/cache@v4` entry keyed on `**/packages.config`

## Steps to Reproduce

1. On a branch, change a package version in every `packages.config` but leave a
   stale `..\packages\<Id>.<OldVersion>\` path in a `.csproj` or in test code.
2. Push. The cache key `nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}`
   misses because the config files changed.
3. The `restore-keys: nuget-${{ runner.os }}-` fallback restores a prior cache
   that still contains `<Id>.<OldVersion>`.
4. `nuget restore` installs the new versions alongside the stale restored ones.
5. The build and the tests pass, despite referencing a version no longer
   declared anywhere in the repository.

## Expected Behavior

CI reflects what a clean checkout produces. A project or test that references a
package version absent from `packages.config` fails the build, because that is
what any developer with a cold `packages` directory experiences.

## Actual Behavior

CI run 31890892701 on `chore/update-nuget` head `8f30fd53` reported
`build-analyzers`, `build-nullable`, and `mstest-coverage` as **success**. The
same commit built locally against a correctly-restored `packages` directory
produced:

- 10 `CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.138\...'
  could not be found` errors (and the matching `Roslynator.Analyzers.4.15.0`
  errors) across 16 projects, because the upgrade advanced `packages.config` and
  the analyzer `<Import>`/`<Error>` lines to 3.0.156 / 4.16.0 but left the
  `<Analyzer Include>` item paths behind.
- 3 test failures in `UtilitiesCS.Test.Extensions.AsyncSerialization_Tests`
  (`InvalidOperationException: The Microsoft.Graph.xml fixture could not be
  located from the test assembly path`), because the fixture hard-coded
  `packages\Microsoft.Graph.6.2.0` while the upgrade moved to 6.5.0.

Only the `format-check` job failed, and it is the one job with no `packages`
cache dependency.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: CI run 31890892701 job list showing three green build/test jobs on the
  commit that fails a cold local build; local msbuild CS0006 output and the TRX
  failure messages quoted above. Both defects were fixed in commit `46ca9210`.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High because it is a gate-fidelity defect, not an ordinary bug: it caused three
protected checks to report green on a commit that does not build from a clean
checkout. Any future package-path drift is silently admitted to `main` the same
way. It also affects the correctness signal of every prior green run whose cache
was populated from an older commit.

## Suspected Cause / Notes

- Affected workflows, all with the same cache block:
  - `.github/workflows/_build-analyzers.yml` (lines 35-45)
  - `.github/workflows/_build-nullable.yml`
  - `.github/workflows/_mstest-coverage.yml`
- The exact-match key is correct; the `restore-keys` prefix fallback is what
  admits foreign content. For a `packages.config` restore the cache is only
  sound as an exact match, because `nuget restore` adds missing packages but
  never removes packages the current configuration does not declare.
- Two independent remedies exist and are complementary:
  1. Drop the `restore-keys` fallback so a `packages.config` change forces a
     cold restore. Costs restore time on dependency changes only.
  2. Add a validation step asserting that every `..\packages\<Id>.<Version>\`
     path referenced by a `.csproj` resolves to a version declared in that
     project's `packages.config`. This catches drift regardless of cache state
     and is the stronger of the two.
- Note that remedy 2 must tolerate genuinely conditional imports: for example
  `QuickFiler.Test.csproj` imports `..\packages\altcover.8.6.45\...` guarded by
  `Condition="Exists(...)"` and is a pre-existing no-op, not a defect.
- Related repository rule: `.claude/rules/ci-workflows.md` already governs
  workflow-authoring hazards that local toolchain stages cannot see. This finding
  is the same class of problem and likely belongs alongside it once fixed.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: if remedy 2 is implemented as a script, it needs
      Pester coverage per `.claude/rules/general-unit-test.md`, including a
      positive case, a stale-reference case, and a conditional-import case.
- [ ] Integration scenario to retest: reproduce the original failure by pushing
      a branch with a deliberately stale `<Analyzer Include>` path and confirming
      CI now fails.
- [ ] Manual verification notes: confirm a clean-cache run still completes within
      the workflows' 30-minute timeouts.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
