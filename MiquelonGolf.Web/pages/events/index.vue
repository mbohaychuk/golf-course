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
  <div class="max-w-6xl mx-auto px-4 py-12">
    <h1 class="font-display text-4xl font-bold text-accent mb-2">Events &amp; Calendar</h1>
    <p class="text-text/70 mb-8">Tournaments, social nights, and member events throughout the season.</p>

    <div v-if="error" class="bg-red-50 border border-red-200 rounded p-4 text-red-700 text-sm">
      Unable to load events right now. Please try again later or call us at (780) 473-2511.
    </div>

    <div v-else-if="events && events.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <EventCard v-for="event in events" :key="event.id" :event="event" />
    </div>

    <div v-else class="text-center py-16 text-text/40">
      <p class="text-lg">No upcoming events posted yet.</p>
      <p class="text-sm mt-2">Check back soon or follow us on Facebook for updates.</p>
    </div>
  </div>
</template>
