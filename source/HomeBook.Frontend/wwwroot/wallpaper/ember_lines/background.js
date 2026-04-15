(function () {
  const canvas = document.getElementById("backgroundCanvas");
  const ctx = canvas.getContext("2d");
  const config = window.WallpaperConfig || {};
  const params = new URLSearchParams(window.location.search);
  const baseInput = params.get("baseColor") || params.get("color") || config.baseColor || "#d5443f";

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

    return { h: h, s: s * 100, l: l * 100 };
  }

  function parseColor(input) {
    const probe = document.createElement("canvas").getContext("2d");
    probe.fillStyle = "#d5443f";
    probe.fillStyle = input;
    const normalized = probe.fillStyle;
    let r = 77;
    let g = 141;
    let b = 220;

    if (normalized.startsWith("#")) {
      let hex = normalized.slice(1);
      if (hex.length === 3) {
        hex = hex.split("").map(function (part) {
          return part + part;
        }).join("");
      }
      r = Number.parseInt(hex.slice(0, 2), 16);
      g = Number.parseInt(hex.slice(2, 4), 16);
      b = Number.parseInt(hex.slice(4, 6), 16);
    } else {
      const match = normalized.match(/rgba?\(([^)]+)\)/);
      if (match) {
        const parts = match[1].split(",").slice(0, 3).map(function (part) {
          return Number.parseFloat(part);
        });
        r = parts[0];
        g = parts[1];
        b = parts[2];
      }
    }

    return rgbToHsl(r, g, b);
  }

  function hsla(h, s, l, a) {
    const hh = ((h % 360) + 360) % 360;
    return "hsla(" + hh + ", " + clamp(s, 0, 100) + "%, " + clamp(l, 0, 100) + "%, " + clamp(a, 0, 1) + ")";
  }

  function createRng(seed) {
    let state = seed >>> 0;
    return function () {
      state = (state * 1664525 + 1013904223) >>> 0;
      return state / 4294967296;
    };
  }

  const TAU = Math.PI * 2;
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
    canvas.style.width = width + "px";
    canvas.style.height = height + "px";
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  }

  const settings = {
  "seed": 5243,
  "lineCount": 15,
  "sparkCount": 23,
  "lineWidth": 1.4,
  "amplitude": 11.8,
  "frequency": 3.2,
  "speed": 0.30000000000000004,
  "hueShift": -14,
  "hueStep": 1.46
};
  const rng = createRng(settings.seed);
  const sparks = Array.from({ length: settings.sparkCount }, function (_, index) {
    return {
      x: rng(),
      y: rng(),
      size: 0.8 + rng() * 2.4,
      speed: 0.02 + rng() * 0.08,
      phase: rng() * TAU + index
    };
  });

  function render(time) {
    const backdrop = ctx.createLinearGradient(0, 0, 0, height);
    backdrop.addColorStop(0, hsla(base.h + settings.hueShift - 30, base.s + 18, 6, 1));
    backdrop.addColorStop(0.5, hsla(base.h + settings.hueShift - 10, base.s + 24, base.l - 20, 1));
    backdrop.addColorStop(1, hsla(base.h + settings.hueShift + 8, base.s + 18, base.l - 12, 1));
    ctx.fillStyle = backdrop;
    ctx.fillRect(0, 0, width, height);

    ctx.save();
    ctx.globalCompositeOperation = "screen";
    ctx.lineWidth = settings.lineWidth;
    for (let line = 0; line < settings.lineCount; line += 1) {
      const yBase = (line / Math.max(settings.lineCount - 1, 1)) * height;
      ctx.beginPath();
      for (let x = -20; x <= width + 20; x += 18) {
        const nx = x / Math.max(width, 1);
        const y = yBase
          + Math.sin(nx * Math.PI * settings.frequency + line * 0.55 + time * settings.speed) * settings.amplitude
          + Math.sin(nx * Math.PI * 12 - time * settings.speed * 1.4 + line) * settings.amplitude * 0.18;
        if (x === -20) {
          ctx.moveTo(x, y);
        } else {
          ctx.lineTo(x, y);
        }
      }
      ctx.strokeStyle = hsla(base.h + settings.hueShift + line * settings.hueStep, base.s + 36, base.l + 30, 0.12);
      ctx.stroke();
    }
    ctx.restore();

    ctx.globalCompositeOperation = "lighter";
    sparks.forEach(function (spark, index) {
      const x = spark.x * width + Math.sin(time * 0.18 + spark.phase) * width * 0.02;
      const y = ((spark.y - time * spark.speed) % 1 + 1) % 1 * height;
      const radius = spark.size * 6;
      const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius);
      gradient.addColorStop(0, hsla(base.h + settings.hueShift + 22, base.s + 52, base.l + 62, 0.32));
      gradient.addColorStop(1, hsla(base.h + settings.hueShift, base.s + 10, base.l, 0));
      ctx.fillStyle = gradient;
      ctx.beginPath();
      ctx.arc(x, y, radius, 0, TAU);
      ctx.fill();
    });
    ctx.globalCompositeOperation = "source-over";
  }

  function frame(now) {
    render(now * 0.001);
    window.requestAnimationFrame(frame);
  }

  resize();
  window.addEventListener("resize", resize, { passive: true });
  window.requestAnimationFrame(frame);
})();
