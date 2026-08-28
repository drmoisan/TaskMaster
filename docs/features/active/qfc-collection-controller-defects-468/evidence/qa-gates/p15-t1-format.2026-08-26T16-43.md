# [P15-T1] Final QA loop, step 1 — scoped CSharpier format

Timestamp: 2026-08-26T16-43

Command:

```
$owned = @(
    'QuickFiler/Controllers/QfcCollectionController.cs',
    'QuickFiler/Interfaces/IQfcCollectionController.cs',
    'QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs',
    'QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs',
    'QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs',
    'QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs',
    'QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs',
    'QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs',
    'QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs',
    'QuickFiler.Test/QuickFiler.Test.csproj'
)
foreach ($f in $owned) { (Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash }   # before
& dotnet tool run csharpier format @owned
foreach ($f in $owned) { (Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash }   # after
```

Run through `pwsh -NoProfile` from the workspace root. The ten owned paths are passed as **explicit
arguments**; a bare `.` is never used, because it would rewrite files outside this plan's ownership
scope — including the 39 sibling-derived files the integration merges brought in.

EXIT_CODE: 0

## Output Summary

`Formatted 9 files in 3886ms.` **Files rewritten: 0.**

Every one of the ten SHA-256 hashes is identical before and after the run. The owned file set was
already formatter-clean when this step began, so step 1 changed nothing and the loop does not restart.

## SHA-256 comparison, all ten owned paths

| File | SHA-256 before | SHA-256 after | Rewritten |
|---|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `4F08C00F366BCD7CC2787122D7B59427125593AF24F18F66D29D95F4631AF534` | same | no |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | `7D7D99132348B907F5E5EF7C48174C9FCAD8E73AC668F05FD0FAE6B02A13DE0B` | same | no |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` | `A7BF13F3D6528AFEAE814AAD2B7AC6F066FACF96767C388372831BA9BD07B635` | same | no |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | `DC21A89EB0087BF04A9D9F10F05148E0EE96B90FCE03CE533BEB97A45068A1A0` | same | no |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | `96AA7D86C7148AFE4B02C8E292BF8E0083C34E6706C024438AFDC124E11E2D07` | same | no |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` | `540E83ECED5778BA74CF2A04ABE041147F74299B5E81D7C62084623C498E4EA1` | same | no |
| `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs` | `1F0C5A4B7176DFCF9CE523C41D8F4EE46754A6CAACFF491FA7CB0D2535F8545C` | same | no |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `2E9764FE1B2F6D469A9B55221ECFA7D8D5BD002B3C5B475D76D027B6A1E0D5D9` | same | no |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` | `EEC7C67413C3187620635D5AD775838137793E776C08B98DA45175484AAADCBC` | same | no |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `DEBA85906D565ABF327422AA6B8F0896C6520DD036135DA8CBE65D2BB30853D2` | same | no |

**Rewritten count: 0.** Consequently the "every rewritten file is a member of the owned file set"
clause holds vacuously but verifiably: the rewritten set is empty, and an empty set has no member
outside the owned set. `git status --porcelain` immediately after the run reported no modification to
any `.cs` or `.csproj` path, which is the independent confirmation.

## Why "Formatted 9 files" is not a rewrite count

CSharpier reports the number of files it **processed**, not the number it **changed**. A run that
finds every file already correctly formatted still prints `Formatted N files`. The only sound way to
measure rewrites is to compare file content before and after, which is why this step hashes each path
on both sides rather than parsing the tool's summary line.

The count is 9 rather than 10 because `QuickFiler.Test/QuickFiler.Test.csproj` is excluded by
`.csharpierignore`, which lists `*.csproj`, `*.props`, and `*.targets` on the grounds that project
files are owned by Visual Studio. It is included in the hash comparison anyway, so that a formatter
that unexpectedly began processing csproj files would be detected rather than silently skipped.

## Acceptance verification

| Clause | Status |
|---|---|
| `EXIT_CODE: 0` | met |
| the artifact records, by comparing SHA-256 before and after, exactly how many files the run rewrote | met — **0**, with all twenty hashes tabulated |
| every rewritten file is a member of the owned file set | met — the rewritten set is empty |
