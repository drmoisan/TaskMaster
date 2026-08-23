Timestamp: 2026-08-22T13-13
Command: pwsh -NoProfile -Command '$lines = Get-Content -LiteralPath "QuickFiler.Test/QuickFiler.Test.csproj"; $hits = 0..($lines.Count - 1) | Where-Object { $lines[$_] -like "*Form1*" }; $window = $hits | ForEach-Object { ($_ - 3)..($_ + 3) } | Sort-Object -Unique | Where-Object { $_ -ge 0 -and $_ -lt $lines.Count }; $window | ForEach-Object { "{0}: {1}" -f ($_ + 1), $lines[$_].Trim() }'
EXIT_CODE: 0
Output Summary: Windowed context around every `Form1` hit recorded below. Observed region A: lines 161-166 inclusive (the two Form1 compile blocks). Observed region B: lines 179-183 inclusive (the Form1.resx ItemGroup). These observed numbers match the numbers cited in the plan text, but the executor uses these observed, re-derived numbers, not any number cited in this plan, in `spec.md`, in the research document, or in `epic.md`, per the plan's "Re-derive every line number" convention.

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
167: <Compile Include="Helper Classes\ConversationResolverTests.cs" />
168: <Compile Include="Helper Classes\EmailMoveMonitorTests.cs" />
177: <Compile Include="SetupAssemblyInitializer.cs" />
178: </ItemGroup>
179: <ItemGroup>
180: <EmbeddedResource Include="Form1.resx">
181: <DependentUpon>Form1.cs</DependentUpon>
182: </EmbeddedResource>
183: </ItemGroup>
184: <ItemGroup>
```

Region A (Form1/Form1.Designer.cs compile block): observed first line 161, observed last line 166.
Region B (Form1.resx ItemGroup): observed first line 179, observed last line 183.
A plain match-only search would report only lines 161, 164, 165, 180, 181 (the lines directly carrying the literal `Form1`), omitting the closing `</Compile>` tag at 163/166 and both `<ItemGroup>` wrapper tags at 179/183. The windowed form above is therefore mandatory and was used to derive the full extent of both owned regions.

---

[P0-T9] Command: pwsh -NoProfile -Command 'Select-String -LiteralPath "QuickFiler.Test/QuickFiler.Test.csproj" -SimpleMatch "System.Drawing", "System.Drawing.Design", "System.Windows.Forms" | ForEach-Object { "{0}: {1}" -f $_.LineNumber, $_.Line.Trim() }'
EXIT_CODE: 0
Output Summary: Exactly three retained reference entries located, matching the plan-cited line numbers.

```
365: <Reference Include="System.Drawing" />
366: <Reference Include="System.Drawing.Design" />
420: <Reference Include="System.Windows.Forms" />
```
