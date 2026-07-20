---
name: 376-capstone-scope-expansion-layers
description: utilitiescs-nullable-ci-capstone (#376) P2-T17 blocking-finding escalation and its resolution across 5 layers of newly-discovered pre-existing warnings-as-errors debt
metadata:
  type: project
---

The #376 capstone's Phase 2 solution-wide rebuild gate (`msbuild TaskMaster.sln /t:Rebuild
/p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`) had never been run to completion on the
real branch tip before this feature, because commit `20d163ac` (PR #361's `/t:Rebuild` fix) is not
yet an ancestor of `origin/main`. Once run for the first time, it surfaced pre-existing
warnings-as-errors debt in 5 successive layers beyond the two originally-declared Phase 2 scope
trees (`SVGControl/**`; `UtilitiesCS/EmailIntelligence/**` + `UtilitiesCS/OutlookObjects/Folder/**`):

1. `ToDoModel.csproj` CS0618
2. `TaskVisualization.csproj` CS4014, `ToDoModel.Test.csproj` CS0169 (dead-code deletion)
3. `QuickFiler.csproj`: CS0108 (interface member hiding, pragma-suppressed — adding `new` isn't one
   of the three authorized patterns), CS0618, CS8600
4. `TaskMaster.csproj`: CS8632 (annotation-context scoping via `#nullable enable
   annotations`/`restore annotations`, NOT full `#nullable enable`), CS8767 (interface nullable
   parameter mismatch, same annotations-context bracket), CS0618; `QuickFiler.Test.csproj`:
   MSTEST0032 (tautological placeholder assert, pragma-suppressed rather than fixed — fixing would
   be a test-behavior change)
5. `TaskMaster.Test.csproj` + `UtilitiesCS.Test.csproj`: 29 more CS8632, 3 CS8625 (null-forgiving
   `!` at deliberate-null guard-clause test call sites), 3 CS0067 (unused `PropertyChanged` events
   required by `INotifyPropertyChanged`-derived interfaces — cannot be deleted, pragma-suppressed)

Orchestrator decision: expand scope to remediate ALL layers using only the three
already-established patterns (nullable annotation/null-forgiving/guard-clause; narrow pragma
bracket with rationale; dead-code deletion after grep-confirmed zero live references), looping
P2-T21/T22/T23 until `EXIT_CODE: 0`, with an explicit stop-and-escalate condition if any diagnostic
required a real behavior change. The stop condition was never triggered across all 5 layers/3 loop
iterations — every diagnostic was resolvable via the three patterns.

**Reusable lesson:** CS0108 (member hiding) and MSTEST0032 (tautological assert) are new diagnostic
CLASSES not previously seen in prior nullable-remediation children this epic; both were resolved
via narrow pragma-suppress-with-rationale rather than the "obvious" fix (adding `new`, or fixing the
placeholder assert), because those "obvious" fixes are NOT among the three authorized patterns and
would each constitute a real, if small, behavior/design change. When a plan's authorized-pattern
list is explicit and narrow, prefer the conservative pragma-suppress-with-rationale interpretation
over an unlisted-but-tempting alternative fix.
