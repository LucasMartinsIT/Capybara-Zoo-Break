# 🐾 Capybara Zoo Break — 3D Endless Runner

![Unity](https://img.shields.io/badge/Engine-Unity%206%20%2F%20URP-black?logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp)
![Shaders](https://img.shields.io/badge/Shaders-HLSL%20%26%20Shader%20Graph-blueviolet)
![Platform](<https://img.shields.io/badge/Platform-Mobile%20(iOS%20%2F%20Android)-orange>)

**Capybara Zoo Break** is a fast-paced, stylized 3D endless runner developed in **Unity (Universal Render Pipeline)** for mobile devices. Play as a runaway capybara escaping from a persistent zookeeper across procedurally generated urban and forest biomes, dodging staggered traffic hazards, climbing elevated truck convoys, collecting coins, and unlocking classic power-up upgrades and custom outfits.

---

## ✨ Implemented Features & Technical Architecture

- **Three-Lane Endless Runner Controller (`RunnerPlayerController`):** Responsive swipe and keyboard input handling with smooth lane switching, calibrated jump physics, ramp climbing, and dynamic vertical camera tracking.
- **Universal Object Pooling (`ObjectPooler`):** Zero-allocation runtime recycling system using dictionary-backed pools for ground chunks, buildings, sidewalk props, obstacles, and collectibles to prevent Garbage Collector spikes on mobile hardware.
- **Procedural Track, Convoy & Coin Generator (`EndlessTrackManager`):**
  - Infinite ground tile streaming with dynamic cleanup.
  - _Subway Surfers_-inspired staggered obstacle sequences with mathematical safe-lane validation (guaranteed reachable paths, diagonal clusters, and dynamic breathing zones).
  - **Vertical Level Design (Elevated Truck Convoys):** Procedural spawning of ramp-led container truck sequences allowing multi-level roof navigation and rooftop-to-rooftop jumping.
  - **Mathematical Coin Trails:** Automated coin pathing featuring sine-wave parabolas (`JumpArc`) over passable barriers, linear interpolation (`RampArc`) aligned with truck ramps, and clamped rooftop lines (`Line`).
- **Modular Environment Decorator (`CityTileDecorator`):** Socket-based procedural placement for buildings and sidewalk props featuring randomized depth setback, lateral jitter, and automatic `MeshRenderer.bounds` clamping to prevent sidewalk clipping.
- **Synchronized Curved World & Toon Outline Shaders (`HLSL` & `Shader Graph`):**
  - **Global GPU Curvature (`CurvedWorld.hlsl` & `CurvedLit`):** Paraboloid vertex deformation (horizontal bend + vertical horizon drop with a near-camera flat zone) controlled globally in real time via `Shader.SetGlobalFloat` (`_GlobalCurveX`, `_GlobalCurveY`, `_GlobalFlatZoneZ`).
  - **Standard & Low-Poly Inverted Hull Outlines (`InvertedHullOutline` & `InvertedHullOutlineLowPoly`):** Engineered to evaluate absolute world-space curvature prior to vertex extrusion, featuring perspective-correct clip-space Z-offsetting, radial hard-edge gap fixing, and native URP Linear Fog integration (`MixFog`) to prevent distant mesh desync.
- **Custom Editor Tooling:**
  - **`PrefabQuickTool`:** One-click Editor automation (`Ctrl + Shift + D`) that wraps raw 3D meshes into clean, zeroed-pivot parent containers and registers prefabs automatically.
  - **`ApplyCurvedShaderTool`:** Automated dependency scanner that batch-converts active URP Lit/Simple Lit prefab materials to `CurvedLit` while preserving base maps (`_BaseMap`) and leaving custom outline materials untouched.

---

## 🗺️ Development Roadmap (Target Release: Late December 2026)

### ✅ Sprint 1: Core Loop, Verticality & Visual Horizon (Completed)

- [x] Core 3-lane movement, ramp climbing & vertical camera follow system
- [x] Universal Object Pooler & infinite road tile streaming
- [x] Procedural building & sidewalk prop decorator with boundary protection
- [x] Coherent staggered obstacle spawning & elevated truck convoy sequences
- [x] Procedural coin trail spawning (`Line`, `JumpArc`, `RampArc`) & collection logic
- [x] Synchronized Curved World Shader Graph, URP Linear Fog & custom HLSL Inverted Hull Outlines

### 🚧 Sprint 2: Dynamic Traffic & Track Chokepoints (Next)

- [ ] **Moving Vehicles (`MovingVehicle`):** Dynamic cars and elevated truck convoys moving toward the player with relative speed adjustments.
- [ ] **Visual Telegraphing:** Blinking headlights and subtle engine vibration cues before stationary traffic accelerates.
- [ ] **Selective Tunnel Intersections (Chokepoints):** Modular overpass/construction structures blocking 1 or 2 lanes to force rapid lane-switching decisions.

### 🔮 Sprint 3: Classic Power-Ups & Active HUD

- [ ] **Centralized Power-Up Architecture (`PowerUpManager`):** Collectible spawning and multi-buff stacking logic.
- [ ] **5 Signature Power-Ups:**
  - **Super Jump (Wings):** Back-mounted flapping wings with heightened jump force and responsive fast-fall gravity.
  - **Coin Magnet (Tesla Coil):** Backpack Tesla coil emitting electric arcs to pull coins across adjacent lanes and elevations.
  - **Immunity Shield (Shopping Cart & Hardhat):** Absorbs 1 obstacle impact, triggering a cart-smash particle effect and clearing nearby forward hazards.
  - **2x Score Multiplier (Star):** Dynamic HSV rainbow material/outline effect with star trail particles.
  - **Mega Capybara (Giant Mode):** Short-duration kaiju transformation that scales up the capybara and kicks oncoming vehicles out of the track.
- [ ] **Active Power-Up HUD:** Real-time duration bars and score multiplier indicators.

### 🛠️ Sprint 4: Upgrade Economy, Game Feel & Art Polish

- [ ] **Progression & Upgrade Shop:** Persistent coin economy (`PlayerPrefs`/JSON) allowing players to upgrade the base duration of all 5 power-ups (Levels 1 to 6).
- [ ] **Slide / Roll & Stumble Mechanics:** Ducking under high barriers, side-impact stumble penalties, and Zookeeper chase catch-up logic.
- [ ] **Complete UI, Audio & VFX Polish:** Main Menu, Pause, Game Over flow, impact VFX, vehicle horns, coin pickup SFX, and soundtrack.
- [ ] **Art, Cutscene & Customization:** Custom stylized texture pass, Capybara cosmetic shop (Hats, Glasses, Skins), and seamless Zoo Escape intro sequence.

---

## 🛠️ Tech Stack & Tools

- **Engine:** Unity 6 (Universal Render Pipeline - URP)
- **Programming & Shaders:** C#, HLSL (ShaderLab), URP Shader Graph
- **3D Modeling, Rigging & Texturing:** Blender, Rigify, Substance Painter

---

## 📄 License & Commercial Notice

This repository is temporarily public for portfolio and version control purposes. All source code, custom shaders, and original 3D assets are proprietary and reserved for upcoming commercial release.
