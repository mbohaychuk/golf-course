<script setup lang="ts">
import type { BookingSettingsDto } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })
useSeoMeta({ title: 'Booking Settings — Admin' })

const { authHeaders } = useAuth()
const api = useApi()
const toast = useToast()

// Setting definitions with metadata for rendering
const settingsDef = [
  {
    key: 'booking-window',
    label: 'Booking Window (days)',
    field: 'bookingWindowDays' as keyof BookingSettingsDto,
    min: 1,
    max: 60,
    description: 'How far ahead golfers can book online. A 14-day window means players can reserve tee times up to two weeks in advance.',
  },
  {
    key: 'turn-time',
    label: 'Turn Time Offset (minutes)',
    field: 'turnTimeOffsetMinutes' as keyof BookingSettingsDto,
    min: 60,
    max: 240,
    description: 'Estimated time for an 18-hole group to reach Hole 10. Used to check availability on the back nine when scheduling new bookings.',
  },
  {
    key: 'tee-time-interval',
    label: 'Tee Time Interval (minutes)',
    field: 'teeTimeIntervalMinutes' as keyof BookingSettingsDto,
    min: 6,
    max: 20,
    description: 'Minutes between tee time slots. Shorter intervals allow more groups per day but reduce spacing between tee-offs.',
  },
  {
    key: 'max-players',
    label: 'Max Players Per Slot',
    field: 'maxPlayersPerSlot' as keyof BookingSettingsDto,
    min: 1,
    max: 8,
    description: 'Maximum number of golfers allowed in a single tee time. Most courses use 4 for standard play.',
  },
]

// State
const loading = ref(true)
const values = ref<Record<string, number>>({})
const savingKey = ref<string | null>(null)

async function loadSettings() {
  loading.value = true
  try {
    const { data } = await api.get<BookingSettingsDto>('/admin/settings', { headers: authHeaders.value })
    if (data.value) {
      for (const def of settingsDef) {
        values.value[def.key] = data.value[def.field]
      }
    }
  } catch {
    toast.error('Failed to load settings.')
  } finally {
    loading.value = false
  }
}

async function saveSetting(key: string) {
  savingKey.value = key
  try {
    await api.put('/admin/settings/' + key, values.value[key], authHeaders.value)
    toast.success('Setting saved.')
  } catch (e: any) {
    const message = e?.data?.detail ?? e?.data?.title ?? e?.data ?? 'Failed to save setting.'
    toast.error(typeof message === 'string' ? message : 'Failed to save setting.')
  } finally {
    savingKey.value = null
  }
}

loadSettings()
</script>

<template>
  <div class="max-w-3xl mx-auto px-4 py-8">
    <h1 class="font-display text-2xl font-bold mb-1" style="color: var(--color-accent);">Booking Settings</h1>
    <p class="text-sm mb-6" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">
      Configure global booking parameters. Changes take effect immediately for new bookings and slot generation.
    </p>

    <div v-if="loading" class="text-sm" style="color: color-mix(in srgb, var(--color-text) 50%, transparent);">Loading...</div>

    <div v-else class="flex flex-col gap-4">
      <div
        v-for="def in settingsDef"
        :key="def.key"
        class="bg-surface border rounded-lg p-4"
        style="border-color: color-mix(in srgb, var(--color-primary) 15%, transparent);"
      >
        <div class="flex flex-wrap items-center gap-4">
          <div class="flex-1 min-w-48">
            <label :for="'setting-' + def.key" class="text-sm font-semibold block" style="color: var(--color-text);">
              {{ def.label }}
            </label>
            <p class="text-xs mt-1" style="color: color-mix(in srgb, var(--color-text) 50%, transparent);">
              {{ def.description }}
            </p>
          </div>

          <div class="flex items-center gap-3">
            <input
              :id="'setting-' + def.key"
              v-model.number="values[def.key]"
              type="number"
              :min="def.min"
              :max="def.max"
              class="border rounded px-3 py-1.5 text-sm w-24 focus:outline-none focus:ring-2 focus:ring-primary/30"
              style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);"
            >
            <span class="text-xs whitespace-nowrap" style="color: color-mix(in srgb, var(--color-text) 40%, transparent);">
              {{ def.min }}–{{ def.max }}
            </span>
            <button
              class="px-3 py-1.5 text-sm font-medium text-white rounded hover:opacity-90 transition-opacity disabled:opacity-50"
              style="background-color: var(--color-primary);"
              :disabled="savingKey === def.key"
              @click="saveSetting(def.key)"
            >
              {{ savingKey === def.key ? 'Saving...' : 'Save' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
