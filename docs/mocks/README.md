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
3. When implementing, translate mock CSS class names and token names:
   - Mock tokens use unprefixed names (`--bg`, `--cta-bg`)
   - Angular `styles.scss` uses `--sn-*` prefix (`--sn-bg`, `--sn-cta-bg`)
   - Always use the prefixed form with a literal fallback in components: `var(--sn-cta-bg, #1a1a18)`

## Component annotations

The interactive mock marks component boundaries as HTML comments:
```html
<!-- ════ <app-navbar>      ════ -->
<!-- ════ <app-hero>        ════ -->
<!-- ════ <app-breadcrumbs> ════ -->  ← projected via ng-content
<!-- ════ <router-outlet>   ════ -->
```
These map directly to the Angular component tree.
