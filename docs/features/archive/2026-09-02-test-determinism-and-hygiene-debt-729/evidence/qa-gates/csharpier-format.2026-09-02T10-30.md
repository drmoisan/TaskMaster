# Scope-locked CSharpier format pass (P6-T1)

Timestamp: 2026-09-02T23-33

Command: `dotnet tool run csharpier format 'TaskMaster/AppGlobals/NonBlockingDelay.cs' 'TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs' 'TaskMaster.Test/packages.config' 'UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs' 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs' 'UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs' 'SVGControl.Test/NoLiveFormInTestAssemblyTests.cs'`

EXIT_CODE: 0

RewrittenFileCount: 0

The `RewrittenFileCount:` above is the final-pass value. It is derived from the SHA-256
before/after comparison below, not from the console line `Formatted 7 files in ...`, which is a
processed-file count and is 7 on every pass whether or not anything was rewritten. A repo-wide
`dotnet tool run csharpier format .` was not run; D4 prohibits it.

## Pass 1 — fourteen hashes

| File | SHA-256 before | SHA-256 after | Rewritten |
|---|---|---|---|
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` | `26DC25D345278E3CA2EF2BD385FBEBFAF9A9B62B81001102B204A9444665FDDD` | `56DF7ACD6DCB92B0252B1C5BEA328C6ADE952A9F8CAAF258054EA6344F24FA75` | yes |
| `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` | `119D97E6285700B45143CEA53A0EA3FF3C9A018674C7116E0BEE7D41991154F1` | `9D6243FE01D773F56E2CE5A14EEBBCF273C20363C53F5C00FE67DF9ED3F6B303` | yes |
| `TaskMaster.Test/packages.config` | `438831BCF58C6BA9381D418B768EF7B1C8FF0043DD53E5683C5994C901358975` | `438831BCF58C6BA9381D418B768EF7B1C8FF0043DD53E5683C5994C901358975` | no |
| `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` | `80E853FADC239DC23FA24F5E240C458993F7C22F3585C9F24EC9BF1DCB0BC91D` | `77D4D4125AC00CAE3B6C0C49AD8FF47416E3D0B71F4BD920A7A982056C4ECEC6` | yes |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | `2FD05CDAB13DC2305D759D4B6A4D9153647DF95557D2160A5A5F0A204DCDF6D9` | `2FD05CDAB13DC2305D759D4B6A4D9153647DF95557D2160A5A5F0A204DCDF6D9` | no |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | `73DA31B89F3A5695CDB7F18D0E92CC1FA8754308551FAC18AF132F044BEE7881` | `73DA31B89F3A5695CDB7F18D0E92CC1FA8754308551FAC18AF132F044BEE7881` | no |
| `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` | `CF177FD7036D5279899D54A5988923CDABE6E4BE83BB8A7C0670F2144AECFDEB` | `CDAB67CD85B312A0E9EBF5E3D926C293F5A23E62DF8CEA29EE312D73A027F608` | yes |

Pass 1 `EXIT_CODE: 0`, `RewrittenFileCount: 4`.

Nature of the four pass-1 rewrites, established by `git diff --stat` over the same four paths
immediately after the pass:

- Three of the four — `TaskMaster/AppGlobals/NonBlockingDelay.cs`,
  `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`, and
  `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` — produced no entry in `git diff --stat`.
  Their content is unchanged; only the on-disk line endings were normalized to CRLF, which the
  repository's line-ending normalization collapses in the index.
- One — `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` — carried a real formatting change
  of 3 insertions and 1 deletion: CSharpier wrapped a `.BeFalse("...")` call whose single-line
  form exceeded the print width onto three lines. Formatter output wins over the plan's Block B
  line shape, per the repository's C# formatting rule.

Because pass 1 rewrote files, the mandatory toolchain loop restarts at step 1, which is what the
pass below performs.

## Pass 2 (final) — fourteen hashes

| File | SHA-256 before | SHA-256 after | Rewritten |
|---|---|---|---|
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` | `56DF7ACD6DCB92B0252B1C5BEA328C6ADE952A9F8CAAF258054EA6344F24FA75` | `56DF7ACD6DCB92B0252B1C5BEA328C6ADE952A9F8CAAF258054EA6344F24FA75` | no |
| `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` | `9D6243FE01D773F56E2CE5A14EEBBCF273C20363C53F5C00FE67DF9ED3F6B303` | `9D6243FE01D773F56E2CE5A14EEBBCF273C20363C53F5C00FE67DF9ED3F6B303` | no |
| `TaskMaster.Test/packages.config` | `438831BCF58C6BA9381D418B768EF7B1C8FF0043DD53E5683C5994C901358975` | `438831BCF58C6BA9381D418B768EF7B1C8FF0043DD53E5683C5994C901358975` | no |
| `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` | `77D4D4125AC00CAE3B6C0C49AD8FF47416E3D0B71F4BD920A7A982056C4ECEC6` | `77D4D4125AC00CAE3B6C0C49AD8FF47416E3D0B71F4BD920A7A982056C4ECEC6` | no |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | `2FD05CDAB13DC2305D759D4B6A4D9153647DF95557D2160A5A5F0A204DCDF6D9` | `2FD05CDAB13DC2305D759D4B6A4D9153647DF95557D2160A5A5F0A204DCDF6D9` | no |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | `73DA31B89F3A5695CDB7F18D0E92CC1FA8754308551FAC18AF132F044BEE7881` | `73DA31B89F3A5695CDB7F18D0E92CC1FA8754308551FAC18AF132F044BEE7881` | no |
| `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` | `CDAB67CD85B312A0E9EBF5E3D926C293F5A23E62DF8CEA29EE312D73A027F608` | `CDAB67CD85B312A0E9EBF5E3D926C293F5A23E62DF8CEA29EE312D73A027F608` | no |

Output Summary: The formatter reached a fixpoint on pass 2. `EXIT_CODE: 0` and
`RewrittenFileCount: 0`, so the remaining toolchain steps P6-T2 through P6-T5 run against a
formatter-stable tree. The one real rewrite that pass 1 applied
(`TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`) lands on a file that an earlier phase
commit already committed, so it is staged and committed by P6-T9 per D15.
