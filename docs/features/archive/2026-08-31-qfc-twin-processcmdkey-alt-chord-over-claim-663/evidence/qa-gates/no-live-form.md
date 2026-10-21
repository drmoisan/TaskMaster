# Phase 5 — Test-shape prohibitions ([P5-T5])

Timestamp: 2026-09-01T23-30

Command:

```
pwsh -NoProfile -Command 'Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern "new Form|: Form|Thread\.Sleep|Task\.Delay|GetTempFileName|GetTempPath"'
```

That pattern is VC-1 from the plan's Verification patterns section, quoted verbatim. The alternation pipes
are deliberately unescaped: `Select-String -Pattern` takes a .NET regular expression, in which `\|` is an
escaped literal pipe that matches nothing, so an escaped spelling would make this "returns zero matches"
assertion pass whatever the executor wrote. The `\.` sequences are correct and retained, because there the
backslash escapes a literal dot.

EXIT_CODE: 0

## Acceptance reading 1 — VC-1 returns zero matches

Match count: **0**, zero as required. This matches the `[P0-T14]` pre-change reading of zero, so the seven
added tests introduced none of the six prohibited constructs: no `new Form`, no `: Form` base-type
declaration, no `Thread.Sleep`, no `Task.Delay`, no `GetTempFileName` and no `GetTempPath`.

The added tests exercise the `internal static` predicate directly with a `Mock<IQfcKeyboardHandler>`
supplying the handler argument, so no window handle and no message pump is needed and no temporary file is
created.

## Acceptance reading 2 — the `[P4-T5]` artifact records the structural guard as passing

The `[P4-T5]` artifact at
`docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/tests-final.md`
records that `ExecutingAssembly_ContainsNoFormDerivedType` is not in the failing list. Confirmed by search
against that artifact:

```
L46: ## Acceptance reading 3 - no failing name belongs to `QfcFormKeyHandlerTests`, and none is `ExecutingAssembly_ContainsNoFormDerivedType`
L51: Passed ExecutingAssembly_ContainsNoFormDerivedType [1 ms]
L69: `QfcFormKeyHandlerTests`, and none is `ExecutingAssembly_ContainsNoFormDerivedType`, which is separately
```

That structural guard, declared at QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs line 17, fails if any
`Form`-derived type is compiled into the test assembly. It passed on the `[P4-T5]` run, which is the
runtime half of AC-12; the VC-1 search above is the source half.

Output Summary: VC-1 returns zero matches over `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`,
matching the `[P0-T14]` pre-change reading, and the `[P4-T5]` artifact records
`ExecutingAssembly_ContainsNoFormDerivedType` as passed and absent from the failing list. AC-12 holds on
both its source and its runtime half.
