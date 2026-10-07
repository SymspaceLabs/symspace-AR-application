# Automatic AR configuration of AI-generated product GLBs (scale, pivot, orientation, category, config sidecar, QA) — as of 2026-10-07

Scope note: this covers only the automation of category/AR-mode classification, real-world size, pivot/orientation normalisation, config encoding for a Unity 6000.2 + AR Foundation 6.2 + UnityGLTF client, and automated QA. The choice of image-to-3D model and the GLB/decimation/KTX2 pipeline are covered in an earlier report and are not repeated.

Access limitations during this research (important for the writer): the research proxy blocked arxiv.org, huggingface.co, alphaxiv.org, modelviewer.dev, help.shopify.com, orient-anythingv2.github.io, tessl.io and maverickframe.com. For those, only search-engine snippets or GitHub mirrors could be read; every such item is marked "(snippet only)" or "(not fetched)". Claims marked "(from memory, not verified this session)" are well-known spec facts the writer should treat as needing a quick confirmation.

---

## Key Question 1 — Category / AR-mode classification (what is it, and which anchor does it belong to?)

### Takeaway
There is no off-the-shelf "AR placement classifier"; the practical, proven route is a 2D vision-language model (Qwen-VL family or similar) prompted with a fixed taxonomy and constrained to a JSON schema, run on the product photo (and optionally on 4–6 rendered views of the mesh as a cross-check). 3D-native zero-shot classifiers (ULIP-2, OpenShape, Uni3D) exist and are open, but their fine-grained accuracy (~47–51 % top-1 on the 1,156-class Objaverse-LVIS) is far below what a product-photo VLM achieves on a 6-way AR-mode taxonomy, so they are better used as a secondary signal than as the primary classifier.

### Cited Findings
- Qwen-VL in Roboflow Workflows supports "Structured Output Generation (structured-answering)" returning JSON with specified fields, plus single-label and multi-label classification tasks — [Roboflow: Qwen-VL block](https://docs.roboflow.com/workflows/blocks/blocks/run-a-model/qwen-vl) (snippet only).
- Roboflow's docs state that "for maximum quality, segmentation and grounding tasks benefit from Qwen3-VL models, while classification, depth, OCR, and VQA tasks benefit from Qwen3.5 models" — [Roboflow: Qwen 3.6 API block](https://docs.roboflow.com/workflows/blocks/blocks/run-a-model/qwen3-6-api) (snippet only; implies Qwen3.5/3.6-VL generations are current in 2026).
- Qwen3-VL-Seg constrains the model to "a deterministic JSON schema" with `bbox_2d`, `label`, and mask placeholder fields, showing the Qwen3-VL family is routinely driven to fixed-schema JSON — [arXiv 2605.07141](https://arxiv.org/pdf/2605.07141) (snippet only).
- A community benchmark of Qwen VLMs as "zero-shot image→JSON labelers" reports that getting a VLM to "emit exactly a specific schema every time ... with no markdown fences is a different problem" and that grammar-constrained decoding "can lift the schema-valid rate by 2–27 points" — [HF: qwen-vlmbench](https://huggingface.co/AbstractPhil/qwen-benchmark) (community source, snippet only).
- 3D-native zero-shot: ULIP-2 reports 50.6 % top-1 on Objaverse-LVIS (1,156 classes), 84.7 % on ModelNet40; OpenShape reports 46.8 % on Objaverse-LVIS and 85.3 % on ModelNet40; Uni3D evaluates on the same 46,832-shape Objaverse-LVIS split and compares against PointCLIP, PointCLIP V2, ULIP and OpenShape — [ULIP-2 (CVPR 2024)](https://arxiv.org/pdf/2305.08275); [OpenShape](https://arxiv.org/pdf/2305.10764); [Uni3D](https://arxiv.org/pdf/2310.06773) (snippets only; Uni3D's own headline number was not confirmed).
- FiftyOne ships a `fiftyone.utils.qwen3_vl` integration for running Qwen3-VL over image datasets, useful for batch classification/labeling — [Voxel51 docs](https://docs.voxel51.com/api/fiftyone.utils.qwen3_vl.html) (snippet only).

### Inferences
- Recommended taxonomy prompt design: give the VLM an enumerated list of AR modes with one-line definitions (`floor_horizontal`, `table_horizontal`, `wall_vertical`, `ceiling`, `wear_hand_finger`, `wear_wrist`, `wear_face_glasses`, `wear_ears`, `wear_neck`, `wear_head_top`, `wear_feet`, `garment_body`), ask for `{product_category, ar_mode, confidence, reasons}` as JSON, and enforce the schema with constrained decoding (vLLM / llama.cpp GBNF / Outlines) rather than trusting free-form JSON, per the 2–27-point schema-validity gain noted above.
- Run the classifier twice — once on the input photo (highest signal: packaging, scale cues, context) and once on a 4-view render grid of the generated mesh — and require agreement; disagreement is a cheap, high-precision flag for human review.
- 3D-native encoders (ULIP-2/Uni3D) are most useful not for the 6-way AR mode but for a sanity check that the generated geometry still looks like the photographed category (e.g. the generated "ring" is still nearest the text embedding "ring" rather than "bracelet"), which catches image-to-3D hallucinations.
- "Table vs floor" is not a geometric property; it depends on size. Decide it from the estimated height after Q2 (e.g. < 0.5 m tall → table-capable), not from the classifier alone.

### Gaps
- No paper or benchmark was found that measures VLM accuracy specifically on an AR-anchor/placement taxonomy; accuracy claims for the 6-way task would have to be established with an internal gold set.
- Qwen3-VL's official structured-output docs and the InternVL3/3.5, Gemma 3, Llama 4, Florence-2 and SigLIP comparisons could not be fetched (huggingface.co and arxiv.org blocked); no head-to-head numbers are reported here.
- ShapeLLM / PointLLM were not researched within the tool budget.

---

## Key Question 2 — Real-world size estimation when the image has no metadata

### Takeaway
Single-image metric size is formally ill-posed; the 2026 Metric VQA benchmark shows VLMs answer largely from category priors plus apparent size, so a VLM dimension guess is roughly a "smart category prior", not a measurement. The accurate-enough ordering for AR try-on is: (1) e-commerce metadata if it exists, (2) category-specific priors tuned per wearable (ring inner diameter, watch case width, glasses frame width, shoe length), (3) a VLM estimate used only as a tie-breaker or for furniture-class objects where ±15 % is acceptable, (4) metric depth (Depth Pro / Depth Anything 3 / UniDepth v2 / Metric3D v2) only when the photo contains a real scene with a known camera, which product packshots usually do not.

### Cited Findings
- Metric VQA (June 2026) "formalizes metric object-size estimation as visual question answering": given one RGB image and a query like "How tall is this microwave?", the model must answer in centimetres. It has 10,813 queries: an Objectron-derived split of 10,482 queries over 9 everyday categories with 3D-box ground truth, and an in-the-wild split of 331 tape-measured queries. The authors frame it as "an ill-posed diagnostic setting ... because physical size cannot be determined from a single uncalibrated image, models must rely on imperfect cues including category priors, target appearance, local context, apparent image size, and scene geometry", and evaluate 12 open-weight VLMs with counterfactual analysis of six evidence channels — [arXiv 2606.24335 "Ill-Posed by Design: Probing Evidence Use in VLMs"](https://arxiv.org/pdf/2606.24335) (snippet only; per-model error numbers could not be read).
- Other VLM spatial-measurement benchmarks that include metric object size: ReVSI Bench (which subsamples "objects with dominant sizes" to stop models relying on semantic priors, with valid size ranges in cm) — [arXiv 2604.24300](https://arxiv.org/pdf/2604.24300); Q-Spatial Bench (reference-object reasoning paths improve quantitative spatial estimates) — [arXiv 2409.09788](https://arxiv.org/html/2409.09788v1); "Seeing Is Not Measuring: Tool-Augmented Metric Spatial Reasoning for VLMs" — [arXiv 2609.29073](https://arxiv.org/pdf/2609.29073); "How Far are VLMs from Visual Spatial Intelligence?" — [arXiv 2509.18905](https://arxiv.org/pdf/2509.18905) (all snippets only).
- Depth Pro (Apple, Oct 2024) is "zero-shot metric monocular depth" with "absolute scale without relying on camera intrinsics metadata", includes "a direct field-of-view estimation module", and produces a 2.25 MP depth map in 0.3 s on a standard GPU — [arXiv 2410.02073](https://arxiv.org/abs/2410.02073); code [apple/ml-depth-pro](https://github.com/apple/ml-depth-pro).
- Depth Anything 3 (Nov 2025) "provides canonical metric depth; multiplying by focal length gives metric depth". DA3-metric reports ETH3D δ1 = 0.917 / AbsRel = 0.104 vs UniDepth v2 δ1 = 0.863, and SUN-RGBD AbsRel = 0.105; UniDepth v1/v2 remain best on NYUv2 and KITTI; Metric3D v2 claims first place on several zero-shot metric-depth benchmarks — [Depth Anything 3, arXiv 2511.10647](https://arxiv.org/pdf/2511.10647); [Metric3D v2, arXiv 2404.15506](https://arxiv.org/html/2404.15506v3); [DA3 metric weights on HF](https://huggingface.co/APaul1/DA3METRIC-LARGE) (snippets only). MoGe-2 (July 2025) also claims "metric scale" monocular geometry — [arXiv 2507.02546](https://arxiv.org/pdf/2507.02546).
- Amazon Berkeley Objects (ABO, CVPR 2022): 147,702 product listings, 398,212 catalog images, "up to 18 unique metadata attributes (category, color, material, weight, dimensions, etc.) per product", 7,953 products with artist-made 3D meshes; licence CC BY-NC 4.0 — [ABO paper](https://arxiv.org/abs/2110.06199v1); [Amazon Science page](https://amazon.science/publications/abo-dataset-and-benchmarks-for-real-world-3d-object-understanding).
- Hunyuan3D-2's post-processor rescales output vertices as `scale = max(||v − center||) × 2.0` then `vertices × (scale_factor / scale)`, i.e. the mesh leaves the generator normalised to a unit-diameter sphere and carries no metric scale — [Hunyuan3D-2 postprocessors.py (HF Space mirror)](https://huggingface.co/spaces/kp-forks/Hunyuan3D-2/blame/main/hy3dgen/shapegen/postprocessors.py) (snippet only).
- Hunyuan3D 2.1 paper documents that training data is normalised: "calculating the axis-aligned bounding box ... then applies uniform scaling to fit the object within a unit cube centered at the origin" — [arXiv 2506.15442](https://arxiv.org/pdf/2506.15442) (snippet only).
- A commercial "Product Photo to 3D Model with Real-World Scale" actor on Apify exists (toolsheder/photo-to-3d), evidence that vendors are selling exactly this scale-recovery step, though its method is undocumented — [Apify listing](https://apify.com/toolsheder/photo-to-3d) (not verified; marketing).

### Inferences
- For wearables the acceptable error is tight (a ring 10 % too large reads as the wrong size; glasses 10 % too wide look cartoonish), so the authoritative dimension should come from a per-category prior table keyed on the classifier's fine category, with the VLM asked only to pick a *size class* (e.g. "women's/men's ring", "38/42/46 mm watch", "narrow/medium/wide frame") rather than a number in mm. For furniture/decor (floor, table, wall modes), a VLM numeric estimate with ±15–20 % error is tolerable for a preview, and the Metric VQA framing implies that is roughly what to expect.
- Metric depth models are the right tool only when a *scene* photo with a known/estimable focal length is available (e.g. the user photographs the object in their room). Depth Pro's built-in FOV estimation makes it the most practical choice for EXIF-less photos; object height = depth × pixel-height / focal length (pixels). For clean white-background packshots there is no scene geometry to exploit and these models add nothing.
- ABO's dimension metadata is the best open source of real product dimension *statistics* per category, but its CC BY-NC licence means the derived prior table should be treated carefully for a commercial product (compute statistics offline and ship only the aggregated numbers, and get a legal read).
- Encode the provenance of the dimension in the config (`size_source: metadata | category_prior | vlm_estimate | metric_depth`) and the confidence, so the Unity client can allow user resize (`ar-scale="auto"` semantics) when confidence is low and lock scale when metadata was used.

### Gaps
- Per-model numeric error rates on Metric VQA (and how Qwen3-VL / InternVL3.5 / Gemma 3 rank) could not be read because arxiv/HF pages were blocked.
- No benchmark was found that evaluates VLM size estimates specifically on packshot-style product images; the closest are Objectron-derived scene images.
- Standard-size tables (ISO 8653 ring sizes, typical watch case diameters, eyewear frame widths, shoe last lengths) were not fetched this session; they are well documented (ISO 8653:2016 defines ring circumference; e.g. US size 7 ≈ 17.3 mm inner diameter) but should be confirmed and sourced before being hard-coded (from memory, not verified this session).
- Apple Object Capture dimension output and "SizeGAN" were not researched within the tool budget.

---

## Key Question 3 — Orientation / canonical pose (up axis, front axis) for generated meshes

### Takeaway
Generated meshes are *not* arbitrarily oriented: TRELLIS bakes a fixed Z-up→Y-up rotation into GLB export and Hunyuan3D outputs are generator-canonical, so the biggest win is to (a) pin down each generator's convention empirically with a gold set, (b) run an image-space orientation model (Orient Anything V1/V2, open, CC-BY-4.0) on the *input photo* to learn the camera-relative front, and (c) fall back to geometric upright/OBB heuristics only for the residual. Pure PCA/OBB is unreliable for the front axis on symmetric products; learned upright estimators (Upright-Net family) are open but trained on limited category sets.

### Cited Findings
- TRELLIS's `postprocessing_utils.to_glb()` rotates vertices with `vertices = vertices @ np.array([[1, 0, 0], [0, 0, -1], [0, 1, 0]])` before building the trimesh, i.e. a fixed Z-up→Y-up conversion, and performs no centering or normalisation at export — [microsoft/TRELLIS postprocessing_utils.py](https://raw.githubusercontent.com/microsoft/TRELLIS/main/trellis/utils/postprocessing_utils.py). The README documents `to_glb(simplify=0.95, texture_size=1024)` and MIT licence but says nothing about front alignment to the input view — [microsoft/TRELLIS README](https://github.com/microsoft/TRELLIS).
- Hunyuan3D-2 post-processing recentres and scales to a unit-diameter sphere (see Q2 finding); no explicit front-axis convention documented — [postprocessors.py mirror](https://huggingface.co/spaces/kp-forks/Hunyuan3D-2/blame/main/hy3dgen/shapegen/postprocessors.py) (snippet only).
- Orient Anything (ICML 2025; code released 24 Dec 2024) estimates "three rotation angles: azimuth, polar angle, and rotation, plus a confidence score" from a single object image; Small/Base/Large variants of 23.3 M / 87.8 M / 305 M parameters; trained on "2M rendered labeled images" of 3D models whose front faces were annotated by a pipeline; licence CC-BY-4.0 — [SpatialVision/Orient-Anything](https://github.com/SpatialVision/Orient-Anything); [paper abstract](https://arxiv.org/abs/2412.18605v1); [HF demo](https://huggingface.co/spaces/Viglong/Orient-Anything).
- Orient Anything V2 (NeurIPS 2025 Spotlight; code 12 Dec 2025) "extends ... to handle objects with diverse rotational symmetries and directly estimate relative rotations", claiming SOTA zero-shot on "orientation estimation, 6DoF pose estimation, and object symmetry recognition across 11 widely used benchmarks" — [arXiv 2601.05573](https://arxiv.org/abs/2601.05573) (snippet only); repo [SpatialVision/Orient-Anything-V2](https://github.com/SpatialVision/Orient-Anything-V2) (~220 stars per [gittrend](https://gittrend.io/repo/SpatialVision/Orient-Anything-V2)).
- Upright-Net (CVPR 2022) "formulates upright orientation estimation as a classification task to extract points on a 3D model that form the natural base, and then the upright orientation is computed as the normal of the natural base"; code at [xufangpang/uprightnet-cvpr2022](https://github.com/xufangpang/uprightnet-cvpr2022) — [CVPR 2022 paper](https://openaccess.thecvf.com/content/CVPR2022/papers/Pang_Upright-Net_Learning_Upright_Orientation_for_3D_Point_Cloud_CVPR_2022_paper.pdf). Upright-Net+ adds a "Global Positional Encoding Module using Relative Distance Histogram Statistics" — [SMU repository](https://ink.library.smu.edu.sg/sis_research/10537). UprightRL treats it as RL sequential decision-making — [Eurographics DL](https://diglib.eg.org/handle/10.1111/cgf14419).
- "Symmetry-Robust 3D Orientation Estimation" (arXiv 2410.02101, Oct 2024) addresses canonical orientation of shapes with symmetries — [arXiv 2410.02101](https://arxiv.org/pdf/2410.02101) (title only; content not fetched).
- trimesh exposes `bounding_box_oriented` (minimum-volume OBB with transform + extents), `apply_obb()` (re-poses mesh so its AABB is centred at origin with OBB dimensions) and `principal_inertia_transform` (moves principal inertia axes onto X/Y/Z, centroid at origin) — [trimesh features list](https://hackmd.io/@py5coding/trimesh-features-list.md) and [trimesh.parent source mirror](https://docs.flexcompute.com/projects/tidy3d/en/stable/_modules/trimesh/parent.html).
- glTF 2.0 specification: right-handed, +Y up, "the front of a glTF asset faces +Z", units metres — [glTF 2.0 spec, Coordinate System and Units](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#coordinate-system-and-units) (from memory, not re-fetched this session).
- Khronos 3D Commerce guidelines: "The product should be positioned upright and forward for the best presentation" — [RealtimeAssetCreationGuidelines.md](https://github.com/KhronosGroup/3DC-Asset-Creation/blob/main/asset-creation-guidelines/RealtimeAssetCreationGuidelines.md).

### Inferences
- Practical pipeline: (1) assume generator Y-up after export (TRELLIS proven above; Hunyuan3D by convention but verify); (2) run Orient Anything V2 on the input photo to get the object's azimuth relative to the camera; because image-to-3D generators condition on that same photo, the mesh front is normally the generator's canonical front rotated by the inverse of that azimuth — calibrate this mapping once per generator on a labelled gold set of ~50 assets; (3) apply only yaw (rotation about Y) to put the front on +Z (glTF convention) so the asset is "upright and forward"; (4) use OBB / Upright-Net only if the gold-set calibration shows the generator's up axis is inconsistent for a category.
- OBB/PCA alone cannot disambiguate front vs back (180° ambiguity) or distinguish a ring's axis from its "front"; combine with the image-based orientation to resolve the sign, and for rotationally symmetric categories (rings, round tables, lamps) mark `front_axis: "symmetric"` in the config so Unity does not try to face the user.
- For wearables, "front" must be defined per category relative to the body anchor (glasses: lenses face +Z, temples extend −Z; ring: hole axis along the finger; shoe: toe +Z). Encode this as an explicit attachment rotation in the config rather than assuming the glTF +Z front suffices.

### Gaps
- Whether TRELLIS.2 / Pixal3D / Hunyuan3D-2.1 outputs are aligned so the input-photo view direction equals a fixed mesh axis (e.g. −Z looking at +Z face) was not confirmed from docs; it must be measured empirically.
- Orient Anything's accuracy on *rendered views of generated meshes* (vs photos) and V2's symmetry classes could not be read (project page / arXiv blocked).
- Upright-Net's training categories (ModelNet40-style) and robustness to jewellery/eyewear are unknown; expect poor results on thin, open shapes.
- DeepUpright, ConDor, Canonical Capsules, "upright orientation via tensor rank" were not researched within budget.

---

## Key Question 4 — Pivot conventions and automatic pivot normalisation

### Takeaway
Industry convention (Khronos 3D Commerce, Shopify, Amazon, model-viewer) is bottom-centre at the origin for floor/table objects, back-centre (−Z face) for wall objects, and top-centre for ceiling objects, in metres, baked into vertices rather than carried as node transforms. For wearables there is no public cross-vendor standard; define a per-category attachment frame (origin + rotation) in the config and compute it geometrically (ring hole axis, glasses bridge, shoe sole/heel).

### Cited Findings
- Khronos 3D Commerce Realtime Asset Creation Guidelines v1.0: "The size of the model must be 1:1 the size of the real item. In glTF 1 unit = 1 meter"; "The general recommendation is for the bottom center of the product to be placed at 0,0,0"; "Different origins may be appropriate depending on the product category and where it should be anchored in the real world, for example top-center for a ceiling-mounted product"; "Both glTF and USDz use the right-handed coordinate system" — [RealtimeAssetCreationGuidelines.md](https://github.com/KhronosGroup/3DC-Asset-Creation/blob/main/asset-creation-guidelines/RealtimeAssetCreationGuidelines.md); announced by [Khronos blog](https://www.khronos.org/blog/3d-commerce-working-group-releases-real-time-asset-creation-guidelines-to-assist-artists-create-efficient-reliable-models-for-retail-and-e-commerce).
- Google `<model-viewer>` `ar-placement`: "Selects whether to place the object on the floor (horizontal surface) or a wall (vertical surface) in AR. The back (negative Z) of the object's bounding box will be placed against the wall and the shadow will be put on this surface as well." Allowed values `floor | wall`, default `floor` — [model-viewer docs.json source](https://github.com/google/model-viewer/blob/master/packages/modelviewer.dev/data/docs.json).
- Shopify 3D model checklist: model must be real-world scale in metres per the glTF standard; "The origin should be at the base, centred, so the model sits on the floor when placed, not hover or sink into it" — [Shopify Partners 3D model standards checklist](https://help.shopify.com/partners/resources/creating-3d-models/3d-model-standards-checklist) (snippet only; page blocked).
- Amazon AR View / 3D asset requirements (as summarised by a vendor): "Pivot at 0,0,0; floor models rest on the Y=0 plane, centred at the base in X and Z"; 200,000-triangle cap; Khronos glTF-Validator must pass; no animation, cameras, lights — [cgifurniture.com summary](https://cgifurniture.com/blog/glb-files-for-amazon-update/) (secondary source; Amazon's own spec page was not fetched).
- trimesh provides `bounding_box_oriented`, `apply_obb`, `principal_inertia_transform` (see Q3) — [trimesh features](https://hackmd.io/@py5coding/trimesh-features-list.md).
- UnityGLTF supports import plugins with per-node callbacks, e.g. `OnAfterImportNode(Node node, int nodeIndex, GameObject nodeObject)` and `OnAfterImportScene`, enabling runtime post-processing of pivots — [UnityGLTF README](https://github.com/KhronosGroup/UnityGLTF); callback signature per [realvirtual API doc of a UnityGLTF plugin](https://realvirtual.io/apidoc/classrealvirtual_1_1realvirtual_import_plugin_context.html).
- Lens Studio uses centimetres and offers a "Convert meters to centimeters" option on glTF import because "the units for linear distances in glTF are meters" — [Snap developer docs: glTF import](https://developers.snap.com/lens-studio/assets-pipeline/3d/importing-content/gltf-import).

### Inferences (recommended algorithm; community/engineering practice, not from a single source)
1. Load with `trimesh.load(path, force='mesh')` (or keep the scene and concatenate), drop components below a volume/vertex-count threshold from `mesh.split(only_watertight=False)` to remove floaters, keep the largest connected set.
2. After orientation (Q3), compute AABB; for `floor/table`: translate so `min_y = 0`, and `x,z` centre of the *support polygon* (convex hull of vertices with `y < min_y + ε`, ε ≈ 0.5 % of height) is at the origin, not the AABB centre — this prevents a chair with a protruding backrest from being off-centre. Check static stability: project `mesh.center_mass` onto XZ and test containment in the support hull; if outside, flag `unstable_base` for review (a tipped-over generation).
3. For `wall`: translate so the back face (`max` or `min` Z after making −Z the wall side, matching model-viewer) is at `z = 0`, X centred, Y centred (or Y at bottom if the object hangs). Record `wall_normal: [0,0,-1]`.
4. For `ceiling`: top-centre at origin (`max_y = 0`) per the Khronos note.
5. For wearables: compute an attachment frame instead of moving the mesh pivot to a surface: ring — fit the through-hole axis (e.g. `trimesh.intersections.mesh_plane` sections at several heights, take the inner contour's circle fit; axis = hole axis, origin = hole centre, record `inner_diameter_m`); glasses — origin at the bridge midpoint (top-centre of the AABB front face), rotation so lenses face +Z; earrings — origin at the hook top; necklace — origin at the clasp/top-centre; watch — origin at the case back centre; shoes — origin at the heel bottom-centre with toe +Z, and record `foot_length_m`.
6. Bake the final transform into vertices (`mesh.apply_transform`) and export with an identity root node. Reasons: downstream tools (gltf-transform decimation/KTX2) and UnityGLTF both preserve node transforms, but a baked mesh avoids ambiguity about whether a Unity script should read `transform.localScale` or the mesh bounds, and keeps `Renderer.bounds` meaningful at load.
7. Coordinate handoff: glTF is right-handed Y-up, Unity left-handed Y-up. UnityGLTF converts by negating X (`CoordinateSpaceConversionScale = (-1,1,1)` in `SchemaExtensions`) so glTF +Z front remains Unity +Z forward (from memory of the UnityGLTF source, not verified this session — confirm in `Runtime/Scripts/SchemaExtensions.cs`). Either way, specify all config vectors in *glTF space* and convert once in Unity.
8. Gotcha: if any parent node carries scale, UnityGLTF applies it to the GameObject hierarchy; a Unity-side "fit to size" script must then read world-space `Renderer.bounds`, not `MeshFilter.sharedMesh.bounds`.

### Gaps
- Wayfair, Target and IKEA pivot/orientation specs were not fetched (no accessible primary page found in budget); the Khronos guidelines were co-authored with Wayfair and Target and can stand in for them.
- No open-source library was found that implements wearable-specific attachment-frame extraction (ring hole fitting, glasses bridge detection); this will need custom code.
- Blender `bpy` headless equivalents (origin-to-geometry, apply transforms) were not documented here but are standard.

---

## Key Question 5 — Encoding the configuration so Unity can apply it at runtime

### Takeaway
Use a versioned sidecar JSON as the source of truth (schema-validated in the pipeline) and mirror the same object into the glTF root `extras` so the GLB is self-describing; read it in Unity through a UnityGLTF import plugin (`GLTFImportPluginContext` callbacks) or by deserialising the sidecar into a plain C# record. ScriptableObjects are appropriate only for editor-authored defaults (category prior tables, anchor mappings), not for per-asset runtime data. Naming should follow the existing public vocabularies (model-viewer `floor|wall`, Apple anchor types `horizontal|vertical|face|image|object`).

### Cited Findings
- glTF 2.0 allows `extras` (application-specific JSON) on every object, and custom extensions via `extensions`/`extensionsUsed` — [glTF 2.0 spec](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) (from memory, not re-fetched).
- UnityGLTF: recommended for "Unity 2021.3+, Unity 2022.3+, Unity 6+" (LTS only); supports runtime import/export; extensibility through import plugins (`GLTFImportPlugin` ScriptableObject holding settings + `GLTFImportPluginContext` with callbacks such as `OnAfterImportScene`), custom extensions by implementing `GLTF.Schema.IExtension`, and plugins "serialized as part of the GLTFSettings asset", enabled under Project Settings > UnityGLTF — [KhronosGroup/UnityGLTF README](https://github.com/KhronosGroup/UnityGLTF). Per-node callback `OnAfterImportNode(Node node, int nodeIndex, GameObject nodeObject)` is "called after each node is imported" and third-party plugins use it to "reconstruct components from extras data" — [realvirtual plugin API doc](https://realvirtual.io/apidoc/classrealvirtual_1_1realvirtual_import_plugin_context.html).
- Unity's own glTFast (`com.unity.cloud.gltfast` 6.x) documents a dedicated "Use case: custom extras" via its Add-on API, importing "custom data ... in the extras property of a glTF JSON object in nodes and other glTF elements" — [glTFast 6.9 docs](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.9/manual/UseCaseCustomExtras.html) (alternative loader if UnityGLTF's extras path proves awkward).
- `<model-viewer>` AR vocabulary: `ar-placement` (`floor|wall`, default floor), `ar-scale` (`auto` = user may pinch-resize, `fixed` = locked 100 %), `ar-modes` (`webxr scene-viewer quick-look`) — [model-viewer docs.json](https://github.com/google/model-viewer/blob/master/packages/modelviewer.dev/data/docs.json).
- Apple AR Quick Look / Reality Composer anchor types (WWDC19 session 612): "Every scene has an anchor type, which describes how the model should be placed in the world" — horizontal (tables, floors), vertical (walls, added iOS 13), image, face (TrueDepth), and object; AR Quick Look lets developers "disable content scaling so that users will always view the object at the correct and intended size" — [WWDC19 "Advances in AR Quick Look"](https://developer.apple.com/videos/play/wwdc2019/612). USD/USDZ default linear unit is centimetres (`metersPerUnit = 0.01`) — [OpenUSD docs](https://openusd.org/release/api/group___usd_geom_linear_units__group.html) (from memory, not fetched).
- Snap Lens Studio: centimetre units; some imported models "may have been normalized to have a scale of 1,1,1 before importing, which means they may not match the actual size in centimeters" — [Snap community thread](https://community.snap.com/snapar/discussion/comment/3035) and [glTF import docs](https://developers.snap.com/lens-studio/assets-pipeline/3d/importing-content/gltf-import).

### Inferences — proposed `*.ar.json` sidecar (mirrored into glTF root `extras.symspace_ar`)
```json
{
  "schema": "symspace.ar-config/1.0",
  "asset_id": "…", "generator": {"name": "trellis2", "version": "…"},
  "units": "m", "coordinate_space": "gltf_rh_yup_front+z",
  "category": {"fine": "ring", "confidence": 0.93, "source": "qwen3-vl"},
  "ar_mode": "wear_hand_finger",
  "placement": {"anchor": "horizontal|vertical|face|body|hand", "surface_offset_m": 0.0,
                "wall_normal": [0,0,-1], "allow_user_scale": false, "allow_user_rotate_y": true},
  "dimensions_m": {"x": 0.0205, "y": 0.0085, "z": 0.0205,
                   "source": "category_prior", "confidence": 0.6},
  "pivot": {"convention": "bottom_center|back_center|top_center|attachment",
            "baked": true},
  "attachment": {"target": "finger_proximal_ring|wrist|face_bridge|ear_left|neck|head_top|foot_left",
                 "offset_m": [0,0,0], "rotation_quat_gltf": [0,0,0,1],
                 "inner_diameter_m": 0.0173, "mirror_for_right": true},
  "front_axis": "+z|symmetric",
  "qa": {"status": "pass|review|fail", "checks": {"watertight": true, "components": 1,
         "stable_base": true, "vlm_views_ok": true}, "gold_set_version": "…"}
}
```
- Unity side: a `GLTFImportPluginContext` subclass reads `root.Extras["symspace_ar"]` in `OnAfterImportScene` (or the sidecar is fetched alongside the GLB) and attaches an `ArPlacementBehaviour` that selects `ARPlaneManager` detection mode (horizontal vs vertical), `ARFaceManager` anchor transform name, the ARKit body joint (`LeftFoot`/`RightFoot`), or the hand-landmark index; the attachment rotation is converted from glTF to Unity once (negate X of position/axis, negate X and W of the quaternion if UnityGLTF's X-flip is confirmed).
- Keep category prior tables and anchor-name maps in a ScriptableObject (editor-authored, versioned); keep per-asset data in JSON so it can be regenerated without a client release.
- Prefer `extras` over a custom `KHR_`-style extension: extras need no `extensionsUsed` declaration, survive gltf-transform/KTX2 passes, and avoid validator warnings about unknown extensions; a vendor extension name (`SYMSPACE_ar_config`) is only worth it if third-party viewers must consume it.

### Gaps
- The exact UnityGLTF property/type for reading root- and node-level extras at runtime (e.g. `GLTFRoot.Extras` as a Newtonsoft `JToken`) could not be confirmed from docs this session (the `Documentation~/ImportPlugins.md` path returned 404); verify in the installed package source.
- Needle's UnityGLTF fork docs, 8th Wall, Niantic Lightship, Spark AR anchor schemas, Shopify's full checklist text, and IKEA/Wayfair/Target spec pages were not fetched.

---

## Key Question 6 — Automated QA / validation of the configuration

### Takeaway
Combine three layers: deterministic geometry checks (glTF-Validator, trimesh watertight/components/bounds, per-category rules such as ring hole diameter and glasses lens count), a VLM render-and-judge pass on a fixed 4–6-view grid (the approach the 3D-generation evaluation literature — GPTEval3D, Eval3D, Hi3DEval, MATE-3D — has converged on), and a gold-set regression suite with ground-truth dimensions/orientation. Note that 2026 work (3D-DefectBench) exists specifically to test how reliable VLM judges are at spotting 3D defects, so VLM verdicts should gate "review", not "pass".

### Cited Findings
- GPTEval3D "utilized GPT-4V to perform pairwise comparisons and established a leaderboard using Elo scoring", using "multi-view renderings to enhance 3D reasoning" — described in [MATE-3D paper, arXiv 2412.11170](https://arxiv.org/pdf/2412.11170) (snippet only).
- MATE-3D (ICCV 2025): 8 prompt categories, 1,280 generated textured meshes, 107,520 human annotations across four dimensions; provides absolute per-sample scores and a learned multi-dimensional evaluator — [ICCV 2025 paper](https://openaccess.thecvf.com/content/ICCV2025/html/Zhang_Benchmarking_and_Learning_Multi-Dimensional_Quality_Evaluator_for_Text-to-3D_Generation_ICCV_2025_paper.html).
- Eval3D (CVPR 2025): "fine-grained, interpretable" evaluation that measures "consistency among various foundation models and tools" (semantic and geometric consistency), with "pixel-wise measurement" and "accurate 3D spatial feedback" — [Eval3D, arXiv 2504.18509](https://arxiv.org/abs/2504.18509v1).
- Hi3DEval (Aug 2025): hierarchical object-level + part-level evaluation, material realism (albedo, saturation, metallicness), "3D-aware automated scoring system based on hybrid 3D representations" using video-based multi-view representations and pretrained 3D features; backed by Hi3DBench with a multi-agent annotation pipeline — [arXiv 2508.05609](https://arxiv.org/html/2508.05609v1).
- "Towards Fine-Grained Text-to-3D Quality Assessment: A Benchmark and…" (Sept 2025) — [arXiv 2509.23841](https://arxiv.org/html/2509.23841v2); "3D-DefectBench: A Controlled Factorial Study of…" (July 2026) studies VLMs "used as automated judges that inspect multi-view renders together with the generation prompt" — [arXiv 2607.10826](https://www.alphaxiv.org/abs/2607.10826) (titles/snippets only).
- Khronos glTF-Validator is required by Amazon before submission — [cgifurniture summary](https://cgifurniture.com/blog/glb-files-for-amazon-update/) (secondary); validator repo: [KhronosGroup/glTF-Validator](https://github.com/KhronosGroup/glTF-Validator).
- Orient Anything outputs a confidence score alongside the three angles — [SpatialVision/Orient-Anything](https://github.com/SpatialVision/Orient-Anything) — usable as an orientation-QA signal on rendered views.

### Inferences (recommended QA battery)
- Structural: `gltf-validator` clean; `mesh.is_watertight` (informational for generated meshes, often false), `len(mesh.split()) == 1` after floater removal, AABB extents within the category's plausible range (e.g. ring 12–25 mm inner diameter, glasses 120–160 mm wide, shoes 200–330 mm long, sofa 1.2–3.5 m wide); non-degenerate faces; texture presence.
- Per-category geometry rules: ring — exactly one through-hole (genus check via Euler characteristic `V − E + F = 0` for a torus-like watertight mesh, or ray-cast through the fitted axis must not hit geometry) with diameter in range; glasses — two lens regions detectable as symmetric about the YZ plane and two temple components extending −Z; shoes — flat sole plane (fraction of vertices within ε of `min_y` above a threshold); wall art — thickness/width ratio below a threshold.
- Orientation: render the normalised mesh from the +Z camera; run Orient Anything on that render and assert azimuth ≈ 0 ± tolerance with confidence above threshold; for `symmetric` assets skip.
- Reprojection: render the mesh from the Orient-Anything-estimated input-photo view, compute mask IoU / silhouette Chamfer against the segmented input image (rembg or SAM mask); low IoU flags a wrong front or a bad generation.
- VLM judge: 4-view grid (front/back/left/right at eye level plus top and bottom for floor objects) with a JSON rubric: `is_upright`, `front_matches_photo`, `looks_like_category`, `visible_defects[]`, `plausible_scale_given_reference` (render next to a reference cube of known size). Treat "fail" as a hard block, "uncertain" as human review.
- Gold set: 50–100 assets per AR mode with human-verified dimensions, pivot, and front; run the whole pipeline nightly and track dimension error (%), yaw error (deg), pivot error (mm), and classifier accuracy; fail the build on regression. Store the gold-set version in each asset's `qa` block (see Q5).

### Gaps
- No paper or tool was found that benchmarks automated QA of *AR placement configuration* (scale/pivot/anchor correctness) specifically; the cited work evaluates generation quality, not AR readiness.
- Eval3D / Hi3DEval code availability and licences were not verified (arxiv/HF blocked).

---

## Key Question 7 — Commercial precedents (reference designs)

### Takeaway
Every major commerce AR surface pushes scale/pivot responsibility onto the asset (metres, bottom-centre origin, upright and forward) and exposes only a coarse placement flag (floor vs wall) plus an "allow resize" toggle; none of the public docs found describe automatic scale inference from images, so the automation in this project is novel relative to the published vendor specs.

### Cited Findings
- Khronos 3D Commerce guidelines were "developed by a diverse collaboration of professionals from companies including Autodesk, 3XR, DGG, Samsung, Target, and Wayfair" and are "3D tool-agnostic" with presets for "desktop, mobile, and AR platforms" — [Khronos blog](https://www.khronos.org/blog/3d-commerce-working-group-releases-real-time-asset-creation-guidelines-to-assist-artists-create-efficient-reliable-models-for-retail-and-e-commerce).
- Shopify: real-world scale in metres, base-centred origin (Q4) — [Shopify checklist](https://help.shopify.com/partners/resources/creating-3d-models/3d-model-standards-checklist) (snippet only).
- Amazon AR View: pivot at origin, Y=0 floor rest, 200 k triangles, glTF-Validator; "incorrect dimensions ... or poor model orientation can delay approval" — [cgifurniture.com](https://cgifurniture.com/blog/amazon-ar-view/) (secondary).
- Google model-viewer (used by Shopify's web viewer): `ar-placement floor|wall`, `ar-scale auto|fixed` — [docs.json](https://github.com/google/model-viewer/blob/master/packages/modelviewer.dev/data/docs.json).
- Apple AR Quick Look: anchor types horizontal/vertical/image/face/object; option to disable content scaling for true-size products — [WWDC19 612](https://developer.apple.com/videos/play/wwdc2019/612).
- Snap Lens Studio: centimetre units; metres→centimetres conversion toggle on glTF import — [Snap docs](https://developers.snap.com/lens-studio/assets-pipeline/3d/importing-content/gltf-import).
- Sketchfab offers a "VR/AR editor" for setting AR placement on uploaded models — [Sketchfab help](https://help.sketchfab.com/en/articles/16152315-vr-ar-editor) (snippet only, not read).

### Inferences
- The Unity client's config vocabulary should be a superset of model-viewer's (`floor|wall` → plus `ceiling`, `table`, and wearable anchors) so assets can later be re-exported for web (`<model-viewer>`) or iOS Quick Look (USDZ with anchoring metadata) without re-authoring.
- Mirror model-viewer's `ar-scale` semantics as `allow_user_scale`: true when `dimensions.source` is `vlm_estimate`, false when `metadata`.

### Gaps
- Zakeke, Threekit, VNTANA, Emersya, Wanna, Perfect Corp, 8th Wall, Niantic Lightship, IKEA Place, Wayfair, Target, and Snap AR-shopping docs were not fetched within the tool budget; no primary-source evidence was found that any of them auto-infer scale from imagery.
- Khronos 3D Commerce Viewer Certification and the Amazon official 3D asset specification page were not fetched.
