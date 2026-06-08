I checked the public `drmoisan` GitHub profile and could not actually inspect `drmoisan/TaskMaster`: the profile currently exposes five public repos, none named `TaskMaster`, and a direct open of `drmoisan/TaskMaster` returns 404. So I can’t give you a line-by-line repo review yet; the recommendation below is based on your described TaskMaster design and the current library/documentation landscape.

You did not misunderstand. **LCPN is a strategy**, not a classifier algorithm. In the standard hierarchy literature and in `hiclass`, LCPN means one local **binary** classifier per node; LCPPN means one local **multiclass** classifier per parent node. For a folder tree where each item must land in exactly one leaf, **LCPPN is the better structural match** because every decision is naturally “which child of this parent?” rather than a set of independent nodewise yes/no tests. That uses sibling relationships directly instead of pretending leaves are unrelated.

So my recommendation is not “LCPN + some learner.” It is:

**Replace the flat Naive Bayes leaf classifier with LCPPN, and use one online logistic classifier per parent node, implemented as `SGDClassifier(loss="log_loss", penalty="elasticnet", average=True)` for each parent.** `SGDClassifier` supports `partial_fit`; with `loss="log_loss"` it is logistic regression and exposes probability estimates; and scikit-learn’s own out-of-core text example shows online linear models such as SGD / Passive-Aggressive outperforming MultinomialNB on streaming text classification. For your constraint set, that is the best tradeoff I see between likely F1 improvement and true online updating.

Why this is a better fit than your current Bayesian all-or-nothing setup:

Naive Bayes explicitly assumes conditional independence between features, which is exactly the weakness you called out. In sparse text problems it is fast and often decent, but its inductive bias is crude. A local per-parent online logistic model is discriminative, lets siblings compete directly, and gives you per-step probabilities you can combine along the path. That is a much better match for “pick one valid path through a hierarchy” than a flat NB over leaves.

There is also a major **auto-update** advantage. With scikit-learn incremental learners, the `classes` for `partial_fit` are fixed across calls. In a flat leaf classifier, adding a new folder means changing the global class set. In a per-parent hierarchy, a new child only forces you to rebuild the **affected parent classifier**, not the whole system. That is the strongest practical reason to go hierarchical in your case.

I would also change the feature pipeline if TaskMaster currently updates its vocabulary. `HashingVectorizer` is stateless, explicitly suited to streaming / `partial_fit` pipelines, and keeps the feature space fixed as new text arrives. That makes online updates much cleaner than trying to mutate a `CountVectorizer` / TF-IDF vocabulary over time.

My concrete architecture choice would be:

1. Keep a tree of `parent -> children`.
2. Keep one classifier per parent.
3. On every labeled example, update only the classifiers on the true path from root to leaf via `partial_fit`.
4. At prediction time, descend top-down using probabilities.
5. Use a small **beam search** over path log-probabilities instead of pure greedy descent, so one uncertain early choice does not permanently lock out the correct branch.

That last point is important: plain top-down greedy routing is better than flat NB for your setup, but **LCPPN + beam search** is better than naive greedy LCPPN because it reduces early-routing errors.

If you want a second model to benchmark, use the **Passive-Aggressive equivalent** via `SGDClassifier(loss="hinge", penalty=None, learning_rate="pa1", eta0=1.0)`. Current scikit-learn deprecates `PassiveAggressiveClassifier` itself in favor of that SGD configuration. I would only pick it if it beats log-loss SGD on your actual leaf-level F1, because you lose native probabilities and that makes hierarchical routing less elegant.

I would not keep plain MultinomialNB as the main model. The only Bayesian piece I’d preserve is a **cold-start fallback** for tiny or badly imbalanced sibling sets. In that corner case, `ComplementNB` is the NB variant worth testing: scikit-learn describes it as correcting MultinomialNB’s severe assumptions and being particularly suited to imbalanced text data, and it also supports `partial_fit`. That makes it a good temporary model for a new parent with very few examples, before promoting that parent to online logistic once you have enough data.

So the short recommendation is:

**Best strategy:** LCPPN, not LCPN.
**Best default classifier for it:** `SGDClassifier(loss="log_loss", penalty="elasticnet", average=True)`.
**Best fallback for tiny/imbalanced nodes:** `ComplementNB`.
**Best inference tweak:** beam search over path probabilities.
**Best featureizer for true online updates:** `HashingVectorizer`.

If TaskMaster’s repo becomes reachable from my side, the next useful step is a code-level migration plan from the current flat Bayes implementation to per-parent online classifiers.
