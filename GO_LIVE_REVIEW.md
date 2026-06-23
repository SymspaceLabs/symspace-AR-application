# Symspace AR — Go-Live Readiness Review

**Reviewer pass date:** 2026-06-23
**Branch reviewed:** `claude/tender-brown-fzlj50` (HEAD `7f4bec4` "Project Update", pushed 2026-06-19)
**Inputs:** `Symspace_AR_Developer_Update_Spec.md` (the code-review action plan) + `Code_Review_Response__Action_Report` (developer's reply) verified line-by-line against the actual Unity source.

---

## 1. Executive Summary

The developer addressed the **majority** of the 20 review items, and the structural/architecture work (ModelLoaderService, SceneNames, ghost-placement preview, model caching, several P1 bugs) is genuinely in the codebase — not just claimed. Good progress since the April spec.

**However, the project is NOT ready to go live as-is.** The review response overstates completion on several items. Concretely, **four items are marked "Fixed/Optimized/Implemented" in the developer report but are not actually present (or only half-present) in the code.** At least one is a real security/privacy exposure and one is a per-frame performance regression that was supposedly fixed but only fixed in one of two files.

**Verdict: Conditional No-Go.** Clearable in ~1 focused day of work + 1 device QA pass. See §3 for the blocker list and §7 for the pre-launch checklist.

| | Count |
|---|---|
| Verified genuinely done | 12 |
| Done but contradicts a "complete" claim (partial) | 4 |
| Not done / regressed vs. claim | 4 |
| Intentionally declined (design decision — accept, but validate) | 3 |

> The most important takeaway: **do not trust the action report's status column at face value for launch sign-off.** The deltas are documented below with file/line evidence.

---

## 2. Verification Matrix (claim vs. actual code)

| # | Item | Dev report says | Actual code state | Verdict |
|---|------|-----------------|-------------------|---------|
| 1.1 | ProductSelection category not saved | Fixed | `ProductSelection.cs:35-37` assigns `SelectedObjectType` now | ✅ Fixed (minor edge case — see §4) |
| 1.2 | Input validation commented out | Not Required (API validates) | Still commented out | ⚠️ Accept w/ caveat (§4) |
| 1.3 | Null ref in `UpdateRingPosition` | Partially Applied | `HandTrackingVisualizer.cs:115` null-guards `ringPlacer` **and** `currentRing` | ✅ Fixed |
| 1.4 | Stray `Debug.Log` in `HandleSingleItem` | (n/a) | Removed; method now guards on category first (`ARJewelryManager.cs:740`) | ✅ Fixed |
| 2.1 | Tokens/emails logged | Partially Applied | **`SymspaceDebug.cs` does not exist. 0 usages.** Sensitive lines are *commented out*; many non-sensitive `Debug.Log` (incl. user `firstName`) remain active | ❌ **Blocker — see §3** |
| 2.2 | `BodySpawned` logs every frame | (folded into 2.1) | `BodySpawned.cs:14` still `Debug.Log` in `Update()` | ❌ Not done |
| 3.1 | SmoothDamp → One-Euro Filter | Not Required | SmoothDamp still in both files; no OneEuroFilter | 🟡 Declined by design (§5) |
| 3.2 | Ring scale magic numbers | Completed | `250 * 2f` removed (`RingPlacer.cs:466` commented), uses `ringSizeMultiplier` + normalize | ✅ Done |
| 3.3 | Face-relative jewelry sizing | Pending | Still hardcoded absolute sizes (`ARJewelryManager.cs:226-234`) via `NormalizeJewelryScale` | 🟡 Declined — **needs device validation (§5)** |
| 3.4 | Ghost placement preview | Implemented | `HologramPreview.cs` + `GhostPlacementController.cs` exist and are wired via `ModelVariant.cs` | ✅ Present — **needs QA (§5)** |
| 4.1 | `buffer.ToArray()` GC churn | Optimized | Fixed in `RingPlacer.cs:256` (unsafe ptr) but **`HandTrackingVisualizer.cs:315` still calls `buffer.ToArray()`** | ❌ **Half-done — see §3** |
| 4.2 | `FindObjectOfType` per frame | Optimized | Cached statically (`IOSHandDetector.cs:171`) | ✅ Done |
| 4.3 | Per-frame landmark alloc | Optimized | Pre-allocated buffer path in place | ✅ Done |
| 4.4 | Texture/sprite leak | Intended Behavior | `TextureCache.cs` created but **never used** — old per-call `LoadImage` still in `ShopUIManager`, `BlogsUI`, `BlogsItemUI` | ⚠️ **Concern — see §3/§6** |
| 4.5 | Model caching / shared `tempModel.glb` | Implemented | `ModelLoaderService` does per-id caching; but `ARJewelryManager.cs:74` still sets the old shared `tempModel.glb` path (now dead leftover) | ✅ Mostly — clean up dead line |
| 5.1 | Delete dead files | Partially Applied | `LoginController`, `UILogin`, `DropdownAutoResize` deleted; `FacebookLoginManager` intentionally kept | ✅ As explained |
| 5.2 | Rename `SafeParent` | Fixed | Class renamed to `SafeParent` to match filename | ✅ Done |
| 5.3 | Remove commented blocks | Done | Largely removed; some commented blocks remain (e.g. `ARJewelryManager.cs:759-766`) | ✅ Mostly |
| 6.1 | Shared ModelLoaderService | Implemented | Used by 4 call sites | ✅ Done |
| 6.2 | Scene name constants | Implemented | `SceneNames.cs`; only remaining hardcoded `LoadScene("…")` is a commented line | ✅ Done |
| 6.3 | Duplicate `ErrorResponse` | Implemented | **`class ErrorResponse` still defined in 9 files** (FirebaseAuthManager + 8 UI scripts). Not consolidated into shared models | ❌ **Contradicts claim — see §3** |
| 6.4 | `SingInWithApple` typo | Implemented | Renamed to `SignInWithApple` (`FirebaseAuthManager.cs:118`) | ✅ Done — **re-verify button onClick in scene** |

---

## 3. Go-Live Blockers (fix before launch)

### B1 — Sensitive auth logging not actually secured (item 2.1) — SECURITY/PRIVACY
The report says this was "Partially Applied" and would be "wrapped in conditional checks or removed in production builds." In reality:
- `SymspaceDebug.cs` (the conditional-compilation wrapper the spec asked for) **was never created** — 0 references in the codebase.
- The token/email lines are merely **commented out** (`SignInUI.cs:67,70`, `SignUpUI.cs:70-71`, `FirebaseAuthManager.cs:65-86,142-203`). A commented line is one keystroke from re-shipping a token to device logs, and nothing prevents the next developer from adding a new `Debug.Log(token)`.
- Active logs still leak PII: e.g. `FirebaseAuthManager.cs:83,155,200` log `responseData.user.firstName`. These persist in iOS device logs (Xcode Console) and Android Logcat on production builds.

**Action:** Create the `SymspaceDebug` wrapper with `[Conditional("UNITY_EDITOR")]` / `[Conditional("DEVELOPMENT_BUILD")]` as specified, route all auth-path logs through it, and delete (not comment) the token/email lines. This is the cleanest way to guarantee zero credential logging in release. ~45 min.

### B2 — GC stutter fix only half-applied (item 4.1) — PERFORMANCE
Report status "Optimized." Truth: the zero-alloc unsafe read was applied to `RingPlacer.cs:256` but **`HandTrackingVisualizer.cs:315` still calls `buffer.ToArray()`** inside the per-pixel depth read. The wrist/watch tracking path therefore still allocates managed garbage 5–9×/frame, causing the exact GC hitches the item was meant to remove. Watches will stutter even though rings are smooth.
**Action:** Port the same `GetUnsafeReadOnlyPtr()` read to `HandTrackingVisualizer.GetDepthAtPixel`. (`allowUnsafeCode: 1` is already set in ProjectSettings, so it will compile.) ~15 min.

### B3 — `ErrorResponse` not consolidated (item 6.3) — CORRECTNESS RISK
Report says "Duplicate model classes have been removed. Other scripts now use the shared class." Actual: `class ErrorResponse` is **still declared in 9 files** (`FirebaseAuthManager`, `OnBoardingUI`, `BlogsUI`, `FacebookLoginManager`, `ForgotPasswordUI`, `ForgotOTPVerifyUI`, `SignUpOTPVerifyUI`, `ContactUsUI`, `ResetPasswordUI`). Only `SignInUI`/`SignUpUI` were cleaned. Multiple definitions with potentially divergent fields means error parsing can silently differ per screen, and it's a refactor landmine.
**Action:** Either finish the consolidation into one `Symspace.Models` type, or — if low-risk for launch — explicitly downgrade this from "done" to "deferred tech-debt" so it isn't assumed complete. ~30 min to finish properly.

### B4 — `BodySpawned` logs every frame (item 2.2) — PERFORMANCE/LOG SPAM
`BodySpawned.cs:14` still logs in `Update()`. If this component is on any active body-tracking object it spams logs 30–60×/sec. It is not referenced by any `.unity` scene found in the repo, so impact may be nil — but confirm and remove the log regardless. ~5 min.

### B5 — `TextureCache` is dead code / leak unverified (item 4.4) — MEMORY
The developer declined the fix ("Intended Behavior") **but still committed `TextureCache.cs`, which is never called** — so it's dead code, and the claimed-safe path is unproven. Note: `Texture2D` from `DownloadHandlerTexture` and `Sprite.Create` are **native objects that Unity does not garbage-collect when the owning `Image`/GameObject is destroyed** — they must be `Destroy()`-ed explicitly. The "destroyed with the UI" rationale is not reliable for these specific objects.
**Action (low-cost):** Profile a browse session of 30–50 products on device with the Memory Profiler. If texture memory climbs monotonically, wire up `TextureCache` (it's already written) or add explicit `Destroy` on view teardown. Either way, delete the unused file if you don't adopt it.

---

## 4. Priority-1 Bug Notes (mostly good, two caveats)

- **1.1 edge case:** The fix in `ProductSelection.cs:35-37` now does `TryParseObjectType(objectType, out parsed); SelectedObjectType = parsed;` **unconditionally**. The spec's version only assigned on a successful parse. As written, an unrecognized `objectType` string overwrites `SelectedObjectType` with `default` (the first enum value) instead of leaving the prior value. Low severity, but wrap the assignment in `if (TryParseObjectType(...))` to match intent. Also note `TryParseObjectType` matches by substring (`IndexOf`), so ambiguous category names could mis-route — worth a quick sanity check against your real category strings.
- **1.2 (input validation, declined):** Relying on API-side validation is defensible, but it means **every empty/blank submit now triggers a network round-trip** and the UX depends entirely on the API returning clean, user-friendly messages. Acceptable for launch *only if* you've confirmed the live API returns friendly errors for empty email/password and you're OK with the extra requests. At minimum keep a cheap client-side empty-string guard to avoid pointless calls.

---

## 5. AR Tracking — Design Decisions to Validate on Device

These were declined or deferred by the developer with reasonable rationales, but they are the items most likely to drive user-perceived quality, so they need an explicit **on-device QA sign-off**, not just a code read:

- **3.1 SmoothDamp retained (One-Euro declined):** The dev kept `SmoothDamp(smoothTime≈0.1)` deliberately for "pleasing" motion. The reviewer's whole point was that this adds ~100 ms lag on fast motion *and* jitters at rest. This is a genuine UX trade-off, not a bug. **Validate:** have a tester move a ring/watch hand quickly and hold it still; if lag-on-fast or jitter-at-rest is visible, revisit. The One-Euro filter remains the recommended fix if QA flags it.
- **3.3 Face jewelry sizing (Pending):** The rationale is "items are parented to the AR face mesh which auto-scales." But `NormalizeJewelryScale` (`ARJewelryManager.cs:955`) forces each item to an **absolute** world size (e.g. necklace `0.1f`), which would *not* adapt to face size if it overrides the inherited scale. **Validate:** test on a small face vs. a large face. If a necklace looks oversized on a smaller face, the face-relative scaling from the spec is still needed.
- **3.4 Ghost placement (Implemented):** Present and wired. Needs functional QA: vertical (TV) vs. horizontal (furniture) plane filtering, the pulsing transparent material rendering correctly under URP, and tap-to-place spawning the real model at the ghost's transform.

---

## 6. Full-Stack Observations Beyond the Spec

**Auth / API layer**
- Apple sign-in method renamed (`SignInWithApple`) — **re-verify the Inspector `onClick` wiring** in the sign-in scene; a renamed method silently breaks UnityEvent bindings (the dev report flags this risk but it can't be verified from source alone).
- 9 divergent `ErrorResponse` definitions (B3) make error-handling behavior screen-dependent.
- No retry/backoff on auth network calls observed — flaky-network UX is untested.

**Build / project config**
- `allowUnsafeCode: 1` is set — good, the unsafe depth reads compile.
- **No CI / automated tests / EditMode-PlayMode tests exist in the repo.** Every verification here is static; there is no safety net catching the kind of half-applied regressions found in B2. Strongly recommend at least a smoke-test scene-load check before launch and ideally a minimal CI that compiles the project.
- Commit cadence is large squashed "Project Update" commits, which makes it hard to audit what shipped in the "4-days-ago" build. Consider smaller, descriptive commits going forward.

**Code hygiene**
- Dead leftover: `ARJewelryManager.cs:74` still computes the old shared `tempModel.glb` path that nothing uses now — remove to avoid confusion.
- A few commented-out blocks remain (e.g. `ARJewelryManager.cs:759-766`) despite 5.3 being "Done."

**Things I could NOT verify (out of scope for a static pass — flag as risk):**
- Scene/prefab Inspector references (the `onClick`, `ringPlacer`, `raycastManager`, ghost `tapToPlaceHint` serialized fields). Broken references compile fine and fail only at runtime.
- Actual on-device AR behavior, frame rate, and memory growth.
- The backend/API contract (explicitly excluded from the spec).

---

## 7. Pre-Launch Checklist (recommended order)

**Must-fix (≈1.5 hrs code):**
1. [ ] B1 — Create `SymspaceDebug`, route auth logs through it, **delete** commented token/email lines.
2. [ ] B2 — Apply unsafe zero-alloc depth read to `HandTrackingVisualizer.cs:315`.
3. [ ] B4 — Remove the per-frame `Debug.Log` in `BodySpawned.Update()`.
4. [ ] B3 — Finish `ErrorResponse` consolidation **or** formally reclassify it as deferred tech-debt.
5. [ ] 1.1 — Guard the `SelectedObjectType` assignment behind a successful parse.
6. [ ] Clean dead `tempModel.glb` line + remaining commented blocks.

**Must-verify on device (QA pass, no code needed):**
7. [ ] Apple sign-in button still fires after rename (Inspector onClick).
8. [ ] Hand/face tracking lag & jitter acceptable with retained SmoothDamp (3.1).
9. [ ] Jewelry scale correct across small vs. large faces (3.3).
10. [ ] Ghost placement: vertical vs horizontal planes, material, tap-to-place (3.4).
11. [ ] Memory: browse 30–50 products, watch texture memory in Profiler (4.4/B5).
12. [ ] Empty-form submit returns a friendly API error (1.2 trade-off).

**Should-do before scaling:**
13. [ ] Add a minimal CI that at least compiles the Unity project.
14. [ ] Decide: adopt or delete the unused `TextureCache.cs`.

---

## 8. Bottom Line

The build is **close** but not launch-clean. The architecture work is real and solid; the gap is that **the action report marks several items "done" that the code shows as missing or half-done** — most importantly the security-logging item (B1) and the watch-path GC fix (B2). None of the blockers are large; the entire must-fix list is roughly **one focused engineering day plus a device QA pass**. After B1–B5 are cleared and the §7 device checks pass, this is good to go live.
