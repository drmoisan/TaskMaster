# Batch D — Nullable Pragma Gate

- Timestamp: 2026-07-19T11-55
- Task: [P4-T4]
- Batch D file: `MyBox.cs`

## Step 1 — CSharpier format

- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output Summary: `Formatted 1406 files`. Only `MyBox.cs` changed.

## Step 2 — Authoritative CS86xx detector (scoped isolated UtilitiesCS build)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx count: 0
- Output Summary: `0 Error(s)`. Zero CS86xx for `MyBox.cs`.

## Step 3 — Plan-mandated solution-wide command (for the record)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 1
- CS86xx count: 0
- Output Summary: Aborts on pre-existing vendored SVGControl CS0649 (invariant); zero CS86xx.

## Annotations applied (Batch D)

- `MyBox.cs`: pragma + `FunctionButtonGroup<T>.Result` → `public T? Result` (was uninitialized `T`,
  CS8618), the deliberate unconstrained-generic decision consistent with `FunctionButton<T>.Value`
  (Batch B). Both `ShowDialog<T>` overloads (the `FunctionButtonGroup<T>` overload and the
  `Dictionary<string, Func<Task<T>>>` overload that returns it) → `public static T? ShowDialog<T>`
  to match the `T?` result (CS8603). The `AsyncLocal<Func<MyBoxViewer, DialogResult>>` invoker seam
  and its `?? RealDialogInvoker` fallback are unmodified. No new runtime guards; the button
  wrappers' non-null `.Button` properties (Batch B) mean `tlp.Controls.Add(...Button, ...)` needs no
  change. The `MessageBoxIcon`/`BoxIcon` switch `default` branches required no annotation.

Result: AC1 satisfied for Batch D; the `T?` changes are additive nullability consistent with the
consumed `WinFormsExtensions.Clone<T>()` contract (AC5).
