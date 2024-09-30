# Saturn
Saturn is a cross platform 2D game engine made to be simple and lightweight. <br/>
If you encounter a problem, please submit an issue.

<br/>

## Supported platforms
### Desktop 🖥️
| OS | ARM | ARM64 | x86 | x64 |
| :---: | :---: | :---: | :---: | :---: |
| Linux (X11 / Wayland) | 🚧 | 🚧 | 🚧 | ✔️ |
| Windows 11 | :x: | 🚧 | 🚧 | ✔️ |
| Windows 10 | :x: | 🚧 | 🚧 | ✔️ |
| Windows 8 / 8.1 | :x: | :x: | 🚧 | 🚧 |
| Windows 7 | :x: | :x: | 🚧 | 🚧 |
| FreeBSD | :x: | 🚧 | 🚧 | 🚧 |
| MacOS | :x: | 🚧 | :x: | 🚧 |

<br/>

### Mobile 📱
| OS | ARM | ARM64 |
| :---: | :---: | :---: |
| Android | 🚧 | 🚧 |
| iOS | 🚧 | 🚧 |

<br/>

## License
Saturn is licensed under the [MIT](https://opensource.org/license/mit) license, meaning you're free to do whatever you'd like with the engine! 
No royalties or fees of any kind will ever be collected.

<br/>

## Extras
### Roadmap
<b>2024 Q4</b>:
* Add 2D click & drag scene editting tools
* Add audio & font & shader support

<br/>

<b>2025 Q1</b>:
* Add controller support
* Implement in-app debugging
* Add high quality documentation

<br/>

### Known bugs
* The current Box2D physics implementation is broken and must be fixed (No collisions)
* Fix the FileSystemWatcher (complains about too many files open on Linux systems)
* Fix scene importing (Scene name does not get set properly)
