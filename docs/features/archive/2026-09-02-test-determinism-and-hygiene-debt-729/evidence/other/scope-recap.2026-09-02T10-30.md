# Scope recap (P0-T2)

Timestamp: 2026-09-03T01-05

Command: n/a — this artifact records orchestrator scope decisions already made; it runs no command.

EXIT_CODE: 0

## (a) Finding 2 expansion to SVGControl.Test

Issue #729 names only `UtilitiesCS.Test/ResourceTests.cs:20` as the live-`Form` site. That file is
an orphan on disk: it is not listed in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, which uses an
explicit `<Compile Include>` list with no wildcard globbing, so it never enters the assembly.
Acting on the issue's literal citation alone would satisfy the letter of the issue while leaving
the actual defect untouched.

`SVGControl.Test` is the only site in the repository where a `Form`-derived type is genuinely
compiled into a unit-test assembly, and therefore the only site where fail-before evidence exists.
The orchestrator's decision is to include `SVGControl.Test` in scope. This expansion is deliberate
and is recorded in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md`
lines 74-79.

## (b) Finding 4 is out of scope and promoted as issue #743

Finding 4 (pump-hosted `QfcItemController` / `PumpTimeoutMs` load sensitivity) is explicitly and
entirely out of scope for issue #729. It has been promoted as follow-up issue #743, whose promotion
record is at `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md`.

The promotion is load-bearing: the prior standalone tracker #711 was already closed as superseded
by #729, so #743 exists specifically so that closing #729 does not drop the finding a second time.

No file under `QuickFiler/` is added, modified, or deleted by this plan.

## (c) Whitespace-in-path harvester limitation

The repository-relative path `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`
contains a space in the directory segment `Filter DASL`. It is written throughout the plan as a
single backticked token, but it will not survive a whitespace-splitting blast-radius or contention
extractor as one token. Renaming the directory is out of scope for this minimal bugfix. Downstream
schedulers must treat this one path specially.

## (d) Block L verbatim — the four Finding-4 reasons quoted from research 4.2

Research artifact
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md`
line 240 states that a test-side replacement of `WinFormsPumpHost` "is nevertheless not
*sufficient*, for four independently verified reasons", and enumerates them at lines 242, 248, 250,
and 252. Their four bolded lead sentences are reproduced verbatim below.

Finding 4 — reasons no test-only fix exists:
1. The production code reads the context off the control, not from an injected seam.
2. The fixture's cost is the real WinForms control tree, not the pump.
3. `[DoNotParallelize]` would be a no-op.
4. Removing `[Timeout]` trades a bounded failure for an unbounded hang.

Output Summary: Scope recap recorded. Finding 2 expanded to include SVGControl.Test; Finding 4 out
of scope and carried by #743; the `Filter DASL` whitespace path limitation surfaced; Block L's
heading line plus its four numbered reason lines written verbatim.
