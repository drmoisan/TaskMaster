# Remediation cycle 1, P1-T1: Azure.Core reference scan over TaskMaster\bin\Debug (AC17 observation ii)

Timestamp: 2026-10-06T20-31
Command: pwsh -NoProfile -Command '$null = [System.Reflection.Assembly]::Load("System.Reflection.Metadata"); $d = "<execution-worktree-root>/TaskMaster/bin/Debug"; $files = @(Get-ChildItem -LiteralPath $d -File | Where-Object { $_.Extension -eq ".dll" -or $_.Extension -eq ".exe" } | Sort-Object Name); $hits = 0; $managed = 0; $skipped = 0; foreach ($f in $files) { try { $pe = [System.Reflection.PortableExecutable.PEReader]::new([System.IO.File]::OpenRead($f.FullName)); try { if (-not $pe.HasMetadata) { $skipped++; "SKIP " + $f.Name + " no-metadata"; continue }; $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe); $n = 0; foreach ($h in $md.AssemblyReferences) { $r = $md.GetAssemblyReference($h); $n++; if ($md.GetString($r.Name) -eq "Azure.Core") { $hits++; "REF " + $f.Name + " Azure.Core=" + $r.Version.ToString() } }; $managed++; "ASM " + $f.Name + " REFS=" + $n } finally { $pe.Dispose() } } catch { $skipped++; "SKIP " + $f.Name + " " + $_.Exception.GetType().Name } }; "FILES=" + $files.Count + " MANAGED=" + $managed + " SKIPPED=" + $skipped + " AZURECORE_REFERRERS=" + $hits'
EXIT_CODE: 0

Printed lines (verbatim):
ASM AngleSharp.dll REFS=7
ASM Apache.Arrow.dll REFS=11
ASM Apache.Arrow.Scalars.dll REFS=7
ASM C.math.dll REFS=1
ASM Deedle.dll REFS=10
ASM ExCSS.dll REFS=3
ASM FSharp.Core.dll REFS=2
ASM Generic.Math.dll REFS=2
ASM log4net.dll REFS=7
ASM log4net.Ext.Json.dll REFS=3
ASM Microsoft.Bcl.AsyncInterfaces.dll REFS=2
ASM Microsoft.Bcl.Memory.dll REFS=5
ASM Microsoft.Bcl.TimeProvider.dll REFS=4
ASM Microsoft.Data.Analysis.dll REFS=6
ASM Microsoft.IO.RecyclableMemoryStream.dll REFS=2
ASM Microsoft.ML.DataView.dll REFS=3
ASM Microsoft.Office.Tools.Common.v4.0.Utilities.dll REFS=3
ASM Microsoft.Office.Tools.Outlook.v4.0.Utilities.dll REFS=4
ASM Microsoft.Web.WebView2.Core.dll REFS=4
ASM Microsoft.Web.WebView2.WinForms.dll REFS=5
ASM Mono.Reflection.dll REFS=2
ASM Newtonsoft.Json.dll REFS=8
ASM ObjectListView.dll REFS=6
ASM QuickFiler.dll REFS=23
ASM Svg.dll REFS=9
ASM SVGControl.dll REFS=10
ASM System.Buffers.dll REFS=1
ASM System.Collections.Immutable.dll REFS=6
ASM System.Diagnostics.DiagnosticSource.dll REFS=6
ASM System.Interactive.Async.dll REFS=6
ASM System.Interactive.dll REFS=3
ASM System.Linq.Async.dll REFS=7
ASM System.Linq.AsyncEnumerable.dll REFS=7
ASM System.Memory.dll REFS=5
ASM System.Numerics.Vectors.dll REFS=2
ASM System.Reactive.Async.dll REFS=2
ASM System.Reactive.dll REFS=7
ASM System.Runtime.CompilerServices.Unsafe.dll REFS=1
ASM System.Text.Encoding.CodePages.dll REFS=5
ASM System.Threading.Tasks.Dataflow.dll REFS=3
ASM System.Threading.Tasks.Extensions.dll REFS=2
ASM Tags.dll REFS=8
ASM TaskMaster.dll REFS=30
ASM TaskTree.dll REFS=9
ASM TaskVisualization.dll REFS=13
ASM Tesseract.dll REFS=3
ASM ToDoModel.dll REFS=18
ASM UtilitiesCS.dll REFS=35
FILES=48 MANAGED=48 SKIPPED=0 AZURECORE_REFERRERS=0

Output Summary:
- pwsh exit 0; summary line present: FILES=48 MANAGED=48 SKIPPED=0 AZURECORE_REFERRERS=0 (FILES equals MANAGED plus SKIPPED; FILES at least 40).
- No REF line: no assembly deployed directly in TaskMaster\bin\Debug references Azure.Core.
- The add-in's own assemblies were scanned: ASM UtilitiesCS.dll REFS=35 and ASM TaskMaster.dll REFS=30.
- No SKIP line. No AZURE-CORE-REFERRER-FOUND and no REFSCAN-ERROR.
- Detection power of this scan is shown by the P1-T2 positive control over UtilitiesCS\bin\Debug.
