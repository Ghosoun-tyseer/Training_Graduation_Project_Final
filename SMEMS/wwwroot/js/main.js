// ═══════════════════════════════════════════════════════════════════
//  SMEMS – main.js  (ASP.NET Core version)
// ═══════════════════════════════════════════════════════════════════

// ── Live clock ─────────────────────────────────────────────────────
(function startClock() {
    function tick() {
        var el = document.getElementById('live-clock');
        if (!el) return;
        var now = new Date();
        el.textContent = now.toLocaleDateString('en-US', {
            weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
        });
    }
    tick();
    setInterval(tick, 60000);
})();

// ── Auto-dismiss toast (TempData success/error) ────────────────────
(function initAutoToast() {
    var t = document.getElementById('auto-toast');
    if (!t) return;
    t.style.opacity = '1';
    t.style.transform = 'translateY(0)';
    setTimeout(function () {
        t.style.opacity = '0';
        t.style.transform = 'translateY(-10px)';
        setTimeout(function () { t.remove(); }, 400);
    }, 3500);
})();

// ── showToast (called from inline Razor pages) ────────────────────
function showToast(msg, type) {
    var t = document.createElement('div');
    t.className = 'toast toast-' + (type || 'info');
    t.textContent = msg;
    document.body.appendChild(t);
    requestAnimationFrame(function () {
        t.style.opacity = '1';
        t.style.transform = 'translateY(0)';
    });
    setTimeout(function () {
        t.style.opacity = '0';
        t.style.transform = 'translateY(-10px)';
        setTimeout(function () { t.remove(); }, 400);
    }, 3500);
}

// ── Client-side table filter (search input) ───────────────────────
function filterTable(inputId, tableId) {
    var q  = document.getElementById(inputId).value.toLowerCase();
    var tbl = document.getElementById(tableId);
    if (!tbl) return;
    Array.from(tbl.querySelectorAll('tbody tr')).forEach(function (row) {
        row.style.display = row.textContent.toLowerCase().includes(q) ? '' : 'none';
    });
}

// ── Filter table by a select column index ─────────────────────────
function filterBySelect(selectId, tableId, colIndex) {
    var val  = document.getElementById(selectId).value.toLowerCase();
    var tbl  = document.getElementById(tableId);
    if (!tbl) return;
    Array.from(tbl.querySelectorAll('tbody tr')).forEach(function (row) {
        var cell = row.cells[colIndex];
        if (!val || (cell && cell.textContent.toLowerCase().includes(val)))
            row.style.display = '';
        else
            row.style.display = 'none';
    });
}
