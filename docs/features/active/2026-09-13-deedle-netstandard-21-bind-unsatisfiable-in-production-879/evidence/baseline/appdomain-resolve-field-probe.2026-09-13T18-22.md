# Phase 0 — Indicative Probe of the Private AppDomain Assembly-Resolution Field

Timestamp: 2026-09-13T23-17

Command:

```
pwsh -NoProfile -Command '
$f = [AppDomain].GetField("_AssemblyResolve", [Reflection.BindingFlags]"Instance,NonPublic")
Write-Output ("PWSH_HOST_RUNTIME=" + [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription)
Write-Output ("PWSH_HOST_FIELD_PRESENT=" + ($null -ne $f))
'
```

EXIT_CODE: 0

Output Summary:

```
PWSH_HOST_RUNTIME=.NET 10.0.11
PWSH_HOST_FIELD_PRESENT=False
```

The runtime observed here is the pwsh host runtime, not net481; this observation is
indicative only and the decisive net481 observation is task [P2-T12].

The negative reading is what the runtime difference predicts. `.NET 10` reimplemented
`AppDomain` as a thin compatibility shim with no private `_AssemblyResolve` instance field,
whereas net481 is a frozen runtime on which the field is expected to be present. Nothing
about the net481 result follows from this reading in either direction, which is precisely why
the plan routes the decisive check through `[P2-T12]`, where the probe runs inside a child
`AppDomain` hosted by a net481 test assembly.

## Fail-Loud Rule

Recorded per `[P0-T13]`.

The child-domain probe resolves the field by the name `_AssemblyResolve` with
`BindingFlags.Instance | BindingFlags.NonPublic`. If the lookup returns `null`, the probe
throws `InvalidOperationException` naming the field. It must never call
`Assert.Inconclusive`, never return a sentinel that the test treats as success, and never
skip. A silently skipped isolation check makes every positive test in the harness vacuous.

The rule covers only the case where the field lookup itself returns `null`. The separate case
in which the lookup succeeds but the field value never becomes non-null is covered by the
second reading in `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall`, which asserts the
handler count is greater than 0 after `InstallProductionFallback()` runs. Without that second
reading a field that is `null` in every state would report an empty invocation list
unconditionally and the first reading would prove nothing.
