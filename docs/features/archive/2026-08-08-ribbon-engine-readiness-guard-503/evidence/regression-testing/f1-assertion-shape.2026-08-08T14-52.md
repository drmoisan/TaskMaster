# F1 — Post-Change Assertion Shape and Three-Condition Non-Vacuity Argument (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T2]
Command: Source inspection of `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` lines 176-216, plus `git diff -- TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` to confirm the change is confined to one method
EXIT_CODE: 0

This artifact records the **source-inspection** half of the F1 proof. The **executable** half — a recorded failing run against a deliberately mutated embedded resource — is P1-T7 (`f1-fail-proof.2026-08-08T14-52.md`), with restoration at P1-T8 and the pass-after state at P1-T10. Source inspection alone is not accepted as the proof; it is the argument the executable proof then confirms.

## Post-change method body, verbatim

```csharp
        [TestMethod]
        public void RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback()
        {
            // Arrange
            var document = LoadRibbonDocument();

            // Act: index every element that carries an id.
            var elementsById = document
                .Descendants()
                .Where(element => element.Attribute("id") != null)
                .ToDictionary(element => element.Attribute("id")!.Value, element => element);

            // Assert
            foreach (var controlId in EngineCommandCatalog.ControlIds)
            {
                elementsById
                    .Should()
                    .ContainKey(
                        controlId,
                        "the catalog control id '{0}' must exist in the ribbon XML",
                        controlId
                    );
                // Bind the attribute first. A null-conditional dereference here would
                // short-circuit the whole assertion chain, including .Should(), so the
                // test would pass silently on the exact regression it exists to catch.
                var getEnabled = elementsById[controlId].Attribute("getEnabled");
                getEnabled
                    .Should()
                    .NotBeNull(
                        "control '{0}' is engine-backed and must declare a getEnabled callback",
                        controlId
                    );
                getEnabled!
                    .Value.Should()
                    .Be(
                        EngineCommandGetEnabledCallback,
                        "control '{0}' is engine-backed and must be disabled until its engine loads",
                        controlId
                    );
            }
        }
```

## Output Summary — the three required failure conditions

`remediation-inputs.2026-08-08T14-26.md` §F1 requires the assertion to fail when the attribute is missing, when it is present with the wrong value, and when it is present but empty. Each condition is mapped to the specific assertion line that fails for it:

| # | Condition | Failing assertion line | Mechanism |
|---|---|---|---|
| 1 | Attribute **absent** | `getEnabled.Should().NotBeNull("control '{0}' is engine-backed and must declare a getEnabled callback", controlId)` | `XElement.Attribute("getEnabled")` returns `null`. The result is bound to a local **before** any dereference, so no `?.` short-circuits the chain. `NotBeNull` is invoked on the null reference through the `ObjectAssertions` extension and fails with a message naming the control id. This is the condition the pre-change form could not detect. |
| 2 | Attribute present with the **wrong value** | `getEnabled!.Value.Should().Be(EngineCommandGetEnabledCallback, ...)` | The attribute is non-null so condition 1 passes; `Value` returns the wrong string and `Be` fails on string inequality. |
| 3 | Attribute present but **empty** | `getEnabled!.Value.Should().Be(EngineCommandGetEnabledCallback, ...)` | The attribute is non-null so condition 1 passes; `Value` returns `""`, which is not `"EngineCommand_GetEnabled"`, and `Be` fails on string inequality. |

The null-forgiving `!` on line `getEnabled!` is a compiler-flow annotation only. It suppresses a nullable-flow warning at the dereference site; it does not suppress a runtime failure and it does not short-circuit. If control reached that line with a null `getEnabled`, the preceding `NotBeNull` assertion would already have failed and thrown, so the dereference is unreachable in the null case.

## Structural verification performed

- **Zero `?.` occurrences** in the method body (lines 170-216), verified by `sed -n '170,216p' ... | grep -c '?\.'` returning 0.
- Both required shapes present: `.Should().NotBeNull(` and `.Value.Should()` ... `.Be(`.
- The `ContainKey` assertion above the change is **unchanged**.
- `git diff --numstat` reports `12 3` for the file — a single hunk, entirely inside this one method.
- `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` (the AC6 sibling test) is **byte-identical** to its pre-change text; it does not appear in the diff. Its use of `?.Value ==` sits inside a LINQ predicate where null-means-no-match is the intended semantics, so it is correct as written and must remain.
- No other member of the file changed.
- File size after the change: **318 physical lines**, under the 500-line cap.

Binary outcome satisfied: all three conditions are mapped to a named assertion line.
