# Symspace AR — Developer Fix Checklist

Cross-referenced from the developer response doc (last month) vs. the actual code on `main` (commit `7f4bec4`, pushed 2026-06-19). Some changes landed, but these gaps remain. Work top to bottom.

---

## 🔴 Feature-breaking (do first)

- [ ] **Add Hand Tracking & Body Tracking scenes to Build Settings.** The product menu (`CategoriesUI`) loads `"Hand Tracking"` and `"AR Body Tracking With Mars"`, but the build list only has Splash, Home, Blogs, AR Scene, AR Face. Rings/watches and clothing will fail to load in a real device build (works in Editor, breaks on device). Also decide if the duplicate `Hand Tracking 1` scene is the real one.
- [ ] **Wire up Apple Sign-In.** `SignInWithApple()` was renamed correctly but nothing calls it and no button references it — the button looks disconnected. Confirm/reconnect the onClick in the scene.

## 🟠 Claimed done but still open

- [ ] **Security logging.** Token/email lines are only commented out, not removed, and the `SymspaceDebug` wrapper described in the response was never created. Create the wrapper (with `[Conditional]` attributes), route auth logs through it, and delete the token/email lines outright.
- [ ] **Depth-buffer GC fix (watch code).** The zero-allocation fix landed in `RingPlacer` but `HandTrackingVisualizer` still calls `buffer.ToArray()` every frame. Apply the same unsafe-pointer read there so watches don't stutter.
- [ ] **Consolidate `ErrorResponse`.** Still defined in ~9 files. Merge into one shared class (or formally log it as known tech-debt — but it isn't "done" yet).
- [ ] **Remove per-frame logging in `BodySpawned`.** Still logs every frame in `Update()`. It's not used in any scene right now, so just delete the file (and its `.meta`).

## 🟡 Smaller fixes

- [ ] **ProductSelection edge case.** The category is now assigned even when parsing fails (resets it to default). Wrap the assignment in a successful-parse check. Also note the parser matches by substring, which can mis-match similar category names.
- [ ] **Unused `TextureCache.cs`.** It was committed but is never called — either wire it in or delete it. The "no memory leak" claim is unverified; worth a Profiler check while browsing many products.

## 🧹 Housekeeping

- [ ] **Remove `Assets/_Recovery/`** — 8.3 MB of Unity crash-recovery scene dumps committed to the repo.
- [ ] **Repo size review** — 3.1 GB assets / 1.6 GB history; check `.gitignore` and consider trimming.

## 📱 Validate on a real device (no code change, just confirm)

- [ ] Hand/face tracking lag & jitter acceptable with the current SmoothDamp (One-Euro filter was declined — confirm it feels right on fast + still hands).
- [ ] Jewelry scale looks correct on a small face vs. a large face (face-relative sizing was declined).
- [ ] Ghost/hologram placement works for both furniture (floor) and TVs (wall).
- [ ] Empty sign-in/sign-up forms return a friendly error from the API (client validation was removed on purpose).

---

*See `CODE_REVIEW_VERIFICATION.md` for the full plain-language explanation of each item.*
