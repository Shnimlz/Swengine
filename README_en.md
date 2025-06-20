<!-- Swengine logo -->
<!-- Icon and text side by side  -->
<h1 align="center"><img src="https://i.imgur.com/FsoLb79.png" alt="Swengine logo" width="40" style="vertical-align:middle;"/>&nbsp;Swengine</h1>

<p align="center">
  <img src="https://i.imgur.com/3zvwTU1.png" alt="Swengine logo"/>
</p>


**Swengine** is a desktop application for Linux that lets you manage, download, upload, and apply animated or static wallpapers from multiple sources, including direct integration with Wallpaper Engine and support for Swww.

---

## 🚀 Main Features

- Download wallpapers from popular sites: MotionBgs, Wallhaven, Moewalls, WallpapersCraft, WallpapersClan, DesktopHut, MyLiveWallpapers, and more.
- Easily upload and apply local wallpapers.
- Integration with **Wallpaper Engine** to import and use wallpapers downloaded locally.
- Support for **Swww** and **FFMPEG** to apply animated wallpapers on Linux desktops.
- Modern and fast graphical interface (Avalonia).

---

## 📦 Installation from AUR

### Automatic installation (recommended)

```sh
yay -S swengine-revanced
or
paru -S swengine-revanced
```

### Manual installation

```sh
git clone https://aur.archlinux.org/swengine-revanced.git
cd swengine-revanced
makepkg -si
```

This will compile the app and install it on your system.

---

## ⚠️ Required folder for Wallpaper Engine integration

For Swengine to import wallpapers from Wallpaper Engine, you must have Wallpaper Engine installed via Steam for the current user. The following folder must exist on your system:

```
/home/[your_user]/.local/share/Steam/steamapps/workshop/content/431960/
```

<video width="560" height="315" controls>
  <source src="https://i.imgur.com/LkrnLm7.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>

This folder is automatically created by Steam when you install and use Wallpaper Engine. If it does not exist, Swengine will not be able to import wallpapers from Wallpaper Engine.

---

## 🛠️ Dependencies

- `vlc`
- `ffmpeg`
- `swww-git`
- `dotnet-sdk` version 9.0 (only for compiling)
- `git` (only for compiling)

---

## 💡 What does Swengine do?

- Searches and downloads wallpapers from multiple providers.
- Imports wallpapers from your local Wallpaper Engine installation.
- Uses Swww and FFMPEG to apply animated wallpapers.
- Allows uploading custom wallpapers.
- Modern and user-friendly graphical interface.

---

## 🗂️ Technical Structure

- `Helpers/`: Utilities for integration with Swww, FFMPEG, Wallpaper Engine, downloads, etc.
- `Scrapers/`: Scrapers for each supported wallpaper provider.
- `Services/`: Backend services for each provider and business logic.
- `ViewModels/`: UI logic and data binding.
- `Views/`: Graphical interface files (Avalonia).

---

## 🖥️ Usage

Run the application from the applications menu or with:

```sh
swengine
```

---

## 🧑‍💻 Authors and License

- Original project: [github.com/eugenenoble2005/swengine](https://github.com/eugenenoble2005/swengine)
- Fork and maintenance: [Shnimlz](https://github.com/Shnimlz)
- License: MIT

---

<p align="center">
  <b>Enjoy your new animated wallpapers on Linux!</b>
</p>

---

## 🆕 Recent Updates

- **Full support for the MPVPAPER backend:** Use videos as animated wallpapers with low CPU usage.
- **Automatic switching between SWWW and MPVPAPER:** Applying a wallpaper kills previous processes from the other backend to avoid conflicts.
- **Automatic startup of swww-daemon:** If not running when applying a wallpaper with SWWW, it will be started automatically.
- **Optimized mpvpaper arguments:** Adjusted for lower CPU and RAM usage.
- **Better UX:** Notifications and dialogs now work correctly with persistent wallpapers (mpvpaper).

Check the documentation for updated details and recommendations for each backend.
