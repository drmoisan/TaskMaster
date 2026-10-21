# Static facts underpinning the verified mechanism

Timestamp: 2026-09-13T09-17
Command: see the four command blocks below
EXIT_CODE: 0
Output Summary: Every structural link in the verified mechanism was measured independently at head c9590a8b7. Deedle references FSharp.Core 4.5.0.0 and netstandard 2.0.0.0; the deployed FSharp.Core is 11.0.0.0 and references netstandard 2.1.0.0; netstandard 2.1.0.0 is absent from bin\Debug and from the GAC; no config file in the repository mentions netstandard; the repository has exactly two first-party AssemblyResolve handlers.

## 1. Deployed assembly identities

Command: PowerShell `[System.Reflection.AssemblyName]::GetAssemblyName` over `QuickFiler.Test\bin\Debug`.

```
FSharp.Core.dll => FSharp.Core, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Deedle.dll      => Deedle, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
```

## 2. Assembly reference tables

Command: PowerShell `System.Reflection.Metadata.MetadataReader.AssemblyReferences` over the same two files.

```
=== Deedle.dll
FSharp.Core 4.5.0.0
netstandard 2.0.0.0
System.Globalization 4.0.11.0
System.Reflection 4.1.2.0
System.Reflection.Emit 4.0.0.0
System.Reflection.Emit.ILGeneration 4.0.0.0
System.Reflection.Emit.Lightweight 4.0.0.0
System.Runtime 4.1.2.0
System.Runtime.Extensions 4.1.2.0
System.Threading 4.0.11.0

=== FSharp.Core.dll
netstandard 2.1.0.0
System.Runtime.Numerics 4.0.1.0
```

The netstandard 2.1.0.0 requirement enters the closure through FSharp.Core 11.0.0.0, not through Deedle,
which asks only for netstandard 2.0.0.0. The redirect that produces FSharp.Core 11.0.0.0 is therefore
the proximate trigger.

## 3. Availability of netstandard

```
QuickFiler.Test/bin/Debug/netstandard.dll  => False
GAC_MSIL netstandard directories           => v4.0_2.0.0.0__cc7b13ffcd2ddd51
```

netstandard 2.0.0.0 and the requested netstandard 2.1.0.0 share the public key token
`cc7b13ffcd2ddd51`, which is why a handler matching on simple name plus public key token satisfies the
bind while the default binder cannot.

## 4. Binding redirects and handlers

Command: repository-wide search of `**/*.config` for `netstandard`.
SearchScope: every `*.config` file tracked in the repository
SearchPatterns: `netstandard`
SearchResult: none

Command: repository-wide search of `**/*.cs` for `AssemblyResolve`.
SearchResult:
```
UtilitiesCS.Test/TestAssemblyInitializer.cs   (handler, installed from [AssemblyInitialize])
SVGControl/SvgRenderer.cs                     (comment plus the SvgAssemblyResolver.Install call)
SVGControl/SvgAssemblyProbe.cs                (probe helper used by the handler)
SVGControl/SvgAssemblyResolver.cs             (handler, installed by SvgAssemblyResolver.Install)
SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs (tests of the probe helper)
```

`SVGControl/SvgRenderer.cs` lines 25 to 28 are the static constructor that calls
`SvgAssemblyResolver.Install()`, which is what makes the SVGControl handler lazy.

FSharp.Core redirects: `QuickFiler.Test/app.config` line 46 and `UtilitiesCS.Test/app.config` line 38,
both `oldVersion="0.0.0.0-11.0.0.0" newVersion="11.0.0.0"`.
