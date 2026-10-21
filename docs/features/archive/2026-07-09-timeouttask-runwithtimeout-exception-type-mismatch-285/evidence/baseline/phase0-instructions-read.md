# P0-T1 — Phase 0 Instructions Read

Timestamp: 2026-09-01T08-02

Policy Order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

## Files Read (explicit list, in the order read)

1. `CLAUDE.md` — read in full (448 lines). Policy Compliance Order, General Code Change Policy including the Bugfix Workflow, C# Code Change Policy (CSharpier 1.2.6 via `dotnet tool run`, `/t:Rebuild` mandatory, no `/p:Nullable=enable`), General Unit Test Policy, C# Unit Test Policy, Tone Policy.
2. `.claude/rules/general-code-change.md` — read in full (81 lines). Design principles, mandatory toolchain loop, 500-line file size limit, error handling, naming, I/O boundaries.
3. `.claude/rules/general-unit-test.md` — read in full (106 lines). Core principles, coverage requirements, coverage exclusion policy, scenario completeness, Arrange-Act-Assert, external dependencies, test file location, determinism infrastructure including the banned-API list naming `Thread.Sleep` and `Task.Delay`.
4. `.claude/rules/csharp.md` — read in full (97 lines). C# toolchain, coding standards, testing standards (MSTest + Moq + FluentAssertions), deterministic test rules, DI seams (injectable delegate seam is the sanctioned pattern for a single call path), analyzer stack, prohibited behaviors.
5. `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/spec.md` — read in full (295 lines). Sole acceptance-criteria source for this `full-bug` work mode item; carries 12 unnumbered checklist bullets under its `## Acceptance Criteria` heading at lines 267-278.

## Notes Bearing on Execution

- Work Mode is `full-bug`; `spec.md` is the sole acceptance-criteria source. No `user-story.md` exists in the feature folder and none is created.
- The Bugfix Workflow in `CLAUDE.md` requires a failing regression test before the fix. The plan's Phase 1 / Phase 2 split implements that ordering.
- `.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay`, and real wall-clock waits in test code. The plan's regression test uses the injectable `timeoutSourceFactory` seam and asserts zero occurrences of the banned APIs.
- `.claude/rules/csharp.md` DI-seam ordering names the injectable delegate seam as the correct choice for a single call path, which is the seam this plan adds.
- The plan's "Correction to the Spec's Recommended Edit" section supersedes `spec.md` line 148: the clause is written `catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)` because `Microsoft.Office.Interop.Outlook` is imported at line 9 of the production file and a bare `Exception` is CS0104-ambiguous.

Acceptance: this artifact exists and lists all five paths.
