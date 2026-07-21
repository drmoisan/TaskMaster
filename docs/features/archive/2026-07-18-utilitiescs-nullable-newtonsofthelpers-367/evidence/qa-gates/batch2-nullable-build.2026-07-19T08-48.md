# Batch 2 Nullable Build Verification (P2-T5)

- Timestamp: 2026-07-19T08-48
- Opted-in files (3): `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`, `ILInstruction.cs`, `MethodBodyReader.cs`

## Genuine nullable gate (authoritative — actually compiles NewtonsoftHelpers)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded, zero errors, zero `CS86xx` in `NewtonsoftHelpers/`/`SDIL Reader/`. CS86xx remains fatal under this gate, so EXIT 0 proves all 3 Batch 2 files are nullable-clean under their `#nullable enable` pragmas.

## Exact plan solution command (invariant, per baseline)

- Command: `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` — invariant with P0-T4 (SVGControl-blocked before UtilitiesCS; edits confined to `NewtonsoftHelpers/SDIL Reader/`, unreachable by the solution command). Executed in full at P9-T3.

## Edits applied (annotation-only)

- `ILGlobals.cs`: `#nullable enable`; `multiByteOpCodes`/`singleByteOpCodes` -> `= null!` (documented invariant: populated by `LoadOpCodes()` before use, preserving the non-null contract consumers rely on); `modules = null` -> `Module[]? modules = null`; `(OpCode)info1.GetValue(null)` -> `(OpCode)info1.GetValue(null)!` (guarded by `FieldType == typeof(OpCode)`, `// why` comment).
- `ILInstruction.cs`: `#nullable enable`; settable fields `object operand`/`byte[] operandData` -> `object? operand`/`byte[]? operandData` and public `Operand`/`OperandData` -> `object?`/`byte[]?`; behavior-preserving `!` on `fOperand.ReflectedType`/`mOperand.ReflectedType` reads (`Type?`). The `operand != null` guard already narrows the switch body.
- `MethodBodyReader.cs`: `#nullable enable`; `instructions` -> `List<ILInstruction>?`; `il` -> `byte[] il = null!` (documented invariant: assigned from the method body in the ctor before Read* deref it); `mi` -> `MethodInfo? mi`; `this.mi!.DeclaringType!.GetGenericArguments()`/`this.mi!.GetGenericArguments()`; ctor `il = mi.GetMethodBody()!.GetILAsByteArray()!` (guarded by the preceding `!= null` check); `GetRefferencedOperand` return -> `object?` (body has a `return null;` path).
