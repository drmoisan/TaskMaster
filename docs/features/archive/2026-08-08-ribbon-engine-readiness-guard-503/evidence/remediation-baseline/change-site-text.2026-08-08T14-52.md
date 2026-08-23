# Phase 0 — Verbatim Pre-Remediation Text of Both Change Sites (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T7]
Command: Read tool inspection of `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` lines 180-207 and `TaskMaster\Ribbon\RibbonExplorer.xml` lines 440-474 and 96-113
EXIT_CODE: 0

## Change site 1 — F1

`C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`, method `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback`, lines 180-207, quoted verbatim with line numbers:

```csharp
180	            var document = LoadRibbonDocument();
181	
182	            // Act: index every element that carries an id.
183	            var elementsById = document
184	                .Descendants()
185	                .Where(element => element.Attribute("id") != null)
186	                .ToDictionary(element => element.Attribute("id")!.Value, element => element);
187	
188	            // Assert
189	            foreach (var controlId in EngineCommandCatalog.ControlIds)
190	            {
191	                elementsById
192	                    .Should()
193	                    .ContainKey(
194	                        controlId,
195	                        "the catalog control id '{0}' must exist in the ribbon XML",
196	                        controlId
197	                    );
198	                elementsById[controlId]
199	                    .Attribute("getEnabled")
200	                    ?.Value.Should()
201	                    .Be(
202	                        EngineCommandGetEnabledCallback,
203	                        "control '{0}' is engine-backed and must be disabled until its engine loads",
204	                        controlId
205	                    );
206	            }
207	        }
```

The defective sequence is visible at lines 198-200: `elementsById[controlId].Attribute("getEnabled")?.Value.Should()`. The null-conditional operator at line 200 short-circuits the entire remaining chain — including the `.Should()` call and the `.Be(...)` assertion — whenever `Attribute("getEnabled")` returns `null`. This is exactly the condition the test exists to detect, so today the test passes silently on the regression it names.

The `ContainKey` assertion at lines 191-197 is correct and is not part of the F1 change; P1-T1 leaves it unchanged.

## Sibling test that must remain unchanged

`RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` at lines 215-236 uses `element.Attribute("getEnabled")?.Value == EngineCommandGetEnabledCallback` at line 224. That `?.` sits **inside a LINQ predicate**, where null-means-no-match is the intended semantics, so the construct is correct there. This test independently enforces AC5 by set equality and must be byte-identical after P1-T1.

## Change site 2 — F2

`C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster\Ribbon\RibbonExplorer.xml`, the three `TriageSet*` `<button>` elements inside `<group id="TriageGroup" ...>`, lines 447-466 quoted verbatim with line numbers (the enclosing `group` open tag at 447 and the following `menu` open tag at 466 are shown for boundary context and are **not** modified):

```xml
447	        <group id="TriageGroup" imageMso="Filter" label="Triage">
448	          <button
449	            id="TriageSetA"
450	            onAction="TriageSetA_Click"
451	            getEnabled="EngineCommand_GetEnabled"
452	            label="Set A"
453	          />
454	          <button
455	            id="TriageSetB"
456	            onAction="TriageSetB_Click"
457	            getEnabled="EngineCommand_GetEnabled"
458	            label="Set B"
459	          />
460	          <button
461	            id="TriageSetC"
462	            onAction="TriageSetC_Click"
463	            getEnabled="EngineCommand_GetEnabled"
464	            label="Set C"
465	          />
466	          <menu id="OtherTriageActions" label="Other" size="normal">
```

The three elements occupy lines 448-465, six lines each. The `<button` open tag is indented ten spaces; its attributes are indented twelve. P2-T1 replaces lines 448-465 with three single lines indented ten spaces, which is a net reduction of fifteen lines (539 to 524).

## Mutation target for the F1 fail-proof

`TaskMaster\Ribbon\RibbonExplorer.xml` lines 99-105, the `<button id="TrainSpam" ...>` element:

```xml
 99	          <button
100	            id="TrainSpam"
101	            imageMso="CancelAll"
102	            onAction="TrainSpam_Click"
103	            getEnabled="EngineCommand_GetEnabled"
104	            label="Train Spam"
105	          />
```

Line 103 is the whole-line deletion target for P1-T5. It is already in multi-line form, so the deletion requires no re-indentation and `git diff --numstat` will report exactly `0` added and `1` deleted. It sits in a different `group` from the three F2 elements, so the fail-proof mutation and the F2 edit do not overlap.

Binary outcome satisfied: both blocks are present above, and the F1 block visibly contains the `?.Value.Should()` sequence at lines 199-200 that F1 removes.
