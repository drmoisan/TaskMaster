# Remediation cycle 1, P1-T2: Azure.Core reference scan over UtilitiesCS\bin\Debug, positive control (AC17 observation iii)

Timestamp: 2026-10-06T20-31
Command: pwsh -NoProfile -Command '$null = [System.Reflection.Assembly]::Load("System.Reflection.Metadata"); $d = "<execution-worktree-root>/UtilitiesCS/bin/Debug"; $files = @(Get-ChildItem -LiteralPath $d -File | Where-Object { $_.Extension -eq ".dll" -or $_.Extension -eq ".exe" } | Sort-Object Name); $hits = 0; $managed = 0; $skipped = 0; foreach ($f in $files) { try { $pe = [System.Reflection.PortableExecutable.PEReader]::new([System.IO.File]::OpenRead($f.FullName)); try { if (-not $pe.HasMetadata) { $skipped++; "SKIP " + $f.Name + " no-metadata"; continue }; $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe); $n = 0; foreach ($h in $md.AssemblyReferences) { $r = $md.GetAssemblyReference($h); $n++; if ($md.GetString($r.Name) -eq "Azure.Core") { $hits++; "REF " + $f.Name + " Azure.Core=" + $r.Version.ToString() } }; $managed++; "ASM " + $f.Name + " REFS=" + $n } finally { $pe.Dispose() } } catch { $skipped++; "SKIP " + $f.Name + " " + $_.Exception.GetType().Name } }; "FILES=" + $files.Count + " MANAGED=" + $managed + " SKIPPED=" + $skipped + " AZURECORE_REFERRERS=" + $hits'
EXIT_CODE: 0

KIOTA-REQUESTS-AZURE-CORE: 1.50.0.0

Printed lines (verbatim):
ASM AngleSharp.dll REFS=7
ASM Apache.Arrow.dll REFS=11
ASM Apache.Arrow.Scalars.dll REFS=7
ASM Azure.Core.dll REFS=21
ASM C.math.dll REFS=1
ASM Deedle.dll REFS=10
ASM ExCSS.dll REFS=3
ASM Fizzler.dll REFS=1
ASM FluentAssertions.dll REFS=7
ASM FSharp.Core.dll REFS=2
ASM Generic.Math.dll REFS=2
ASM log4net.dll REFS=7
ASM log4net.Ext.Json.dll REFS=3
ASM Microsoft.Bcl.AsyncInterfaces.dll REFS=2
ASM Microsoft.Bcl.Cryptography.dll REFS=9
ASM Microsoft.Bcl.HashCode.dll REFS=2
ASM Microsoft.Bcl.Memory.dll REFS=5
ASM Microsoft.Bcl.Numerics.dll REFS=1
ASM Microsoft.Bcl.TimeProvider.dll REFS=4
ASM Microsoft.Data.Analysis.dll REFS=6
ASM Microsoft.Extensions.Configuration.Abstractions.dll REFS=5
ASM Microsoft.Extensions.DependencyInjection.Abstractions.dll REFS=5
ASM Microsoft.Extensions.Diagnostics.Abstractions.dll REFS=5
ASM Microsoft.Extensions.FileProviders.Abstractions.dll REFS=3
ASM Microsoft.Extensions.Hosting.Abstractions.dll REFS=9
ASM Microsoft.Extensions.Logging.Abstractions.dll REFS=5
ASM Microsoft.Extensions.Options.dll REFS=6
ASM Microsoft.Extensions.Primitives.dll REFS=5
REF Microsoft.Graph.Core.dll Azure.Core=1.50.0.0
ASM Microsoft.Graph.Core.dll REFS=18
REF Microsoft.Graph.dll Azure.Core=1.50.0.0
ASM Microsoft.Graph.dll REFS=9
ASM Microsoft.Identity.Client.dll REFS=14
ASM Microsoft.Identity.Client.Extensions.Msal.dll REFS=6
ASM Microsoft.IdentityModel.Abstractions.dll REFS=1
ASM Microsoft.IdentityModel.JsonWebTokens.dll REFS=11
ASM Microsoft.IdentityModel.Logging.dll REFS=4
ASM Microsoft.IdentityModel.Protocols.dll REFS=8
ASM Microsoft.IdentityModel.Protocols.OpenIdConnect.dll REFS=12
ASM Microsoft.IdentityModel.Tokens.dll REFS=15
ASM Microsoft.IdentityModel.Validators.dll REFS=11
ASM Microsoft.IO.RecyclableMemoryStream.dll REFS=2
ASM Microsoft.Kiota.Abstractions.dll REFS=4
REF Microsoft.Kiota.Authentication.Azure.dll Azure.Core=1.50.0.0
ASM Microsoft.Kiota.Authentication.Azure.dll REFS=5
ASM Microsoft.Kiota.Http.HttpClientLibrary.dll REFS=10
ASM Microsoft.Kiota.Serialization.Form.dll REFS=3
ASM Microsoft.Kiota.Serialization.Json.dll REFS=6
ASM Microsoft.Kiota.Serialization.Multipart.dll REFS=3
ASM Microsoft.Kiota.Serialization.Text.dll REFS=3
ASM Microsoft.ML.Core.dll REFS=5
ASM Microsoft.ML.CpuMath.dll REFS=3
ASM Microsoft.ML.Data.dll REFS=12
ASM Microsoft.ML.DataView.dll REFS=3
ASM Microsoft.ML.KMeansClustering.dll REFS=6
ASM Microsoft.ML.PCA.dll REFS=6
ASM Microsoft.ML.StandardTrainers.dll REFS=8
ASM Microsoft.ML.Transforms.dll REFS=10
ASM Microsoft.Office.Tools.Common.v4.0.Utilities.dll REFS=3
ASM Microsoft.Office.Tools.Outlook.v4.0.Utilities.dll REFS=4
ASM Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll REFS=6
ASM Microsoft.Web.WebView2.Core.dll REFS=4
ASM Microsoft.Web.WebView2.WinForms.dll REFS=5
ASM Microsoft.Web.WebView2.Wpf.dll REFS=9
ASM Mono.Cecil.dll REFS=3
ASM Mono.Cecil.Mdb.dll REFS=4
ASM Mono.Cecil.Pdb.dll REFS=3
ASM Mono.Cecil.Rocks.dll REFS=3
ASM Mono.Reflection.dll REFS=2
ASM Newtonsoft.Json.Bson.dll REFS=6
ASM Newtonsoft.Json.dll REFS=8
ASM ObjectListView.dll REFS=6
ASM Std.UriTemplate.dll REFS=1
ASM Svg.dll REFS=9
ASM SVGControl.dll REFS=10
ASM System.Buffers.dll REFS=1
ASM System.ClientModel.dll REFS=15
ASM System.CodeDom.dll REFS=2
ASM System.Collections.Immutable.dll REFS=6
ASM System.Diagnostics.DiagnosticSource.dll REFS=6
ASM System.Drawing.Common.dll REFS=2
ASM System.Formats.Asn1.dll REFS=6
ASM System.IdentityModel.Tokens.Jwt.dll REFS=11
ASM System.Interactive.Async.dll REFS=6
ASM System.Interactive.dll REFS=3
ASM System.IO.FileSystem.AccessControl.dll REFS=1
ASM System.IO.Pipelines.dll REFS=4
ASM System.Linq.Async.dll REFS=7
ASM System.Linq.AsyncEnumerable.dll REFS=7
ASM System.Memory.Data.dll REFS=4
ASM System.Memory.dll REFS=5
ASM System.Net.Http.WinHttpHandler.dll REFS=6
ASM System.Net.ServerSentEvents.dll REFS=6
ASM System.Numerics.Tensors.dll REFS=5
ASM System.Numerics.Vectors.dll REFS=2
ASM System.Reactive.Async.dll REFS=4
ASM System.Reactive.dll REFS=7
ASM System.Runtime.CompilerServices.Unsafe.dll REFS=1
ASM System.Security.AccessControl.dll REFS=1
ASM System.Security.Cryptography.ProtectedData.dll REFS=2
ASM System.Security.Principal.Windows.dll REFS=1
ASM System.Text.Encoding.CodePages.dll REFS=5
ASM System.Text.Encodings.Web.dll REFS=6
ASM System.Text.Json.dll REFS=12
ASM System.Threading.Channels.dll REFS=3
ASM System.Threading.Tasks.Dataflow.dll REFS=3
ASM System.Threading.Tasks.Extensions.dll REFS=2
ASM Tesseract.dll REFS=3
ASM UtilitiesCS.dll REFS=35
FILES=106 MANAGED=106 SKIPPED=0 AZURECORE_REFERRERS=3

Output Summary:
- pwsh exit 0; summary line FILES=106 MANAGED=106 SKIPPED=0 AZURECORE_REFERRERS=3 (FILES equals MANAGED plus SKIPPED).
- Three REF lines, equal to AZURECORE_REFERRERS: Microsoft.Graph.Core.dll, Microsoft.Graph.dll and Microsoft.Kiota.Authentication.Azure.dll, each referencing Azure.Core 1.50.0.0.
- `REF Microsoft.Kiota.Authentication.Azure.dll Azure.Core=1.50.0.0` present; KIOTA-REQUESTS-AZURE-CORE: 1.50.0.0, which as a System.Version is at or below 1.63.0.0 and inside the corrected range 0.0.0.0-1.63.0.0.
- The scan detects an Azure.Core reference where one exists, so the P1-T1 zero over TaskMaster\bin\Debug is a measured absence. No POSITIVE-CONTROL-FAILED, REQUEST-ABOVE-RANGE or REFSCAN-ERROR.
- UtilitiesCS.dll itself (REFS=35) carries no Azure.Core reference, consistent with remediation-inputs B-1.
