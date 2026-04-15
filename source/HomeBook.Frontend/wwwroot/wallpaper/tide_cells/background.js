(function () {
  const canvas = document.getElementById("backgroundCanvas");
  const ctx = canvas.getContext("2d");
  const config = window.WallpaperConfig || {};
  const params = new URLSearchParams(window.location.search);
  const baseInput = params.get("baseColor") || params.get("color") || config.baseColor || "#4ebfd0";

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
    probe.fillStyle = "#4ebfd0";
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
  "seed": 6647,
  "blobCount": 6,
  "radiusMin": 170,
  "radiusMax": 254,
  "speedMin": 0.065,
  "speedMax": 0.128,
  "blur": 30,
  "hueShift": -11
};
  const rng = createRng(settings.seed);
  const blobs = Array.from({ length: settings.blobCount }, function (_, index) {
    return {
      x: rng(),
      y: rng(),
      radius: settings.radiusMin + rng() * (settings.radiusMax - settings.radiusMin),
      speedX: settings.speedMin + rng() * (settings.speedMax - settings.speedMin),
      speedY: settings.speedMin + rng() * (settings.speedMax - settings.speedMin),
      phase: rng() * TAU + index,
      hueShift: -20 + rng() * 40
    };
  });

  function render(time) {
    const background = ctx.createLinearGradient(0, 0, width, height);
    background.addColorStop(0, hsla(base.h + settings.hueShift - 14, base.s + 26, base.l - 24, 1));
    background.addColorStop(0.5, hsla(base.h + settings.hueShift + 2, base.s + 30, base.l - 18, 1));
    background.addColorStop(1, hsla(base.h + settings.hueShift + 18, base.s + 18, base.l - 28, 1));
    ctx.fillStyle = background;
    ctx.fillRect(0, 0, width, height);

    ctx.save();
    ctx.globalCompositeOperation = "lighter";
    ctx.filter = "blur(" + settings.blur + "px)";
    blobs.forEach(function (blob) {
      const x = blob.x * width + Math.sin(time * blob.speedX + blob.phase) * width * 0.18;
      const y = blob.y * height + Math.cos(time * blob.speedY + blob.phase * 0.7) * height * 0.18;
      const radius = blob.radius * (0.84 + Math.sin(time * 0.34 + blob.phase) * 0.1);
      const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius);
      gradient.addColorStop(0, hsla(base.h + settings.hueShift + blob.hueShift, base.s + 44, base.l + 42, 0.34));
      gradient.addColorStop(0.5, hsla(base.h + settings.hueShift + blob.hueShift * 0.4, base.s + 28, base.l + 18, 0.18));
      gradient.addColorStop(1, hsla(base.h + settings.hueShift, base.s + 10, base.l, 0));
      ctx.fillStyle = gradient;
      ctx.beginPath();
      ctx.arc(x, y, radius, 0, TAU);
      ctx.fill();
    });
    ctx.restore();

    ctx.strokeStyle = hsla(base.h + settings.hueShift + 22, base.s + 32, base.l + 54, 0.06);
    ctx.lineWidth = 1.2;
    blobs.forEach(function (blob, index) {
      const x = blob.x * width + Math.sin(time * blob.speedX + blob.phase) * width * 0.18;
      const y = blob.y * height + Math.cos(time * blob.speedY + blob.phase * 0.7) * height * 0.18;
      ctx.beginPath();
      ctx.ellipse(x, y, blob.radius * 0.72, blob.radius * 0.42, time * 0.08 + index * 0.2, 0, TAU);
      ctx.stroke();
    });
  }

  function frame(now) {
    render(now * 0.001);
    window.requestAnimationFrame(frame);
  }

  resize();
  window.addEventListener("resize", resize, { passive: true });
  window.requestAnimationFrame(frame);
})();
