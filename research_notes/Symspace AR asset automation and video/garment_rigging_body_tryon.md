# Body-tracked garment try-on: auto-rigging, body fitting, cloth, and the 2D/video alternative (as of 2026-10-07)

Scope note: Unity 6000.2 + AR Foundation 6.2, iOS ARKit body tracking (ARHumanBodyManager, rear camera) with a BoneController, GLB assets via UnityGLTF, no Android body-tracking path. Shoes/jewelry/watches are covered elsewhere; these notes cover garments and anything needing a skeleton.

Research-environment caveat: the egress proxy blocked arxiv.org, huggingface.co, docs.unity3d.com, developer.apple.com (returned header only), developers.google.com / ai.google.dev, developers.snap.com, lightship.dev, meshcapade.com, mpg.de, blog.google, techcrunch.com, businesswire.com and project GitHub-Pages sites. Where only a search snippet was available, the finding is marked "(snippet only, unverified)". GitHub repositories (README/LICENSE) were readable and are the primary sources for licenses below.

---

## KQ1. Automatic rigging of generated garment meshes: which tools, what skeleton, Unity/ARKit compatibility, garment quality

### Takeaway
The open-source state of the art (UniRig, MIT, SIGGRAPH 2025; its successor SkinTokens, Feb 2026) predicts an *arbitrary* skeleton from a mesh, not a Unity-Humanoid/ARKit-named template, so a mapping step (or a template-based tool such as Make-It-Animatable) is still required; none of the surveyed papers evaluates garments-without-a-body, so a product-photo-derived dress mesh is out-of-distribution for all of them and this must be treated as unproven.

### Cited Findings
- UniRig is "the official implementation for the SIGGRAPH'25 (TOG) UniRig framework", developed by Tsinghua University and Tripo (VAST); repository license is MIT; it accepts ".obj, .fbx, .glb, and .vrm"; the skeleton stage is "an GPT-like transformer [that] autoregressively predicts a topologically valid skeleton hierarchy using a novel Skeleton Tree Tokenization scheme"; it handles "diverse model categories (humans, animals, objects)" and produces arbitrary (not Mixamo/humanoid-template) skeletons; inference needs a "CUDA-enabled GPU with at least 8GB VRAM"; skin training needs ≥60 GB on one GPU; known limitation: skinning quality "degrades significantly with inaccurate skeletons"; README contains no mention of Mixamo, humanoid templates, clothes or garments; an optional Blender VRM add-on is provided for .vrm import/export — [UniRig GitHub](https://github.com/VAST-AI-Research/UniRig); [UniRig README](https://raw.githubusercontent.com/VAST-AI-Research/UniRig/main/README.md)
- SkinTokens is announced in the UniRig README as "the powerful successor to UniRig (SIGGRAPH '25)" with "98%–133% improvement in skinning accuracy and 17%–22% improvement in bone prediction over state-of-the-art baselines"; it "unifies both [skeleton prediction and skinning] into a single autoregressive sequence via learned discrete skin tokens" with reinforcement learning and an "Efficient Skinning Compression Module"; the TokenRig transformer "uses Qwen3-0.6B architecture" (snippet only for architecture and the MIT license claim; the Hugging Face model card and arXiv 2602.04805 were blocked) — [UniRig GitHub](https://github.com/VAST-AI-Research/UniRig); [search snippet re VAST-AI/SkinTokens HF card and arXiv 2602.04805](https://huggingface.co/VAST-AI/SkinTokens)
- Make-It-Animatable (CVPR 2025) is "a novel data-driven method to make any 3D humanoid model ready for character animation in less than one second", generating "blend weights, bones, and pose transformations", supports "meshes and 3D Gaussian splats", and claims robustness "even for characters with non-standard skeleton structures" — [Make-It-Animatable arXiv](https://arxiv.org/abs/2411.18197v1); [CVPR 2025 paper](https://openaccess.thecvf.com/content/CVPR2025/papers/Guo_Make-It-Animatable_An_Efficient_Framework_for_Authoring_Animation-Ready_3D_Characters_CVPR_2025_paper.pdf)
- Puppeteer (Aug 2025) "integrates automatic rigging and animation into a unified pipeline", using "auto-regressive transformers featuring joint-based tokenization and hierarchical sequence ordering" for skeletons and "topology-aware joint attention" for skinning — [Puppeteer arXiv HTML](https://arxiv.org/html/2508.10898v1)
- MagicArticulate (CVPR 2025) "automatically transforms static 3D models into articulation-ready assets" and introduces Articulation-XL, "over 33k 3D models with high-quality articulation annotations"; its Hugging Face release is under Apache 2.0 (snippet only) — [MagicArticulate review](https://liner.com/ko/review/magicarticulate-make-your-3d-models-articulationready); [Seed3D/MagicArticulate on HF](https://www.huggingface.co/Seed3D/MagicArticulate/tree/main)
- RigNet (SIGGRAPH 2020) is GPL-3.0; outputs joints, hierarchy and skinning weights as `*_rig.txt` convertible to FBX; training/test meshes were constrained to "between 1K and 5K vertices" (meshes <1K subdivided, >5K simplified) — [RigNet GitHub](https://github.com/zhan-xu/RigNet)
- Tripo offers a closed, hosted auto-rig endpoint (reference only) — [Tripo rig API docs](https://developers.tripo3d.com/en/models/rig)
- Unity's Humanoid retargeting relies on "Humanoid models hav[ing] similar basic structures representing major articulate parts of the body", which "makes it easy to map animations from one humanoid skeleton to another" (Unity Avatar docs; page itself blocked, snippet only) — [Unity Avatar Creation and Setup](https://docs.unity3d.com/Manual/AvatarCreationandSetup.html)

### Inferences
- Because UniRig/SkinTokens/Puppeteer/MagicArticulate emit arbitrary skeletons, a Unity pipeline must either (a) post-process the predicted skeleton into Unity Humanoid bone slots (heuristic name/position matching, then `AvatarBuilder.BuildHumanAvatar` at runtime), or (b) use a template-based approach (Make-It-Animatable targets humanoids) so bone semantics are known in advance. Option (b) is the safer match for ARKit-joint-driven BoneController mapping.
- For garments, the more practical approach is to skip "rigging the garment" entirely and instead transfer skin weights from a canonical rigged body (SMPL-style or a generic Unity humanoid) to the garment by nearest-surface/vertex projection — this is how draped-garment pipelines (GarmentCode, ContourCraft) treat the garment relative to the body. A dress then simply inherits the body's leg weights where it overlaps the legs, which is visually acceptable for rigid/skinned try-on. This is an inference; no surveyed tool documents it as a garment-specific feature.
- GPL-3.0 (RigNet) is incompatible with a closed-source commercial app if linked at runtime; it is fine as an offline server tool only if the obligations are acceptable, but UniRig (MIT) supersedes it technically anyway.

### Gaps
- No surveyed auto-rigging paper reports results on body-less garment meshes (e.g. a standalone dress). Quality on such inputs is unverified.
- SkinTokens license (reported MIT in a search snippet) and exact release date (arXiv 2602.04805 → February 2026) could not be verified because arXiv and Hugging Face were blocked.
- No source found documenting an off-the-shelf converter from UniRig/SkinTokens output to a Unity Humanoid Avatar or to ARKit joint names.
- Anymate, HumanRig, ARMO, Rig3R, DRiVE, "Articulated Kinematics Distillation": no reliable results surfaced in the searches run; not covered. Mixamo auto-rigger 2026 status was not researched.

---

## KQ2. Garment-specific 3D generation from a product photo: garment-only output, mannequin separation, sewing-pattern generators, licenses

### Takeaway
Two open routes exist: (1) sewing-pattern generation from an image via a VLM (ChatGarment, Apache-2.0, built on the MIT-licensed GarmentCode DSL) which yields a simulation-ready garment draped on a parametric body, and (2) generic image-to-3D followed by part segmentation (Hunyuan3D-Part, Tencent community license with EU/UK/Korea exclusion and a 1M-MAU clause); the pattern route gives cleaner, body-relative garments but is slower and currently limited to the garment classes GarmentCode covers.

### Cited Findings
- ChatGarment (CVPR 2025) "finetunes a VLM to produce GarmentCode, a JSON-based, language-friendly format for 2D sewing patterns, enabling both estimating and editing from images and text instructions"; repository license Apache-2.0; outputs "2D sewing patterns and JSON configurations" that are stitched into "draped 3D garments"; built on LLaVA/LISA, "GarmentCodeRC" and "ContourCraft-CG" for simulation refinement; acknowledged limitation: "ChatGarment may occasionally produce garments with incorrect lengths or widths from input images" — [ChatGarment arXiv](https://www.arxiv.org/abs/2412.17811); [ChatGarment GitHub](https://github.com/biansy000/chatgarment)
- GarmentCode is "a modular programming framework for designing parametric sewing patterns", MIT licensed, with a companion body-measurement project (GarmentMeasurements) rather than SMPL; GarmentCodeData is "A Dataset of 3D Made-to-Measure Garments With Sewing Patterns" (v2 September 2024); archived pipeline used "Maya+Qualoth"; v2.0.0 released August 2024 — [GarmentCode GitHub](https://github.com/maria-korosteleva/GarmentCode)
- AIpparel "fine-tunes large multimodal models on a custom-curated large-scale dataset of over 120,000 unique garments, each with multimodal annotations including text, images, and sewing patterns" — [AIpparel paper mirror](https://fugumt.com/fugumt/paper_check/2412.03937v5_enmode). The GitHub repo fetched (georgenakayama/AIpparel) is only a project-page site with no license or code details — [AIpparel GitHub page](https://github.com/georgenakayama/AIpparel)
- Dress-1-to-3 (2025) "reconstructs physics-plausible, simulation-ready separated garments with sewing patterns and humans from an in-the-wild image", combining "a pre-trained image-to-sewing pattern generation model" with "a pre-trained multi-view diffusion model" and refining via "a differentiable garment simulator" — [Dress-1-to-3 arXiv](https://arxiv.org/abs/2502.03449)
- Garment3DGen (2024) synthesizes "3D garment assets from a base mesh given a single input image as guidance"; requires a base garment mesh plus a target geometry "obtained via InstantMesh from RGB images"; output garments "fit on top of parametric bodies and simulate"; depends on nvdiffrast, PyTorch3D, Neural Jacobian Fields; a LICENSE.md exists but its terms were not visible in the fetched page — [Garment3DGen arXiv](https://arxiv.org/abs/2403.18816?context=cs); [Garment3DGen GitHub](https://github.com/nsarafianos/Garment3DGen)
- GarmentCrafter (March 2025, CMU / Texas A&M / Google AR) does single-view 3D garment reconstruction via "progressive depth prediction and image warping to approximate novel views, then trains a multi-view diffusion model to complete occluded and unknown clothing regions", and "can reconstruct both geometry and texture from synthetic garment images" — [GarmentCrafter arXiv](https://arxiv.org/abs/2503.08678v1)
- Hunyuan3D-Part comprises P3-SAM ("native 3D part segmentation… can handle any input mesh") and X-Part ("high-fidelity and structure-coherent shape decomposition"); X-Part recommends "scanned or AI-generated meshes (e.g., from Hunyuan3D V2.5 or V3.0) as input"; "the full X-Part version [is] exclusive to the studio platform" — [Hunyuan3D-Part GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-Part); [Hunyuan3D-Part HF](https://huggingface.co/tencent/Hunyuan3D-Part)
- Hunyuan3D-Part license: "Tencent Hunyuan 3D-Part Community License Agreement"; commercial use permitted, but at 1 million MAU you must "request a license from Tencent, which Tencent may grant to You in its sole discretion"; the agreement "does not apply in the European Union, United Kingdom and South Korea"; requires a Notice file and disclosure of non-affiliation — [Hunyuan3D-Part LICENSE](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-Part/main/LICENSE)
- PartField (NVIDIA, 2025) is "a feedforward approach for learning part-based 3D features… without relying on predefined templates or text-based names", "up to 20% more accurate and often orders of magnitude faster than other recent class-agnostic part-segmentation methods" — [PartField arXiv](https://arxiv.org/html/2504.11451v1)

### Inferences
- For a "product photo → garment" pipeline, ChatGarment (Apache-2.0) + GarmentCode (MIT) + ContourCraft (MIT) is the only fully permissive open chain found that yields a garment already in a canonical draped pose on a body; everything else either depends on non-commercial body models (SMPL) or on licenses with usage/territory clauses (Hunyuan).
- Product photos are typically flat-lay or on-model/mannequin. Image-to-3D of an on-model photo will produce a fused body+garment solid; P3-SAM/PartField can segment it, but the garment back-faces/interior will be missing (the generator saw a solid), so the result is a shell that must be hollowed/thickened. This limitation is an inference from how these generators work; no source tested garments specifically.
- Sewing-pattern generators cover a finite garment vocabulary (GarmentCode's parametric families); unusual silhouettes will be approximated.

### Gaps
- Dress-1-to-3 code availability and license: project page blocked; not verified whether code is released.
- AIpparel code license and whether it drapes on SMPL: not verified (project page only).
- Garment3DGen LICENSE.md contents not visible; Meta research projects of this era are commonly CC-BY-NC but this is unverified.
- Whether TRELLIS.2 / Hunyuan3D v3 can be prompted to generate a garment alone (no mannequin): no source found; untested claim.
- GarmentDreamer, Design2GarmentCode, Sewformer, SewingLDM, DressCode, GarmentX, GarmentDiffusion, GarmentImage: not individually verified in this pass (DressCode paper exists — [DressCode on PapersWithCode](https://paperswithcode.com/paper/dresscode-autoregressively-sewing-and) — but license/outputs not checked).

---

## KQ3. Fitting a garment to a tracked skeleton: SMPL licensing, Unity humanoid retargeting, body measurement from ARKit joints / single photo

### Takeaway
SMPL/SMPL-X model files are non-commercial by default and commercially licensed via Meshcapade (reported €1,500/yr micro to €25,000/yr SME), so a commercial Unity app should either license SMPL or use a generic Unity Humanoid body; open body-estimation code (4D-Humans/HMR2.0, MIT) still needs the SMPL .pkl, and the best clothed/multi-person estimators (Multi-HMR, Sapiens) are non-commercial.

### Cited Findings
- SMPL-X model license is "for academic research purposes… non-commercial scientific research purposes"; commercial licensing "through Meshcapade.com" (sales@meshcapade.com) (snippet only; license page blocked) — [SMPL-X model license](https://smpl-x.is.tue.mpg.de/modellicense.html)
- Meshcapade pricing (snippet only, page blocked): SMPL commercial license "€1,500/year for Micro (individuals, startups, micro enterprises), €25,000/year for SME… or available upon request for Enterprise"; a separate SMPL-X/hand line at "€600/year for Micro, €10,000/year for SME" — [Meshcapade Body Models](https://meshcapade.com/assets/body-models)
- 4D-Humans (HMR 2.0) code is MIT, but "requires the neutral SMPL model file (basicModel_neutral_lbs_10_207_0_v1.0.0.pkl), which must be obtained separately from the official SMPL website" under SMPL's own license; outputs "3D pose and shape" parameters and .obj meshes from images/video; training used "8 A100 GPUs for 7 days" — [4D-Humans GitHub](https://github.com/shubham-goel/4D-Humans)
- Multi-HMR (Naver, 2024) "is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 4.0 license" — [Multi-HMR License on HF](https://huggingface.co/spaces/naver/multi-hmr/blob/bac33d63480e549ddd799f63a7be4f28e0df07b6/Multi-HMR_License.txt)
- Meta's research model licences of this family (e.g. "FAIR Noncommercial Research License") restrict to "Noncommercial Research Uses… not primarily intended for commercial advantage" (general Meta licence finding; Sapiens-specific license not fetched) — [ScanCode licence DB (seamless-2023)](https://scancode-licensedb.aboutcode.org/seamless-2023.yml)
- Unity's Humanoid system: "Humanoid models have similar basic structures representing major articulate parts of the body, making it easy to map animations from one humanoid skeleton to another, allowing retargeting" (snippet only) — [Unity AvatarCreationandSetup](https://docs.unity3d.com/Manual/AvatarCreationandSetup.html)
- A reference implementation of tracking-to-humanoid retargeting exists in Meta's Movement SDK: "OVRUnityHumanoidSkeletonRetargeter… is responsible for retargeting from body tracking bones to third-party humanoid skeletons, allowing you to apply body tracking to characters imported as Unity Humanoids"; the tool "will automatically find known joints and bone transforms on the character, which should be verified and adjusted" — [Meta Movement body tracking docs](https://developers.meta.com/horizon/documentation/unity/move-body-tracking/); [OVRUnityHumanoidSkeletonRetargeter reference](https://developers.meta.com/horizon/reference/unity/v81/class_o_v_r_unity_humanoid_skeleton_retargeter/)
- Industry contrast on measurement-based fit: "Zalando uses detailed body measurements and 3D scans of clothing to predict fit, while Zara uses generative AI to show shoppers how different outfits might look on an avatar resembling them. Zalando's avatars can make shoppers uncomfortable, while Zara's system was more engaging" (secondary source) — [Business Report](https://www.businessreport.com/?p=276402)

### Inferences
- The lowest-risk commercial path is: keep the team's existing ARKit → BoneController mapping, drive a *generic Unity Humanoid* proxy body (e.g. a Mixamo-style or hand-made base mesh under a permissive licence), and bind garments to that proxy. SMPL only becomes necessary if the team wants photo-based body-shape estimation (HMR2.0 etc. output SMPL betas) — in that case budget a Meshcapade licence.
- ARKit's 3D body skeleton gives bone lengths (with an estimated scale factor) but no girth; "size recommendation" from ARKit joints alone is limited to height/limb proportions. Girth needs either a photo-based shape estimator (SMPL betas) or user-entered measurements. (Inference; no source quantifies ARKit measurement accuracy.)
- Meta's retargeter is a usable design reference (source-to-target joint matching with manual verification), not a drop-in — it is tied to the Quest/Movement SDK.

### Gaps
- Apple's ARBodyTrackingConfiguration page returned only a header; the A12+/rear-camera/91-joint facts come from the team's own context and were not independently verified in this pass.
- Sapiens (Meta) exact licence (CC-BY-NC-4.0 per common reporting) not verified; SMPLer-X, ECON/ICON, CameraHMR, WHAM, TRAM, PromptHMR not researched here.
- No academic "size recommendation" papers were retrieved.

---

## KQ4. Cloth behaviour on mobile: Unity Cloth, Magica Cloth 2, learned cloth (HOOD/ContourCraft), or rigid skinned garments

### Takeaway
Built-in Unity Cloth is CPU-heavy and considered a hitching risk above ~1,500 vertices in avatar contexts; Magica Cloth 2 (paid) runs on iOS/Android but its MeshCloth mode is flagged as CPU-costly on mobile; learned simulators (HOOD/ContourCraft, MIT) are server-side GPU research code tied to SMPL, not phone-ready. The realistic mobile baseline is a skinned, mostly rigid garment with optional bone-chain secondary motion on hems/sleeves.

### Cited Findings
- Unity Cloth in avatar workloads: "Cloth components with 1500 or more vertices can cause significant hitching during loading and while scaling an avatar, which can be seconds long"; Cloth places "a huge burden on the CPU's cache and PCIE lanes, as well as the GPU's geometry frontend, with random vertex access being problematic" (VRChat developer update discussion) — [VRChat Developer Update 28 Aug 2025](https://ask.vrchat.com/t/developer-update-28-august-2025/46531?page=2)
- Magica Cloth: "works fine on iOS and can be used on all platforms except WebGL. However, since mobile devices including Android have a low number of CPU cores, care is necessary when using a large amount of MeshCloth, as it consumes a lot of CPU resources… There is no problem with BoneCloth" — [Unity Discussions: Magica Cloth](https://discussions.unity.com/t/released-magica-cloth/773425?page=49)
- Unity Cloth performance drivers: "Solver Frequency and Self Collision/Intercollision are parameters with very strong effects on performance" — [Unity forum thread](https://forum.unity.com/threads/cloth-performance-degrades-over-time-unity-2018-4-4-2019-2-0.719696/)
- HOOD "leverages graph neural networks, multi-level message passing, and unsupervised training to enable real-time prediction of realistic clothing dynamics for arbitrary types of garments and body shapes… generalizes to new garment types and shapes not seen during training" — [HOOD arXiv](https://arxiv.org/html/2212.07242v3)
- ContourCraft (SIGGRAPH 2024) adds "a novel intersection contour loss that penalizes interpenetrations" on top of HOOD; its repo is MIT licensed, currently supports the SMPL body (SMPL-X is a TODO), is "restricted to single-layer garments", and requires manual garment conversion — [ContourCraft GitHub](https://github.com/dolorousrtur/ContourCraft); [ContourCraft arXiv](https://arxiv.org/html/2405.09522v2)
- Snap's production approach combines "3D Body Tracking, Cloth Simulation, Body Mesh, and Physics Collider, on different types of clothing to create a realistic try-on experience" in Lens Studio (snippet; docs blocked) — [Snap cloth simulation try-on](https://developers.snap.com/lens-studio/features/try-on/cloth-simulation-try-on)

### Inferences
- HOOD's "real-time" is on a desktop GPU with a SMPL body; nothing in the sources suggests phone deployment, and its SMPL dependency carries the SMPL licence problem. Treat learned cloth as a *server-side preview-render* option (e.g. generating a turntable video), not a live-AR option.
- A pragmatic mobile tier list: (1) rigid skinned garment via the body proxy's weights (zero extra cost); (2) BoneCloth/jiggle chains on skirt hems and sleeves (Magica Cloth 2 BoneCloth mode, or a simple custom spring-chain) — low CPU; (3) MeshCloth / Unity Cloth only for hero items on high-end devices with a strict vertex budget (well under 1,500 simulated vertices) and low solver frequency.
- Because ARKit body joints arrive with jitter and occasional scale changes, any physics layer must be damped or it will amplify tracking noise (inference from the hitching report about avatar scaling).

### Gaps
- Unity's Cloth manual page was blocked; the collider-type (sphere/capsule only) and SkinnedMeshRenderer-only limitations are well-known but not verified here.
- Magica Cloth 2 pricing, Obi Cloth and Dynamic Bone were not researched. No GPU-cloth-on-mobile benchmarks found. SNUG/GAPS/"Neural cloth" not researched.

---

## KQ5. Platform body-tracking options: ARKit vs MediaPipe vs ML-HMR on device; Android/ARCore/Android XR; Snap; Lightship

### Takeaway
ARCore has no body tracking and Android XR's ARCore-for-Jetpack-XR exposes only device pose; Lightship ARDK 3.x explicitly lacks body tracking and defers to AR Foundation; the only cross-platform open path is MediaPipe Pose Landmarker (33 landmarks with 3D world coordinates) via the MIT-licensed homuler MediaPipeUnityPlugin, which gives positions not rotations and carries stability caveats.

### Cited Findings
- MediaPipe Pose Landmarker "outputs body pose landmarks in image coordinates and in 3-dimensional world coordinates" and ships as an Android task library (`com.google.mediapipe:tasks-vision`) with iOS equivalents (docs pages blocked; snippet) — [Pose Landmarker Android guide](https://developers.google.com/edge/mediapipe/solutions/vision/pose_landmarker/android)
- homuler MediaPipeUnityPlugin: MIT licence; supports iOS, Android, Linux, macOS, Windows, WebGL; wraps MediaPipe 0.10.22; "GPU mode is not supported on macOS and Windows"; pre-built packages are CPU-only; warns "Your application may crash!" due to native-library bugs; Android requires bundling `libstdc++_shared.so`; requires Unity 2022.3+ — [MediaPipeUnityPlugin GitHub](https://github.com/homuler/MediaPipeUnityPlugin)
- ARCore for Jetpack XR (Android XR) documents device pose only: "your app can retrieve a device's pose: the orientation… and a position… of the device relative to the world origin"; "Not all XR devices support the DeviceTrackingMode.SPATIAL mode" — no body-tracking API appears — [ARCore for Jetpack XR device pose](https://developer.android.com/develop/xr/jetpack-xr-sdk/arcore/device-pose?hl=en)
- Lightship ARDK 3 feature list: "Depth, Occlusion, Playback, Meshing, Navigation Mesh, Semantics, Shared AR, Lightship VPS, Project Validation, and Model Preloading", with Object Detection added in 3.4; body tracking "is not a native Lightship ARDK feature" — users are told to mix Lightship with "Unity's reliable AR frameworks like face and body tracking" — [Lightship ARDK 3.4 features](https://lightship.dev/docs/ardk/3.4/features); [Niantic community: body/pose detection](https://community.nianticspatial.com/t/does-lightship-has-a-way-to-detect-human-body-and-poses/788)
- Snap (closed, reference): the Body Mesh "creates a custom mesh that mimics the user's body as a 3D mesh in real time"; the Clothing Try-On template lets "outfits automatically deform to any body without needing to be rigged using the External Mesh feature"; Lenses can be embedded in a brand's app via Camera Kit — [Snap clothing try-on](https://developers.snap.com/lens-studio/features/try-on/clothing-try-on); [Snap body mesh overview](https://developers.snap.com/lens-studio/features/ar-tracking/body/body-mesh-overview); [Snap AR try-on blog (Apr 2026)](https://ar.snap.com/blog/ar-virtual-try-on-ecommerce-lens-studio)
- Unity Mars (legacy) "proxies support human avatar rigs and body poses defined by Mecanim" — [Unity MARS blog](https://blog.unity.com/technology/whats-new-in-unity-mars)

### Inferences
- The Android path realistically is MediaPipe Pose (or an equivalent 2D/3D keypoint model run through Unity Inference Engine). Because it returns 33 *positions*, the team must derive bone rotations (look-at between landmark pairs, with a roll heuristic) before feeding the same BoneController abstraction used for ARKit — a second, lower-fidelity joint provider behind the same interface.
- Snap's "External Mesh" (vertex-bind a garment to a tracked body mesh, no rig) is the best design reference for the Symspace product: it avoids rigging altogether. The open-source equivalent is to bind garment vertices to a tracked proxy body mesh driven by ARKit joints.
- On-device monocular mesh recovery (HMR2.0-class ViT backbones) was trained on 8×A100 for a week; the inference models are ViT-H class, so phone deployment at AR frame rates is not realistic without distillation. Treat as server-side.

### Gaps
- MediaPipe lite/full/heavy latency numbers, licence (Apache-2.0 is standard for MediaPipe but unverified here), and single-vs-multi-person limits: docs blocked.
- Unity Inference Engine (ex-Sentis) docs not fetched; no verified example of MediaPipe Pose running under Inference Engine.
- ARCore 2026 roadmap for body tracking: nothing found; treat as absent.
- RTMPose/RTMO/ViTPose/Sapiens-lite, WHAM/TRAM/PromptHMR/CameraHMR on-device feasibility: not researched.

---

## KQ6. The 2D image / video virtual try-on alternative: quality, licences, VRAM, latency

### Takeaway
Image try-on is mature and cheap to serve (CatVTON ~2.3 GB VRAM at 512×384, Leffa ~6 s on an A100) but almost every strong open model — IDM-VTON, CatVTON, FitDiT, CatV2TON, MagicTryOn — is CC BY-NC-SA 4.0; Leffa (MIT) is the notable permissive exception. Video try-on (MagicTryOn on Wan2.1, 14B/1.3B/Turbo variants through April 2026) exists but is non-commercial and heavyweight, so a commercial product needs either Leffa, a self-trained/LoRA model on a permissive base, or a paid API.

### Cited Findings
- VRAM at 512×384: "IDM-VTON requires 14.62 GB VRAM, CatVTON requires 2.26 GB, Leffa requires 3.91 GB" (from DeCo-VTON paper, Nov 2025); CatVTON "can handle 1024×768 resolution with inference requiring less than 8GB VRAM" — [DeCo-VTON arXiv](https://arxiv.org/pdf/2511.18775); [CatVTON on SourcePulse](https://www.sourcepulse.org/projects/1916167)
- Quality (FID on VITON-HD, from aggregated benchmark reporting): "FitDiT achieved an FID score of 4.7309, Leffa achieved 4.54, IDM-VTON achieved 6.290" — [HyperAI VITON-HD benchmark](https://hyper.ai/fr/sota/tasks/virtual-try-on/benchmark/virtual-try-on-on-viton-hd)
- IDM-VTON (ECCV 2024): "codes and checkpoints in this repository are under the CC BY-NC-SA 4.0 license"; SDXL + IP-Adapter; inference at 768×1024; needs DensePose, human parsing and pose preprocessing — [IDM-VTON GitHub](https://github.com/yisol/IDM-VTON)
- CatVTON: "licensed under Creative Commons BY-NC-SA 4.0 for non-commercial use only" — [CatVTON on SourcePulse](https://www.sourcepulse.org/projects/1916167)
- Leffa (CVPR 2025): MIT licence; independent authors (not Meta); "generating an image in 6 seconds (on A100)" after a Jan 2025 ref-UNet acceleration — [Leffa GitHub](https://github.com/franciszzj/Leffa)
- FitDiT (Dec 2024): CC BY-NC-SA 4.0, "This model can only be used for non-commercial use"; DiT architecture; demo resolution 1152×1536; offers bf16/fp16/CPU-offload modes; two-step mask-then-try-on pipeline — [FitDiT GitHub](https://github.com/BoyuanJiang/FitDiT)
- CatV2TON (Jan 2025, weights Feb 24 2025): CC BY-NC-SA 4.0; DiT-based; 256 and 512 resolution variants; "supports both image and video virtual try-on" — [CatV2TON GitHub](https://github.com/Zheng-Chong/CatV2TON)
- MagicTryOn (vivo): CC BY-NC-SA 4.0; Wan2.1 backbone; "Code and 14B weights released June 9, 2025; MagicTryOn-1.3B released December 26, 2025; MagicTryOn-Turbo released April 19, 2026"; trained on VITON-HD, DressCode, ViViD; community notes it is trained on controlled benchmarks "rather than diverse real-world scenarios" — [Magic-TryOn GitHub](https://github.com/vivoCameraResearch/Magic-TryOn)
- Newer editing-model routes: "FLUX Virtual Try-On is a virtual try-on image editing model from Black Forest Labs that generates apparel try-on results from a person image plus one or more garment references"; "TryAnything, a virtual try-on LoRA designed to transfer clothing from product images onto people, trained on Flux Kontext"; ComfyUI Qwen-Image-Edit try-on workflows exist (snippets; licences of FLUX dev weights are non-commercial and the LoRA inherits that — unverified) — [Comfy hub: flux virtual try-on](https://comfy.org/hub/models/flux-virtual-try-on.md); [TryAnything LoRA on HF](https://huggingface.co/Alissonerdx/TryAnything/blob/main/README.md); [RunComfy Qwen try-on workflow](https://www.runcomfy.com/pt/comfyui-workflows/comfyui-virtual-try-on-workflow-qwen-model-clothing-fitting)
- Commercial hosted alternatives (reference): FASHN VTON v1.5 is "a segmentation-free virtual try-on model that generates photorealistic results directly in pixel space"; OpenTryon offers "open-source APIs, SDKs, and models" (quality/licence not verified) — [FASHN blog](https://fashn.ai/tr/blog/category/ai?page=2); [OpenTryon GitHub](https://github.com/tryonlabs/opentryon)

### Inferences
- Garment fidelity in 2D try-on (logos, prints, drape, wrinkles) now exceeds what a rigid skinned 3D garment in AR can show, at a fraction of the engineering cost; the trade-off is no live camera, no walking around, and a per-request GPU cost of seconds.
- Licence reality: a commercial Symspace product cannot ship IDM-VTON/CatVTON/FitDiT/CatV2TON/MagicTryOn outputs. Leffa (MIT) is the only permissive open image model found with competitive FID; for video the open options are all non-commercial as of this date.
- A hybrid is attractive: server-side 2D try-on photo for the "how does it look on me" question, and live AR reserved for rigid categories (shoes, watches, jewelry) where the team already has tracking.

### Gaps
- Latency/VRAM for FitDiT, CatV2TON, MagicTryOn not documented in their READMEs.
- OOTDiffusion, Any2AnyTryon, BooW-VTON, MV-VTON, VTON-360, ViViD, Dress&Dance, VITON-DiT, ViTryOn licences not checked in this pass.
- Licence of the FLUX Virtual Try-On model and the Qwen-Image-Edit try-on LoRAs not verified.

---

## KQ7. Industry reality check 2024–2026: 3D AR vs 2D generative for apparel

### Takeaway
Every major apparel player surveyed has converged on 2D/video generative try-on for clothing (Google Shopping try-on + Doppl video app, Perfect Corp "AI Clothes Try-on" and Jan-2026 generative fashion APIs, Doji avatars, Walmart via Zeekit's 2D model-photo approach), while live 3D body-tracked garment AR survives mainly inside Snap's Lens ecosystem; the evidence favours 2D generative for garments and 3D AR for rigid accessories.

### Cited Findings
- Google Shopping virtual try-on: the generative model "can take just one clothing image and accurately reflect how it would drape, fold, cling, stretch and form wrinkles and shadows on a diverse set of real models in various poses", with models "ranging in size from XXS to XXXL" (2023 launch; blog blocked, snippet) — [Google blog](https://blog.google/products/shopping/ai-virtual-try-on-google-shopping/); [VentureBeat](https://venturebeat.com/ai/how-google-is-using-generative-ai-for-virtual-try-ons)
- July 2025: Google extended try-on to the user's own photo — "upload a full-length photo of themselves, and see what they might look like wearing the clothing", across Search, Shopping and Images in the US (snippet; TechCrunch blocked) — [TechCrunch 2025-07-24](https://techcrunch.com/2025/07/24/googles-new-ai-feature-lets-you-virtually-try-on-clothes); [Google Merchant Center: apparel virtual try-on](https://support.google.com/merchants/answer/14096369)
- Google Doppl (experimental app, iOS/Android, US, 18+): users "upload a full-body photo of themselves along with a screenshot of any outfit"; it "first generates a still image… then offers animation options"; "the entire process takes just a few minutes per outfit"; Google warns "fit, appearance and clothing details may not always be accurate" — [getcoai on Doppl](https://getcoai.com/news/googles-doppl-app-creates-virtual-try-on-videos-from-any-outfit); [Techloy on Doppl](https://www.techloy.com/google-unveils-doppl-a-new-virtual-outfit-try-on-app/)
- Perfect Corp, May 2025: "AI Clothes Try-on, a generative AI-powered experience where consumers can… virtually try on entire outfit collections"; Jan 2026: "nine new APIs for virtual try-on… for watches, bracelets, rings, earrings, necklaces, scarves, hats, shoes, and bags… powered by Generative AI and computer vision… generates highly realistic images of people wearing selected items" (snippets; BusinessWire blocked) — [Perfect Corp May 2025 release](https://www.businesswire.com/news/home/20250523976398/en/Available-Now---Perfect-Corp.-Debuts-New-GenAI-Clothes-Virtual-Try-On-for-Brand-and-Retailer-Websites-Apps-and-API); [Perfect Corp Jan 2026 release](https://www.businesswire.com/news/home/20260115898437/en/Perfect-Corp.-Unveils-Range-of-New-Modular-APIs-to-Power-Next-Generation-of-Fashion-Virtual-Try-On-Experiences-Powered-by-Generative-AI)
- Doji raised "$14 million in seed funding in May 2025" (Thrive Capital lead); users "upload selfies and full-body images to create personalized avatars", with "its own diffusion models" — [TechCrunch on Doji](https://techcrunch.com/2025/05/15/doji-raises-14m-to-make-virtual-try-ons-fun-through-ai-avatars)
- Walmart acquired Zeekit (founded 2013) to let consumers "virtually 'try on' clothing when shopping online"; prior clients included "Macy's, Asos, Tommy Hilfiger, adidas" — [MarTechVibe](https://martechvibe.com/article/walmart-acquires-virtual-clothing-try-on-startup-zeekit)
- Snap (3D camp): Lens Studio Clothing Try-On deforms outfits to the body mesh "without needing to be rigged", and brands "can integrate these Lenses directly into their own shopping apps and websites using the Camera Kit SDK" — [Snap clothing try-on](https://developers.snap.com/lens-studio/features/try-on/clothing-try-on)
- Zalando (measurement/3D-scan fit) vs Zara (generative avatar) comparison: Zalando's "avatars can make shoppers uncomfortable, while Zara's system was more engaging and visually appealing but had performance limitations" (secondary source) — [Business Report](https://www.businessreport.com/?p=276402); [Fast Company on generative AI in fashion](https://www.fastcompany.com/91421896/how-generative-ai-is-redefining-fashion)

### Inferences
- The market signal in 2025–2026 is unambiguous for garments: generative 2D (and now short video) on the user's own photo is the shipping product at Google, Perfect Corp, Doji and Walmart/Zeekit. Live 3D body-tracked AR garments remain a Snap-ecosystem feature and are not the primary apparel try-on at any surveyed retailer.
- For Symspace, the defensible recommendation is: (a) generate a 3D garment for product visualization/turntables and for rigid-ish items; (b) for "on my body" try-on of garments, run a server-side 2D (optionally short-video) try-on; (c) keep live AR body tracking for accessories and for a lightweight "rigid garment preview" on iOS only, clearly labelled as approximate.

### Gaps
- Shopify's current (2026) apparel try-on offering was not found in this pass.
- Wanna (Farfetch) 2026 status not found.
- No quantified conversion/return-rate evidence comparing 3D AR vs 2D generative apparel try-on was retrieved.

---

## Summary of licence posture (for the report writer)

| Component | Licence | Commercial OK? | Source |
|---|---|---|---|
| UniRig | MIT | Yes | [GitHub](https://github.com/VAST-AI-Research/UniRig) |
| SkinTokens | MIT (snippet only) | Likely; verify | [HF card (blocked)](https://huggingface.co/VAST-AI/SkinTokens) |
| MagicArticulate | Apache-2.0 (snippet) | Likely; verify | [HF](https://www.huggingface.co/Seed3D/MagicArticulate/tree/main) |
| RigNet | GPL-3.0 | Copyleft | [GitHub](https://github.com/zhan-xu/RigNet) |
| ChatGarment | Apache-2.0 | Yes | [GitHub](https://github.com/biansy000/chatgarment) |
| GarmentCode | MIT | Yes | [GitHub](https://github.com/maria-korosteleva/GarmentCode) |
| ContourCraft/HOOD code | MIT (needs SMPL) | Code yes; SMPL no | [GitHub](https://github.com/dolorousrtur/ContourCraft) |
| Hunyuan3D-Part | Tencent Community (no EU/UK/KR; 1M MAU) | Conditional | [LICENSE](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-Part/main/LICENSE) |
| SMPL / SMPL-X model files | Non-commercial; Meshcapade paid | Paid | [SMPL-X licence](https://smpl-x.is.tue.mpg.de/modellicense.html) |
| 4D-Humans (HMR2.0) | MIT code + SMPL files | Code yes; SMPL paid | [GitHub](https://github.com/shubham-goel/4D-Humans) |
| Multi-HMR | CC BY-NC-SA 4.0 | No | [HF](https://huggingface.co/spaces/naver/multi-hmr/blob/bac33d63480e549ddd799f63a7be4f28e0df07b6/Multi-HMR_License.txt) |
| MediaPipeUnityPlugin | MIT | Yes | [GitHub](https://github.com/homuler/MediaPipeUnityPlugin) |
| Leffa | MIT | Yes | [GitHub](https://github.com/franciszzj/Leffa) |
| IDM-VTON, CatVTON, FitDiT, CatV2TON, MagicTryOn | CC BY-NC-SA 4.0 | No | repos linked above |
