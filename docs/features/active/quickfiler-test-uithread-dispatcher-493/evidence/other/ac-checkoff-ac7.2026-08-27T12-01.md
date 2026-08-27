# AC-7 Check-Off (P5-T7)

Timestamp: 2026-08-27T12-01
Task: [P5-T7]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-7 ("`UtilitiesCS/Threading/UiThread.cs` is unmodified") is verified against the
recorded five-path scope-lock diff with `ProductionSourcePathCount: 0` and against the unchanged
`UiThread.cs` hash, and is checked off in `spec.md`. `PairsN: 7`, `PairsNMinus1: 6`, so exactly one
further checkbox changed state.

PairsN: 7
PairsNMinus1: 6

`pairs(7) - pairs(6) == 1`. `pairs(6)` is the value recorded by `P5-T6` in
`<FEATURE>/evidence/other/ac-checkoff-ac6.2026-08-27T11-59.md`.

## Cited artifacts, resolved per § Conventions

| Stem | Resolved filename |
| --- | --- |
| `scope-lock` | `<FEATURE>/evidence/qa-gates/scope-lock.2026-08-27T11-46.md` |
| `unowned-file-identity` | `<FEATURE>/evidence/qa-gates/unowned-file-identity.2026-08-27T11-26.md` |

## Clause-by-clause verification of AC-7 as written

### "The file does not appear in the feature's diff"

`scope-lock` records the output of
`git diff --name-only 125c36b0669d9dd6095f156901bba138e2272f56..HEAD -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets' '**/packages.config'`
as exactly five paths, all inside `QuickFiler.Test`:

```
QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
```

`UtilitiesCS/Threading/UiThread.cs` is absent, and there is no sixth path.

`unowned-file-identity` independently records the file's recomputed SHA-256 as
`87b4fde609398c59346557fb688ba192639ebc888104d74fea35d24dd18bdeaa`, equal to the value `P0-T11`
recorded, with its line count at 163.

### "no `InternalsVisibleTo("QuickFiler.Test")` grant is added to `UtilitiesCS`"

`UtilitiesCS/Properties/AssemblyInfo.cs` carries exactly three grants, unchanged:

```
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("UtilitiesCS.Test")]
[assembly: InternalsVisibleTo("ToDoModel.Test")]
```

None names `QuickFiler.Test`. A scoped `git diff --name-only <BASE_SHA>..HEAD -- 'UtilitiesCS/**'`
returns zero paths, so no file anywhere in that assembly changed and no grant could have been added.

### "no production assembly is changed by this feature"

`scope-lock` records `ProductionSourcePathCount: 0`. All five diff paths are inside the
`QuickFiler.Test` test project. The diff pathspec covers `*.cs`, `*.csproj`, `*.sln`, `*.props`,
`*.targets`, and `**/packages.config` — the complete set of file kinds that could alter a compiled
assembly or the build graph — so no production assembly's source, project file, or package pin moved.

## Conditional permission not exercised

`issue.md` § Constraints grants permission to edit `UtilitiesCS/Threading/UiThread.cs` "only if the
fix genuinely requires it". Per § Decisions Record D3, spec § Proposed Fix, and research §6, the fix
does not require it: the atomicity and mutual-exclusion the fix needs are properties of the mutators,
not of the field, and are supplied entirely inside `QuickFiler.Test`. The permission is therefore
deliberately unexercised, and this criterion records that outcome.

## Result

`- [ ] **AC-7 …` changed to `- [x] **AC-7 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the checkbox changed.
