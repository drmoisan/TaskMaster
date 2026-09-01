# Phase 0 — Requirements read ([P0-T2])

Timestamp: 2026-09-01T21-46

## The five requirement sources read

1. `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/spec.md`
2. `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/issue.md`
3. `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/research/2026-09-01T01-05-qfc-alt-chord-over-claim-research.md`
4. `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/call-site-compile-inclusion.md`
5. `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/qfc-mnemonic-inventory.md`

## Transcribed acceptance identifiers (fifteen, none repeated, none omitted)

- AC-1 — `QfcFormKeyHandler.ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)` exists as an
  `internal static bool` member and returns `true` if and only if the handler is non-null, `keyData`
  carries the `Keys.Alt` flag, and `keyData & Keys.KeyCode` equals `Keys.Menu` or `Keys.None`.
- AC-2 — A bare Alt press is still claimed, pinned in both key-data shapes.
- AC-3 — `Keys.Alt | Keys.M` is not claimed, so the `&Move Options` mnemonic reaches `base.ProcessCmdKey`.
- AC-4 — A representative non-mnemonic Alt chord is not claimed: `Keys.Alt | Keys.F4` and
  `Keys.Alt | Keys.Left`.
- AC-5 — A chord that does not carry the `Keys.Alt` flag is not claimed.
- AC-6 — A null handler is not claimed.
- AC-7 — `QfcFormViewer.ProcessCmdKey` delegates its claim decision to `ClaimsAltChord` and contains no
  independent Alt test.
- AC-8 — `QfcFormKeyHandler.IsAltKeyCommand` is unchanged and its four existing tests still pass
  unmodified.
- AC-9 — No file is added to or removed from either QuickFiler csproj.
- AC-10 — The full C# toolchain passes in order: format, analyzers, nullable/type-check, tests.
- AC-11 — Coverage shows no regression on changed lines, and `ClaimsAltChord` meets the `>= 90%`
  new-method floor.
- AC-12 — No test constructs, shows, or derives from a `System.Windows.Forms.Form`, and the new tests use
  no temporary files, `Thread.Sleep`, or `Task.Delay`.
- AC-13 — No new `[ExcludeFromCodeCoverage]` attribute is introduced anywhere in the change.
- AC-14 — The production and test change set is exactly the three named files, call sites 2 through 5 are
  untouched, and the pre-existing unused locals are retained.
- AC-15 — The live-host manual validation of bare Alt, Alt+M and Alt+F4 is recorded at the strength of the
  evidence actually obtained.

Count of acceptance identifiers listed: 15.

## The three in-scope file paths from the spec's "In scope" subsection

1. `QuickFiler/Controllers/QfcFormKeyHandler.cs`
2. `QuickFiler/Viewers/QfcFormViewer.cs`
3. `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`

Count of in-scope file paths listed: 3.

## Corrected prose citation carried forward from the delegation

The plan's reading guide states that `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` is compiled
through `QuickFiler.Test/QuickFiler.Test.csproj` line 151. After the merge of `origin/main`
(`9ca9e99a86428717891a4b54fed70f573a0a2d65`) into this branch, the entry
`<Compile Include="Controllers\QfcFormKeyHandlerTests.cs" />` is on **line 152**, because `origin/main`
added a line above it. Measured directly:

```
$ grep -n "QfcFormKeyHandlerTests.cs" QuickFiler.Test/QuickFiler.Test.csproj
152:    <Compile Include="Controllers\QfcFormKeyHandlerTests.cs" />
```

No acceptance condition in the plan or in `spec.md` asserts line 151, so this is a prose citation only and
is non-gating. The csproj is not edited; AC-9 forbids it. The same off-by-one applies to the spec's
"QuickFiler.Test.csproj:151" citation in its Test Strategy subsection and to the research document's
table row, for the same reason and with the same non-gating disposition.

Command: Read tool invocations against the five requirement-source paths listed above, plus
`grep -n "QfcFormKeyHandlerTests.cs" QuickFiler.Test/QuickFiler.Test.csproj`.

EXIT_CODE: 0

Output Summary: All five requirement sources were read. Fifteen acceptance identifiers AC-1 through AC-15
were transcribed with no identifier repeated and none omitted, and the three in-scope file paths were
transcribed from the spec's "In scope" subsection. One stale prose citation was measured and corrected:
the test file's compile item is on line 152 of `QuickFiler.Test/QuickFiler.Test.csproj`, not line 151. No
acceptance condition depends on that line number.
