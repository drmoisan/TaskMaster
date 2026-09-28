# Open Risk — the netstandard 2.0.0.0 Leg

Recorded by `[P6-T2]`. This is the explicit statement of what this work does and does not
establish about the `netstandard, Version=2.0.0.0` frame in the maintainer's production trace.

Timestamp: 2026-09-14T12-58

## The reported production behaviour

The maintainer's reproduced production trace shows the resolution chain falling back to
`netstandard, Version=2.0.0.0` and failing there as well. The trace does not stop at the
unsatisfiable `2.1.0.0` reference: it records a subsequent attempt at the `2.0.0.0` identity,
and that attempt also fails inside the live Outlook add-in process.

## This plan cannot explain the 2.0.0.0 frame

**This plan cannot explain the `2.0.0.0` frame.** Nothing in this repository accounts for it. A
`netstandard 2.0.0.0` facade exists for .NET Framework and is present on the machine, and the
child-domain measurement below shows it loading cleanly by full display name with no resolve
handler subscribed. No code path, configuration file, binding redirect or project reference in
this repository has been identified that would make that same identity fail to locate inside the
add-in `AppDomain`. The cause is unidentified and remains open after this work.

## What this work does establish

After the installer runs, both the `2.1.0.0` and the `2.0.0.0` full display names bind in a
clean child `AppDomain`. The evidence is the test
`AfterInstall_BothNetstandardVersionsBind` in
`TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`, whose pass-after result is
recorded at `evidence/regression-testing/pass-after-harness.2026-09-13T18-22.md`. The remedy is
therefore sufficient for both legs in the environment the harness can construct, and it does not
depend on which of the two identities the binder asks for.

The remedy is also robust to whatever the unexplained frame turns out to mean. Ladder rung 3
loads the facade from the runtime directory by absolute path, which bypasses assembly-cache
lookup entirely, so a failure localised to cache lookup inside the add-in domain cannot defeat
it.

## What this work does not establish

**This work does not establish** why the default binder failed to locate the GAC copy of
`netstandard 2.0.0.0` inside the Outlook add-in `AppDomain`. That question is untouched by every
measurement in this plan. The measurements were taken in child `AppDomain`s created by a test
host, not in a live Outlook add-in domain, and no measurement in this plan observes the add-in
domain's binder state.

## The child-domain observation, copied verbatim

The following is reproduced verbatim from
`evidence/other/netstandard-2-0-0-0-child-domain-observation.2026-09-13T18-22.md`.

> The observation is taken in the INSTALLER-FREE second child domain, rooted at the `QuickFiler.Test` build
> output directory. In that domain, with no resolve handler subscribed, a full display name at
> `Version=2.0.0.0` loads. The sibling observation in the same domain and the same run,
> `NegativeControl_WithoutInstall_Netstandard21Throws`, records a `FileNotFoundException` for the same
> identity at `Version=2.1.0.0`.
>
> This narrows the open risk that `## R2` records. The maintainer's reproduced production trace shows the
> chain falling back to `netstandard, Version=2.0.0.0` and failing there as well. That did not reproduce
> here: the `2.0.0.0` leg succeeded in a clean child domain on this machine. The difference is therefore
> localised to the add-in `AppDomain` in the live Outlook host rather than to the machine's assembly cache.
>
> This does not settle why the `2.0.0.0` frame appeared in the production trace. Nothing in this repository
> accounts for it and this measurement does not explain it. The remedy does not depend on the answer: ladder
> rung 3 loads the facade from the runtime directory by absolute path and bypasses cache lookup entirely.
> Issue #879 must not be reported as having explained the `2.0.0.0` leg.

The recorded measurement itself is `NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=LOADED`, taken
with `TRX_MATCH_COUNT=1`.

## Reporting limit

Issue 879 must not be reported as closed on the strength of a 2.1.0.0 result alone.
