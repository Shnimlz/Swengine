<h1 align="center">Swengine</h1>

<p align="center">
  <img src="https://i.imgur.com/DNUkC6w.png" alt="Swengine logo"/>
</p>

**Swengine** es una aplicación de escritorio para Linux que te permite gestionar, descargar, subir y aplicar wallpapers animados o estáticos desde múltiples fuentes, incluyendo integración directa con Wallpaper Engine y soporte para Swww.

---

## 🚀 Características principales

- Descarga wallpapers desde sitios populares: MotionBgs, Wallhaven, Moewalls, WallpapersCraft, WallpapersClan, DesktopHut, MyLiveWallpapers y más.
- Sube y aplica wallpapers locales fácilmente.
- Integración con **Wallpaper Engine** para importar y usar wallpapers descargados localmente.
- Soporte para **Swww** y **FFMPEG** para aplicar wallpapers animados en escritorios Linux.
- Interfaz gráfica moderna y rápida (Avalonia).

---

## 📦 Instalación desde AUR

### Instalación automática (recomendado)

```sh
yay -S swengine-revanced
paru -S swengine-revanced
```

### Instalación manual

```sh
git clone https://aur.archlinux.org/swengine-revanced.git
cd swengine-revanced
makepkg -si
```

Esto compilará la app y la instalará en tu sistema.

---

## 🛠️ Dependencias

- `vlc`
- `ffmpeg`
- `swww`
- `dotnet-sdk` version 9.0 (solo para compilar)
- `git` (solo para compilar)

---

## 💡 ¿Qué hace Swengine?

- Busca y descarga wallpapers desde múltiples proveedores.
- Importa wallpapers desde tu instalación local de Wallpaper Engine.
- Usa Swww y FFMPEG para aplicar wallpapers animados.
- Permite subir wallpapers personalizados.
- Interfaz gráfica moderna y amigable.

---

## 🗂️ Estructura técnica

- `Helpers/`: Utilidades para integración con Swww, FFMPEG, Wallpaper Engine, descargas, etc.
- `Scrapers/`: Scrapers para cada proveedor de wallpapers soportado.
- `Services/`: Servicios de backend para cada proveedor y lógica de negocio.
- `ViewModels/`: Lógica de la UI y binding de datos.
- `Views/`: Archivos de interfaz gráfica (Avalonia).

---

## 🖥️ Uso

Ejecuta la aplicación desde el menú de aplicaciones o con:

```sh
swengine
```

---

## 🧑‍💻 Autoría y licencia

- Proyecto original: [github.com/eugenenoble2005/swengine](https://github.com/eugenenoble2005/swengine)
- Fork y mantenimiento: [Shnimlz](https://github.com/Shnimlz)
- Licencia: MIT

---

<p align="center">
  <b>¡Disfruta de tus nuevos wallpapers animados en Linux!</b>
</p>
