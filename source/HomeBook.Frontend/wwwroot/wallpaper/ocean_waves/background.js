(function () {
  const canvas = document.getElementById("backgroundCanvas");
  const ctx = canvas.getContext("2d");
  const config = window.WallpaperConfig || {};
  const params = new URLSearchParams(window.location.search);
  const baseInput = params.get("baseColor") || params.get("color") || config.baseColor || "#4d8ddc";

  function clamp(value, min, max) {
    return Math.max(min, Math.min(max, value));
  }

  function rgbToHsl(r, g, b) {
    r /= 255;
    g /= 255;
    b /= 255;
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    let h = 0;
    let s = 0;
    const l = (max + min) / 2;

    if (max !== min) {
      const d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      switch (max) {
        case r:
          h = (g - b) / d + (g < b ? 6 : 0);
          break;
        case g:
          h = (b - r) / d + 2;
          break;
        default:
          h = (r - g) / d + 4;
      }
      h *= 60;
    }

    return { h, s: s * 100, l: l * 100 };
  }

  function parseColor(input) {
    const probe = document.createElement("canvas").getContext("2d");
    probe.fillStyle = "#4d8ddc";
    probe.fillStyle = input;
    const normalized = probe.fillStyle;
    let r = 77;
    let g = 141;
    let b = 220;

    if (normalized.startsWith("#")) {
      let hex = normalized.slice(1);
      if (hex.length === 3) {
        hex = hex.split("").map((part) => part + part).join("");
      }
      r = Number.parseInt(hex.slice(0, 2), 16);
      g = Number.parseInt(hex.slice(2, 4), 16);
      b = Number.parseInt(hex.slice(4, 6), 16);
    } else {
      const match = normalized.match(/rgba?\(([^)]+)\)/);
      if (match) {
        [r, g, b] = match[1].split(",").slice(0, 3).map((part) => Number.parseFloat(part));
      }
    }

    return rgbToHsl(r, g, b);
  }

  function hsla(h, s, l, a) {
    return `hsla(${((h % 360) + 360) % 360}, ${clamp(s, 0, 100)}%, ${clamp(l, 0, 100)}%, ${clamp(a, 0, 1)})`;
  }

  const base = parseColor(baseInput);
  let width = 1;
  let height = 1;
  let dpr = 1;

  function resize() {
    dpr = Math.min(window.devicePixelRatio || 1, 2);
    width = window.innerWidth;
    height = window.innerHeight;
    canvas.width = Math.floor(width * dpr);
    canvas.height = Math.floor(height * dpr);
    canvas.style.width = `${width}px`;
    canvas.style.height = `${height}px`;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  }

  function drawGlow(x, y, radius, hueShift, alpha) {
    const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius);
    gradient.addColorStop(0, hsla(base.h + hueShift, base.s + 24, base.l + 28, alpha));
    gradient.addColorStop(0.55, hsla(base.h + hueShift * 0.5, base.s + 12, base.l + 10, alpha * 0.35));
    gradient.addColorStop(1, hsla(base.h, base.s, base.l, 0));
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.arc(x, y, radius, 0, Math.PI * 2);
    ctx.fill();
  }

  function drawWave(layer, time) {
    const depth = layer / 5;
    const yBase = height * (0.34 + depth * 0.44) + Math.sin(time * 0.16 + layer) * 18;
    const amplitude = 18 + layer * 12 + height * 0.012;
    const phase = time * (0.24 + depth * 0.12) + layer * 1.1;

    ctx.beginPath();
    ctx.moveTo(-40, height + 40);
    ctx.lineTo(-40, yBase);

    for (let x = -40; x <= width + 40; x += 12) {
      const nx = x / Math.max(width, 1);
      const y =
        yBase +
        Math.sin(nx * Math.PI * (3 + depth * 2.4) + phase) * amplitude +
        Math.sin(nx * Math.PI * 10 - phase * 1.4) * amplitude * 0.2;
      ctx.lineTo(x, y);
    }

    ctx.lineTo(width + 40, height + 40);
    ctx.closePath();

    const gradient = ctx.createLinearGradient(0, yBase - amplitude * 1.5, 0, height);
    gradient.addColorStop(0, hsla(base.h + layer * 4, base.s + 16, base.l + 22 - layer * 2, 0.32 - depth * 0.05));
    gradient.addColorStop(0.55, hsla(base.h - 4 + layer * 2, base.s + 24, base.l + 6 - layer * 2, 0.64));
    gradient.addColorStop(1, hsla(base.h - 8, base.s + 24, base.l - 24 - layer * 4, 0.98));

    ctx.fillStyle = gradient;
    ctx.fill();

    ctx.strokeStyle = hsla(base.h + 18, base.s + 34, base.l + 34 - layer * 3, 0.18);
    ctx.lineWidth = 2.4 - depth * 0.6;
    ctx.stroke();
  }

  function render(time) {
    const background = ctx.createLinearGradient(0, 0, 0, height);
    background.addColorStop(0, hsla(base.h - 10, base.s + 28, base.l - 34, 1));
    background.addColorStop(0.55, hsla(base.h - 2, base.s + 14, base.l - 22, 1));
    background.addColorStop(1, hsla(base.h + 8, base.s + 26, base.l - 10, 1));
    ctx.fillStyle = background;
    ctx.fillRect(0, 0, width, height);

    ctx.globalCompositeOperation = "screen";
    drawGlow(width * 0.78, height * 0.18, Math.max(width, height) * 0.34, 16, 0.22);
    drawGlow(width * 0.2, height * 0.68, Math.max(width, height) * 0.28, -18, 0.18);
    drawGlow(width * 0.52, height * 0.45, Math.max(width, height) * 0.2, 4, 0.14);
    ctx.globalCompositeOperation = "source-over";

    ctx.filter = "blur(10px)";
    for (let layer = 0; layer < 6; layer += 1) {
      drawWave(layer, time);
    }
    ctx.filter = "none";

    ctx.globalCompositeOperation = "lighter";
    ctx.strokeStyle = hsla(base.h + 22, base.s + 40, base.l + 42, 0.08);
    ctx.lineWidth = 1.2;
    for (let i = 0; i < 5; i += 1) {
      const y = height * (0.38 + i * 0.1) + Math.sin(time * 0.35 + i) * 12;
      ctx.beginPath();
      for (let x = -20; x <= width + 20; x += 16) {
        const py = y + Math.sin(x * 0.012 + time * 0.6 + i) * (8 + i * 1.5);
        if (x === -20) {
          ctx.moveTo(x, py);
        } else {
          ctx.lineTo(x, py);
        }
      }
      ctx.stroke();
    }
    ctx.globalCompositeOperation = "source-over";

    const vignette = ctx.createRadialGradient(width * 0.5, height * 0.55, height * 0.1, width * 0.5, height * 0.55, Math.max(width, height) * 0.8);
    vignette.addColorStop(0, "rgba(0, 0, 0, 0)");
    vignette.addColorStop(1, "rgba(0, 0, 0, 0.42)");
    ctx.fillStyle = vignette;
    ctx.fillRect(0, 0, width, height);
  }

  function frame(now) {
    render(now * 0.001);
    window.requestAnimationFrame(frame);
  }

  resize();
  window.addEventListener("resize", resize, { passive: true });
  window.requestAnimationFrame(frame);
})();
