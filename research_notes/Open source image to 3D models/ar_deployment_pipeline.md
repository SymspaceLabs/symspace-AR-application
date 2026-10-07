# Production pipeline: open-source image-to-3D output -> Unity mobile AR (ARKit/ARCore) assets

Research date: 2026-10-07. Scope: post-processing, export formats, Unity runtime loading, hosting/serving, cost, licensing, QC, and text-to-image front ends. Note on access: during this research the network proxy blocked huggingface.co, replicate.com, fal.ai, modal.com, runpod.io, docs.unity3d.com, discussions.unity.com, stability.ai and blog.salad.com; facts from those sites come from search-engine snippets or third-party mirrors and are marked as such. GitHub (raw and rendered), developer.apple.com forums, and arXiv were reachable.

---

## Key Question 1: Post-processing chain after generation (remesh/decimate, UVs, PBR baking, polycount/texture targets, compression, GLB/USDZ export, gotchas)

### Takeaway
Every major open model already ships a mesh post-process (Hunyuan3D: pymeshlab-based FloaterRemover / DegenerateFaceRemover / FaceReducer with a 40k-face default; TRELLIS: PyVista quadric decimation + hole fill + xatlas UVs + baked 1024-2048 px texture; TripoSG/SPAR3D: face-count targets or quad/tri remesh), so the production chain is "model's own post-process -> gltf-transform/gltfpack (simplify, Draco or meshopt, KTX2) -> GLB for Unity; optionally Reality Converter / Blender for USDZ". Apple's still-current AR Quick Look budget (100k polys, one 2048x2048 PBR set) is the practical upper bound; 10k-50k tris with 1K-2K textures is the sensible target band, though no vendor publishes a formal "mobile AR triangle budget".

### Cited Findings

**Model-side post-processing (what you get for free)**
- Hunyuan3D 2.1 ships `hy3dshape/postprocessors.py` with `FaceReducer` (quadric edge collapse via pymeshlab, default `max_facenum=40000`), `FloaterRemover` (removes small disconnected components via pymeshlab), `DegenerateFaceRemover` (cleans degenerate faces by round-tripping through pymeshlab/PLY), `MeshSimplifier` (external `mesh_simplifier.bin` binary; normalizes output to a unit sphere with a 1.2x scale factor), plus `mesh_normalize()` ("normalize mesh vertices to sphere") and trimesh<->pymeshlab converters — [Hunyuan3D-2.1 postprocessors.py](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/hy3dshape/hy3dshape/postprocessors.py)
- Hunyuan3D 2.1 outputs PBR materials "including albedo, metallic, roughness, and normal maps", exports GLB and OBJ; VRAM: 10 GB shape-only, 21 GB texturing, 29 GB end-to-end; shape model 3.3B params, paint model 2B — [Hunyuan3D-2.1 README](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1)
- Hunyuan3D 2.0 pipeline returns trimesh objects "which you could save to glb/obj (or other format) file"; shape 6 GB VRAM, shape+texture 16 GB — [Hunyuan3D-2 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/README.md)
- TRELLIS `postprocessing_utils.to_glb(app_rep, mesh, simplify=0.95, fill_holes=True, fill_holes_max_size=0.04, texture_size=1024)`: pipeline = PyVista quadric edge-collapse decimation (`postprocess_mesh` default `simplify_ratio=0.9`) -> `_fill_holes` (removes invisible faces by multi-view rasterization, graph mincut, pymeshfix boundary repair) -> `parametrize_mesh` via **xatlas** -> `bake_texture` from 100 rendered views at 1024 px (modes 'fast' nearest-neighbour or 'opt' = Adam optimisation 2500 steps, TV weight 1e-2; function default `texture_size=2048`) -> builds a PBR material and **rotates the mesh from Z-up to Y-up** before returning a trimesh — [TRELLIS postprocessing_utils.py](https://raw.githubusercontent.com/microsoft/TRELLIS/main/trellis/utils/postprocessing_utils.py)
- TRELLIS.2 (4B, MIT) generates base color, roughness, metallic and opacity; exports GLB with an optional WebP texture-compression option; post-processing exposes face-count decimation targets (example 1,000,000; nvdiffrast limit 16,777,216), `texture_size` 4096 default, and remeshing band/projection parameters; needs 24 GB VRAM, Linux only; 512^3 ~3 s, 1024^3 ~17 s, 1536^3 ~60 s on H100 — [microsoft/TRELLIS.2](https://github.com/microsoft/TRELLIS.2). Third-party summaries additionally claim UV layout plus baked normal and AO maps — [aiindigo (community)](https://aiindigo.com/blog/trellis-2-review-a-practical-look-at-microsoft-s-open-source-3d-engine)
- TripoSG inference script accepts a `--faces` parameter to limit polygon count before GLB export; geometry only (no texture generation documented); uses RMBG-1.4 for background removal; 8 GB VRAM — [VAST-AI-Research/TripoSG](https://github.com/VAST-AI-Research/TripoSG)
- SPAR3D offers remesh modes none / triangle / quad (quad converted to triangles for GLB export), target vertex/face counts as "rough targets", `--texture-resolution` flag; 10.5 GB VRAM (7 GB low-VRAM mode) — [Stability-AI/stable-point-aware-3d](https://github.com/Stability-AI/stable-point-aware-3d)
- InstantMesh exports `.obj` with vertex colours by default; `--export_texmap` produces a UV texture but "will cost longer time" (UV unwrapping step) — [TencentARC/InstantMesh](https://github.com/TencentARC/InstantMesh)
- Step1X-3D exports `.glb` (untextured or textured with UVs); geometry 1.3B + texture 3.5B params; ~27-29 GB GPU memory, ~152 s for 50 steps — [stepfun-ai/Step1X-3D](https://github.com/stepfun-ai/Step1X-3D)

**Polycount / texture budgets for mobile AR**
- Apple staff (Graphics and Games Engineer, Apple Developer Forums, Nov 2021) reaffirmed the WWDC 2018 AR Quick Look rule of thumb: **100K polygons, one set of 2048x2048 textures, 10 s of animation**, chosen so devices down to iPhone 7 can load the model; RealityKit apps have "a bit more wiggle room"; recommended shipping multiple model versions per device tier; AR Quick Look should auto-downsample textures for low-memory devices — [Apple Developer Forums thread 693867](https://developer.apple.com/forums/thread/693867)
- Same guidance repeated by community: textures should be square power-of-2 (2048/1024/512), single material/texture set preferred — [netguru (community)](https://netguru.com/blog/ar-quick-look-and-usdz)
- Android/Unity generic guidance: "somewhere between 300 and 1500 polygons per mesh" for mobile; vertex processing is expensive on mobile — [Unity Learn / developer.android.com via search](https://developer.android.com/games/optimize/geometry?hl=en). Community guides quote 1,500-5,000 tris for mobile game assets and a 50K ceiling for WebAR — [neural4d blog (community)](https://blog.neural4d.com/?p=2409)

**Compression and optimisation tooling**
- gltfpack (meshoptimizer): `-c` emits EXT_meshopt_compression, `-cc` higher compression (best with gzip delivery), `-cf` fallback buffers, `-tc` converts textures to KTX2/BasisU (KHR_texture_basisu), `-tq` quality, `-tw` WebP, `-si R` simplification ratio (0-1) — [meshoptimizer gltf/README](https://github.com/zeux/meshoptimizer/blob/master/gltf/README.md)
- glTF-Transform CLI (MIT): `optimize`, `draco --method edgebreaker`, `meshopt --level medium`, `resize --width 1024 --height 1024`, `webp --slots baseColor`, `uastc --level 4 --rdo --rdo-lambda 4 --zstd 18`, `etc1s --quality 255`; KTX2 encoding depends on KTX-Software `toktx` — [donmccurdy/glTF-Transform](https://github.com/donmccurdy/glTF-Transform)

**USDZ export (iOS Quick Look only)**
- Reality Converter (macOS) drag-and-drops `.obj`, `.gltf`, `.fbx`, `.usd` to USDZ, lets you swap material textures, edit metadata and preview under IBL — [cgpress](https://cgpress.org/archives/reality-converter-converts-3d-formats-to-usdz.html); [monstar-lab](https://engineering.monstar-lab.com/en/post/2020/04/26/how-to-convert-3d-models-to-usdz-files-using-apples-reality-converter/)
- Apple's Python `usdzconvert`/USDPython tools: community reports they have been retired from the Apple developer downloads; earlier versions (0.63/0.64) were buggy and Python 3.7-bound; Blender's USD exporter and Adobe tooling are cited as alternatives — [AOUSD forum](https://forum.aousd.org/t/obj-gltf-to-usdz-usdzconvert-discontinued/1648); [Apple forums 665427](https://developer.apple.com/forums/thread/665427)
- Blender->USDZ gotchas (community and Apple forum reports): missing UV layout makes PBR materials render black on iOS; 8K textures break export; oversized meshes/files silently fail to display on iOS; some Blender-exported USDZs show white (textures unlinked) in Quick Look; Apple recommends validating with Reality Converter — [Apple forums 777035](https://developer.apple.com/forums/thread/777035); [Apple forums 107094](https://developer.apple.com/forums/thread/107094)
- Apple's current creation-tools page now headlines Reality Composer Pro 3 (macOS 26.5+, visionOS/iOS content prep) — [Apple AR tools page](https://developer.apple.com/augmented-reality/tools/)

**Coordinate systems**
- TRELLIS output is Z-up internally and is rotated to Y-up at GLB export — [TRELLIS postprocessing_utils.py](https://raw.githubusercontent.com/microsoft/TRELLIS/main/trellis/utils/postprocessing_utils.py)
- Hunyuan3D normalises generated meshes to a unit sphere (1.2x factor in MeshSimplifier) — [Hunyuan3D-2.1 postprocessors.py](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/hy3dshape/hy3dshape/postprocessors.py)

### Inferences
- Recommended chain for an AR commerce app: (1) model-native post-process (Hunyuan: FloaterRemover -> DegenerateFaceRemover -> FaceReducer(max_facenum ~20-40k); TRELLIS: `to_glb(simplify=0.95, texture_size=1024 or 2048)`), (2) `gltf-transform` pass: `weld`, `simplify` if still >50k tris, `resize` textures to 1024/2048, `uastc` or `etc1s` to KTX2, then `meshopt` or `draco`, (3) store GLB on object storage, (4) only derive USDZ when you need iOS Quick Look outside the Unity app; inside a Unity AR Foundation app GLB is the single runtime format on both iOS and Android.
- Because every model normalises to a unit cube/sphere, real-world scale is lost; the backend must re-scale from known product dimensions (or user input) before export, and should set the pivot to the base of the bounding box so AR placement sits on the plane.
- ETC1S/KTX2 is the right default for albedo on mobile (small, GPU-native on both ASTC-capable iOS and Android); UASTC for normal/ORM maps where ETC1S block artifacts are visible. Draco gives smaller downloads but higher decode CPU; meshopt decodes faster and streams — both are supported by glTFast (see KQ2).
- The models output metallic-roughness glTF materials already (Hunyuan 2.1, TRELLIS.2), which maps directly to Unity's URP Lit metallic workflow via glTFast; for TRELLIS 1.x / TripoSG / InstantMesh you only get baseColor (or vertex colours) and must treat the material as rough dielectric.

### Gaps
- No vendor-published triangle budget for Unity AR Foundation on mobile exists; the 100k/2048 Apple figure is the only official number found and it is from 2018/2021. The 10k-50k target is an inference from Apple's ceiling plus community mobile guidance.
- Could not fetch the Khronos glTF spec text (registry.khronos.org blocked) to quote the coordinate/units clause; the well-known spec statement (Y-up, right-handed, metres, +Z front) is not cited here.
- Exact normal-map convention handling (OpenGL vs DirectX) in Hunyuan3D 2.1's normal output was not verified.
- Whether Hunyuan3D 2.1's `api_server.py` applies the post-processors automatically could not be confirmed from the file summary.

---

## Key Question 2: Loading generated GLB at runtime in Unity (glTFast, UnityGLTF, Piglet, TriLib), Draco/meshopt/KTX2, URP, iOS/Android, AR Foundation versions

### Takeaway
Unity's official glTFast (`com.unity.cloud.gltfast`, Apache-2.0) is the default choice: it supports Draco, meshopt and KTX2/BasisU via the companion `com.unity.cloud.draco` and `com.unity.cloud.ktx` packages, URP/HDRP/Built-in, and iOS/Android (arm64/armv7), with a trivial `GltfAsset.url` runtime API; UnityGLTF (Khronos/prefrontal cortex, pure C#) is the flexible alternative with broader KHR_materials coverage. AR Foundation 6.x on Unity 6 is current, and ARKit XR Plugin 6.4+ requires Xcode 26.

### Cited Findings
- `com.unity.cloud.gltfast` is "the official Unity glTF package, maintained by Unity Technologies", Apache License 2.0, forked from atteneder/glTFast; development repo has 2,142 commits — [Unity-Technologies/com.unity.cloud.gltfast](https://github.com/Unity-Technologies/com.unity.cloud.gltfast)
- glTFast supports "a large and growing number of glTF extensions" including Draco compression, meshopt, KTX2/basisu and KHR_materials_*; compatible with Universal, High Definition and Built-In render pipelines; platforms Android, iOS, Linux, macOS, Windows, WebGL; dependencies "Draco for Unity" and "KTX for Unity"; runtime example `var gltf = gameObject.AddComponent<GLTFast.GltfAsset>(); gltf.url = "...";` — [atteneder/glTFast README](https://github.com/atteneder/glTFast)
- glTFast 6.15.0 (released 2025-11-17) added loading KTX textures from data URIs, enforces up-to-date Draco for Unity / KTX for Unity versions via compiler error, and fixed Draco morph-target resource disposal — [glTFast 6.15 changelog (docs.unity3d.com via search snippet)](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.15/changelog/CHANGELOG.html)
- Draco for Unity (`com.unity.cloud.draco`) supports iOS (device and simulator, arm64 and armv7a) and Android (x86, x86_64, arm64, armv7a), Unity 2020.3+; KTX for Unity (`com.unity.cloud.ktx`) supports iOS (arm64, armv7a) and Android, Unity 2020.1+ — [Draco for Unity prerequisites (search snippet)](https://docs.unity3d.com/Packages/com.unity.cloud.draco@5.1/manual/prerequisites.html); [KTX for Unity installation (search snippet)](https://docs.unity3d.com/Packages/com.unity.cloud.ktx@3.4/manual/installation.html)
- UnityGLTF (KhronosGroup): recommends Unity 2021.3+/2022.3+/6000.0+; URP "fully supported", Built-in supported, HDRP "not actively maintained"; import-only support for KHR_draco_mesh_compression (requires com.unity.cloud.draco), KHR_texture_basisu (requires com.unity.cloud.ktx), EXT_meshopt_compression, KHR_mesh_quantization; import+export for KHR_materials_transmission/volume/ior/clearcoat/sheen/iridescence/emissive_strength, KHR_texture_transform, KHR_lights_punctual, MSFT_lod; "UnityGLTF doesn't have any native dependencies (pure C#)"; both packages can coexist with glTFast taking importer precedence — [KhronosGroup/UnityGLTF](https://github.com/KhronosGroup/UnityGLTF)
- UnityGLTF release 2.18.8 (2026-02-05) added meshopt 0.2 support and importer deduplication statistics — [gitclear release notes (third-party)](https://www.gitclear.com/open_repos/KhronosGroup/unitygltf/release/release~2.18.8)
- AR Foundation 6.3 is listed for Unity 6000.3; ARCore XR Plugin 6.3.5 released for Unity 6000.3; ARKit XR Plugin 6.4.0 was rebuilt with Xcode 26.0.1 and "you are now required to use Xcode 26 or newer to build iOS apps that depend on this package" — [Unity docs via search](https://docs.unity3d.com/Packages/com.unity.xr.arkit@6.5/changelog/CHANGELOG.html); [Unity Manual 6000.3 ARCore](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.xr.arcore.html)
- ARCore Extensions for AR Foundation supports AR Foundation 4, 5 and 6; AR Foundation 6.x requires the Input System package (TrackedPoseDriver dependency) — [Google ARCore docs](https://developers.google.com/ar/develop/unity-arf/upgrade-to-ar-foundation-6)

### Inferences
- For Symspace: install `com.unity.cloud.gltfast` + `com.unity.cloud.draco` + `com.unity.cloud.ktx`, use URP, and load via `GltfImport.Load(url)` / `GltfAsset` with a `CancellationToken`; a Draco- or meshopt-compressed, KTX2-textured GLB will then load on both ARKit and ARCore without format branching. Keep a Draco-free fallback only if you must support WebGL without the native decoder.
- Piglet and TriLib (paid Asset Store packages) were not researched here because the official and Khronos packages cover the need; they remain options if you need FBX/OBJ import at runtime.
- Match package majors: AR Foundation 6.x <-> ARKit 6.x <-> ARCore 6.x, and plan on Xcode 26 for iOS builds as of 2026.

### Gaps
- docs.unity3d.com and discussions.unity.com were blocked, so the exact newest glTFast version after 6.15.0 (Nov 2025) and its full extension table could not be verified; the 6.15 changelog snippet is the latest concrete evidence.
- Piglet and TriLib 2026 status (Draco/KTX2 support, URP) not researched.
- No primary source found on glTFast's handling of KHR_materials_* on URP mobile specifically (e.g., transmission on mobile).

---

## Key Question 3: Serving/hosting architecture (server-side GPU vs on-device), latency, cost per asset, Docker images, hosted APIs, ComfyUI, queueing for mobile

### Takeaway
Generation must be server-side: the open models need 8-29 GB of GPU VRAM and take tens of seconds to minutes per asset (Hunyuan3D 2.1 median ~139 s on RTX 4090 per SaladCloud; ~116 s on an L40S on Replicate), so the mobile client should submit a job and poll/receive a push, exactly the `/send` + `/status/{uid}` pattern Hunyuan3D's own `api_server.py` implements. Self-hosting on RunPod/Modal costs roughly $0.03-0.10 per asset in GPU time (L40S/4090 class), while hosted endpoints charge ~$0.06-0.35 per run (Replicate TRELLIS ~$0.058, Hunyuan3D-2 ~$0.11, fal TRELLIS.2 $0.25-0.35).

### Cited Findings

**Reference server pattern (from the model repos)**
- Hunyuan3D 2.1 `api_server.py`: endpoints `POST /generate` (synchronous, returns FileResponse GLB), `POST /send` (async, returns task uid), `GET /status/{uid}` (returns `model_base64` GLB when done), `GET /health`; "model worker executes the model" with semaphore-based concurrency (default 5), background threads and UUID task tracking; staged outputs `{uid}_initial.glb` then `{uid}_textured.glb` — [Hunyuan3D-2.1 api_server.py](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/api_server.py)
- Hunyuan3D 2.0 launch: `python api_server.py --host 0.0.0.0 --port 8080`; curl example posts `{"image": "<base64>"}` to `/generate` and saves `test2.glb` — [Hunyuan3D-2 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/README.md)
- TRELLIS requires an NVIDIA GPU with at least 16 GB, validated on A100/A6000, CUDA 11.8/12.2 — [microsoft/TRELLIS](https://github.com/microsoft/TRELLIS); TRELLIS.2 needs 24 GB, tested A100/H100, Linux only, ships `app.py` Gradio demo but "no dedicated server architecture documented" — [microsoft/TRELLIS.2](https://github.com/microsoft/TRELLIS.2)
- Hunyuan3D 2.1 VRAM: 10 GB shape / 21 GB texture / 29 GB end-to-end; low-VRAM mode available in the Gradio app — [Hunyuan3D-2.1 README](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1)

**Latency**
- SaladCloud benchmark: "median generation time of 139.2 seconds" across 900+ Hunyuan3D 2.1 generations on RTX 4090 GPUs; cost $0.0148/generation (High priority) or $0.009/generation (Batch priority) — [SaladCloud blog (search snippet; page blocked)](https://blog.salad.com/hunyuan3d-2-1/)
- Replicate `ndreca/hunyuan3d-2`: "approximately $0.11 to run", runs on Nvidia L40S, "predictions typically complete within 116 seconds" — [Replicate (search snippet; page blocked)](https://replicate.com/ndreca/hunyuan3d-2)
- TRELLIS.2 on H100: ~3 s at 512^3, ~17 s at 1024^3, ~60 s at 1536^3 — [microsoft/TRELLIS.2](https://github.com/microsoft/TRELLIS.2)
- Step1X-3D geometry+texture: ~152 s for 50 steps — [stepfun-ai/Step1X-3D](https://github.com/stepfun-ai/Step1X-3D)

**Hosted-API per-run prices (third-party or snippet sources; verify before budgeting)**
- Replicate `firtoz/trellis` ~$0.058/run; `fishwowater/trellis2` ~$0.82/run — [Replicate via search](https://replicate.com/firtoz/trellis); [Replicate trellis2 via search](https://replicate.com/fishwowater/trellis2)
- fal.ai TRELLIS.2: $0.25 (512p), $0.30 (1024p), $0.35 (1536p) per generation — [layer.ai / cloudprice (third-party)](https://cloudprice.net/models/fal-ai-trellis)
- fal.ai Hunyuan3D: v2 multi-view turbo $0.015/model; v3.1 rapid $0.225; v3.1 pro $0.375; v3 image-to-3D $0.375 — [costgoat aggregator (third-party)](https://costgoat.com/pricing/hunyuan-3d)
- Eachlabs Hunyuan3D 2.1: $0.30 per execution — [Eachlabs](https://www.eachlabs.ai/tencent/hunyuan-3d/hunyuan-3d-v2-1)
- Segmind lists Hunyuan3D-2 and Hunyuan3D-2mv pricing pages (amounts not captured) — [Segmind](https://www.segmind.com/models/hunyuan3d-2mv/pricing)

**GPU rental prices (for self-hosting; third-party summaries, official pages blocked)**
- RunPod (July 2026 update): A100 80GB $1.19/h Community, A100 SXM $1.39/h; L40S 48GB $0.79/h Community; RTX 4090 $0.34/h Community, $0.69/h Secure; per-second billing — [deploybase (third-party)](https://deploybase.ai/articles/runpod-gpu-pricing); [hivenet (third-party)](https://www.hivenet.com/post/runpod-pricing-complete-guide-to-gpu-cloud-costs)
- Modal: H100 $0.001097/s (~$3.95/h), A100 80GB $0.000694/s ($2.50/h), L40S $0.000542/s ($1.95/h); true per-second billing, no minimum increment — [spheron (third-party)](https://www.spheron.network/blog/modal-gpu-pricing-2026-per-second-billing/); [budgetforge (third-party)](https://www.budgetforge.dev/tools/modal-pricing-2026)
- Hugging Face Inference Endpoints (dedicated, billed per minute): AWS L4 $0.80/h, A10G $1.00/h, A100 80GB $2.50/h; GCP L4 $0.70/h, A100 $3.60/h — [HF pricing docs (search snippet)](https://huggingface.co/docs/inference-endpoints/support/pricing); [spheron summary](https://www.spheron.network/blog/hugging-face-inference-endpoints-pricing-2026/)

**Docker / serverless packaging**
- A community Docker image `shiftupai/hunyuan2.1-runpod` (`docker pull shiftupai/hunyuan2.1-runpod:prod`) exists for RunPod — [Docker Hub](https://hub.docker.com/r/shiftupai/hunyuan2.1-runpod)
- RunPod Serverless: build `docker build --platform linux/amd64 ...`, push, create endpoint with GPU config and worker count; scales to zero — [RunPod docs](https://docs.runpod.io/serverless/workers/deploy)
- Modal's unit of work is a Python function (no Dockerfile/registry needed) versus RunPod's Docker container — [RunPod comparison page](https://www.runpod.io/articles/comparison/runpod-vs-modal)

**ComfyUI**
- kijai's ComfyUI-Hunyuan3DWrapper (~1.03k stars, last updated 2026-07-29, 35 nodes: Hy3D Model Loader, Generate Mesh, Apply Texture, Delight Image, Paint/Delight loaders, Torch Compile settings) "currently offers the most complete support and performance in ComfyUI, capable of implementing complete rendering output from model to texture"; ComfyUI also has native Hunyuan3D-2 support — [comfyui-wiki](https://comfyui-wiki.com/en/tutorial/advanced/3d/huanyuan3d-2); [runcomfy node listing](https://www.runcomfy.com/comfyui-nodes/ComfyUI-Hunyuan3DWrapper)
- TRELLIS.2 and Pixal3D ComfyUI workflows exist (community) — [runcomfy workflow](https://www.runcomfy.com/comfyui-workflows/pixal3d-and-trellis2-comfyui-pbr-image-to-3d-glb)

### Inferences
- On-device generation is not feasible: the smallest usable model (TripoSG, 8 GB VRAM, geometry only) already exceeds mobile GPU memory, and Hunyuan3D 2.1 end-to-end is 29 GB.
- Self-host cost per asset: at ~140 s/asset on a 4090 at $0.34-0.69/h -> $0.013-0.027 GPU time; on an L40S (~116 s at $0.79/h RunPod, $1.95/h Modal) -> $0.025-0.063; on an A100 80GB ($1.19-2.50/h) with the full 29 GB PBR pipeline -> roughly $0.04-0.10. Hosted APIs (~$0.06-0.35) are 2-10x the raw GPU cost but remove ops burden and cold starts; SaladCloud's $0.009-0.015 figure is a batch/spot tier.
- Cold start matters: model weights are multi-GB (Hunyuan 2.1 shape 3.3B + paint 2B; TRELLIS.2 4B), so a scale-to-zero serverless worker will take tens of seconds to minutes to load; keep one warm worker during business hours, or use hosted endpoints for burst and self-host for baseline.
- Recommended architecture for a mobile client: Unity app -> HTTPS to your API (auth, rate limit) -> enqueue job (Redis/SQS) -> GPU worker (FastAPI/Modal function wrapping the model + post-process + gltf-transform) -> write GLB/KTX2 to S3/GCS/CDN -> push/poll result URL -> glTFast loads over HTTPS with caching. Never let the phone hold an open HTTP request for 2+ minutes; use the `/send` + `/status` pattern or a webhook/push notification.
- ComfyUI is viable as an internal batch/experimentation server but adds a large dependency surface; for production an explicit FastAPI/Modal worker built on the model's own pipeline classes is simpler to harden.

### Gaps
- Official pricing pages of Replicate, fal.ai, Modal, RunPod and Hugging Face were blocked; all prices above are from aggregators or search snippets and should be re-verified before budgeting.
- No official cold-start timings for any of these models on serverless platforms were found.
- No primary benchmark of Hunyuan3D 2.1 on H100/A100 was located (the arXiv report was not fetched); only the 4090 (SaladCloud) and L40S (Replicate) numbers.
- Replicate pages for Hunyuan3D 2.1 specifically (with PBR) were not found/verified; the $0.11 figure is for Hunyuan3D-2.

---

## Key Question 4: Licensing for commercial use (Hunyuan3D 2.0/2.1/3.0/Omni, TRELLIS/TRELLIS.2, TripoSG, Step1X-3D, SPAR3D, SAM 3D Objects, InstantMesh, Objaverse training-data risk)

### Takeaway
The license text itself (fetched from GitHub) shows Tencent Hunyuan3D 2.0 and 2.1 are usable commercially but **exclude the EU, UK and South Korea** and require a separate Tencent license if the licensee's products exceed **1 million MAU** (not 100 million) — a hard problem for a global AR commerce app. MIT-licensed TRELLIS/TRELLIS.2 and TripoSG, and Apache-2.0 Step1X-3D and InstantMesh, carry no territorial or MAU restrictions; SAM 3D Objects (SAM License) permits worldwide commercial use with trade-control/military exclusions. All of them, however, train on Objaverse(-XL)/Sketchfab-scraped data, which is under active copyright dispute.

### Cited Findings

**Tencent Hunyuan3D**
- Hunyuan3D 2.0 LICENSE: "Tencent Hunyuan 3D 2.0 Community License Agreement"; Territory = "worldwide territory, excluding the territory of the European Union, United Kingdom and South Korea"; "If, on the Tencent Hunyuan 3D 2.0 version release date, the monthly active users of all products or services made available by or for Licensee is greater than 1 million monthly active users in the preceding calendar month, You must request a license from Tencent"; required notice "Tencent Hunyuan 3D 2.0 is licensed under the Tencent Hunyuan 3D 2.0 Community License Agreement, Copyright © 2025 Tencent. All Rights Reserved. The trademark rights of 'Tencent Hunyuan' are owned by Tencent or its affiliate." — [Hunyuan3D-2 LICENSE](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/LICENSE)
- Hunyuan3D 2.1 LICENSE: "Tencent Hunyuan 3D 2.1 Community License Agreement"; same EU/UK/South Korea exclusion; same >1 million MAU clause ("You must request a license from Tencent"); must include the copyright notice, mark modified files, is encouraged to display "Powered by Tencent Hunyuan", and must "clearly, accurately, and prominently disclose ... that Tencent is not affiliated with, associated with, sponsoring, or endorsing" the product; Acceptable Use Policy (Exhibit A) lists ~20 prohibited uses including military use — [Hunyuan3D-2.1 LICENSE](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/LICENSE)
- Hunyuan3D-Omni is governed by the "TENCENT HUNYUAN 3D OMNI COMMUNITY LICENSE AGREEMENT" with the same EU/UK/South Korea exclusion and >1M MAU license requirement — [deepwiki summary of repo (third-party)](https://deepwiki.com/Tencent-Hunyuan/Hunyuan3D-Omni/6-reference)
- Hunyuan3D 3.0 is offered through Tencent's Hunyuan 3D platform/API ("freely available ... with an API"), with an open-source "Hunyuan 3D Omni" announced; no open 3.0 weights found — [Notebookcheck](https://www.notebookcheck.net/Tencent-s-new-Hunyuan-3D-3-0-brings-realistic-AI-model-generation-to-developers-and-creators.1116964.0.html); [3D Printing Industry](https://3dprintingindustry.com/news/tencent-launches-hunyuan-3d-engine-accelerating-ai-driven-asset-creation-246958/)
- Caution: a search hit claiming "Hunyuan 3.0 ... switched to Apache 2.0, removing EU/UK/South Korea restrictions" (July 2026) refers to the Hunyuan **LLM** (Hy3 MoE), not the 3D model — [aicoin news flash](https://www.aicoin.com/en/news-flash/2969704)

**Permissive models**
- TRELLIS: codebase and model weights MIT; submodules `diffoctreerast` (derived from diff-gaussian-rasterization) and a modified Flexicubes carry their own licenses; trained on TRELLIS-500K "curated from Objaverse(XL), ABO, 3D-FUTURE, HSSD, and Toys4k" — [microsoft/TRELLIS](https://github.com/microsoft/TRELLIS)
- TRELLIS.2: code MIT, weights MIT; trained on Objaverse-XL (Sketchfab subset) — [microsoft/TRELLIS.2](https://github.com/microsoft/TRELLIS.2)
- TripoSG: MIT license; 1.5B params — [VAST-AI-Research/TripoSG](https://github.com/VAST-AI-Research/TripoSG)
- Step1X-3D: "licensed under the Apache License 2.0" (weights and code); training set of 2M assets from Objaverse (320K), Objaverse-XL (480K), Objaverse texture subset (30K) plus proprietary assets — [stepfun-ai/Step1X-3D](https://github.com/stepfun-ai/Step1X-3D)
- InstantMesh: Apache-2.0 — [TencentARC/InstantMesh](https://github.com/TencentARC/InstantMesh)
- SAM 3D Objects: "model checkpoints and code are licensed under SAM License"; released 2025-11-19, encoder weights and "3D Artist Object Set" added June 2026 — [facebookresearch/sam-3d-objects](https://github.com/facebookresearch/sam-3d-objects). SAM License text: "non-exclusive, worldwide, non-transferable and royalty-free limited license" including commercial use; prohibits ITAR/trade-control-restricted end uses "including those related to military or warfare purposes", nuclear, espionage, illegal weapons; no territory exclusion; no MAU threshold; redistribution must include the agreement; derivative works owned by licensee — [sam-3d-objects LICENSE](https://raw.githubusercontent.com/facebookresearch/sam-3d-objects/main/LICENSE)
- SPAR3D: gated Hugging Face model with a LICENSE.md (Stability AI Community License) — [Stability-AI/stable-point-aware-3d](https://github.com/Stability-AI/stable-point-aware-3d) (threshold not verifiable here; see Gaps)

**Training-data risk**
- Objaverse data comes from Sketchfab under a mix of Creative Commons licenses; authors were not informed; Sketchfab's ToS prohibits use of user content for generative-AI development and the dataset includes models tagged NoAI — [The Decoder](https://the-decoder.com/sketchfab-objaverse-ai-copyright-dispute-enters-third-dimension/); [FlippedNormals (community)](https://blog.flippednormals.com/objaverse-raises-concerns-about-ethics-of-scraping-3d-content/)
- A reported legal complaint alleges defendants "built or powered commercial generative-3D systems using 3D assets harvested via Objaverse-XL while stripping the machine-readable copyright management information (CMI)" — [P4SC4L newsletter summary (secondary)](https://p4sc4l.beehiiv.com/p/defendants-allegedly-built-or-powered-commercial-generative-3d-systems-using-3d-assets-harvested-via)
- Licensed-data vendors argue aggregated datasets make buyers "inherit thousands of separate rights decisions you did not make and cannot audit" — [CGAxis (vendor opinion)](https://cgaxis.com/objaverse-alternative-licensed-3d-data/)

### Inferences
- Safest open models for a global commercial AR product, by license text: TRELLIS / TRELLIS.2 (MIT, best PBR quality), TripoSG (MIT, geometry only), Step1X-3D (Apache-2.0), InstantMesh (Apache-2.0, older/lower quality), SAM 3D Objects (SAM License, commercial OK with trade-control carve-outs). Hunyuan3D 2.x is the riskiest for Symspace because (a) EU/UK/South Korea users are outside the licensed Territory and (b) the 1M MAU trigger is low for a consumer commerce app; using Hunyuan via a hosted API (fal/Replicate/Tencent Cloud) does not obviously remove the end-user territorial restriction and should be reviewed by counsel.
- The MAU clause in Tencent's text is measured "on the ... version release date", i.e., it bites organisations that already had >1M MAU when the model was released, which is the Llama-style construction; still, treat it as a hard ceiling.
- Objaverse exposure is common to every model studied (TRELLIS, TRELLIS.2, Step1X-3D, and by reputation Hunyuan), so model choice does not eliminate dataset risk; document provenance, keep generated assets separable, and prefer models whose authors disclose training sources.

### Gaps
- Stability AI Community License revenue threshold for SPAR3D could not be verified (stability.ai and huggingface.co blocked); it is widely reported as a US$1M annual-revenue cap but that figure is not cited here.
- Hunyuan3D 2.1 training-data composition and whether Tencent's AUP/territory terms apply to outputs generated via third-party hosted APIs were not resolved.
- Hunyuan3D 3.0 open-weight status: no primary source found confirming open weights as of Oct 2026; treat 3.0 as API-only.
- HunyuanImage 3.0 license terms were not extractable from the README (LICENSE file not displayed).

---

## Key Question 5: Quality control (watertightness, floaters, scale normalisation, gravity alignment, background removal, input-image practices, multi-view input)

### Takeaway
Use the models' built-in cleaners (Hunyuan FloaterRemover/DegenerateFaceRemover, TRELLIS invisible-face removal + pymeshfix hole fill, Step1X-3D's watertight TSDF geometry) plus a trimesh/pymeshlab check for watertightness and component count; normalise scale from product metadata because all models emit unit-cube/sphere meshes; remove backgrounds before generation with an MIT model (BiRefNet, or rembg's `birefnet-general`/`isnet-general-use`) rather than BRIA RMBG-2.0 (CC BY-NC); and feed one centred object on a plain background in 3/4 view, upgrading to Hunyuan3D-2mv (front/back/left/right, 90° apart) or TRELLIS's tuning-free multi-image mode when back-side hallucination matters.

### Cited Findings
- Hunyuan3D 2.1 post-processors: FloaterRemover (drops small disconnected components), DegenerateFaceRemover, FaceReducer, mesh normalisation to unit sphere — [Hunyuan3D-2.1 postprocessors.py](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/hy3dshape/hy3dshape/postprocessors.py)
- TRELLIS `_fill_holes`: removes invisible faces via 500-view rasterisation, separates inner/outer surfaces with graph mincut, refines boundaries with pymeshfix; GLB export rotates Z-up to Y-up — [TRELLIS postprocessing_utils.py](https://raw.githubusercontent.com/microsoft/TRELLIS/main/trellis/utils/postprocessing_utils.py)
- Step1X-3D geometry stage produces "watertight TSDF representations" — [stepfun-ai/Step1X-3D](https://github.com/stepfun-ai/Step1X-3D)
- Hunyuan3D-2mv is finetuned from Hunyuan3D-2 for multiview-controlled shape generation; accepts front, back, left and right views (front only is also allowed); "front, back, and left should be 90° apart" — [Segmind model page](https://www.segmind.com/models/hunyuan3d-2mv); [ComfyUI docs](https://docs.comfy.org/tutorials/3d/hunyuan3d-2)
- TRELLIS implements "multi-image conditioning for TRELLIS-image model" as a "tuning-free algorithm without training a specialized model", which "may not give the best results for all input images" — [microsoft/TRELLIS](https://github.com/microsoft/TRELLIS)
- SAM 3D Objects takes a single image plus an object mask and is designed to handle "occlusion and clutter" — [facebookresearch/sam-3d-objects](https://github.com/facebookresearch/sam-3d-objects)
- rembg: MIT; models include `u2net`, `isnet-general-use`, `birefnet-general`, `birefnet-portrait`, `sam`, and the default `bria-rmbg`, which "is released under a BRIA license that requires a paid agreement for commercial use"; can run as an HTTP server (`rembg s --host 0.0.0.0 --port 7000`) — [danielgatis/rembg](https://github.com/danielgatis/rembg)
- BiRefNet: MIT license; variants general (swin_v1_large), lightweight (swin_v1_tiny), BiRefNet_HR (2048x2048), BiRefNet_dynamic (256-2304 px), matting variants; ~3.5 GB VRAM FP16, ~58 ms per 1024x1024 image on RTX 4090 — [ZhengPeng7/BiRefNet](https://github.com/ZhengPeng7/BiRefNet)
- BRIA RMBG-2.0: CC BY-NC 4.0 for non-commercial use; commercial use requires an agreement with BRIA; built on BiRefNet; trained on licensed data — [Bria-AI/RMBG-2.0](https://github.com/Bria-AI/RMBG-2.0)
- TripoSG bundles RMBG-1.4 for background removal — [VAST-AI-Research/TripoSG](https://github.com/VAST-AI-Research/TripoSG)
- Input-image guidance (community tutorial): clear subject on simple/white background; front or three-quarter angle preferred ("a slightly angled view gives it more information"); background removal is "the single biggest thing you can do to improve quality"; even lighting, no harsh shadows; at least 512x512; one object per image; transparent/glass objects are hard — [3D AI Studio tutorial (community)](https://www.3daistudio.com/blog/how-to-use-trellis-2-online-image-to-3d-tutorial)

### Inferences
- Automated QC gate (Python, in the worker): `trimesh.load(glb)` -> assert `mesh.is_watertight` (or run pymeshfix), count `mesh.split()` components (reject >1 after FloaterRemover unless product is genuinely multi-part), check `mesh.bounding_box.extents` and rescale to real dimensions, verify UVs exist and texture is power-of-2, check triangle count against the 50k ceiling, and ensure the glTF node has the base of the bounding box at y=0 for plane placement. Store the QC report with the asset.
- Gravity alignment: none of the models infer "up" from physics; the input image's camera-up becomes the mesh's up. Enforce "shoot the product upright" on the capture UI and, if needed, snap the mesh's principal axis to Y before export.
- For commercial compliance, run background removal with BiRefNet (MIT) or `isnet-general-use`; do not rely on rembg's default `bria-rmbg`.

### Gaps
- No open benchmark comparing background-removal models specifically for image-to-3D input quality was found.
- Era3D and other multi-view diffusion front-ends were not researched.
- No primary source on automatic orientation/gravity estimation for generated meshes.

---

## Key Question 6: Text-to-image front end for text-to-3D (3D-friendly single-object images) and licensing

### Takeaway
For a commercial pipeline, Apache-2.0 models are the safe picks: Z-Image / Z-Image-Turbo (Tongyi, 6B, Nov 2025), Qwen-Image, FLUX.1 [schnell], and FLUX.2-klein (4B); FLUX.1 [dev], FLUX.1 Kontext [dev] and FLUX.2 [dev] are non-commercial without a paid BFL license, and HunyuanImage 3.0 is an 80B MoE needing >=3x80 GB GPUs and has unverified license terms. Prompt for a single centred product on a plain white background in a three-quarter view with soft even lighting, then run background removal before the 3D model.

### Cited Findings
- FLUX.1 [schnell]: Apache-2.0; FLUX.1 [dev] and FLUX.1 Kontext [dev]: "FLUX.1-dev Non-Commercial License" — [black-forest-labs/flux](https://github.com/black-forest-labs/flux)
- FLUX.2 [dev] is released under the FLUX Non-Commercial License; BFL offers a self-hosted Dev license at $1,999/month for production; FLUX.2-klein-4B and klein-base-4B are Apache 2.0 — [Vercel AI Gateway model notes](https://vercel.com/ai-gateway/models/flux-2-klein-4b/about); [toolworthy (third-party)](https://www.toolworthy.ai/en/tool/flux-2)
- Qwen-Image: Apache 2.0 — [sozee (third-party roundup)](https://sozee.ai/resources/best-open-source-image-models)
- Z-Image: Tongyi-MAI (Alibaba) 6B S3-DiT; Z-Image-Turbo released 2025-11-27 under Apache 2.0, runs on 16 GB consumer GPUs, bilingual text rendering; base Z-Image released 2026-01-27 — [comfyui-wiki](https://comfyui-wiki.com/en/models/z-image/z-image); [codesota](https://codesota.com/news/z-image-turbo-consumer-gpu)
- HunyuanImage 3.0: 80B total / 13B active MoE; ">= 3 x 80 GB" VRAM (Instruct variant >= 8 x 80 GB); base model lacks prompt enhancement, authors "recommend community partners to use deepseek to rewrite the prompts" — [Tencent-Hunyuan/HunyuanImage-3.0](https://github.com/Tencent-Hunyuan/HunyuanImage-3.0)
- Image guidance applicable to generated inputs: white/simple background, front or three-quarter view, even lighting, single object, >=512 px — [3D AI Studio tutorial (community)](https://www.3daistudio.com/blog/how-to-use-trellis-2-online-image-to-3d-tutorial)

### Inferences
- Prompt template (inferred from the above input-image practices; not from a vendor doc): "studio product photograph of a single {object}, centered, three-quarter view, isolated on a seamless pure white background, soft even diffuse lighting, no shadows, no reflections, no text, no people, full object visible, 4k". Negative/avoid: "cropped, multiple objects, glass, transparent, cluttered, dramatic lighting".
- Z-Image-Turbo is the cheapest commercial-safe choice (sub-second on datacenter GPUs, 16 GB cards), and FLUX.1 [schnell]/FLUX.2-klein are proven alternatives; keep FLUX [dev] variants out of production unless the BFL license is purchased.

### Gaps
- No head-to-head study ranking T2I models by downstream image-to-3D quality was found; the "3D-friendly" claim rests on general image-quality rankings and input-image practices.
- Qwen-Image and FLUX.2 license facts come from third-party summaries, not the model cards (huggingface.co blocked).
- HunyuanImage 3.0 license text (likely a Tencent community license with territory/MAU clauses similar to Hunyuan3D) was not verified.
- SD3.5 (Stability Community License) was not researched in this pass.
