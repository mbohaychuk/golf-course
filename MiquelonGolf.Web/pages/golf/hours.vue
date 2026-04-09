<script setup lang="ts">
useSeoMeta({
  title: 'Opening Hours — Miquelon Hills Golf Course',
  description: 'Opening hours for Miquelon Hills Golf Course near Camrose, Alberta.',
})

const api = useApi()
const days = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday']
const labels: Record<string, string> = {
  monday: 'Monday', tuesday: 'Tuesday', wednesday: 'Wednesday',
  thursday: 'Thursday', friday: 'Friday', saturday: 'Saturday', sunday: 'Sunday',
}

const allContent = await $fetch<{ key: string; value: string }[]>(api.url('/site-content')).catch(() => [])

const hours = (day: string): string => {
  const entry = allContent.find(r => r?.key === `hours.${day}`)
  return entry?.value ?? 'Call for hours'
}
</script>

<template>
  <div>
    <PageHero title="Opening Hours" subtitle="Hours are subject to seasonal changes. Always call ahead during shoulder season." />
    <div class="max-w-2xl mx-auto px-4 py-12">
      <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
        <table class="w-full text-sm">
          <tbody class="divide-y divide-gray-100">
            <tr v-for="day in days" :key="day" class="hover:bg-background transition-colors">
              <td class="px-5 py-3.5 font-medium text-text">{{ labels[day] }}</td>
              <td class="px-5 py-3.5 text-right text-text/70">{{ hours(day) }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <p class="mt-6 text-sm text-text/50">
        Season runs May through October. Course opens at dawn and closes at dusk.
        Contact the pro shop at <a href="tel:+17804732511" class="text-primary hover:underline">(780) 473-2511</a> for current conditions.
      </p>

      <section class="bg-primary rounded-lg p-8 text-center mt-10">
        <h2 class="font-display text-2xl font-bold text-white mb-3">Ready for a Round?</h2>
        <p class="text-white/70 mb-5">Book your tee time online.</p>
        <NuxtLink to="/book" class="inline-flex items-center px-6 py-3 bg-accent text-white font-semibold rounded hover:opacity-90 transition-opacity">
          Book a Tee Time
        </NuxtLink>
      </section>
    </div>
  </div>
</template>
