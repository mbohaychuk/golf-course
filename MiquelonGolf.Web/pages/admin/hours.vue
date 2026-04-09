<script setup lang="ts">
import type { OperatingHoursDto, CourseHolidayDto, SpecialHoursDto } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })
useSeoMeta({ title: 'Hours & Availability — Admin' })

const { authHeaders } = useAuth()
const api = useApi()

type Tab = 'schedule' | 'holidays' | 'special'
const activeTab = ref<Tab>('schedule')

// ── Weekly Schedule ────────────────────────────────────────────────────────
const schedule = ref<OperatingHoursDto[]>([])
const scheduleLoading = ref(false)
const scheduleError = ref<string | null>(null)
const savingDay = ref<number | null>(null)

async function loadSchedule() {
  scheduleLoading.value = true
  try {
    const { data } = await api.get<OperatingHoursDto[]>('/course-hours/schedule')
    schedule.value = data.value ?? []
  } finally {
    scheduleLoading.value = false
  }
}

async function saveDay(row: OperatingHoursDto) {
  savingDay.value = row.dayOfWeek
  scheduleError.value = null
  try {
    const updated = await api.put<OperatingHoursDto>(
      `/course-hours/schedule/${row.dayOfWeek}`,
      {
        isOpen: row.isOpen,
        openTime: row.openTime,
        closeTime: row.closeTime,
        intervalMinutes: row.intervalMinutes,
        maxPlayers: row.maxPlayers,
      },
      authHeaders.value
    )
    const idx = schedule.value.findIndex(r => r.dayOfWeek === row.dayOfWeek)
    if (idx >= 0) schedule.value[idx] = updated
  } catch (e: any) {
    scheduleError.value = e?.data?.detail ?? e?.data?.title ?? 'Failed to save.'
  } finally {
    savingDay.value = null
  }
}

// ── Holidays ──────────────────────────────────────────────────────────────
const holidays = ref<CourseHolidayDto[]>([])
const holidaysLoading = ref(false)
const holidayForm = reactive({ date: '', reason: '' })
const holidayError = ref<string | null>(null)
const holidaySubmitting = ref(false)

async function loadHolidays() {
  holidaysLoading.value = true
  try {
    const { data } = await api.get<CourseHolidayDto[]>('/course-hours/holidays')
    holidays.value = data.value ?? []
  } finally {
    holidaysLoading.value = false
  }
}

async function addHoliday() {
  holidayError.value = null
  holidaySubmitting.value = true
  try {
    const created = await api.post<CourseHolidayDto>(
      '/course-hours/holidays',
      { date: holidayForm.date, reason: holidayForm.reason },
      authHeaders.value
    )
    holidays.value.push(created)
    holidays.value.sort((a, b) => a.date.localeCompare(b.date))
    holidayForm.date = ''
    holidayForm.reason = ''
  } catch (e: any) {
    holidayError.value = e?.data?.detail ?? e?.data?.title ?? 'Failed to add holiday.'
  } finally {
    holidaySubmitting.value = false
  }
}

async function deleteHoliday(id: string) {
  if (!confirm('Remove this holiday?')) return
  try {
    await api.del(`/course-hours/holidays/${id}`, authHeaders.value)
    holidays.value = holidays.value.filter(h => h.id !== id)
  } catch {
    alert('Could not remove holiday.')
  }
}

// ── Special Hours ─────────────────────────────────────────────────────────
const specialHours = ref<SpecialHoursDto[]>([])
const specialLoading = ref(false)
const specialForm = reactive({ date: '', openTime: '07:00', closeTime: '19:00', reason: '' })
const specialError = ref<string | null>(null)
const specialSubmitting = ref(false)
const editingSpecial = ref<SpecialHoursDto | null>(null)

async function loadSpecial() {
  specialLoading.value = true
  try {
    const { data } = await api.get<SpecialHoursDto[]>('/course-hours/special')
    specialHours.value = data.value ?? []
  } finally {
    specialLoading.value = false
  }
}

async function submitSpecial() {
  specialError.value = null
  specialSubmitting.value = true
  try {
    if (editingSpecial.value) {
      const updated = await api.put<SpecialHoursDto>(
        `/course-hours/special/${editingSpecial.value.id}`,
        { date: specialForm.date, openTime: specialForm.openTime, closeTime: specialForm.closeTime, reason: specialForm.reason },
        authHeaders.value
      )
      const idx = specialHours.value.findIndex(s => s.id === editingSpecial.value!.id)
      if (idx >= 0) specialHours.value[idx] = updated
    } else {
      const created = await api.post<SpecialHoursDto>(
        '/course-hours/special',
        { date: specialForm.date, openTime: specialForm.openTime, closeTime: specialForm.closeTime, reason: specialForm.reason },
        authHeaders.value
      )
      specialHours.value.push(created)
      specialHours.value.sort((a, b) => a.date.localeCompare(b.date))
    }
    cancelSpecialEdit()
  } catch (e: any) {
    specialError.value = e?.data?.detail ?? e?.data?.title ?? 'Failed to save.'
  } finally {
    specialSubmitting.value = false
  }
}

function editSpecial(row: SpecialHoursDto) {
  editingSpecial.value = row
  specialForm.date = row.date
  specialForm.openTime = row.openTime
  specialForm.closeTime = row.closeTime
  specialForm.reason = row.reason
}

function cancelSpecialEdit() {
  editingSpecial.value = null
  specialForm.date = ''
  specialForm.openTime = '07:00'
  specialForm.closeTime = '19:00'
  specialForm.reason = ''
  specialError.value = null
}

async function deleteSpecial(id: string) {
  if (!confirm('Remove these special hours?')) return
  try {
    await api.del(`/course-hours/special/${id}`, authHeaders.value)
    specialHours.value = specialHours.value.filter(s => s.id !== id)
  } catch {
    alert('Could not remove special hours.')
  }
}

watch(activeTab, tab => {
  if (tab === 'schedule' && schedule.value.length === 0) loadSchedule()
  if (tab === 'holidays' && holidays.value.length === 0) loadHolidays()
  if (tab === 'special' && specialHours.value.length === 0) loadSpecial()
}, { immediate: true })
</script>

<template>
  <div class="max-w-4xl mx-auto px-4 py-8">
    <h1 class="font-display text-2xl font-bold mb-1" style="color: var(--color-accent);">Hours &amp; Availability</h1>
    <p class="text-sm mb-6" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">
      Configure weekly operating hours, holidays, and date-specific overrides.
      Tee time slots are auto-generated each day for the next 7 days.
    </p>

    <!-- Tabs -->
    <div class="flex gap-1 border-b mb-6" style="border-color: color-mix(in srgb, var(--color-primary) 20%, transparent);">
      <button
        v-for="tab in (['schedule', 'holidays', 'special'] as const)"
        :key="tab"
        class="px-4 py-2 text-sm font-medium -mb-px border-b-2 transition-colors capitalize"
        :style="activeTab === tab
          ? 'border-color: var(--color-primary); color: var(--color-primary);'
          : 'border-color: transparent; color: color-mix(in srgb, var(--color-text) 50%, transparent);'"
        @click="activeTab = tab"
      >
        {{ tab === 'schedule' ? 'Weekly Schedule' : tab === 'holidays' ? 'Holidays' : 'Special Hours' }}
      </button>
    </div>

    <!-- Weekly Schedule Tab -->
    <div v-show="activeTab === 'schedule'">
      <p class="text-xs mb-4" style="color: color-mix(in srgb, var(--color-text) 50%, transparent);">
        Changes take effect the next time slots are auto-generated. Existing slots are not affected.
      </p>
      <div v-if="scheduleError" class="mb-4 bg-red-50 border border-red-200 rounded p-3 text-red-700 text-sm">{{ scheduleError }}</div>
      <div v-if="scheduleLoading" class="text-sm" style="color: color-mix(in srgb, var(--color-text) 50%, transparent);">Loading…</div>
      <div v-else class="flex flex-col gap-3">
        <div
          v-for="row in schedule"
          :key="row.dayOfWeek"
          class="bg-surface rounded-lg border p-4"
          style="border-color: color-mix(in srgb, var(--color-primary) 15%, transparent);"
        >
          <div class="flex flex-wrap items-center gap-4">
            <!-- Day name + toggle -->
            <div class="w-28 flex items-center gap-2 flex-shrink-0">
              <button
                type="button"
                class="relative inline-flex h-5 w-9 items-center rounded-full transition-colors flex-shrink-0"
                :style="row.isOpen ? 'background-color: var(--color-primary);' : 'background-color: #d1d5db;'"
                @click="row.isOpen = !row.isOpen"
              >
                <span
                  class="inline-block h-3.5 w-3.5 transform rounded-full bg-white transition-transform"
                  :class="row.isOpen ? 'translate-x-4' : 'translate-x-0.5'"
                />
              </button>
              <span class="text-sm font-semibold" style="color: var(--color-text);">{{ row.dayName }}</span>
            </div>

            <template v-if="row.isOpen">
              <div class="flex items-center gap-2">
                <label class="text-xs" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Open</label>
                <input v-model="row.openTime" type="time" class="border rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
              </div>
              <div class="flex items-center gap-2">
                <label class="text-xs" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Close</label>
                <input v-model="row.closeTime" type="time" class="border rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
              </div>
              <div class="flex items-center gap-2">
                <label class="text-xs" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Every</label>
                <select v-model.number="row.intervalMinutes" class="border rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
                  <option :value="5">5 min</option>
                  <option :value="8">8 min</option>
                  <option :value="10">10 min</option>
                  <option :value="12">12 min</option>
                  <option :value="15">15 min</option>
                  <option :value="20">20 min</option>
                </select>
              </div>
              <div class="flex items-center gap-2">
                <label class="text-xs" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Max players</label>
                <input v-model.number="row.maxPlayers" type="number" min="1" max="8" class="border rounded px-2 py-1 text-sm w-16 focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
              </div>
            </template>
            <span v-else class="text-sm italic" style="color: color-mix(in srgb, var(--color-text) 40%, transparent);">Closed</span>

            <button
              class="ml-auto px-3 py-1.5 text-sm font-medium text-white rounded hover:opacity-90 transition-opacity disabled:opacity-50"
              style="background-color: var(--color-primary);"
              :disabled="savingDay === row.dayOfWeek"
              @click="saveDay(row)"
            >
              {{ savingDay === row.dayOfWeek ? 'Saving…' : 'Save' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Holidays Tab -->
    <div v-show="activeTab === 'holidays'">
      <p class="text-xs mb-4" style="color: color-mix(in srgb, var(--color-text) 50%, transparent);">
        Dates the course is fully closed. No tee times will be generated for these days.
      </p>

      <!-- Add form -->
      <div class="bg-surface border rounded-lg p-4 mb-5" style="border-color: color-mix(in srgb, var(--color-primary) 15%, transparent);">
        <h3 class="text-sm font-semibold mb-3" style="color: var(--color-text);">Add Holiday</h3>
        <div v-if="holidayError" class="mb-3 bg-red-50 border border-red-200 rounded p-2 text-red-700 text-xs">{{ holidayError }}</div>
        <div class="flex flex-wrap gap-3 items-end">
          <div>
            <label class="block text-xs mb-1" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Date</label>
            <input v-model="holidayForm.date" type="date" class="border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
          </div>
          <div class="flex-1 min-w-40">
            <label class="block text-xs mb-1" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Reason</label>
            <input v-model="holidayForm.reason" type="text" placeholder="e.g. Labour Day" class="w-full border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
          </div>
          <button
            class="px-4 py-1.5 text-sm font-medium text-white rounded hover:opacity-90 transition-opacity disabled:opacity-50"
            style="background-color: var(--color-primary);"
            :disabled="holidaySubmitting || !holidayForm.date || !holidayForm.reason"
            @click="addHoliday"
          >
            {{ holidaySubmitting ? 'Adding…' : 'Add Holiday' }}
          </button>
        </div>
      </div>

      <!-- List -->
      <div v-if="holidaysLoading" class="text-sm" style="color: color-mix(in srgb, var(--color-text) 50%, transparent);">Loading…</div>
      <div v-else-if="holidays.length === 0" class="text-sm italic" style="color: color-mix(in srgb, var(--color-text) 40%, transparent);">No holidays configured.</div>
      <div v-else class="flex flex-col gap-2">
        <div
          v-for="h in holidays"
          :key="h.id"
          class="flex items-center gap-3 bg-surface border rounded-lg px-4 py-3"
          style="border-color: color-mix(in srgb, var(--color-primary) 15%, transparent);"
        >
          <span class="text-sm font-mono font-medium" style="color: var(--color-primary);">{{ h.date }}</span>
          <span class="text-sm flex-1" style="color: var(--color-text);">{{ h.reason }}</span>
          <button class="text-xs text-red-500 hover:text-red-700" @click="deleteHoliday(h.id)">Remove</button>
        </div>
      </div>
    </div>

    <!-- Special Hours Tab -->
    <div v-show="activeTab === 'special'">
      <p class="text-xs mb-4" style="color: color-mix(in srgb, var(--color-text) 50%, transparent);">
        Override hours for a specific date — e.g. tournament days, early closures, or extended weekend hours.
      </p>

      <!-- Form -->
      <div class="bg-surface border rounded-lg p-4 mb-5" style="border-color: color-mix(in srgb, var(--color-primary) 15%, transparent);">
        <h3 class="text-sm font-semibold mb-3" style="color: var(--color-text);">
          {{ editingSpecial ? 'Edit Special Hours' : 'Add Special Hours' }}
        </h3>
        <div v-if="specialError" class="mb-3 bg-red-50 border border-red-200 rounded p-2 text-red-700 text-xs">{{ specialError }}</div>
        <div class="flex flex-wrap gap-3 items-end">
          <div>
            <label class="block text-xs mb-1" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Date</label>
            <input v-model="specialForm.date" type="date" :disabled="!!editingSpecial" class="border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:opacity-50" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
          </div>
          <div>
            <label class="block text-xs mb-1" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Open</label>
            <input v-model="specialForm.openTime" type="time" class="border rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
          </div>
          <div>
            <label class="block text-xs mb-1" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Close</label>
            <input v-model="specialForm.closeTime" type="time" class="border rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
          </div>
          <div class="flex-1 min-w-40">
            <label class="block text-xs mb-1" style="color: color-mix(in srgb, var(--color-text) 55%, transparent);">Reason</label>
            <input v-model="specialForm.reason" type="text" placeholder="e.g. Tournament — opens at 1pm" class="w-full border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" style="border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);">
          </div>
          <div class="flex gap-2">
            <button
              class="px-4 py-1.5 text-sm font-medium text-white rounded hover:opacity-90 transition-opacity disabled:opacity-50"
              style="background-color: var(--color-primary);"
              :disabled="specialSubmitting || !specialForm.date || !specialForm.reason"
              @click="submitSpecial"
            >
              {{ specialSubmitting ? 'Saving…' : editingSpecial ? 'Update' : 'Add' }}
            </button>
            <button v-if="editingSpecial" class="px-3 py-1.5 text-sm border rounded hover:bg-gray-50" @click="cancelSpecialEdit">Cancel</button>
          </div>
        </div>
      </div>

      <!-- List -->
      <div v-if="specialLoading" class="text-sm" style="color: color-mix(in srgb, var(--color-text) 50%, transparent);">Loading…</div>
      <div v-else-if="specialHours.length === 0" class="text-sm italic" style="color: color-mix(in srgb, var(--color-text) 40%, transparent);">No special hours configured.</div>
      <div v-else class="flex flex-col gap-2">
        <div
          v-for="s in specialHours"
          :key="s.id"
          class="flex items-center gap-3 bg-surface border rounded-lg px-4 py-3"
          style="border-color: color-mix(in srgb, var(--color-primary) 15%, transparent);"
        >
          <span class="text-sm font-mono font-medium" style="color: var(--color-primary);">{{ s.date }}</span>
          <span class="text-sm" style="color: color-mix(in srgb, var(--color-text) 65%, transparent);">{{ s.openTime }} – {{ s.closeTime }}</span>
          <span class="text-sm flex-1" style="color: var(--color-text);">{{ s.reason }}</span>
          <button class="text-xs hover:underline mr-2" style="color: var(--color-primary);" @click="editSpecial(s)">Edit</button>
          <button class="text-xs text-red-500 hover:text-red-700" @click="deleteSpecial(s.id)">Remove</button>
        </div>
      </div>
    </div>
  </div>
</template>
