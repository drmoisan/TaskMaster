---
name: timeprovider-seam-gotchas
description: TimeProvider seam pitfalls when removing banned DateTime.Now/Task.Delay in this net48 repo — Moq cannot mock GetLocalNow, optional TimeProvider params force consumer references
metadata:
  type: project
---

Removing banned `DateTime.Now`/`Task.Delay` here means injecting `System.TimeProvider` (Bcl backport). Two non-obvious traps hit during issue #222 (QFC seams):

1. `Mock<TimeProvider>.Setup(x => x.GetLocalNow())` throws at runtime: "Non-overridable members (here: TimeProvider.GetLocalNow) may not be used in setup". In `Microsoft.Bcl.TimeProvider` 10.0.7, `GetLocalNow()` is non-virtual (delegates to virtual `GetUtcNow()` + `LocalTimeZone`). For timestamp tests use `FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`, PublicKeyToken `31bf3856ad364e35`, NOT the usual Extensions token `adb9793829ddae60`) and derive expected values from `fake.GetLocalNow().LocalDateTime` so the test mirrors production exactly regardless of host timezone. For delay tests, `FakeTimeProvider` + `Advance(TimeSpan)` gates `TimeProvider.Delay(...)`.

2. Adding an optional `TimeProvider timeProvider = null` parameter to a PUBLIC method (e.g. a static factory) causes **CS0012 in every consuming project** ("type 'TimeProvider' is defined in an assembly that is not referenced"), even callers that omit the arg — the C# compiler needs the param type's assembly to bind the call. Fix: add the `Microsoft.Bcl.TimeProvider` `<package>` + `<Reference>` (HintPath to the already-restored net462 DLL) to each consumer's packages.config + csproj. Plans that list only the seam-owning project's files will miss this.

**Why:** both surfaced only at build/test time, not at plan authoring. **How to apply:** when introducing a TimeProvider seam, budget for consumer-project references and use FakeTimeProvider (not Mock) for clock control. See also [[project-build-test-env]] (vstest `/InIsolation` for Moq assemblies, `MSYS_NO_PATHCONV` for vstest paths) and the COM/VSTO coverage exemption: a static lifecycle factory whose body runs live-Outlook `InitAsync`/`LoadAsync` (e.g. `QfcHomeController.LaunchAsync`) is not deterministically unit-testable, so its seam-assignment + catch lines stay uncovered under the testable-denominator rule.
