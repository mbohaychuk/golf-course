<script setup lang="ts">
import type { EventDto } from '~/types/api'

useSeoMeta({
  title: 'Events & Calendar — Miquelon Hills Golf Course',
  description: 'Tournaments, ladies nights, men\'s nights, and social events at Miquelon Hills Golf Course near Camrose, Alberta.',
})

const api = useApi()
const { data: events, error } = await api.get<EventDto[]>('/events')
</script>

<template>
  <div>
    <PageHero title="Events & Calendar" subtitle="Tournaments, social nights, and member events throughout the season." />
    <div class="max-w-6xl mx-auto px-4 py-12">
      <div v-if="error" class="bg-red-50 border border-red-200 rounded p-4 text-red-700 text-sm">
        Unable to load events right now. Please try again later or call us at (780) 473-2511.
      </div>

      <div v-else-if="events && events.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <EventCard v-for="event in events" :key="event.id" :event="event" />
      </div>

      <div v-else class="text-center py-16 text-text/60">
        <svg class="w-12 h-12 mx-auto mb-4 text-primary/30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
        </svg>
        <p class="text-lg font-medium">No upcoming events posted yet.</p>
        <p class="text-sm mt-2">Check back soon or follow us on Facebook for updates.</p>
      </div>
    </div>
  </div>
</template>
