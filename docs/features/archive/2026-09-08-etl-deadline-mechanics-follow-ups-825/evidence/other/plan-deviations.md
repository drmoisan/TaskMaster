# Execution Deviations from the Plan Text

Timestamp: 2026-09-09T16-44

This artifact records every point at which execution departed from the literal text of
plan.2026-09-08T23-51.md, with the measurement that forced the departure and the reasoning. It is
maintained during the run and referenced from the P9-T5 review handoff.

## D1 — D7 sanitisation extended by a third rewrite (affects P0-T6, P0-T7, P1-T2, P8-T3, P8-T4)

D7 names two rewrites for every MSBuild file log: the worktree root to the literal `<repo-root>`
and the main checkout root to the literal `<main-checkout-root>`. Both were applied to every log,
in that order, because the main checkout root is a proper prefix of the worktree root and rewriting
the shorter string first would corrupt the longer one.

After those two rewrites, the first log still carried 21 lines containing the token `C:\Users\`, in
two leak classes D7 does not name: an MSBuildUserExtensionsPath property expanded from the
environment, and a _DeploymentUrl property reassignment naming a OneDrive folder. Both carry the
host account name. P9-T4's confirming check requires a zero count of `C:\Users\` in each of the five
committed logs, so that check could not have passed under the two-rewrite form.

A third rewrite was therefore added, mapping the user profile root to the literal
`<user-profile-root>`, applied after the two D7 rewrites so that it reaches only the residual
occurrences. Every log this plan writes carries a per-log confirmation that its `C:\Users\` count is
zero.

## D2 — Repo-local .NET SDK installed before P0-T4

The first `dotnet tool restore` in this fresh worktree failed with exit code -2147450725 and the
message directing the operator to run ./scripts/vscode/Install-RepoDotNetSdk.ps1. The worktree
carried no .dotnet-sdk directory. That script was run as a micro-action, installing .NET SDK 8.0.205
into the git-ignored .dotnet-sdk directory, and the restore was then re-run and succeeded. No
tracked file was changed and no diff is affected.

## D3 — P2-T2 pinned Compile Include count disagrees with the tree

P2-T2's acceptance requires that the count of lines in UtilitiesCS.Test/UtilitiesCS.Test.csproj
containing the token `Compile Include` be exactly 471, one greater than a stated pre-change 470.

Measured against this worktree at the D3 anchor, the pre-change count is 474 and the post-change
count is 475. The stated 470 does not describe this tree; it appears to have been measured against a
different checkout or an earlier state of the shared project file, which several sibling features in
this epic also add entries to.

The task's substance is unaffected and is proven by the two falsifiable clauses that do hold:

- The file contains exactly one line containing the token `GetTableInViewAsyncClockTests.cs`.
- `git diff $b --numstat -- UtilitiesCS.Test/UtilitiesCS.Test.csproj` at the D3 anchor reports
  exactly 1 added line and exactly 0 removed lines.

The numstat clause is the stronger of the two forms: it proves that exactly one line was added and
nothing was reordered or reformatted, which is what AC29 requires and what the fan-in merge depends
on. The absolute count clause proves nothing the numstat does not, and its stated literal is an
authoring-time figure rather than a property of the delivered change. P2-T2 is therefore recorded as
passed on the two discriminating clauses, with the stale literal recorded here and reported.

## D4 — The new test file must bind through reflection, not a direct call

P2-T1 specifies that the new test "calls GetTableInViewAsync on a mocked Explorer". The first
implementation did so directly. That form does not compile. The build failed with

    error CS1769: Type 'Task<Table>' from assembly 'UtilitiesCS, Version=1.0.0.0, Culture=neutral,
    PublicKeyToken=null' cannot be used across assembly boundaries because it has a generic type
    argument that is an embedded interop type.

`Outlook.Table` is an embedded interop type, so `Task<Outlook.Table>` cannot be awaited from
UtilitiesCS.Test at all. This is the same CS1769 constraint spec.md discusses at item 5, and it is
why all four existing binding sites in OlTableExtensions_Tests go through the reflective
InvokeAsyncResult helper declared at lines 1820-1842 of that file rather than calling the method
directly.

The new test file therefore carries its own reflective helper, InvokeGetTableInViewAsync, built on
the same pattern: bind the method by name and an explicit parameter-Type array, invoke it, await the
returned non-generic Task, and read the boxed Result through reflection. The acceptance conditions
P2-T1 states are unaffected; the mechanism is the only thing that changed, and it changed because
the specified mechanism does not compile.

Consequence carried into Phase 3. The reflective binding names the parameter-Type array explicitly,
so P3-T2's addition of a trailing TimeProvider parameter invalidates it exactly as it invalidates
the four binding sites P3-T9 updates. Updating the new file's array and argument list is therefore a
mechanically necessary micro-action inside Phase 3, of the same kind P3-T7 already makes explicit
for the three BuildExplorer call sites: the file does not compile without it.

## D5 — Artifact timestamps corrected to observed write times

Ten artifacts written between 16-33 and 16-44 initially carried extrapolated Timestamp values rather
than observed ones, and four of those values ran ahead of the real clock. Every Timestamp line in
this feature's evidence folder was reset to the file's actual LastWriteTime on disk, rounded to the
yyyy-MM-ddTHH-mm form the conventions require. No other field was altered. Timestamps written after
this correction are read from the clock at write time.
