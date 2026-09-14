# Issue 879 Update Mirror

Recorded by `[P6-T25]`.

Timestamp: 2026-09-14T13-09

PostedAs: unknown

This update has not been posted. The executor does not post to GitHub in this run, so the text
below is mirrored locally and the field records that truthfully rather than claiming a posting
that did not occur.

## Exact text intended for the issue

### Remedy

`UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` adds a host-neutral assembly-binding fallback
for the add-in process. It installs a handler on the application domain's assembly-resolution
event at a deterministic point in add-in startup, from an explicit static constructor in
`TaskMaster/ThisAddIn.cs`, rather than relying on an unrelated component having installed one
first.

The handler delegates to an ordered four-rung resolution ladder whose every external source is an
injected delegate:

1. an already-loaded assembly matching on simple name and public key token, deliberately not
   comparing version, because supplying a same-token assembly of a different version is the whole
   purpose of the fallback;
2. a load by full display name, with the `netstandard` identity pinned to `Version=2.0.0.0`, the
   only `netstandard` version that exists for .NET Framework;
3. the runtime-directory facade loaded from an absolute path, which bypasses assembly-cache
   lookup entirely;
4. a directory probe for the simple name beside the executing assembly.

Installation is idempotent: the installed flag is exchanged atomically, so concurrent callers
attach exactly one handler. The installer never throws, because it is reachable from a type
initializer where an escaping exception would disable the whole add-in.

A `netstandard` `dependentAssembly` block was also added to `TaskMaster/app.config` covering
`0.0.0.0-2.1.0.0` to `2.0.0.0`. This is declarative hardening and NOT the fix: a binding redirect
rewrites an identity and cannot manufacture an assembly. That distinction is recorded at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/hardening-not-the-fix.2026-09-13T18-22.md`.

### Negative control

The fail-before measurement is at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md`.
Against a build carrying no fix it records
`DEEDLE_RECORD_CONVERSION_OUTCOME=NETSTANDARD-BIND-FAILURE:TypeInitializationException`. The
pass-after pair is at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-harness.2026-09-13T18-22.md`
and records `INVOKED-NO-EXCEPTION` for the same line.

The installer-free negative controls run in a clean child application domain with no resolve
handler subscribed: `NegativeControl_WithoutInstall_Netstandard21Throws` records a
`FileNotFoundException` for the `2.1.0.0` identity, which is the defect reproduced in isolation.

### Both-versions criterion

`AfterInstall_BothNetstandardVersionsBind` establishes that after the installer runs, both the
`2.1.0.0` and the `2.0.0.0` full display names bind in a clean child application domain. Its
evidence is at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-harness.2026-09-13T18-22.md`.

### Manual gate status

The manual live-Outlook gate is NOT discharged. Its recorded result is `PENDING-MAINTAINER` at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/manual-live-outlook-gate.2026-09-13T18-22.md`.
The procedure is: start a fresh Outlook session with the rebuilt add-in registered, open no
SVG-bearing surface first, click the QuickFiler ribbon button, and observe whether Deedle loads
and the data model is populated. The corresponding acceptance criterion is left unchecked and the
artifact carries a `Check-Off Withheld:` field.

The Fusion binding-log measurement is likewise `PENDING-MAINTAINER`. It is explicitly
non-blocking, because ladder rung 3 loads the facade from the runtime directory by absolute path
and bypasses cache lookup entirely.

### The 2.0.0.0 limit statement

Reproduced verbatim from
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/netstandard-2-0-0-0-open-risk.2026-09-13T18-22.md`:

> The maintainer's reproduced production trace shows the resolution chain falling back to
> `netstandard, Version=2.0.0.0` and failing there as well.
>
> **This plan cannot explain the `2.0.0.0` frame.** Nothing in this repository accounts for it.
>
> What this work does establish: after the installer runs, both the `2.1.0.0` and the `2.0.0.0`
> full display names bind in a clean child `AppDomain`, evidenced by
> `AfterInstall_BothNetstandardVersionsBind`.
>
> What this work does not establish: why the default binder failed to locate the GAC copy of
> `netstandard 2.0.0.0` inside the Outlook add-in `AppDomain`.
>
> Issue 879 must not be reported as closed on the strength of a 2.1.0.0 result alone.

### Out of scope

The `FSharp.Core` HintPath split between `lib/netstandard2.0` and `lib/netstandard2.1` is the
root cause of the unsatisfiable reference but is out of scope for this work and is tracked
separately as issue #895. No `FSharp.Core` reference, version or binding-redirect value was
changed by this work.
