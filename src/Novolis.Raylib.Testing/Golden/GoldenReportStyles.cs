namespace Novolis.Raylib.Testing.Golden;

/// <summary>Shared CSS for golden QA HTML reports (aligned with TUnit in-process test report styling).</summary>
internal static class GoldenReportStyles
{
    internal const string Css = """
        :root{--bg:#0b0d11;--surface-0:#12151c;--surface-1:#181c25;--surface-2:#1f2430;--surface-3:#282e3a;--border:rgba(255,255,255,.06);--border-h:rgba(255,255,255,.10);--text:#e2e4e9;--text-2:#9ba1b0;--text-3:#5f6678;--emerald:#34d399;--emerald-d:rgba(52,211,153,.12);--rose:#fb7185;--rose-d:rgba(251,113,133,.12);--amber:#fbbf24;--amber-d:rgba(251,191,36,.10);--slate:#94a3b8;--slate-d:rgba(148,163,184,.10);--indigo:#818cf8;--indigo-d:rgba(129,140,248,.10);--violet:#a78bfa;--font:'Segoe UI Variable','Segoe UI',-apple-system,BlinkMacSystemFont,system-ui,sans-serif;--mono:'Cascadia Code','JetBrains Mono','Fira Code','SF Mono',ui-monospace,monospace;--r:8px;--r-lg:14px;--ease:cubic-bezier(.4,0,.2,1)}
        :root[data-theme="light"]{--bg:#f8f9fb;--surface-0:#fff;--surface-1:#f0f1f4;--surface-2:#e4e6eb;--surface-3:#d1d5de;--border:rgba(0,0,0,.08);--border-h:rgba(0,0,0,.14);--text:#1a1d24;--text-2:#5a5f6e;--text-3:#8b91a0;--emerald-d:rgba(52,211,153,.15);--rose-d:rgba(251,113,133,.15);--amber-d:rgba(251,191,36,.12);--slate-d:rgba(148,163,184,.12);--indigo-d:rgba(129,140,248,.12)}
        *,*::before,*::after{box-sizing:border-box;margin:0;padding:0}
        html{-webkit-font-smoothing:antialiased;-moz-osx-font-smoothing:grayscale}
        body{font-family:var(--font);background:var(--bg);color:var(--text);line-height:1.55;font-size:14px;min-height:100vh;overflow-x:hidden}
        .grain{position:fixed;inset:0;pointer-events:none;z-index:9999;opacity:.018;background:url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E")}
        :root[data-theme="light"] .grain{opacity:.008}
        .shell{max-width:1200px;margin:0 auto;padding:28px 24px 64px}
        .hdr{display:flex;align-items:center;flex-wrap:wrap;gap:10px 14px;margin-bottom:16px}
        .hdr-brand{display:flex;align-items:center;gap:12px}
        .hdr-logo{width:38px;height:38px;flex-shrink:0}
        .hdr-name{font-size:1.35rem;font-weight:700;letter-spacing:-.02em;background:linear-gradient(135deg,#e2e4e9 30%,#818cf8);-webkit-background-clip:text;-webkit-text-fill-color:transparent;background-clip:text}
        :root[data-theme="light"] .hdr-name{background:linear-gradient(135deg,#1a1d24 30%,#6366f1);-webkit-background-clip:text;background-clip:text}
        .hdr-sub{font-size:.78rem;color:var(--text-3);letter-spacing:.06em;text-transform:uppercase}
        .hdr-meta{display:flex;gap:6px;flex-wrap:wrap;align-items:center;margin-left:auto}
        .chip{display:inline-flex;align-items:center;gap:6px;padding:5px 12px;border-radius:100px;background:var(--surface-1);border:1px solid var(--border);font-size:.78rem;color:var(--text-2);white-space:nowrap}
        .chip code{font-family:var(--mono);font-size:.72rem;color:var(--text)}
        .theme-btn{display:flex;align-items:center;justify-content:center;width:36px;height:36px;border-radius:100px;background:var(--surface-1);border:1px solid var(--border);color:var(--text-2);cursor:pointer;flex-shrink:0;transition:border-color .2s var(--ease),color .2s var(--ease)}
        .theme-btn:hover{border-color:var(--border-h);color:var(--text)}
        .theme-btn svg{width:18px;height:18px}
        [data-theme="dark"] .theme-sun{display:none}
        [data-theme="light"] .theme-moon{display:none}
        .dash{display:flex;align-items:center;gap:28px;flex-wrap:wrap;padding:24px;margin-bottom:20px;background:var(--surface-0);border:1px solid var(--border);border-radius:var(--r-lg);position:relative;overflow:hidden}
        .dash::before{content:'';position:absolute;inset:0;background:radial-gradient(ellipse 60% 50% at 20% 50%,rgba(99,102,241,.04),transparent),radial-gradient(ellipse 40% 60% at 80% 30%,rgba(52,211,153,.03),transparent);pointer-events:none}
        .ring-wrap{position:relative;width:110px;height:110px;flex-shrink:0}
        .ring{width:100%;height:100%}
        .ring-center{position:absolute;inset:0;display:flex;flex-direction:column;align-items:center;justify-content:center}
        .ring-pct{font-size:1.45rem;font-weight:800;letter-spacing:-.03em;line-height:1}
        .ring-pct small{font-size:.55em;font-weight:600;opacity:.6}
        .ring-lbl{font-size:.65rem;color:var(--text-3);margin-top:2px;letter-spacing:.04em;text-transform:uppercase}
        .stats{display:flex;gap:10px;flex-wrap:wrap;flex:1}
        .stat{position:relative;flex:1;min-width:80px;padding:14px 12px;border-radius:var(--r);background:var(--surface-1);border:1px solid var(--border);text-align:center;overflow:hidden}
        .stat::after{content:'';position:absolute;top:0;left:0;right:0;height:2px;background:var(--accent,var(--indigo));border-radius:2px 2px 0 0;opacity:.7}
        .stat-n{display:block;font-size:1.5rem;font-weight:800;letter-spacing:-.03em;line-height:1.1;font-variant-numeric:tabular-nums}
        .stat-l{display:block;font-size:.7rem;color:var(--text-3);margin-top:4px;text-transform:uppercase;letter-spacing:.06em}
        .stat.passed .stat-n{color:var(--emerald)}
        .stat.failed .stat-n{color:var(--rose)}
        .stat.skipped .stat-n{color:var(--amber)}
        .story-meta{margin-bottom:20px;padding:12px 16px;background:var(--surface-0);border:1px solid var(--border);border-radius:var(--r);font-size:.85rem;color:var(--text-2)}
        .story-meta strong{color:var(--text)}
        .story-meta code{font-family:var(--mono);font-size:.78rem;color:var(--text-2)}
        .frames{display:flex;flex-direction:column;gap:8px}
        .grp{background:var(--surface-0);border:1px solid var(--border);border-radius:var(--r);overflow:hidden}
        .grp-hd{display:flex;align-items:center;gap:12px;padding:10px 16px;border-bottom:1px solid var(--border);background:var(--surface-1)}
        .grp-indicator{width:4px;height:18px;border-radius:2px;flex-shrink:0;background:var(--emerald);opacity:.7}
        .grp.fail .grp-indicator{background:var(--rose)}
        .grp.skip .grp-indicator{background:var(--amber)}
        .grp-name{font-weight:600;font-size:.95rem;flex:1;word-break:break-word}
        .grp-caption{font-size:.82rem;color:var(--text-3);margin-top:2px;font-weight:400}
        .t-badge{font-size:.7rem;font-weight:700;padding:3px 9px;border-radius:100px;text-transform:uppercase;letter-spacing:.04em;white-space:nowrap;line-height:1}
        .t-badge.passed{background:var(--emerald-d);color:var(--emerald)}
        .t-badge.failed{background:var(--rose-d);color:var(--rose)}
        .t-badge.skipped{background:var(--amber-d);color:var(--amber)}
        .t-badge.captured{background:var(--indigo-d);color:var(--indigo)}
        .grp-body-pad{padding:16px 18px}
        .d-sec{margin-bottom:14px}
        .d-sec:last-child{margin-bottom:0}
        .d-lbl{font-size:.68rem;font-weight:700;text-transform:uppercase;color:var(--text-3);margin-bottom:8px;letter-spacing:.07em}
        .golden-img{max-width:100%;width:min(960px,100%);height:auto;border:1px solid var(--border);border-radius:var(--r);display:block;margin-bottom:8px;background:var(--surface-2)}
        .golden-missing{padding:40px 24px;text-align:center;color:var(--text-3);background:var(--surface-1);border:1px dashed var(--border-h);border-radius:var(--r);font-size:.88rem}
        .d-info{display:flex;gap:16px;flex-wrap:wrap;padding:10px 14px;border-radius:var(--r);background:var(--surface-1);border:1px solid var(--border);font-size:.8rem;color:var(--text-2);margin-top:8px}
        .d-info code{font-family:var(--mono);font-size:.72rem;word-break:break-all}
        .d-checklist{margin:0;padding-left:1.25rem;color:var(--text-2);font-size:.88rem}
        .d-checklist li{margin-bottom:4px}
        .d-msg{font-size:.85rem;color:var(--text-2);margin-top:6px;padding:8px 12px;border-radius:var(--r);background:var(--surface-1);border-left:2px solid var(--amber)}
        .grp.fail .d-msg{border-left-color:var(--rose)}
        .report-footer{margin-top:32px;padding-top:16px;border-top:1px solid var(--border);font-size:.82rem;color:var(--text-3)}
        .report-footer code{font-family:var(--mono);font-size:.78rem;color:var(--text-2)}
        """;
}
