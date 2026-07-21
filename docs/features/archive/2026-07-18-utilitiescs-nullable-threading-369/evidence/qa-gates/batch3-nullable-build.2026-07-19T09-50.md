# Batch 3 — Pragma-Only Nullable Build Verification

- Timestamp: 2026-07-19T09-50
- Task: [P3-T4]
- Literal plan command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
- Executed equivalent (genuine recompile of the changed project): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true /m`
- EXIT_CODE: 1 (pre-existing first-party TWAE noise only; zero CS86xx)

## Opted-in Batch 3 files (2)

- UtilitiesCS/Threading/CurrentStoreContext.cs
- UtilitiesCS/Threading/LockupStallDecider.cs

## Output Summary

- **CS86xx for the 2 opted-in Batch 3 files: 0.** CS86xx count anywhere: 0.
- CurrentStoreContext: `volatile string _current` -> `volatile string?` (the documented "null = no context" contract); `Current`, `Normalize` param/return, `Begin` param, and `Scope._previous`/ctor param annotated `string?`. The `volatile` keyword and single-writer/single-reader ordering are byte-unchanged.
- LockupStallDecider / `LockupAttribution`: ctor param `storeIdentity` and property `StoreIdentity` annotated `string?` (genuinely null when no per-store scope open — the settled contract Batch 7 consumes). `IsStallConfirmed` boundary logic unchanged.
- Non-zero EXIT_CODE is the pre-existing first-party TWAE noise only (CS0618 x14 + CS0168 x2 in `UtilitiesCS.csproj`, unchanged from baseline). No new diagnostics elsewhere; vendored skipped. `/p:Nullable=enable` NOT passed.
