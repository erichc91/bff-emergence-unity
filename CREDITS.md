# Credits

## Science — The BFF Algorithm

**Agüera y Arcas, B., Alakuijala, J., Evans, J., Laurie, B., Mordvintsev, A.,
Niklasson, E., Randazzo, E., Versari, L.**
*"Computational Life: How Well-formed, Self-replicating Programs Emerge from
Simple Interaction"*
arXiv:2406.19108 (2024)
https://arxiv.org/abs/2406.19108

The Bytewise Function Fields virtual machine, instruction set, replication mechanism,
and Red Queen dynamics documented in this project are entirely their discovery.
This GPU visualization exists to make their work visible in real time.

---

## Architecture Inspiration — GPU Pipeline

**Sebastian Lague**
*Coding Adventure: Ant and Slime Simulations*
https://www.youtube.com/watch?v=X-iSQQgOd1A
https://github.com/SebLague/Slime-Simulation (GPL v3)

**This Unity project was started from a copy of Lague's, and is therefore a
derivative work. It is licensed GPL-3.0 to match.**

Earlier versions of this file and of `LICENSE` claimed that no code was copied
or adapted. That was not accurate, and the evidence is in this repository:

- 19 of the 21 files in `ProjectSettings/` are byte-identical to his project.
- `ProjectSettings.asset` still reads `productName: Slime Mold`, and
  `EditorBuildSettings.asset` still points at `Assets/Scenes/Slime.unity` — a
  scene from his project that does not exist here.
- The trail diffuse-and-decay step at `BFFSim.compute:64` follows his
  implementation closely; the comment on that line says so.
- The integer hash at `:52-57` uses the same constants, which he in turn
  credits to Schechter and Bridson.
- `decayRate`, `diffuseRate` and `TrailMap` are his names.

What is mine: the BFF virtual machine, its instruction set, the replication and
mutation mechanics, the species-identity hashing, the chemotaxis trail layer,
and every experiment and parameter choice. Those were written from the paper
specification, not from his project.

Copyright (C) 2026 Erich Curtis. Licensed under GPL-3.0 — see `LICENSE`.

---

## AI Co-Development

**GitHub Copilot** (Claude Sonnet 4.6)

Most of the code in this repository was written by GitHub Copilot under human
direction. This includes:
- The BFF compute shader (BFFSim.compute) — all 4 kernels
- The C# orchestration (BFFSimulation.cs)
- The bloom shader and camera effect
- The scene creation editor tooling
- The species identity FNV-1a hashing
- The chemotaxis trail layer implementation
- All documentation in this repository

The AI did not observe, experiment, or make creative decisions.

---

## Project Direction and Experimental Work

**Erich Curtis**
https://github.com/erichc91

- Identified the BFF paper and the goal of making it GPU-visualisable
- Chose Sebastian Lague's architectural conventions as the right model
- Ran all experiments, tuned all parameters in real time
- Observed the yellow bubble / blue chaos Red Queen dynamics
- Identified the CMB parallel (entropy 7.84/8.00 = CMB epoch)
- Derived the mutation-as-temperature framework from direct observation
- All creative and experimental decisions — every phase direction call
- Recorded bff_recording.mp4 showing the CMB-to-galaxy-formation arc

---

## Honest Note on What This Is

Erich is not a GPU shader developer by background. This project was built
through the combination of:
1. A compelling piece of published science (the BFF paper)
2. A master-level reference implementation to learn architecture from (Lague)
3. AI code generation to bridge the skill gap
4. Genuine curiosity and hands-on experimentation to find what it does

The result is a working real-time GPU simulation of a published computational
life system with novel extensions — built without the traditional prerequisites.
That's not a disclaimer. It's a description of how this kind of work can happen now.
