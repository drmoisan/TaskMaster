# Fail-Before Exception Dossier — Cycle 4 / AC25 (#177)

Timestamp: 2026-06-16T10-26
Plan task: [P1-T1] (red-before-green regression test) — INV-5
Status: SCOPE-CHANGE FINDING. The plan's specified failing regression test cannot be made RED
against the current HEAD production code. Production code was NOT modified (containment held).

## WhyFailingRunImpossible

The defect described by the plan/spec/research (an `AdjustForMaxPath()` NRE dereferencing a null
`_fileExtension`/`_fileStemSuffix` during Json.NET default-constructor property-set deserialization)
is NOT reproducible through `JsonConvert.DeserializeObject<FilePathHelper>(...)` on the current HEAD
code, for any property ordering, with default settings OR with the SmartSerializable settings
(`TypeNameHandling.Auto` + `PreserveReferencesHandling.Objects`), standalone OR nested inside a
`NewSmartSerializableConfig.Disk` populated via `FromSeed` (the exact cycle-3 scenario).

Therefore the specified regression test
`DeserializeFromSeedJson_WhenFileStemSeedSetBeforeExtension_DoesNotThrow` and the round-trip test
`DeserializeFromSeedJson_RoundTrip_PreservesAllStemFields` both PASS against the unmodified
production code. There is no RED state to capture, so INV-5 (red-before-green) and the repo bugfix
workflow ("test must fail before the fix") cannot be satisfied as the plan is written.

## Root-cause analysis of why the NRE is unreachable on HEAD

`AdjustForMaxPath()` (FilePathHelper.cs:292-308) is guarded by `StemInitialized()`
(FilePathHelper.cs:183-191):

```
internal bool StemInitialized()
{
    if (FileStemSeed is null || FileStemSuffix is null || FileExtension is null)
    {
        if (FolderPath.IsNullOrEmpty() || !TryParseFileName(FileName))
            return false;          // partial-init -> returns false, AdjustForMaxPath returns false
    }
    return !FolderPath.IsNullOrEmpty();
}
```

The NRE in research requires reaching line 298 (`FileExtension.Length`) with a null backing field.
That requires `StemInitialized()` to return `true` while a stem field is null. The only path into
the body when a stem field is null is the inner branch, which calls `TryParseFileName(FileName)`:

- If `FileName` is empty or unparseable -> `TryParseFileName` returns false -> inner returns false ->
  `StemInitialized()` returns false -> `AdjustForMaxPath()` returns false. No dereference. No NRE.
- If `FileName` parses successfully -> `TryParseFileName` (FilePathHelper.cs:271-290) SETS
  `_fileStemSeed`, `_fileStemSuffix`, `_fileStem`, `_fileExtension` to non-null before returning
  true. So by the time line 298 runs, all three fields are non-null. No NRE.

The inner branch is therefore self-healing: it either bails out or fully populates the fields. The
actual serialized document shape produced by `JsonConvert.SerializeObject(FilePathHelper)` is
(verified via a throwaway probe, since removed):

```
{"FilePath":null,"FolderPath":"C:\\data","FileName":"","FileStemSeed":"report",
 "FileStemSuffix":"_bk","FileStem":null,"FileExtension":".json"}
```

`FolderPath` is serialized BEFORE `FileStemSeed`, and `FileName` is `""`. On deserialize, the
`FileStemSeed` setter fires with `FileName == ""`, so `TryParseFileName("")` returns false and the
method bails. This holds for the nested `Config.Disk` cycle-3 case as well (probe variant E = OK).

## SearchScope / SearchPatterns / SearchResult (negative-evidence audit)

- SearchScope: deserialization of `FilePathHelper` standalone and as `NewSmartSerializableConfig.Disk`,
  with default settings and with `TypeNameHandling.Auto` + `PreserveReferencesHandling.Objects`.
- SearchPatterns: property orderings A (real shape), B (FolderPath+parseable FileName first),
  C (FilePath first), D (SmartSerializable settings round-trip), E (nested Config via FromSeed).
- SearchResult: A=OK, B=OK, C=OK, D=OK, E=OK. No throw observed on any path against HEAD.

## Evidence-of-state

- Production `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`: NO diff (untouched).
- The two plan-specified regression tests, run against unmodified production code:
  `vstest.console.exe ... /TestCaseFilter:"FullyQualifiedName~DeserializeFromSeedJson"` ->
  Total 2, Passed 2, Failed 0, EXIT_CODE 0. (Both GREEN before any fix — the blocking condition.)

## Why I am not proceeding

Continuing would force a policy violation:
1. Applying the production null-guard for a defect the specified test cannot demonstrate would
   violate the bugfix workflow (no failing test precedes the fix) and produce an unfalsifiable
   regression test (passes with and without the guard) — it would not protect against regression.
2. Manufacturing a RED by weakening/contorting the test to assert on internal state the public
   deserialize path never reaches is prohibited by `.claude/rules/csharp.md` ("Weakening assertions
   or relaxing test expectations to make tests pass").

This is a new finding outside the plan's stated assumptions (the plan assumes the test is RED before
the fix). Per the executor scope-change rule, I stopped before mutating production and report it for
a plan revision by atomic-planner.
