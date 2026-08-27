# Plan Design Rulings PD-1 and PD-2 (P0-T18)

Timestamp: 2026-08-27T20-09

Recorded verbatim from the plan of record
`FF/plan.2026-08-24T09-40.md` -> `#### Design rulings made by this plan (recorded, with evidence)`.

---

## PD-1

> **PD-1 — `RunSynchronous` settles the lease on its own skip path.** AC-19 requires the lease-leak
> regression test to live in `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs`,
> to compile against HEAD, to be RED there, and to be green after the fix. That test calls
> `lifetime.RunSynchronous(lease, action)` directly, with no coordinator in the picture, so it can
> only turn green if `RunSynchronous` itself settles the skipped lease. `RunSynchronous` therefore
> calls `Abandon(lease)` when `TryRunCurrent` returns `false`. Both call sites additionally branch on
> `false` exactly as `spec.md` -> `## Proposed Fix` -> `#502` specifies. The duplicate `Abandon` is
> harmless and is verified idempotent by construction:
> `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:266-271` returns early once
> `CancellationStarted` is set, and the disposal predicates at `:246` and `:285` are both guarded by
> `!lease.SourceDisposed`.

### PD-1 file:line evidence, independently re-verified at BASELINE_SHA

All three citations are ACCURATE at `BASELINE_SHA` `4f238289090e4c97ca505511a5a73e8092dce0f9`:

| Citation | Observed content | Verdict |
| --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:266-271` | `if (lease.CancellationStarted)` at `:266`, its `return;` body, then `lease.CancellationStarted = true;` at `:270` | ACCURATE — the method returns early once the flag is set, so a second `Abandon` is a no-op |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:246` | `dispose = lease.Cancelled && !lease.SourceDisposed;` | ACCURATE — guarded by `!lease.SourceDisposed` |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:285` | `dispose = lease.Settled && !lease.SourceDisposed;` | ACCURATE — guarded by `!lease.SourceDisposed` |

PD-1 stands as written and is the route this execution will follow.

---

## PD-2

> **PD-2 — AC-11's logging half is verified by a source assertion, not by a runtime log assertion.**
> `QuickFiler.Test/QuickFiler.Test.csproj` carries no `log4net` reference (verified: zero `log4net`
> matches in that project file, and zero `log4net` matches in any `QuickFiler.Test/**/*.cs`), so a
> `MemoryAppender` test of the kind used in `TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs:228-247`
> would not compile there. Adding a `<Reference>` is outside this feature's authorized project-file
> budget of exactly one `<Compile Include>` line per project file (spec `## Scope & Non-Goals`). The
> reflective alternative (resolving `log4net` types by name at run time) is rejected as brittle and
> as ~30 lines against 86 lines of headroom. AC-11's second half — `PostJson` does not propagate the
> surface throw — is asserted by a real test. Recorded as a plan-level verification-route decision;
> it changes no spec requirement.

### PD-2 file:line evidence, independently re-verified at BASELINE_SHA — PREMISE DOES NOT HOLD

PD-2's factual premise is **FALSE** at `BASELINE_SHA`. Both of its "zero matches" claims are
contradicted by the tree:

| PD-2 claim | Command | Observed result | Verdict |
| --- | --- | --- | --- |
| "zero `log4net` matches in that project file" | `grep -n 'log4net' QuickFiler.Test/QuickFiler.Test.csproj` | **2 matches**, at `QuickFiler.Test/QuickFiler.Test.csproj:205-206` | **FALSE** |
| "zero `log4net` matches in any `QuickFiler.Test/**/*.cs`" | `grep -rln 'log4net' QuickFiler.Test/ --include=*.cs` | **1 file**: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs` | **FALSE** |

The observed reference, verbatim from `QuickFiler.Test/QuickFiler.Test.csproj:205-206`:

```xml
<Reference Include="log4net, Version=3.3.2.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL">
  <HintPath>..\packages\log4net.3.3.2\lib\net462\log4net.dll</HintPath>
```

### Consequence, recorded now rather than discovered later

1. PD-2's stated reason for choosing a source-level assertion over a runtime `MemoryAppender`
   assertion — that a `MemoryAppender` test "would not compile there" and that adding a `<Reference>`
   would breach the project-file budget — is **moot**: the reference already exists, no project-file
   edit would be needed, and an existing test file in the same assembly already consumes `log4net`.
2. This execution nevertheless follows the plan as written. The plan is the sole source of truth and
   P5-T8 is the task it assigns to AC-11's logging half; substituting a runtime log assertion would be
   work not described by any task, which the atomic-execution contract forbids.
3. **P5-T8's acceptance condition is therefore expected to be partially unsatisfiable.** Its third
   conjunct requires the artifact to cite "`QuickFiler.Test/QuickFiler.Test.csproj` carrying no
   `log4net` reference as the reason the assertion is source-level". That is a false statement about the
   tree, and this executor will not write a false statement into an evidence artifact to clear a gate.
   When P5-T8 is reached its first two conjuncts (exactly one matched log literal, enclosing method
   named) will be verified and recorded honestly, the false third conjunct will be recorded as
   contradicted with this artifact cross-referenced, and the task box will be left UNCHECKED and
   escalated in the final report.
4. AC-11's substantive requirement is unaffected either way: the log call is genuinely implemented by
   P5-T6 and the non-propagation half is asserted by a real test authored in P5-T1. The gap is in the
   plan's chosen *verification route* for the logging half, not in the delivered behaviour.
