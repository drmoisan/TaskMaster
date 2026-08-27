# AC-8 Check-Off (P5-T8)

Timestamp: 2026-08-27T12-03
Task: [P5-T8]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-8 ("Every owned and new file is at or under 500 lines", with the two
`<Compile Include>` entries added in the `Qfc*` neighbourhood immediately after the
`QfcItemController.TestSupport.cs` entry) is verified against four measured line counts all at or
below the ceiling and against the recorded adjacency of the two new entries, and is checked off in
`spec.md`. `PairsN: 8`, `PairsNMinus1: 7`, so exactly one further checkbox changed state.

PairsN: 8
PairsNMinus1: 7

`pairs(8) - pairs(7) == 1`. `pairs(7)` is the value recorded by `P5-T7` in
`<FEATURE>/evidence/other/ac-checkoff-ac7.2026-08-27T12-01.md`.

## Cited artifacts, resolved per § Conventions

| Stem | Resolved filename |
| --- | --- |
| `file-size-audit` | `<FEATURE>/evidence/qa-gates/file-size-audit.2026-08-27T11-33.md` |
| `csproj-compile-entries` | `<FEATURE>/evidence/other/csproj-compile-entries.2026-08-27T10-35.md` |

## Clause 1 — four measured counts at or below the ceiling

`file-size-audit` records, measured after the final formatter pass:

| Path | Measured lines | At or below 500 |
| --- | --- | --- |
| `QfcItemController.TestSupport.cs` | 440 | yes |
| `QfcItemController.InitializationTests.Part2.cs` | 393 | yes |
| `QfcItemController.UiThreadDispatcherFixture.cs` | 278 | yes |
| `QfcItemController.UiThreadDispatcherFixtureTests.cs` | 346 | yes |

These are the exact four files AC-8 names. All four are at or below the ceiling; the tightest is
`QfcItemController.TestSupport.cs` with 60 lines of headroom.

Per § Decisions Record D2 these are fresh measurements, not restatements of the research §8
projections. That distinction is load-bearing here: `QfcItemController.TestSupport.cs` measured
**489** lines at `BASE_SHA`, not the 365 research recorded against `main`, because sibling epic
features have since added shared arrange helpers to its tail. Had the projection been restated instead
of measured, the audit would have reported 135 lines of headroom where only 11 existed. The change is a
net deletion in both owned files, so it relieved rather than consumed headroom.

## Clause 2 — the two `<Compile Include>` entries in the `Qfc*` neighbourhood

`csproj-compile-entries` records the anchor at `L = 157` with exactly one match for
`QfcItemController.TestSupport.cs`, and:

| Line | Text |
| --- | --- |
| 157 (`L`) | `<Compile Include="Controllers\QfcItemController.TestSupport.cs" />` |
| 158 (`L+1`) | `<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixture.cs" />` |
| 159 (`L+2`) | `<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />` |

The two entries are immediately after the `QfcItemController.TestSupport.cs` entry, in that order,
inside the grouped `QfcItemController.*` block — the `Qfc*` neighbourhood AC-8 requires. The artifact
also records `git diff --stat` for that path as `2 insertions(+)` with zero deletions, so nothing else
in the project file changed, and that the file's UTF-8 BOM and CRLF line terminators were preserved.

## Result

`- [ ] **AC-8 …` changed to `- [x] **AC-8 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the checkbox changed.
