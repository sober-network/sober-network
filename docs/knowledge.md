# Sober Network — Project Knowledge Document

> Living document. Topics marked 🐾 are "dog-ears" — open questions to be discussed and resolved.

---

## Vision

**Sober Network** is a multi-tenant web platform for Alcoholics Anonymous home groups. Each home group (e.g., *Early Bird Zoom*) gets its own space on the network. Groups self-govern their space while sharing a common infrastructure. New groups can adopt the platform voluntarily.

The platform must honor the **12 Traditions of A.A.** at every design decision.

---

## Core Principles

### Security First
- All users must be authenticated before accessing member content
- Role-Based Access Control (RBAC) governs what each user can see and do
- Public-facing content is strictly limited (meeting times, contact form, no member data)
- Phone lists and member information are never exposed publicly

### AA 12 Traditions — Full Text & Design Implications

These are non-negotiable constraints. Every feature, endpoint, data model, and UX decision must be evaluated against them.

---

**Tradition 1** — *"Our common welfare should come first; personal recovery depends upon A.A. unity."*
- No feature may be designed to serve one member at the expense of the group
- Platform governance must support group unity, not individual prestige or control
- Disputes between members must be resolvable without destroying the group's digital space
- `superadmin` power is a stewardship role, not ownership

**Tradition 2** — *"For our group purpose there is but one ultimate authority — a loving God as He may express Himself in our group conscience. Our leaders are but trusted servants; they do not govern."*
- `group_admin` is a **trusted servant**, not a ruler — the role has limits
- Platform must support group conscience decisions (polls, announcements, voting features)
- No single admin account should be an unrecoverable single point of failure
- Succession planning is a platform feature, not an afterthought

**Tradition 3** — *"The only requirement for A.A. membership is a desire to stop drinking."*
- No demographic, financial, or personal data may be required beyond what's needed for auth
- Sobriety date is **opt-in only** — never required
- Phone number, last name, location — all optional, never exposed publicly
- The join request process must not be a gatekeeping mechanism beyond basic identity

**Tradition 4** — *"Each group should be autonomous except in matters affecting other groups or A.A. as a whole."*
- Groups configure their own content rules, roles, and moderation policies independently
- Platform cannot force one group's settings onto another
- `superadmin` can only intervene if a group's actions affect the platform or other groups
- Multi-group membership must not leak one group's data to another

**Tradition 5** — *"Each group has but one primary purpose — to carry its message to the alcoholic who still suffers."*
- Features must serve recovery, not distract from it
- No advertising, no gamification, no engagement-bait mechanics
- The meeting schedule and newcomer contact features are highest-priority public-facing content
- Every feature should pass the test: "Does this help carry the message?"

**Tradition 6** — *"An A.A. group ought never endorse, finance, or lend the A.A. name to any related facility or outside enterprise, lest problems of money, property, and prestige divert us from our primary purpose."*
- Platform is named **Sober Network**, never "AA [anything]"
- No AA logos, trademarks, or official branding may be used
- No partnerships, sponsors, or affiliate links — ever
- The platform serves AA groups but is not AA itself

**Tradition 7** — *"Every A.A. group ought to be fully self-supporting, declining outside contributions."*
- Each group's costs should be transparent and covered by that group's own voluntary contributions
- No outside donations, VC funding, or sponsorships
- Platform cost dashboard is a planned feature: groups can see their share of running costs
- Payment processing (if ever added) must be voluntary and group-managed, not platform-mandated

**Tradition 8** — *"Alcoholics Anonymous should remain forever non-professional, but our service centers may employ special workers."*
- Platform developers are **volunteers or trusted servants** — not a commercial service provider
- If paid infrastructure is ever used (Fly.io, etc.), it is a service worker cost, not a professional AA service
- No professional counseling, therapy, or clinical features may be offered through the platform

**Tradition 9** — *"A.A., as such, ought never be organized; but we may create service boards or committees directly responsible to those they serve."*
- `superadmin` and platform governance exist to serve groups, not to control them
- Platform structure is flat — no hierarchy between groups
- Groups may organize their own internal structure (officers, committees) through custom roles
- Platform governance docs (how decisions are made) must be public and in the repo

**Tradition 10** — *"Alcoholics Anonymous has no opinion on outside issues; hence the A.A. name ought never be drawn into public controversy."*
- No political content, social cause endorsements, or outside controversy features
- Discussion boards must be scoped to recovery — no off-topic general forums
- Moderation must remove content that draws the group into outside controversy
- Platform itself must never take political or social positions

**Tradition 11** — *"Our public relations policy is based on attraction rather than promotion; we need always maintain personal anonymity at the press, radio, and films level."*
- No public member roster, no public profiles, no searchable member directory
- Last names are never displayed publicly — first name or display name only
- No social media sharing buttons, no "invite friends" viral mechanics
- Public-facing pages are minimal: meeting times, a contact form, nothing more

**Tradition 12** — *"Anonymity is the spiritual foundation of all our traditions, ever reminding us to place principles before personalities."*
- Anonymity is a **first-class architectural constraint**, not a setting
- Display names default to first name only; full names stored but never shown publicly
- No public-facing profile pages
- Member data (phone, email, sobriety date) is accessible only to authenticated group members
- Phone lists are never indexed, cached, or exposed via any public endpoint
- Audit logs record actions, not personal narratives — dignity in every log line

---

## Architecture

### Multi-Tenancy Model
- Each home group = one **tenant**
- Groups get a subdomain: `earlybird.sobernetwork.org`, `groupname.sobernetwork.org`
- Tenant data is logically (and ideally physically) isolated
- A group can join or leave the platform independently

### 🐾 Microservices vs. Modular Monolith
**Open question** — see discussion notes below.

**Arguments for microservices:**
- Services scale independently
- Teams (trusted servants) could own separate domains
- Natural fit for multi-tenancy at scale

**Arguments against (for now):**
- High operational complexity (service mesh, API gateway, CI/CD per service)
- Free/low-cost hosting targets (GitHub Pages, Vercel, Supabase) don't naturally support microservices
- Premature optimization for a platform starting with one home group
- Harder to hand off to non-technical trusted servants

**Recommended approach:** Start with a **Modular Monolith** architected with clean service boundaries. Modules can be extracted into microservices later if scale demands it. This is sometimes called "monolith-first" (per Martin Fowler).

---

## Content Moderation

### Philosophy
Balance AA's spirit of open sharing with protection against disruptive actors ("bombers"). 
Anonymity and member responsibility are both honored.

### Content Areas & Permissions

| Area | Who Can Post | Moderation Model |
|------|-------------|-----------------|
| Announcements | `group_admin` only | N/A |
| Events | `group_admin` only | N/A |
| Documents | `group_admin` only | N/A |
| Phone List | `group_admin` only | N/A |
| Discussion / Sharing Board | Any member | Post-moderation (flag & remove) |
| New member posts (probationary) | New members (<30 days or <10 approved posts) | Pre-moderation queue |

### Probationary Period
- New members' posts go to admin review queue before publishing
- After trust threshold (30 days + 10 approved posts) → posts publish immediately
- Prevents hit-and-run bombers; most never survive the approval step

### Flagging System
- Any member can flag any post as inappropriate
- Flagged content triggers admin notification
- Content remains visible until admin acts (or auto-hides after N flags)

### Admin Actions (escalating severity)
1. **Remove flag** — false alarm, content stays
2. **Hide post** — content removed from view, member notified
3. **Delete post** — permanent removal (soft-delete for audit trail)
4. **Warn member** — formal warning logged
5. **Suspend member** — temporary loss of posting rights
6. **Ban member** — removed from group, account flagged

### Audit Trail
- All moderation actions are logged (who acted, when, what)
- Soft-deleted content retained for `superadmin` review
- Supports accountability without public shaming

### Must-Have (MVP)
- [ ] Group news / announcements
- [ ] Events calendar
- [ ] Document library (group conscience notes, formats, etc.)
- [ ] Phone list (authenticated members only)
- [ ] Email list / group mailing (via groups.io integration or built-in)
- [ ] User authentication + RBAC

### 🐾 Nice-to-Have (discuss priority)
- [ ] Meeting schedule page (public)
- [ ] Speaker meeting audio archive
- [ ] Sobriety chip tracker / anniversary recognition
- [ ] Links to AA literature and resources
- [ ] Multi-language support

---

## Security & Access Control

### Authentication
- Users must register and be approved (no open self-registration)
- Authentication provider: TBD 🐾 (options: Supabase Auth, Auth0 free tier, GitHub OAuth)

### Roles (proposed)
| Role | Description |
|------|-------------|
| `superadmin` | Platform-level admin (you); can manage all tenants |
| `group_admin` | Trusted servant for a group; manages members and content |
| `member` | Authenticated group member; full read + some write |
| `guest` | Unauthenticated public visitor; sees public content only |

### 🐾 Open Security Questions
- Should members be able to belong to multiple groups?
- How does a new member request access — self-service form, or sponsor-sponsored?
- How do we handle member removal (relapse, misconduct, etc.) gracefully and with dignity?
- Password reset flow — email-based? What if a member's email changes?

---

## Technology Stack ✅ DECIDED

### Stack: C# + Angular (developer-familiar, free-hosted)

| Layer | Choice | Notes |
|-------|--------|-------|
| Frontend | **Angular** | SPA, deployed to GitHub Pages or Vercel (free) |
| Backend | **ASP.NET Core Web API** (C#) | REST API, hosted on Render.com or Fly.io (free tier) |
| Database | **Supabase PostgreSQL** | Free tier, 500MB, accessed via EF Core + Npgsql |
| Auth | **ASP.NET Core Identity + JWT** | RBAC baked in, tokens stored client-side |
| File Storage | **Supabase Storage** | Free tier for documents, phone lists |
| Email List | **groups.io** (free) | Handles mailing list; may integrate later |
| CI/CD | **GitHub Actions** | Free for public repos |
| Domain | Custom (~$12/yr, Namecheap/Porkbun) | Subdomains per group |

### ⚠️ Free Hosting Caveat — Backend Cold Starts
Render.com's free tier **spins down** after 15 minutes of inactivity → ~30 second cold start on first visit. 
**Mitigation options:**
- Use **Fly.io** free tier instead (3 shared VMs, stays warmer)
- Add a lightweight ping/keepalive via GitHub Actions cron job
- Accept it for MVP; upgrade to paid (~$7/mo) when groups grow

### Architecture Pattern
**Modular Monolith** (monolith-first, microservice-ready)
- Clean module boundaries: `Auth`, `Groups`, `Events`, `Documents`, `Members`, `Notifications`
- Each module is a self-contained C# project/folder within one solution
- Can be extracted to microservices later if needed

---

## Hosting & Cost

### Target: $0–$20/month total
- **Vercel** (free tier): Frontend hosting
- **Supabase** (free tier): Auth, PostgreSQL DB, file storage
- **GitHub**: Source control, CI/CD via Actions
- **groups.io** (free for small groups): Email list fallback
- **Domain**: ~$12/year (Namecheap or Porkbun)

### 🐾 Cost Scaling Question
As more groups join, Supabase/Vercel free tiers may be exceeded. Should each group:
- (A) Contribute to a shared platform fund (Tradition 7 consideration)?
- (B) Run their own independent instance (fork the repo)?
- (C) Platform absorbs cost up to a threshold, then asks for group contributions?

---

## Domain & Branding

- **Name**: Sober Network
- **🐾 Domain**: Check availability of `sobernetwork.org` / `.com` / `.group` / `.net`
- Logo/branding: TBD — should feel welcoming, not clinical

---

## Open Dog-Ears 🐾 (Discussion Queue)

1. ✅ **Microservices vs. Modular Monolith** — Modular Monolith, monolith-first
2. ✅ **Tech stack choice** — C# (ASP.NET Core) + Angular + Supabase PostgreSQL
3. ✅ **Authentication provider** — ASP.NET Core Identity + JWT
4. ✅ **Multi-group membership** — YES. One user account, multiple group memberships with per-group roles.
5. ✅ **Member onboarding** — Admin-approved. Member submits a join request; `group_admin` approves. No open self-registration into a group.
6. ✅ **Member offboarding** — Soft removal preferred. Admin can warn → suspend → ban. Audit trail retained. Content soft-deleted (hidden, not purged).
7. ✅ **Nice-to-have features** — ALL are MVP. 🐾 Detailed scalability notes per feature to be documented later.
8. ✅ **Domain name** — `sobernetwork.group` (~$15/yr, Namecheap or Porkbun). Subdomains per group: `earlybird.sobernetwork.group`
9. ✅ **Cost scaling model** — Voluntary contributions from each group's 7th Tradition donations. Free to start. Transparent pricing page visible to all group admins. 🐾 Future feature: cost dashboard showing platform running costs, per-group usage, and contribution status.
10. ✅ **Handoff plan** — Use a **GitHub Organization** (`sobernetwork-group` or similar) from day one, not a personal account. Co-admin role for succession. Architecture docs live in the repo. 🐾 Future consideration: define a platform service structure (analogous to AA's GSO) for long-term governance.

---

*Last updated: 2026-06-02*
