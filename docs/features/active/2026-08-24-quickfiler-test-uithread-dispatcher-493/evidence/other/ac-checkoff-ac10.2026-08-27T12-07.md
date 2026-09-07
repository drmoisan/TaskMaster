# AC-10 Check-Off (P5-T10)

Timestamp: 2026-08-27T12-07
Task: [P5-T10]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-10 ("Fail-before evidence is captured in the form the defect permits") is verified
against both fail-before artifacts and is checked off in `spec.md`. `PairsN: 10`,
`PairsNMinus1: 9`, so exactly one further checkbox changed state. A confirming search finds zero
remaining unchecked `- [ ] **AC-` lines in `spec.md`.

PairsN: 10
PairsNMinus1: 9

`pairs(10) - pairs(9) == 1`. `pairs(9)` is the value recorded by `P5-T9` in
`<FEATURE>/evidence/other/ac-checkoff-ac9.2026-08-27T12-05.md`.

## Cited artifacts, resolved per § Conventions

| Producing task | Stem | Resolved filename |
| --- | --- | --- |
| `P0-T14` | `fail-before-exception` | `<FEATURE>/evidence/regression-testing/fail-before-exception.2026-08-27T10-27.md` |
| `P1-T4` | `fail-before-compile` | `<FEATURE>/evidence/regression-testing/fail-before-compile.2026-08-27T10-44.md` |

Both are under `<FEATURE>/evidence/regression-testing/`, the canonical location the
`evidence-and-timestamp-conventions` skill names for fail-before evidence.

## Verification — first artifact

It quotes the verbatim pre-change body of `QfcItemControllerTestSupport.EnsureUiThreadDispatcher`
inside a fenced `csharp` block — the twelve-line span the spec cites as `TestSupport.cs:238-249`,
located at lines 241-252 in the tree at `BASE_SHA` — and it carries the required field:

```
WhyFailingRunImpossible: At `BASE_SHA` the helper is declared
internal static void EnsureUiThreadDispatcher(), so a regression test that binds its result to a
variable or wraps it in `using` produces a compile error rather than a test failure. A test that
cannot be compiled cannot be executed, so there is no run in which it reports as failed; the honest
fail-before artefact is therefore this pre-change source excerpt plus the compile-error evidence
P1-T4 records.
```

It also names `P1-T4` as the task supplying the compile-level half of the demonstration, and records
`SearchScope:`, `SearchPatterns:`, and `SearchResult:` so a reviewer's negative-claim search is
auditable.

Its filename stem is `fail-before-exception`, matching the `fail-before-exception.*.md` pattern the
skill names as the minimum a reviewer must search before writing a negative claim that no fail-before
evidence exists.

## Verification — second artifact

| Field | Recorded value | Required |
| --- | --- | --- |
| `ExpectedExitCode:` | `1` | present |
| `EXIT_CODE:` | `1` | non-zero |
| `FailBeforeErrorLineCount:` | `6` | greater than zero |

The six lines are three distinct `CS0029: Cannot implicitly convert type 'void' to
'System.IDisposable'` diagnostics at source lines 56, 114, and 160 of
`QfcItemController.UiThreadDispatcherFixtureTests.cs` — one each for R1, R2, and R3 — each reported
twice by MSBuild. All three are quoted verbatim in redacted form in that artifact. It is this plan's
only `[expect-fail]` task, and the failure is the evidence.

## The pass-after counterpart

`P2-T3` records the counterpart at
`<FEATURE>/evidence/regression-testing/pass-after-compile.2026-08-27T10-58.md`: `EXIT_CODE: 0` with
zero lines containing both `QfcItemController.UiThreadDispatcherFixtureTests.cs` and `error CS`, and
zero `error CS` lines anywhere in the log. The only change between the two runs is the `P2-T1` and
`P2-T2` edits, so the fail-before / pass-after pair is attributable to exactly the fix.

## Why this form rather than a red test run

AC-10's own text asks for this form: "rather than asserting a red test run that cannot exist". The
repository's Bugfix Workflow requires a failing regression test first, and this criterion records the
specific, honest shape that requirement takes when the fix is a signature change. No red run was
fabricated.

## Result

`- [ ] **AC-10 …` changed to `- [x] **AC-10 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the checkbox changed. A
search for `- [ ] **AC-` in `spec.md` now returns zero matches, so all ten acceptance criteria are
checked off.
