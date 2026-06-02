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

### AA 12 Traditions Alignment
- **Tradition 1**: Platform design supports group unity; no feature should fracture a group
- **Tradition 6**: Platform is not "AA" — it's *Sober Network*, an independently operated tool
- **Tradition 7**: Each group is self-supporting; billing/costs should be per-group where applicable
- **Tradition 11**: Anonymity at the public level — no last names, photos optional, no public rosters
- **Tradition 12**: Anonymity is the spiritual foundation — built into the data model, not bolted on

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
