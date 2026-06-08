# Improving TaskMaster’s Leaf-Node Folder Prediction With Hierarchical, Incremental Learning

## Repository context and constraints

You described a TaskMaster workflow where an “all-or-nothing” Bayesian classifier predicts a *leaf* node in a hierarchical folder tree, and the key reason for using Bayes/Naive Bayes is that it can be updated incrementally (e.g., by updating counts) without a full retraining pass. You also highlighted a structural mismatch: in a tree, siblings (mutual-parent subfolders) are semantically related, but the current implementation treats each leaf class as independent.

I attempted to review the repository you referenced (`drmoisan/TaskMaster`) directly, but the repository URL returns a 404 (Not Found) from GitHub.  I also checked the publicly visible repositories for the `drmoisan` account, and TaskMaster does not appear on that listing.  This strongly suggests one of the following: the repo is private, renamed, deleted, or otherwise inaccessible to unauthenticated browsing from my environment. Because of this, I cannot anchor recommendations to specific code modules/classes in TaskMaster; instead, I ground the research in (a) your stated design constraints and (b) well-established hierarchical + incremental text-classification methods that map cleanly onto folder-tree prediction.

Your definition of “better” is higher **leaf-level F1**, while **incremental updating** remains a hard constraint. That combination is important: many high-performing modern text classifiers can achieve strong F1, but they often assume batch retraining or expensive fine-tuning when new labels/data arrive. The goal here is to move to methods that (1) exploit the hierarchy explicitly and (2) can be updated online (or at least warm-started locally) when the user corrects a prediction.

## What hierarchical classification changes mathematically

Hierarchical classification is a standard setting where labels are organized as a tree or DAG and prediction may be constrained to be hierarchy-consistent. A central distinction in the literature is between **flat** approaches (ignore the hierarchy) and **hierarchy-aware** approaches, especially “local” top-down approaches that decompose the problem into smaller decisions along the tree. citeturn22view0

A particularly relevant family for your use case is **local classifier per parent node (LCPN)** and its close cousin **local classifier per node (LCN)**:

- In LCPN, each internal node has a multi-class classifier over *its children*; inference starts at the root and repeatedly selects a child until a leaf is reached.   
- In LCN, each node has a binary classifier (“does this instance belong under this node?”), which can be combined with a top-down decision rule.   

This directly addresses your “siblings are related” observation: instead of treating every leaf as an unrelated class in one big classifier, the model **only compares siblings when it is at their parent**, which is closer to how humans navigate a folder tree.

This also aligns with “all-or-nothing” / abstention behavior. The hierarchical-classification literature explicitly discusses **non-mandatory leaf node prediction** and threshold-based “blocking” (stop descending when confidence is low), which is effectively a hierarchy-aware abstention mechanism.  While you want leaf prediction when confident, this matters operationally because tuning thresholds affects the precision–recall balance and thus F1.

Finally, there is a useful probabilistic view that connects directly to your Bayesian framing: the probability of a leaf can be written as a *product of conditional probabilities along the path* (root → … → leaf). This is the same mathematical idea used by **hierarchical softmax**, where a probability over many classes is decomposed into a sequence of binary decisions along a tree.  In folder terms, this means you can model:

\[
P(\text{leaf}\mid x) = \prod_{(p \rightarrow c)\in \text{path}} P(c \mid p, x)
\]

So rather than “classes are independent,” the model is explicitly **tree-structured**: siblings compete at each parent, and the leaf probability depends on the entire path.

image_group{"layout":"carousel","aspect_ratio":"16:9","query":["local classifier per parent node hierarchical classification diagram","hierarchical softmax tree probability diagram","email folder hierarchy tree diagram"],"num_per_query":1}

## Strong drop-in upgrades to Naive Bayes that respect the hierarchy

If you want a conservative evolution from “flat Naive Bayes over leaves” while keeping the simplicity and incremental updates of count-based Bayes, there are hierarchy-aware Bayesian variants specifically designed for exactly the problem you described.

### Hierarchical Naive Bayes via shrinkage to parent distributions

A classic result (text categorization with topic hierarchies) shows that Naive Bayes can be made significantly more accurate by **shrinking** a data-sparse child’s parameters toward its parent’s parameters—i.e., smoothing that explicitly uses the hierarchy rather than smoothing toward a global background model. citeturn32view0

Conceptually, instead of estimating each leaf’s word distribution only from leaf data, you estimate it as a weighted combination of:

- the leaf’s own maximum-likelihood estimate (from leaf counts), and  
- the parent’s distribution (which itself may be similarly smoothed up the tree).   

This is a very direct fix for “siblings are related”: siblings share a parent distribution, so they share statistical strength when each sibling has limited data.

**Why this matches your incremental-update constraint:** it remains count-based. You keep token counts at each node and update counts along the path when new labeled examples arrive. You do not need to retrain a global model from scratch; you just update counts and recompute smoothed probabilities for affected nodes.

### Hierarchical Dirichlet / hierarchical Bayesian smoothing

A related line of work formulates the hierarchy explicitly as a Bayesian structure where class-conditional feature distributions are “inter-related due to the hierarchical Bayesian structure” (their phrase).  This gives a principled justification for using parent-informed priors (Dirichlet priors over multinomial word distributions) and can be approximated in practical systems as “parent-informed pseudo-counts.”

This can be viewed as: for each node, the word distribution is drawn from a Dirichlet distribution centered on the parent’s distribution; updates from new data shift the posterior in a way that naturally pools information through the tree. citeturn19search1

### Limits of “better Bayes” for maximizing F1

Even with hierarchy-aware smoothing, you are still in a generative-family regime. As a general phenomenon, discriminative models (e.g., logistic regression) often achieve **lower asymptotic error** than generative Naive Bayes, though Naive Bayes can reach its performance plateau faster with small data.  This suggests an important practical takeaway:

- If your folder leaves have **very few** labeled examples per leaf and you rely on rapid personalization, hierarchical Bayes/shrinkage can be a strong improvement while preserving online updates.   
- If you have **moderate** data per internal node / sibling set, you can usually do better (higher F1) with an online-updated discriminative model—especially when you exploit the hierarchy to keep each decision small.   

## A higher-F1 path: hierarchical online discriminative models that still auto-update

The strongest “likely to improve F1” direction under your constraints is: **keep incremental updates**, but switch the per-decision learner away from Naive Bayes to a better online learner—*and* do it in a hierarchy-aware way so you never have to train one huge flat multi-class model.

### Local classifier per parent node with online updates

Using LCPN, you train one classifier at each internal node to pick among its children. This is a standard hierarchical strategy.  It gives you three compounding advantages specifically for your TaskMaster folder tree:

1. **Hierarchy utilization:** siblings are compared directly; distant leaves never compete directly.   
2. **Incremental updates are localized:** when the user corrects a prediction to a leaf, you update only the classifiers along that true path (or even only the classifier at the first mistaken parent), not the whole world. This is the operational analog of hierarchical softmax’s “update only along the path.”   
3. **Class-growth pain is reduced:** adding a new leaf folder only forces you to update the classifier at its parent, not a global K-way classifier. This matters because many incremental APIs require that all classes be known up-front (a known limitation in common “partial fit” interfaces).   

The LCPN structure can be paired with multiple online learners that typically outperform Naive Bayes in text classification:

- **Online logistic regression / linear models trained with SGD** (incremental gradient steps). scikit-learn explicitly positions SGD-based linear models as supporting minibatch/online learning via `partial_fit`.   
- **Passive-Aggressive (PA) online classification**, which is a classic, widely-cited online learning algorithm family designed for incremental updates on streaming data.   

If TaskMaster is implemented in .NET, a particularly relevant point is that entity["company","Microsoft","software company"]’s ML.NET documentation explicitly describes “retraining models using learned model parameters as a starting point” and lists multiple trainers that can be “retrainable,” including OnlineGradientDescent, AveragedPerceptron, LinearSVM, and SGD variants.  Even if you don’t adopt ML.NET wholesale, this is strong evidence that “incremental-ish” updating (warm-start retraining) is supported in the .NET ecosystem without full retraining from scratch.

### Product-of-node logistic models as a hierarchy-consistent probability model

There is also a clean “Bayesian-feeling” discriminative formulation: model each node’s child decision with logistic regression and compute leaf probability by multiplying probabilities along the path (equivalent to the earlier product decomposition). A concrete example of this idea appears in hierarchical discriminative text classification work, where “HierLR” is defined as the product of logistic-regression models at each node, and local-classifier-per-parent is used to make discriminative models feasible at larger scales. citeturn23view0

For TaskMaster, this suggests a very direct architecture:

- One *small* multiclass model per internal folder, predicting among its children.
- Score a leaf by multiplying (or summing log) probabilities along the path.
- Apply your “all-or-nothing” threshold at the path level (or at each parent to enable early stopping).   

### A practical hybrid that often beats both: NB log-count ratios + linear model (NBSVM-style)

If you like Bayes because it gives strong, stable token statistics and very cheap incremental updates, a powerful compromise is to use Bayes as a **feature generator** and then use a better classifier on top.

A widely used and well-supported example is the NBSVM approach (“Naive Bayes log-count ratios as feature values” used inside a linear SVM/logistic model), which the authors report as a robust performer across multiple text classification tasks and datasets.  The key idea is:

- Maintain Naive Bayes style counts (incremental, cheap).  
- Convert a document into transformed features that reflect NB’s “how indicative is this token for class vs not-class?” signal (log-count ratios).  
- Feed those transformed features into a linear classifier (SVM or logistic regression), which often yields significantly stronger decision boundaries than pure NB.   

In a hierarchy-aware TaskMaster design, you don’t even need a single global NBSVM. You can do **NBSVM-at-each-parent**:

- For each parent folder, compute NB log-count ratios for its children using just the examples routed under that parent.
- Train/update a lightweight linear model for that parent’s child prediction.
- Updates remain local, and the Bayes part remains trivially incremental.   

This hybrid is especially attractive if your current implementation already maintains word counts per folder: you can reuse nearly all of that machinery and “swap in” a stronger discriminative decision layer.

## Alternative approach: centroid or embedding prototypes for incremental updates

If you want an approach that is *not* “train a classifier” at all—i.e., avoids even online optimization—you can frame folder prediction as **prototype matching**:

- Represent each class (folder) by a centroid/prototype vector.
- Classify by choosing the nearest centroid, optionally top-down in the tree.
- Update centroids incrementally via a running mean when new labeled examples arrive.

There is published work explicitly adapting centroid-based classifiers to hierarchical classification.  This is important for your constraints because centroid methods are naturally incremental: adding one more labeled example updates only the centroid statistics, not a trained model.

In TaskMaster terms, you can do a hierarchy-aware centroid method:

- Each leaf folder maintains a centroid vector (TF-IDF centroid, hashed n-gram centroid, or embedding centroid).
- Each internal folder maintains a centroid computed from its children (or from all examples under it).
- At prediction time, traverse the tree by choosing the child whose centroid is most similar, repeating until a leaf (or until confidence drops and you abstain).   

The most modern flavor is to use **semantic embeddings** (rather than sparse token vectors). That can improve F1 when folder distinctions are semantic (e.g., “finance” vs “legal” vs “project X”), but it introduces operational considerations: embedding quality, compute cost, and whether embeddings require external APIs (privacy). The centroid-based literature itself is not tied to embeddings, but the incremental math works the same. 

Where this approach tends to shine:

- Fast, stable incremental behavior.
- Very easy handling of new folders (a new leaf starts with its first centroid).
- Often strong performance when classes are “clusterable” in representation space.

Where it can underperform:

- When sibling folders are separated by subtle lexical cues or complex decision boundaries that a linear classifier can learn but centroid similarity cannot.

## Practical integration plan for TaskMaster

Because your goal is *higher leaf F1* under *incremental updates*, the key is to architect the solution so you can improve modeling without destabilizing the product.

### Establish an evaluation that matches “auto-update” reality

Hierarchical folder predictors are usually “trained” from user corrections over time. A realistic evaluation is therefore **time-sliced**: train on earlier examples, evaluate on later examples, and optionally simulate online updates (update after each labeled example). This is the fairest way to measure whether an approach will improve *real F1 under incremental learning*, not just in a random split. 

Also explicitly define how “all-or-nothing” abstentions count in F1 (e.g., treat abstain as “no prediction,” which becomes a false negative for the true class; or evaluate F1 only over non-abstained cases). Threshold choice can move F1 substantially. The hierarchical literature’s “blocking” discussion is essentially about this confidence/abstention trade-off. 

### Recommended implementation path in descending order of “likely F1 gain”

First, implement **LCPN + online linear models** as the mainline candidate. This is the cleanest, most principled match to “siblings are related” and one of the most common hierarchy-aware strategies.  Combine it with a strong incremental learner (SGD logistic regression, PA, or a warm-start retrainable linear model). 

Second, if you want maximum reuse of your current Bayes code, implement **hierarchical shrinkage Naive Bayes** (parent-informed smoothing). It’s a direct correction to your “independent leaves” limitation while preserving pure incremental updating. 

Third, implement **NBSVM-at-each-parent** as a hybrid: keep Naive Bayes counts (incremental) but wrap them in a linear classifier that is often stronger than NB alone.  This is frequently a sweet spot when you want “Bayesian updating” but higher F1.

Fourth, consider **centroid/prototype hierarchical matching** (sparse centroids or embeddings) if you want minimal training complexity and extremely simple incremental updates. citeturn24view0

### Handling taxonomy changes without global retraining

Folder trees are not static. A hierarchy-aware decomposition is especially valuable because you can localize the “blast radius” of changes:

- **New leaf folder under an existing parent:** only the parent’s child-classifier needs to learn a new child label; the rest of the tree is unchanged. This avoids the hard problem that many incremental APIs have: they require the full class list to be declared early (typical for partial-fit style learners). 
- **Reparenting / moving a subtree:** you can preserve learned models within the subtree and only update ancestors on the new path (or rebuild local models where sibling sets changed).

### Why this is likely to improve leaf F1 for TaskMaster’s structure

Your current “flat Naive Bayes over leaves” implicitly forces the model to learn boundaries among *all* leaves at once, and it cannot share strength among siblings except indirectly via token overlap. Naive Bayes also rests on the conditional-independence assumption and tends to be less competitive than discriminative models once you have meaningful data, even though it can learn quickly early on. 

Hierarchy-aware models address both root causes you identified:

- They encode the folder tree directly in the inference procedure (only siblings compete), which is exactly how your class structure is related.   
- They enable principled information sharing (hierarchical Bayes/shrinkage) or higher-capacity decision boundaries (online discriminative models) while remaining incremental. 

The net effect—supported by the hierarchical-classification literature and by strong “hybrid NB + linear” baselines in text classification—is that you can retain auto-updating while moving to a model family that is better aligned with the hierarchical label structure and typically yields higher predictive performance. 