# BFF Emergence — Unity GPU Simulation

A real-time GPU visualization of **computational emergence** in the BFF (Bytewise Function Fields)
virtual machine — running 262,144 parallel BFF VMs on your RTX 4070 Ti.

Inspired by: [Agüera y Arcas et al. arXiv:2406.19108](https://arxiv.org/abs/2406.19108)
Architecture: [Sebastian Lague's slime sim conventions](https://github.com/SebLague/Slime-Simulation)

---

## What You're Watching

Each pixel is a BFF cell (16-byte tape). Colors show the dominant instruction type:

| Colour | Instruction | Meaning |
|--------|------------|---------|
| Black | `0x00` | Null / empty |
| Blue | `< >` | Movement |
| Purple | `{ }` | Aux head movement |
| Green | `+ -` | Math |
| **Gold** | **`. ,`** | **Copy — the replicators** |
| Red | `[ ]` | Loop structure — parasites |
| Grey | other | Raw data |

**What emergence looks like:**
1. Random static → gold spreads (replicators taking over)
2. Red flickers in (parasites exploiting replicators)
3. Waves and oscillations (Red Queen arms race)

---

## Architecture (Lague conventions)

```
Assets/
  Scripts/BFF/
    BFFSettings.cs      — ScriptableObject: all parameters
    BFFSimulation.cs    — MonoBehaviour: GPU orchestrator
  Shaders/
    BFFSim.compute      — Two kernels: StepEpoch + UpdateColourMap
  Editor/
    BFFSceneCreator.cs  — Tools > BFF > Create Scene
  Scenes/
    BFF.unity
  Settings/
    Default.asset       — Default simulation parameters
```

---

## Quick Start

```powershell
.\launch.ps1
# Then: Tools > BFF > Create Scene > Play
```

---

## Parameters (BFFSettings inspector)

| Parameter | Default | Effect |
|-----------|---------|--------|
| width / height | 512 × 512 | Grid size (higher = more cells, slower) |
| tapeSize | 16 | Bytes per cell — more = richer programs |
| instructionLimit | 64 | Max BFF cycles per interaction |
| stepsPerFrame | 1 | Speed — increase for faster evolution |
| mutationRate | 0.00024 | Mutation probability per byte |

---

## Phase Roadmap

- **Phase 1** ✅ — Baseline GPU BFF sim running in Unity
- **Phase 2** ✅ — Visual polish: HSV colour model, bloom, entropy HUD, presets A/B/C/D
- **Phase 3** ✅ — Species territory map (FNV-1a hash → hue), S-key toggle
- **Phase 4** ✅ — Chemotaxis trail layer + information-density colour muting
- **Phase 5** — Structure formation: mutation-as-temperature, CMB → galaxy-formation transition

---

## The CMB Observation (2026-03-09)

At high mutation rate (0.00002), entropy stabilises at ~7.84/8.00 bits and the sim
produces a near-uniform blue-indigo noise field with scattered vivid specks.
This is the **CMB epoch of the BFF universe**.

The parallel is structural, not just aesthetic:

| Universe | BFF Sim |
|----------|---------|
| CMB at 2.73 K — near-uniform thermal noise | Entropy 7.84/8.00 — near-maximum Shannon noise |
| Temperature anisotropies 1 part in 100,000 | Vivid specks in the noise field |
| Mutation rate = temperature of the BFF universe | High mutation → hot → no structure |
| Cooling → galaxy formation | Lower mutation → territory wars emerge |
| Boltzmann fluctuation → galaxy seed | Yellow bubble → Boltzmann fluctuation that dissolved |
| MEPP: life accelerates entropy production | Replicators maximise local entropy output |

**To advance from CMB epoch to galaxy formation:**
- Drop `mutationRate`: `0.00002` → `0.000002`
- Shannon entropy will fall from 7.84 → 4-6 range
- Red Queen territory wars (the "galaxies") crystallise
- Press **S** for species-identity view to see the mosaic clearly

---

## License

BFF algorithm: [Agüera y Arcas et al. 2024](https://arxiv.org/abs/2406.19108) (open research)
Unity architecture inspired by Sebastian Lague (GPL v3)
