<!-- MiquelonGolf.Web/pages/admin/content.vue -->
<script setup lang="ts">
import type { AnnouncementDto, SiteContentDto, HoleDto } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })

useSeoMeta({ title: 'Content — Admin' })

const api = useApi()
const { authHeaders } = useAuth()

// ─── Tab state ───────────────────────────────────────────────────────────
type Tab = 'announcements' | 'content' | 'holes'
const activeTab = ref<Tab>('announcements')

// ─── Announcements ───────────────────────────────────────────────────────
const announcements = ref<AnnouncementDto[]>([])
const annLoading = ref(false)
const annError = ref<string | null>(null)

async function loadAnnouncements() {
  annLoading.value = true
  annError.value = null
  try {
    announcements.value = await $fetch<AnnouncementDto[]>(api.url('/announcements'), { headers: authHeaders.value })
  } catch {
    annError.value = 'Could not load announcements.'
  } finally {
    annLoading.value = false
  }
}

type AnnFormMode = 'add' | 'edit' | null
const annFormMode = ref<AnnFormMode>(null)
const annEditingId = ref<string | null>(null)

const annForm = reactive({
  message: '',
  isActive: true,
  type: 'General',
  expiresAt: '',
})

function openAnnAdd() {
  Object.assign(annForm, { message: '', isActive: true, type: 'General', expiresAt: '' })
  annEditingId.value = null
  annFormMode.value = 'add'
}

function openAnnEdit(a: AnnouncementDto) {
  Object.assign(annForm, {
    message: a.message,
    isActive: a.isActive,
    type: a.type,
    expiresAt: a.expiresAt ? a.expiresAt.slice(0, 16) : '',
  })
  annEditingId.value = a.id
  annFormMode.value = 'edit'
}

function closeAnnForm() {
  annFormMode.value = null
  annEditingId.value = null
  annSubmitError.value = null
}

const annSubmitting = ref(false)
const annSubmitError = ref<string | null>(null)

async function submitAnnForm() {
  annSubmitting.value = true
  annSubmitError.value = null
  const body = {
    message: annForm.message,
    isActive: annForm.isActive,
    type: annForm.type,
    expiresAt: annForm.expiresAt || null,
  }
  try {
    if (annFormMode.value === 'add') {
      await $fetch(api.url('/announcements'), { method: 'POST', headers: authHeaders.value, body })
    } else {
      await $fetch(api.url(`/announcements/${annEditingId.value}`), { method: 'PUT', headers: authHeaders.value, body })
    }
    closeAnnForm()
    await loadAnnouncements()
  } catch (e: any) {
    annSubmitError.value = e?.data?.title ?? e?.data?.detail ?? 'Could not save announcement.'
  } finally {
    annSubmitting.value = false
  }
}

const annDeletingId = ref<string | null>(null)

async function deleteAnnouncement(id: string) {
  if (!confirm('Delete this announcement?')) return
  annDeletingId.value = id
  try {
    await $fetch(api.url(`/announcements/${id}`), { method: 'DELETE', headers: authHeaders.value })
    await loadAnnouncements()
  } catch (e: any) {
    alert(e?.data?.title ?? e?.data?.detail ?? 'Could not delete announcement.')
  } finally {
    annDeletingId.value = null
  }
}

const annTypes = ['General', 'CourseConditions', 'Closure']

// ─── Site Content ─────────────────────────────────────────────────────────
const siteContent = ref<SiteContentDto[]>([])
const contentLoading = ref(false)
const contentError = ref<string | null>(null)
const contentEdits = ref<Record<string, string>>({})
const contentSaving = ref<Record<string, boolean>>({})
const contentSaveError = ref<Record<string, string | null>>({})

async function loadSiteContent() {
  contentLoading.value = true
  contentError.value = null
  try {
    siteContent.value = await $fetch<SiteContentDto[]>(api.url('/site-content'), { headers: authHeaders.value })
    for (const item of siteContent.value) {
      contentEdits.value[item.key] = item.value
    }
  } catch {
    contentError.value = 'Could not load site content.'
  } finally {
    contentLoading.value = false
  }
}

async function saveSiteContent(key: string) {
  contentSaving.value[key] = true
  contentSaveError.value[key] = null
  try {
    await $fetch(api.url(`/site-content/${key}`), {
      method: 'PUT',
      headers: authHeaders.value,
      body: { value: contentEdits.value[key] },
    })
    const item = siteContent.value.find(c => c.key === key)
    if (item) item.value = contentEdits.value[key]
  } catch (e: any) {
    contentSaveError.value[key] = e?.data?.title ?? e?.data?.detail ?? 'Save failed.'
  } finally {
    contentSaving.value[key] = false
  }
}

const contentGroups = computed(() => {
  const groups: Record<string, SiteContentDto[]> = {}
  for (const item of siteContent.value) {
    const prefix = item.key.includes('.') ? item.key.split('.')[0] : 'other'
    if (!groups[prefix]) groups[prefix] = []
    groups[prefix].push(item)
  }
  return groups
})

// ─── Holes ─────────────────────────────────────────────────────────────────
const holes = ref<HoleDto[]>([])
const holesLoading = ref(false)
const holesError = ref<string | null>(null)
const expandedHole = ref<number | null>(null)
const holeEdits = ref<Record<number, HoleDto>>({})
const holeSaving = ref<number | null>(null)
const holeSaveError = ref<string | null>(null)

async function loadHoles() {
  holesLoading.value = true
  holesError.value = null
  try {
    holes.value = await $fetch<HoleDto[]>(api.url('/holes'))
  } catch {
    holesError.value = 'Could not load holes.'
  } finally {
    holesLoading.value = false
  }
}

function toggleHole(hole: HoleDto) {
  if (expandedHole.value === hole.holeNumber) {
    expandedHole.value = null
    return
  }
  expandedHole.value = hole.holeNumber
  holeEdits.value[hole.holeNumber] = { ...hole }
  holeSaveError.value = null
}

async function saveHole(holeNumber: number) {
  holeSaving.value = holeNumber
  holeSaveError.value = null
  const edit = holeEdits.value[holeNumber]
  try {
    await $fetch(api.url(`/holes/${holeNumber}`), {
      method: 'PUT',
      headers: authHeaders.value,
      body: {
        par: edit.par,
        handicap: edit.handicap,
        yardageBlue: edit.yardageBlue,
        yardageWhite: edit.yardageWhite,
        yardageRed: edit.yardageRed,
        description: edit.description,
        imageUrl: edit.imageUrl || null,
        diagramUrl: edit.diagramUrl || null,
      },
    })
    const idx = holes.value.findIndex(h => h.holeNumber === holeNumber)
    if (idx !== -1) holes.value[idx] = { ...holes.value[idx], ...edit }
    expandedHole.value = null
  } catch (e: any) {
    holeSaveError.value = e?.data?.title ?? e?.data?.detail ?? 'Could not save hole.'
  } finally {
    holeSaving.value = null
  }
}

// ─── Load on tab switch ────────────────────────────────────────────────────
watch(activeTab, (tab) => {
  if (tab === 'announcements' && announcements.value.length === 0) loadAnnouncements()
  if (tab === 'content' && siteContent.value.length === 0) loadSiteContent()
  if (tab === 'holes' && holes.value.length === 0) loadHoles()
}, { immediate: true })
</script>

<template>
  <div class="p-6 max-w-5xl">
    <h1 class="font-display text-3xl font-bold text-accent mb-6">Content</h1>

    <!-- Tab nav -->
    <div class="flex gap-1 border-b border-gray-200 mb-6">
      <button
        v-for="tab in ([
          { key: 'announcements', label: 'Announcements' },
          { key: 'content',       label: 'Site Content' },
          { key: 'holes',         label: 'Hole Descriptions' },
        ] as const)"
        :key="tab.key"
        :class="[
          'px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors',
          activeTab === tab.key
            ? 'border-primary text-primary'
            : 'border-transparent text-text/50 hover:text-text'
        ]"
        @click="activeTab = tab.key"
      >
        {{ tab.label }}
      </button>
    </div>

    <!-- ── Announcements tab ───────────────────────────────────────────────── -->
    <div v-show="activeTab === 'announcements'">
      <div class="flex items-center justify-between mb-4">
        <h2 class="font-semibold text-text">Announcements</h2>
        <button
          class="px-3 py-1.5 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
          @click="annFormMode ? closeAnnForm() : openAnnAdd()"
        >
          {{ annFormMode ? 'Cancel' : '+ Add' }}
        </button>
      </div>

      <!-- Announcement form -->
      <div v-if="annFormMode" class="bg-surface rounded-lg shadow-sm p-5 mb-4 border border-primary/20">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
          <div class="sm:col-span-2">
            <label class="block text-xs font-medium text-text/60 mb-1" for="ann-msg">Message</label>
            <textarea id="ann-msg" v-model="annForm.message" rows="2" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"></textarea>
          </div>
          <div>
            <label class="block text-xs font-medium text-text/60 mb-1" for="ann-type">Type</label>
            <select id="ann-type" v-model="annForm.type" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white">
              <option v-for="t in annTypes" :key="t" :value="t">{{ t }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-medium text-text/60 mb-1" for="ann-expires">Expires (optional)</label>
            <input id="ann-expires" v-model="annForm.expiresAt" type="datetime-local" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
          </div>
          <div class="sm:col-span-2">
            <label class="flex items-center gap-2 text-sm cursor-pointer">
              <input v-model="annForm.isActive" type="checkbox" class="w-4 h-4 accent-primary">
              Active (shown on the public site immediately)
            </label>
          </div>
        </div>
        <div v-if="annSubmitError" class="text-red-600 text-sm mb-3">{{ annSubmitError }}</div>
        <button
          :disabled="annSubmitting"
          class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
          @click="submitAnnForm"
        >
          {{ annSubmitting ? 'Saving…' : (annFormMode === 'add' ? 'Add Announcement' : 'Save Changes') }}
        </button>
      </div>

      <!-- Announcements list -->
      <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
        <div v-if="annLoading" class="p-6 text-center text-text/40 text-sm">Loading…</div>
        <div v-else-if="annError" class="p-4 text-red-600 text-sm">{{ annError }}</div>
        <div v-else-if="announcements.length === 0" class="p-6 text-center text-text/40 text-sm">No announcements.</div>
        <div v-else class="divide-y divide-gray-50">
          <div v-for="a in announcements" :key="a.id" class="px-5 py-3 flex items-start justify-between gap-4 text-sm">
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-0.5">
                <span
                  :class="[
                    'text-xs font-semibold px-1.5 py-0.5 rounded',
                    a.type === 'Closure' ? 'bg-red-100 text-red-700' : a.type === 'CourseConditions' ? 'bg-green-100 text-green-700' : 'bg-amber-100 text-amber-700'
                  ]"
                >{{ a.type }}</span>
                <span v-if="!a.isActive" class="text-xs text-text/40">(inactive)</span>
              </div>
              <p class="text-text/80 line-clamp-2">{{ a.message }}</p>
              <p v-if="a.expiresAt" class="text-xs text-text/40 mt-0.5">Expires {{ a.expiresAt }}</p>
            </div>
            <div class="flex gap-3 shrink-0">
              <button class="text-xs text-primary hover:underline" @click="openAnnEdit(a)">Edit</button>
              <button
                :disabled="annDeletingId === a.id"
                class="text-xs text-red-600 hover:underline disabled:opacity-50"
                @click="deleteAnnouncement(a.id)"
              >Delete</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Site Content tab ───────────────────────────────────────────────── -->
    <div v-show="activeTab === 'content'">
      <div v-if="contentLoading" class="p-8 text-center text-text/40 text-sm">Loading…</div>
      <div v-else-if="contentError" class="p-4 text-red-600 text-sm">{{ contentError }}</div>
      <div v-else class="space-y-6">
        <div v-for="(items, group) in contentGroups" :key="group" class="bg-surface rounded-lg shadow-sm p-5">
          <h3 class="font-semibold text-text capitalize mb-4">{{ group }}</h3>
          <div class="space-y-3">
            <div v-for="item in items" :key="item.key" class="flex items-start gap-3">
              <label class="w-40 shrink-0 text-xs font-medium text-text/60 pt-2">{{ item.key }}</label>
              <div class="flex-1">
                <textarea
                  v-model="contentEdits[item.key]"
                  rows="2"
                  class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 resize-y"
                ></textarea>
                <div v-if="contentSaveError[item.key]" class="text-red-600 text-xs mt-1">{{ contentSaveError[item.key] }}</div>
              </div>
              <button
                :disabled="contentSaving[item.key]"
                class="mt-1 px-3 py-1.5 bg-primary text-white text-xs font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity shrink-0"
                @click="saveSiteContent(item.key)"
              >
                {{ contentSaving[item.key] ? '…' : 'Save' }}
              </button>
            </div>
          </div>
        </div>
        <div v-if="Object.keys(contentGroups).length === 0" class="p-8 text-center text-text/40 text-sm">No site content found.</div>
      </div>
    </div>

    <!-- ── Holes tab ──────────────────────────────────────────────────────── -->
    <div v-show="activeTab === 'holes'">
      <div v-if="holesLoading" class="p-8 text-center text-text/40 text-sm">Loading…</div>
      <div v-else-if="holesError" class="p-4 text-red-600 text-sm">{{ holesError }}</div>
      <div v-else class="bg-surface rounded-lg shadow-sm overflow-hidden">
        <div v-for="hole in holes" :key="hole.holeNumber" class="border-b border-gray-50 last:border-b-0">
          <!-- Hole row (click to expand) -->
          <button
            class="w-full flex items-center justify-between px-5 py-3 hover:bg-gray-50 transition-colors text-left"
            @click="toggleHole(hole)"
          >
            <div class="flex items-center gap-4 text-sm">
              <span class="w-10 font-semibold text-text">Hole {{ hole.holeNumber }}</span>
              <span class="text-text/50">Par {{ hole.par }}</span>
              <span class="text-text/50 hidden sm:inline">Blue {{ hole.yardageBlue }}y / White {{ hole.yardageWhite }}y / Red {{ hole.yardageRed }}y</span>
            </div>
            <span class="text-xs text-text/40">{{ expandedHole === hole.holeNumber ? '▲ Close' : '▼ Edit' }}</span>
          </button>

          <!-- Edit form (expanded) -->
          <div v-if="expandedHole === hole.holeNumber && holeEdits[hole.holeNumber]" class="px-5 pb-5 border-t border-gray-100 bg-gray-50/50">
            <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-4 mb-4">
              <div>
                <label class="block text-xs font-medium text-text/60 mb-1" :for="`h${hole.holeNumber}-par`">Par</label>
                <input :id="`h${hole.holeNumber}-par`" v-model.number="holeEdits[hole.holeNumber].par" type="number" min="3" max="5" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
              </div>
              <div>
                <label class="block text-xs font-medium text-text/60 mb-1" :for="`h${hole.holeNumber}-hcp`">Handicap</label>
                <input :id="`h${hole.holeNumber}-hcp`" v-model.number="holeEdits[hole.holeNumber].handicap" type="number" min="1" max="18" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
              </div>
              <div>
                <label class="block text-xs font-medium text-text/60 mb-1" :for="`h${hole.holeNumber}-blue`">Blue Yds</label>
                <input :id="`h${hole.holeNumber}-blue`" v-model.number="holeEdits[hole.holeNumber].yardageBlue" type="number" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
              </div>
              <div>
                <label class="block text-xs font-medium text-text/60 mb-1" :for="`h${hole.holeNumber}-white`">White Yds</label>
                <input :id="`h${hole.holeNumber}-white`" v-model.number="holeEdits[hole.holeNumber].yardageWhite" type="number" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
              </div>
              <div>
                <label class="block text-xs font-medium text-text/60 mb-1" :for="`h${hole.holeNumber}-red`">Red Yds</label>
                <input :id="`h${hole.holeNumber}-red`" v-model.number="holeEdits[hole.holeNumber].yardageRed" type="number" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
              </div>
              <div class="col-span-2 sm:col-span-3">
                <label class="block text-xs font-medium text-text/60 mb-1" :for="`h${hole.holeNumber}-img`">Image URL (optional)</label>
                <input :id="`h${hole.holeNumber}-img`" v-model="holeEdits[hole.holeNumber].imageUrl" type="url" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
              </div>
            </div>
            <div class="mb-4">
              <label class="block text-xs font-medium text-text/60 mb-1" :for="`h${hole.holeNumber}-desc`">Description / Strategy Tip</label>
              <textarea :id="`h${hole.holeNumber}-desc`" v-model="holeEdits[hole.holeNumber].description" rows="3" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"></textarea>
            </div>
            <div v-if="holeSaveError && holeSaving === null" class="text-red-600 text-sm mb-3">{{ holeSaveError }}</div>
            <button
              :disabled="holeSaving === hole.holeNumber"
              class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
              @click="saveHole(hole.holeNumber)"
            >
              {{ holeSaving === hole.holeNumber ? 'Saving…' : 'Save Hole' }}
            </button>
          </div>
        </div>
        <div v-if="holes.length === 0" class="p-8 text-center text-text/40 text-sm">No holes found.</div>
      </div>
    </div>
  </div>
</template>
