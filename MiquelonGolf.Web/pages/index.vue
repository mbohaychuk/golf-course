<script setup lang="ts">
import type { AnnouncementDto, EventDto } from '~/types/api'

useSeoMeta({
  title: 'Miquelon Hills Golf Course — Golf Near Edmonton, Alberta',
  description: 'An 18-hole public golf course east of Edmonton, bordering Miquelon Lake Provincial Park. Book tee times online. Seasonal RV sites available.',
  ogTitle: 'Miquelon Hills Golf Course',
  ogDescription: 'Golf near Camrose, Alberta — an hour east of Edmonton, a world away.',
})

useHead({
  script: [{
    type: 'application/ld+json',
    innerHTML: JSON.stringify({
      '@context': 'https://schema.org',
      '@type': 'GolfCourse',
      name: 'Miquelon Hills Golf Course',
      description: '18-hole public golf course east of Edmonton, bordering Miquelon Lake Provincial Park.',
      url: 'https://miquelonhillsgolf.com',
      telephone: '+17804732511',
      email: 'info@miquelonhillsgolf.com',
      image: 'https://miquelonhillsgolf.com/images/fairway-corridor.avif',
      address: {
        '@type': 'PostalAddress',
        addressLocality: 'Camrose County',
        addressRegion: 'AB',
        addressCountry: 'CA',
      },
      geo: {
        '@type': 'GeoCoordinates',
        latitude: 53.1,
        longitude: -112.9,
      },
      openingHoursSpecification: {
        '@type': 'OpeningHoursSpecification',
        dayOfWeek: ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'],
        opens: '07:00',
        closes: '19:00',
      },
      priceRange: '$$',
      sameAs: [
        'https://www.facebook.com/miquelonhillsgolf',
        'https://www.instagram.com/miquelonhillsgolf',
      ],
    }),
  }],
})

const api = useApi()
const { data: announcements } = await api.get<AnnouncementDto[]>('/announcements/active')
const { data: events } = await api.get<EventDto[]>('/events?limit=3')
</script>

<template>
  <div>
    <!-- Hero — dark forest canopy with photo -->
    <section
      class="relative overflow-hidden text-white"
      style="background-color: var(--color-forest); background-image: linear-gradient(to right, rgba(11,30,5,0.92) 0%, rgba(11,30,5,0.72) 55%, rgba(11,30,5,0.50) 100%), url('/images/fairway-corridor.avif'); background-size: cover; background-position: center 40%;"
    >
      <div class="max-w-6xl mx-auto px-4 pt-24 pb-40 md:pt-36 md:pb-56 flex flex-col items-start gap-8 relative z-10">
        <div class="text-xs font-semibold tracking-[0.25em] uppercase text-white/50">
          Camrose County, Alberta
        </div>
        <h1 class="font-display text-5xl md:text-7xl font-bold text-white leading-[1.05] max-w-2xl">
          An hour from<br>Edmonton.<br>
          <span style="color: var(--color-accent);">A world away.</span>
        </h1>
        <p class="text-white/70 text-lg max-w-lg leading-relaxed">
          18 holes carved through the boreal edge of Miquelon Lake Provincial Park.
          Open meadows on the front. Tight forest corridors on the back.
        </p>
        <div class="flex flex-wrap gap-3 pt-2">
          <NuxtLink
            to="/book"
            class="inline-flex items-center px-7 py-3.5 font-semibold rounded transition-opacity hover:opacity-90 text-white"
            style="background-color: var(--color-accent);"
          >
            Book a Tee Time
          </NuxtLink>
          <NuxtLink
            to="/golf/course"
            class="inline-flex items-center px-7 py-3.5 border border-white/30 text-white font-semibold rounded hover:bg-white/10 transition-colors"
          >
            View the Course
          </NuxtLink>
        </div>
      </div>

      <!-- SVG treeline — back layer (depth) -->
      <div class="absolute bottom-0 left-0 right-0 z-10 pointer-events-none" aria-hidden="true">
        <svg viewBox="0 0 1440 180" preserveAspectRatio="none" class="w-full block" style="height: 180px;">
          <path
            fill="#2A5410"
            fill-opacity="0.4"
            d="M0,180 L0,125 L22,85 L44,120 L66,72 L88,108 L110,52 L132,95 L154,75 L176,112 L198,58 L220,98 L242,78 L264,115 L286,62 L308,100 L330,42 L352,88 L374,65 L396,105 L418,48 L440,90 L462,68 L484,108 L506,52 L528,95 L550,58 L572,98 L594,35 L616,82 L638,58 L660,100 L682,45 L704,88 L726,65 L748,108 L770,48 L792,90 L814,38 L836,82 L858,58 L880,102 L902,45 L924,88 L946,62 L968,105 L990,38 L1012,82 L1034,58 L1056,98 L1078,42 L1100,88 L1122,62 L1144,108 L1166,48 L1188,90 L1210,68 L1232,108 L1254,42 L1276,88 L1298,58 L1320,98 L1342,48 L1364,88 L1386,62 L1408,105 L1430,52 L1440,88 L1440,180 Z"
          />
        </svg>
      </div>

      <!-- SVG treeline — front layer (silhouette, matches background below) -->
      <div class="absolute bottom-0 left-0 right-0 z-20 pointer-events-none" aria-hidden="true">
        <svg viewBox="0 0 1440 120" preserveAspectRatio="none" class="w-full block" style="height: 120px;">
          <path
            fill="var(--color-background)"
            d="M0,120 L0,82 L18,50 L36,80 L54,35 L72,68 L90,20 L108,58 L126,40 L144,72 L162,25 L180,62 L198,42 L216,75 L234,28 L252,65 L270,15 L288,55 L306,35 L324,68 L342,22 L360,60 L378,38 L396,72 L414,25 L432,62 L450,30 L468,68 L486,12 L504,52 L522,32 L540,72 L558,22 L576,60 L594,38 L612,75 L630,20 L648,60 L666,38 L684,75 L702,25 L720,65 L738,18 L756,58 L774,32 L792,72 L810,22 L828,62 L846,40 L864,78 L882,25 L900,65 L918,40 L936,75 L954,20 L972,60 L990,32 L1008,70 L1026,20 L1044,60 L1062,38 L1080,75 L1098,25 L1116,65 L1134,40 L1152,78 L1170,25 L1188,65 L1206,38 L1224,75 L1242,20 L1260,60 L1278,38 L1296,70 L1314,25 L1332,65 L1350,40 L1368,75 L1386,30 L1404,65 L1422,45 L1440,82 L1440,120 Z"
          />
        </svg>
      </div>
    </section>

    <!-- Announcements banner -->
    <AnnouncementBanner :announcements="announcements ?? []" />

    <!-- 3-col highlights -->
    <section class="max-w-6xl mx-auto px-4 py-14 grid grid-cols-1 md:grid-cols-3 gap-6">
      <NuxtLink
        to="/book"
        class="group bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-3 hover:shadow-md hover:-translate-y-1 transition-all border-l-4 border-primary"
      >
        <div class="w-12 h-12 rounded-full flex items-center justify-center" style="background-color: color-mix(in srgb, var(--color-primary) 12%, transparent);">
          <svg class="w-5 h-5" style="color: var(--color-primary);" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
        </div>
        <h2 class="font-display text-lg font-bold" style="color: var(--color-accent);">
          Book a Tee Time
          <span class="inline-block translate-x-0 group-hover:translate-x-1 transition-transform">→</span>
        </h2>
        <p class="text-sm leading-relaxed" style="color: color-mix(in srgb, var(--color-text) 65%, transparent);">Reserve your spot online — choose your date, pick a time, and you're set.</p>
      </NuxtLink>

      <NuxtLink
        to="/events"
        class="group bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-3 hover:shadow-md hover:-translate-y-1 transition-all border-l-4 border-primary"
      >
        <div class="w-12 h-12 rounded-full flex items-center justify-center" style="background-color: color-mix(in srgb, var(--color-primary) 12%, transparent);">
          <svg class="w-5 h-5" style="color: var(--color-primary);" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0"/>
          </svg>
        </div>
        <h2 class="font-display text-lg font-bold" style="color: var(--color-accent);">
          Upcoming Events
          <span class="inline-block translate-x-0 group-hover:translate-x-1 transition-transform">→</span>
        </h2>
        <p class="text-sm leading-relaxed" style="color: color-mix(in srgb, var(--color-text) 65%, transparent);">Tournaments, ladies nights, social events — there's always something on at Miquelon.</p>
      </NuxtLink>

      <NuxtLink
        to="/golf/course"
        class="group bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-3 hover:shadow-md hover:-translate-y-1 transition-all border-l-4 border-primary"
      >
        <div class="w-12 h-12 rounded-full flex items-center justify-center" style="background-color: color-mix(in srgb, var(--color-primary) 12%, transparent);">
          <svg class="w-5 h-5" style="color: var(--color-primary);" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7"/>
          </svg>
        </div>
        <h2 class="font-display text-lg font-bold" style="color: var(--color-accent);">
          Course Layout
          <span class="inline-block translate-x-0 group-hover:translate-x-1 transition-transform">→</span>
        </h2>
        <p class="text-sm leading-relaxed" style="color: color-mix(in srgb, var(--color-text) 65%, transparent);">Explore all 18 holes — yardages, par, handicap, and strategy tips.</p>
      </NuxtLink>
    </section>

    <!-- Scenic photo strip -->
    <div class="grid grid-cols-3 h-56 md:h-72 overflow-hidden">
      <div class="relative overflow-hidden group">
        <img src="/images/green.png" alt="Putting green at Miquelon Hills Golf Course" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105" />
        <div class="absolute inset-0 bg-gradient-to-t from-forest/70 to-transparent" />
        <p class="absolute bottom-0 left-0 p-3 text-white text-xs font-semibold tracking-wide uppercase">The Green</p>
      </div>
      <div class="relative overflow-hidden group">
        <img src="/images/fairway-open.avif" alt="Open fairway on the front nine" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105" />
        <div class="absolute inset-0 bg-gradient-to-t from-forest/70 to-transparent" />
        <p class="absolute bottom-0 left-0 p-3 text-white text-xs font-semibold tracking-wide uppercase">Open Fairways</p>
      </div>
      <div class="relative overflow-hidden group">
        <img src="/images/approach.avif" alt="Approach shot on the back nine" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105" />
        <div class="absolute inset-0 bg-gradient-to-t from-forest/70 to-transparent" />
        <p class="absolute bottom-0 left-0 p-3 text-white text-xs font-semibold tracking-wide uppercase">The Approach</p>
      </div>
    </div>

    <!-- SEO copy section -->
    <section class="py-14" style="background-color: var(--color-surface);">
      <div class="max-w-4xl mx-auto px-4">
        <div class="w-12 h-1 mb-6 rounded" style="background-color: var(--color-primary);"></div>
        <h2 class="font-display text-3xl font-bold mb-5" style="color: var(--color-accent);">Play Golf Near Miquelon Lake</h2>
        <p class="leading-relaxed mb-4" style="color: var(--color-text);">
          Miquelon Hills Golf Course sits in the heart of Camrose County, just one hour east of Edmonton
          and 45 minutes from Sherwood Park — close enough for a weekday round after work, beautiful
          enough to plan a whole weekend around. Our 18-hole public course borders
          Miquelon Lake Provincial Park, giving you genuine Alberta wilderness as your backdrop.
        </p>
        <p class="leading-relaxed mb-4" style="color: var(--color-text);">
          The <strong>front nine</strong> offers wider, more open fairways — perfect for players of all
          skill levels looking to settle into a round. The <strong>back nine</strong> tightens into
          narrow forest corridors with trees pressing in on both sides, rewarding accuracy and
          punishing the wayward drive. It's a course that plays differently every time.
        </p>
        <p class="leading-relaxed" style="color: var(--color-text);">
          Whether you're searching for golf near Camrose, golf east of Edmonton, or an 18-hole
          Alberta golf experience unlike any other, Miquelon Hills is worth the drive.
        </p>
      </div>
    </section>

    <!-- Upcoming events strip -->
    <section v-if="events && events.length > 0" class="max-w-6xl mx-auto px-4 py-14">
      <div class="flex items-center justify-between mb-6">
        <h2 class="font-display text-2xl font-bold" style="color: var(--color-accent);">Upcoming Events</h2>
        <NuxtLink to="/events" class="text-sm font-semibold hover:underline" style="color: var(--color-primary);">View all events →</NuxtLink>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <EventCard v-for="event in events" :key="event.id" :event="event" />
      </div>
    </section>

    <!-- Stay & Play — dark forest section -->
    <section class="py-16 text-white relative overflow-hidden" style="background-color: var(--color-forest);">
      <div class="max-w-6xl mx-auto px-4 flex flex-col md:flex-row items-center gap-10 relative z-10">
        <div class="flex-1">
          <div class="w-10 h-1 mb-5 rounded" style="background-color: var(--color-accent);"></div>
          <h2 class="font-display text-3xl font-bold text-white mb-4">Stay &amp; Play</h2>
          <p class="text-white/70 leading-relaxed mb-6">
            Make a weekend of it. Miquelon Hills offers seasonal RV sites steps from the first tee,
            right on the edge of the provincial park. It's the "Stay &amp; Play" experience you
            won't find at any other course this close to Edmonton.
          </p>
          <NuxtLink
            to="/rv"
            class="inline-flex items-center px-6 py-3 font-semibold rounded hover:opacity-90 transition-opacity text-white"
            style="background-color: var(--color-accent);"
          >
            Learn More About RV Sites
          </NuxtLink>
        </div>
        <div
          class="w-full md:w-96 h-56 rounded-lg overflow-hidden flex-shrink-0"
          style="box-shadow: 0 0 0 2px color-mix(in srgb, var(--color-canopy) 50%, transparent);"
        >
          <img
            src="/images/clubhouse.avif"
            alt="Miquelon Hills Golf Course clubhouse"
            class="w-full h-full object-cover object-center"
          />
        </div>
      </div>
    </section>
  </div>
</template>
