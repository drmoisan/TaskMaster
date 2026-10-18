import test from "node:test";
import assert from "node:assert/strict";
import { source } from "./issue-997-source-loader.mjs";
import { createBrowser, suggestionRow, selectorView } from "./issue-997-browser-harness.mjs";

function show(browser, rows = [suggestionRow()], render = {}, view = {}) {
  browser.receive({ type: "render", rows, ...render });
  browser.receive(selectorView(view));
}
const child = () => ({ identity: "alpha", displayName: "Alpha", folderPath: "Projects\\Apollo\\Alpha", hasChildren: false, expanded: false, selected: true, children: [] });

test("empty startup focuses the collapsed dropdown", () => {
  const browser = createBrowser(source);
  browser.windowFocus();
  assert.equal(browser.dropdown.focusCount, 1);
  assert.equal(browser.list.children.length, 0);
  assert.deepEqual(browser.messages, []);
});

for (const value of [null, undefined, false, "message", {}, { type: 12 }, { type: "unknown" }]) {
  test(`invalid or unknown message is ignored: ${JSON.stringify(value)}`, () => {
    const browser = createBrowser(source);
    browser.receive(value);
    assert.equal(browser.list.children.length, 0);
    assert.deepEqual(browser.messages, []);
  });
}

for (const renderFirst of [true, false]) {
  test(`render and selector view work in either order: ${renderFirst}`, () => {
    const browser = createBrowser(source);
    const render = { type: "render", rows: [suggestionRow()] };
    if (renderFirst) { browser.receive(render); browser.receive(selectorView()); }
    else { browser.receive(selectorView()); browser.receive(render); }
    assert.equal(browser.list.querySelectorAll(".row").length, 1);
    assert.equal(browser.list.getAttribute("role"), "listbox");
    const active = browser.list.querySelector(".row.active");
    assert.equal(active.getAttribute("aria-selected"), "true");
    assert.equal(browser.list.getAttribute("aria-activedescendant"), active.id);
    assert.equal(active.scrolls.length, 1);
    browser.windowFocus();
    assert.equal(browser.list.focusCount, 1);
  });
}

test("collapsed view resolves committed identity instead of another selected row", () => {
  const browser = createBrowser(source);
  show(browser, [suggestionRow(), suggestionRow({ rowIndex: 1, selected: false, percentText: "18%" })], {}, {
    mode: "collapsed", committedIdentity: "row-b", pendingIdentity: "row-b",
    options: [{ identity: "row-a", isSelectable: true }, { identity: "row-b", isSelectable: true }]
  });
  assert.equal(browser.list.querySelectorAll(".row").length, 1);
  assert.equal(browser.list.querySelector(".pct").textContent, "18%");
  assert.equal(browser.list.getAttribute("role"), null);
  assert.equal(browser.list.getAttribute("aria-activedescendant"), null);
});

test("unknown committed identity falls back to only the first selected row", () => {
  const browser = createBrowser(source);
  show(browser, [suggestionRow(), suggestionRow({ rowIndex: 1 })], {}, { mode: "collapsed", committedIdentity: "missing" });
  assert.equal(browser.list.querySelectorAll(".row").length, 1);
});

test("empty and invalid optional collections produce an empty list", () => {
  const browser = createBrowser(source);
  browser.receive({ type: "render", rows: null });
  browser.receive({ type: "selectorView", mode: "invalid", isOpen: "true", options: {} });
  assert.equal(browser.list.children.length, 0);
  assert.equal(browser.root.getAttribute("data-mode"), "collapsed");
  assert.equal(browser.dropdown.getAttribute("aria-expanded"), "false");
});

test("row cells retain literal names, title, truncation and host percentage", () => {
  const browser = createBrowser(source);
  show(browser, [suggestionRow({ percentText: "custom 17%", cells: [
    { kind: "segment", text: "<Apollo>", segmentIndex: 0, truncationEligible: true },
    { kind: "arrow", text: "", segmentIndex: -1 },
    { kind: "segment", text: "Leaf", segmentIndex: 1, truncationEligible: false }
  ] })]);
  const segment = browser.list.querySelector(".seg.trunc");
  assert.equal(segment.textContent, "<Apollo>");
  assert.equal(segment.title, "<Apollo>");
  assert.equal(browser.list.querySelector(".arrow").textContent, "→");
  assert.equal(browser.list.querySelector(".pct").textContent, "custom 17%");
  const event = browser.dispatch(segment, "dblclick");
  assert.equal(event.propagationStopped, true);
  assert.deepEqual(browser.messages, [{ type: "segmentDoubleClick", rowIndex: 0, segmentIndex: 0 }]);
});

for (const kind of ["plus", "minus"]) {
  test(`root ${kind} only toggles, despite normal parent bubbling`, () => {
    const browser = createBrowser(source);
    show(browser, [suggestionRow({ cells: [{ kind, text: "", segmentIndex: -1 }] })]);
    const toggle = browser.list.querySelector(".aff");
    assert.equal(toggle.textContent, kind === "plus" ? "+" : "−");
    const event = browser.dispatch(toggle, "click");
    assert.equal(event.propagationStopped, true);
    assert.deepEqual(browser.messages, [{ type: "affordanceToggle", rowIndex: 0 }]);
  });
}

test("clicking a selectable row uses its host identity", () => {
  const browser = createBrowser(source);
  show(browser);
  browser.dispatch(browser.list.querySelector(".seg"), "click");
  assert.deepEqual(browser.messages, [{ type: "selectorActivate", identity: "row-a" }]);
});

test("nonselectable and absent options do not create activation listeners", () => {
  const browser = createBrowser(source);
  show(browser, [suggestionRow({ isSuggestion: false })], {}, { options: [{ identity: 3, isSelectable: false }], pendingIdentity: null, committedIdentity: null });
  const row = browser.list.querySelector(".row");
  assert.equal(row.getAttribute("role"), null);
  browser.dispatch(row, "click");
  assert.deepEqual(browser.messages, []);
  browser.receive(selectorView({ options: null }));
  assert.equal(browser.list.querySelector(".row").getAttribute("role"), null);
});

for (const [theme, expected] of [["dark", "dark"], ["light", "light"], ["unexpected", "light"]]) {
  test(`theme uses controlled value: ${theme}`, () => {
    const browser = createBrowser(source);
    browser.receive({ type: "themeChange", theme });
    assert.equal(browser.root.getAttribute("data-theme"), expected);
    assert.equal(browser.dropdown.getAttribute("aria-expanded"), null);
  });
}

test("dropdown click requests selector toggle and mirrors host open state", () => {
  const browser = createBrowser(source);
  browser.dispatch(browser.dropdown, "click");
  assert.deepEqual(browser.messages, [{ type: "selectorToggle" }]);
  browser.receive(selectorView({ isOpen: true }));
  assert.equal(browser.dropdown.getAttribute("aria-expanded"), "true");
  browser.receive(selectorView({ isOpen: false, mode: "collapsed" }));
  assert.equal(browser.dropdown.getAttribute("aria-expanded"), "false");
});

for (const [key, value] of [["ArrowUp", "up"], ["ArrowDown", "down"], ["Enter", "enter"], ["Escape", "escape"]]) {
  test(`selector key ${key} prevents browser default and sends exact payload`, () => {
    const browser = createBrowser(source);
    const event = browser.dispatch(browser.dropdown, "keydown", { key });
    assert.equal(event.defaultPrevented, true);
    assert.deepEqual(browser.messages, [{ type: "selectorKey", key: value }]);
  });
}

for (const [key, direction] of [["ArrowLeft", "left"], ["ArrowRight", "right"]]) {
  for (const type of ["suggestion", "plain", "collapsed", "expanded", "affordance", "empty"]) {
    test(`arrow ${direction} routes ${type} state through host contract`, () => {
      const browser = createBrowser(source);
      const row = suggestionRow({ isSuggestion: type === "suggestion", collapsed: type === "collapsed", leafExpanded: type === "expanded" });
      if (type === "affordance") row.cells = [{ kind: "plus", text: "", segmentIndex: -1 }];
      show(browser, type === "empty" ? [] : [row]);
      const event = browser.dispatch(browser.list, "keydown", { key });
      assert.equal(event.defaultPrevented, true);
      assert.deepEqual(browser.messages, [{ type: ["plain", "empty"].includes(type) ? "unhandledArrow" : "arrowKey", direction }]);
    });
  }
}

test("unrecognized keys preserve browser behavior", () => {
  const browser = createBrowser(source);
  const event = browser.dispatch(browser.list, "keydown", { key: "Tab" });
  assert.equal(event.defaultPrevented, false);
  assert.deepEqual(browser.messages, []);
});

for (const action of ["click", "Enter", " "]) {
  test(`direct-child ${JSON.stringify(action)} activates exact variant address once`, () => {
    const browser = createBrowser(source);
    const node = child();
    show(browser, [suggestionRow({ leafExpanded: true, subfolders: [node] })], { selectedSubfolderIndex: 0, selectedFolder: node.folderPath });
    const element = browser.list.querySelector(".sub");
    const event = browser.dispatch(element, action === "click" ? "click" : "keydown", { key: action });
    assert.equal(event.defaultPrevented, true);
    assert.equal(event.propagationStopped, true);
    assert.deepEqual(browser.messages, [source.variant === "baseline"
      ? { type: "selectorSubfolderActivate", rowIdentity: "row-a", subfolderIndex: 0 }
      : { type: "selectorDescendantActivate", rowIdentity: "row-a", nodeIdentity: "alpha" }]);
    assert.equal(element.getAttribute("aria-selected"), "true");
    assert.equal(element.tabIndex, 0);
    assert.equal(browser.list.getAttribute("aria-activedescendant"), element.id);
  });
}

test("unhandled child key bubbles to selector navigation", () => {
  const browser = createBrowser(source);
  show(browser, [suggestionRow({ leafExpanded: true, subfolders: [child()] })]);
  const event = browser.dispatch(browser.list.querySelector(".sub"), "keydown", { key: "ArrowDown" });
  assert.equal(event.propagationStopped, false);
  assert.deepEqual(browser.messages, [{ type: "selectorKey", key: "down" }]);
});

for (const index of [-1, 3, "0", null]) {
  test(`invalid selected-child index falls back to row: ${JSON.stringify(index)}`, () => {
    const browser = createBrowser(source);
    show(browser, [suggestionRow({ subfolders: [child()] })], { selectedSubfolderIndex: index, selectedFolder: child().folderPath }, { mode: "collapsed" });
    assert.equal(browser.list.querySelector(".seg").textContent, "Apollo");
  });
}

test("legacy matching selected child displays its full committed value", () => {
  const browser = createBrowser(source);
  show(browser, [suggestionRow({ subfolders: [child()] })], { selectedSubfolderIndex: 0, selectedFolder: child().folderPath }, { mode: "collapsed" });
  assert.equal(browser.list.querySelector(".seg").textContent, child().folderPath);
  browser.receive({ type: "selectionChange", subfolderIndex: "invalid" });
  browser.receive(selectorView({ mode: "collapsed" }));
  assert.equal(browser.list.querySelector(".seg").textContent, "Apollo");
});

test("mismatched selected child does not masquerade as committed destination", () => {
  const browser = createBrowser(source);
  show(browser, [suggestionRow({ subfolders: [child()] })], { selectedSubfolderIndex: 0, selectedFolder: "other" }, { mode: "collapsed" });
  assert.equal(browser.list.querySelector(".seg").textContent, "Apollo");
});

test("numeric legacy selection change restores the matching direct child display", () => {
  const browser = createBrowser(source);
  show(browser, [suggestionRow({ subfolders: [child()] })], { selectedSubfolderIndex: 0, selectedFolder: child().folderPath }, { mode: "collapsed" });
  browser.receive({ type: "selectionChange", subfolderIndex: -1 });
  browser.receive(selectorView({ mode: "collapsed" }));
  assert.equal(browser.list.querySelector(".seg").textContent, "Apollo");
  browser.receive({ type: "selectionChange", subfolderIndex: 0 });
  browser.receive(selectorView({ mode: "collapsed" }));
  assert.equal(browser.list.querySelector(".seg").textContent, child().folderPath);
  assert.deepEqual(browser.messages, []);
});

for (const options of [{ chrome: false }, { webview: false }]) {
  test(`missing native messaging is a safe no-op: ${JSON.stringify(options)}`, () => {
    const browser = createBrowser(source, options);
    browser.dispatch(browser.dropdown, "click");
    browser.receive({ type: "render", rows: [suggestionRow()] });
    browser.windowFocus();
    assert.deepEqual(browser.messages, []);
    assert.equal(browser.dropdown.focusCount, 1);
  });
}
