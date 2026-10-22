# P0-T7 project anchors (issue #973; read-only)

Timestamp: 2026-10-03T10-43
Command: Grep tool `id="System.Linq.Async"` -n over the five packages.config; Grep `Include="System.Linq.Async,` -n -A 2 over the five csproj; Grep `System.Linq.AsyncEnumerable` over glob */*.csproj and over glob */packages.config; Grep `extern alias` over glob */**/*.cs
EXIT_CODE: 0
Output Summary: System.Linq.Async package lines at 97, 47, 20, 43, 91; Reference elements start at 398, 183, 85, 244, 896, each followed by the lib\net48 HintPath line and `</Reference>`; no csproj or packages.config names System.Linq.AsyncEnumerable; no .cs file declares extern alias. No ANCHOR-DRIFT.

## packages.config (`id="System.Linq.Async"`)

- UtilitiesCS/packages.config:97 `<package id="System.Linq.Async" version="7.0.1" targetFramework="net481" />`
- QuickFiler/packages.config:47 (same text)
- ToDoModel/packages.config:20 (same text)
- TaskMaster/packages.config:43 (same text)
- UtilitiesCS.Test/packages.config:91 (same text)

## csproj (`Include="System.Linq.Async,` -A 2)

- UtilitiesCS/UtilitiesCS.csproj:398-400
- QuickFiler/QuickFiler.csproj:183-185
- ToDoModel/ToDoModel.csproj:85-87
- TaskMaster/TaskMaster.csproj:244-246
- UtilitiesCS.Test/UtilitiesCS.Test.csproj:896-898

Each element reads:
    <Reference Include="System.Linq.Async, Version=7.0.0.0, Culture=neutral, PublicKeyToken=94bc3704cddfc263, processorArchitecture=MSIL">
      <HintPath>..\packages\System.Linq.Async.7.0.1\lib\net48\System.Linq.Async.dll</HintPath>
    </Reference>

## Negative checks

- `System.Linq.AsyncEnumerable` over */*.csproj: no match
- `System.Linq.AsyncEnumerable` over */packages.config: no match
- `extern alias` over */**/*.cs: no match
