# Batch B — Nullable Pragma Gate

- Timestamp: 2026-07-19T11-35
- Task: [P2-T5]
- Batch B files: `ActionButton.cs`, `DelegateButton.cs`, `FunctionButton.cs`

## Step 1 — CSharpier format

- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output Summary: `Formatted 1406 files`. Only the 3 Batch B files changed.

## Step 2 — Authoritative CS86xx detector (scoped isolated UtilitiesCS build)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx count: 0
- Output Summary: `0 Error(s)`. Zero CS86xx across the 3 Batch B opted-in files.

## Step 3 — Plan-mandated solution-wide command (for the record)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 1
- CS86xx count: 0
- Output Summary: Aborts on pre-existing vendored SVGControl CS0649 (invariant); zero CS86xx.

## Annotations applied (Batch B — consistent across the trio)

- Fields `_name` → `string?`, `_button` → `Button?`, and the delegate-typed field
  (`_action`/`_delegate`/`_function`) → nullable, reflecting the parameterless-constructor and
  partial-construction paths (CS8618). `_template` stays non-null (inline-initialized).
- Public property getters `Name`/`Button`/`Delegate` keep their existing non-null return types
  (AC5: the spec's explicit signature-change list does not include these), suppressing CS8603 with a
  runtime-neutral `!` on the getter (`get => _field!;`) rather than widening the public surface.
- `Button_Click` delegate invocation reaches non-null through the `!`-guarded getter (ActionButton/
  DelegateButton use `_action!`/`_delegate!` directly), not a new runtime guard.
- `FunctionButton<T>`: additionally `_buttonClicked`/`_buttonClickedAsync` → nullable (set only on
  some construction paths; wired only under existing not-null guards); `public T? Value` (was
  uninitialized `T`, CS8618) per the plan's unconstrained-generic decision, consistent with
  `MyBox.FunctionButtonGroup<T>.Result` (Batch D) and the `WinFormsExtensions.Clone<T>()` contract.

Result: AC1 satisfied for Batch B (zero CS86xx); AC5 preserved (only additive nullability on
`FunctionButton<T>.Value`; Name/Button/Delegate signatures unchanged).
