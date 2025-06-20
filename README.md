<!-- Swengine logo -->
<!-- Icono y texto alado  -->
<h1 align="center"><img src="https://i.imgur.com/FsoLb79.png" alt="Swengine logo" width="40" style="vertical-align:middle;"/>&nbsp;Swengine</h1>

<p align="center">
  <img src="https://i.imgur.com/3zvwTU1.png" alt="Swengine logo"/>
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
o
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

## ⚠️ Alto consumo de memoria RAM

> [!WARNING]
> No se recomienda usar Swengine con menos de 8GB de RAM.
> Ni mucho menos en laptops no sabemos si consumira su bateria.

Swengine requiere un alto consumo de memoria RAM para funcionar correctamente. Asegúrate de tener suficiente RAM disponible en tu sistema.

Esto es debido a que Swengine utiliza FFMPEG para convertir los wallpapers en GIFs y Swww para aplicarlos cuanta mas resolución y FPS se elija mas memoria se consumirá.

> [!NOTE]
> Esto es de un wallpaper a q1080p con 60 fps.

![img](https://i.imgur.com/W8JxxZp.png)

> [!NOTE]
> Esto es de un wallpaper a q2160p con 60 fps, no se nota la diferencia pero el consumo de CPU es significativo.

![img](https://i.imgur.com/04mXS5w.png)


---

## ⚠️ Carpeta necesaria para integración con Wallpaper Engine

Para que Swengine pueda importar wallpapers desde Wallpaper Engine, debes tener instalada la aplicación Wallpaper Engine desde Steam en el usuario actual. Es imprescindible que exista la siguiente carpeta en tu sistema:

```
/home/[tu_usuario]/.local/share/Steam/steamapps/workshop/content/431960/
```

[<video src="https://github.com/Shnimlz/Swengine/raw/main/example/_wallpaper.mp4" controls="controls" width="560" height="315"></video>
](https://github.com/user-attachments/assets/511e74ac-05cc-4b5d-8753-2fb80a1903ce)

Esta carpeta es creada automáticamente por Steam al instalar y usar Wallpaper Engine. Si no existe, Swengine no podrá importar wallpapers desde Wallpaper Engine.

---

## 🛠️ Dependencias

- `vlc`
- `ffmpeg`
- `swww-git`
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

---

## 🆕 Novedades recientes

- **Soporte completo para el backend MPVPAPER:** Usa videos como fondo animado con bajo consumo de CPU.
- **Cambio automático entre SWWW y MPVPAPER:** Al aplicar un fondo, se matan procesos previos del otro backend para evitar conflictos.
- **Arranque automático de swww-daemon:** Si no está corriendo al aplicar un fondo con SWWW, se inicia automáticamente.
- **mpvpaper optimizado:** Argumentos ajustados para menor consumo de CPU y RAM.
- **Mejor UX:** Notificaciones y diálogos funcionan correctamente con fondos persistentes (mpvpaper).

Revisa la documentación para detalles y recomendaciones actualizadas sobre el uso de cada backend.
