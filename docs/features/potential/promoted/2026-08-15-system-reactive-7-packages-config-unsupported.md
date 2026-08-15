# system-reactive-7-packages-config-unsupported (Issue #570)

- Date captured: 2026-08-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/system-reactive-7-packages-config-unsupported/ (Issue #570)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #570
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/570
- Last Updated: 2026-08-15
## Summary

The NuGet upgrade in PR #568 moved System.Reactive to 7.0.0, which explicitly
does not support `packages.config`. Every build of the solution now emits five
warnings from the package's own guard target stating the configuration is
unsupported. The repository's projects are legacy non-SDK `packages.config`
projects, so the package is being consumed in a scenario its authors disclaim.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8.1)
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- Data source or fixture: `packages/System.Reactive.7.0.0`

## Steps to Reproduce

1. Check out `main` at or after merge commit `97065e55`.
2. Run `nuget restore TaskMaster.sln`.
3. Run the analyzer or nullable toolchain stage, for example
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`.
4. Observe the warning summary at the end of the build.

## Expected Behavior

The solution builds with zero warnings from its dependency set, and the reactive
dependency is consumed in a configuration its maintainers support.

## Actual Behavior

The build succeeds with 5 warnings, all of the following form:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5):
warning : The project contains a packages.config file, which is not supported by
System.Reactive v7.0 or later. Please migrate to PackageReference. (You can
suppress this message by setting the RxUseUnsupportedPackagesConfig property to
true, but be aware this is an unsupported scenario.)
```

Affected projects: `ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test`,
and one further project in the same build.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see the warning text quoted above; reproduced verbatim from the
  local analyzer and nullable toolchain stages on 2026-08-15.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The build is not broken today and both protected gates pass. The severity is
Medium rather than Low because the package guard is not cosmetic: the vendor
states the scenario is unsupported, so assembly-binding or runtime-loading
behavior for Rx may diverge from a supported configuration without further
warning, and the warning count masks any new warning that appears later.

## Suspected Cause / Notes

- The upgrade advanced System.Reactive across a major version boundary (6.x to
  7.0.0) where the package added `System.Reactive.PackagesConfigCheck.targets`
  as a deliberate guard against `packages.config` consumption.
- The repository is intentionally on `packages.config` (see
  `.claude/rules/csharp.md`, "Mechanism"): the projects are legacy non-SDK
  VSTO / .NET Framework projects, and PackageReference / Central Package
  Management were explicitly not introduced.
- Three responses exist and should be weighed rather than assumed:
  1. Pin System.Reactive back to the last 6.x release that supports
     `packages.config`.
  2. Set `RxUseUnsupportedPackagesConfig=true` to silence the guard, accepting
     the vendor-disclaimed configuration.
  3. Migrate the affected projects to PackageReference, which conflicts with
     the documented repository decision and is a much larger change.
- Option 1 is the most likely correct answer given the repository's documented
  stance, but it needs verification that nothing added in Rx 7.0 is required.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: existing reactive-dependent tests in `UtilitiesCS.Test`
      and `QuickFiler.Test` must continue to pass under whichever option is taken.
- [ ] Integration scenario to retest: full `vstest.console.exe` run with
      `/EnableCodeCoverage` across all nine test assemblies.
- [ ] Manual verification notes: confirm the build warning count returns to zero
      and that assembly binding redirects for `System.Reactive` in each
      `app.config` still resolve at runtime.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
