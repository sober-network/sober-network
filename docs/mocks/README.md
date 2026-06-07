# UI Mocks

Self-contained HTML files that serve as the **design spec** for the Angular implementation.
Open any file directly in a browser — no build step required.

| File | Purpose |
|------|---------|
| `sn-interactive-mock.html` | **Primary reference.** Navigable mock: guest/member nav states, Community mega-menu, group hub with all tabs, breadcrumbs, and all major pages. Click around to feel the UX. |
| `sn-nav-mock.html` | Site hierarchy tree · all three nav states · breadcrumb pattern demos |
| `sn-pages-mock.html` | Guest landing page mock · Member community dashboard mock |

## Workflow convention

1. **Mock is updated first** when a design decision changes.
2. **Components implement to match** the mock — "refer to the mock" is a valid instruction.
3. **Changes are surgical.** The mock defines appearance and UX behaviour — not implementation. Existing Angular bindings, form controls, service calls, validators, and business logic are always preserved. Never rewrite working code just to match mock styling.
4. **When a mock is ambiguous or silent on a detail — ask before implementing.** Don't interpolate.
5. When implementing, translate mock CSS class names and token names:
   - Mock tokens use unprefixed names (`--bg`, `--cta-bg`)
   - Angular `styles.scss` uses `--sn-*` prefix (`--sn-bg`, `--sn-cta-bg`)
   - Always use the prefixed form with a literal fallback in components: `var(--sn-cta-bg, #1a1a18)`

## Rebasing the mock

As the codebase evolves, the mock will drift from the live components. Periodically run a **"rebase the mock"** pass to bring `sn-interactive-mock.html` back into sync with the actual code.

**When to rebase:**
- After completing a significant feature or UI change
- Before starting a new feature sprint (so the mock is a reliable spec baseline)
- Any time you notice the mock no longer matches what the app renders

**How to rebase:**
Launch a `general-purpose` agent (recommend `claude-opus-4.5`) with the following instruction:

> Read every live Angular component in the codebase (navbar, landing, groups-dashboard, group-hero, group-hub and all its tabs, base-form-modal, and any others added since the last rebase). Read `styles.scss` for design tokens. Translate `--sn-*` tokens to unprefixed equivalents. Update `docs/mocks/sn-interactive-mock.html` in place to match the current components pixel-accurately. Preserve the JS routing, nav state switching, mega-menu, tab switching, and breadcrumb logic. Keep `pill-cs Soon` stubs for unimplemented features. If anything is ambiguous, list ambiguities and stop — do not guess.

**Important:** The mock is a spec, not the source of truth for code. If the mock and the live components disagree, **the live components win** — they are the working implementation. Rebase updates the mock to match the code, never the other way around (unless you are intentionally redesigning).

## Component annotations

The interactive mock marks component boundaries as HTML comments:
```html
<!-- ════ <app-navbar>      ════ -->
<!-- ════ <app-hero>        ════ -->
<!-- ════ <app-breadcrumbs> ════ -->  ← projected via ng-content
<!-- ════ <router-outlet>   ════ -->
```
These map directly to the Angular component tree.
