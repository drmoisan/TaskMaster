# Fail-Before Exception Dossier — Issue #648

Timestamp: 2026-09-01T14-16

Command:
```
ls -1 fail-before-exception.*.md
```
run in
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/regression-testing/`
**before this dossier was written**, so the `SearchResult:` value below is a statement about prior
artifacts and not a claim that this dossier failed to match its own filename pattern.

EXIT_CODE: 2

The exit code is 2 because the shell's glob matched nothing and `ls` reported
`No such file or directory`. That non-zero code is the recorded outcome of the negative-claim search,
not a failure of this task.

## Negative-claim fields

SearchScope: `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/regression-testing/`

SearchPatterns: `fail-before-exception.*.md`

SearchResult: none

## WhyFailingRunImpossible

The defect lives entirely inside a test method body, so there is no unit under test and no production
line changes; a regression test asserting a production behavior therefore has nothing to assert
against. Reproducing the hazard requires forcing an interleaving between this call site's ungated
read-modify-write and a fixture transaction held by another test class, and that interleaving cannot
be forced, which is the same limitation already recorded at
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:17-21`: nothing can
force the second caller to reach its acquisition point while the first still holds the gate, and
there is no deterministic way to prove the second caller is currently blocked without a timed wait,
which the repository's determinism rules forbid. CI additionally runs the assembly with no
`/Settings:` argument (`.github/workflows/_mstest-coverage.yml:83`), so class-level parallelization is
not in force and the race is dormant in the only run that gates merge.

The one deterministic alternative — a source-scanning guard test of the kind already present at
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:49-50`, which reads controller source text
and asserts over it — is excluded by AC-6, because adding one would create a second changed `.cs`
path and AC-6 permits exactly one.

No red run is fabricated.

## Alternative proof

The substitute evidence for this change is:

1. **Structural measurement, P1-T4** —
   `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/regression-testing/p1-t4-ac2-no-reflection.md`.
   Records that `GetField`, `SetValue`, and `using System.Reflection;` each return zero matches in the
   changed file under two independent search methods, against a baseline in which all three were
   present.

2. **Structural measurement, P1-T5** —
   `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/regression-testing/p1-t5-ac1-single-owner.md`.
   Records that the quoted literal `"_dispatcher"` now appears on exactly one tracked `*.cs` line
   beneath `QuickFiler.Test/`, in the shared fixture, against a baseline of two.

3. **Behavior-preservation runs, P1-T8, P2-T5, and P2-T6.** These are the three tasks that execute the
   rewritten test:
   - `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/regression-testing/p1-t8-scoped-run.md`
   - `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/qa-gates/p2-t5-scoped-run.md`
   - `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/qa-gates/p2-t6-quickfiler-test-full.md`

   The P2-T5 and P2-T6 artifacts are written after this dossier and are named here as forward
   references. The AC-7 check-off in P2-T16 is the point at which their existence on disk is
   confirmed.

   P2-T7 and P2-T8 are **not** behavior-preservation runs and are deliberately not named as such.
   P2-T7's acceptance is confined to Cobertura numeric fields and to bookkeeping about which run
   produced the document it read, and P2-T8 runs no command at all, so neither records whether the
   rewritten test passed.

4. **The six inherited regression tests of issue #493**, at
   `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:40-344`. Their
   six `[TestMethod]` attributes are at `:40`, `:103`, `:153`, `:195`, `:262`, and `:309` of a
   346-line file. These are the authoritative fail-before and pass-after evidence for the underlying
   clobber mechanism: they exercise the contract of `UiThreadDispatcherFixture` and
   `UiThreadDispatcherTransaction` that this change now routes through, including the deterministic
   R1 reproduction of the issue #230 clobber precondition. Issue #648 consumes that contract
   unchanged rather than re-proving it.
