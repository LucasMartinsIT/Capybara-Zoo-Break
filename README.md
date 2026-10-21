# 🐾 Capybara Zoo Break — 3D Endless Runner

![Unity](https://img.shields.io/badge/Engine-Unity%206%20%2F%20URP-black?logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp)
![Shaders](https://img.shields.io/badge/Shaders-HLSL%20Custom-blueviolet)
![Platform](<https://img.shields.io/badge/Platform-Mobile%20(iOS%20%2F%20Android)-orange>)

**Capybara Zoo Break** is a fast-paced, stylized 3D endless runner developed in **Unity (Universal Render Pipeline)** for mobile devices. Play as a runaway capybara escaping from a persistent zookeeper across procedurally generated urban and forest biomes, dodging staggered traffic hazards, collecting coins, and unlocking custom outfits and run upgrades.

---

## ✨ Implemented Features & Technical Architecture

- **Three-Lane Endless Runner Controller:** Responsive swipe and keyboard input handling with smooth lane switching, jump physics, and collision detection.
- **Universal Object Pooling (`ObjectPooler`):** Zero-allocation runtime recycling system using dictionary-backed pools for ground chunks, buildings, sidewalk props, obstacles, and collectibles to prevent Garbage Collector spikes on mobile hardware.
- **Procedural Track & Obstacle Generator (`EndlessTrackManager`):**
  - Infinite ground tile streaming with dynamic cleanup.
  - _Subway Surfers_-inspired staggered obstacle sequences with mathematical safe-lane validation (guaranteed reachable paths, diagonal clusters, and dynamic breathing zones).
- **Modular Environment Decorator (`CityTileDecorator`):** Socket-based procedural placement for buildings and sidewalk props featuring randomized depth setback, lateral jitter, and automatic `MeshRenderer.bounds` clamping to prevent sidewalk clipping.
- **Custom URP Toon Outline Shaders (`HLSL`):**
  - **Standard Inverted Hull (`InvertedHullOutline`):** Optimized for smooth organic meshes with clip-space Z-offset depth pushing and GPU Instancing support.
  - **Low-Poly Hard-Edge Variant (`InvertedHullOutlineLowPoly`):** Blends vertex normals with radial center expansion to eliminate split-edge gaps on sharp low-poly vehicles and props.
- **Custom Editor Tooling (`PrefabQuickTool`):** One-click Editor automation (`Ctrl + Shift + D`) that wraps raw 3D meshes into clean, zeroed-pivot parent containers and registers prefabs automatically.

---

## 🗺️ Development Roadmap (Target Release: Late December 2026)

- [x] Core 3-lane movement & camera follow system
- [x] Universal Object Pooler & infinite road tile streaming
- [x] Procedural building & sidewalk prop decorator with boundary protection
- [x] Custom HLSL Inverted Hull Outline shaders (Smooth & Low-Poly variants)
- [x] Coherent staggered obstacle spawning system
- [ ] **Phase 1 (Core Loop & Environment Polish):**
  - [ ] Coin trail spawning & collection integration
  - [ ] Distance Fog & Curved World vertex shader integration
  - [ ] Obstacle distribution fine-tuning & difficulty scaling
  - [ ] In-run progression system (XP, levels, and power-up upgrades)
- [ ] **Phase 2 (UI, Audio & VFX):**
  - [ ] Complete UI/UX flow (Main Menu, HUD, Pause, Game Over, Upgrade Shop)
  - [ ] Visual Effects (VFX) for coin pickup, collisions, and level-up
  - [ ] SFX & background soundtrack integration
- [ ] **Phase 3 (Art, Cutscene & Customization):**
  - [ ] Custom hand-painted/stylized texture pass for buildings and props
  - [ ] Capybara cosmetic customization system (Hats, Glasses, Skins)
  - [ ] 3D Zookeeper character (Modeling, Texturing, Rigging, and Chasing Animation)
  - [ ] seamless Intro Sequence (Zoo escape transition into gameplay)

---

## 🛠️ Tech Stack & Tools

- **Engine:** Unity (Universal Render Pipeline - URP)
- **Programming:** C#, HLSL (ShaderLab)
- **3D Modeling & Texturing:** Blender, Substance Painter

---

## 📄 License & Commercial Notice

This repository is temporarily public for portfolio and version control purposes. All source code, custom shaders, and original 3D assets are proprietary and reserved for upcoming commercial release.
