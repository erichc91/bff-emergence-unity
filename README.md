# BFF Emergence — Unity GPU Simulation

> **Making computational life visible in real time.**

### Recording 1 — The CMB Arc (entropy 7.97 → 0.99)
![BFF Emergence — CMB to galaxy formation](preview.gif)

*Blue-indigo noise field (entropy ~8.00, CMB epoch) → yellow/purple two-species territory mosaic
after mutation rate reduction. The phase transition is sharp, not gradual.*

### Recording 2 — Colony Nucleation & Total Conversion
![BFF Emergence — colony nucleation and expansion](emergence.gif)

*Individual replicator colonies nucleate from noise, grow into amoeba-shaped blobs, merge,
and eventually convert the entire grid — leaving only one dark fractal void before total coverage.*

**Full videos on Google Drive:**
- [bff_recording.mp4](https://drive.google.com/file/d/19xj6tYnocVRSzLsqj60Vhsxp20uHzluz/view?usp=drivesdk) — 2548×1340, 39s

---

## What This Is

A real-time GPU visualization of the **BFF (Bytewise Function Fields)** computational
life system, running 262,144 parallel virtual machines simultaneously on an RTX 4070 Ti.

The BFF algorithm and all core dynamics (replicator emergence, Red Queen arms race,
entropy collapse under selection) are the work of:

> Agüera y Arcas et al., *"Computational Life: How Well-formed, Self-replicating Programs Emerge from Simple Interaction"*, arXiv:2406.19108 (2024)

This project makes their published findings **visible in real time**. The BFF paper
documented behavior in Python on CPU. This is a GPU port with visual extensions.

### What is new here (not in the original paper)
- Real-time GPU visualization at 60fps (262,144 VMs in parallel)
- Chemotaxis trail layer — replicator fronts form organic branching tendrils
- Species identity color model — evolutionary distance readable as hue gradient
- Information-density color muting — evolved programs glow, empty cells are dark
- The mutation-as-temperature / CMB interpretive framework

### Honest note on how this was built
Most code was written by **GitHub Copilot** (Claude Sonnet) under direction of Erich Curtis.
The architectural conventions were learned from Sebastian Lague's slime simulation.
Erich is not a GPU shader developer by background. The CMB parallel was his direct
observation during live experimentation. See [CREDITS.md](CREDITS.md) for full attribution.

---

## Mutation rate behaves like a temperature

At high mutation rate the simulation sits at near-maximum Shannon entropy
(~7.97 of 8.00 bits) — a near-uniform blue-indigo noise field with rare vivid
specks. Lowering the mutation rate collapses entropy sharply rather than
gradually: a phase transition, after which territory wars crystallise.

Treating mutation rate as a temperature is what made that legible while tuning,
and the high-entropy state is a loose analogy for the cosmic microwave
background. **It is an analogy between two descriptions, not two physical
systems** — there is no spectrum, no temperature in any physical sense, and no
isotropy measurement here. `SCIENCE.md` says where the comparison stops.

---

## Architecture (Lague conventions)

```
Assets/
  Scripts/BFF/
    BFFSettings.cs        -- ScriptableObject: all parameters
    BFFSimulation.cs      -- MonoBehaviour: GPU orchestrator, entropy HUD
  Shaders/
    BFFSim.compute        -- 4 kernels: DiffuseTrail, StepEpoch, DepositTrail, UpdateColourMap
    Bloom.shader          -- 4-pass bloom (bright extract, H blur, V blur, composite)
  Scripts/BFF/
    BloomEffect.cs        -- Camera bloom via OnRenderImage
  Editor/
    BFFSceneCreator.cs    -- Tools > BFF > Create Scene, presets A/B/C/D
  Scenes/
    BFF.unity
  Settings/
    Default.asset
```

**Dispatch order each epoch (order is critical):**
1. `DiffuseTrail` — spread and decay existing trail
2. `StepEpoch` — BFF VM execution, trail-biased neighbor selection
3. `DepositTrail` — active cells deposit new signal
4. `UpdateColourMap` — render display texture

---

## Quick Start

**Requirements:** Unity 6000.3.10f1, Windows (DX11/DX12), NVIDIA GPU recommended

```powershell
git clone https://github.com/erichc91/bff-emergence-unity
# Open in Unity Hub -> Unity 6000.3.10f1
# Tools > BFF > Create Scene
# Press Play
```

**Controls:**
- `S` key — toggle between instruction-category view and species-identity view
- All parameters live-editable in BFFSettings Inspector while running

---

## Parameters (BFFSettings Inspector)

### Grid / Virtual Machine
| Parameter | Default | Effect |
|-----------|---------|--------|
| width / height | 512 × 512 | Grid size |
| tapeSize | 16 | Bytes per cell (4–128, powers of 2) |
| instructionLimit | 64 | Max BFF cycles per cell interaction |
| stepsPerFrame | 5 | Epochs per rendered frame |
| mutationRate | 0.00024 | Mutation probability per byte per epoch |

### Chemotaxis (Trail Layer)
| Parameter | Default | Effect |
|-----------|---------|--------|
| trailWeight | 1.5 | Signal deposited per active instruction per epoch |
| decayRate | 0.015 | Fraction of trail lost per epoch |
| diffuseRate | 0.25 | How much trail spreads to neighbors |
| chemotaxisStrength | 0.65 | 0 = fully random neighbor, 1 = always follow trail |

### Tuning Guide
| Goal | Change |
|------|--------|
| See CMB epoch (noise) | mutationRate 0.00002, let run to 300k+ epochs |
| See galaxy formation | Drop mutationRate to 0.000002 |
| See branching tendrils | Increase chemotaxisStrength to 0.8+ |
| Slow down to watch | Reduce stepsPerFrame to 1–3 |

---

## Phase Roadmap

- **Phase 1** ✅ — Baseline GPU BFF sim running in Unity
- **Phase 2** ✅ — HSV colour model, bloom, entropy HUD, presets A/B/C/D
- **Phase 3** ✅ — Species territory map (FNV-1a hash → hue), S-key toggle
- **Phase 4** ✅ — Chemotaxis trail layer + information-density colour muting
- **Phase 5** — Structure formation: entropy history HUD, cooling curve recording

---

## Documentation

| Document | Contents |
|----------|---------|
| [SCIENCE.md](SCIENCE.md) | Physics framework, CMB parallel, what this shows |
| [CREDITS.md](CREDITS.md) | Full attribution — paper, Lague, Copilot, Erich |
| [docs/BUILD_LOG.md](docs/BUILD_LOG.md) | Phase-by-phase dev diary, technical decisions |
| [data/proof/READINGS.md](data/proof/READINGS.md) | Entropy measurements, reproducibility data |

---

## License

**Code:** GPL-3.0 — see [LICENSE](LICENSE). This Unity project derives from Sebastian Lague's GPL-3.0 Slime-Simulation; see [CREDITS.md](CREDITS.md) for exactly what was reused.

BFF algorithm: Agüera y Arcas et al. 2024 (open research, arXiv:2406.19108)
Architecture inspired by: Sebastian Lague's Slime Simulation (GPL v3, not derived)
