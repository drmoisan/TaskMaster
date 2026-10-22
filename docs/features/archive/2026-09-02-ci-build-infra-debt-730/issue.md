# ci-build-infra-debt (Issue #730)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ci-build-infra-debt/ (Issue #730)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #730
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/730
- Last Updated: 2026-09-02
- Work Mode: full-bug

## Summary

Two consolidated CI/build-infrastructure findings: a NuGet cache fallback with no restore-verification step, and an unsuppressed unsupported-package warning. Consolidated into one issue rather than two since both are build-pipeline configuration debt in the same category (silent tolerance of a degraded/unverified state) rather than application code defects.

## Environment

- OS/version: Windows 11 Pro (repo default) / GitHub Actions `windows-latest` runners
- Python version: n/a — GitHub Actions workflow YAML and `packages.config`
- Command/flags used: n/a — findings are from direct workflow/config inspection
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable — both findings are static configuration inspections. See "Actual Behavior."

## Expected Behavior

CI's NuGet restore either hits a valid cache or fails loudly rather than silently resolving stale packages; a deliberately-accepted unsupported-package warning is suppressed with a documented rationale rather than firing unacknowledged on every build.

## Actual Behavior

**1. Three CI workflows carry a bare-prefix NuGet cache `restore-keys` fallback with no restore-verification step.** Confirmed at `.github/workflows/_build-analyzers.yml:40`, `.github/workflows/_build-nullable.yml:40`, and `.github/workflows/_mstest-coverage.yml:40`, each with `restore-keys: nuget-${{ runner.os }}-` (bare prefix, no lock-file hash component). A cache-key miss (guaranteed by any `packages.config`/`.csproj` change) falls back to restoring a stale, pre-change package tree, and nothing in these workflows verifies the restored packages actually match the current lock state before proceeding — risking builds silently running against stale package versions. *(Source: #569.)*

**2. `packages.config` pins `System.Reactive 7.0.0`, which is unsupported for packages.config-style references, and no `RxUseUnsupportedPackagesConfig` suppression exists anywhere in the repo.** Confirmed: the property does not appear in any `.csproj`/`Directory.Build.props`/config file — the only occurrence repo-wide is inside a committed evidence log quoting the warning text itself. The guard-target warning fires on every build referencing this package and has been observed as recently as 2026-08-26 evidence. *(Source: #570.)*

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations above, each confirmed directly against `origin/main` on 2026-09-02.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: neither finding causes a build failure today, but both represent a build pipeline silently tolerating a degraded-confidence state (unverified stale-cache restore; an unacknowledged unsupported-package warning) rather than failing loudly or being deliberately suppressed with a documented rationale.

## Suspected Cause / Notes

Each finding traces to a specific issue, cited inline above. Both are configuration-only fixes with no application-code footprint — a workflow YAML edit and a project-file/MSBuild property addition, respectively.

## Proposed Fix / Validation Ideas

- [ ] Add a restore-verification step (or a lock-file-hash-scoped cache key) to the three named workflows so a cache-key miss can't silently resolve a stale package tree
- [ ] Either add `<RxUseUnsupportedPackagesConfig>true</RxUseUnsupportedPackagesConfig>` with a comment explaining the accepted trade-off, or migrate the `System.Reactive` reference to `PackageReference` as the warning itself recommends — a maintainer decision on which path, since one is a suppression and the other is a real migration

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
