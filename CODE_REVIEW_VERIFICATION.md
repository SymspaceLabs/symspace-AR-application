# Symspace AR — Code Review Verification & Findings

**Date:** 2026-06-23
**Branch:** `claude/tender-brown-fzlj50` (latest build, pushed 2026-06-19)
**What this is:** I took the developer's written response to the code review and checked every claim against the actual code — and the scene/build files, not just the C# scripts. This document explains, in plain terms, **where the developer's report matches reality, where it doesn't, and what else I found that neither document mentions.**

---

## The Big Picture (read this first)

Think of the developer's response document as a **status report**, and this document as the **audit of that status report**.

The good news: real work happened. Most of the structural cleanup and several of the bug fixes are genuinely in the code. The developer wasn't making things up wholesale.

The catch: **the status report is more optimistic than the code.** Several items are marked "Fixed," "Optimized," or "Implemented" that are actually missing, only half-done, or contradicted by what's in the files. If you take the report at face value, you'd believe the app is in better shape than it is.

The most important thing I found has nothing to do with the review items at all: **two of the main AR features (hand tracking for rings/watches, and body tracking for clothing) can't even load in a real build right now** because their scenes were never added to the build list. A teammate testing in the Editor might miss this entirely; it only shows up on a device. This is exactly the kind of thing that a code-only review can't catch but a build/scene audit can — and it's the single biggest issue in the project today.

Here's the scoreboard:

| Category | Count | Meaning |
|---|---|---|
| ✅ Genuinely done | 12 | Verified in code, matches the claim |
| 🟠 Overstated | 4 | Claimed complete, but code shows missing/half-done |
| 🟡 Judgment call | 3 | Developer declined the suggestion on purpose — reasonable, but needs device testing |
| 🔴 New issues I found | 4 | Not in either document; some are serious |

---

## Part 1 — Where the Developer's Report Doesn't Match the Code

These are the **inconsistencies**: the report says one thing, the code says another. Plain-English version of each.

### 🟠 1. "Security logging is handled" — it isn't, really
- **Report says:** Sensitive logs (tokens, emails) were addressed; will be "wrapped in conditional checks or removed in production."
- **What's actually true:** The proposed safety tool (`SymspaceDebug`) was **never created** — it appears nowhere in the code. The risky lines that print access tokens and emails were simply **commented out**, not removed. And other logs that print the user's name are still live and will end up in phone logs on a real build.
- **Why it matters:** A commented-out line is one careless keystroke away from shipping a user's login token into device logs. "Commented out" is not "secured." There's no guardrail stopping the next person from logging a token again.

### 🟠 2. "GC/stutter fix optimized" — only half the fix landed
- **Report says:** The per-frame memory-allocation problem in the depth-reading code was optimized away.
- **What's actually true:** It was fixed in the **ring** code (`RingPlacer`) but the **identical problem was left untouched in the watch/wrist code** (`HandTrackingVisualizer` still calls `buffer.ToArray()` every frame).
- **Why it matters:** Rings will track smoothly; watches will still stutter from the same garbage-collection hitches the fix was meant to remove. Same bug, same file pattern, only one of two places fixed.

### 🟠 3. "Duplicate data classes removed" — they're still everywhere
- **Report says:** Duplicate `ErrorResponse` classes were removed and everything uses one shared class.
- **What's actually true:** The `ErrorResponse` class is **still defined separately in 9 files.** Only two screens were cleaned up.
- **Why it matters:** Nine slightly-different copies of the same "error message" structure means error handling can behave differently on different screens, and it's a trap for the next developer who edits "the" class and doesn't realize there are eight others.

### 🟠 4. "Per-frame body logging removed" — still there (but harmless, see below)
- **Report says:** (folded into the security item) frame-rate logging would be removed.
- **What's actually true:** `BodySpawned` still prints a log on **every single frame**. *However* — and this is the silver lining — I checked the scenes and this component **isn't actually used in any live scene**, so it's currently doing nothing. It's dead code with a landmine in it.
- **Why it matters:** Low urgency, but it should be deleted so nobody drops it into a scene later and tanks the frame rate.

---

## Part 2 — New Issues I Found (not mentioned in either document)

These came from looking past the C# scripts into the **scene and build configuration** — the part a pure code review skips.

### 🔴 5. Two major AR features can't load in a build (most serious finding)
The app's product menu (`CategoriesUI`) sends users into the **"Hand Tracking"** scene (rings, watches) and the **"AR Body Tracking With Mars"** scene (clothing). But the project's build list only contains **5 scenes**: Splash, Home, Blogs, AR Scene, AR Face. **Hand Tracking and Body Tracking are not in it.**
- **Plain version:** When a user taps a ring or a clothing item in a real installed app, Unity will try to open a scene that isn't included in the build and **fail to load it** (Unity throws: *"scene couldn't be loaded because it has not been added to the build settings"*).
- **Why this is easy to miss:** It works fine in the Unity Editor (the Editor can open any scene), so a developer testing on their machine wouldn't see it. It only breaks on an actual device build — which is precisely why a real device test matters.
- **Fix:** Add Hand Tracking, AR Body Tracking, and the Mars variants to the build settings (and decide whether the duplicate "Hand Tracking 1" scene is the real one). 5 minutes — but it's a feature-breaking gap until done.

### 🔴 6. Apple Sign-In appears to be disconnected
The method was correctly renamed from the `SingInWithApple` typo to `SignInWithApple` (good). But I can't find **anything that calls it** — no code calls it, and no button in any scene references it by name.
- **Plain version:** The "Sign in with Apple" button may currently do nothing, because the thing it's supposed to trigger isn't wired to it. This is the exact risk the developer's own report warned about ("re-wire the button after renaming") — and the evidence suggests the re-wiring didn't happen.
- **Caveat:** Button wiring lives in the scene file and I'm reading it as text; it's *possible* it's connected in a way I can't see. This one needs a 30-second check in the Editor to confirm — but it looks orphaned.

### 🔴 7. The "category not saved" fix has a small new edge case
The original bug (item 1.1) is fixed, but the new version assigns the parsed category **even when parsing fails**, which resets it to the default value instead of leaving it alone. Also, the parser matches category names by *substring*, so similarly-named categories could be matched incorrectly. Low severity, worth a 2-minute tidy.

### 🔴 8. Repository hygiene: crash dumps and bloat are committed
- `Assets/_Recovery/` contains **8.3 MB of Unity crash-recovery scene dumps** (`0 (1).unity`, `0 (2).unity`, …) that should never be in source control — a sign the Editor crashed and the auto-saved junk got committed.
- The repo is heavy: **3.1 GB of assets and a 1.6 GB git history.** Worth a `.gitignore` review and possibly a history cleanup so the project stays clonable.

---

## Part 3 — What the Developer Got Right (credit where due)

These I verified as genuinely done, matching the report:

- ✅ The category-routing bug is fixed (the core problem, aside from the edge case above).
- ✅ Null-crash guard added to the ring visualizer.
- ✅ The stray debug log inside `HandleSingleItem` is gone.
- ✅ Ring scale "magic numbers" (`* 250 * 2f`) removed; models now normalize to a real-world size on load.
- ✅ Ghost/hologram placement preview for furniture & TVs exists and is wired in.
- ✅ The expensive per-frame `FindObjectOfType` lookup is now cached.
- ✅ Per-frame landmark memory allocation eliminated.
- ✅ The shared `ModelLoaderService` exists and is used by all four download sites.
- ✅ Scene names are centralized into constants (hardcoded strings gone).
- ✅ Model caching by product ID is in place.
- ✅ Dead files deleted; `SafeParent` filename/class mismatch fixed.
- ✅ Apple typo renamed (the rename itself — see issue #6 for the wiring).

---

## Part 4 — Judgment Calls (the developer declined on purpose)

Not inconsistencies — the developer deliberately chose not to do these and gave a reason. The reasons are defensible, but each is a **"feel" decision that can only be confirmed on a real device**, so don't sign them off from code alone:

- **Kept SmoothDamp instead of the One-Euro Filter (item 3.1).** Reason: smoother, more "pleasing" motion. Trade-off: the reviewer's concern was that this adds noticeable lag on fast hand movement and jitter when holding still. → *Have someone move their hand fast, then hold it still, and judge.*
- **Did not add face-relative jewelry sizing (item 3.3).** Reason: items are attached to the AR face mesh, which already scales. But the code also forces items to fixed absolute sizes, which may fight that. → *Test a necklace on a small face vs. a large face.*
- **Left server-side input validation only (item 1.2).** Reason: avoid duplicating the API's checks. Trade-off: every empty-form tap now makes a network call, and the experience depends entirely on the API returning friendly messages. → *Confirm the live API returns clean errors for blank email/password.*

---

## Part 5 — The Honest Limits of This Review

I read the **source, the scenes (as text), and the build settings** — that's how I caught the build-list and Apple-button issues. But some things genuinely can't be known without opening the project in Unity 6000.2.8f1 on a machine with a display, and ultimately running it on a phone:

- Whether buttons/serialized references are actually connected (I can read the scene text, but Unity's GUID wiring is best confirmed in the Editor).
- Real frame rate, memory growth, and AR tracking quality on a device.
- Anything involving the live camera (hand/face/body tracking can't run headless or even in the Editor without a device).
- The backend/API behavior (explicitly out of scope).

This environment is a headless Linux container with no Unity and no display, so a true in-Editor preview isn't possible here — these need a developer's machine or a device build.

---

## Part 6 — Suggested Fix Order

**Fix first (feature-breaking or risky):**
1. Add **Hand Tracking + Body Tracking scenes to the build settings** (#5) — features are currently unreachable in a build.
2. Confirm/repair the **Apple Sign-In button wiring** (#6).
3. Create the **`SymspaceDebug` wrapper**, route auth logs through it, and **delete** (not comment) the token/email lines (#1).

**Fix soon (quality/consistency):**
4. Apply the **zero-allocation depth read to the watch code** to match the ring code (#2).
5. **Delete `BodySpawned`** (or remove its per-frame log) (#4).
6. Guard the **category assignment** behind a successful parse (#7).
7. Decide on **`ErrorResponse`**: finish consolidating into one shared class, or accept it as known tech-debt — but stop calling it "done" (#3).

**Housekeeping:**
8. Remove `Assets/_Recovery/` crash dumps and review `.gitignore` / repo size (#8).
9. Delete the unused `TextureCache.cs` **or** wire it in — right now it's written but never used, so the "no memory leak" claim is unproven. Worth a Profiler check while browsing many products.

**Validate on a device (no code needed):**
10. Hand/face tracking lag & jitter, jewelry scale across face sizes, ghost placement, and empty-form API errors (Part 4).

---

## Bottom Line

The developer did real work and most of the cleanup is solid. But the written response **reads as more finished than the code is** — four items are overstated, and separately, **two core AR features don't load in a build and Apple Sign-In looks disconnected.** None of these are huge fixes; most are minutes of work. The key takeaway is to treat the developer's status report as a starting point to verify, not as a finished checklist — and to get the project onto an actual device, because the most damaging issues are the ones that only appear in a real build.
