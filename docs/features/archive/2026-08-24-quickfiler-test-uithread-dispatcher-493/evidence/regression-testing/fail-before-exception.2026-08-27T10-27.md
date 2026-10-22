# Fail-Before Exception Dossier — Part 1 of the AC-10 Evidence (P0-T14)

Timestamp: 2026-08-27T10-27
Task: [P0-T14]
Command: `sed -n '241,252p' QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (the twelve-line span of `EnsureUiThreadDispatcher` at `BASE_SHA`)
EXIT_CODE: 0
Output Summary: The pre-change body of `QfcItemControllerTestSupport.EnsureUiThreadDispatcher` is
quoted verbatim below. It returns `void`, so the regression tests R1, R2, and R3 — which consume its
return value as an `IDisposable` scope — cannot compile against the base branch, and no red *test
run* can exist for this defect. The compile-level half of the demonstration is supplied by `P1-T4`.

WhyFailingRunImpossible: At `BASE_SHA` the helper is declared
`internal static void EnsureUiThreadDispatcher()`, so a regression test that binds its result to a
variable or wraps it in `using` produces a compile error rather than a test failure. A test that
cannot be compiled cannot be executed, so there is no run in which it reports as failed; the honest
fail-before artefact is therefore this pre-change source excerpt plus the compile-error evidence
`P1-T4` records, exactly as spec § Test Strategy "Fail-before evidence" prescribes.

## Verbatim pre-change body

The span is 12 source lines. The plan cites it as `QfcItemController.TestSupport.cs` lines 238-249;
the actual span at `BASE_SHA` `125c36b0669d9dd6095f156901bba138e2272f56` is lines **241-252**, a
uniform `+3` shift recorded and explained in
`<FEATURE>/evidence/baseline/file-inventory-baseline.2026-08-27T10-18.md`. The member is identified
by name, not by line number, so the shift changes nothing about which twelve lines are quoted.

```csharp
        internal static void EnsureUiThreadDispatcher()
        {
            FieldInfo field = typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist");
            if (field.GetValue(null) == null)
            {
                field.SetValue(null, GetDedicatedDispatcher());
            }
        }
```

## Defect properties visible in the excerpt

- Return type is `void`. There is no restore path anywhere in the method or in the file.
- The read at `field.GetValue(null) == null` and the write at
  `field.SetValue(null, GetDedicatedDispatcher())` form an unsynchronized check-then-act. No lock,
  no `Monitor`, no semaphore, and no atomic primitive guards the pair.
- The value installed comes from `GetDedicatedDispatcher()`, a parked STA dispatcher that never runs
  a frame, so anything posted to it is enqueued and never completes.
- The write is conditional on the field currently being `null`, so the method never overwrites a
  live value in isolation — but under an interleaving with an unsynchronized swap it can, which is
  the #230 mechanism.

## Companion artifact

`P1-T4` supplies the compile-level half of the demonstration: with the fixture and the six
regression tests present but this helper still declared `void`, the analyzer msbuild step is expected
to fail with `error CS` diagnostics naming
`QfcItemController.UiThreadDispatcherFixtureTests.cs`. That artifact is written to
`<FEATURE>/evidence/regression-testing/fail-before-compile.<TS>.md`.

## Filename rationale

The stem is `fail-before-exception` and not any other spelling because
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md` names `fail-before-exception.*.md` as
the minimum search pattern a reviewer must use before writing a negative claim that no fail-before
evidence exists. A differently-named artifact would be invisible to that search.

SearchScope: `<FEATURE>/evidence/regression-testing/`
SearchPatterns: `fail-before-exception.*.md`
SearchResult: this file
