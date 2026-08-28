# File Sizes After D2 ([P2-T7])

Timestamp: 2026-08-28T05-36

Command: `wc -l` over the two named files, plus a read-only
`dotnet tool run csharpier check` over the same two files.
EXIT_CODE: 0

## Line counts

| File | Baseline | Now | Delta | Limit | Result |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | **497** | **+16** | at most 500 | pass |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 382 | **419** | +37 | at most 500 | pass |

## The constrained production file, against its headroom

`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` is one of the two files constraint C2
names as constrained. Its baseline was **481** lines, giving it **19 lines of headroom** to the
500-line ceiling.

**The delivered delta is +16 lines, which is within that nineteen-line headroom.** The file now stands
at 497, three lines below the ceiling. Capacity rule 1 is satisfied and no excess needs removing.

The delivered +16 against constraint C2's planned +6 breaks down as: three lines for the retained-theme
field and its two-line comment; one line for the `_retainedTheme = theme;` assignment in `SetTheme`;
one blank separator line; a five-line comment block in `ConfigureHost` recording why the replay is
confined to the newly-adopted branch; and six lines for the guard itself, which is one line longer than
first written because the `retained != null` conjunct and its local were required to clear the CS8604
nullable warning documented in `488-d2-pass.md`.

This is the **only** edit this feature makes to this file. Constraint C2 admits no other edit to it, so
no further growth is planned and the three remaining lines of headroom are not at risk from any later
phase.

## The counts above are post-format counts

The read-only `dotnet tool run csharpier check` over both files returned **EXIT_CODE 0** with
`Checked 2 files in 769ms` and reported neither file as unformatted. Both are therefore already in
CSharpier's canonical form, and the mutating format pass in `[P8-T1]` will not rewrite either of them
or change either line count.

This matters because capacity rule 1 states that a hand count taken before the format stage is not
authoritative, since CSharpier reflows argument lists. Here the format stage has effectively already
been verified for these two files, so **497** and **419** are the delivered figures and not provisional
ones. `[P8-T8]` re-records them after the formal format pass.

Output Summary: `BreadcrumbItemViewerLifecycleCoordinator.cs` is **497** lines, a **+16** delta against
its 481-line baseline and within its **19 lines of headroom**;
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs` is **419** lines. Both are at most 500. Both are
already CSharpier-clean, so these are post-format figures.
