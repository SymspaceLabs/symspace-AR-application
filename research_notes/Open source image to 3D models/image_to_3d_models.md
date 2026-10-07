# Open-Source / Open-Weight Image-to-3D Generative Models (state as of 2026-10-07)

Research scope: open-weight single-image and multi-image to 3D models (2024 – Oct 2026), with emphasis on what matters for a Unity-based mobile AR pipeline (GLB/USDZ, PBR, polycount, licensing). Sources are primarily GitHub READMEs/LICENSE files, official project pages, and arXiv/HF listings surfaced via search. Several hosts were unreachable from this environment (huggingface.co, arxiv.org, docs.comfy.org, most SEO comparison blogs), so some facts come from search-result snippets and are flagged as such.

Research-environment note: `huggingface.co`, `arxiv.org`, `alphaxiv.org`, `cdn.jsdelivr.net`, `docs.comfy.org`, `comfyui-wiki.com`, `3daistudio.com`, `triposr.org`, `pixazo.ai`, `sloyd.ai`, `top3d.ai`, `tripo3d.ai`, `*.github.io` and the GitHub REST API were blocked. GitHub web pages (github.com) were reachable. Where a fact comes only from a search snippet of a blocked page, it is marked "(snippet)".

---

## Key Question 1: Per-model fact sheet (paper, repo, license, date, architecture, output, PBR, VRAM/speed, metrics)

### Takeaway
As of October 2026 the open-weight frontier is (a) Microsoft TRELLIS.2 (Dec 2025, MIT, 4B params, O-Voxel latents, native PBR incl. opacity, needs 24 GB VRAM), (b) its pixel-aligned derivative Pixal3D from TencentARC (SIGGRAPH 2026, MIT, TRELLIS.2 backbone since May 2026), (c) Tencent Hunyuan3D-2.1 (Jun 2025, 3.3B shape + 2B PBR paint, Tencent Community License with EU/UK/South-Korea exclusion; still the newest open Hunyuan3D weights — 2.5/3.0/3.1/PolyGen are hosted-only), and (d) Meta SAM 3D Objects (Nov 2025, SAM License, strong on real-world cluttered photos, outputs Gaussian splats + mesh). TripoSG (VAST, MIT, 1.5B, geometry only), Step1X-3D (Apache-2.0), Direct3D-S2 (MIT), and the Stability SF3D/SPAR3D pair (Stability Community License, revenue-capped) fill out the practical tier; most 2024-era multi-view+LRM methods (InstantMesh, CRM, LGM, Unique3D, Wonder3D, Era3D) are superseded in quality but remain useful for low-VRAM/speed.

### Cited Findings

#### TRELLIS.2 (Microsoft) — current open-weight quality leader
- Paper: arXiv 2512.14692 (Dec 2025), "Native and Compact Structured Latents for 3D Generation"; CVPR 2026 oral — [GitHub](https://github.com/microsoft/TRELLIS.2); [awesome-3D-Generative-Models listing](https://github.com/wendashi/awesome-3D-Generative-Models)
- 4B parameters; "field-free" sparse voxel representation **O-Voxel** with a sparse 3D VAE (16× spatial downsampling) and vanilla Diffusion Transformers; outputs textured meshes with full PBR (base color, roughness, metallic, opacity) — [GitHub README](https://github.com/microsoft/TRELLIS.2)
- License: code MIT, weights MIT; dependencies nvdiffrast / nvdiffrec carry separate (NVIDIA) licenses — [GitHub README](https://github.com/microsoft/TRELLIS.2)
- Speed on H100: 512³ ≈ 3 s (2 s shape + 1 s material); 1024³ ≈ 17 s (10 + 7); 1536³ ≈ 60 s (35 + 25) — [GitHub README](https://github.com/microsoft/TRELLIS.2)
- Hardware: "an NVIDIA GPU with at least 24GB of memory is necessary"; Linux only; CUDA 12.4 recommended; flash-attention default with xformers fallback for older GPUs — [GitHub README](https://github.com/microsoft/TRELLIS.2)
- Export: GLB with WebP texture compression; README example `to_glb()` uses `decimation_target=1,000,000`, `texture_size=4096`, `remesh=True`, `remesh_band=1`, `remesh_project=0`, preceded by `simplify=16,777,216` — [GitHub README](https://github.com/microsoft/TRELLIS.2)
- HF model `microsoft/TRELLIS.2-4B`, HF demo `microsoft/TRELLIS.2`; texture-generation code and training/fine-tuning code released — [GitHub README](https://github.com/microsoft/TRELLIS.2)
- Third-party summary (snippet): "4-billion-parameter model that produces 1536-resolution assets in under 20 seconds on a single 24GB GPU" — [3daistudio State of AI 3D 2026 (snippet)](https://www.3daistudio.com/state-of-ai-3d-generation-2026)
- Known artifacts (from a 2026 paper that builds on it): meshes "occasionally contain holes, since it does not enforce a watertight-mesh assumption", textures "exhibit a color-shifting artifact, which could result in the generation of metallic surfaces for transparent objects" — [search summary citing arXiv 2607.00382 / 2606.04108](https://arxiv.org/pdf/2607.00382)
- A GGUF quantization by a community user exists (`ChrisColeTech/TRELLIS.2-4B-GGUF`) — [HF listing via search](https://huggingface.co/ChrisColeTech/TRELLIS.2-4B-GGUF)

#### TRELLIS (v1, Microsoft) — still widely used base
- Paper arXiv 2412.01506 (Dec 2024), CVPR 2025 Spotlight; training code + text models released 2025-03-25 — [GitHub](https://github.com/microsoft/TRELLIS)
- Models: TRELLIS-image-large 1.2B; text-base 342M, text-large 1.1B, text-xlarge 2.0B; Structured LATent (SLAT) + rectified-flow transformers; trained on ~500K objects — [GitHub](https://github.com/microsoft/TRELLIS)
- Outputs 3D Gaussians, radiance fields, and meshes; GLB export; multi-image conditioning; min 16 GB VRAM — [GitHub](https://github.com/microsoft/TRELLIS)
- License: code and weights MIT; submodules (diffoctreerast, modified FlexiCubes) under their own licenses — [GitHub](https://github.com/microsoft/TRELLIS)

#### Pixal3D (TencentARC) — newest notable 2026 open release
- "Pixel-Aligned 3D Generation from Images", SIGGRAPH 2026 (accepted Apr 2026), arXiv 2605.10922; lifts pixel features into 3D by back-projection for "near-reconstruction-level fidelity with detailed geometry and PBR textures" — [GitHub](https://github.com/TencentARC/Pixal3D); [search summary](https://github.com/TencentARC/Pixal3D/blob/master/README.md)
- Main branch = improved implementation on the **TRELLIS.2 backbone (released May 2026)**; `paper` branch = original Direct3D-S2-based implementation matching the paper — [GitHub](https://github.com/TencentARC/Pixal3D)
- Three-stage cascade: sparse structure (32→64), shape (256→512→1024), texture (256→512→1024); pixel-aligned projection conditioning with view-aligned latents (2 views default) — [GitHub](https://github.com/TencentARC/Pixal3D)
- Output GLB with PBR; standard mode 1536 res; low-VRAM mode 1024 res with on-demand model loading — [GitHub](https://github.com/TencentARC/Pixal3D)
- License: MIT for code; third-party components retain their licenses (NOTICE file); weights on HF `TencentARC/Pixal3D` — [GitHub](https://github.com/TencentARC/Pixal3D); [HF listing via search](https://huggingface.co/TencentARC/Pixal3D)
- ComfyUI: native core support ("Trellis.2 and Pixal3D are now first-class citizens inside ComfyUI with native support built directly into ComfyUI core") plus community wrapper `Saganaki22/Pixal3D-ComfyUI` — [NextDiffusion tutorial (snippet)](https://www.nextdiffusion.ai/tutorials/trellis-2-pixal3d-native-image-to-3d-generation-inside-comfyui); [GitHub](https://github.com/TencentARC/Pixal3D)

#### Hunyuan3D-2.1 (Tencent) — newest OPEN Hunyuan3D weights
- Released 2025-06-13; tech report 2025-06-19 — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1)
- Components: Hunyuan3D-Shape-v2-1 (3.3B, image-to-shape, flow-matching DiT on VecSet latents) + Hunyuan3D-Paint-v2-1 (2B, PBR texture: albedo/metallic/roughness) — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1); [awesome-3D-Generative-Models](https://github.com/wendashi/awesome-3D-Generative-Models)
- VRAM: shape 10 GB, texture 21 GB, both 29 GB — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1)
- Benchmarks (own README): shape ULIP-T 0.0774 / ULIP-I 0.1395 / Uni3D-T 0.2556 / Uni3D-I 0.3213; texture CLIP-FID 24.78 / CMMD 2.191 / CLIP-I 0.9207 / LPIPS 0.1211 — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1)
- License: **Tencent Hunyuan 3D 2.1 Community License Agreement**; territory excludes "the European Union, United Kingdom and South Korea"; >1M MAU in preceding month requires commercial license from Tencent; outputs may not be used to improve other AI models; deployed services must disclose that Tencent is not affiliated — [LICENSE](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1/blob/main/LICENSE)
- Hunyuan3D-2.5 (hosted, Apr 23 2025; report Jun 23 2025) and Hunyuan3D-PolyGen (Jul 8 2025) have **no open weights**; community issue asking for open-source plans (opened 2025-07-17) has no maintainer response — [Issue #111](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1/issues/111)
- Hunyuan3D 3.0 and 3.1: hosted only (Hunyuan 3D platform/Studio, Tencent Cloud API, and "since February 2026, ComfyUI partner nodes that call the hosted model"); "no open weights for 3.0 or 3.1" (snippet, SEO blog) — [triposr.org (snippet)](https://triposr.org/blog/hunyuan3d-versions)
- Hunyuan3D 3.1: up to 8 multi-view inputs, up to 1.5M faces, optional PBR; launched on 3d.hunyuanglobal.com with 20 free generations/day — [Tencent announcement (snippet)](https://www.tencent.com/en-us/articles/2202235.html); [scenario.com model page (snippet)](https://www.scenario.com/models/hunyuan-3d-31-pro-multiview)
- LATTICE (arXiv 2512.03052, Nov/Dec 2025): "family of large-scale image-to-3D generation models with up to 4.5 billion parameters" (0.6B / 1.9B / 4.5B); described as the foundation behind Hunyuan3D 2.5/3.0; GitHub repo `zeqiang-lai/lattice` referenced but returned 404 when fetched and "no releases have been published" (snippet) — [search summary](https://arxiv.org/html/2512.03052v1); [LATTICE page](https://lattice3d.github.io/)

#### Hunyuan3D-2 / 2mini / 2mv / Turbo (Tencent)
- Variants and dates: DiT-v2-0 1.1B (Jan 21 2025), -Fast (Feb 3 2025), -Turbo (Mar 19 2025); Paint-v2-0 1.3B (Jan 21 2025), Paint-Turbo (Apr 1 2025); 2mini 0.6B family (Mar 18–19 2025); 2mv multiview 1.1B family (Mar 18–19 2025); Delight-v2-0 1.3B — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2)
- VRAM: shape 6 GB min; shape+texture 16 GB — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2)
- Benchmarks (README): CMMD 3.193, FID_CLIP 49.165, FID 282.429, CLIP-score 0.809 — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2)
- Output: trimesh objects → GLB/OBJ; integrations: ComfyUI-3D-Pack, ComfyUI-Hunyuan3DWrapper, Blender addon, local API server — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2)
- License: Tencent Hunyuan 3D 2.0 Community License; "DOES NOT APPLY IN THE EUROPEAN UNION, UNITED KINGDOM AND SOUTH KOREA"; >1M MAU requires approval via hunyuan3d@tencent.com; NOTICE attribution; cannot use outputs to train competing models — [LICENSE](https://github.com/Tencent-Hunyuan/Hunyuan3D-2/blob/main/LICENSE)

#### Hunyuan3D-Omni and Hunyuan3D-Part (Tencent, Sept 2025)
- Hunyuan3D-Omni: released 2025-09-25; controllable image-to-shape with point cloud / voxel / bbox / skeleton control via a unified control encoder; 3.3B; 10 GB VRAM; License.txt + Notice.txt present (terms not extracted) — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-Omni)
- Hunyuan3D-Part: P3-SAM (native 3D part segmentation, arXiv 2509.06784) + X-Part (shape decomposition, arXiv 2509.08643); input any mesh, outputs part segmentation + complete parts; weights `tencent/Hunyuan3D-Part`; README says it is a "light version" with full version on Hunyuan3D-Studio; license not stated in README excerpt — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-Part); [README](https://github.com/Tencent-Hunyuan/Hunyuan3D-Part/blob/main/README.md)

#### SAM 3D Objects (Meta, Nov 2025)
- Released 2025-11-19 (checkpoints, web demo, paper "SAM 3D: 3Dfy Anything in Images", arXiv 2511.16624); encoder weights 2026-06-01; "3D Artist Object Set and HF Leaderboard" 2026-06-02 — [GitHub README](https://github.com/facebookresearch/sam-3d-objects/blob/main/README.md)
- Architecture (secondary sources): two-stage; geometry model is a 1.2B-parameter flow transformer with Mixture-of-Transformers (shape stream + layout stream), DINOv2 image encoder; texture & refinement stage is a 600M sparse latent flow transformer on active voxels — [Roboflow blog (snippet)](https://blog.roboflow.com/sam-3d/)
- Outputs: shape, texture, and layout/pose from a single image; Gaussian splat export `output["gs"].save_ply(...)`; mesh referenced but GLB/OBJ export not documented in README — [GitHub README](https://github.com/facebookresearch/sam-3d-objects/blob/main/README.md)
- Claims: "outperforms prior 3D generation models in human preference tests on real-world objects and scenes"; win rate "at least 5:1 (object), 6:1 (scene)" on SA-3DAO, LVIS, MetaCLIP; post-training on 3.14M shapes / 100K textures — [Roboflow blog (snippet)](https://blog.roboflow.com/sam-3d/); [GitHub](https://github.com/facebookresearch/sam-3d-objects)
- SA-3DAO benchmark: 1,000 image/artist-mesh pairs; 100 public, 900 withheld for leaderboard — [Meta dataset page (snippet)](https://ai.meta.com/datasets/sa-3dao-sam-3d-artist-objects/)
- License: **SAM License** (updated 2025-11-19): royalty-free, worldwide, commercial use permitted; prohibits ITAR/military/nuclear/espionage/weapons uses, reverse engineering; must not be a Trade Controls target; patent-litigation termination — [LICENSE](https://github.com/facebookresearch/sam-3d-objects/blob/main/LICENSE)
- Weights access on HF is gated (login/approval) (snippet) — [Roboflow model page](https://playground.roboflow.com/models/meta/sam-3d-objects)
- VRAM / inference time not specified in README — [GitHub README](https://github.com/facebookresearch/sam-3d-objects/blob/main/README.md)

#### TripoSG / TripoSF (VAST, Mar 2025)
- TripoSG: 1.5B rectified-flow transformer (paper describes MoE), SDF-based VAE with hybrid SDF/normal/eikonal supervision; 2048 latent tokens; trained on 2M image-SDF pairs; min 8 GB VRAM; output GLB with face-count limiting; **no texture generation**; code MIT; TripoSG-scribble variant Apr 2025 — [GitHub](https://github.com/VAST-AI-Research/TripoSG)
- TripoSF: Mar 2025; only the **SparseFlex VAE** (1024³ reconstruction) + inference scripts are released, not a full image-to-3D generator ("full version to be unveiled in Tripo 3.0"); MIT; ≥12 GB VRAM for 1024³ — [GitHub](https://github.com/VAST-AI-Research/TripoSF); [Tripo tweet (snippet)](https://x.com/tripoai/status/1906740953647648776)
- VAST "Open Source Month" (Mar 2025) released eight projects across the pipeline (TripoSG, TripoSF, plus related: MV-Adapter, UniRig, HoloPart, MIDI) (snippet) — [VAST blog (snippet)](https://www.tripo3d.ai/blog/vast-open-source-month)
- Tripo 3.0 (hosted) and a ~$200M Series A+/A++ round (Jun 2026) — no evidence of newer open weights from VAST in 2026 — [GlobeNewswire](https://www.globenewswire.com/news-release/2026/06/01/3304603/0/en/tripo-ai-raises-nearly-200-million-in-series-a-and-series-a-financing-to-advance-ai-3d-and-world-model-roadmap.html)

#### Step1X-3D (StepFun, May 2025)
- Released 2025-05-13 with inference + training code, weights, dataset; arXiv 2505.07747 — [GitHub](https://github.com/stepfun-ai/Step1X-3D)
- Geometry: 1.3B hybrid VAE-DiT (perceiver latent encoding + sharp-edge sampling) producing watertight TSDF; texture: 3.5B SD-XL-based multi-view texture synthesis with geometric conditioning — [GitHub](https://github.com/stepfun-ai/Step1X-3D)
- License **Apache-2.0** (code and weights); VRAM 27–29 GB; 152 s for 50 steps (geometry + texture); outputs GLB; dataset subsets (320K Objaverse, 480K Objaverse-XL, 30K texture) on ModelScope; ComfyUI "listed in plan but not yet released" — [GitHub](https://github.com/stepfun-ai/Step1X-3D)

#### Direct3D-S2 (DreamTech, May 2025)
- Paper 2025-05-26 (arXiv 2505.17412), NeurIPS 2025; code/model 2025-05-30; v1.1 ~2× faster; v1.2 (character generation) announced in prep 2025-06-03 — [GitHub](https://github.com/DreamTechAI/Direct3D-S2)
- Sparse volumetric SDF with Spatial Sparse Attention (SSA); up to 1024³; VRAM ≥10 GB at 512, ~24 GB at 1024; output OBJ with optional remeshing; geometry only (no texture); code MIT; HF model card lists MIT (snippet) — [GitHub](https://github.com/DreamTechAI/Direct3D-S2)

#### Stability AI: SF3D (Aug 2024) and SPAR3D (Jan 2025)
- SF3D: based on TripoSR, adds UV unwrapping, illumination disentanglement (delighting), material-parameter prediction; ~6 GB VRAM; GLB output; remesh options none / triangle / quad; gated HF weights — [GitHub](https://github.com/Stability-AI/stable-fast-3d)
- SPAR3D: Jan 2025; point-cloud-conditioned (point diffusion) improves back side; ~10.5 GB VRAM (7 GB low-VRAM mode); GLB with PBR material params; remesh none/triangle/quad with target vertex/face counts; experimental Windows/MPS — [GitHub](https://github.com/Stability-AI/stable-point-aware-3d)
- License (both): **Stability AI Community License** — free for research and commercial use **below USD 1M annual revenue**; above that the license terminates and an enterprise license is required; "Powered by Stability AI" attribution; no use to train foundational generative models — [SPAR3D LICENSE.md](https://github.com/Stability-AI/stable-point-aware-3d/blob/main/LICENSE.md); [SF3D LICENSE.md](https://github.com/Stability-AI/stable-fast-3d/blob/main/LICENSE.md)
- TripoSR (Stability + VAST, Mar 2024): "<500 ms on an A100", ~6 GB VRAM (snippet) — [cmarix blog (snippet)](https://www.cmarix.com/blog/top-open-source-ai-models-for-3d-image-generation/)

#### Hi3DGen / Stable3DGen (Stable-X, Mar 2025)
- "High-fidelity 3D Geometry Generation from Images via Normal Bridging" (arXiv 2503.22236); framework adapted from TRELLIS; MIT code and weights; NVIDIA dependencies (kaolin, nvdiffrast, flexicubes) removed specifically "to enable commercial use"; geometry-focused; marked WIP — [GitHub](https://github.com/Stable-X/Hi3DGen)

#### Part-aware generation
- PartCrafter (NeurIPS 2025): arXiv 2025-06-09, code 2025-07-13, HF demo 2025-08-15, scene version 2025-07-23; fine-tunes TripoSG's DiT (VAE fixed); single image → multiple part meshes in one shot; ≥8 GB VRAM; GLB; MIT — [GitHub](https://github.com/wgsxm/PartCrafter)
- PartCrafter vs HoloPart: Chamfer 0.1726 vs 0.1916, F-score 0.7472 vs 0.6916, 34 s vs 18 min (snippet from paper) — [PartCrafter paper (snippet)](https://arxiv.org/pdf/2506.05573)
- PartPacker (NVIDIA): dual volume packing; ~10 GB VRAM fp16; **NVIDIA Source Code License — non-commercial only** ("only may be used or intended for use non-commercially") — [license.md](https://github.com/NVlabs/PartPacker/blob/main/license.md)
- HoloPart (VAST, Apr 2025): amodal part completion from a pre-segmented GLB; MIT; built on TripoSG — [GitHub](https://github.com/VAST-AI-Research/HoloPart)
- Related 2025–26 part work: OmniPart (2507.06165), X-Part (2509.08643), FullPart (2510.26140), UniPart (2512.09435), ISAP-3D (2606.12099), SAM3D-Part (SIGGRAPH Asia 2026, interactive part generation from a click) — [search results](https://github.com/Jiahao620/sam3d-part)

#### Artist-mesh / autoregressive retopology
- MeshAnything V2 (ICCV 2025, arXiv 2408.02555): adjacent mesh tokenization; trained on <1600 faces and "cannot generate meshes with more than 1600 faces"; ~8 GB and ~45 s on A6000; needs sharp input geometry, "performs poorly with feed-forward 3D generation outputs"; license file not retrievable (404) — [GitHub](https://github.com/buaacyw/MeshAnythingV2)
- BPT (Tencent, CVPR 2025, arXiv 2411.07025): blocked + patchified tokenization, ~75% shorter sequences, meshes >8k faces; ~12 GB fp16; ~2 min per mesh; point-cloud/image/text input; license file not retrievable — [GitHub](https://github.com/Tencent-Hunyuan/bpt)
- DeepMesh (ICCV 2025): weights 2025-03-20, optimized inference 2025-04-01; 0.5B (1B planned); autoregressive + DPO; up to ~30k faces (snippet); input point clouds with normals; Apache-2.0; HF `zzzrw/DeepMesh` — [GitHub](https://github.com/zhaorw02/DeepMesh); [alphaxiv (snippet)](https://www.alphaxiv.org/overview/2503.15265)
- Mesh-RFT (Tencent Hunyuan + HKUST, NeurIPS 2025 Spotlight): Masked DPO at face granularity; -24.6% Hausdorff, +3.8% topology score vs pretrained; repo has only README + MIT LICENSE, 1 commit, no releases — [GitHub](https://github.com/hitcslj/Mesh-RFT); [search summary](https://hitcslj.github.io/mesh-rft/)
- LATO.2 (Meshy AI + HUST + TUM, Jul 2026, arXiv 2607.10623): factorizes into Vertex Flow + Topology Flow; input an existing mesh; output OBJ with controllable vertex count (default 2000, range 200–5000); ~8 GB VRAM; ~5 s per mesh on H800; code + HF weights + Space; **MIT**; README warns meshes "may still contain holes and incorrect connectivity" — [GitHub](https://github.com/LoHhhha/LATO.2)
- Newer AR-mesh papers 2025–26 (code status not verified): FlashMesh (CVPR 2026), MeshRipple (2512.07514), QuadGPT (2509.21420), Mesh-Pro (2603.00526), LATO (ICML 2026) — [search results](https://github.com/TianhaoZhao668/LATO)
- Hunyuan3D-PolyGen (Jul 8 2025): autoregressive art-grade mesh/retopology, **hosted only** — [Tencent tweet (snippet)](https://x.com/TencentHunyuan/status/1942174221976981881)

#### 2024-era multi-view diffusion + reconstruction / LRM family (still open, mostly superseded)
- InstantMesh (TencentARC, Apr 2024): LRM/Instant3D-style sparse-view reconstruction on Zero123++ views; OBJ with vertex color by default, `--export_texmap` for UV texture; code Apache-2.0 — but depends on Zero123++ — [GitHub](https://github.com/TencentARC/InstantMesh)
- Zero123++ (SUDO AI): code Apache-2.0, **weights CC-BY-NC 4.0** ("cannot use the model in a commercial product pipeline, but you can still use the outputs") — [GitHub (snippet)](https://github.com/SUDO-AI-3D/zero123plus)
- One-2-3-45++ repo exists; original One-2-3-45 is Apache-2.0 — [GitHub](https://github.com/SUDO-AI-3D/One2345plus); [One-2-3-45](https://github.com/One-2-3-45/One-2-3-45)
- CRM (ECCV 2024): ~10 s; OBJ + UV texture; MIT — [GitHub](https://github.com/thu-ml/CRM)
- LGM (ECCV 2024 oral): Gaussian splats (PLY) + mesh conversion script; ~10 GB VRAM; MIT — [GitHub](https://github.com/3DTopia/LGM)
- Unique3D (NeurIPS 2024): ~30 s; MIT (repo); sensitive to facing direction and occlusion — [GitHub](https://github.com/AiuniAI/Unique3D). Note: one aggregator says CC BY-NC-SA 4.0 (snippet) — conflict, treat repo as authoritative but verify weights card — [search summary](https://arxiv.org/html/2405.20343v1)
- Wonder3D: MIT per repo footer (an aggregator says AGPL-3.0 — conflict); Wonder3D++ branch 2024-12-22; 2–3 min — [GitHub](https://github.com/xxlong0/Wonder3D)
- Era3D: **AGPL-3.0** (copyleft) (snippet) — [GitHub](https://github.com/pengHTYX/Era3D)

#### Models that are NOT (or not fully) open
- Sparc3D: project repo has no releases; issue #22 (2025-06-26) alleges code/weights never released and HF demo removed to push users to commercial "hitem3d"; no maintainer reply — [Issue #22](https://github.com/lizhihao6/Sparc3D/issues/22); [Releases](https://github.com/lizhihao6/Sparc3D/releases)
- Ultra3D (Jul 2025): repo contains only a project web page; awesome-list marks it "closed/proprietary" — [GitHub](https://github.com/buaacyw/Ultra3D); [awesome list](https://github.com/wendashi/awesome-3D-Generative-Models)
- Seed3D 1.0 (ByteDance, Oct 2025, arXiv 2510.19944): DiT-based, "simulation-ready" assets; available via Volcano Engine API; GitHub org only hosts Dora/Puppeteer/MagicArticulate (Apache-2.0), no Seed3D weights — [GitHub org](https://github.com/Seed3D); [ByteDance blog (snippet)](https://seed.bytedance.com/en/blog/seed3d-1-0-released-generate-high-fidelity-3d-models-from-single-images-featuring-sota-texturing)
- Amodal3R (ICCV 2025): TRELLIS-based occlusion-aware reconstruction; HF weights released for sparse-structure and SLAT modules; LICENSE.txt present (terms not extracted) — [GitHub](https://github.com/Sm0kyWu/Amodal3R)
- Pandora3D / Tencent-XR-3DGen: full code + Google-Drive weights; "MIT License with additional restrictions prohibiting use for harmful purposes, discriminatory applications, or within the European Union" — [GitHub](https://github.com/Tencent/Tencent-XR-3DGen)
- Rodin / Meshy / Tripo commercial engines: no open weights found other than Meshy's LATO.2 retopology model (above) and VAST's 2025 releases.

### Inferences
- For a Unity AR pipeline with commercial intent and possible EU users, the cleanest licenses are TRELLIS.2 / Pixal3D / TRELLIS (MIT), TripoSG, Direct3D-S2, Hi3DGen, PartCrafter, HoloPart, LATO.2 (MIT), Step1X-3D and DeepMesh (Apache-2.0), SAM 3D Objects (SAM License, commercial OK). Hunyuan3D 2.x is unusable in EU/UK/South Korea, Stability models stop being free above $1M revenue, PartPacker is non-commercial, and Zero123++-based pipelines (InstantMesh) inherit a CC-BY-NC weight license.
- Hunyuan3D's open line has effectively frozen at 2.1 (June 2025); every later Tencent 3D model (2.5, PolyGen, 3.0, 3.1, LATTICE) is hosted-only as of Oct 2026.
- TRELLIS.2 is becoming the de facto open backbone (Pixal3D, SymTRELLIS build on it) much as TRELLIS v1 was for Hi3DGen/Amodal3R.

### Gaps
- Could not retrieve arXiv/HF pages directly, so parameter counts for SAM 3D Objects and Pixal3D benchmark tables come from secondary sources or are missing.
- License text of BPT, MeshAnything V2, Hunyuan3D-Part, Hunyuan3D-Omni, Amodal3R not retrievable (404 on guessed paths / blocked); verify before commercial use.
- Direct3D-S2 and TripoSG weight licenses on HF not directly verified (MIT per HF card snippet / repo).
- No reliable inference-time numbers for SAM 3D Objects, Hunyuan3D-2.1 end-to-end, or Pixal3D.

---

## Key Question 2: Who tops community leaderboards (3D Arena, 3DGen-Arena, SA-3DAO), with scores and dates?

### Takeaway
3D Arena (HF, Dylan Ebert) is the main public image-to-3D preference leaderboard; its June 2025 paper snapshot had TRELLIS-3DGS 1384, TRELLIS (mesh) 1306 and Hunyuan3D-2 1298 ELO; third-party snapshots in 2026 report hosted Hunyuan3D-2.5 at ~1325 leading with TRELLIS/TRELLIS 2 around 1290–1306 (dates and exact values could not be verified because huggingface.co was unreachable). Meta's SA-3DAO leaderboard (June 2026) is the newest benchmark for real-photo reconstruction but no public standings were retrievable.

### Cited Findings
- 3D Arena paper (arXiv 2506.18787, June 2025): since June 2024 collected 123,243 votes from 8,096 users across 19 models; ELO-based pairwise ranking; Gaussian-splat outputs get +16.6 ELO over meshes; textured models +144.1 ELO over untextured; identical TRELLIS model gains +78 ELO just from splat rendering — [HF paper page (snippet)](https://huggingface.co/papers/2506.18787); [arXiv (snippet)](https://arxiv.org/html/2506.18787v1)
- Paper snapshot numbers (snippet): TRELLIS mesh ELO 1306 (4877 votes, 67.0% win rate); Hunyuan3D-2 ELO 1298 (4195 votes, 65.5%); TRELLIS-3DGS 1384 — [search summary of arXiv 2506.18787](https://arxiv.org/html/2506.18787v1)
- Aggregator snapshot (snippet, described as "May 2026"): Hunyuan3D-2.5 leads at 1325; TRELLIS at 1290 (another phrasing: TRELLIS 2 #2 at 1306, Hunyuan3D-2 #3 at 1298; Meshy 5 following) — [pixazo leaderboard (snippet)](https://www.pixazo.ai/leaderboard/ai-3d-model-generation); [sloyd blog (snippet)](https://www.sloyd.ai/blog/ai-3d-model-generator-rankings). Caveat: these two numbers sets look like the paper's 2025 values re-labelled; treat the "May 2026" date as unverified.
- Live leaderboard location: HF Space `3d-arena/3d-arena` (formerly `dylanebert/3d-arena`); dataset `3d-arena/3d-arena` — [HF Space](https://huggingface.co/spaces/3d-arena/3d-arena); [HF dataset](https://huggingface.co/datasets/3d-arena/3d-arena)
- Dylan Ebert on adding Hunyuan3D-2 (Jan 2025): "definitely a leader, but may still be outperformed by TRELLIS, which is smaller and produces less dense topology" — [X post](https://x.com/dylan_ebert_/status/1882189255755387004)
- Community topology voting (snippet): "Hunyuan3D-2 ranked #1 for topology, TRELLIS ranked #2, and Hunyuan3D-2.1 ranked #3" — [trellis2.com blog (snippet)](https://trellis2.com/blog/trellis-2-vs-hunyuan3d-image-to-3d)
- Low-trust alternative arena top3d.ai (small vote counts): Hunyuan 3D v3.1 #6 at 1059 ELO (78 wins, 66.1%); TRELLIS.2 #20 at 874 (27 wins, 23.3%); SAM 3D #21 at 816 (3 wins, 2.4%) — [top3d.ai (snippet)](https://www.top3d.ai/leaderboard)
- 3DGen-Arena / 3DGen-Bench (arXiv 2503.21745): arena-style with normal-map, untextured and textured 360° videos; 9 text-to-3D and 13 image-to-3D models; 8,045 public votes at paper time; trains 3DGen-Score (CLIP) and 3DGen-Eval (MLLM) automatic judges — [arXiv (snippet)](https://arxiv.org/pdf/2503.21745)
- SA-3DAO (Meta): 1,000 artist-made image-aligned meshes; 100 public, 900 withheld; HF leaderboard opened 2026-06-02 — [Meta dataset page (snippet)](https://ai.meta.com/datasets/sa-3dao-sam-3d-artist-objects/); [GitHub README](https://github.com/facebookresearch/sam-3d-objects/blob/main/README.md)
- SAM 3D paper claims ≥5:1 (objects) and ≥6:1 (scenes) human-preference win rates over "other leading models" on SA-3DAO/LVIS/MetaCLIP (snippet) — [Roboflow blog](https://blog.roboflow.com/sam-3d/)
- SymTRELLIS (arXiv 2606.04108, 2 Jun 2026): on 266 symmetric objects, reduces symmetry error vs TRELLIS.2, Hunyuan3D-2.1 and TripoSG while keeping reconstruction accuracy; works on top of TRELLIS.2 without retraining — [HF paper page (snippet)](https://huggingface.co/papers/2606.04108)
- Multiple 2026 SEO comparison sites call TRELLIS 2 "the quality leader among fully open models" (snippet) — [cmarix (snippet)](https://www.cmarix.com/blog/top-open-source-ai-models-for-3d-image-generation/); [3daistudio (snippet)](https://www.3daistudio.com/state-of-ai-3d-generation-2026)

### Inferences
- Among open weights, TRELLIS.2 (and Pixal3D on top of it) is the consensus leader in 2026 for geometry fidelity; Hunyuan3D-2.1 keeps an edge for texture quality via its dedicated PBR paint model; SAM 3D Objects wins on in-the-wild photos with occlusion/clutter rather than clean product shots.
- 3D Arena rankings are strongly influenced by rendering (splat vs mesh, textured vs not), so ELO alone is a poor proxy for AR mesh usability.

### Gaps
- Could not load the live 3D Arena leaderboard (huggingface.co blocked); exact current ELO for TRELLIS.2, Pixal3D, SAM 3D Objects, Hunyuan3D-2.1 on 3D Arena is unverified.
- No public SA-3DAO leaderboard standings were found.
- No independent Toys4K/GSO numeric comparison across the 2026 models was found.

---

## Key Question 3: Mesh topology, UV/PBR textures, and real-time AR (Unity, GLB/USDZ) suitability; polycount defaults and remeshing tooling

### Takeaway
All frontier models emit dense marching-cubes-style triangle meshes with baked PBR textures, not artist topology: TRELLIS.2's example export decimates to 1M faces with 4096² textures and Hunyuan3D-2.1 needs explicit decimation; SF3D/SPAR3D are the only generators that ship quad/triangle remeshing with target counts and ~6–10 GB VRAM, and autoregressive retopology models (LATO.2, DeepMesh, BPT, MeshAnything V2) are a separate post-process with face caps (1.6k–30k). For Unity mobile AR, plan on decimating to tens of thousands of triangles, re-baking textures to ≤2048², and importing GLB via glTFast; USDZ must be produced outside Unity.

### Cited Findings
- TRELLIS.2 export example: `decimation_target=1,000,000`, `texture_size=4096`, `remesh=True`, `simplify=16,777,216` before export; GLB with WebP textures; arbitrary topology incl. open surfaces and non-manifold; meshes may have small holes (hole-filling step shipped) — [GitHub README](https://github.com/microsoft/TRELLIS.2); [search summary](https://www.3daistudio.com/blog/trellis-2-vs-hunyuan-3d-differences-explained)
- TRELLIS v1 GLB export supports a `simplify` ratio (e.g., 0.95 reduces polygons by 95%) (snippet) — [clore.ai guide (snippet)](https://docs.clore.ai/guides/3d-generation/trellis-3d)
- Hunyuan3D-2.1 ComfyUI wrapper exposes a Meshlib decimation node with `target_face_num` (default 0 = ignored) — [RunComfy node doc (snippet)](https://www.runcomfy.com/comfyui-nodes/ComfyUI-Hunyuan3d-2-1/hy3-d21-simple-meshlib-decimate)
- Hosted Hunyuan3D 3.1 Pro offers target face count 40,000–1,500,000 (snippet) — [search summary](https://layer.ai/models/tencent-hunyuan3d-v3-1-pro)
- Hunyuan3D-2.1 claims "production-ready PBR material" (albedo, metallic, roughness) and "holeless" watertight meshes vs TRELLIS.2's arbitrary topology (snippet) — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1); [trellis2.com (snippet)](https://trellis2.com/blog/trellis-2-vs-hunyuan3d-image-to-3d)
- SF3D: UV-unwrapped mesh, delighted albedo, material parameters "for game engine integration", remesh none/triangle/quad (quad converted to tris in GLB) — [GitHub](https://github.com/Stability-AI/stable-fast-3d)
- SPAR3D: PBR material params, remesh none/triangle/quad with configurable vertex/face targets, `--texture-resolution` — [GitHub](https://github.com/Stability-AI/stable-point-aware-3d)
- TripoSG: GLB output with face-count limit option; no textures — [GitHub](https://github.com/VAST-AI-Research/TripoSG); Direct3D-S2: OBJ output with remeshing option to reduce triangles; no textures — [GitHub](https://github.com/DreamTechAI/Direct3D-S2)
- Step1X-3D: textured GLB; texture module is SD-XL multi-view baking — [GitHub](https://github.com/stepfun-ai/Step1X-3D)
- Retopology models: MeshAnything V2 ≤1600 faces, needs sharp inputs; BPT >8k faces, ~2 min; DeepMesh up to ~30k faces; LATO.2 default 2000 vertices (200–5000), ~5 s, OBJ output, may have holes — [MeshAnythingV2](https://github.com/buaacyw/MeshAnythingV2); [BPT](https://github.com/Tencent-Hunyuan/bpt); [DeepMesh](https://github.com/zhaorw02/DeepMesh); [LATO.2](https://github.com/LoHhhha/LATO.2)
- Mesh-RFT states it "can generate product-ready meshes when conditioned on point cloud derived from dense meshes generated by Hunyuan3D" (snippet) — [project page](https://hitcslj.github.io/mesh-rft/)
- SAM 3D Objects primary documented export is Gaussian splat PLY; mesh export not documented in README — [GitHub README](https://github.com/facebookresearch/sam-3d-objects/blob/main/README.md)
- LGM / TRELLIS-3DGS output Gaussian splats (PLY); LGM has a Gaussians→mesh script — [LGM](https://github.com/3DTopia/LGM); [TRELLIS](https://github.com/microsoft/TRELLIS)
- Unity glTFast: imports .gltf/.glb at runtime and in Editor, glTF 2.0, URP/HDRP/Built-in, all Unity platforms incl. iOS (snippet) — [Unity glTFast docs (snippet)](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@5.0/manual/features.html)
- Unity has no built-in runtime USDZ/Reality export; community threads asking for it remain unresolved (snippet) — [Unity Discussions (snippet)](https://discussions.unity.com/t/export-to-reality-format/766147); RapidCompact CLI offers GLB→USDZ conversion (snippet) — [RapidCompact docs](https://www.rapidcompact.com/doc/cli/v03/Usdz/index.html)
- Blender decimate vs remesh workflow for AI meshes is a common 2026 post-process (snippet) — [StraySpark blog (snippet)](https://www.strayspark.studio/blog/reduce-polycount-ai-3d-model-blender-decimate-vs-remesh)

### Inferences
- A practical mobile-AR pipeline: TRELLIS.2 or Pixal3D (geometry + PBR) → export GLB at a lower `decimation_target` (e.g., 20k–100k faces) and `texture_size` 1024–2048 → optional LATO.2/DeepMesh retopology if you need animation-friendly topology → glTFast import in Unity; for iOS Quick Look, convert GLB→USDZ offline (RapidCompact, usd tooling) since Unity cannot export USDZ at runtime.
- TRELLIS.2's WebP-compressed GLB textures need to be verified against glTFast's supported texture formats (glTFast supports EXT_texture_webp in recent versions, but this was not verified here).
- Hunyuan3D-2.1 is the better choice when the texture pass matters most and the deployment region is outside EU/UK/KR; SF3D/SPAR3D are the best "ready-to-game-engine" lightweight options (UVs, material params, quad remesh) if the revenue cap is acceptable.

### Gaps
- No official statement of default/typical raw face counts for Hunyuan3D-2.1 or Pixal3D outputs was found.
- glTFast WebP/KTX support details could not be fetched (docs.unity3d.com blocked).
- No benchmark of retopology models on TRELLIS.2 outputs specifically was found.

---

## Key Question 4: Known failure modes

### Takeaway
Reported failure modes cluster into: non-watertight holes and metallic/transparency color shift (TRELLIS.2), Janus/back-side hallucination and geometric distortion on compositional scenes (TRELLIS, Hunyuan3D), facing-direction and occlusion sensitivity (multi-view methods like Unique3D), poor results from retopology models on smooth feed-forward meshes (MeshAnything), and incomplete/incorrect connectivity in vertex-flow retopology (LATO.2). Symmetry breaking is a measurable enough defect that a 2026 paper (SymTRELLIS) targets it.

### Cited Findings
- TRELLIS.2: "generated meshes occasionally contain holes, since it does not enforce a watertight-mesh assumption"; "textures ... exhibit a color-shifting artifact, which could result in the generation of metallic surfaces for transparent objects" (from 2026 follow-up papers, snippet) — [arXiv 2607.00382 (snippet)](https://arxiv.org/pdf/2607.00382); [SymTRELLIS 2606.04108 (snippet)](https://arxiv.org/pdf/2606.04108)
- TRELLIS and Hunyuan3D baselines show "Janus artifacts, geometric distortions, and inconsistent feature fusion, particularly under challenging compositional scenarios" (snippet) — [arXiv 2509.02357 (snippet)](https://arxiv.org/pdf/2509.02357)
- TRELLIS.2 strengths offsetting the above: "objects with hollow interiors, thin walls, or internal structures tend to come out cleaner" (snippet) — [3daistudio blog (snippet)](https://www.3daistudio.com/blog/trellis-2-vs-hunyuan-3d-differences-explained)
- SymTRELLIS benchmark shows TRELLIS.2, Hunyuan3D-2.1 and TripoSG all produce measurable symmetry errors on strictly symmetric objects (snippet) — [HF paper page (snippet)](https://huggingface.co/papers/2606.04108)
- Pixal3D's motivation: prior image-to-3D is "plausible" but low-fidelity to the input pixels ("Fidelity is a hidden bottleneck") — [author tweet (snippet)](https://x.com/WangZhao_0849/status/2054076811559182466)
- SPAR3D explicitly exists to fix SF3D's back side: "improving upon the backside of the mesh by conditioning on a point cloud" — [GitHub](https://github.com/Stability-AI/stable-point-aware-3d)
- Unique3D: "sensitive to the facing direction of input images"; "images with occlusions will cause worse reconstructions"; squashed results if longest edge not visible — [GitHub](https://github.com/AiuniAI/Unique3D)
- MeshAnything V2: cannot exceed 1600 faces; "requires sharp input geometry; struggles with low-quality shapes; performs poorly with feed-forward 3D generation outputs" — [GitHub](https://github.com/buaacyw/MeshAnythingV2)
- LATO.2: "generated meshes may still contain holes and incorrect connectivity" — [GitHub](https://github.com/LoHhhha/LATO.2)
- InstantMesh: texture-map export "may cost long time in the UV unwrapping step" — [GitHub](https://github.com/TencentARC/InstantMesh)
- TRELLIS text models "less creative and detailed due to data limitations" (image path recommended) — [GitHub](https://github.com/microsoft/TRELLIS)
- SAM 3D Objects is positioned for "occlusion and clutter" in real-world scenes — [GitHub](https://github.com/facebookresearch/sam-3d-objects)
- Multi-object scenes: PartCrafter offers a scene mode (2048 tokens/part); SAM 3D predicts layout/pose; most others assume a single segmented object — [PartCrafter](https://github.com/wgsxm/PartCrafter); [SAM 3D Objects](https://github.com/facebookresearch/sam-3d-objects)

### Inferences
- For AR product visualisation, pre-segment the object (SAM 3 / rembg) and feed a clean 3/4 view; use multi-view inputs (TRELLIS/TRELLIS.2 multi-image, Hunyuan3D-2mv) when back-side fidelity matters.
- Expect to run a hole-fill / manifold-repair pass (TRELLIS.2 ships one) before decimation and UV re-bake for mobile.

### Gaps
- No controlled 2026 study quantifying back-side hallucination or thin-structure failure across TRELLIS.2 / Pixal3D / Hunyuan3D-2.1 / SAM 3D was retrievable; a hands-on 9-image comparison blog exists but was blocked — [hawkymisc blog (unreachable)](https://hawkymisc.github.io/blog/trellis2-vs-hunyuan3d-gfx1151.html)

---

## Key Question 5: Maintenance, Docker/ComfyUI, API wrappers, HF Spaces

### Takeaway
TRELLIS.2 and Pixal3D have the strongest 2026 ecosystem (native ComfyUI core nodes, HF Spaces, community wrappers, training code); Hunyuan3D-2/2.1 has the most mature third-party tooling (ComfyUI-3D-Pack, ComfyUI-Hunyuan3DWrapper, Blender addon, local API server) but its open line is no longer advancing; SAM 3D Objects is actively maintained by Meta (June 2026 updates). Several research repos (Mesh-RFT, Sparc3D, Ultra3D, LATTICE) are paper-only.

### Cited Findings
- TRELLIS.2: HF demo `microsoft/TRELLIS.2`; training code; roadmap items without dates — [GitHub](https://github.com/microsoft/TRELLIS.2). ComfyUI: native core support plus `visualbruno/ComfyUI-Trellis2` custom nodes and an official ComfyUI tutorial page (snippet) — [ComfyUI docs (snippet)](https://docs.comfy.org/tutorials/3d/trellis2); [comfy.icu listing](https://comfy.icu/extension/visualbruno__ComfyUI-Trellis2)
- Pixal3D: ComfyUI native + `Saganaki22/Pixal3D-ComfyUI` with Windows/WSL guides; Pinokio one-click installer listing — [GitHub](https://github.com/TencentARC/Pixal3D); [Pinokio (snippet)](https://pinokio.co/apps/github-com-tencentarc-pixal3d)
- Hunyuan3D-2: ComfyUI-3D-Pack and ComfyUI-Hunyuan3DWrapper, Blender addon, local API server; roadmap "TensorRT version" — [GitHub](https://github.com/Tencent-Hunyuan/Hunyuan3D-2); example end-to-end ComfyUI repo using Hunyuan3D-2 with TRELLIS 2 as alternative geometry backend — [image-to-3d-comfyui](https://github.com/Alex92908/image-to-3d-comfyui); ComfyUI-3D-Pack general suite — [GitHub](https://github.com/MrForExample/ComfyUI-3D-Pack)
- Hunyuan3D 3.x: ComfyUI "partner nodes" since Feb 2026 call the hosted API only (snippet) — [triposr.org (snippet)](https://triposr.org/blog/hunyuan3d-versions)
- SAM 3D Objects: updates 2026-06-01 (encoder weights) and 2026-06-02 (SA-3DAO leaderboard); HF `facebook/sam-3d-objects`; `transformers` integration issue open — [GitHub README](https://github.com/facebookresearch/sam-3d-objects/blob/main/README.md); [transformers issue](https://github.com/huggingface/transformers/issues/42464)
- Step1X-3D: online demo, dataset, training code (May–Jun 2025); ComfyUI planned but not released — [GitHub](https://github.com/stepfun-ai/Step1X-3D)
- PartCrafter: HF demo (Aug 2025), Windows guide — [GitHub](https://github.com/wgsxm/PartCrafter); HoloPart: HF Space — [GitHub](https://github.com/VAST-AI-Research/HoloPart); LATO.2: HF Space + weights — [GitHub](https://github.com/LoHhhha/LATO.2)
- SF3D / SPAR3D: gated HF weights requiring login; experimental Windows and Mac MPS — [SF3D](https://github.com/Stability-AI/stable-fast-3d); [SPAR3D](https://github.com/Stability-AI/stable-point-aware-3d)
- Hi3DGen/Stable3DGen: marked WIP, 12 commits, 36 open issues, ~1.3k stars at fetch time — [GitHub](https://github.com/Stable-X/Hi3DGen)
- Mesh-RFT: README + LICENSE only, 1 commit; Sparc3D: no releases, unanswered issues; Ultra3D: project page only — [Mesh-RFT](https://github.com/hitcslj/Mesh-RFT); [Sparc3D releases](https://github.com/lizhihao6/Sparc3D/releases); [Ultra3D](https://github.com/buaacyw/Ultra3D)
- Hosted-API alternatives with these open models behind them exist (e.g., 3daistudio TRELLIS.2 API; Scenario hosts Hunyuan 3.1 Pro) (snippet) — [3daistudio API (snippet)](https://www.3daistudio.com/Platform/API/Trellis2); [Scenario (snippet)](https://www.scenario.com/models/hunyuan-3d-31-pro-multiview)

### Inferences
- If the team wants a self-hosted, commercially clean service for a Unity app, a ComfyUI (or plain Python) server running TRELLIS.2/Pixal3D on a 24 GB+ Linux GPU is the most future-proof option in Oct 2026; Hunyuan3D-2.1 can be added as a texture-quality alternative outside the excluded territories.

### Gaps
- No official Docker images were found for any of the models (community Dockerfiles likely exist but were not verified).
- Exact ComfyUI version that introduced native TRELLIS.2/Pixal3D nodes could not be retrieved (docs.comfy.org blocked).
