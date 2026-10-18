import test from "node:test";
import assert from "node:assert/strict";
import { source } from "./issue-997-source-loader.mjs";
import { createBrowser, suggestionRow, selectorView } from "./issue-997-browser-harness.mjs";

function node(identity, children = [], overrides = {}) {
  return { identity, displayName: identity, folderPath: `Projects\\Apollo\\${identity}`,
    hasChildren: children.length > 0, expanded: children.length > 0, selected: false,
    children, ...overrides };
}
function tree() {
  return [node("Alpha", [node("Beta", [node("Gamma")]), node("Delta")]), node("Omega")];
}
function show(children = tree(), render = {}, view = {}, row = {}) {
  const browser = createBrowser(source);
  browser.receive({ type: "render", rows: [suggestionRow({ leafExpanded: true, subfolders: children, ...row })], ...render });
  browser.receive(selectorView(view));
  return browser;
}
function sub(browser, identity) {
  return browser.list.querySelectorAll(".sub").find((element) => element.id === `folder-option-0-node-${identity}`);
}

test("three levels retain provider order and show controls only for nonleaves", () => {
  const browser = show();
  const entries = browser.list.querySelectorAll(".sub");
  assert.deepEqual(entries.map((entry) => entry.querySelector(".seg").textContent), ["Alpha", "Beta", "Gamma", "Delta", "Omega"]);
  assert.equal(browser.list.querySelectorAll(".aff").length, 2);
  for (const identity of ["Alpha", "Beta"]) {
    const toggle = sub(browser, identity).querySelector(".aff");
    assert.equal(toggle.textContent, "−");
    assert.equal(toggle.getAttribute("role"), "button");
    assert.equal(toggle.getAttribute("aria-expanded"), "true");
    assert.equal(toggle.getAttribute("aria-label"), `Collapse ${identity}`);
    assert.equal(toggle.tabIndex, 0);
  }
  for (const identity of ["Gamma", "Delta", "Omega"]) assert.equal(sub(browser, identity).querySelector(".aff"), null);
});

test("collapsed nested branch shows plus and omits deeper descendants", () => {
  const children = tree();
  children[0].children[0].expanded = false;
  const browser = show(children);
  const toggle = sub(browser, "Beta").querySelector(".aff");
  assert.equal(toggle.textContent, "+");
  assert.equal(toggle.getAttribute("aria-expanded"), "false");
  assert.equal(toggle.getAttribute("aria-label"), "Expand Beta");
  assert.equal(sub(browser, "Gamma"), undefined);
  assert.ok(sub(browser, "Delta"));
});

for (const identity of ["Alpha", "Beta"]) {
  for (const action of ["click", "Enter", " "]) {
    test(`${identity} toggle ${JSON.stringify(action)} sends one toggle and suppresses activation`, () => {
      const browser = show();
      const event = browser.dispatch(sub(browser, identity).querySelector(".aff"), action === "click" ? "click" : "keydown", { key: action });
      assert.equal(event.defaultPrevented, true);
      assert.equal(event.propagationStopped, true);
      assert.deepEqual(browser.messages, [{ type: "selectorDescendantToggle", rowIdentity: "row-a", nodeIdentity: identity }]);
    });
  }
}

for (const identity of ["Alpha", "Beta", "Gamma"]) {
  for (const action of ["click", "Enter", " "]) {
    test(`${identity} name ${JSON.stringify(action)} activates exact descendant once`, () => {
      const browser = show();
      const element = sub(browser, identity);
      const name = element.querySelector(".seg");
      const event = browser.dispatch(name, action === "click" ? "click" : "keydown", { key: action });
      assert.equal(event.defaultPrevented, true);
      assert.equal(event.propagationStopped, true);
      assert.equal(element.getAttribute("role"), "option");
      assert.equal(element.getAttribute("aria-selected"), "false");
      assert.equal(name.title, `Projects\\Apollo\\${identity}`);
      assert.deepEqual(browser.messages, [{ type: "selectorDescendantActivate", rowIdentity: "row-a", nodeIdentity: identity }]);
    });
  }
}

test("toggle navigation key bubbles to selector without toggle or activation", () => {
  const browser = show();
  const event = browser.dispatch(sub(browser, "Beta").querySelector(".aff"), "keydown", { key: "ArrowUp" });
  assert.equal(event.propagationStopped, false);
  assert.equal(event.defaultPrevented, true);
  assert.deepEqual(browser.messages, [{ type: "selectorKey", key: "up" }]);
});

test("toggle unrecognized key leaves browser default and sends no message", () => {
  const browser = show();
  const event = browser.dispatch(sub(browser, "Beta").querySelector(".aff"), "keydown", { key: "Tab" });
  assert.equal(event.propagationStopped, false);
  assert.equal(event.defaultPrevented, false);
  assert.deepEqual(browser.messages, []);
});

test("committed grandchild displays durable path collapsed and regains highlight on reopen", () => {
  const children = tree();
  children[0].children[0].children[0].selected = true;
  const browser = show(children, { selectedDescendantIdentity: "Gamma", selectedFolder: "Alpha\\Beta\\Gamma" }, { mode: "collapsed", isOpen: false });
  assert.equal(browser.list.querySelector(".seg").textContent, "Alpha\\Beta\\Gamma");
  assert.equal(browser.list.querySelector(".seg").title, "Alpha\\Beta\\Gamma");
  assert.equal(browser.list.querySelector(".sub"), null);
  browser.windowFocus();
  assert.equal(browser.document.activeElement, browser.dropdown);
  browser.receive(selectorView());
  const active = sub(browser, "Gamma");
  assert.equal(active.classList.contains("active"), true);
  assert.equal(active.getAttribute("aria-selected"), "true");
  assert.equal(browser.list.getAttribute("aria-activedescendant"), active.id);
  assert.equal(active.scrolls.length, 1);
  assert.equal(browser.list.querySelector(".row.active"), null);
  browser.windowFocus();
  assert.equal(browser.document.activeElement, browser.list);
});

for (const render of [
  { selectedDescendantIdentity: 4, selectedFolder: "Alpha\\Beta" },
  { selectedDescendantIdentity: "Beta", selectedFolder: null },
  { selectedDescendantIdentity: null, selectedFolder: "Alpha\\Beta" }
]) {
  test(`invalid optional descendant state preserves root display: ${JSON.stringify(render)}`, () => {
    const browser = show(tree(), render, { mode: "collapsed" });
    assert.equal(browser.list.querySelector(".seg").textContent, "Apollo");
    assert.equal(browser.list.getAttribute("aria-activedescendant"), null);
  });
}

test("invalid optional child arrays do not recurse", () => {
  const browser = show([node("Alpha", [], { hasChildren: true, expanded: true, children: null })]);
  assert.equal(browser.list.querySelectorAll(".sub").length, 1);
  assert.equal(sub(browser, "Alpha").querySelector(".aff").getAttribute("aria-expanded"), "true");
  assert.deepEqual(browser.messages, []);
});

test("selected descendant in a different pending row does not claim active state", () => {
  const children = tree();
  children[0].selected = true;
  const browser = show(children, { selectedDescendantIdentity: "Alpha", selectedFolder: "Alpha" }, { pendingIdentity: "other" });
  assert.equal(browser.list.querySelector(".sub.active"), null);
  assert.equal(browser.list.getAttribute("aria-activedescendant"), null);
});

test("no selected descendant retains root active state", () => {
  const browser = show(tree(), { selectedDescendantIdentity: "unknown", selectedFolder: "unknown" });
  assert.ok(browser.list.querySelector(".row.active"));
  assert.equal(browser.list.querySelector(".sub.active"), null);
});
