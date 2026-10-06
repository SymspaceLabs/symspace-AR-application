# Symspace AR — Full Project Review

**Date:** 2026-10-06
**Reviewed build:** `Symspace_IOS_V0.1` @ `9024c15` (2026-09-09) — the most feature-complete branch (superset of Android `main`).
**Scope:** whole stack — features, architecture, code quality, UX, security/hygiene. 118 scripts, ~19,200 lines.

> This is a holistic review, separate from the earlier code-review audits (which were all verified complete). Nothing here is a regression of those items — this looks at the app *as a product*.

---

## 1. What the app is today

A Unity (6000.2.8f1, URP) AR try-on commerce app for iOS. Users sign in, browse a catalog, and preview products on themselves / in their space via four AR modes. It has matured a lot: there's now a real navigation system, a search + filter suite, cart & favorites, and polished UI motion. The bones of a strong product are here.

---

## 2. Feature Inventory — what's in, partial, and missing

### ✅ Included and working
- **Authentication (broad):** email/password sign-in & sign-up, OTP verification (sign-up + forgot-password), forgot/reset password, Google, Apple (iOS), Facebook. User-facing error messages + loading panel are wired through `MenuManager.ShowError`.
- **Catalog & discovery:** product listing from the API, category browsing, **search** (debounced input + recent searches), and a **rich filter suite** (category, size, price range, color, brand).
- **Content:** Blogs/editorial, onboarding flow with body-measurement capture (ruler slider + unit toggle + haptics).
- **Cart & Favorites:** add/remove, size selection, quantity.
- **AR try-on (the core differentiator) — four modes:**
  - **Face jewelry** — necklaces, earrings, nose pins, glasses, caps (`ARJewelryManager`).
  - **Hand** — rings & watches with a custom iOS hand detector + depth-aware placement (`RingPlacer`, `HandTrackingVisualizer`).
  - **Body/clothing** — body tracking via Unity MARS.
  - **Surface placement** — furniture/TV on detected planes with the pulsing "ghost" preview + tap-to-place.
- **Navigation & polish:** bottom bar, back-stack, swipe-back gesture, animated loaders, gradient backgrounds, blur panels, safe-area handling, pressable/glow UI.

### 🟠 Partial / works but with a real catch
- **Cart & Favorites are device-local only.** They persist to `PlayerPrefs` (`CartData`, `FavoritesData`) as JSON — **not synced to the backend.** Consequences: the cart is lost on reinstall, never follows the user to another device, and can't drive a server-side checkout. For a shopping app this is a significant limitation, not a minor one.
- **There is no real authenticated session.** After login the app stores only the user `id` and `RememberMe=1` in `PlayerPrefs`. **No access token is attached to any API request** — there is not a single `Authorization`/`Bearer` header in the codebase. So the backend is effectively used for *auth and catalog only*; there's no authenticated per-user data layer (server cart, orders, profile). This is the root reason cart/favorites are local.
- **Error UX is good for auth, thin elsewhere.** Auth screens show server messages nicely, but the error path assumes a JSON body — on a true network failure the error text can be empty, so the user may see a blank error with no "check your connection / retry."
- **Images are re-downloaded every time.** `ShopUIManager`, `CartItemUI`, `FavoriteItemUI`, and the blog views each fetch textures per-call with no shared cache (the `TextureCache` written during the earlier review was never adopted). Causes jank on scroll and avoidable memory/network use.

### ❌ Not included (notable gaps for a commerce app)
- **No checkout / payment / order flow.** "Proceed to checkout" is a stub (logs a line, goes nowhere). No cart→order submission, no payment provider, no order confirmation.
- **No order history / tracking.**
- **No account management** beyond displaying the email on the profile page — no edit profile, saved addresses, or a clearly surfaced logout.
- **No camera-permission UX.** Nothing handles the user *denying* the camera — the app relies on AR Foundation's auto-prompt. If denied, AR scenes will likely show a black/broken view with no guidance or "Open Settings" path.
- **No first-run AR coaching.** Aside from the plane "Tap to place" hint, there's no overlay telling users what to do ("point at your hand," "face the camera," "scan a flat surface") and no "tracking lost / move slowly / improve lighting" state. AR is unfamiliar to most users; this is the biggest UX gap in the try-on experience.
- **No empty/offline states** in the catalog (no "no results", no "you're offline — retry").
- **No analytics or crash reporting**, no automated tests, no CI.
- **No localization** — all strings are hardcoded English.

---

## 3. Architecture & Code Cleanliness

What's good: there's a sensible folder split (Home/Shop/Cart/Blogs/UI/AR), a shared `ModelLoaderService` and `SceneNames`, a reusable `AuthAPI` wrapper, and a real navigation stack. The earlier audit items (security logging, GC, dedup, etc.) are all resolved on this branch.

Where it needs work:

- **God objects.** A few files carry far too much:
  - `Blogs/CategoriesUI.cs` — **2,243 lines, 51 methods** (this is the catalog brain; it should be several classes).
  - `AR/ARJewelryManager.cs` — 1,024 lines, 31 methods.
  - `UI/CategoryFilterUI.cs` — 901 lines, 62 methods.
  - `AR/Hand Tracking/HandItemSelector.cs` — 722 lines.
  These are the hardest files to change safely and the most likely home for future bugs. Split by responsibility (data fetch / view binding / state).
- **Zero namespaces.** All 118 scripts live in the global namespace. No `Symspace.*` boundaries → name-collision risk (you already hit this with duplicate `ErrorResponse`), and no clear module edges.
- **Logging.** 166 active `Debug.Log` calls. You *did* adopt a debug-gate flag (`isDebug`/`isDebugMode`, ~86 uses) which is good, but it's inconsistent and there's still no compile-time stripping (the `[Conditional]` `SymspaceDebug` wrapper was never created), so a lot of logging ships in release.
- **Thin error handling.** Only ~19 `try/catch` across ~19K lines. Many `JsonUtility.FromJson<...>()` calls run directly on raw response/error text — malformed or empty responses can throw `NullReferenceException` and crash the flow instead of showing a friendly message.
- **Leftover / experimental junk in the tree:**
  - `Assets/CapsuleJump.cs` (loose script at the Assets root),
  - `GradientTest.unity` + `Home/GradientInkTest.cs` (475 lines of experiment),
  - `Assets/_Recovery/` (Unity crash-recovery scenes) is **back** in the tree,
  - ~146 lines of commented-out code still scattered around,
  - a couple of misspelled names (`ARFaceSystemChecke.cs`, `Shadders` folder).
- **Duplicated image-download logic** across 4+ files (the shared cache exists but is unused).
- **Two divergent branches.** iOS and Android still share only the old June ancestor; fixes get applied twice by hand. This is a standing maintenance tax and a source of drift.

---

## 4. Security & Repo Hygiene (address before any public release)

- **🔴 The Android signing keystore is committed** — `Keystore/symspace.keystore`, tracked by git and **not** in `.gitignore`. Anyone with repo access can sign builds as Symspace. Remove it from the repo *and its history*, rotate the key, and keep it in a secrets manager / CI secret. Check that its password isn't also committed in Gradle/ProjectSettings.
- **🟠 Firebase configs committed** — `GoogleService-Info.plist`, `google-services.xml`, `google-services-desktop.json`. These ship in the app anyway, but they embed API keys; at minimum lock the Firebase API key down (bundle-id / referrer restrictions) and don't treat the repo as the source of truth.
- **No `.gitignore` coverage** for secrets, keystores, or local config.
- **Repo bloat** — multi-GB assets + heavy history; slow to clone and work with.

---

## 5. Prioritized Recommendations

**Tier 1 — needed for this to function as a shopping app**
1. **Add real authenticated sessions:** persist the access token securely and attach it (`Authorization: Bearer …`) to API calls via the `AuthAPI` wrapper.
2. **Move cart & favorites server-side** (tied to the account), so they survive reinstalls and sync across devices.
3. **Build the checkout/order flow** (cart → order → payment → confirmation), or explicitly label it "coming soon" so it isn't a dead button.
4. **Remove & rotate the committed keystore**; add a proper `.gitignore`.

**Tier 2 — the UX that makes AR feel good**
5. **Camera-permission screen:** detect denial, explain why it's needed, offer "Open Settings."
6. **First-run AR coaching per mode** + a "tracking lost / move slowly / better lighting" state.
7. **Loading, empty, and offline/retry states** across catalog, search, cart, and AR model downloads (show download progress for GLBs).
8. **Adopt the image cache** everywhere for smooth scrolling and lower memory.
9. **Harden the network layer:** wrap JSON parsing in try/catch, and show a generic "something went wrong — retry" on non-JSON/offline failures.

**Tier 3 — long-term cleanliness & confidence**
10. **Break up the god objects** (`CategoriesUI`, `ARJewelryManager`, `CategoryFilterUI`) and introduce `Symspace.*` namespaces.
11. **Finish the logging story:** one `SymspaceDebug` wrapper with `[Conditional]` stripping; remove ad-hoc logs.
12. **Reconcile the iOS/Android branches** (merge, or move platform differences behind `#if UNITY_IOS` in one branch) so fixes stop being done twice.
13. **Delete experimental junk** (`CapsuleJump.cs`, `GradientTest`, `_Recovery`, commented blocks) and fix the misspelled file/folder names.
14. **Add crash reporting/analytics** (you already have Firebase) and a minimal CI that compiles both platforms.

---

## 6. Bottom line

The app has grown into a genuinely capable AR try-on front end with a polished UI and four working AR modes — the hard, differentiating parts are real. The gap is that it's currently a **great catalog-and-try-on demo more than a finished store**: the money path (authenticated session → server cart → checkout) isn't built, shopping state is device-local, and the AR modes lack the permission/coaching UX that first-time users need. Close the Tier-1 commerce gaps and the Tier-2 AR UX, and this moves from "impressive prototype" to "shippable product." The cleanliness items (Tier 3) won't block launch but will decide how painful the next six months of changes are.
