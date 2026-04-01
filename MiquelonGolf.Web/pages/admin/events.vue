<!-- MiquelonGolf.Web/pages/admin/events.vue -->
<script setup lang="ts">
import type { EventDto } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })

useSeoMeta({ title: 'Events — Admin' })

const api = useApi()
const { authHeaders } = useAuth()

// --- List ---
const events = ref<EventDto[]>([])
const loading = ref(false)
const loadError = ref<string | null>(null)

async function loadEvents() {
  loading.value = true
  loadError.value = null
  try {
    events.value = await $fetch<EventDto[]>(api.url('/events'), { headers: authHeaders.value })
  } catch {
    loadError.value = 'Could not load events.'
  } finally {
    loading.value = false
  }
}

onMounted(loadEvents)

// --- Add / Edit form ---
type FormMode = 'add' | 'edit' | null
const formMode = ref<FormMode>(null)
const editingId = ref<string | null>(null)

const form = reactive({
  title: '',
  description: '',
  eventDate: '',
  startTime: '',
  isPublic: true,
  category: 'Other',
  imageUrl: '',
})

function openAdd() {
  Object.assign(form, {
    title: '', description: '', eventDate: '', startTime: '',
    isPublic: true, category: 'Other', imageUrl: '',
  })
  editingId.value = null
  formMode.value = 'add'
}

function openEdit(e: EventDto) {
  Object.assign(form, {
    title: e.title, description: e.description,
    eventDate: e.eventDate, startTime: e.startTime ?? '',
    isPublic: e.isPublic, category: e.category,
    imageUrl: e.imageUrl ?? '',
  })
  editingId.value = e.id
  formMode.value = 'edit'
}

function closeForm() {
  formMode.value = null
  editingId.value = null
  submitError.value = null
}

const submitting = ref(false)
const submitError = ref<string | null>(null)

async function submitForm() {
  submitting.value = true
  submitError.value = null
  const body = {
    title: form.title,
    description: form.description,
    eventDate: form.eventDate,
    startTime: form.startTime || null,
    isPublic: form.isPublic,
    category: form.category,
    imageUrl: form.imageUrl || null,
  }
  try {
    if (formMode.value === 'add') {
      await $fetch(api.url('/events'), { method: 'POST', headers: authHeaders.value, body })
    } else {
      await $fetch(api.url(`/events/${editingId.value}`), { method: 'PUT', headers: authHeaders.value, body })
    }
    closeForm()
    await loadEvents()
  } catch (e: any) {
    submitError.value = e?.data?.title ?? e?.data?.detail ?? 'Could not save event.'
  } finally {
    submitting.value = false
  }
}

// --- Delete ---
const deletingId = ref<string | null>(null)

async function deleteEvent(id: string, title: string) {
  if (!confirm(`Delete "${title}"?`)) return
  deletingId.value = id
  try {
    await $fetch(api.url(`/events/${id}`), { method: 'DELETE', headers: authHeaders.value })
    await loadEvents()
  } catch (e: any) {
    alert(e?.data?.title ?? e?.data?.detail ?? 'Could not delete event. Please try again.')
  } finally {
    deletingId.value = null
  }
}

const categories = ['Tournament', 'SocialNight', 'LadiesNight', 'MensNight', 'Other']
</script>

<template>
  <div class="p-6 max-w-5xl">
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-display text-3xl font-bold text-accent">Events</h1>
      <button
        class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
        @click="formMode ? closeForm() : openAdd()"
      >
        {{ formMode ? 'Cancel' : '+ Add Event' }}
      </button>
    </div>

    <!-- Add / Edit form -->
    <div v-if="formMode" class="bg-surface rounded-lg shadow-sm p-5 mb-6 border border-primary/20">
      <h2 class="font-semibold text-text mb-4">{{ formMode === 'add' ? 'Add New Event' : 'Edit Event' }}</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-text/60 mb-1" for="ev-title">Title</label>
          <input id="ev-title" v-model="form.title" type="text" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-text/60 mb-1" for="ev-desc">Description</label>
          <textarea id="ev-desc" v-model="form.description" rows="3" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"></textarea>
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="ev-date">Event Date</label>
          <input id="ev-date" v-model="form.eventDate" type="date" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="ev-time">Start Time (optional)</label>
          <input id="ev-time" v-model="form.startTime" type="time" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="ev-cat">Category</label>
          <select id="ev-cat" v-model="form.category" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white">
            <option v-for="c in categories" :key="c" :value="c">{{ c }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="ev-image">Image URL (optional)</label>
          <input id="ev-image" v-model="form.imageUrl" type="url" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div class="sm:col-span-2">
          <label class="flex items-center gap-2 text-sm cursor-pointer">
            <input v-model="form.isPublic" type="checkbox" class="w-4 h-4 accent-primary">
            Public event (visible to all visitors — uncheck for members-only)
          </label>
        </div>
      </div>
      <div v-if="submitError" class="text-red-600 text-sm mb-3">{{ submitError }}</div>
      <button
        :disabled="submitting"
        class="px-5 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
        @click="submitForm"
      >
        {{ submitting ? 'Saving…' : (formMode === 'add' ? 'Add Event' : 'Save Changes') }}
      </button>
    </div>

    <!-- Events table -->
    <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
      <div v-if="loading" class="p-8 text-center text-text/40 text-sm">Loading…</div>
      <div v-else-if="loadError" class="p-4 text-red-600 text-sm">{{ loadError }}</div>
      <div v-else-if="events.length === 0" class="p-8 text-center text-text/40 text-sm">No events yet.</div>
      <table v-else class="w-full text-sm">
        <thead class="bg-gray-50 border-b border-gray-100">
          <tr>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Event</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide hidden md:table-cell">Date</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide hidden md:table-cell">Category</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Visibility</th>
            <th class="text-right px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          <tr v-for="e in events" :key="e.id">
            <td class="px-4 py-3">
              <div class="font-medium text-text">{{ e.title }}</div>
              <div class="text-xs text-text/50 line-clamp-1">{{ e.description }}</div>
            </td>
            <td class="px-4 py-3 text-text/60 hidden md:table-cell">
              <div>{{ e.eventDate }}</div>
              <div v-if="e.startTime" class="text-xs">{{ e.startTime }}</div>
            </td>
            <td class="px-4 py-3 hidden md:table-cell">
              <span class="text-xs font-semibold bg-primary/10 text-primary px-2 py-0.5 rounded">{{ e.category }}</span>
            </td>
            <td class="px-4 py-3 text-center">
              <span
                :class="[
                  'text-xs font-semibold px-2 py-0.5 rounded',
                  e.isPublic ? 'bg-green-100 text-green-700' : 'bg-amber-100 text-amber-700'
                ]"
              >{{ e.isPublic ? 'Public' : 'Members' }}</span>
            </td>
            <td class="px-4 py-3 text-right">
              <div class="flex justify-end gap-3">
                <button class="text-xs text-primary hover:underline" @click="openEdit(e)">Edit</button>
                <button
                  :disabled="deletingId === e.id"
                  class="text-xs text-red-600 hover:underline disabled:opacity-50"
                  @click="deleteEvent(e.id, e.title)"
                >
                  Delete
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
