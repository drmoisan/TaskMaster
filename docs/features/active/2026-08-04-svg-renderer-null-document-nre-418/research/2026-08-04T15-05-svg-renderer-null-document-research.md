# Research — SvgRenderer Null-Document NullReferenceException (Issue #418)

- Issue: #418
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/418
- Feature folder: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/`
- Timestamp: 2026-08-04T15-05
- Work mode: minor-audit
- Satisfies: AC-7 (underlying failure identified in writing)
- Author: task-researcher

## Evidence Classification Legend

Every finding below is tagged:

- **[VERIFIED]** — read directly from a repository file, package metadata, or an authoritative upstream source in this session.
- **[GIVEN]** — supplied by the orchestrator from `System.Reflection.Metadata.PEReader` inspection; treated as input, not re-derived.
- **[INFERRED]** — a conclusion drawn from verified facts plus documented CLR/host behavior. The reasoning and its limits are stated.
- **[UNVERIFIED]** — a claim that could not be established with the tools available in this session. Named explicitly so it is not mistaken for evidence.

No process was launched, no build was run, and no debugger or fusion log was captured during this research. Tool access was limited to file read, content search, and web fetch.

---

## 1. Current State Analysis

### 1.1 The defect surface

`SVGControl/SvgRenderer.cs:320-331` **[VERIFIED]**:

```csharp
public static SvgDocument GetSvgDocument(byte[] file)
{
    Stream stream = new MemoryStream(file);
    try
    {
        return SvgDocument.Open<SvgDocument>(stream);
    }
    catch (Exception)
    {
        return null;
    }
}
```

Two constructors dereference the result without a guard (`SVGControl/SvgRenderer.cs:126-142`) **[VERIFIED]**:

```csharp
public SvgRenderer(byte[] doc, Size size, AutoSize autoSize)
{
    _doc = GetSvgDocument(doc);
    _original = _doc.Draw().Size;      // line 129 — NRE when _doc is null
    ...
}

public SvgRenderer(byte[] doc, Size size, Padding margin, AutoSize autoSize)
{
    _doc = GetSvgDocument(doc);
    _original = _doc.Draw().Size;      // line 138 — NRE when _doc is null
    ...
}
```

The `MemoryStream` is also never disposed and there is no argument-null guard on `file` — `new MemoryStream(null)` throws `ArgumentNullException` from outside the `try`, so a null argument surfaces as `ArgumentNullException` rather than the swallow path. **[VERIFIED]** by reading the method.

### 1.2 The construction chain that reaches the byte-array constructor

**[VERIFIED]** by reading each file:

```
MyBoxViewer.InitializeComponent()          UtilitiesCS/Dialogs/MyBoxViewer.Designer.cs:38
  -> new SVGControl.PictureBoxSVG()
       -> new SvgImageSelector(Size, Padding(0), MaintainAspectRatio, useDefaultImage: true)
                                             SVGControl/PictureBoxSVG.cs:24-29
            -> new SvgRenderer(Defaults.GetDefault.SvgImage, outer, margin, autoSize)
                                             SVGControl/SvgImageSelector.cs:44
                 -> GetSvgDocument(byte[])   SVGControl/SvgRenderer.cs:137
                 -> _doc.Draw()              SVGControl/SvgRenderer.cs:138
```

`ButtonSVG` is **not** on this path. `SVGControl/ButtonSVG.cs:21-25` calls the three-argument `SvgImageSelector(Size, Padding, AutoSize)` overload, which routes to `SvgRenderer(Size, Padding, AutoSize)` (`SVGControl/SvgRenderer.cs:162-169`) and never parses a document. **[VERIFIED]**

### 1.3 Dependency topology

| Fact | Source | Class |
|---|---|---|
| `Svg.dll` identity is `Svg, Version=3.4.0.0`; references `ExCSS, Version=4.2.3.0` | PEReader | [GIVEN] |
| Only `ExCSS 4.3.1` exists on disk (`packages/ExCSS.4.3.1/`); no 4.2.3 or 4.2.4 directory exists | `Glob packages/ExCSS*/**` returned only `ExCSS.4.3.1` | [VERIFIED] |
| `SVGControl.csproj:57-58` references `ExCSS, Version=4.3.1.0` via `..\packages\ExCSS.4.3.1\lib\net48\ExCSS.dll` | file read | [VERIFIED] |
| `SVGControl/bin/Debug/` contains `ExCSS.dll`, `Svg.dll`, `Fizzler.dll`, `SVGControl.dll.config` | `Glob SVGControl/bin/Debug/*` | [VERIFIED] |
| `Svg.dll` metadata contains the string `StylesheetParser` (an ExCSS type) | `Grep StylesheetParser SVGControl/bin/Debug/Svg.dll` → 1 match | [VERIFIED] |
| No file in `SVGControl/bin/Debug/` contains the string `Fizzler` except `SVGControl.dll.config` | `Grep Fizzler SVGControl/bin/Debug` → 1 match, in the config only | [VERIFIED] |
| Deployed Fizzler is 1.3.1.0; `ExCSS 4.3.1` does not reference Fizzler | PEReader | [GIVEN] |

The Fizzler observation is decisive for Q4: neither `Svg.dll` nor `ExCSS.dll` carries a Fizzler assembly reference. `SVGControl/PictureBoxSVG.cs:14` has `using Fizzler;` but no Fizzler type is used, so the C# compiler emits no `AssemblyRef` row for it. Fizzler is deployed as an unused transitive artifact.

### 1.4 Upstream `Svg` behavior (cross-referenced against the tagged source)

Fetched from `https://raw.githubusercontent.com/svg-net/SVG/v3.4.7/Source/SvgDocument.cs` **[VERIFIED — authoritative upstream at the exact deployed version tag]**:

```csharp
private static T Create<T>(XmlReader reader, string css = null)
    where T : SvgDocument, new()
{
    var styles = new List<ISvgNode>();
    var elementFactory = new SvgElementFactory();

    var svgDocument = Create<T>(reader, elementFactory, styles);

    if (css != null) { styles.Add(new SvgUnknownElement() { Content = css }); }

    if (styles.Any())
    {
        var cssTotal = string.Join(Environment.NewLine, styles.Select(s => s.Content).ToArray());
        var stylesheetParser = new StylesheetParser(true, true, tolerateInvalidValues: true);
        var stylesheet = stylesheetParser.Parse(cssTotal);
        foreach (var rule in stylesheet.StyleRules) { /* ... */ }
    }

    svgDocument?.FlushStyles(true);
    return svgDocument;
}
```

Two consequences, both important:

1. **The only ExCSS reference on the `Open` path lives inside this single method**, guarded by `if (styles.Any())`. **[VERIFIED]**
2. **`Create<T>(XmlReader, SvgElementFactory, List<ISvgNode>)` initialises `T svgDocument = null` and assigns only when it encounters an `XmlNodeType.Element` at an empty element stack. Element-free input therefore returns `null` with no exception.** **[VERIFIED via upstream source]**

Consequence 2 is a contract detail that the issue text does not capture: `SvgDocument.Open<SvgDocument>` can return `null` **without throwing**. `GetSvgDocument` therefore has two distinct null-producing paths, and AC-3's "InnerException is the original exception from `SvgDocument.Open`" is unachievable for the second one.

---

## 2. Q1 — Mechanism

### 2.1 Conclusion

**Confirmed, with one refinement and one correction.**

`SvgDocument.Open<SvgDocument>` fails because the CLR cannot satisfy `Svg`'s reference to `ExCSS, Version=4.2.3.0` in any host that does not apply a binding redirect covering that request. `GetSvgDocument` catches the resulting exception and returns `null`, and the constructor at `SVGControl/SvgRenderer.cs:138` dereferences it.

**Exception type concluded: `System.IO.FileNotFoundException`.**

Message shape (reconstructed from the standard .NET Framework binder message; the exact text was not captured in this session — **[INFERRED]**):

```
System.IO.FileNotFoundException: Could not load file or assembly
'ExCSS, Version=4.2.3.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a'
or one of its dependencies. The system cannot find the file specified.
```

Evidence for `FileNotFoundException` rather than `FileLoadException`:

- The prior author recorded exactly this type in an in-file comment at `SVGControl/SvgRenderer.cs:24-31`: "SvgDocument.Open throws FileNotFoundException for ExCSS 4.2.3." That comment was written by someone who had the failure in front of them. **[VERIFIED as a repository claim; not independently re-observed.]**
- .NET Framework appbase probing rejects a ref/def-mismatched candidate and continues probing; when probing is exhausted with no match, the binder raises `AssemblyResolve` and then throws `FileNotFoundException`. `FileLoadException` (HRESULT `0x80131040`, "The located assembly's manifest definition does not match the assembly reference") is the outcome when the assembly is bound through an explicit `<codeBase>` hint or is already loaded in the AppDomain under a conflicting identity — neither applies here. **[INFERRED from documented binder behavior.]**

A fix must not depend on the distinction. Both `FileNotFoundException` and `FileLoadException` derive from `System.IO.IOException`, and the parse path can also produce `System.Xml.XmlException`, `TypeInitializationException`, and `ArgumentException`. The correct contract is "catch broadly, log, and rethrow wrapped", not "catch a specific binder exception type".

### 2.2 Refinement — the failure is a JIT-time assembly load, not a runtime CSS parse

The ExCSS reference sits inside the `if (styles.Any())` branch, but the .NET Framework JIT compiles the whole method body when the method is first invoked, resolving the metadata tokens for `newobj StylesheetParser` and the `Stylesheet` local regardless of which branch executes. The assembly load therefore occurs when `Create<T>(XmlReader, string)` is JIT-compiled, not when the branch is taken. **[INFERRED from CLR JIT semantics; not empirically confirmed.]**

**This eliminates an otherwise attractive fix direction.** Rewriting `Defaults.GetDefault.SvgImage` to drop its `<style>` element and inline presentation attributes would make `styles.Any()` false but would **not** prevent the ExCSS bind, because the bind happens at JIT time. The planner should not pursue that direction without first disproving this inference (see § 9.3 for the verification step).

### 2.3 Correction to the issue text

The issue's Actual Behavior section states the exception "is unavailable because `GetSvgDocument` catches `Exception`". That is correct but incomplete. There is a second, exception-free route to `null`: an SVG payload containing no XML elements returns `null` from `SvgDocument.Open` with nothing thrown (§ 1.4). The fix must handle both, and AC-3's inner-exception requirement can only be met for the throwing route.

---

## 3. Q2 — Host Matrix

The decisive variable is which configuration file the CLR uses for the AppDomain in which `SVGControl.dll` executes, since that file determines whether the `ExCSS 4.2.3.0 → 4.3.1.0` redirect is applied.

### 3.1 Correction to the question's premise

There is no `TaskMaster.exe`. `TaskMaster/TaskMaster.csproj:21,25` **[VERIFIED]**:

```
<ProjectTypeGuids>{BAA0C2D2-18E2-41B9-852F-F413020CAA33};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>
<OutputType>Library</OutputType>
```

`{BAA0C2D2-…}` is the Office/VSTO project type. Production is a VSTO add-in DLL hosted inside `OUTLOOK.EXE`. The VSTO runtime creates a dedicated AppDomain per add-in and sets `AppDomainSetup.ConfigurationFile` to the deployed `TaskMaster.dll.config`, which is why the add-in's own redirects apply even though the process config is `outlook.exe.config`. **[INFERRED from the project type plus documented VSTO AppDomain behavior; not observed.]**

### 3.2 Host matrix

| Host | Config file governing the AppDomain | ExCSS redirect applied? | Reproduces? | Basis |
|---|---|---|---|---|
| **WinForms designer** — `devenv.exe`, legacy in-process designer (net481 project, so not `DesignToolsServer.exe`) | `devenv.exe.config` (Visual Studio install dir) | **No** — no ExCSS entry exists there, and the file is outside the repository | **Yes** | [INFERRED] |
| **`vstest.console.exe` test host** — `testhost.exe`/`testhost.x86.exe` with a per-source AppDomain | `SVGControl.Test.dll.config` (generated from `SVGControl.Test/app.config`) | **Applied, but the redirect is wrong** — see § 3.3 | **Would reproduce but for the `AssemblyResolve` fallback** | [VERIFIED config + INFERRED host behavior] |
| **Production add-in** — `OUTLOOK.EXE`, VSTO AppDomain | `TaskMaster.dll.config` (from `TaskMaster/app.config:81-83`, `newVersion="4.3.1.0"`) | **Yes** | **No** | [VERIFIED config + INFERRED host behavior] |

### 3.3 New finding — `SVGControl.Test/app.config` carries a broken ExCSS redirect

`SVGControl.Test/app.config:21-24` **[VERIFIED]**:

```xml
<dependentAssembly>
  <assemblyIdentity name="ExCSS" publicKeyToken="bdbe16be9b936b9a" culture="neutral" />
  <bindingRedirect oldVersion="0.0.0.0-4.2.4.0" newVersion="4.2.4.0" />
</dependentAssembly>
```

`newVersion="4.2.4.0"` names a version that does not exist anywhere on disk. Every other ExCSS redirect in the repository targets `4.3.1.0`. Full inventory **[VERIFIED by `Grep ExCSS *.config`]**:

- `newVersion="4.3.1.0"` (16 files): `SVGControl/app.config:15`, `UtilitiesCS/app.config:84`, `ToDoModel/app.config:84`, `TaskMaster/app.config:82`, `QuickFiler/app.config:79`, `Tags/app.config:71`, `TaskTree/app.config:71`, `TaskVisualization/app.config:71`, and the eight corresponding `*.Test/app.config` files (`QuickFiler.Test:71`, `Tags.Test:363`, `TaskTree.Test:363`, `TaskMaster.Test:143`, `TaskVisualization.Test:71`, `ToDoModel.Test:71`, `UtilitiesCS.Test:71`, `VBFunctions.Test:127`).
- `newVersion="4.2.4.0"` (1 file, the outlier): `SVGControl.Test/app.config:23`.

Effect in the test host: the `ExCSS 4.2.3.0` request falls inside `0.0.0.0-4.2.4.0`, is redirected to `4.2.4.0`, and `4.2.4.0` is not on disk. The redirect converts a resolvable request into an unresolvable one. **[VERIFIED as a config fact; the runtime consequence is [INFERRED].]**

The reason this does not currently surface as a test failure is that no test in `SVGControl.Test` exercises `SvgRenderer` (§ 8.1), and the `AssemblyResolve` fallback added in `0b4c5c43` masks the redirect (§ 4.3). It is a live trap for the first test that touches this code — which is exactly what AC-1 and AC-5 require.

---

## 4. Q3 — Why the `AssemblyResolve` Fallback Does Not Rescue the Designer

### 4.1 The handler is reached

`SVGControl/SvgRenderer.cs:36-42` installs the handler from the static constructor. **[VERIFIED]** The static constructor runs before the first instance constructor body executes. Loading the `SvgRenderer` type requires resolving the `SvgDocument` field type, which loads `Svg.dll`, but `Svg.dll`'s ExCSS reference is not materialised until `Create<T>` is JIT-compiled inside `SvgDocument.Open` — strictly after the static constructor has completed. The handler is therefore installed before the failing bind. **[INFERRED from CLR type-initialisation and lazy-assembly-load ordering.]**

Answering the AC-7 sub-question directly: **the fallback at `SVGControl/SvgRenderer.cs:36-104` is reached in the failing host. It is reached and returns `null`.**

### 4.2 Strategy 1 cannot succeed on a first load

`SVGControl/SvgRenderer.cs:51-69` scans `AppDomain.CurrentDomain.GetAssemblies()` for a loaded assembly with simple name `ExCSS` and a matching public key token. The failing request *is* the first attempt to load ExCSS into the AppDomain, so no ExCSS assembly is loaded when the handler runs. Strategy 1 finds nothing.

This is host-independent: strategy 1 can only succeed when some other code path has already loaded ExCSS 4.3.1 by a matching reference. In the test host, `SVGControl.Test.csproj` has no ExCSS reference at all **[VERIFIED — the `<Reference>` list at lines 122-152 contains no ExCSS entry]**, so nothing pre-loads it. **Confirmed.**

### 4.3 Strategy 2 resolves against the wrong probing path

`SVGControl/SvgRenderer.cs:83-92`:

```csharp
var byName = System.Reflection.Assembly.Load(new System.Reflection.AssemblyName(requested.Name));
```

`Assembly.Load` binds against the **current AppDomain's** `ApplicationBase` and `PrivateBinPath`, not against the directory the calling assembly was loaded from. The hypothesis under evaluation is therefore **confirmed**:

- **Test host** — the AppDomain's `ApplicationBase` is the test source directory, which contains `ExCSS.dll` (copied transitively from the `SVGControl` project reference). A partial-name bind carries no version, so no `<bindingRedirect>` version range applies to it, and the binder returns whatever `ExCSS.dll` it probes up — 4.3.1.0. The public key token matches, the handler returns the assembly, and the CLR accepts it as satisfying the 4.2.3.0 request (values returned from `AssemblyResolve` bypass version checking). **This is why the test host currently works, and it is also why the broken `4.2.4.0` redirect in § 3.3 is invisible.** **[INFERRED; the copy of `ExCSS.dll` into `SVGControl.Test/bin/Debug/` could not be verified because that directory does not exist in the working tree.]**
- **Designer host** — the AppDomain is `devenv.exe`'s, whose `ApplicationBase` is the Visual Studio IDE directory. `ExCSS.dll` is not there. `Assembly.Load` fails, the nested `AssemblyResolve` it raises is short-circuited by the re-entrance guard at lines 76-80 (which correctly returns `null` to prevent recursion), the `catch` at lines 94-97 swallows the failure, and the handler returns `null` at line 103. The original `FileNotFoundException` propagates into `SvgDocument.Open`, is caught by `GetSvgDocument`, and becomes the `NullReferenceException`. **Confirmed. [INFERRED from binder semantics and the code as written.]**

### 4.4 What the fallback would have to do instead

The handler must resolve relative to the **location of the requesting assembly**, not the host's probing path. The direction (not a full implementation) is:

1. Build an ordered candidate-directory list rather than a single directory:
   - `Path.GetDirectoryName(typeof(SvgRenderer).Assembly.Location)` when `Location` is non-empty;
   - the directory derived from `typeof(SvgRenderer).Assembly.CodeBase` (convert the `file://` URI) — this survives some cases where `Location` is unhelpful;
   - `AppDomain.CurrentDomain.BaseDirectory` as a last resort.
2. For each candidate, probe for `<simpleName>.dll` and load the first hit with `Assembly.LoadFrom`.
3. Keep the existing public-key-token equality check on the loaded result before returning it.
4. Keep the existing re-entrance guard.

**Risks the planner must weigh:**

- **`Location` is empty for byte-array loads.** `Assembly.Load(byte[])` produces an assembly whose `Location` is `""`. Visual Studio's designer type-resolution service has historically used `Assembly.LoadFrom` against a shadow-copy directory under `%LOCALAPPDATA%\Microsoft\VisualStudio\<ver>\ProjectAssemblies\<hash>\`, which yields a non-empty `Location` — but whether `ExCSS.dll` is present in that shadow directory alongside `SVGControl.dll` is **[UNVERIFIED]**. If VS shadow-copies only the assemblies it explicitly resolves, `ExCSS.dll` should be there because it is an explicit `<Reference>` in both `SVGControl.csproj:57` and `UtilitiesCS.csproj:67`. This must be confirmed empirically before the fix is declared to close AC-8.
- **`LoadFrom` context divergence.** An assembly loaded via `Assembly.LoadFrom` enters the LoadFrom context. If the same ExCSS assembly is later bound into the default context by a different code path, the CLR can end up with two distinct type identities for the same types, producing `InvalidCastException` at the boundary. In practice the risk is low when the `LoadFrom` path is the same file the default binder would have found, because the CLR matches on identity, but the risk is real in the designer where the shadow-copy path is not the default probing path. `UtilitiesCS` references ExCSS 4.3.1 directly, so a second default-context bind is plausible. The planner should document this and prefer returning an already-loaded match (strategy 1) whenever one exists — the existing code already does this, and that ordering should be preserved.
- **Empty-candidate fallthrough.** If no candidate directory yields the file, the handler must still return `null` so other resolvers and default resolution can run. It must not throw from inside an `AssemblyResolve` handler.

### 4.5 Constraint the fix must satisfy regardless of approach

Even a perfect `AssemblyResolve` fallback is a mitigation, not a guarantee, because it depends on host-specific probing behavior that the repository does not control. AC-3 (a diagnosable exception instead of an NRE) is the only part of the remedy that is fully within repository control and is host-independent. It should be treated as the primary deliverable; the binding remedy is secondary.

---

## 5. Q4 — Fizzler Redirect Defect

### 5.1 Classification: **(b) latent defect, currently inert**

- Nothing in the deployed dependency graph requests Fizzler. `Grep Fizzler SVGControl/bin/Debug` returned exactly one match, in `SVGControl.dll.config` — i.e. in the redirect itself, not in any assembly's metadata. **[VERIFIED]**
- `ExCSS 4.3.1` does not reference Fizzler. **[GIVEN]**
- `Svg 3.4.7` does not reference Fizzler (its CSS selector work goes through ExCSS `StylesheetParser`; the `Fizzler` string is absent from `Svg.dll`). **[VERIFIED]**
- `SVGControl/PictureBoxSVG.cs:14` has an unused `using Fizzler;` directive, which produces no `AssemblyRef` row. **[VERIFIED]**
- `Fizzler.dll` (1.3.1.0) is nonetheless deployed because `SVGControl.csproj:60-62` and `UtilitiesCS.csproj:70` declare explicit `<Reference>` items with `HintPath`s. **[VERIFIED]**

It is **not** an active contributor to issue #418. The redirect would break any future consumer that requests Fizzler in the `0.0.0.0-1.3.0.0` range, because it redirects to `1.3.0.0` while only `1.3.1.0` is on disk — the same failure shape as the ExCSS defect in § 3.3.

### 5.2 Full inventory (13 files) **[VERIFIED by `Grep Fizzler *.config`]**

| File | Line |
|---|---|
| `QuickFiler/app.config` | 82-84 |
| `QuickFiler.Test/app.config` | 74-76 |
| `SVGControl/app.config` | 18-20 |
| `SVGControl.Test/app.config` | 26-28 |
| `Tags/app.config` | 74-76 |
| `TaskMaster/app.config` | 85-87 |
| `TaskTree/app.config` | 74-76 |
| `TaskVisualization/app.config` | 74-76 |
| `TaskVisualization.Test/app.config` | 74-76 |
| `ToDoModel/app.config` | 87-89 |
| `ToDoModel.Test/app.config` | 74-76 |
| `UtilitiesCS/app.config` | 87-89 |
| `UtilitiesCS.Test/app.config` | 74-76 |

All 13 carry `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"`.

### 5.3 Recommendation: **separate issue**

Rationale:

- It is provably inert today, so it cannot be covered by a fail-before/pass-after regression test scoped to #418. Bundling it would put an untestable change inside a bug fix.
- The change touches 13 files across 9 projects, all outside the #418 blast radius. Under `minor-audit` work mode that is disproportionate scope.
- The two defects share a *shape* (redirect target not present on disk) but not a *cause*. The right cross-cutting remedy is a single guard that validates every `bindingRedirect` `newVersion` against the versions present under `packages/` — a repository-hygiene item, not a bug fix.

**In scope for #418:** `SVGControl.Test/app.config:23` only, because that file is directly on the path of the AC-1/AC-5 test work and its redirect is actively wrong for the assembly the new tests will load.

---

## 6. Q5 — Default SVG Payload

### 6.1 Well-formedness: **confirmed well-formed. Eliminated as a cause.**

`SVGControl/SvgImageSelector.cs:315-331` is a C# verbatim string in which `""` denotes a single `"`. Resolved, the payload is:

```xml
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
  <defs>
    <style>.canvas{fill: none; opacity: 0;} … .light-blue{fill: #005dba; opacity: 1;}</style>
  </defs>
  <title>IconLightImage</title>
  <g id="canvas" class="canvas">
    <path class="canvas" d="M16,16H0V0H16Z" />
  </g>
  <g id="level-1">
    <path class="light-defaultgrey-10" d="M14.5,2.5v12H1.5V2.5Z" />
    <path class="light-defaultgrey" d="M14.5,2H1.5L1,2.5v12l.5.5h13l.5-.5V2.5ZM14,14H2V3H14Z" />
    <path class="light-yellow" d="M12,5.5A1.5,1.5,0,1,1,10.5,4,1.5,1.5,0,0,1,12,5.5Z" />
    <path class="light-blue" d="M14,11.09V12.5l-2.819-2.82L8.988,11.877H8.281L4.814,8.41,2,11.225V9.811L4.461,7.35h.707l3.466,3.466,2.193-2.193h.707Z" />
  </g>
</svg>
```

Checks performed by inspection **[VERIFIED]**:

- Exactly one root element (`<svg>`), correctly closed.
- Every child element is closed or self-closed (`<defs>`, `<style>`, `<title>`, two `<g>`, five `<path />`).
- Every attribute value is quoted and balanced.
- The `<style>` text content contains `{`, `}`, `#`, `:`, `;`, `.`, `,` and no `<`, `&`, or `]]>`, so it needs no CDATA section and cannot break XML parsing.
- No XML declaration is present, which is legal.
- The `d` path data contains `.`, `,`, `-` and digits only — no XML-significant characters.

### 6.2 `Encoding.ASCII.GetBytes` safety: **safe for this literal, fragile as a contract**

Every character in the literal is in the ASCII range: element/attribute names, hex colour digits (`#212121`, `#996f00`, `#005dba`), path data, and whitespace. `Encoding.ASCII.GetBytes` therefore produces a byte-for-byte faithful encoding today. **[VERIFIED by character inspection.]**

The fragility is real but latent: `Encoding.ASCII` silently replaces any character above `U+007F` with `?` rather than throwing. A future edit that introduces a non-ASCII character (for example a typographic dash in `<title>`) would produce corrupt bytes without any diagnostic. Since the payload declares no encoding, the XML reader defaults to UTF-8, and ASCII is a strict subset of UTF-8, so switching to `Encoding.UTF8.GetBytes` would be byte-identical for the current literal while removing the silent-corruption mode. That is a low-risk hygiene improvement the planner may fold in, not a defect.

### 6.3 Verdict

Malformed XML is **eliminated** as a candidate cause of the reported `NullReferenceException`. The original diagnosis in `## Suspected Cause / Notes` bullet 3 of the issue is not supported.

---

## 7. Q6 — Blast Radius of the Contract Change

### 7.1 Call sites of `SvgRenderer.GetSvgDocument(byte[])` **[VERIFIED by `Grep GetSvgDocument`]**

| # | Site | Result currently used how | Null tolerated? |
|---|---|---|---|
| 1 | `SVGControl/SvgRenderer.cs:128` (`SvgRenderer(byte[], Size, AutoSize)`) | assigned to `_doc`, dereferenced on line 129 | **No — NRE** |
| 2 | `SVGControl/SvgRenderer.cs:137` (`SvgRenderer(byte[], Size, Padding, AutoSize)`) | assigned to `_doc`, dereferenced on line 138 | **No — NRE (the reported crash)** |
| 3 | `SVGControl/SvgImageSelector.cs:130` (`ResourceName` setter) | assigned to `_renderer.Document` | **Yes — via the setter's null check** |
| 4 | `SVGControl/SvgImageSelector.cs:284` (`SetDefaultImage()`) | assigned to `_renderer.Document` | **Yes — via the setter's null check** |

`SVGControl/SVGParser.cs:72-77` declares a **separate, unrelated** `GetSvgDocument(byte[])` instance method on the internal `SVGParser` class. It does not catch, so it already propagates. It has no call sites outside `SVGParser` itself. It is out of scope for AC-4 but is a useful reference for what a non-swallowing implementation looks like. **[VERIFIED]**

### 7.2 Consumers that tolerate a null document **[VERIFIED]**

| Member | Location | Null-tolerant behavior | Impact of the change |
|---|---|---|---|
| `SvgRenderer.Document` setter | `SvgRenderer.cs:218-230` | `if (value != null) { _original = _doc.Draw().Size; }` — assigns null, skips measurement | **Must keep accepting null.** It is the sink for call sites 3 and 4 and is the only way to clear the image. Do not tighten this to non-null. |
| `SvgRenderer.Render()` | `SvgRenderer.cs:239-244` | `if (_doc == null) return null;` | **Must keep the guard.** `Document` can still be set to null explicitly. |
| `SvgImageSelector.SaveRendering` setter | `SvgImageSelector.cs:190-245` | branches on `_renderer.Document != null` and on `== null` (the `== null` arm is an empty block with a commented-out `MessageBox`) | Unchanged. Note the `else if (_renderer.Document == null)` arm at line 239-242 is dead-effect code — it does nothing. |
| `SvgImageSelector.ResourceName` setter | `SvgImageSelector.cs:108-136` | `value is null \|\| value.Name == ""` → either `SetDefaultImage()` or `_renderer.Document = null` | If `GetSvgDocument` starts throwing, line 130 propagates into a designer property-grid edit. The setter must decide: propagate (fail fast) or catch-log-and-leave-unchanged. **This is a behavior decision the planner must make explicitly.** |
| `SvgImageSelector.UseDefaultImage` setter | `SvgImageSelector.cs:170-188` | sets `_renderer.Document = null` on the false branch | Unchanged; does not call `GetSvgDocument`. |
| `SvgImageSelector.SetDefaultImage()` | `SvgImageSelector.cs:282-285` | assigns the result straight to `Document` | Same decision as `ResourceName`. Reached from `ResourceName` and `UseDefaultImage`. |
| `PictureBoxSVG` ctor | `PictureBoxSVG.cs:21-33` | calls `_imageSvg.Render()` at line 30, which tolerates a null document | **This is the designer-facing failure point.** If the byte-array `SvgRenderer` ctor throws, `PictureBoxSVG`'s ctor throws, and every designer that hosts it fails to load — with a *diagnosable* message instead of an NRE. |
| `ButtonSVG` ctor | `ButtonSVG.cs:16-28` | never parses | **Unaffected.** |
| `SvgOptionsConverter1.ConvertTo` / `SvgOptionsConverter2` | `SvgOptionsConverter.cs:23`, `SvgOptionsConverter2.cs:23` | read `AboluteImagePath`/`AutoSize` only | Unaffected. |
| `DropDownEditor.EditValue` | `DropDownEditor.cs:65` | reads `context.Instance` as `SvgImageSelector`, sets `ResourceName` indirectly via the property grid | Inherits the `ResourceName` decision. |

### 7.3 Controls affected downstream **[VERIFIED by `Grep PictureBoxSVG|ButtonSVG`]**

`PictureBoxSVG` (the `useDefaultImage: true` path) appears in:
`UtilitiesCS/Dialogs/MyBoxViewer.Designer.cs`, `UtilitiesCS/Dialogs/FolderNotFoundViewer.Designer.cs`, `UtilitiesCS/Threading/ProgressMultiStepViewer.Designer.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/Config/ConfigViewer.Designer.cs`, `QuickFiler/Viewers/ItemViewer.Designer.cs`, `QuickFiler/Viewers/EfcViewer.Designer.cs`, `QuickFiler/Viewers/Form1.Designer.cs`, `UtilitiesCS.Test/Form1.Designer.cs`, `UtilitiesCS.Test/Form2.Designer.cs`, `SVGControl.Test/Form1.Designer.cs`, `SVGControl.Test/Form2.Designer.cs`.

**Implication for AC-4:** the change from "returns null" to "throws" converts a silent degradation (blank icon) into a hard construction failure across every one of those forms — including at production runtime inside Outlook, not only in the designer. `QuickFiler/Viewers/ItemViewer` is on a hot production path. The planner must therefore decide, explicitly and in writing, whether:

- **Option A (fail-fast at the constructor):** the byte-array `SvgRenderer` constructors throw. Matches AC-3 literally. Risk: converts a cosmetic degradation into a control-construction failure in production if the binding regresses.
- **Option B (fail-fast at the parse boundary, tolerant at the control boundary):** `GetSvgDocument` gains a non-swallowing sibling that throws, the byte-array constructors use it, and `PictureBoxSVG`/`SvgImageSelector` decide whether to catch-and-log at the control boundary so a production icon failure degrades rather than crashes.

AC-3 as written mandates Option A at the `SvgRenderer` constructor level. Option B is compatible with AC-3 provided the exception is thrown by the constructor and any tolerance is introduced strictly above it. The evidence favours Option B for `PictureBoxSVG` specifically, given the production blast radius. This is a scope question the orchestrator should resolve before planning, since it is not settled by the issue text.

---

## 8. Q7 — Testability

### 8.1 Current state of `SVGControl.Test`

**[VERIFIED]** The project contains only `Form1.cs`, `Form1.Designer.cs`, `Form2.cs`, `Form2.Designer.cs`, `GetRelativePath_Test.cs`, `RelativePathCoverageTests.cs`, `Properties/AssemblyInfo.cs`, `Resources.Designer.cs`, `Properties/Resources.Designer.cs`. There is no coverage of `SvgRenderer`, `SvgImageSelector`, `SVGParser`, `PictureBoxSVG`, or `ButtonSVG`.

### 8.2 `InternalsVisibleTo` — already present

`SVGControl/RelativePath.cs:19` **[VERIFIED]**:

```csharp
[assembly: InternalsVisibleTo("SVGControl.Test")]
```

`SVGControl` is not strong-named (`SVGControl/Properties/AssemblyInfo.cs` declares no key file **[VERIFIED]**), so no public-key suffix is required. `SvgRenderer` is `internal` with `public` members, so `SvgRenderer.GetSvgDocument` and the byte-array constructors are directly reachable from `SVGControl.Test`. **No new `InternalsVisibleTo` is required for that project.**

If tests were instead placed in `UtilitiesCS.Test`, a new `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` on `SVGControl` **and** a new `ProjectReference` would both be required — `UtilitiesCS.Test.csproj` currently has no reference to `SVGControl` at all **[VERIFIED by `Grep SVGControl UtilitiesCS.Test/UtilitiesCS.Test.csproj` → no matches]**.

### 8.3 Blocking prerequisite — `SVGControl.Test` does not currently build or run

This is the largest practical finding in this research and it directly conditions AC-1, AC-5, and AC-6.

1. **The project is not in the solution.** `Grep '^Project\(' TaskMaster.sln` lists 19 projects. `SVGControl` is present (line 40); **`SVGControl.Test` is absent**. **[VERIFIED]** It is therefore not built by `msbuild TaskMaster.sln`, not covered by the analyzer gate, and not covered by the nullable gate.
2. **Its pinned test packages are not on disk.** `SVGControl.Test/packages.config` pins `Castle.Core 5.1.1`, `FluentAssertions 6.12.0`, `Moq 4.20.69`, `MSTest.TestAdapter 3.1.1`, `MSTest.TestFramework 3.1.1`. **[VERIFIED]** What is actually present under `packages/` is `Castle.Core.5.2.1`, `FluentAssertions.8.3.0` and `8.8.0`, `Moq.4.20.72`, `MSTest.TestAdapter.3.9.3` and `4.1.0`, `MSTest.TestFramework.3.9.3` and `4.1.0`. **[VERIFIED]** A `Glob` for `packages/MSTest.TestAdapter.3.1.1/**/*.props` and for the other four pinned versions returned **no files**.
3. **The project has a hard `<Error>` guard on the missing package.** `SVGControl.Test.csproj:158-170` defines `EnsureNuGetPackageBuildImports` with `BeforeTargets="PrepareForBuild"` that emits an MSBuild `<Error>` when `..\packages\MSTest.TestAdapter.3.1.1\build\net462\MSTest.TestAdapter.props` does not exist. **[VERIFIED]** Since that path does not exist, the project fails at build start.
4. **Corroborating history.** `docs/features/archive/2026-06-12-taskmaster-ribbon-tab-185/evidence/remediation-baseline/baseline-test-assembly-set.md:20` records: "SVGControl.Test has no built bin/Debug DLL output" and that it is excluded from the test-assembly enumeration. Multiple archived QA artifacts record recurring "`[SVGControl.Test]` cannot resolve DLLs from NuGet packages" warnings from `scripts/vscode/Invoke-VSBuild.ps1`. **[VERIFIED by reading those artifacts.]** `SVGControl/bin/Debug/` exists but `SVGControl.Test/bin/` does not. **[VERIFIED]**
5. **The repo's auto-repair script cannot fix it.** `scripts/vscode/Sync-PackageReferences.ps1` rewrites `<HintPath>` values to a version that exists on disk, but it does not touch `packages.config` pins, `<Reference>` `Version=` attributes, or the `EnsureNuGetPackageBuildImports` `<Error>` guard. **[VERIFIED by reading the script's matched lines.]**

**Consequence:** AC-1 ("a deterministic MSTest regression test in `SVGControl.Test`") cannot be satisfied without first repairing the test project. The required repair is:

- retarget `SVGControl.Test/packages.config` and the corresponding `<Reference>`/`<HintPath>` entries to the MSTest, Moq, FluentAssertions, and Castle.Core versions actually present under `packages/` (matching what the other in-solution test projects use);
- update the `EnsureNuGetPackageBuildImports` `<Error>` conditions and both `MSTest.TestAdapter` `<Import>` paths to the retargeted version;
- add `SVGControl.Test` to `TaskMaster.sln` with `Debug|Any CPU` / `Release|Any CPU` configuration mappings, so the analyzer and nullable gates cover it and `Invoke-MSTest` enumerates its output;
- fix `SVGControl.Test/app.config:23` (§ 3.3) at the same time, since the new tests are the first consumers of that redirect.

This is real, unavoidable scope. The orchestrator should decide whether it belongs inside #418 or in a prerequisite issue before planning proceeds. It is not optional: without it there is no place to put the AC-1 regression test.

### 8.4 Seams required for deterministic parse-failure testing

`SvgDocument.Open<T>` is a static method on a third-party type and cannot be mocked with Moq. Two levels of seam are available, and only the first is needed for the ACs as written.

**Level 1 — no new seam (recommended, sufficient for AC-1/AC-3/AC-5).**

Deterministic parse failure can be produced purely through input, with no production seam and no temporary files:

| Input | Expected `SvgDocument.Open` behavior | Confidence |
|---|---|---|
| `Encoding.ASCII.GetBytes("this is not xml")` | throws `System.Xml.XmlException` ("Data at the root level is invalid") | [INFERRED — standard `XmlTextReader` behavior] |
| `Encoding.ASCII.GetBytes("<svg><g></svg>")` | throws `System.Xml.XmlException` (mismatched end tag) | [INFERRED] |
| `Array.Empty<byte>()` | **returns `null` with no exception** (§ 1.4) | [VERIFIED via upstream source] |
| `Defaults.GetDefault.SvgImage` | returns a non-null `SvgDocument` in a correctly-bound host | [VERIFIED payload well-formedness; host binding [INFERRED]] |
| `null` | `new MemoryStream(null)` throws `ArgumentNullException` **outside** the `try` | [VERIFIED by reading the method] |

The empty-array row is the important one: it gives AC-1 a failing test that does **not** depend on assembly binding at all, and it exercises the exception-free null path that the issue text does not describe. That makes the regression test deterministic across all three hosts, which is exactly what UT1 requires.

**Level 2 — an injectable parse seam (only if the planner chooses Option B in § 7.3).**

The minimal seam consistent with `.claude/rules/csharp.md` (preference order: interface → delegate → adapter) is an injectable delegate:

- add an internal `Func<byte[], SvgDocument>` parse hook on `SvgRenderer`, defaulting to the production implementation;
- expose an internal constructor overload on `SvgRenderer` (and, if `SvgImageSelector`/`PictureBoxSVG` tolerance is added, on `SvgImageSelector`) that accepts it;
- tests then supply a delegate that throws a chosen exception or returns `null`, and assert the wrapping contract without depending on `Svg` or ExCSS at all.

This seam is what makes it possible to assert AC-3's "`InnerException` is the original exception from `SvgDocument.Open`" against a *known* inner exception instance, which input-driven testing cannot do precisely.

### 8.5 Constraints the tests must observe

- **No temporary files** (UT4, currently zero approved exceptions). All inputs are in-memory byte arrays; nothing in the proposed strategy needs the filesystem.
- **No `Thread.Sleep`/`Task.Delay`** — not relevant here; the code is synchronous.
- **Global side effect to be aware of.** Touching `SvgRenderer` for the first time runs its static constructor, which registers a **process-wide** `AppDomain.CurrentDomain.AssemblyResolve` handler (`SvgRenderer.cs:36-42`). The `Interlocked.Exchange` guard makes it idempotent, and it is never removed. This does not break UT1 independence (the handler is additive and order-independent), but it means any test asserting on assembly-resolution behavior is asserting against permanently mutated AppDomain state and cannot be reliably ordered relative to other tests. **Do not write a test that asserts the fallback is absent.**
- **`SvgDocument.Draw()` uses GDI+.** Success-path assertions that call `Render()` or the constructor's `_doc.Draw()` allocate a real `Bitmap`. That is deterministic and in-process (no external service), but it is a real GDI+ dependency; the returned `Bitmap` must be disposed. Prefer asserting on `Document != null` and on `Size` over asserting on pixel content.
- **FluentAssertions version.** Whatever version the repaired project targets must be consistent with the rest of the repo; `SVGControl.Test` currently pins 6.12.0 while `UtilitiesCS` uses 8.9.0. Assertion API differences between FluentAssertions 6 and 8 are material and should be settled during the § 8.3 repair, not during test authoring.

---

## 9. Automation Feasibility

### 9.1 Fully automatable

| Step | How |
|---|---|
| Reproduce the `NullReferenceException` from a byte-array `SvgRenderer` constructor | MSTest with `Array.Empty<byte>()` or malformed-XML bytes. No host dependency, no binding dependency, no filesystem. |
| Verify AC-2 (no silent swallow) | Static inspection plus a test that asserts an exception is observed (or a log entry is produced through the existing `log4net` logger, captured with an in-memory `IAppender`). |
| Verify AC-3 (diagnosable exception with correct `InnerException`) | With the Level-2 delegate seam (§ 8.4), inject a sentinel exception and assert `InnerException` identity. |
| Verify AC-4 (call sites keep their contract) | Tests for `SvgRenderer.Document = null`, `Render()` returning `null`, `SetDefaultImage()`, and the `ResourceName` clear path. |
| Verify AC-5 (coverage) | `vstest.console.exe … /EnableCodeCoverage` once `SVGControl.Test` is in the solution and building (§ 8.3). |
| Verify the default SVG round-trips | `SvgRenderer.GetSvgDocument(Defaults.GetDefault.SvgImage)` should be non-null in the test host. This is also the automated regression for the ExCSS binding in the test host. |
| Verify AC-8 for the config defect | A test that reads `AppDomain.CurrentDomain.SetupInformation.ConfigurationFile` (the deployed `SVGControl.Test.dll.config`) and asserts that every `bindingRedirect/@newVersion` matches the version of the correspondingly-named assembly actually loaded or present next to the test assembly. This is deterministic, uses no temporary files, and needs no working-directory assumption. |
| Verify AC-6 (toolchain) | Standard `csharpier` → analyzer msbuild → nullable msbuild → `vstest.console.exe` loop. |

### 9.2 **Not** automatable — human interaction required

**Two steps require a human. The orchestrator must resolve both before this feature can be declared complete.**

**H-1 — Confirm the WinForms designer loads `MyBoxViewer` without exception after the fix.**

The issue itself lists this under `## Proposed Fix / Validation Ideas` ("Manual verification notes"). There is no automatable substitute: reproducing the designer's assembly-resolution environment requires `devenv.exe`'s AppDomain, its configuration file, and its shadow-copy type-resolution service, none of which can be created from a test process. Constructing a surrogate AppDomain with a hand-built `AppDomainSetup` would require writing a synthetic `.config` file to disk, which UT4 prohibits (no temporary files, zero approved exceptions).

Exactly what a human must do:

1. Build the solution in `Debug|Any CPU`.
2. Open Visual Studio and open `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the WinForms designer (double-click the file in Solution Explorer, or right-click → View Designer).
3. Observe whether the designer surface renders or shows the "An error occurred while loading the document" panel.
4. If an error appears, click **Show Call Stack / Details** and capture the full exception type, message, and stack trace verbatim.
5. Repeat for `UtilitiesCS/Dialogs/FolderNotFoundViewer.cs` and `QuickFiler/Viewers/ItemViewer.cs`, which host the same control.
6. Save the captured text to `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/designer-load-<timestamp>.md` using the standard evidence schema.

**H-2 — Capture the actual exception type and message from the designer host (needed to close AC-7 empirically rather than by inference).**

AC-7 asks for "the actual exception type and message thrown by `SvgDocument.Open`". This research names `System.IO.FileNotFoundException` for `ExCSS, Version=4.2.3.0` and reconstructs the message, but marks it **[INFERRED]** because no designer-host stack trace exists anywhere in the repository and none could be produced with the tools available in this session.

There is a favourable sequencing property here: **the AC-2/AC-3 fix is itself the instrument that produces this evidence.** Once `GetSvgDocument` stops discarding the exception and the constructor rethrows it wrapped, the designer's own error panel will print the real type and message. So H-2 collapses into H-1: run H-1 *after* the AC-3 fix is in place, and the captured text simultaneously satisfies AC-7 empirically and demonstrates AC-3.

If the orchestrator wants the evidence *before* the fix, the human alternative is to enable Fusion assembly-binding logging (`HKLM\SOFTWARE\Microsoft\Fusion`: `EnableLog=1`, `ForceLog=1`, `LogFailures=1`, `LogPath=<dir>`), restart Visual Studio, open the designer, and read the failed-bind log for `ExCSS`. That modifies machine registry state and is explicitly a human action, not an agent action.

### 9.3 Additional unverified item the planner should resolve

**U-1 — Whether ExCSS is bound at JIT time regardless of `styles.Any()` (§ 2.2).** This determines whether "remove `<style>` from the default SVG" is a viable remedy. It can be settled without a designer by a one-off throwaway probe (a console program that calls `SvgDocument.Open` on a style-free SVG in an AppDomain without the redirect and observes whether ExCSS appears in `AppDomain.CurrentDomain.GetAssemblies()`), or by decompiling `SvgDocument.Create<T>(XmlReader, string)` from `packages/Svg.3.4.7/lib/net481/Svg.dll` and confirming that the ExCSS tokens are in that method body rather than in a separate helper. The research position is that the remedy is **not** viable, and the planner should not adopt it unless U-1 disproves that.

**U-2 — Whether `ExCSS.dll` is present in Visual Studio's `ProjectAssemblies` shadow-copy directory alongside `SVGControl.dll` (§ 4.4).** This determines whether a `LoadFrom`-against-`Assembly.Location` fallback actually helps the designer. It can be checked by a human inspecting `%LOCALAPPDATA%\Microsoft\VisualStudio\<ver>\ProjectAssemblies\` after opening the designer, and is naturally folded into H-1.

---

## 10. Recommended Direction and Constraints

The following states a direction and the constraints a fix must satisfy. It deliberately does not specify an implementation.

### 10.1 Recommended direction

Treat #418 as **two separable remedies with different confidence levels**, and sequence them so the high-confidence one produces the evidence for the low-confidence one.

**Remedy 1 (primary, host-independent, fully within repository control) — make the failure diagnosable.**

`GetSvgDocument` must stop discarding exceptions. The byte-array constructors must not dereference a possibly-null document. This is entirely determined by the code in this repository, is testable without any host dependency, and closes AC-1 through AC-5. It is the correct primary deliverable and would have prevented this investigation from being necessary.

**Remedy 2 (secondary, host-dependent, partially outside repository control) — fix the binding.**

Two repository-scoped items:

- `SVGControl.Test/app.config:23` — the `newVersion="4.2.4.0"` redirect targets a version that does not exist. Correct it to `4.3.1.0` to match all sixteen sibling configs. This is the only config change in scope for #418.
- The `AssemblyResolve` fallback in `SVGControl/SvgRenderer.cs:44-104` — extend strategy 2 to probe the requesting assembly's own directory (§ 4.4), with the stated `LoadFrom`-context and empty-`Location` caveats documented in-code.

Remedy 2 cannot be *proved* to fix the designer without H-1. If H-1 shows the designer still fails after Remedy 2, AC-8's escape clause applies: document the designer-host limitation with the H-1 evidence and let AC-3 stand as the delivered mitigation.

### 10.2 Constraints the fix must satisfy

1. **Handle both null paths.** `SvgDocument.Open` can throw *and* can return `null` for element-free input (§ 1.4). The exception contract must cover the null-return case, where no `InnerException` exists.
2. **Do not narrow the catch by exception type.** The failure surface spans `FileNotFoundException`, `FileLoadException`, `XmlException`, `TypeInitializationException`, and `ArgumentException`. A type-narrowed catch would reintroduce silent NREs for the uncovered types.
3. **Preserve null tolerance where it is intentional.** `SvgRenderer.Document`'s setter, `Render()`'s null guard, and `SvgImageSelector`'s clear paths (§ 7.2) must keep accepting null; that is how the image is deliberately cleared. Only the *parse result* changes contract.
4. **Decide the production tolerance question explicitly (§ 7.3).** `PictureBoxSVG` appears in at least eleven designers including `QuickFiler/Viewers/ItemViewer`, a hot production path inside Outlook. Converting a blank icon into a control-construction failure at production runtime is a real behavior change and must be a stated decision, not an accident of the AC wording.
5. **Log through the existing logger.** `SVGControl/SvgRenderer.cs:20-22` already declares a `log4net.ILog`. AC-2 requires it be used. Do not introduce console output.
6. **Dispose the `MemoryStream`.** `GetSvgDocument` currently leaks it on every call.
7. **Do not widen scope to the Fizzler redirects.** Thirteen files, provably inert, no possible regression test (§ 5.3). Open a separate issue.
8. **Repair `SVGControl.Test` before writing tests in it, or move the tests.** Section 8.3 is a hard prerequisite for AC-1, AC-5, and AC-6.

### 10.3 Rejected alternatives (brief)

- **Remove `<style>` from `Defaults.GetDefault.SvgImage` to avoid ExCSS.** Rejected: the ExCSS bind is a JIT-time assembly load of `Create<T>`, not a runtime consequence of the `styles.Any()` branch (§ 2.2). Contingent on U-1.
- **Downgrade the deployed ExCSS to 4.2.3 to match `Svg 3.4.7`'s compile-time reference.** Rejected for this change: ExCSS 4.2.3 is not on disk, seventeen `app.config` files and three `.csproj` files reference 4.3.1.0, and `QuickFiler`/`UtilitiesCS` bind it independently. The blast radius is disproportionate to a `minor-audit` bug fix, and it would not help the designer in any case because the designer applies no redirect and would still need the version it finds to match. Worth recording as a possible future dependency-alignment issue.
- **Add ExCSS entries to `devenv.exe.config`.** Rejected: that file is outside the repository, is per-developer-machine, and is overwritten by Visual Studio updates. It is not a shippable remedy.
- **Reproduce the designer host in an MSTest surrogate AppDomain.** Rejected: constructing an AppDomain with a hand-authored `ConfigurationFile` requires writing a config file to disk, which UT4 prohibits with zero approved exceptions.
- **Place the new tests in `UtilitiesCS.Test` to avoid repairing `SVGControl.Test`.** Rejected as the default: it needs a new `ProjectReference` plus a new `InternalsVisibleTo("UtilitiesCS.Test")` on `SVGControl` (§ 8.2), and it puts `SVGControl` tests in the wrong project, leaving `SVGControl.Test` permanently broken. Retained as a fallback if the orchestrator rules the § 8.3 repair out of scope for #418.

---

## 11. Answers Summary

| Q | Answer |
|---|---|
| **Q1** | Confirmed. `System.IO.FileNotFoundException` for `ExCSS, Version=4.2.3.0, PublicKeyToken=bdbe16be9b936b9a`, raised when `SvgDocument.Create<T>(XmlReader, string)` is JIT-compiled inside `SvgDocument.Open`. `FileLoadException` is the alternative if the binder reports a ref/def mismatch instead. Refinement: `Open` can also return `null` without throwing, for element-free input. |
| **Q2** | Designer (`devenv.exe`, legacy in-proc for net481): **reproduces** — `devenv.exe.config` carries no ExCSS redirect. Test host (`vstest`/`testhost`): **would reproduce**, and its own `app.config` makes it worse by redirecting to a nonexistent `4.2.4.0`; it is currently masked by the `AssemblyResolve` fallback. Production: **does not reproduce** — the VSTO AppDomain uses `TaskMaster.dll.config`, which redirects correctly to `4.3.1.0`. Correction: there is no `TaskMaster.exe`; production is a VSTO add-in inside `OUTLOOK.EXE`. |
| **Q3** | Hypothesis **confirmed**. The handler *is* reached and returns `null`. Strategy 1 fails because the failing request is the first ExCSS load. Strategy 2's `Assembly.Load(new AssemblyName("ExCSS"))` binds against the host AppDomain's `ApplicationBase` — the VS IDE directory — not the directory holding `SVGControl.dll`. Required change: probe the requesting assembly's own directory and `LoadFrom` it. Risks: empty `Assembly.Location` under byte-array loads, `LoadFrom`-context type-identity divergence, and the unverified question of whether `ExCSS.dll` is in VS's shadow-copy directory. |
| **Q4** | **(b) latent, currently inert.** Nothing in the deployed graph references Fizzler — verified by the absence of the string from every DLL in `SVGControl/bin/Debug/`. Present in 13 `app.config` files (enumerated in § 5.2). **Recommend a separate issue**, not this change. |
| **Q5** | The default SVG is **well-formed** and **ASCII-clean**; `Encoding.ASCII.GetBytes` is safe for the current literal. Malformed XML is **eliminated** as a cause. `Encoding.UTF8` would be byte-identical today and would remove the silent-substitution failure mode — optional hygiene, not a defect. |
| **Q6** | Four `GetSvgDocument` call sites (two crash, two tolerate null). Null-tolerant consumers that must keep their contract: `SvgRenderer.Document` setter, `Render()`, `SvgImageSelector.SaveRendering`/`ResourceName`/`UseDefaultImage`/`SetDefaultImage`. `PictureBoxSVG` is affected and appears in 11 designers including a production QuickFiler path; `ButtonSVG` is unaffected. AC-4 requires an explicit decision on whether `PictureBoxSVG` tolerates or propagates. |
| **Q7** | `InternalsVisibleTo("SVGControl.Test")` **already exists** (`SVGControl/RelativePath.cs:19`) and `SVGControl` is not strong-named, so no new attribute is needed. **But `SVGControl.Test` is not in `TaskMaster.sln`, its five pinned test packages are absent from `packages/`, and its `EnsureNuGetPackageBuildImports` `<Error>` guard blocks the build.** Repairing it is a hard prerequisite for AC-1/AC-5/AC-6. Deterministic parse-failure testing needs no production seam (malformed bytes, empty array); asserting AC-3's exact `InnerException` identity needs a small injectable `Func<byte[], SvgDocument>` seam. |

---

## 12. Files Referenced

Production and configuration:
- `SVGControl/SvgRenderer.cs`
- `SVGControl/SvgImageSelector.cs`
- `SVGControl/SVGParser.cs`
- `SVGControl/PictureBoxSVG.cs`
- `SVGControl/ButtonSVG.cs`
- `SVGControl/DropDownEditor.cs`
- `SVGControl/SvgOptionsConverter.cs`
- `SVGControl/RelativePath.cs`
- `SVGControl/Properties/AssemblyInfo.cs`
- `SVGControl/SVGControl.csproj`, `SVGControl/app.config`, `SVGControl/packages.config`
- `SVGControl.Test/SVGControl.Test.csproj`, `SVGControl.Test/app.config`, `SVGControl.Test/packages.config`
- `UtilitiesCS/Dialogs/MyBoxViewer.Designer.cs`, `UtilitiesCS/app.config`, `UtilitiesCS/packages.config`, `UtilitiesCS/UtilitiesCS.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `UtilitiesCS.Test/app.config`
- `TaskMaster/TaskMaster.csproj`, `TaskMaster/app.config`
- `TaskMaster.sln`, `TaskMaster.runsettings`
- `scripts/vscode/Sync-PackageReferences.ps1`
- All 13 `app.config` files listed in § 5.2 and all 17 listed in § 3.3

Package artifacts:
- `SVGControl/bin/Debug/Svg.dll`, `SVGControl/bin/Debug/Svg.xml`, `SVGControl/bin/Debug/SVGControl.dll.config`
- `packages/ExCSS.4.3.1/lib/net48/ExCSS.dll`

Upstream:
- `https://raw.githubusercontent.com/svg-net/SVG/v3.4.7/Source/SvgDocument.cs` (tag `v3.4.7`, matching the deployed `Svg 3.4.7` package)

Historical evidence consulted:
- `docs/features/archive/2026-06-12-taskmaster-ribbon-tab-185/evidence/remediation-baseline/baseline-test-assembly-set.md`
- `docs/features/archive/2026-07-04-coverage-gaps-test-seams-236/remediation-plan.2026-07-04T17-29.md`
- Multiple archived QA-gate artifacts recording the recurring `SVGControl.Test` package-resolution warnings
