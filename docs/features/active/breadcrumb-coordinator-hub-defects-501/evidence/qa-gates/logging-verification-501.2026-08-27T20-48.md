# QA Gate — #501 Logging Verification (P5-T8) — PARTIAL, TASK LEFT UNCHECKED

Timestamp: 2026-08-27T20-48

**Outcome: the first two acceptance conjuncts are SATISFIED and verified below. The third conjunct is
NOT SATISFIABLE because it asserts a fact that is false of the tree. The task box is therefore left
UNCHECKED and escalated in the final report. No false statement was written into this artifact to clear
the gate.**

---

## Conjunct 1 — exactly one matched line. SATISFIED.

Command: `git grep -F -n 'Breadcrumb surface delivery failed.' -- QuickFiler/Viewers/BreadcrumbMessengerHub.cs`

Output, verbatim — **exactly one** matched line:

```
QuickFiler/Viewers/BreadcrumbMessengerHub.cs:167:                        .Error("Breadcrumb surface delivery failed.", exception);
```

Matched line number: **167**.

## Conjunct 2 — the match sits inside the per-surface catch of the broadcast loop, and the surrounding statement calls `log4net.LogManager.GetLogger`. SATISFIED.

Enclosing method: **`Broadcast`** — the extracted broadcast helper that `PostJson` delegates to. The
task text explicitly admits "`PostJson` (or the extracted broadcast helper)" as the enclosing method.
`PostJson` was kept at 490 lines' worth of budget by extracting the loop, exactly as P5-T6 authorized
("If the file would exceed 500 lines, extract the broadcast loop into a small private method rather than
adding another file").

Source read back verbatim, `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:155-170`:

```csharp
private static void Broadcast(Attachment[] snapshot, string json, string? type)
{
    foreach (Attachment attachment in snapshot)
    {
        try
        {
            PostToSurface(attachment, json, type);
        }
        catch (Exception exception)
        {
            log4net
                .LogManager.GetLogger(typeof(BreadcrumbMessengerHub))
                .Error("Breadcrumb surface delivery failed.", exception);
        }
    }
}
```

The matched line at `:167` is the third line of the statement that begins at `:165`. That statement is
inside the `catch (Exception exception)` block at `:163-168`, which is the PER-SURFACE catch inside the
`foreach` over the attachment snapshot at `:157`. The statement calls
`log4net.LogManager.GetLogger(typeof(BreadcrumbMessengerHub))` and then `.Error(...)` on the returned
logger. This is the same pattern the file already used in `SafeUnsubscribe`. Control continues to the
next attachment after the catch, which is what I-501.1 requires and I-501.4 makes diagnosable.

## Conjunct 3 — "cites `QuickFiler.Test/QuickFiler.Test.csproj` carrying no `log4net` reference as the reason the assertion is source-level". NOT SATISFIABLE.

That statement is **false of the tree at `BASELINE_SHA`**. Verified:

| Claim under conjunct 3 | Command | Observed | Verdict |
| --- | --- | --- | --- |
| the test csproj carries no `log4net` reference | `grep -n 'log4net' QuickFiler.Test/QuickFiler.Test.csproj` | 2 matches at `:206-207` | **FALSE** |
| no `QuickFiler.Test/**/*.cs` uses `log4net` | `grep -rln 'log4net' QuickFiler.Test/ --include=*.cs` | 1 file: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs` | **FALSE** |

The reference, verbatim from `QuickFiler.Test/QuickFiler.Test.csproj:206-207`:

```xml
<Reference Include="log4net, Version=3.3.2.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL">
  <HintPath>..\packages\log4net.3.3.2\lib\net462\log4net.dll</HintPath>
```

Writing the required citation would put a false factual claim into the audit trail, which this executor
will not do. The defect is in ruling PD-2's premise, not in the delivered behaviour. It was detected and
recorded at P0-T18, before any implementation work began; see
`FF/evidence/other/design-rulings.2026-08-27T20-09.md` for the full analysis.

## Effect on AC-11

AC-11 has two halves. Neither is left unverified by this gate:

- **Logging half** — "a per-surface delivery failure is logged through the hub's existing `log4net`
  logger". Delivered by P5-T6 and verified at source by conjuncts 1 and 2 above: exactly one log call,
  inside the per-surface catch, through `log4net.LogManager.GetLogger`.
- **Non-propagation half** — "`PostJson` does not propagate the surface throw to its caller". Verified by
  a REAL runtime test, not by source inspection:
  `PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache` asserts
  `post.Should().NotThrow(...)`. It was RED before the fix (the propagated
  `System.InvalidOperationException: Surface delivery rejected` is quoted in
  `FF/evidence/regression-testing/red-501-starvation.2026-08-27T20-40.md`) and is GREEN after
  (`FF/evidence/regression-testing/green-500-hub-and-501.2026-08-27T20-47.md`).

What is NOT delivered is a runtime `MemoryAppender` assertion on the log call itself. Because the
`log4net` reference does in fact exist in the test project, such an assertion would have been possible
without any project-file edit — PD-2's stated obstacle does not exist. Authoring one is nonetheless work
no task in this plan describes, and the atomic-execution contract forbids performing work outside the
plan. This is recorded as a plan defect for the reviewer rather than silently worked around.
