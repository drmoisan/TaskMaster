# Phase 2 — Decisive net481 Isolation Check

Timestamp: 2026-09-14T11-27

This check is taken before the fix exists, so it cannot be confounded by it. This artifact was
overwritten by the Revision R5 re-run of `[P2-T12]`, as that task directs.

Source read: the REGENERATED `[P2-T11]` artifact at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md`,
timestamped 2026-09-14T11-26. It is neither the superseded copy `[P1-T5]` preserved at
`expect-fail-run-superseded-probe-surface.2026-09-13T18-22.md` nor the Revision R2 content that
previously occupied the plan-named path. The distinction is load-bearing: the isolation outcomes are
properties of the run, the run changed under Revision R5, and the Revision R2 run did not contain the
fourth outcome below at all.

Command: (read of the [P2-T11] artifact)

EXIT_CODE: 0

Output Summary:

```
ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed
ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed
ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed
ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed
NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed
NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed
The private AppDomain assembly-resolution field is present and readable on net481; the isolation assertion did not skip.
```

Acceptance Condition: MET. All six outcomes read `Passed`.

The fourth is the Revision R5 addition and corresponds to criterion 6 in the plan's `## R1`. It is what
establishes that the domain the other five describe is rooted at the `QuickFiler.Test` build output
directory rather than at the host test assembly's own output directory, which is the difference between
a harness that can observe the bind and one that cannot.

`ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` passing is the decisive net481 reading. It takes
two readings of `CountAssemblyResolveHandlers()`: `0` before `InstallProductionFallback()` and a value
greater than `0` after it. The second reading is what makes the first an observation rather than a
constant, because a reflected field that is null in every state would report an empty invocation list
unconditionally. The method's fail-loud rule, recorded at `[P0-T13]`, covers the case where the field
lookup itself returns null; the second reading covers the case where the lookup succeeds and the value
never becomes non-null. Both readings behaved as specified, so the private `AppDomain` field is present,
readable and live on this runtime.

`NegativeControl_WithoutInstall_Netstandard21Throws` passing is the other half of the argument: the
`2.1.0.0` identity remains unsatisfiable in the installer-free domain, so isolation between the positive
and negative domains holds and no positive result in this harness is inherited from a handler an
unrelated component installed first.
