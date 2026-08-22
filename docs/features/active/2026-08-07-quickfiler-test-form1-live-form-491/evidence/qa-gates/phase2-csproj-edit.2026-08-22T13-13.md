Timestamp: 2026-08-22T13-13
Command: pwsh -NoProfile -Command '$lines = Get-Content -LiteralPath "QuickFiler.Test/QuickFiler.Test.csproj"; $hits = 0..($lines.Count - 1) | Where-Object { $lines[$_] -like "*Form1*" }; $window = $hits | ForEach-Object { ($_ - 3)..($_ + 3) } | Sort-Object -Unique | Where-Object { $_ -ge 0 -and $_ -lt $lines.Count }; $window | ForEach-Object { "{0}: {1}" -f ($_ + 1), $lines[$_].Trim() }'
EXIT_CODE: 0
Output Summary: Re-derived region A (Form1/Form1.Designer.cs compile block): lines 161-166, unchanged from P0-T8. Re-derived region B (Form1.resx ItemGroup): lines 180-184, shifted by exactly +1 from the P0-T8 observation of 179-183. This +1 shift is fully explained by the one line inserted at P1-T2 (`<Compile Include="NoLiveFormInTestAssemblyTests.cs" />` at line 167), which lies above region B and pushes every subsequent line down by one. No other discrepancy exists. The executor edits by these re-derived numbers (161-166 for region A, 180-184 for region B), not by any number cited elsewhere in this plan.

Raw windowed output:
```
158: <Compile Include="Controllers\QfcQueueTests.cs" />
159: <Compile Include="TestSupport\WinFormsPumpHost.cs" />
160: <Compile Include="TestSupport\WinFormsPumpHostTests.cs" />
161: <Compile Include="Form1.cs">
162: <SubType>Form</SubType>
163: </Compile>
164: <Compile Include="Form1.Designer.cs">
165: <DependentUpon>Form1.cs</DependentUpon>
166: </Compile>
167: <Compile Include="NoLiveFormInTestAssemblyTests.cs" />
168: <Compile Include="Helper Classes\ConversationResolverTests.cs" />
178: <Compile Include="SetupAssemblyInitializer.cs" />
179: </ItemGroup>
180: <ItemGroup>
181: <EmbeddedResource Include="Form1.resx">
182: <DependentUpon>Form1.cs</DependentUpon>
183: </EmbeddedResource>
184: </ItemGroup>
185: <ItemGroup>
```

---

[P2-T2] `git rm QuickFiler.Test/Form1.cs QuickFiler.Test/Form1.Designer.cs QuickFiler.Test/Form1.resx` — EXIT_CODE: 0. All three paths staged as deletions (`D`); none exists on disk afterward.

[P2-T3] Deleted the six-line Form1/Form1.Designer.cs compile block at the re-derived lines 161-166. `Select-String -SimpleMatch "Form1"` count after: 2 (the two survivors are the `Form1.resx` embedded-resource tag and its `DependentUpon` child, removed next by P2-T4).

[P2-T4] Deleted the entire five-line Form1.resx `<ItemGroup>` at the re-derived lines 180-184. `Select-String -SimpleMatch "Form1"` count after: 0. Visual inspection of the surrounding lines confirms no dangling empty `<ItemGroup>` remains at that position; the compile-items `</ItemGroup>` is immediately followed by the `<None Include="app.config" />` item group.

[P2-T5] Command: `pwsh -NoProfile -Command '$b = [System.IO.File]::ReadAllBytes(...); ... "LF=$lf CRLF=$crlf"'` — Output: `LF=473 CRLF=473`. CRLF count equals LF count, proving no line was converted to a bare LF.

[P2-T6] Re-ran the P0-T9 command. Exactly three reference entries located:
```
355: <Reference Include="System.Drawing" />
356: <Reference Include="System.Drawing.Design" />
410: <Reference Include="System.Windows.Forms" />
```
Text is byte-identical to the P0-T9 record (`<Reference Include="System.Drawing" />`, `<Reference Include="System.Drawing.Design" />`, `<Reference Include="System.Windows.Forms" />`). Line numbers shifted down by exactly 10 (365→355, 366→356, 420→410), consistent with the net -10 line delta from Phase 1/Phase 2 edits (+1 line at P1-T2, -11 lines at P2-T3/P2-T4).

[P2-T7] `git diff --numstat -- QuickFiler.Test/QuickFiler.Test.csproj`:
```
1	11	QuickFiler.Test/QuickFiler.Test.csproj
```
`git diff -U0 -- QuickFiler.Test/QuickFiler.Test.csproj` shows exactly two hunks: one hunk at the former region A position adding the `NoLiveFormInTestAssemblyTests.cs` compile entry and removing the six `Form1`/`Form1.Designer.cs` lines, and one hunk at the former region B position removing the five-line `Form1.resx` `<ItemGroup>`. Added-line count: 1. Deleted-line count: 11. No hunk touches the `Controllers` compile item group (closes at line 178, per P0-T8/P2-T1) or any `Reference` item group (lines 355-356, 410, confirmed unmodified in P2-T6).
