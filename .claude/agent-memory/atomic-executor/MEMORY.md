# Atomic Executor Memory Index

- [Project Build/Test Env](project_build_test_env.md) — git-bash toolchain quirks: MSBuild dash-switches, MSYS_NO_PATHCONV for vstest, csharpier v1 syntax, forced-nullable Rebuild + Debug-restore, legacy csproj Compile includes, IVT for Moq, C# 7.3 in QuickFiler.Test
- [PowerShell new files need UTF-8 BOM](powershell-bom-required.md) — PSScriptAnalyzer enforces PSUseBOMForUnicodeEncodedFile; prepend BOM after Write or restart the format loop
- [SecurityCodeScan incompatible with Roslyn 5.6](project_securitycodescan_roslyn56_incompat.md) — SecurityCodeScan.VS2019 5.6.7 throws CS8032/YamlDotNet under VS18 Roslyn 5.6, breaking TreatWarningsAsErrors gate; other 5 analyzers OK; Meziantou/Roslynator need roslyn-version subfolders
- [vstest /InIsolation + FilePathHelper serialization](project_vstest_isolation_and_filepathhelper_serialization.md) — Moq test assemblies need vstest /InIsolation (else STTE 4.2.0.1 Setup FileNotFound); FilePathHelper.FilePath is "" default but null after JSON deserialize of empty helper
