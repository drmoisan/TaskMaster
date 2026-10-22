# Phase 2 Verification — Session-Metrics Writer Files (issue #742, [P2-T4])

Timestamp: 2026-09-14T02-16

Command: `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.Metrics.cs`

EXIT_CODE: 0

Output Summary:

```
QuickFiler/Controllers/QfcHomeController.Metrics.cs:2
```

The command printed exactly one line, for `QfcHomeController.Metrics.cs` with a count of 2, and
printed no line for `EfcHomeController.Metrics.cs`. The baseline combined total for these two files
was 4 + 4 = 8 ([P0-T9] control 1).

Acceptance: satisfied. The residual 2 are the two commented-out predecessor statements at the top of
`QuickFileMetrics_WRITE`:

```
//var curDateText = DateTime.Now.ToString("MM/dd/yyyy");
//var curTimeText = DateTime.Now.ToString("hh:mm");
```

This change deliberately does not edit them. They are inert comments and carry no runtime behaviour.

## Discovery-control note (guard against a vacuous zero)

The same pattern was run in the same pass against the three UI-facing files, which Phase 2 does not
touch, and printed `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:1`,
`QuickFiler/Controllers/QfcCollectionController.cs:3` and
`QuickFiler/Controllers/EfcItemController.cs:2`, exit code 0. The pattern therefore still matches on
this tree, so the zero-match result for `EfcHomeController.Metrics.cs` is a real observation rather
than a search that cannot match.

## Invocation note

The pattern contains both double quotes and a backslash escape, which neither a `pwsh -Command`
string nor a Bash-quoted argument reproduces reliably on this host. It was therefore placed verbatim
in a single-quoted PowerShell variable inside a throwaway scratchpad script run with
`pwsh -NoProfile -File`, which passes the pattern to `git grep` as one unmodified argument. The
script lives outside the repository, is a temporary throwaway created and deleted within this
session, and contains no repository logic beyond the literal pattern and pathspec quoted above.
