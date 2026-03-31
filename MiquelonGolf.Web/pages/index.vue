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
      '@type': 'SportsActivityLocation',
      name: 'Miquelon Hills Golf Course',
      description: '18-hole public golf course east of Edmonton, bordering Miquelon Lake Provincial Park.',
      url: 'https://miquelonhillsgolf.com',
      telephone: '+17804732511',
      address: {
        '@type': 'PostalAddress',
        addressRegion: 'AB',
        addressCountry: 'CA',
        addressLocality: 'Camrose County',
      },
      geo: {
        '@type': 'GeoCoordinates',
        latitude: 53.1,
        longitude: -112.9,
      },
    }),
  }],
})

const api = useApi()
const { data: announcements } = await api.get<AnnouncementDto[]>('/announcements/active')
const { data: events } = await api.get<EventDto[]>('/events?limit=3')
</script>

<template>
  <div>
    <!-- Hero -->
    <section class="relative bg-primary text-white">
      <div class="max-w-6xl mx-auto px-4 py-24 md:py-36 flex flex-col items-start gap-6">
        <h1 class="font-display text-4xl md:text-6xl font-bold text-white leading-tight max-w-2xl">
          An hour from Edmonton.<br>A world away.
        </h1>
        <p class="text-white/80 text-lg max-w-xl leading-relaxed">
          18 holes of public golf on the edge of Miquelon Lake Provincial Park.
          Open fairways on the front, tight forest corridors on the back.
        </p>
        <div class="flex flex-wrap gap-3">
          <NuxtLink
            to="/book"
            class="inline-flex items-center px-6 py-3 bg-accent text-white font-semibold rounded hover:opacity-90 transition-opacity"
          >
            Book a Tee Time
          </NuxtLink>
          <NuxtLink
            to="/golf/course"
            class="inline-flex items-center px-6 py-3 border border-white/50 text-white font-semibold rounded hover:bg-white/10 transition-colors"
          >
            View the Course
          </NuxtLink>
        </div>
      </div>
    </section>

    <!-- Announcements banner -->
    <AnnouncementBanner :announcements="announcements ?? []" />

    <!-- 3-col highlights -->
    <section class="max-w-6xl mx-auto px-4 py-12 grid grid-cols-1 md:grid-cols-3 gap-6">
      <NuxtLink to="/book" class="group bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-3 hover:shadow-md transition-shadow">
        <div class="w-10 h-10 bg-primary/10 rounded-full flex items-center justify-center">
          <svg class="w-5 h-5 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
        </div>
        <h2 class="font-display text-lg font-bold text-accent group-hover:underline">Book a Tee Time</h2>
        <p class="text-sm text-text/70 leading-relaxed">Reserve your spot online — choose your date, pick a time, and you're set.</p>
      </NuxtLink>

      <NuxtLink to="/events" class="group bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-3 hover:shadow-md transition-shadow">
        <div class="w-10 h-10 bg-primary/10 rounded-full flex items-center justify-center">
          <svg class="w-5 h-5 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0"/>
          </svg>
        </div>
        <h2 class="font-display text-lg font-bold text-accent group-hover:underline">Upcoming Events</h2>
        <p class="text-sm text-text/70 leading-relaxed">Tournaments, ladies nights, social events — there's always something on at Miquelon.</p>
      </NuxtLink>

      <NuxtLink to="/golf/course" class="group bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-3 hover:shadow-md transition-shadow">
        <div class="w-10 h-10 bg-primary/10 rounded-full flex items-center justify-center">
          <svg class="w-5 h-5 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7"/>
          </svg>
        </div>
        <h2 class="font-display text-lg font-bold text-accent group-hover:underline">Course Layout</h2>
        <p class="text-sm text-text/70 leading-relaxed">Explore all 18 holes — yardages, par, handicap, and strategy tips.</p>
      </NuxtLink>
    </section>

    <!-- SEO copy section -->
    <section class="bg-surface py-12">
      <div class="max-w-4xl mx-auto px-4">
        <h2 class="font-display text-3xl font-bold text-accent mb-4">Play Golf Near Miquelon Lake</h2>
        <p class="text-text leading-relaxed mb-4">
          Miquelon Hills Golf Course sits in the heart of Camrose County, just one hour east of Edmonton
          and 45 minutes from Sherwood Park — close enough for a weekday round after work, beautiful
          enough to plan a whole weekend around. Our 18-hole public course borders
          Miquelon Lake Provincial Park, giving you genuine Alberta wilderness as your backdrop.
        </p>
        <p class="text-text leading-relaxed mb-4">
          The <strong>front nine</strong> offers wider, more open fairways — perfect for players of all
          skill levels looking to settle into a round. The <strong>back nine</strong> tightens into
          narrow forest corridors with trees pressing in on both sides, rewarding accuracy and
          punishing the wayward drive. It's a course that plays differently every time.
        </p>
        <p class="text-text leading-relaxed">
          Whether you're searching for golf near Camrose, golf east of Edmonton, or an 18-hole
          Alberta golf experience unlike any other, Miquelon Hills is worth the drive.
        </p>
      </div>
    </section>

    <!-- Upcoming events strip -->
    <section v-if="events && events.length > 0" class="max-w-6xl mx-auto px-4 py-12">
      <div class="flex items-center justify-between mb-6">
        <h2 class="font-display text-2xl font-bold text-accent">Upcoming Events</h2>
        <NuxtLink to="/events" class="text-sm font-semibold text-primary hover:underline">View all events →</NuxtLink>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <EventCard v-for="event in events" :key="event.id" :event="event" />
      </div>
    </section>

    <!-- Stay & Play RV teaser -->
    <section class="bg-primary/5 border-t border-primary/10 py-12">
      <div class="max-w-6xl mx-auto px-4 flex flex-col md:flex-row items-center gap-8">
        <div class="flex-1">
          <h2 class="font-display text-3xl font-bold text-accent mb-3">Stay &amp; Play</h2>
          <p class="text-text leading-relaxed mb-4">
            Make a weekend of it. Miquelon Hills offers seasonal RV sites steps from the first tee,
            right on the edge of the provincial park. It's the "Stay &amp; Play" experience you
            won't find at any other course this close to Edmonton.
          </p>
          <NuxtLink
            to="/rv"
            class="inline-flex items-center px-5 py-2.5 bg-primary text-white font-semibold rounded hover:opacity-90 transition-opacity"
          >
            Learn More About RV Sites
          </NuxtLink>
        </div>
        <div class="w-full md:w-80 h-48 bg-primary/20 rounded-lg flex items-center justify-center text-primary/40 text-sm">
          RV site photo
        </div>
      </div>
    </section>
  </div>
</template>
