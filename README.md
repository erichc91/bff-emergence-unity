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
- **Phase 2** — Visual polish: bloom, HDR, entropy HUD, species heat map
- **Phase 3** — Parameter exploration UI, preset system (like Lague's A/B/C/D)
- **Phase 4** — Connect to bff-emergence Python experiment pipeline

---

## License

BFF algorithm: [Agüera y Arcas et al. 2024](https://arxiv.org/abs/2406.19108) (open research)
Unity architecture inspired by Sebastian Lague (GPL v3)
