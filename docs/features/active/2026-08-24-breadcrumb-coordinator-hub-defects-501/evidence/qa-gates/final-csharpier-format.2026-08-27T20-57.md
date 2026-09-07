# Final QA — Step 1, CSharpier Format (P7-T1, AC-29 first half)

Timestamp: 2026-08-27T20-57

Command: `dotnet tool run csharpier format .`

EXIT_CODE: 0

Output Summary: `Formatted 1542 files in 7750ms.`

## Rewritten-file count: 3

The rewritten-file count is computed from a **before/after SHA-256 comparison**, NOT from the tool's own
processed-file count. The tool's `Formatted 1542 files` line is the number of files it PROCESSED; it
reports that same figure whether or not any file changed, so it cannot serve as the rewritten count.

Method: hash every `*.cs`, `*.xml` and `packages.config` file under `WS`, excluding `bin`, `obj`,
`packages`, `.dotnet-sdk`, `.git` and `node_modules` directory segments, with
`Get-FileHash -Algorithm SHA256`; run `csharpier format .`; re-hash the same set; count the paths whose
hash differs.

- Files hashed before: **1865**
- Files hashed after: **1865** (no file created or deleted by the formatter)
- **Files whose SHA-256 changed: 3**

The three rewritten paths, workspace rendered as the literal token `WS`:

```
WS\QuickFiler\Viewers\BreadcrumbBridgeCoordinator.Suggestions.cs
WS\QuickFiler.Test\Viewers\BreadcrumbBridgeCoordinatorSupersessionTests.cs
WS\QuickFiler.Test\Viewers\BreadcrumbCoordinatorUpgradeLifetimeTests.cs
```

## Why the count is significant beyond AC-29

All three rewritten files are **this feature's own** files: the new production partial part, the new test
file, and one existing test file this feature extended. **No sibling-owned file and no unrelated file was
rewritten.** That matters for two later gates:

- The P9-T5 scope lock admits only this feature's own `.cs` and `.csproj` paths. A repository-wide
  formatting pass that rewrote a sibling-owned or unrelated file would put that path into the diff and
  fail the lock. It did not.
- The P0-T11 baseline recorded `csharpier check .` at `EXIT_CODE: 0` with zero files needing formatting,
  so there was no pre-existing formatting debt for this pass to disturb. The 3 rewrites are therefore
  attributable entirely to code this feature authored during Phases 1 through 5.

The changes the formatter made were line-wrapping only: the `SetSuggestionsCore` parameter list was split
across three lines, and several FluentAssertions reason strings were moved onto their own lines. No
behaviour was altered.

Acceptance: `EXIT_CODE: 0` and a rewritten-file count computed from before/after SHA-256 comparison (3).
PASS.
