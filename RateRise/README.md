# RateRise — Pricing Studio for Service Entrepreneurs

**RateRise is a sellable digital product.** It's a single, self-contained web app that helps
service-based business owners stop undercharging: it reverse-engineers the rate they need to
hit their income goals, builds tiered service packages, generates client-ready quotes, and
tracks a monthly revenue goal.

It runs **100% in the browser** — no server, no database, no monthly hosting bill. Everything a
user types is saved locally on their own device. That makes it cheap to sell and cheap to run:
every sale is essentially pure profit.

## Who it's for

Coaches, virtual assistants, photographers, designers, beauty & wellness pros, wedding & event
vendors, Etsy/handmade sellers, and consultants — the service businesses that most often
underprice themselves.

## What's inside (`index.html`)

1. **Rate Calculator** — turns a desired take-home income (plus expenses, tax set-aside, working
   weeks, and billable hours) into a defensible hourly rate, day rate, and minimum project price.
   Optionally shows how much money is being left on the table at a current rate.
2. **Package Builder** — bundles services into Essentials / Signature / Premium tiers, priced
   automatically from cost + a target profit margin (Good/Better/Best stacking).
3. **Quote Generator** — produces a branded, printable quote (auto business initials, line items,
   tax, deposit, terms) that can be saved as a PDF straight from the browser's print dialog.
4. **Goal Tracker** — converts a monthly revenue goal into a concrete "clients needed" number and
   tracks logged wins against it with a progress bar.
5. **Launch & Earn** — a built-in monetization playbook, an editable checkout button the buyer
   can point at their own Gumroad/Payhip/Stripe link, plus an **email lead-capture form** (free
   "pricing cheat-sheet") with a lead manager: view captured emails, export them to CSV, and
   optionally forward them to a form endpoint (Formspree/Getform/ConvertKit) so you receive them
   automatically.

## Supporting documents

- **`USER-GUIDE.md`** — plain-English owner's manual: what the tool is, how to open it, how to
  use every screen, and a map of which document answers which question. Start here.
- **`DEPLOYMENT.md`** — how to put it online free (Netlify Drop / GitHub Pages / Vercel), set up
  the email-capture endpoint so leads reach your inbox, and deliver the promised cheat-sheet.
- **`LISTING.md`** — ready-to-paste sales/listing copy (titles, descriptions, benefit bullets,
  FAQ, tags, and price points) for Gumroad, Payhip, or Etsy.
- **`SELLING-ROADMAP.md`** — a phased go-to-market plan: how to set it up, get your first 5 sales,
  build steady sales, scale, plus a marketing content bank and realistic revenue math.

## How it makes money

Two proven models, both explained in-app on the **Launch & Earn** tab:

- **Sell it as a digital product** ($27–$49 one-time) on Gumroad, Payhip, or Etsy. Rebrand it,
  upload the single HTML file as the download, and it sells while you sleep with zero delivery cost.
- **Use it as a free lead magnet** that funnels warm, self-diagnosed "I'm underpricing" leads into a
  higher-ticket offer — a pricing audit, coaching, or done-for-you packaging.

## Use it now

Open `index.html` in any modern browser — that's it. No build step, no install, works offline.

## Rebrand it for resale

- Change the name "RateRise" in the header and `<title>`.
- Change the accent color: edit the `--brand` / `--brand-2` CSS variables in the `<style>` block.
- Set your storefront link on the **Launch & Earn** tab (it's saved with the file's local storage,
  or hard-code it into the `l_link` input's default value before selling).

## Tech notes

- Pure HTML/CSS/vanilla JS, no dependencies, no network calls.
- Data persists via `localStorage` under the `rr_` prefix, scoped per device/browser.
- Light and dark themes via `prefers-color-scheme`.
- Print styles isolate the quote so "Save as PDF" produces a clean one-page document.

Verified end-to-end in a headless Chromium run: all four calculators compute correctly, data
survives a page reload, and there are no console errors.
