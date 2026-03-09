# Build Log — BFF Emergence Unity

A phase-by-phase development diary of the BFF GPU simulation project.
Built over multiple sessions in March 2026. Most code written by GitHub Copilot
under direction of Erich Curtis.

---

## Motivation

Erich watched Sebastian Lague's video "Coding Adventure: Ant and Slime Simulations"
and was captivated by the visual quality of real-time GPU slime physics. Separately,
the BFF paper (Agüera y Arcas et al., 2024) described a minimal computational life
system with rich emergent behavior. The goal: combine Lague's visual approach with
the BFF system to make the paper's findings visible in real time.

Hardware: RTX 4070 Ti, Windows 11, Unity 6000.3.10f1.

---

## Phase 1 — MVP: BFF Running on GPU

**Goal:** Get a working BFF simulation running as a Unity compute shader.

**Key decisions:**
- Lague's architecture: C# MonoBehaviour (BFFSimulation.cs) orchestrates a
  ComputeShader (BFFSim.compute) with a ScriptableObject for settings (BFFSettings.cs)
- Two kernels to start: StepEpoch (VM execution) + UpdateColourMap (display)
- Cells stored as flat uint[] buffer: `TapeData[cell_index * tapeSize + byte_index]`
- Display via RenderTexture wired to a Quad's material mainTexture
- Scene built by editor tooling: `Tools > BFF > Create Scene`

**Unity 6 compat fix:**
`GraphicsFormat.R16G16B16A16_SFloat` moved namespace in Unity 6.
Fix: use `RenderTextureFormat.DefaultHDR` instead. This affects Lague's original
project too — documented for anyone porting to Unity 6.

**Result:** 512×512 grid of BFF VMs running at 60fps. Random colored noise at startup,
gradual convergence to dominant programs visible within seconds.

**Commit:** `49fc629` Phase 1: GPU BFF simulation running in Unity

---

## Phase 2 — Visual Polish

**Goal:** Make the output beautiful and informative, not just functional.

**Changes:**
- HSV color model: hue = dominant instruction category, saturation = instruction density
- Instruction categories → hues:
  - Movement `< >` → blue (0.58)
  - Aux movement `{ }` → purple (0.72)
  - Math `+ -` → green (0.33)
  - Copy `. ,` → gold (0.12) — the replicators
  - Loop `[ ]` → red (0.02) — the parasites
- Bloom shader: 4-pass (bright extract → horizontal blur → vertical blur → composite),
  camera OnRenderImage ping-pong
- Entropy HUD: Shannon entropy sampled from 512 random cells every 30 frames
- Preset system A/B/C/D mirroring Lague's approach

**DX11 warning fix:**
Backward bracket scan used `(scan - 1) % combined` which triggers a DX11 signed int
modulo performance warning. Fix: `((scan - 1) % combined + combined) % combined`
(all-unsigned arithmetic).

**Commit:** `e28898f` Phase 2: visual polish

---

## Phase 3 — Species Territory Map

**Goal:** Make the Red Queen arms race visible as competing identities, not just
competing instruction categories.

**Approach:** FNV-1a hash of entire tape content → unique hue per program identity.
Byte-exact copies share color. Single-byte mutations shift hue slightly.
Evolutionary distance becomes visible as color gradient.

**Implementation:**
- `displayMode` uniform: 0 = instruction categories, 1 = species identity
- S key toggles at runtime with console log confirmation
- `BFFSceneCreator` updated with new presets

**Why FNV-1a:** Fast, well-distributed, no external library needed in HLSL.
32-bit output divided by max uint gives 0-1 hue value.

**Commit:** `f8084fc` Phase 3: species territory map

---

## Fix — Runtime tapeSize

**Problem discovered:** `TAPE_SIZE` was a compile-time `#define`. Changing tapeSize
in the Inspector resized the C# buffer but the shader still executed 16-byte programs.
Larger tape sizes silently did nothing.

**Root cause:** HLSL local arrays must be compile-time sized. The shader had
`uint buf[TAPE_SIZE]` where TAPE_SIZE was a macro.

**Fix:**
- Declare arrays at max capacity: `#define MAX_TAPE 128` / `#define MAX_COMBINED 256`
- Pass `tapeSize` as a runtime uniform (`int tapeSize`)
- All loop bounds use the runtime uniform: `for (int i = 0; i < tapeSize; i++)`
- Arrays are always MAX_TAPE sized; only the first `tapeSize` elements are used

Now any power-of-2 tape size from 4 to 128 works live from the Inspector.

**Commit:** `24d820f` Fix: tapeSize now fully runtime-driven

---

## Phase 4 — Chemotaxis Trail Layer + Information-Density Muting

**Goal:** Add visual depth without changing the BFF VM. Make evolved programs glow.
Make empty cells dark and atmospheric. Give replicator fronts organic motion.

**Chemotaxis trail (Lague's approach applied to BFF):**
- New kernels: `DiffuseTrail` and `DepositTrail`
- Active cells deposit signal onto `TrailMap` (RHalf RenderTexture, R channel only)
- `DiffuseTrail`: 3×3 mean blur + `(1 - decayRate)` decay — Lague's exact approach
- `StepEpoch` reads `DiffusedTrailMap`: with probability `chemotaxisStrength`,
  picks the cardinal neighbor with highest trail instead of a random one
- Dispatch order is critical: DiffuseTrail → StepEpoch → DepositTrail → UpdateColourMap
  (trail must be spread before VM reads it; VM must run before deposit updates it)

**Information-density muting:**
- `density = activeInstructions / tapeSize` (0.0 to 1.0)
- Steep power curve: `saturation = pow(density * 3.2, 2.8)`
  - density 0.05 → saturation 0.01 (nearly invisible)
  - density 0.30 → saturation 0.50 (clear color)
  - density 0.50 → saturation 1.00 (fully vivid)
- Value also dims low-density cells: `lerp(0.12, 1.0, pow(density * 2.2, 1.4))`
- Ghost cells (zero instructions): `float3(0.04, 0.06, 0.14)` — very dark blue-grey
- Trail glow: `trailGlow = saturate(trail / 3.0)` boosts brightness of active paths

**New BFFSettings parameters:**
- `trailWeight` (default 1.5) — how much active cells deposit per epoch
- `decayRate` (default 0.015) — fraction of trail lost per epoch
- `diffuseRate` (default 0.25) — how much trail spreads to neighbors
- `chemotaxisStrength` (default 0.65) — 0 = fully random, 1 = always follow trail

**Commit:** `f67c27e` Phase 4: chemotaxis trail + information-density colour curve

---

## The CMB Observation (2026-03-09)

During a live session, Erich noted that the simulation at maximum entropy
(7.84/8.00 bits) looks like the cosmic microwave background radiation.
Research confirmed this is structurally correct, not just aesthetic.

See SCIENCE.md for the full framework.

**Commit:** `be61ec8` Docs: CMB observation + Phase 5 roadmap

---

## Phase 5 — Planned: Structure Formation

**Goal:** Document and demonstrate the CMB-to-galaxy-formation arc.

**Plan:**
- Drop mutationRate from 0.00002 to 0.000002
- Watch entropy fall from ~8.00 to 1-3 range as programs dominate
- Record species-identity view (S key) during the transition
- Add entropy history graph to HUD (line chart, last 1000 samples)
- The cooling curve is the "proof" of the mutation-as-temperature framework
