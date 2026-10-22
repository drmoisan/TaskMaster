// Issue 1002: in the QFC selector, every nested subfolder row shows its +/- after the name,
// and each nested level is indented one 16px step further than its visible parent.
import test from "node:test";
import assert from "node:assert/strict";
import { source } from "./issue-997-source-loader.mjs";
import { createBrowser, suggestionRow, selectorView } from "./issue-997-browser-harness.mjs";

function node(identity, children = []) {
  return { identity, displayName: identity, folderPath: `Projects\\Apollo\\${identity}`,
    hasChildren: children.length > 0, expanded: children.length > 0, selected: false, children };
}

// Three expanded levels: Alpha (depth 0) > Beta (depth 1) > Gamma (depth 2), plus a sibling leaf.
function show() {
  const browser = createBrowser(source);
  const tree = [node("Alpha", [node("Beta", [node("Gamma")])]), node("Omega")];
  browser.receive({ type: "render", rows: [suggestionRow({ leafExpanded: true, subfolders: tree })] });
  browser.receive(selectorView());
  return browser;
}

function sub(browser, identity) {
  return browser.list.querySelectorAll(".sub").find((element) => element.id === `folder-option-0-node-${identity}`);
}

test("nested toggle follows the name at every depth", () => {
  const browser = show();
  const withToggle = browser.list.querySelectorAll(".sub").filter((element) => element.querySelector(".aff"));
  assert.equal(withToggle.length, 2, "Alpha and Beta are expandable");
  for (const element of withToggle) {
    const name = element.children.indexOf(element.querySelector(".seg"));
    const toggle = element.children.indexOf(element.querySelector(".aff"));
    assert.ok(name >= 0, "the name is a direct child of the row");
    assert.ok(toggle > name, `the toggle of ${element.id} follows its name`);
  }
});

test("nested rows carry data-depth and per-depth padding", () => {
  const browser = show();
  const expected = [["Alpha", "0", 24], ["Beta", "1", 40], ["Gamma", "2", 56], ["Omega", "0", 24]];
  for (const [identity, depth, padding] of expected) {
    const element = sub(browser, identity);
    assert.ok(element, `${identity} is rendered`);
    assert.equal(element.getAttribute("data-depth"), depth);
    assert.equal(element.getAttribute("style"), `padding-left: ${padding}px`);
  }
});
