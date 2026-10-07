# Benchmarks, Leaderboards and Evaluation Evidence for Open Image-to-3D / Text-to-3D Models (2024 – Oct 2026)

Research date: 2026-10-07. Method note: the research sandbox blocked direct fetches of arxiv.org, huggingface.co, github.com (HTML), ai.meta.com and most blogs; findings below come from (a) web-search result extracts of those pages, (b) README files pulled from raw.githubusercontent.com, and (c) the GitHub API via the GitHub connector (repo stats, queried 2026-10-07 06:40 UTC). Where a number comes only from a search-engine extract of a page I could not open, it is marked "(via search extract)". Every number is tagged **[vendor]** (reported by the model's own authors/company), **[independent]** (third party with no model to sell) or **[commercial/SEO]** (sites that sell a product or are affiliate/SEO content).

---

## Key Question 1: What leaderboards exist and what do they currently say?

### Takeaway
The only large-scale, open, human-preference leaderboard for image-to-3D is Hugging Face's 3D Arena (123k votes, 19 models as of the June 2025 paper); its last paper-verified standings put CSM/Cube (closed) first, TRELLIS-3DGS second (ELO 1384) and TRELLIS-mesh / Hunyuan3D-2 close behind (1306 / 1298). I could not open the live Space to confirm October-2026 standings, and the 2026 numbers circulating on SEO sites appear to be the 2025 figures mis-attributed to TRELLIS.2. Academic benchmarks (3DGen-Bench, Hi3DEval, GPTEval3D, T3Bench, Eval3D) mostly cover 2023-2024 models; Hi3DEval is the most recent to score Hunyuan3D 2.5 / TRELLIS / SPAR3D on one scale.

### Cited Findings

**3D Arena (Hugging Face, dylanebert/3d-arena) — [independent, academic]**
- The 3D Arena paper (arXiv 2506.18787, June 2025) reports that since launching in June 2024 the platform had collected 123,243 votes from 8,096 users across 19 models, using pairwise blind comparisons where users can rotate/zoom the models — [3D Arena paper](https://arxiv.org/html/2506.18787v1) (via search extract)
- Paper leaderboard (June 2025 snapshot): CSM/Cube #1 with 1405 ELO (3,027 votes); TRELLIS-3DGS 1384 ELO (3,648 votes, 80.1% win rate); TRELLIS (mesh) 1306 ELO (67.0% win rate); Hunyuan3D-2 1298 ELO (65.5% win rate); InstantMesh 1278; SF3D 1190; LGM 1100 — [3D Arena paper](https://arxiv.org/html/2506.18787v1) (via search extract); same figures echoed in [Hi3DEval](https://arxiv.org/html/2508.05609v1)
- Format bias: Gaussian-splat outputs hold a 16.6 ELO advantage over meshes (1215.1 vs 1198.5; 51.9% vs 49.7% win rate) and textured models a 144.1 ELO advantage over untextured; the platform claims 99.75% user authenticity via statistical fraud detection — [3D Arena paper](https://arxiv.org/html/2506.18787v1)
- Dylan Ebert (Jan 2025, on adding Hunyuan3D-2): "It looks impressive and is definitely a leader, but may still be outperformed by TRELLIS, which is smaller and produces less dense topology" — [X post](https://x.com/dylan_ebert_/status/1882189255755387004)
- The arena added an "open-source only" filter and announced TRELLIS as the highest-ranked open-source model, surpassing InstantMesh — [HF post (filter)](https://huggingface.co/posts/dylanebert/731431287832353); [HF post (TRELLIS top OSS)](https://huggingface.co/posts/dylanebert/600222040158881)
- Live Space URL: https://huggingface.co/spaces/dylanebert/3d-arena (could not be fetched from this sandbox; standings as of Oct 2026 UNVERIFIED).
- CAUTION on 2026 SEO re-reports: triposplat.com (claims "3D Arena, May 2026": "TRELLIS-2 #2 Elo 1306, Hunyuan3D-2 #3 Elo 1298, Unique3D #4, Direct3D-S2 #5, InstantMesh #6, Pixal3D #7, TripoSR #8") — the ELO values 1306/1298 are identical to the June-2025 paper's TRELLIS (v1) and Hunyuan3D-2 values, so this very likely conflates TRELLIS with TRELLIS.2 and should not be trusted — [triposplat.com](https://triposplat.com/blog/best-3d-ai-generators-2026) **[commercial/SEO]**

**3DGen-Arena / 3DGen-Bench (3DTopia, arXiv 2503.21745, Mar 2025) — [independent, academic]**
- 3DGen-Arena is an anonymous pairwise battle platform; assets shown as three 360° videos (normal map, untextured geometry, textured) and voted on five dimensions: geometry plausibility, geometry detail, texture quality, geometry–texture coherence, prompt–asset alignment — [3DGen-Bench](https://arxiv.org/html/2503.21745v2)
- 13 image-to-3D models evaluated; 8,045 public anonymous votes total (1,694 on the image-to-3D track); the expert-annotation leaderboard is computed from 13.8k comparison annotations — [3DGen-Bench](https://arxiv.org/html/2503.21745v2); [3DGen-Bench v3](https://arxiv.org/html/2503.21745v3)
- Expert-annotation image-to-3D leaderboard (avg ELO): Wonder3D 1304.05, OpenLRM 1279.67, Stable Zero123 1200.69, … LGM 1060.35. Text-to-3D top-3: MVDream 1177.66, LucidDreamer 1112.21, Magic3D 1088.93 — [3DGen-Bench](https://arxiv.org/pdf/2503.21745) (via search extract)
- The model set is 2023–early-2024 vintage (Wonder3D, OpenLRM, LGM, CRM, InstantMesh era); Hunyuan3D-2, TRELLIS, TripoSG etc. are NOT in the paper leaderboard. Code/data: [GitHub 3DTopia/3DGen-Bench](https://github.com/3DTopia/3DGen-Bench) (README confirms arena at huggingface.co/spaces/ZhangYuhan/3DGen-Arena and dataset 3DGen/3DGen-Bench).
- A combined HF leaderboard, 3DTopia/3DGen-Leaderboard, "integrates results from three complementary benchmarks: Hi3DEval, 3DGenBench, and GPTEval3D" — [3DGen-Leaderboard source](https://huggingface.co/spaces/3DTopia/3DGen-Leaderboard/blob/main/serve/markdown.py) (via search extract; could not open)

**Hi3DEval (arXiv 2508.05609; NeurIPS 2025 Datasets & Benchmarks) — [independent, academic]**
- Scoring benchmark (not pairwise) with five dimensions: Geometry Plausibility (GP), Geometry Detail (GD), Texture Quality (TQ), Geometry-Texture Consistency (GTC), Prompt Alignment (PA) — [Hi3DEval](https://arxiv.org/html/2508.05609v1)
- Object-level results (image-to-3D): Hunyuan3D 2.5 overall 16.561 (GP 6.46, GD 2.86, TQ 2.79, GTC 0.981, PA 3.47); Hunyuan3D 2.0 16.1988; TRELLIS 15.1989 (GP 5.8626, GD 2.392, TQ 2.4693, GTC 0.9702, PA 3.5048); SPAR3D 15.0014 (5.7791, 2.3031, 2.4749, 0.9601, 3.4842); TripoSR 14.3404 (5.2216, 2.4225, 2.3758, 0.9562, 3.3643); InstantMesh 14.2775 (5.4242, 2.2252, 2.3063, 0.9587, 3.363); CRM 13.5572 (4.745, 2.2991, 2.3777, 0.9164, 3.219). "Image-to-3D methods generally dominate the upper rankings, with Hunyuan3D, Trellis, and SPAR3D forming the top three." — [Hi3DEval PDF](https://arxiv.org/pdf/2508.05609); [NeurIPS version](https://papers.neurips.cc/paper_files/paper/2025/file/42ffaddcc6edc9fb05ff9f9b49fca700-Paper-Datasets_and_Benchmarks_Track.pdf) (via search extract)
- Note: TRELLIS scores highest on Prompt Alignment (3.5048 vs 3.47 for Hunyuan3D 2.5) even though Hunyuan3D 2.5 wins overall. Hunyuan3D 2.5's weights were never released (see Q6), so the top Hi3DEval entry is not an open model.

**GPTEval3D (CVPR 2024) — [independent, academic]**
- Uses GPT-4V on multi-view renderings to do pairwise comparisons and builds an ELO leaderboard; covers text-to-3D SDS-era methods (MVDream, ProlificDreamer, Magic3D, DreamFusion…) — [3DGen-Bench discussion of GPTEval3D](https://arxiv.org/html/2503.21745v2); [Eval3D](https://arxiv.org/pdf/2504.18509)
- No GPTEval3D results for Hunyuan3D/TRELLIS-generation models were found.

**T3Bench (arXiv 2310.02977) — [independent, academic]**
- First text-to-3D benchmark with prompts at three complexity levels; two automatic metrics (multi-view quality with regional convolution; GPT-4 caption-based alignment); benchmarks 10 text-to-3D methods — [T3Bench](https://arxiv.org/pdf/2310.02977). A leaderboard aggregator lists SOTA-style numbers such as "Single Object Score 58.3, CLIP 31.4, Average T3Bench 50.88" without clear model attribution — [hyper.ai T3Bench leaderboard](https://hyper.ai/ja/sota/tasks/text-to-3d/benchmark/text-to-3d-on-t-3-bench) (unverified)

**Eval3D (CVPR 2025, arXiv 2504.18509) — [independent, academic]**
- Fine-grained, interpretable evaluation with pixel-wise measurement; 160 text prompts (80 single-object, 80 multi-object) plus optional SDXL images for image-to-3D; claims closer alignment with human judgment than prior metrics — [Eval3D](https://arxiv.org/pdf/2504.18509); [CVPR page](https://cvpr.thecvf.com/virtual/2025/poster/35203)

**SA-3DAO leaderboard (Meta, 2026) — [vendor-hosted]**
- SAM 3D Objects README: "06/02/2026 – 3D Artist Object Set and HF Leaderboard are out" (huggingface.co/spaces/facebook/sa3dao-leaderboard); "06/01/2026 – Encoder weights are out"; "11/19/2025 – Checkpoints launched" — [sam-3d-objects README](https://raw.githubusercontent.com/facebookresearch/sam-3d-objects/main/README.md). Leaderboard contents could not be fetched.

**Commercial / community arenas (2026) — [commercial/SEO], treat with caution**
- top3d.ai claims a chess-style ELO arena with "186,939 votes cast as of August 3, 2026" / "220,000+ blind votes"; its board lists SAM 3D at #21 with ELO 816 and 2.4% win rate — [top3d.ai leaderboard](https://www.top3d.ai/leaderboard) (via search extract; who runs it is unclear)
- sloyd.ai (a commercial 3D-generator vendor) publishes "ELO Arena Rankings" where TRELLIS.2 (4B) is #20 with ELO 874 (27 wins / 89 losses, 23.3%) and "Hunyuan 3D v3.1" #6 with ELO 1059 (78 W / 40 L, 66.1%) — [sloyd.ai blog](https://www.sloyd.ai/blog/ai-3d-model-generator-rankings). These rankings contradict every academic/vendor source on TRELLIS.2 and come from a competitor; methodology unverifiable.
- pixazo.ai aggregator: "Hunyuan3D-2.5 leads with a score of 1325, followed by Microsoft's TRELLIS (1290) and Meshy 5 (1280)"; and "10 of 14 models [tracked] can be self-hosted" — [pixazo leaderboard](https://www.pixazo.ai/leaderboard/ai-3d-model-generation) (methodology unknown)

### Inferences
- As of the last verifiable snapshot (June 2025), TRELLIS (v1) was the top open model on 3D Arena, marginally ahead of Hunyuan3D-2, with only closed CSM/Cube above it; both the arena's format bias (splats +16.6 ELO) and the small gap mean TRELLIS vs Hunyuan3D-2 is effectively a tie on human preference.
- No independent, large-vote leaderboard verifiably includes TRELLIS.2, SAM 3D Objects or Hunyuan3D 3.x as of Oct 2026; all 2026 "arena" numbers located are either SEO re-prints of 2025 data or vendor-run arenas.
- Academic benchmarks lag the model releases by ~12 months; the most up-to-date independent scale (Hi3DEval, Aug 2025) stops at Hunyuan3D 2.5 / TRELLIS v1.

### Gaps
- Current (Oct 2026) 3D Arena standings, vote counts and whether TRELLIS.2 / SAM 3D / Hunyuan3D-2.1 have been added — Space not reachable from the sandbox.
- 3DGen-Leaderboard (3DTopia) combined table and SA-3DAO leaderboard contents — not reachable.
- GPTEval3D / T3Bench numbers for any 2025–2026 model — none found; these benchmarks appear not to have been re-run on native-3D models.

---

## Key Question 2: Standard datasets/metrics and directly comparable numbers across models

### Takeaway
There is no single table that scores all of Hunyuan3D 2.x/TRELLIS/TRELLIS.2/TripoSG/Step1X-3D/Direct3D-S2/SPAR3D/Hi3DGen/SAM 3D on the same dataset and metric. The most useful apples-to-apples sets are: (1) Hunyuan3D-2.1's ULIP/Uni3D shape table (vendor) covering TripoSG, Step1X-3D, TRELLIS, Direct3D-S2; (2) Cue3D's independent GSO/Toys4K Chamfer/F-score table for Hunyuan3D-2 vs TRELLIS; (3) Hi3DEval (independent) for Hunyuan3D 2.0/2.5, TRELLIS, SPAR3D, TripoSR, InstantMesh, CRM; and (4) SAM 3D's vendor preference table vs TRELLIS and Hunyuan3D 2.0/2.1. Metrics differ by family (ULIP/Uni3D embedding similarity vs Chamfer/F-score vs CLIP vs human preference), and are never comparable across tables.

### Cited Findings

**Datasets and metric families in use**
- Toys4K (TRELLIS image-to-3D protocol): 1,250 held-out instances from 3,229 filtered Toys4K assets; TRELLIS paper reports CLIP and FD_dinov2 on it — [paperswithcode Toys4K/TRELLIS protocol](https://paperswithcode.co/benchmark/toys4k-trellis-image-to-3d-protocol); [TRELLIS paper](https://arxiv.org/pdf/2412.01506)
- GSO and OmniObject3D are the standard reconstruction sets for the LRM family (InstantMesh, TripoSR, CRM, LGM, SF3D, SPAR3D), with PSNR/SSIM/LPIPS on novel views plus Chamfer Distance and F-score — [MeshFormer](https://arxiv.org/pdf/2408.10198); [SPAR3D](https://arxiv.org/pdf/2501.04689); [SF3D](https://arxiv.org/pdf/2408.00653); [InstantMesh](https://arxiv.org/pdf/2404.07191)
- ULIP-T/ULIP-I and Uni3D-T/Uni3D-I (3D-text / 3D-image embedding similarity) are the shape metrics used by Hunyuan3D 2.x and Step1X-3D — [Hunyuan3D-2.1 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/README.md); [Step1X-3D](https://arxiv.org/pdf/2505.07747)
- TRELLIS.2 introduces "Mesh Distance" + F-score (arguing Chamfer is sensitive to point density) — [TRELLIS.2 paper](https://arxiv.org/html/2512.14692v1) (via search extract)
- SAM 3D introduces SA-3DAO (artist-made ground truth for real photos), ISO3D (isolated objects) and a human-preference set from robotics/egocentric domains — [SAM 3D paper](https://arxiv.org/pdf/2511.16624) (via search extract)
- Objaverse / Objaverse-XL are the training-side standard: Step1X-3D released 320K Objaverse + 480K Objaverse-XL curated UIDs (800K total) — [Step1X-3D README](https://raw.githubusercontent.com/stepfun-ai/Step1X-3D/main/README.md)

**Table A — Shape quality, ULIP/Uni3D (Hunyuan3D-2.1 tech report, June 2025) [vendor: Tencent]**
Source: [Hunyuan3D-2.1 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/README.md) (copied verbatim); same table in [arXiv 2506.15442](https://arxiv.org/html/2506.15442v1)

| Model | ULIP-T ↑ | ULIP-I ↑ | Uni3D-T ↑ | Uni3D-I ↑ |
|---|---|---|---|---|
| Michelangelo | 0.0752 | 0.1152 | 0.2133 | 0.2611 |
| Craftsman | 0.0745 | 0.1296 | 0.2375 | 0.2987 |
| TripoSG | 0.0767 | 0.1225 | 0.2506 | 0.3129 |
| Step1X-3D | 0.0735 | 0.1183 | 0.2554 | 0.3195 |
| TRELLIS | 0.0769 | 0.1267 | 0.2496 | 0.3116 |
| Direct3D-S2 | 0.0706 | 0.1134 | 0.2346 | 0.2930 |
| Hunyuan3D-Shape-2.1 | **0.0774** | **0.1395** | **0.2556** | **0.3213** |

- Differences between TRELLIS, TripoSG, Step1X-3D and Hunyuan3D-2.1 are in the 2nd–3rd decimal; only Direct3D-S2 and the 2024 baselines sit clearly lower. Evaluation set is not specified in the README.

**Table B — Texture quality (Hunyuan3D-2.1 tech report) [vendor]**
Source: [Hunyuan3D-2.1 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/README.md)

| Model | CLIP-FID ↓ | CMMD ↓ | CLIP-I ↑ | LPIPS ↓ |
|---|---|---|---|---|
| SyncMVD-IPA | 28.39 | 2.397 | 0.8823 | 0.1423 |
| TexGen | 28.24 | 2.448 | 0.8818 | 0.1331 |
| Hunyuan3D-2.0 (Paint) | 26.44 | 2.318 | 0.8893 | 0.1261 |
| Hunyuan3D-Paint-2.1 | **24.78** | **2.191** | **0.9207** | **0.1211** |

**Table C — Hunyuan3D 2.0 vs anonymised competitors (Jan 2025) [vendor]**
Source: [Hunyuan3D-2 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/README.md)

| Model | CMMD ↓ | FID_CLIP ↓ | FID ↓ | CLIP-score ↑ |
|---|---|---|---|---|
| Top open-source model 1 | 3.591 | 54.639 | 289.287 | 0.787 |
| Top closed-source model 1 | 3.600 | 55.866 | 305.922 | 0.779 |
| Top closed-source model 2 | 3.368 | 49.744 | 294.628 | 0.806 |
| Top closed-source model 3 | 3.218 | 51.574 | 295.691 | 0.799 |
| Hunyuan3D 2.0 | **3.193** | **49.165** | **282.429** | **0.809** |

- Competitors are anonymised; the paper identifies TRELLIS as the open-source comparator — [Hunyuan3D 2.0 paper](https://arxiv.org/html/2501.12202v5) (via search extract)

**Table D — Independent geometry metrics, Cue3D (NeurIPS 2025) [independent]**
Source: [Cue3D](https://arxiv.org/html/2511.22121v1) (via search extract); [OpenReview PDF](https://openreview.net/pdf/f52c307dad32e7c4a4d7e80ad2dec63d6d25a59a.pdf)

| Dataset | Model | CD ×1000 ↓ | F-score ↑ | Symmetry F-score ↑ |
|---|---|---|---|---|
| GSO | Hunyuan3D-2 | 41.82 | 0.087 | 0.894 |
| GSO | TRELLIS | 39.64 | 0.092 | 0.867 |
| Toys4K | Hunyuan3D-2 | 38.65 | 0.126 | 0.913 |
| Toys4K | TRELLIS | 37.78 | 0.137 | 0.904 |

- Cue3D's conclusion: native 3D generative methods (Hunyuan3D-2, TRELLIS) "clearly outperform other methods across both datasets"; TRELLIS slightly better on overall 3D metrics, Hunyuan3D-2 better on symmetry and visible-surface quality — [Cue3D](https://arxiv.org/html/2511.22121v1). Other methods (Hi3DGen, TripoSG, SF3D…) are in the paper but their numbers were not retrievable.

**Table E — TRELLIS.2 vs TRELLIS vs Hunyuan3D 2.1 (TRELLIS.2 paper, Dec 2025) [vendor: Microsoft]**
Source: [TRELLIS.2 paper](https://arxiv.org/html/2512.14692v1) (via search extract)
- Toys4K VAE reconstruction at a 9.6K-token budget: TRELLIS Mesh-Distance (×10⁶) 85.07 / F1 0.074 vs TRELLIS.2 at 1024³ "mesh distance 0.0042 / F1 0.971" (figure as reported in the extract; units look inconsistent — verify against the PDF). This is a VAE *reconstruction* test, not generation.
- Generation (Table 2): CLIP similarity TRELLIS.2 0.894 vs Hunyuan3D 2.1 0.869 vs TRELLIS 0.876.
- User study (~40 participants, 100 AI-generated image prompts, uncurated outputs, two question types: full render and normal-map shape): TRELLIS.2-4B preferred 66.5% vs Hunyuan3D 2.1 13.3% — [TRELLIS.2 paper](https://arxiv.org/html/2512.14692v1)

**Table F — SAM 3D Objects human preference (Nov 2025) [vendor: Meta]**
"Preference rate for SAM 3D over baseline" (%), via search extract of [SAM 3D paper](https://arxiv.org/pdf/2511.16624) and [CVPR 2026 supplement](https://openaccess.thecvf.com/content/CVPR2026/supplemental/Chen_SAM_3D_3Dfy_CVPR_2026_supplemental.pdf)

| Baseline | ISO3D | Preference set | SA-3DAO | LVIS |
|---|---|---|---|---|
| TRELLIS | 81.1 | 87.0 | 86.2 | 89.1 |
| Hunyuan3D-2.1 | 63.8 | 87.0 | 86.2 | 89.1 |
| Hunyuan3D-2.0 | 70.1 | 77.5 | 77.4 | 85.7 |

- CAUTION: the Hunyuan3D-2.1 and TRELLIS rows carry identical values in three columns in the extract — likely an extraction error; verify in the PDF.
- Meta blog: "at least a 5:1 win rate" for objects and 6:1 for scenes in pairwise tests across SA-3DAO, LVIS, MetaCLIP — [Meta AI blog](https://ai.meta.com/blog/sam-3d/) (via search extract); on LVIS humans preferred SAM 3D ~80%, Hunyuan3D 2.0 ~12%, others 8% — [deeplearning.ai The Batch](https://charonhub.deeplearning.ai/metas-sam-3-image-segmentation-suite-analyzes-and-creates-3d-bodies-and-other-objects/)
- Automatic metric on ISO3D (isolated, clean objects): SAM 3D Uni3D 0.3707 vs TRELLIS 0.3698 — i.e., essentially tied where the input is a clean product-style image — [SAM 3D review](https://liner.com/review/sam-3d-3dfy-anything-in-images) (via search extract)
- SAM 3D's human preference set is drawn from "robotic manipulation and egocentric vision" domains (cluttered, occluded real photos) — [SAM 3D paper](https://arxiv.org/pdf/2511.16624)

**Other vendor tables located but not fully retrieved**
- Sparc3D (May 2025) reports TRELLIS Chamfer Distance 1.32 (ABO), 4.29 (Objaverse), 0.70 (Wild) as its baseline, with its own numbers lower; metrics CD, Absolute Normal Consistency, F1 — [Sparc3D](https://arxiv.org/html/2505.14521v2) (via search extract) **[vendor]**
- Hi3DGen (Mar 2025) ran a user study vs Hunyuan3D-2.0, Dora, Clay, Tripo-2.5 and TRELLIS — percentages not retrieved — [Hi3DGen](https://arxiv.org/html/2503.22236v2) **[vendor]**
- Step1X-3D (May 2025) claims "state-of-the-art performance that exceeds existing open-source methods, while also achieving competitive quality with proprietary solutions" — [Step1X-3D README](https://raw.githubusercontent.com/stepfun-ai/Step1X-3D/main/README.md) **[vendor]**
- Hunyuan3D 2.5 (June 2025 tech report, weights unreleased) — [arXiv 2506.16504](https://arxiv.org/pdf/2506.16504) **[vendor]**
- BTC3D (Sept 2026) reports a Toys4K table including Hunyuan3D-2.1, TRELLIS.2 and TripoSG; one extract gives TRELLIS.2 on Toys4K PSNR 20.6741 / SSIM 0.8480 / LPIPS 0.5802 (other columns 0.3676, 0.3327, 0.4693, unlabeled) — [BTC3D](https://arxiv.org/html/2609.39709) (via search extract; column semantics unverified)
- For the 2024 LRM family (InstantMesh, CRM, LGM, TripoSR, SF3D, SPAR3D) GSO/OmniObject3D tables exist in [MeshFormer Table 1](https://arxiv.org/pdf/2408.10198), [SPAR3D Table 1](https://arxiv.org/pdf/2501.04689), [SF3D Table A1](https://arxiv.org/pdf/2408.00653), [Fancy123 Table 2](https://arxiv.org/pdf/2411.16185) — numbers not retrievable from the sandbox.

### Inferences
- On the two independent scales that include both (Hi3DEval, Cue3D), TRELLIS v1 and Hunyuan3D-2.0 are within noise of each other on geometry; Hunyuan3D wins on texture/overall in Hi3DEval, TRELLIS wins on prompt alignment and raw Chamfer/F-score in Cue3D.
- Every 2025–26 vendor paper (Hunyuan3D-2.1, TRELLIS.2, SAM 3D, Step1X-3D, Seed3D, Buffalo) reports beating the others on its own metric/dataset; the metric choice differs each time (ULIP/Uni3D vs Mesh Distance vs preference), so vendor tables cannot be chained into a single ranking.
- The largest *vendor* preference margins (TRELLIS.2 66.5% vs Hunyuan3D-2.1 13.3%; SAM 3D >80% vs TRELLIS on cluttered real photos) are both plausible and domain-specific: TRELLIS.2 was judged on AI-generated clean images, SAM 3D on in-the-wild photos.

### Gaps
- No independent Chamfer/F-score table that includes TRELLIS.2, TripoSG, Step1X-3D, Direct3D-S2, Hi3DGen, Sparc3D or SAM 3D together; Cue3D is the closest but only its Hunyuan3D-2/TRELLIS rows were retrievable.
- Hunyuan3D 2.1 vs TRELLIS.2 on the *same* independent metric: only the vendor TRELLIS.2 study exists.
- Could not retrieve PSNR/SSIM/LPIPS numbers for the LRM-family comparisons (InstantMesh/CRM/LGM) from the primary PDFs.

---

## Key Question 3: Independent comparisons and consensus on geometry, texture, topology, speed, VRAM

### Takeaway
The few genuinely independent hands-on comparisons agree that (a) TRELLIS.2 and Hunyuan3D-2.1 are the two open leaders, (b) TRELLIS.2 wins on speed, VRAM and sharp/complex geometry, (c) Hunyuan3D-2.1 is favoured for texture realism and smooth, hole-free meshes, and (d) neither produces game-ready topology without remeshing. Most "2026 comparison" articles are SEO or vendor content and should be discounted.

### Cited Findings

**Independent hands-on tests**
- hawkymisc (Japanese blog, AMD Ryzen AI MAX+ 395 / gfx1151, 64 GB shared memory, 9 test images): at an equal 40k-polygon budget TRELLIS.2 averaged 306.7 s with 8.65 GiB peak VRAM versus Hunyuan3D-2.1 637.0 s (≈2.1× slower) with 28.80 GiB peak (≈3.3× more); both succeeded on all tests — [hawkymisc blog](https://hawkymisc.github.io/blog/trellis2-vs-hunyuan3d-gfx1151.html) (via search extract) **[independent]**
- Polymedium benchmark (run 2026-08-29): every live image-to-3D endpoint on fal.ai (41 engines) tested against TRELLIS v1 as fixed control; 18 screened head-to-head, 23 excluded with reasons; five subjects chosen for failure modes; outputs rendered as an identical six-view Blender ring. Findings: Rodin v2.5 beat TRELLIS 2 on a game character (facial likeness, engraved armour, cloak embroidery); SAM-3 Objects matched TRELLIS 1 at identical price and was 3× faster, beating the control on a dog's face; Meshy v7 was "the one genuinely additive engine" (game-ready topology by default, part separation, rigging in one call) — [polymedium.app/benchmark](https://polymedium.app/benchmark) (via search extract; Polymedium appears to be an app vendor, so semi-independent) 
- SaladCloud (GPU cloud) measured Hunyuan3D 2.1 median generation time of 139 s over 900+ generations — [SaladCloud blog](https://blog.salad.com/hunyuan3d-2-1/) (via search extract) **[independent of model vendors; GPU type not captured]**
- Stun0perator/local-3dgen (Blender add-on author, tested on RTX 5070 Ti 16 GB, Windows): Hunyuan3D 2.1 shape ≈10 GB, Hunyuan3D-2mv ≈10 GB, TRELLIS image-large ≈8 GB; "each model takes about a minute" after first load; "TRELLIS.2 is not supported: it currently needs Linux and 24 GB or more of GPU memory"; "384 [octree res] with 50 steps is the sweet spot" — [local-3dgen README](https://raw.githubusercontent.com/Stun0perator/local-3dgen/main/README.md) **[independent]**
- Furkan Gözükara (dev.to, SECourses): "TRELLIS is still the lead open source AI model to generate high quality 3D assets from static images" (date/method not retrievable) — [dev.to](https://dev.to/furkangozukara/trellis-is-still-the-lead-open-source-ai-model-to-generate-high-quality-3d-assets-from-static-images-5hfa) **[independent creator; also sells tutorials]**
- YouTube: "Comparison between Trellis, TripoSG and Hunyuan 2.5 for generating 3D models" — [YouTube](https://www.youtube.com/watch?v=cFcXoVHYjJ8) (content not retrievable); a Digital Art Live video compared Hitem3D V2, Tripo, Meshy and TRELLIS across several images "with each tool showing strengths in different categories" — reported by [fuser.studio](https://fuser.studio/articles/best-ai-3d-model-generators)
- Single-photo figurine test (three generators, same front-facing photo): "Hunyuan3D V3 gave the smoothest, cleanest body and softened the glaze speckle. Trellis 2 kept the sculpted facets and texture closest to the photo, with a slightly warmer glaze. Tripo 2.5 with PBR textures produced the most pronounced speckle." — [cinevva guide](https://app.cinevva.com/guides/ai-3d-model-generators) (cinevva is a game-tools vendor; via search extract)
- Scenario.com (3D platform hosting multiple models, guide updated Dec 2025): Hunyuan 3D 3.0 Pro "1024 geometry resolution, 4K PBR textures, clean topology, ~90 s"; Hunyuan 3D 2.1 "ideal balance of speed and quality"; TRELLIS "performs particularly well when given multiple reference images and excels in geometry-driven tasks"; Rodin Gen-2 "10B parameter model … quad-based meshes, ~60 s" — [Scenario help](https://help.scenario.com/articles/1263568892-comparing-generative-3d-models) **[commercial reseller]**

**Consensus statements (mostly from commercial/SEO sources — weight accordingly)**
- Geometry: TRELLIS.2 "supports arbitrary topology up to 1536³ with full PBR including opacity"; Hunyuan3D 2.1 "produces holeless production meshes" — [triposr.org](https://triposr.org/blog/hunyuan3d-vs-trellis) **[SEO]**; "Hunyuan 3D at maximum settings can produce slightly more detailed and refined output for hero assets" — [3daistudio](https://www.3daistudio.com/blog/trellis-2-vs-hunyuan-3d-differences-explained) **[commercial]**
- Texture: "Hunyuan3D 2.1 was the first publicly available model that came with production-ready PBR texture synthesis and is still ahead in texture realism in most independent benchmark tests" — [swiftwand.com](https://swiftwand.com/open-source-3d-generation-trellis-hunyuan-2026/) **[SEO; "independent benchmark tests" not cited]**
- Topology: "According to community voting on 3D-arena, Hunyuan3D-2 ranked #1 for topology, with TRELLIS at #2" — [3daistudio](https://www.3daistudio.com/blog/trellis-2-vs-hunyuan-3d-differences-explained) **[commercial; 3D Arena has no topology sub-vote in the paper — unverified]**; Dylan Ebert: TRELLIS "produces less dense topology" than Hunyuan3D-2 — [X](https://x.com/dylan_ebert_/status/1882189255755387004) **[independent]**; Meshy 6 "is the only one with a quad topology option and A-Pose/T-Pose control" — [3daistudio](https://www.3daistudio.com/blog/hitem3d-vs-meshy-vs-tripo-comparison) **[commercial]**
- Speed (hosted): "TRELLIS 2 generates a model in 1 to 3 minutes, while Hunyuan 3D takes 2 to 6 minutes" and costs "10 credits vs 35–100 credits" on 3daistudio's own service — [3daistudio](https://www.3daistudio.com/blog/trellis-2-vs-hunyuan-3d-differences-explained) **[commercial]**
- Community sentiment: "it can't compete with Hunyuan 3.0, but gives a nice run for the money compared to other closed-source models … the #1 open source model at the moment" (user quote about TRELLIS.2) — [aiindigo review](https://aiindigo.com/blog/trellis-2-review-a-practical-look-at-microsoft-s-open-source-3d-engine) **[SEO]**
- TRELLIS.2 "is still much closer to a research project than a polished commercial product" — [fuser.studio](https://fuser.studio/articles/best-ai-3d-model-generators) **[commercial]**

### Inferences
- The only quantitative independent head-to-head of the two open leaders (hawkymisc) favours TRELLIS.2 on cost (2× faster, 1/3 VRAM) at equal polygon count; quality was called a draw on that test set.
- "Topology cleanliness" is uniformly reported as a weakness of *all* open models — every source that praises topology names a closed model (Meshy 6/7 quad, Rodin Gen-2 quad, Tripo auto-rig).
- Reddit/YouTube mega-thread consensus could not be verified directly; the SEO ecosystem (triposr.org, trellis2.app, 3daistudio, cinevva, pixazo, fuser) heavily recycles the same claims, so apparent "consensus" may be one source echoed.

### Gaps
- No Reddit r/StableDiffusion or r/3Dmodeling mega-thread could be fetched (reddit.com blocked); no Chinese-language (机器之心 / 量子位) comparison surfaced in searches.
- No independent test that includes SAM 3D Objects, Step1X-3D, Direct3D-S2, Hi3DGen or Sparc3D alongside TRELLIS.2/Hunyuan3D-2.1 on identical inputs with published images, other than the Polymedium fal.ai run (which could not be read in full).

---

## Key Question 4: Open vs closed commercial models — parity in 2026?

### Takeaway
Evidence points to open models being at or near parity on raw geometry/texture for clean single-object inputs (Hi3DEval ranked open Hunyuan3D 2.0/TRELLIS above several closed entries; Hunyuan3D 2.0's own anonymised table beat three closed models), but closed services (Hunyuan3D 3.x hosted, Meshy 6/7, Tripo 3.x, Rodin Gen-2, Seed3D 2.0) lead on topology, rigging, part separation and the newest-generation quality tier. Tencent's own 2.5/3.0/3.1 are closed, so the best "Hunyuan3D" is no longer open.

### Cited Findings
- Hunyuan3D 3.0: "pushed to 1536-cubed geometric resolution … 3.6B voxel ultra-HD modeling and three times higher precision"; "available only through the Hunyuan 3D platform, the Tencent Cloud API, and ComfyUI partner"; hosted engine "currently Hunyuan 3D 3.1, launched globally in November 2025"; "As of September 2026, GitHub issue trackers show no maintainer responses to requests about when 3.0 weights will be released" — [triposr.org Hunyuan versions](https://triposr.org/blog/hunyuan3d-versions) **[SEO; dates plausible but unverified]**
- Hunyuan3D 2.5 exists only as a tech report (June 23, 2025); the Hunyuan3D-2 README lists 2.1 (June 13, 2025) as the last open release — [Hunyuan3D-2 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/README.md) **[vendor]**
- Seed3D 2.0 (ByteDance, arXiv 2605.13862, 2026): user-study win rates vs Hunyuan3D-2.5, Hunyuan3D-3.1, Tripo 3.0, Rodin Gen2 v1.9 and HiTem v2.0 range from 55.2% (vs Hunyuan3D-3.1) to 98.3% (vs Seed3D 1.0); TRELLIS was not among the baselines — [Seed3D 2.0](https://arxiv.org/html/2605.13862v1) (via search extract) **[vendor]**; availability (weights vs API) not stated in retrieved material
- Hunyuan3D-Buffalo 1.0 (arXiv 2608.02711, Aug 2026): unified Qwen-VL + TRELLIS + Hunyuan3D model trained on 87M samples; human study vs TRELLIS, Universe3D, Omni123 gives 56.6% overall preference (chance 25%), 55.2% text alignment (TRELLIS 14.9%), 57.1% geometry (TRELLIS 12.4%); Edit3D-Bench CD 0.0091 — [Buffalo paper](https://arxiv.org/html/2608.02711v1); [emergentmind summary](https://www.emergentmind.com/papers/2608.02711) **[vendor]**; open-weight status not confirmed
- Meshy 7 (press release 2026-08-12): new geometry-alignment benchmark scored against held-out reference models; Meshy 7 81.0% overall proportion, 79.7% spatial distribution, 59.8% surface detail, leading four competitors (anonymised T1, H1, R1, H2 ≈ Tripo, Hunyuan3D, Rodin, +1) by 5.3 pts on surface detail; "Nobody tested clears 60% on surface detail from a single image"; multi-view input closes the field to ~2 pts — [PR Newswire](https://tools.prnewswire.com/en-us/live/20813/release/20260812EN24698); [Fabbaloo](https://www.fabbaloo.com/?p=239703) **[vendor]**
- Meshy 6 vs Tripo 3.1: preferred by 63.8% of 1,331 senior 3D artists from NetEase and Tencent in a blind test — [3daistudio](https://www.3daistudio.com/blog/hitem3d-vs-meshy-vs-tripo-comparison); [onyxranked](https://onyxranked.com/tripo-ai-vs-meshy-ai-2026/) **[vendor-originated (Meshy) claim, relayed by SEO sites]**
- Rodin Gen-2: "10 billion parameters … highest quality output in the market" with quad topology — [medium/ideas-with-wings](https://medium.com/ideas-with-wings/best-ai-3d-model-generators-in-2026-tripo-ai-vs-meshy-rodin-kaedim-and-more-7eea7b05eb11) **[SEO]**; Polymedium: Rodin v2.5 beat TRELLIS 2 on a game-character test — [polymedium](https://polymedium.app/benchmark)
- "Open source 3D model generation APIs in 2026 are genuinely competitive with closed-source in a way they simply weren't two years ago … Hunyuan3D having closed most of the quality gap"; but "Meshy still leading on ecosystem (auto-rigging, plug-ins, UI), API reliability and topology cleanliness on edge cases"; cost: "below high-volume thresholds, closed-source APIs win on total cost … above that, self-hosting Hunyuan3D becomes the right call" — [pixazo blog](https://www.pixazo.ai/blog/best-open-source-3d-model-generation-apis) **[commercial]**
- CSM/Cube (closed) was #1 on 3D Arena in June 2025 at 1405 ELO vs best open 1384 (TRELLIS-3DGS) — [3D Arena paper](https://arxiv.org/html/2506.18787v1) **[independent]**
- Hunyuan3D 2.0's own anonymised comparison showed it beating "Top Close-source Model 1/2/3" on CMMD/FID/CLIP (Table C above) — [Hunyuan3D-2 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/README.md) **[vendor]**

### Inferences
- In June 2025 the open/closed gap on 3D Arena was ~21 ELO (1405 vs 1384) — effectively parity for single-object preference. Since then closed vendors shipped at least two generations (Hunyuan3D 3.0/3.1, Meshy 6/7, Tripo 3.x, Rodin Gen-2/v2.5, Seed3D 2.0) while the open frontier moved once (TRELLIS.2, Dec 2025; SAM 3D, Nov 2025). Vendor studies in 2026 (Seed3D 2.0, Meshy 7) benchmark only against closed peers, which itself signals that they no longer regard open models as the bar.
- The consistent qualitative gap is pipeline features (quad/retopo, rigging, part segmentation, multi-view fusion), not raw mesh fidelity.

### Gaps
- No independent 2026 study scoring an open model (TRELLIS.2 / Hunyuan3D-2.1 / SAM 3D) against Hunyuan3D 3.1, Meshy 7, Tripo 3.x and Rodin Gen-2 on the same inputs with published numbers, other than Polymedium (not fully retrievable).
- Luma, Sloyd and Kaedim: no benchmark evidence located at all (Sloyd publishes its own arena; Kaedim only appears in listicles).

---

## Key Question 5: Hardware reality (inference time, VRAM, consumer GPUs)

### Takeaway
Officially, TRELLIS.2 needs 24 GB (Linux, 3 s / 17 s / 60 s at 512³/1024³/1536³ on H100), Hunyuan3D-2.1 needs 10 GB (shape) / 29 GB (shape+PBR texture), TRELLIS v1 16 GB (≈8 GB in practice), TripoSG 8 GB, SPAR3D 6–10.5 GB, Direct3D-S2 10 GB (512) / 24 GB (1024), Step1X-3D 27–29 GB. Independent measurements show TRELLIS.2 actually peaking at ~8.7 GiB at 40k polys on AMD, and community ComfyUI/GGUF forks claim 6–8 GB operation for TRELLIS.2; consumer 8–12 GB cards can run shape-only Hunyuan3D-2 mini/2.1 and TRELLIS v1 but not Hunyuan3D-2.1 PBR texturing.

### Cited Findings

**Official requirements [vendor READMEs]**
- TRELLIS.2: "An NVIDIA GPU with at least 24GB of memory is necessary. The code has been verified on NVIDIA A100 and H100 GPUs"; timing on H100: 512³ ~3 s (2 s shape + 1 s material), 1024³ ~17 s (10+7), 1536³ ~60 s (35+25); single 4B checkpoint — [TRELLIS.2 README](https://raw.githubusercontent.com/microsoft/TRELLIS.2/main/README.md)
- TRELLIS (v1): "at least 16GB of memory … verified on NVIDIA A100 and A6000"; Linux only officially — [TRELLIS README](https://raw.githubusercontent.com/microsoft/TRELLIS/main/README.md)
- Hunyuan3D-2.1: "10 GB VRAM for shape generation, 21GB for texture generation and 29GB for shape and texture generation in total"; `--low_vram_mode` flag; Shape 3.3B, Paint 2B — [Hunyuan3D-2.1 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/README.md)
- Hunyuan3D-2.0: "6 GB VRAM for shape generation and 16 GB for shape and texture generation"; mini (0.6B) / turbo / FlashVDM variants and `--low_vram_mode` — [Hunyuan3D-2 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/README.md)
- Hunyuan3D-Omni: "10 GB VRAM for generation" (3.3B, 2025-09-25) — [Hunyuan3D-Omni README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-Omni/main/README.md)
- TripoSG: "CUDA-enabled GPU with at least 8GB VRAM"; 1.5B rectified-flow model, 2048 latent tokens — [TripoSG README](https://raw.githubusercontent.com/VAST-AI-Research/TripoSG/main/README.md)
- Step1X-3D: Geometry-1300m + Texture: 27 GB, 152 s for 50 steps; Geometry-Label + Texture: 29 GB, 152 s (GPU not stated) — [Step1X-3D README](https://raw.githubusercontent.com/stepfun-ai/Step1X-3D/main/README.md)
- Direct3D-S2: "512 resolution requires at least 10GB of VRAM, and 1024 resolution needs around 24GB. We don't recommend generating models at 512 resolution … quality is noticeably lower" — [Direct3D-S2 README](https://raw.githubusercontent.com/DreamTechAI/Direct3D-S2/main/README.md)
- SPAR3D: default 10.5 GB; `SPAR3D_LOW_VRAM=1` ≈7 GB (slower); "default options takes about 6GB VRAM for a single image input"; experimental Windows and Apple MPS support (tested on M4 Max 36 GB) — [SPAR3D README](https://raw.githubusercontent.com/Stability-AI/stable-point-aware-3d/main/README.md)
- SAM 3D Objects README gives no VRAM/time figures — [sam-3d-objects README](https://raw.githubusercontent.com/facebookresearch/sam-3d-objects/main/README.md)

**Independent measurements**
- AMD Ryzen AI MAX+ 395 (gfx1151, ROCm): TRELLIS.2 @40k polys 306.7 s avg / 8.65 GiB peak; Hunyuan3D-2.1 @40k polys 637.0 s / 28.80 GiB peak — [hawkymisc](https://hawkymisc.github.io/blog/trellis2-vs-hunyuan3d-gfx1151.html) **[independent]**
- RTX 5070 Ti 16 GB, Windows: Hunyuan3D 2.1 shape and TRELLIS v1 each ~1 min per model after warm-up; TRELLIS v1 ≈8 GB, Hunyuan3D 2.1 shape ≈10 GB; TRELLIS runs "without xformers, flash-attn or kaolin" on RTX 50-series — [local-3dgen README](https://raw.githubusercontent.com/Stun0perator/local-3dgen/main/README.md) **[independent]**
- Hunyuan3D 2.1 median 139 s per generation across 900+ generations on SaladCloud (GPU class not captured) — [SaladCloud](https://blog.salad.com/hunyuan3d-2-1/) **[independent]**
- RTX 3060 (12 GB): Hunyuan3D-2.1 with texture, 60 steps, octree 512, "around 600 seconds (10 minutes)" — reported in a hobbyist write-up surfaced by search ([tspi.at, May 2026](https://www.tspi.at/2026/05/01/opensource3dassets.html)); attribution via search extract, unverified
- RTX 4090: "approximately 20 seconds for shape generation … 35 seconds for the full pipeline" for Hunyuan3D 2.1 — surfaced from a GPU-rental guide ([clore.ai docs](https://docs.clore.ai/guides/3d-generation/hunyuan3d)) via search extract; unverified **[commercial]**
- "Consumer hardware (RTX 4090) achieves approximately 2-3x longer generation times compared to … H100" (so ~34–51 s at 1024³ and ~2–3 min at 1536³ for TRELLIS.2) and "even with an RTX 4090 (24 GB), out-of-memory errors can occur at times … reducing the guidance strength settings typically fixes the issue" — [trellis2.app](https://trellis2.app/blog/how-to-use-trellis-2); [codesota](https://www.codesota.com/news/trellis-2-3d-generation) **[SEO; extrapolated, not measured]**

**Low-VRAM / quantised forks**
- TRELLIS.2 ComfyUI low-VRAM guide claims: minimum 8 GB with low-VRAM mode at 512³; "RTX 3060 with 8GB VRAM, 512³ with Low VRAM mode uses approximately 6.5 GB VRAM and takes about 60 seconds per model"; recommends GGUF Q4/Q5 weights, batch 1, texture 512, remesh off; ComfyUI `--lowvram`/`--novram` — [trellis2.app low-VRAM](https://trellis2.app/blog/trellis-2-low-vram) **[SEO; claims not independently verified; note the RTX 3060 8 GB variant exists but most are 12 GB]**
- One-click ComfyUI Windows installer for TRELLIS.2 exists (TheLocalLab, Patreon) — [Patreon](https://www.patreon.com/TheLocalLab/posts/trellis-2-sota-146806567)
- Hunyuan3D-2 ships `--low_vram_mode`, `--enable_flashvdm`, mini-turbo (0.6B) and a Windows portable bundle (YanWenKun/Hunyuan3D-2-WinPortable) — [Hunyuan3D-2 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/README.md)
- Apple Silicon port of Hunyuan3D-2.1 (MLX) exists — [dgrauet/Hunyuan3D-2.1-mlx](https://github.com/dgrauet/Hunyuan3D-2.1-mlx)
- TaylorSeer acceleration fork for TRELLIS.2-4B — [Archerkattri/fast-trellis2](https://github.com/Archerkattri/fast-trellis2)
- Fal.ai hosts SAM 3D Objects, Hunyuan3D v2 etc. as pay-per-call endpoints — [fal.ai SAM 3D](https://fal.ai/models/fal-ai/sam-3/3d-objects); Polymedium found SAM-3 Objects "3x faster" than TRELLIS 1 at identical price on fal — [polymedium](https://polymedium.app/benchmark)

### Inferences
- The 24 GB TRELLIS.2 requirement is a worst-case (1536³) figure; at 512³–1024³ and ≤40k polygons an independent run peaked under 9 GiB, consistent with community 8 GB ComfyUI claims. A 12 GB consumer card is therefore realistic for TRELLIS.2 at reduced resolution, and 16 GB comfortable.
- Hunyuan3D-2.1 full PBR texturing (21–29 GB) is the single biggest VRAM outlier among open models; a 24 GB RTX 3090/4090 is the practical floor for the full pipeline, and 32 GB RTX 5090 avoids OOM.
- All published consumer-GPU timings (1–10 min) are 20–100× slower than the H100 figures vendors headline.

### Gaps
- No measured RTX 3090 / 4090 / 5090 / A100 timings for TRELLIS.2 from an independent source; RTX 4090 numbers above are extrapolations or unverified guides.
- No VRAM/time data for SAM 3D Objects, Hi3DGen or Sparc3D on consumer GPUs.

---

## Key Question 6: Reproducibility and maintenance signals (stars, issues, last commit, licensing, weight availability)

### Takeaway
As of 2026-10-07, TRELLIS.2 (pushed July 2026) and TRELLIS (June 2026) are the only major open repos with 2026 commits; Hunyuan3D-2 / 2.1 / Omni last pushed Oct 2025 and Tencent has moved 2.5/3.0/3.1 behind an API; TripoSG, Step1X-3D, Direct3D-S2, SPAR3D, InstantMesh, LGM, CRM are effectively frozen (last push 2024–Sept 2025). Licensing splits cleanly: MIT (TRELLIS/TRELLIS.2/TripoSG/Direct3D-S2/LGM/CRM/Hi3DGen), Apache-2.0 (Step1X-3D, InstantMesh), and restricted community licences (Tencent: excludes EU/UK/South Korea; Stability: $1M revenue cap; Meta SAM License: no military use).

### Cited Findings

**GitHub API snapshot, 2026-10-07 06:40 UTC** (source: GitHub repository search API via connector; stars/forks/open issues (issues+PRs)/last push/licence)

| Repo | Stars | Forks | Open issues | Last push | Created | Licence (SPDX) |
|---|---|---|---|---|---|---|
| Tencent-Hunyuan/Hunyuan3D-2 | 15,030 | 1,562 | 251 | 2025-10-28 | 2025-01-21 | NOASSERTION (Tencent Community) |
| microsoft/TRELLIS | 13,771 | 1,353 | 262 | 2026-06-26 | 2024-12-02 | MIT |
| microsoft/TRELLIS.2 | 11,487 | 1,387 | 156 | 2026-07-10 | 2025-11-26 | MIT |
| facebookresearch/sam-3d-objects | 7,489 | 887 | 114 | 2026-06-02 | 2025-09-29 | NOASSERTION (SAM License) |
| VAST-AI-Research/TripoSR | 7,020 | 907 | 106 | 2026-06-04 | 2024-02-07 | MIT |
| TencentARC/InstantMesh | 4,553 | 505 | 121 | 2025-01-03 | 2024-04-10 | Apache-2.0 |
| Tencent-Hunyuan/Hunyuan3D-2.1 | 4,134 | 618 | 153 | 2025-10-17 | 2025-06-13 | NOASSERTION (Tencent Community) |
| 3DTopia/LGM | 2,118 | 140 | 64 | 2024-08-20 | 2024-02-06 | MIT |
| VAST-AI-Research/TripoSG | 1,820 | 199 | 47 | 2025-04-18 | 2025-03-24 | MIT |
| DreamTechAI/Direct3D-S2 | 1,285 | 111 | 57 | 2025-09-26 | 2025-05-23 | MIT |
| Stability-AI/stable-point-aware-3d (SPAR3D) | 1,077 | 106 | 39 | 2025-05-05 | 2024-12-03 | NOASSERTION (Stability Community) |
| stepfun-ai/Step1X-3D | 896 | 65 | 40 | 2025-09-08 | 2025-05-13 | Apache-2.0 |
| thu-ml/CRM | 693 | 55 | 26 | 2024-11-28 | 2024-03-10 | MIT |
| Tencent-Hunyuan/Hunyuan3D-Omni | 638 | 62 | 8 | 2025-10-17 | 2025-09-25 | NOASSERTION |

(Repo URLs: https://github.com/<full_name>. Stable-X/Hi3DGen returned no result in the API search — repo may have been renamed/moved; its README still resolves at [raw README](https://raw.githubusercontent.com/Stable-X/Hi3DGen/main/README.md).)

**Licence details**
- TRELLIS.2: "This model and code are released under the MIT License", with nvdiffrast / nvdiffrec dependencies under their own NVIDIA licences — [TRELLIS.2 README](https://raw.githubusercontent.com/microsoft/TRELLIS.2/main/README.md); TRELLIS v1 likewise MIT with diffoctreerast and modified FlexiCubes submodules under separate licences — [TRELLIS README](https://raw.githubusercontent.com/microsoft/TRELLIS/main/README.md)
- Hi3DGen/Stable3DGen: MIT, and "we have specifically removed [TRELLIS's] dependencies on certain NVIDIA libraries (kaolin, nvdiffrast, flexicube) to ensure this adapted version can be used commercially" — [Hi3DGen README](https://raw.githubusercontent.com/Stable-X/Hi3DGen/main/README.md)
- Hunyuan3D 2.0/2.1: Tencent Hunyuan 3D Community License — commercial use allowed until 1M MAU, then a licence must be requested; territory "excludes the European Union, the United Kingdom and South Korea" and forbids use of model, derivatives or outputs outside the territory — [Tencent Cloud techpedia](https://www.tencentcloud.com/techpedia/148273?lang=en); [local-3dgen README](https://raw.githubusercontent.com/Stun0perator/local-3dgen/main/README.md); community reaction: [HN thread 1](https://news.ycombinator.com/item?id=42786403), [HN thread 2](https://news.ycombinator.com/item?id=43420870); [GitHub issue #254 "Does Hunyuan3D-2 allow for commercial use?"](https://github.com/Tencent-Hunyuan/Hunyuan3D-2/issues/254)
- SAM 3D Objects: "licensed under SAM License" — [README](https://raw.githubusercontent.com/facebookresearch/sam-3d-objects/main/README.md); described as worldwide royalty-free, commercial use not prohibited, with no-military/ITAR, no-reverse-engineering and patent-retaliation clauses — [Roboflow model page](https://playground.roboflow.com/models/meta/sam-3d-objects)
- SPAR3D: Stability AI Community License, free for commercial use under $1M annual revenue — [Stability AI announcement](https://stability.ai/news-updates/stable-point-aware-3d)
- TripoSG: MIT for code and weights (an issue asks about commercial rights when FlashVDM is used) — [TripoSG issue #69](https://github.com/VAST-AI-Research/TripoSG/issues/69); Step1X-3D: Apache-2.0 — [Step1X-3D README](https://raw.githubusercontent.com/stepfun-ai/Step1X-3D/main/README.md); Direct3D-S2: MIT — [Direct3D-S2 README](https://raw.githubusercontent.com/DreamTechAI/Direct3D-S2/main/README.md)

**Weight availability / release history**
- Hunyuan3D-2.1 (June 13, 2025) "for the first time releases full model weights and training code"; Hunyuan3D 2.5 only a tech report (June 23, 2025); no newer open 3D weights listed through the README's last entry (July 26, 2025, HunyuanWorld-1.0) — [Hunyuan3D-2 README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2/main/README.md); Hunyuan3D-Omni weights released 2025-09-25 — [Omni README](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-Omni/main/README.md)
- Hunyuan3D 3.0 / 3.1: hosted/API only; no weights to download (see Q4) — [triposr.org](https://triposr.org/blog/hunyuan3d-versions) **[SEO; corroborated by absence of a 3.x repo in GitHub API search]**
- TRELLIS.2: paper, inference code, 4B checkpoint, texture inference and training code all released (checklist in README) — [TRELLIS.2 README](https://raw.githubusercontent.com/microsoft/TRELLIS.2/main/README.md)
- SAM 3D Objects: checkpoints 11/19/2025, encoder weights 06/01/2026, SA-3DAO dataset + leaderboard 06/02/2026 — [README](https://raw.githubusercontent.com/facebookresearch/sam-3d-objects/main/README.md)
- Direct3D-S2: v1.0/v1.1 released May 30, 2025; "v1.2 … enhanced character generation" announced June 3, 2025 (no later entry in README) — [Direct3D-S2 README](https://raw.githubusercontent.com/DreamTechAI/Direct3D-S2/main/README.md)
- TripoSG: 1.5B model + VAE released 2025-03; scribble variant 2025-04; no later updates — [TripoSG README](https://raw.githubusercontent.com/VAST-AI-Research/TripoSG/main/README.md)
- Step1X-3D: weights, training code, 800K curated UIDs released May 13, 2025; last README news June 26, 2025 — [Step1X-3D README](https://raw.githubusercontent.com/stepfun-ai/Step1X-3D/main/README.md)
- No evidence was found of any of these models' weights being withdrawn from Hugging Face or re-licensed more restrictively after release.

### Inferences
- Maintenance is concentrated at Microsoft (TRELLIS/TRELLIS.2) and Meta (SAM 3D); Tencent's open line has been static for ~12 months while its closed line advanced, and the smaller labs (VAST TripoSG, StepFun, DreamTech, Stability) show no activity since mid/late 2025.
- For EU/UK/South-Korea deployments, Hunyuan3D 2.x is contractually unavailable, leaving TRELLIS.2 (MIT), SAM 3D Objects (SAM License) and TripoSG/Step1X-3D/Direct3D-S2 as the compliant options.
- High open-issue counts relative to stars (TRELLIS 262, Hunyuan3D-2 251) are consistent with installation friction (CUDA extensions, Linux-only) reported in community guides.

### Gaps
- Could not read issue trackers to confirm the "no maintainer response on 3.0 weights" claim or to count install-related issues.
- Hi3DGen and Sparc3D repo stats not retrievable; Sparc3D's official repo/weights status unknown.
- Whether Seed3D 2.0 or Hunyuan3D-Buffalo 1.0 ship open weights — not determined.
