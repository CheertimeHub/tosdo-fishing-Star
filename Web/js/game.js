// ════════════════════════════════════════════════════════════════════
//  Tosdo Fishing Star — HTML5 Prototype
// ════════════════════════════════════════════════════════════════════

const canvas = document.getElementById('gameCanvas');
const ctx    = canvas.getContext('2d');

const W       = canvas.width;   // 800
const H       = canvas.height;  // 500
const WATER_Y = 160;            // y ผิวน้ำ
const MAX_DEPTH = H - 40;       // ลึกสุดที่ hook ลงได้

// ── Game State ───────────────────────────────────────────────────────────────

const State = { MENU: 'menu', PLAYING: 'playing', GAMEOVER: 'gameover' };

const GameManager = {
  state:         State.MENU,
  duration:      90,
  timeRemaining: 90,

  start() {
    this.state         = State.PLAYING;
    this.timeRemaining = this.duration;
    ScoreManager.reset();
    HungerSystem.reset();
    FishSpawner.reset();
    FishingController.reset();
    popups.length = 0;
  },

  update(dt) {
    if (this.state !== State.PLAYING) return;
    this.timeRemaining -= dt;
    if (this.timeRemaining <= 0) { this.timeRemaining = 0; this.gameOver(); }
  },

  gameOver() {
    if (this.state === State.GAMEOVER) return;
    this.state = State.GAMEOVER;
    ScoreManager.saveBest();
  },
};

// ── Score ────────────────────────────────────────────────────────────────────

const ScoreManager = {
  current: 0,
  best:    parseInt(localStorage.getItem('fishBest') || '0'),

  reset()   { this.current = 0; },
  add(n)    { this.current = Math.max(0, this.current + n); },
  saveBest() {
    if (this.current > this.best) {
      this.best = this.current;
      localStorage.setItem('fishBest', this.best);
    }
  },
};

// ── Hunger ───────────────────────────────────────────────────────────────────

const HungerSystem = {
  max:          100,
  current:      70,
  decayPerSec:  5,

  reset()      { this.current = 70; },
  get percent(){ return this.current / this.max; },

  update(dt) {
    if (GameManager.state !== State.PLAYING) return;
    this.current = Math.max(0, this.current - this.decayPerSec * dt);
    if (this.current <= 0) GameManager.gameOver();
  },

  restore(n) { this.current = Math.min(this.max, this.current + n); },
};

// ── Fish Data ────────────────────────────────────────────────────────────────

const FISH_TYPES = [
  { name: 'ปลาทอง',     score:  100, hunger: 15, speed: 110, size: 22, color: '#FFD700', isJunk: false },
  { name: 'ปลาใหญ่',    score:  500, hunger: 35, speed:  55, size: 40, color: '#64CDFF', isJunk: false },
  { name: 'ปลาเร็ว',    score:  200, hunger: 10, speed: 190, size: 16, color: '#90EE90', isJunk: false },
  { name: 'หม้อโอเดน',  score: 2000, hunger: 60, speed:  40, size: 48, color: '#F4A460', isJunk: false },
  { name: 'กระป๋อง',    score: -150, hunger:  0, speed:  75, size: 20, color: '#888',    isJunk: true  },
];

// ── Fish Entity ──────────────────────────────────────────────────────────────

class Fish {
  constructor(data, x, y, dir) {
    this.data   = data;
    this.x      = x;
    this.y      = y;
    this.dir    = dir;   // +1 ขวา / -1 ซ้าย
    this.caught = false;
    this.dead   = false;
  }

  update(dt) {
    if (this.dead) return;
    this.x += this.dir * this.data.speed * dt;
    if (this.x < -100 || this.x > W + 100) this.dead = true;
  }

  draw() {
    if (this.dead) return;
    const { x, y, data, dir } = this;
    const s = data.size;

    ctx.save();
    ctx.translate(x, y);
    if (dir < 0) ctx.scale(-1, 1);

    if (data.isJunk) {
      // กระป๋องสี่เหลี่ยม
      ctx.fillStyle   = data.color;
      ctx.strokeStyle = '#555';
      ctx.lineWidth   = 2;
      ctx.beginPath();
      ctx.roundRect(-s * 0.5, -s * 0.6, s, s * 1.2, 4);
      ctx.fill(); ctx.stroke();
      // เส้นบนกระป๋อง
      ctx.strokeStyle = '#aaa';
      ctx.lineWidth   = 1.5;
      ctx.beginPath();
      ctx.moveTo(-s * 0.4, -s * 0.6);
      ctx.lineTo(-s * 0.4, s * 0.6);
      ctx.stroke();
    } else {
      // ลำตัวปลา
      ctx.fillStyle = data.color;
      ctx.beginPath();
      ctx.ellipse(0, 0, s, s * 0.52, 0, 0, Math.PI * 2);
      ctx.fill();

      // หาง
      ctx.beginPath();
      ctx.moveTo(-s * 0.9, 0);
      ctx.lineTo(-s * 1.4,  s * 0.45);
      ctx.lineTo(-s * 1.4, -s * 0.45);
      ctx.closePath();
      ctx.fill();

      // ครีบบน
      ctx.beginPath();
      ctx.moveTo(-s * 0.1, -s * 0.52);
      ctx.quadraticCurveTo(s * 0.3, -s * 0.9, s * 0.5, -s * 0.52);
      ctx.fill();

      // ตาขาว
      ctx.fillStyle = '#fff';
      ctx.beginPath();
      ctx.arc(s * 0.5, -s * 0.1, s * 0.2, 0, Math.PI * 2);
      ctx.fill();
      // ตาดำ
      ctx.fillStyle = '#111';
      ctx.beginPath();
      ctx.arc(s * 0.55, -s * 0.1, s * 0.1, 0, Math.PI * 2);
      ctx.fill();

      // ขอบเล็กน้อย
      ctx.strokeStyle = 'rgba(0,0,0,0.2)';
      ctx.lineWidth   = 1;
      ctx.beginPath();
      ctx.ellipse(0, 0, s, s * 0.52, 0, 0, Math.PI * 2);
      ctx.stroke();
    }

    ctx.restore();
  }

  overlapsHook(hx, hy) {
    const dx = this.x - hx, dy = this.y - hy;
    return Math.sqrt(dx * dx + dy * dy) < this.data.size + 10;
  }

  getCaught() {
    if (this.caught) return;
    this.caught = true;
    this.dead   = true;
    ScoreManager.add(this.data.score);
    if (!this.data.isJunk) HungerSystem.restore(this.data.hunger);
    popups.push(new ScorePopup(this.x, this.y, this.data.score));
  }
}

// ── Fish Spawner ─────────────────────────────────────────────────────────────

const FishSpawner = {
  fishes:   [],
  timer:    0,
  interval: 1.6,

  reset() { this.fishes = []; this.timer = 0; },

  update(dt) {
    if (GameManager.state !== State.PLAYING) return;
    this.timer += dt;
    if (this.timer >= this.interval) { this.timer = 0; this.spawn(); }
    this.fishes.forEach(f => f.update(dt));
    this.fishes = this.fishes.filter(f => !f.dead);
  },

  spawn() {
    const data = FISH_TYPES[Math.floor(Math.random() * FISH_TYPES.length)];
    const dir  = Math.random() > 0.5 ? 1 : -1;
    const x    = dir > 0 ? -80 : W + 80;
    const y    = WATER_Y + 35 + Math.random() * (MAX_DEPTH - WATER_Y - 55);
    this.fishes.push(new Fish(data, x, y, dir));
  },

  draw() { this.fishes.forEach(f => f.draw()); },

  checkHook(hx, hy) {
    for (const f of this.fishes)
      if (!f.caught && f.overlapsHook(hx, hy)) { f.getCaught(); return; }
  },
};

// ── Fishing Controller ───────────────────────────────────────────────────────

const HookSt = { IDLE: 0, DESCENDING: 1, WAITING: 2, ASCENDING: 3 };

const FishingController = {
  rodX: 108, rodY: WATER_Y - 8,   // จุดปลาย rod
  hookX: 108, hookY: WATER_Y - 8,

  st:           HookSt.IDLE,
  waitTimer:    0,
  waitDuration: 0.45,
  descentSpeed: 200,
  ascentSpeed:  320,

  // ฟองอากาศ
  bubbles: [],

  reset() {
    this.hookX    = this.rodX;
    this.hookY    = this.rodY;
    this.st       = HookSt.IDLE;
    this.bubbles  = [];
  },

  get active() {
    return this.st === HookSt.DESCENDING || this.st === HookSt.WAITING;
  },

  update(dt, mouseHeld) {
    if (GameManager.state !== State.PLAYING) return;

    switch (this.st) {
      case HookSt.IDLE:
        if (mouseHeld) this.st = HookSt.DESCENDING;
        break;

      case HookSt.DESCENDING:
        this.hookY += this.descentSpeed * dt;
        if (this.hookY >= MAX_DEPTH)   this.hookY = MAX_DEPTH;
        if (!mouseHeld || this.hookY >= MAX_DEPTH) {
          this.st        = HookSt.WAITING;
          this.waitTimer = this.waitDuration;
        }
        // สร้างฟอง
        if (Math.random() < 0.4)
          this.bubbles.push({ x: this.hookX + (Math.random()-0.5)*12, y: this.hookY - 5, life: 0.6 + Math.random()*0.4 });
        break;

      case HookSt.WAITING:
        this.waitTimer -= dt;
        if (this.waitTimer <= 0) this.st = HookSt.ASCENDING;
        break;

      case HookSt.ASCENDING:
        this.hookY -= this.ascentSpeed * dt;
        if (this.hookY <= this.rodY) { this.hookY = this.rodY; this.st = HookSt.IDLE; }
        break;
    }

    // อัปเดตฟอง
    this.bubbles.forEach(b => { b.y -= 60 * dt; b.life -= dt; });
    this.bubbles = this.bubbles.filter(b => b.life > 0);

    if (this.active) FishSpawner.checkHook(this.hookX, this.hookY);
  },

  draw() {
    // สายเบ็ด
    ctx.strokeStyle = 'rgba(255,255,255,0.85)';
    ctx.lineWidth   = 1.5;
    ctx.setLineDash([]);
    ctx.beginPath();
    ctx.moveTo(this.rodX, this.rodY);
    // เส้นโค้งเล็กน้อยตามแรงโน้มถ่วง
    const mx = this.hookX + 6, my = (this.rodY + this.hookY) / 2 + 8;
    ctx.quadraticCurveTo(mx, my, this.hookX, this.hookY);
    ctx.stroke();

    // hook
    ctx.strokeStyle = '#ddd';
    ctx.lineWidth   = 2.5;
    ctx.beginPath();
    ctx.arc(this.hookX, this.hookY + 7, 7, Math.PI * 0.05, Math.PI * 0.95);
    ctx.stroke();
    // เส้นตรงบน hook
    ctx.beginPath();
    ctx.moveTo(this.hookX, this.hookY);
    ctx.lineTo(this.hookX, this.hookY + 7);
    ctx.stroke();

    // ฟองอากาศ
    ctx.fillStyle = 'rgba(200,230,255,0.5)';
    for (const b of this.bubbles) {
      ctx.globalAlpha = b.life;
      ctx.beginPath();
      ctx.arc(b.x, b.y, 2.5, 0, Math.PI * 2);
      ctx.fill();
    }
    ctx.globalAlpha = 1;
  },
};

// ── Score Popup ──────────────────────────────────────────────────────────────

class ScorePopup {
  constructor(x, y, score) {
    this.x = x; this.y = y; this.score = score;
    this.elapsed = 0; this.duration = 0.85; this.dead = false;
  }
  update(dt) {
    this.elapsed += dt;
    this.y       -= 55 * dt;
    if (this.elapsed >= this.duration) this.dead = true;
  }
  draw() {
    const a    = 1 - this.elapsed / this.duration;
    const text = this.score >= 0 ? `+${this.score}` : `${this.score}`;
    ctx.save();
    ctx.globalAlpha = a;
    ctx.font        = 'bold 22px Arial';
    ctx.textAlign   = 'center';
    ctx.strokeStyle = '#000';
    ctx.lineWidth   = 4;
    ctx.fillStyle   = this.score >= 0 ? '#FFE033' : '#FF5050';
    ctx.strokeText(text, this.x, this.y);
    ctx.fillText  (text, this.x, this.y);
    ctx.restore();
  }
}

const popups = [];

// ── Background Renderer ──────────────────────────────────────────────────────

let waveOffset = 0;

function drawBackground() {
  // ท้องฟ้า gradient
  const sky = ctx.createLinearGradient(0, 0, 0, WATER_Y);
  sky.addColorStop(0, '#5BB8E8');
  sky.addColorStop(1, '#A8D8F0');
  ctx.fillStyle = sky;
  ctx.fillRect(0, 0, W, WATER_Y);

  // เมฆ
  drawCloud(160, 38, 1.0);
  drawCloud(400, 52, 1.3);
  drawCloud(640, 32, 0.9);

  // น้ำ gradient
  const water = ctx.createLinearGradient(0, WATER_Y, 0, H);
  water.addColorStop(0,   '#1880AA');
  water.addColorStop(0.4, '#125E80');
  water.addColorStop(1,   '#082E45');
  ctx.fillStyle = water;
  ctx.fillRect(0, WATER_Y, W, H - WATER_Y);

  // ลายน้ำใต้ทะเล (แสงสะท้อน)
  ctx.strokeStyle = 'rgba(255,255,255,0.07)';
  ctx.lineWidth   = 18;
  for (let i = 0; i < 5; i++) {
    const lx = ((i * 180 + waveOffset * 0.3) % (W + 100)) - 50;
    ctx.beginPath();
    ctx.moveTo(lx, WATER_Y);
    ctx.lineTo(lx - 40, H);
    ctx.stroke();
  }

  // เส้นผิวน้ำ
  waveOffset += 0.4;
  ctx.strokeStyle = 'rgba(255,255,255,0.35)';
  ctx.lineWidth   = 2.5;
  ctx.beginPath();
  for (let x = 0; x <= W; x += 4) {
    const y = WATER_Y + Math.sin((x + waveOffset) * 0.045) * 3;
    x === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
  }
  ctx.stroke();

  // foam ที่ผิวน้ำ
  ctx.fillStyle = 'rgba(255,255,255,0.18)';
  ctx.fillRect(0, WATER_Y - 1, W, 5);
}

function drawCloud(cx, cy, scale) {
  ctx.save();
  ctx.translate(cx, cy);
  ctx.scale(scale, scale);
  ctx.fillStyle = 'rgba(255,255,255,0.78)';
  const puffs = [[-30,0,22],[-10,-10,26],[20,-6,22],[45,0,18]];
  for (const [x, y, r] of puffs) {
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.restore();
}

// ── Player (Placeholder) ─────────────────────────────────────────────────────

function drawPlayer() {
  const px = 68, py = WATER_Y + 4;

  // ปลา mount (สีทอง)
  ctx.fillStyle = '#FFD700';
  ctx.beginPath();
  ctx.ellipse(px, py + 8, 32, 12, 0, 0, Math.PI * 2);
  ctx.fill();
  // หางปลา mount
  ctx.beginPath();
  ctx.moveTo(px - 30, py + 8);
  ctx.lineTo(px - 46, py + 2);
  ctx.lineTo(px - 46, py + 14);
  ctx.closePath();
  ctx.fill();
  // ตาปลา mount
  ctx.fillStyle = '#333';
  ctx.beginPath();
  ctx.arc(px + 22, py + 5, 3, 0, Math.PI * 2);
  ctx.fill();

  // ตัวละคร (สีแดง)
  ctx.fillStyle = '#DD3333';
  // ลำตัว
  ctx.beginPath();
  ctx.ellipse(px + 6, py - 7, 10, 12, 0, 0, Math.PI * 2);
  ctx.fill();
  // หัว
  ctx.fillStyle = '#FDBCB4';
  ctx.beginPath();
  ctx.arc(px + 6, py - 22, 9, 0, Math.PI * 2);
  ctx.fill();
  // ผม (placeholder สีแดงเข้ม)
  ctx.fillStyle = '#AA1111';
  ctx.beginPath();
  ctx.arc(px + 6, py - 27, 7, Math.PI, Math.PI * 2);
  ctx.fill();

  // คันเบ็ด
  ctx.strokeStyle = '#7B4A1A';
  ctx.lineWidth   = 3;
  ctx.lineCap     = 'round';
  ctx.beginPath();
  ctx.moveTo(px + 8, py - 28);
  ctx.quadraticCurveTo(px + 30, py - 25, FishingController.rodX, FishingController.rodY);
  ctx.stroke();
  ctx.lineCap = 'butt';
}

// ── UI ───────────────────────────────────────────────────────────────────────

function drawUI() {
  ctx.textAlign = 'left';

  // Score
  ctx.font        = 'bold 28px Arial';
  ctx.strokeStyle = '#000';
  ctx.lineWidth   = 5;
  ctx.fillStyle   = '#FFE033';
  const scoreStr = ScoreManager.current.toLocaleString();
  ctx.strokeText(scoreStr, 14, 36);
  ctx.fillText  (scoreStr, 14, 36);

  // Timer
  const sec = Math.ceil(GameManager.timeRemaining);
  ctx.textAlign   = 'right';
  ctx.fillStyle   = sec <= 10 ? '#FF5050' : '#fff';
  ctx.strokeStyle = '#000';
  ctx.strokeText(`${sec}s`, W - 14, 36);
  ctx.fillText  (`${sec}s`, W - 14, 36);

  // Hunger bar
  const bw = 220, bh = 18, bx = W / 2 - bw / 2, by = 10;
  // label
  ctx.font      = 'bold 12px Arial';
  ctx.textAlign = 'center';
  ctx.fillStyle = '#fff';
  ctx.strokeStyle = '#000';
  ctx.lineWidth   = 3;
  ctx.strokeText('HUNGER', W / 2, by - 1);
  ctx.fillText  ('HUNGER', W / 2, by - 1);
  // bg
  ctx.fillStyle = 'rgba(0,0,0,0.55)';
  ctx.beginPath();
  ctx.roundRect(bx - 2, by + 2, bw + 4, bh + 4, 5);
  ctx.fill();
  // fill
  const pct   = HungerSystem.percent;
  const bColor = pct > 0.5 ? '#44DD44' : pct > 0.25 ? '#FFAA22' : '#FF3333';
  ctx.fillStyle = bColor;
  ctx.beginPath();
  ctx.roundRect(bx, by + 4, bw * pct, bh, 4);
  ctx.fill();

  // hint
  if (GameManager.state === State.PLAYING && FishingController.st === HookSt.IDLE) {
    ctx.font      = '13px Arial';
    ctx.fillStyle = 'rgba(255,255,255,0.55)';
    ctx.textAlign = 'center';
    ctx.fillText('กดค้างเพื่อหย่อนสาย  |  ปล่อยเพื่อหยุด', W / 2, H - 12);
  }
}

// ── Overlay Screens ───────────────────────────────────────────────────────────

function drawMenu() {
  ctx.fillStyle = 'rgba(0,0,0,0.42)';
  ctx.fillRect(0, 0, W, H);

  ctx.textAlign   = 'center';
  ctx.strokeStyle = '#000';
  ctx.lineWidth   = 6;

  ctx.font      = 'bold 46px Arial';
  ctx.fillStyle = '#FFE033';
  ctx.strokeText('🎣 Tosdo Fishing Star', W / 2, H / 2 - 25);
  ctx.fillText  ('🎣 Tosdo Fishing Star', W / 2, H / 2 - 25);

  ctx.font      = '20px Arial';
  ctx.fillStyle = '#fff';
  ctx.lineWidth = 3;
  ctx.strokeText('คลิกเพื่อเริ่มเกม', W / 2, H / 2 + 22);
  ctx.fillText  ('คลิกเพื่อเริ่มเกม', W / 2, H / 2 + 22);

  if (ScoreManager.best > 0) {
    ctx.font      = '16px Arial';
    ctx.fillStyle = '#aaa';
    ctx.fillText(`Best: ${ScoreManager.best.toLocaleString()}`, W / 2, H / 2 + 55);
  }
}

// พื้นที่ปุ่ม Play Again
const BTN = { x: W / 2 - 95, y: H / 2 + 58, w: 190, h: 46 };

function drawGameOver() {
  ctx.fillStyle = 'rgba(0,0,0,0.65)';
  ctx.fillRect(0, 0, W, H);

  ctx.textAlign   = 'center';
  ctx.strokeStyle = '#000';
  ctx.lineWidth   = 6;

  ctx.font      = 'bold 54px Arial';
  ctx.fillStyle = '#FF5050';
  ctx.strokeText('GAME OVER', W / 2, H / 2 - 50);
  ctx.fillText  ('GAME OVER', W / 2, H / 2 - 50);

  ctx.font      = 'bold 28px Arial';
  ctx.fillStyle = '#FFE033';
  ctx.lineWidth = 4;
  ctx.strokeText(`Score: ${ScoreManager.current.toLocaleString()}`, W / 2, H / 2 + 2);
  ctx.fillText  (`Score: ${ScoreManager.current.toLocaleString()}`, W / 2, H / 2 + 2);

  ctx.font      = '20px Arial';
  ctx.fillStyle = '#aaa';
  ctx.lineWidth = 3;
  ctx.strokeText(`Best:  ${ScoreManager.best.toLocaleString()}`, W / 2, H / 2 + 36);
  ctx.fillText  (`Best:  ${ScoreManager.best.toLocaleString()}`, W / 2, H / 2 + 36);

  // ปุ่มเล่นอีกครั้ง
  ctx.fillStyle   = '#FFE033';
  ctx.strokeStyle = '#AA8800';
  ctx.lineWidth   = 3;
  ctx.beginPath();
  ctx.roundRect(BTN.x, BTN.y, BTN.w, BTN.h, 10);
  ctx.fill(); ctx.stroke();

  ctx.font        = 'bold 20px Arial';
  ctx.fillStyle   = '#333';
  ctx.strokeStyle = 'transparent';
  ctx.fillText('เล่นอีกครั้ง', W / 2, BTN.y + 30);
}

// ── Input ────────────────────────────────────────────────────────────────────

let mouseHeld = false;

function getCanvasPos(e) {
  const r = canvas.getBoundingClientRect();
  return {
    x: (e.clientX - r.left) * (W / r.width),
    y: (e.clientY - r.top)  * (H / r.height),
  };
}

canvas.addEventListener('mousedown', e => {
  const { x, y } = getCanvasPos(e);
  mouseHeld = true;
  if (GameManager.state === State.MENU)     { GameManager.start(); return; }
  if (GameManager.state === State.GAMEOVER) {
    if (x > BTN.x && x < BTN.x + BTN.w && y > BTN.y && y < BTN.y + BTN.h)
      GameManager.start();
  }
});

canvas.addEventListener('mouseup',    () => mouseHeld = false);
canvas.addEventListener('mouseleave', () => mouseHeld = false);

canvas.addEventListener('touchstart', e => {
  e.preventDefault();
  mouseHeld = true;
  if (GameManager.state === State.MENU) GameManager.start();
}, { passive: false });
canvas.addEventListener('touchend', e => { e.preventDefault(); mouseHeld = false; }, { passive: false });

// ── Game Loop ────────────────────────────────────────────────────────────────

let lastTime = 0;

function loop(ts) {
  const dt = Math.min((ts - lastTime) / 1000, 0.05);
  lastTime  = ts;

  // Update
  GameManager.update(dt);
  HungerSystem.update(dt);
  FishingController.update(dt, mouseHeld);
  FishSpawner.update(dt);
  popups.forEach(p => p.update(dt));
  for (let i = popups.length - 1; i >= 0; i--)
    if (popups[i].dead) popups.splice(i, 1);

  // Draw
  drawBackground();
  FishSpawner.draw();
  FishingController.draw();
  drawPlayer();
  popups.forEach(p => p.draw());
  drawUI();

  if (GameManager.state === State.MENU)     drawMenu();
  if (GameManager.state === State.GAMEOVER) drawGameOver();

  requestAnimationFrame(loop);
}

requestAnimationFrame(loop);
