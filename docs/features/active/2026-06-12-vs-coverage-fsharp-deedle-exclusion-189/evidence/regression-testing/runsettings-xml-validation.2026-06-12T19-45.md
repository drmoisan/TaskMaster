# Runsettings XML Validation (P1-T2, AC1/AC2)

Timestamp: 2026-06-12T19-45

Command:
```
powershell -NoProfile -Command "[xml]$d = Get-Content 'TaskMaster.runsettings' -Raw; ..."
git diff TaskMaster.runsettings
```

EXIT_CODE: 0

Output Summary:
- XML well-formedness: `[xml]` load succeeded -> `XML_PARSE_OK`. The file is valid `<RunSettings>` XML.
- `<MSTest><Parallelize>` preserved: parsed `Workers=0 Scope=ClassLevel`, identical to the Phase 0 baseline (`evidence/baseline/runsettings-and-coverage-config.2026-06-12T19-45.md`). The `git diff` confirms the MSTest subtree (lines 3–8) is byte-for-byte unchanged; the edit is purely additive, inserting the collector block before `</RunSettings>`.
- New `<DataCollectionRunSettings>` block present with `DataCollector friendlyName="Code Coverage"`.
- `enabled` attribute: NOT present (parsed value empty `[]`), so coverage is opt-in (AC3) — confirmed in P2-T2.
- `ModulePathCount=7`, matching coverage.config verbatim and in the same order:
  1. `.*Deedle.*`
  2. `.*FSharp.*`
  3. `.*Castle\.Core.*`
  4. `.*FluentAssertions.*`
  5. `.*Moq.*`
  6. `.*Microsoft\.Testing.*`
  7. `.*MSTest.*`
- AC1 minimum requirement (`.*FSharp.*` and `.*Deedle.*` present, plus the remaining five mirrored) satisfied.

## git diff (verbatim)

```diff
@@ -6,4 +6,38 @@
       <Scope>ClassLevel</Scope>
     </Parallelize>
   </MSTest>
+  <!-- (opt-in collector comment) -->
+  <DataCollectionRunSettings>
+    <DataCollectors>
+      <DataCollector friendlyName="Code Coverage">
+        <Configuration>
+          <CodeCoverage>
+            <ModulePaths>
+              <Exclude>
+                <ModulePath>.*Deedle.*</ModulePath>
+                <ModulePath>.*FSharp.*</ModulePath>
+                <ModulePath>.*Castle\.Core.*</ModulePath>
+                <ModulePath>.*FluentAssertions.*</ModulePath>
+                <ModulePath>.*Moq.*</ModulePath>
+                <ModulePath>.*Microsoft\.Testing.*</ModulePath>
+                <ModulePath>.*MSTest.*</ModulePath>
+              </Exclude>
+            </ModulePaths>
+          </CodeCoverage>
+        </Configuration>
+      </DataCollector>
+    </DataCollectors>
+  </DataCollectionRunSettings>
 </RunSettings>
```
