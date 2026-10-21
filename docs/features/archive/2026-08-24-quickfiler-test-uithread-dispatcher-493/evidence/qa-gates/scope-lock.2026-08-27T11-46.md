# Scope Lock Verified Against the Committed Diff (P4-T7)

Timestamp: 2026-08-27T11-46
Task: [P4-T7]
Command: `git diff --name-only 125c36b0669d9dd6095f156901bba138e2272f56..HEAD -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets' '**/packages.config'`
EXIT_CODE: 0
Output Summary: The command returned exactly the five source paths in § Scope Lock and no sixth path.
`ProductionSourcePathCount: 0`.

ProductionSourcePathCount: 0

`BASE_SHA` used: `125c36b0669d9dd6095f156901bba138e2272f56`, as recorded by `P0-T2` in
`<FEATURE>/evidence/baseline/toolchain-resolution.2026-08-27T09-53.md`.
`HEAD` at the time of the command: `2057a3fd`.

## Returned paths, verbatim

```
QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
```

Five paths returned, five paths in § Scope Lock, set equality holds in any order. There is no sixth
path.

## Cross-check against § Scope Lock

| § Scope Lock path | Disposition | Present in diff |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | new | yes |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | new | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | modified | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | modified | yes |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified, two `<Compile Include>` entries only | yes |

Paths the plan must not write, confirmed absent from the diff:

| Path | Absent |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (sibling-owned) | yes |
| `UtilitiesCS/Threading/UiThread.cs` | yes |
| any `QuickFiler/**` production source | yes |
| any `.github/workflows/**` file | yes (and it is outside the pathspec's extensions) |
| `TaskMaster.sln` | yes |
| any `packages.config` | yes |

## ProductionSourcePathCount

`ProductionSourcePathCount: 0`. All five returned paths are inside the `QuickFiler.Test` test project.
No production assembly's source, project file, props file, targets file, or `packages.config` appears
in the diff, so no production assembly is changed by this feature and no assembly's public surface
moves.

The pathspec covers `*.cs`, `*.csproj`, `*.sln`, `*.props`, `*.targets`, and `**/packages.config`,
which is the complete set of file kinds that could alter a compiled assembly or the build graph. The
`packages/` back-fill performed by `P0-T6` copied files into a git-ignored directory and therefore
correctly does not appear.
