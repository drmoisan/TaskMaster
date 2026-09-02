# Phase 0 — carrier construction-site inventory (P0-T13, AC3)

Timestamp: 2026-09-01T21-44

Base ref: `807fb0bb6e5e49f43efa6b256b05960bf078ca19`. Every list below was re-derived directly against
the tree at that base ref by an ordinal substring scan over every `.cs` file under `QuickFiler/` and
`QuickFiler.Test/`, excluding `bin/` and `obj/`. No entry is copied from the research document or
from any prior enumeration.

## List 1 — `new QfcPreScoredItem(` in `QuickFiler` and `QuickFiler.Test`

| # | File | Line | Text |
|---:|---|---:|---|
| 1 | QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 86 | `.Select(result => new QfcPreScoredItem(result.item, result.topFolder))` |
| 2 | QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 195 | `accepted.Add(new QfcPreScoredItem(mailItem, topFolder));` |
| 3 | QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | 307 | `var carrier = new QfcPreScoredItem(mail, @"\\Archive\Projects\Active");` |
| 4 | QuickFiler.Test/Controllers/QfcFormControllerTests.cs | 814 | `new QfcPreScoredItem(new Mock<MailItem>().Object, @"\\A\folder"),` |

**COUNT: 4** — two production sites and two test sites.

AC3 requires every **production** construction site to populate the new member. There are exactly
two: `QfcHighConfidencePreFilter.cs:86` and `QfcStreamingDequeueConfidenceGate.cs:195`.

Site 1 (`QfcHighConfidencePreFilter.cs:86`) is inside `QfcHighConfidencePreFilter.FilterAsync`, which
AC13 requires to remain dormant. Dormancy does not exempt it: the constructor signature is widened
by P1-T4, so this site must be updated to compile at all, and it must populate the new member with
the handler its own `ScoreAsync` call now returns rather than with a null placeholder. The plan's
P1-T4 prose names `:98-122`, `:143-147`, `:170-189` and `:184` but does not name `:86`; it is
recorded here so P1-T4 covers it.

Site 2 (`QfcStreamingDequeueConfidenceGate.cs:195`) is the live producer.

The two test sites are collateral owned by P1-T4 per the P1-T10 assignment clause.

## List 2 — `IFolderScoringService` in `QuickFiler.Test`

| # | File | Line | Classification |
|---:|---|---:|---|
| 1 | QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 337 | **Strict-behaviour setup** — `new Mock<IFolderScoringService>(MockBehavior.Strict)` |
| 2 | QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | 18 | **Reference of another kind** — `<see cref="IFolderScoringService"/>` inside a class-level XML documentation comment |
| 3 | QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | 65 | **Reference of another kind** — `<see cref="IFolderScoringService"/>` inside a helper-method XML documentation comment |
| 4 | QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | 68 | **Mock declaration** — the return type `Mock<IFolderScoringService>` of the `BuildScoringMock` helper |
| 5 | QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | 72 | **Strict-behaviour setup** — `new Mock<IFolderScoringService>(MockBehavior.Strict)` |
| 6 | QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 160 | **Strict-behaviour setup** — `new Mock<IFolderScoringService>(MockBehavior.Strict)` |
| 7 | QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 221 | **Strict-behaviour setup** — `new Mock<IFolderScoringService>(MockBehavior.Strict)` |

**COUNT: 7** — 4 strict-behaviour setups, 1 mock declaration, 2 documentation references.

The four strict-behaviour setups are load-bearing for P1-T4: `MockBehavior.Strict` throws on any
invocation that has no matching `Setup`, so widening `ScoreAsync`'s return type invalidates every
`ReturnsAsync` whose tuple arity no longer matches, and each of the four fails loudly rather than
degrading quietly. The two documentation references need no code change but are recorded so a later
audit does not read their absence from the edit list as an omission.

## List 3 — `ScoringServiceFactory` in `QuickFiler` and `QuickFiler.Test`

| # | File | Line | Text |
|---:|---|---:|---|
| 1 | QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 260 | `internal Func<IFolderScoringService> ScoringServiceFactory { get; set; } =` |
| 2 | QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 268 | `var scoringService = ScoringServiceFactory();` |
| 3 | QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 323 | `/// Scoring is driven through the <c>ScoringServiceFactory</c> seam added by [P1-T5] so no` |
| 4 | QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 349 | `model.ScoringServiceFactory = () => scoringService.Object;` |
| 5 | QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 142 | `/// Scoring is driven through the <c>ScoringServiceFactory</c> seam so no live Outlook COM` |
| 6 | QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 178 | `model.ScoringServiceFactory = () => scoringService.Object;` |
| 7 | QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 242 | `model.ScoringServiceFactory = () => scoringService.Object;` |

**COUNT: 7** — 1 production declaration, 1 production call, 3 test assignments, 2 documentation
references.

The production declaration at `:260-261` is the existing injectable-delegate-seam precedent that
P1-T6 mirrors for leg B, per `.claude/rules/csharp.md:52`.

Output Summary: 4 `new QfcPreScoredItem(` sites (2 production, 2 test); 7 `IFolderScoringService`
sites in `QuickFiler.Test`; 7 `ScoringServiceFactory` sites across both projects. All counts derived
at the base ref by direct scan.
