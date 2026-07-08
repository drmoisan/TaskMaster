# AC8 — Pending User Confirmation (Visual Studio)

Timestamp: 2026-06-12T19-22

PostedAs: unknown

## Status: PENDING USER ACTION

AC8 is the authoritative acceptance for the coverage-exclusion's runtime EFFECT. It is PENDING user confirmation
in Visual Studio because the CLI cannot reproduce the Visual Studio static-coverage failure:

- Standalone `vstest.console` uses dynamic coverage and does NOT exercise the Visual Studio static Code Coverage
  data collector (`datacollector://microsoft/CodeCoverage/2.0`) that throws
  `System.Security.VerificationException` when instrumenting `FSharp.Core`/`Deedle`.
- Evidence: `evidence/regression-testing/exclusion-effect-not-cli-verifiable.2026-06-12T19-22.md` and the prior
  scope-change finding `evidence/other/scope-change-finding.2026-06-12T19-45.md`.

The instrument for obtaining AC8 confirmation is the verification checklist at
`evidence/issue-updates/vs-verification-checklist.2026-06-12T19-22.md`, which directs the user to:
(a) run the Deedle tests in VS with no coverage (expect green, no coverage collected), and
(b) run "Analyze Code Coverage" on the Deedle tests (expect green, no `VerificationException`).

The plan is NOT blocked on driving Visual Studio. All CLI-verifiable acceptance criteria (AC1-AC7) are satisfied;
AC8 awaits the user's Visual Studio confirmation.

## AC8 exact text (from issue.md)

> AC8 (user action, pending): User confirms in Visual Studio that (a) "Run Tests" runs the listed Deedle tests
> green with no coverage collected, and (b) "Analyze Code Coverage" runs them green with no `VerificationException`
> because the root `TaskMaster.runsettings` exclusions apply. This is the authoritative acceptance for the
> exclusion's effect, since the CLI cannot reproduce the VS static-coverage failure.

This criterion remains `[ ]` (unchecked) in `issue.md` until the user completes the checklist.
