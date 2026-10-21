# P1-T3 — AC2 live control: the formatter no longer owns the config manifests

Timestamp: 2026-09-19T12-14

Command: `dotnet tool run csharpier check .` (CMD-CSHARPIER-CHECK), run with three files transiently
perturbed and reverted immediately afterwards with
`git checkout -- UtilitiesCS/packages.config UtilitiesCS/app.config UtilitiesCS/Extensions/EnumExtensions.cs`

EXIT_CODE: 1

ExpectedExitCode: 1

The non-zero exit is the expected outcome: the C# control file is deliberately mis-formatted so the
check has something to fail on. A run that exited 0 would mean the check was not live and the
absence of the two config paths from its output would prove nothing.

## Arrangement — the three transient perturbations

The perturbations were applied byte-exactly with `[System.IO.File]::ReadAllText` and `WriteAllText`
per gate rule 14, never with `sed` through the Bash tool.

| File | Perturbation | Measured after perturbation |
|---|---|---|
| `UtilitiesCS/packages.config` | every `<package .../>` element collapsed onto one line | 143 single-line `<package .../>` elements; **0** lines whose text is exactly `<package` |
| `UtilitiesCS/app.config` | every `<assemblyIdentity .../>` element collapsed onto one line | 61 single-line `<assemblyIdentity .../>` elements; **0** lines whose text is exactly `<assemblyIdentity` |
| `UtilitiesCS/Extensions/EnumExtensions.cs` | four consecutive blank lines inserted inside the `GenericBitwiseStatic<TFlagEnum>` type body, immediately after its opening brace | 1 occurrence of the inserted blank run |

The two config files were in CSharpier's wrapped form before the perturbation, so the collapse is a
real change the formatter would have objected to had it still owned those paths. That is what makes
the negative result below meaningful rather than vacuous.

## Captured output, verbatim

```
Error .\UtilitiesCS\Extensions\EnumExtensions.cs - Was not formatted.
  ----------------------------- Expected: Around Line 26 -----------------------------
      {
          private static readonly Func<TFlagEnum, TFlagEnum, TFlagEnum> _and = And().Compile();
          private static readonly Func<TFlagEnum, TFlagEnum> _not = Not().Compile();
  ----------------------------- Actual: Around Line 26 -----------------------------
      {



Checked 1623 files in 4465ms.
```

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| The captured output names `UtilitiesCS/Extensions/EnumExtensions.cs` | named on the `Error` line as `.\UtilitiesCS\Extensions\EnumExtensions.cs` | PASS |
| The captured output names neither `UtilitiesCS/packages.config` nor `UtilitiesCS/app.config` | neither string occurs anywhere in the output | PASS |
| Post-revert `git status --porcelain --untracked-files=all -- UtilitiesCS` is empty | empty | PASS |

The C# perturbation is the control that proves the check was live: CSharpier scanned 1623 files and
reported the one deliberately broken C# file, so the silence on the two collapsed config files is a
measured exclusion produced by the P1-T2 `.csharpierignore` patterns rather than a run that did
nothing. Without that control a no-op run would have read identically on the two config paths.

## Revert

```
git checkout -- UtilitiesCS/packages.config UtilitiesCS/app.config UtilitiesCS/Extensions/EnumExtensions.cs
```

Exit 0. Post-revert porcelain over `UtilitiesCS` is empty, so all three files are back to their
committed content and no perturbation survives into any later task or commit.

Output Summary: with `UtilitiesCS/packages.config` collapsed to 143 one-line `<package>` elements,
`UtilitiesCS/app.config` collapsed to 61 one-line `<assemblyIdentity>` elements and four blank lines
inserted into `UtilitiesCS/Extensions/EnumExtensions.cs`, CSharpier checked 1623 files, exited 1,
and reported the C# file alone. Neither config path appears in its output, which is the positive
demonstration that the `.csharpierignore` patterns added at P1-T2 remove them from the formatting
gate. All three files were reverted and `UtilitiesCS` porcelain is empty. **AC2 is checked off in
`spec.md`.**
