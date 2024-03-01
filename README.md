<h2 align="centre">DanceGraph-Unity</h2>
<p align="center">DanceGraph Unity is an adapter endpoint for the DanceGraph low-latency engine-agnostic interactive avatar platform for dancing and having fun online. DanceGraph is a low-latency engine agnostic interactive avatar platform for dancing and having fun online. It is provided as an open source toolset from the <a href="https://carouseldancing.org">CAROUSEL+</a> EU funded FET PROACT project #101017779</p>

<div align="center">

[![Carousel Dancing Discord](https://dcbadge.vercel.app/api/server/eMcjUHN8rQ?style=flat)](https://discord.gg/eMcjUHN8rQ)
[![Twitter Follow](https://img.shields.io/twitter/follow/CarouselDancing.svg?style=social&label=Follow)](https://twitter.com/CarouselDancing)
[![Youtube Subscribe](https://img.shields.io/youtube/channel/subscribers/UCz2rCoDtFlJ4K1yOExu0AWQ?style=social)](https://www.youtube.com/channel/UCz2rCoDtFlJ4K1yOExu0AWQ?sub_confirmation=1)
[![Github Stars](https://img.shields.io/github/stars/CarouselDancing/dancegraph-unity?style=social)](https://github.com/CarouselDancing/dancegraph-unity/stargazers)
[![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-brightgreen.svg)](https://github.com/CarouselDancing/dancegraph-unity/graphs/commit-activity)
[![License](https://img.shields.io/badge/License-BSD_3--Clause-blue.svg)](https://opensource.org/licenses/BSD-3-Clause)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](CODE_OF_CONDUCT.md)
![Lines of code](https://tokei.rs/b1/github/CarouselDancing/dancegraph-unity)
<!--[![Github Downloads (total)](https://img.shields.io/github/downloads/CarouselDancing/dancegraph-unity/total.svg)](https://github.com/CarouselDancing/dancegraph-unity/releases)-->

</div>

## Overview

DanceGraph provides an application for *as-direct-as-possible* low-latency network signal transportation, specifically with referencing to ameliorating the latency issues with online dancing. It aims to provide as-direct-as-possible transport between the incoming network signals and the resulting visual output, as well as providing facilities for short-term latency prediction.

As far as possible, the architecture decouples the generation and handling of signal data, by the use of 'producers', which create signals, and 'consumers' which receive them. This allows for some producers and consumers to be written in a signal-agnostic fashion.

Transformers, which take a number of signals as consumer-style input and produce a new or altered signal, are an upcoming feature.

Currently, development is taking place with DanceGraph implemented as a Unity plugin, but the code has been written to support DanceGraph's use as a standalone executable when entirely decoupled from the engine, with only a stub plugin used to communicate with the standalone client via IPC producers and consumers.

This repository contains the Unity client, intended to be used in conjunction with the main 'dancegraph' repository containing the native Unity plugin, as well as the server code.

## 1. Installing and Running

For more detailed instructions on downloading and installing the native plugin, please refer to the README.md document provided in the main 'dancegraph' [project](https://github.com/CarouselDancing/dancegraph) on Github.

### Downloading and building the Unity client

Install Unity Hub from [here](https://unity.com/unity-hub)

Then clone the unity repo, e.g. with

    git clone git@github.com:CarouselDancing/dancegraph-unity.git

or

    git clone https://github.com/CarouselDancing/dancegraph-unity

Using Unity hub to start the project using 'Add project from disk'

Install the correct Unity version for the project (as at time of writing, 2022.3.5f1)

### Editing the client settings

The settings.json file contains a number of configuration settings that might warrant tweaking.

In the "preset" section, you will find the desired username, the "scene", which should match that of the server, the "role" (which should be one of the roles in the "scene" section of dancegraph.json", and, importantly, the server and client IP addresses, which should match those of your networking setup.

```json
    "username": "Fred",
    "scene": "env_testscene",
    "role": "dancer",
    "address": {
      "ip": "192.168.0.154",
      "port": 7800
    },
    "server_address": {
      "ip": "192.168.0.154",
      "port": 7777
    },
```

Optionally, the signal producer can be edited; (in the case below, it's been altered to produce a tpose skeleton rather than a pose from a zed camera), and consumers can be added or removed; in the below case, a consumer has been added to dump signal data to a file in the top level Unity hierarchy.

A third alternative for operation without a ZED camera is to use the 'null' producer, 'generic/null/v1.0', which will produce no signal and generate no avatar.

```json
    "producer_overrides": {
      "zed/v2.1": {
        "name": "tpose"
      },
      "env/v1.0": {
        "name": "generic/prod_ipc/v1.0",
        "opts": {
          "ipcInBufferName": "DanceGraph_Env_In",
          "ipcBufferEntries": 5
        }
      }
    },
    "user_signal_consumers": {
      "zed/v2.1": [
        {
          "name": "generic/dump2file/v1.0",
          "opts": {
          }
        }
      ]
    },
```

### VR Headsets

The Oculus Quest 2 headset is the main headset for the DanceGraph project. The project uses OpenXR, so other headsets may work, but are not supported.

Powering on the headset and putting it in in Air Link (via wifi) or Quest Link mode (using the supplied cable) and then starting the unity project should be enough to have the user's headset view from inside the gameworld. Note that the Quest 2 drops out of Air Link mode after some idle time.

### Running

The client software expects to find configuration files and libraries in %LOCALAPPDATA%/Dancegraph which are typically created and installed by building the dancegraph project (but may be created by an alternative installer). The client works either as a standalone windows executable or by pressing play inside the Unity Editor.

Currently, VR controllers can be used to move the HMD inside the game area, independently of the tracked avatar. Pressing the space bar should pause the music for all connected clients, and pressing tab cycles between the installed music tracks.


## References

B. Koniaris, D. Sinclair, K. Mitchell: _[DanceMark: An open telemetry framework for latency sensitive real-time networked immersive experiences](https://napier-repository.worktribe.com/output/3492930/dancemark-an-open-telemetry-framework-for-latency-sensitive-real-time-networked-immersive-experiences)_, IEEE VR 2024 OpenVRLab Workshop on Open Access Tools and Libraries for Virtual Reality.

D. Sinclair, A. Ademola, B. Koniaris, K. Mitchell: _[DanceGraph: A Complementary Architecture for Synchronous Dancing Online](https://farpeek.com/DanceGraph.pdf)_, 2023 36th International Computer Animation & Social Agents (CASA) . 
