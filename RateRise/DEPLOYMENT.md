# RateRise — Deployment Guide (Put It Online, Free)

You only need this guide for **Model B** (free lead magnet) or if you want a live demo link for
your sales page. For **Model A** (selling the file as a download on Gumroad/Etsy), no deployment
is needed at all — the buyer downloads the file and opens it. Nothing to host.

All options below are **free** and need **no coding**.

---

## Option 1 — Netlify Drop (easiest, ~2 minutes)

1. Go to **app.netlify.com/drop** and create a free account.
2. Put `index.html` inside an empty folder on your computer.
3. Drag that folder onto the Netlify Drop page.
4. Done — you get a live link like `your-name.netlify.app`. Share it anywhere.
5. (Optional) In Site settings you can rename the link or connect a custom domain
   (e.g. `pricing.yourbusiness.com`).

To update the site later, drag the folder in again.

## Option 2 — GitHub Pages (free, uses what you already have)

Your file already lives in a GitHub repository, so:

1. On GitHub, open your repository → **Settings → Pages**.
2. Under "Build and deployment," choose the branch that contains the `RateRise/` folder and save.
3. After a minute your app is live at
   `https://<your-username>.github.io/<repo-name>/RateRise/`.

Every time you push a change to that branch, the live site updates itself.

## Option 3 — Vercel / Cloudflare Pages

Both have free tiers and work the same way: create an account, "Add new project," point it at
your GitHub repository (or drag-and-drop the folder), deploy. Choose whichever you prefer —
for a single HTML file they're all equivalent.

---

## Setting up email capture (the lead-magnet engine)

The Home page has a "free pricing cheat-sheet" email form. Captured emails are always stored
locally in the visitor's browser — but for a lead magnet you want them sent to **you**. Two ways:

### A. Form endpoint (recommended — takes 5 minutes)

1. Create a free account at **Formspree.io** (or Getform, or Basin — all similar).
2. Create a new form; it gives you an endpoint URL like `https://formspree.io/f/abcd1234`.
3. In RateRise, open the **Launch & Earn** tab and paste that URL into
   **"Email form endpoint."**
4. Redeploy the file (Netlify: drag again; GitHub Pages: commit and push).
5. From now on, every signup also lands in your Formspree dashboard and email inbox.
   Formspree's free tier covers ~50 submissions/month — plenty to start.

**Important:** the endpoint value is saved in browser storage, which means it must be set on the
*deployed* copy you share (set it once, on any device, before sharing won't work — storage is
per-device). The reliable way: open `index.html` in a text editor, find the line containing
`id="cap_ep"`, and add your URL as the default value:
`<input class="inp" id="cap_ep" value="https://formspree.io/f/abcd1234" ...>`.
Then deploy that edited file. Now it's baked in for every visitor.

### B. CSV export (no setup)

If you skip the endpoint, emails still collect in the **Launch & Earn → Your captured leads**
table *on the device where they were entered*. This works when you use RateRise in person — e.g.
at a market stall, a workshop, or a consult where clients type their email on **your** laptop or
tablet. Click **Export leads (CSV)** and import the file into your email tool
(MailerLite, ConvertKit, Mailchimp — all have free tiers).

> Note: without an endpoint, emails typed by visitors on *their own* devices stay on their
> devices — you'll never see them. For a shared public link, set up the endpoint (Option A).

---

## Delivering the "pricing cheat-sheet" you promised

The form promises a freebie, so have one ready (keeps trust + gives you a reason to email):

1. Write one page: *"5 Steps to Raise Your Rates Without Losing Clients"* — you can pull the
   content straight from the SELLING-ROADMAP.md content bank and the in-app tips.
2. Save it as a PDF (write it in Google Docs → File → Download → PDF).
3. Set your email tool (MailerLite/ConvertKit free tier) to auto-send it to new subscribers, or
   simply reply manually while volume is small — manual replies also start conversations that
   lead to sales.

---

## Checklist

- [ ] Deployed to Netlify / GitHub Pages (Model B or demo link)
- [ ] Form endpoint created and baked into the deployed file
- [ ] Test: submit your own email on the live link, confirm it arrives
- [ ] Cheat-sheet PDF written and auto-delivery set up
- [ ] Live link added to your social bios and Gumroad/Etsy listing
