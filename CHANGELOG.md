## Orion Changelog

# [beta-0.2.1]

- fix: Post-processing shader not suited for every screen ratio

# [beta-0.2.0]

- feat: changed 1D line for a 3D model arrow as the pointing-arrow
- feat: changed the user's waypoint for a red-wave plane acting as a sonar-like
- feat: improved camera lerping movement on earth-interface
- feat: added connexion to the Websocket of the robot with motor movement
- feat: improved focus mode on earth-interface, with ability to directly change location
- feat: added graphical post-processing with chromatic aberration and noise
##
- fix: texture switch on earth-interface focus now works properly
- fix: ISS pointing now works properly according to the directions
- fix: click sound playing even when not in terminal-interface

# [beta-0.1.0]
- feat: virtual 2D compass in top-screen intended for orientation
- feat: planets information recovery from AstronomyAPI + displaying in orion terminal
- feat: planets position calculation based on orientation + 3D displaying
- feat: implemented targets for the Sun and the Moon
##
- fix: inserting viewpoint modifications through terminal didn't update the arrow
- fix: pointer orientation for ISS didn't work properly -> introduced North-West offseting

# [beta-0.0.0]

- feat: terminal-screen intended for robot-control and viewpoint modification 
- feat: layered render used for alpha blending with holograms
- feat: theoretical astral-target modification through terminal-screen
- feat: viewpoint coordinates modification through terminal-screen
- feat: new skybox texture
- feat: light sound-design with ambient music
- feat: better quality earth-globe texture
- feat: earth-globe closeup when clicked on (later intended for map-tiling)
##
- fix: circular-border crossing when leaving/entering an interface
- fix: unclamped camera rotation-angle causing ridiculously fast spins when entering the terminal's interface
- fix: 'e' character causing interface closing when typing text through textboxes
- fix: offset ground tiles
- fix: default cursor causing offseting in terminal-screen
- fix: updated RayGUI-cs version to avoid cursor-state bugs

# [alpha-0.2.0]

- feat: earth-globe's UX interface
- feat: orion terminal's interface opening
- feat: lerped interfaces' opening/closing actions
- feat: upgraded ECEF functions by adding rotation values + reverse ECEF

# [alpha-0.1.0]

- feat: an earth-globe stands in the middle of the circular platform
- feat: the ISS revolves around the globe in real-time
- feat: implemented a Uniray-designed environment
- feat: PBR-based materials and lighting

---

[beta-0.2.1]: https://git.s2.rpn.ch/ComtesseE1/orion/-/releases/beta-0.2.1
[beta-0.2.0]: https://git.s2.rpn.ch/ComtesseE1/orion/-/releases/beta-0.2.0
[beta-0.1.0]: https://git.s2.rpn.ch/ComtesseE1/orion/-/tags/beta-0.1.0
[beta-0.0.0]: https://git.s2.rpn.ch/ComtesseE1/orion/-/releases/beta-0.0.0
[alpha-0.2.0]: https://git.s2.rpn.ch/ComtesseE1/orion/-/tags/alpha-0.2.0
[alpha-0.1.0]: https://git.s2.rpn.ch/ComtesseE1/orion/-/tags/alpha-0.1.0