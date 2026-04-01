<!-- MiquelonGolf.Web/pages/admin/members.vue -->
<script setup lang="ts">
import type { MemberDto } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })

useSeoMeta({ title: 'Members — Admin' })

const api = useApi()
const { authHeaders } = useAuth()

// --- List ---
const members = ref<MemberDto[]>([])
const loading = ref(false)
const loadError = ref<string | null>(null)

async function loadMembers() {
  loading.value = true
  loadError.value = null
  try {
    members.value = await $fetch<MemberDto[]>(api.url('/members'), { headers: authHeaders.value })
  } catch {
    loadError.value = 'Could not load members.'
  } finally {
    loading.value = false
  }
}

onMounted(loadMembers)

// --- Add / Edit form ---
type FormMode = 'add' | 'edit' | null
const formMode = ref<FormMode>(null)
const editingId = ref<string | null>(null)

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  membershipType: 'Adult',
  memberSince: '',
  seasonYear: new Date().getFullYear(),
  purchaseDate: '',
  expiryDate: '',
  cartTrackage: false,
  seasonalCartRental: false,
})

function openAdd() {
  Object.assign(form, {
    firstName: '', lastName: '', email: '', phone: '',
    membershipType: 'Adult', memberSince: '',
    seasonYear: new Date().getFullYear(),
    purchaseDate: '', expiryDate: '',
    cartTrackage: false, seasonalCartRental: false,
  })
  editingId.value = null
  formMode.value = 'add'
}

function openEdit(m: MemberDto) {
  Object.assign(form, {
    firstName: m.firstName, lastName: m.lastName,
    email: m.email ?? '', phone: m.phone ?? '',
    membershipType: m.membershipType,
    memberSince: m.memberSince,
    seasonYear: m.seasonYear,
    purchaseDate: m.purchaseDate,
    expiryDate: m.expiryDate,
    cartTrackage: m.cartTrackage,
    seasonalCartRental: m.seasonalCartRental,
  })
  editingId.value = m.id
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
  try {
    if (formMode.value === 'add') {
      await $fetch(api.url('/members'), {
        method: 'POST',
        headers: authHeaders.value,
        body: {
          firstName: form.firstName, lastName: form.lastName,
          email: form.email || null, phone: form.phone || null,
          membershipType: form.membershipType,
          memberSince: form.memberSince,
          seasonYear: form.seasonYear,
          purchaseDate: form.purchaseDate, expiryDate: form.expiryDate,
          cartTrackage: form.cartTrackage, seasonalCartRental: form.seasonalCartRental,
        },
      })
    } else {
      await $fetch(api.url(`/members/${editingId.value}`), {
        method: 'PUT',
        headers: authHeaders.value,
        body: {
          firstName: form.firstName, lastName: form.lastName,
          email: form.email || null, phone: form.phone || null,
          membershipType: form.membershipType,
          seasonYear: form.seasonYear,
          purchaseDate: form.purchaseDate, expiryDate: form.expiryDate,
          cartTrackage: form.cartTrackage, seasonalCartRental: form.seasonalCartRental,
        },
      })
    }
    closeForm()
    await loadMembers()
  } catch (e: any) {
    submitError.value = e?.data?.title ?? e?.data?.detail ?? 'Could not save member.'
  } finally {
    submitting.value = false
  }
}

// --- Delete ---
const deletingId = ref<string | null>(null)

async function deleteMember(id: string, name: string) {
  if (!confirm(`Remove ${name} from the members list?`)) return
  deletingId.value = id
  try {
    await $fetch(api.url(`/members/${id}`), { method: 'DELETE', headers: authHeaders.value })
    await loadMembers()
  } catch (e: any) {
    alert(e?.data?.title ?? e?.data?.detail ?? 'Could not remove member. Please try again.')
  } finally {
    deletingId.value = null
  }
}

const membershipTypes = ['Adult', 'Senior', 'Junior', 'Family', 'YoungAdult', 'SeniorCouple']
</script>

<template>
  <div class="p-6 max-w-6xl">
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-display text-3xl font-bold text-accent">Members</h1>
      <button
        class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
        @click="formMode ? closeForm() : openAdd()"
      >
        {{ formMode ? 'Cancel' : '+ Add Member' }}
      </button>
    </div>

    <!-- Add / Edit form -->
    <div v-if="formMode" class="bg-surface rounded-lg shadow-sm p-5 mb-6 border border-primary/20">
      <h2 class="font-semibold text-text mb-4">{{ formMode === 'add' ? 'Add New Member' : 'Edit Member' }}</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-first">First Name</label>
          <input id="m-first" v-model="form.firstName" type="text" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-last">Last Name</label>
          <input id="m-last" v-model="form.lastName" type="text" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-email">Email</label>
          <input id="m-email" v-model="form.email" type="email" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-phone">Phone</label>
          <input id="m-phone" v-model="form.phone" type="tel" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-type">Membership Type</label>
          <select id="m-type" v-model="form.membershipType" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white">
            <option v-for="t in membershipTypes" :key="t" :value="t">{{ t }}</option>
          </select>
        </div>
        <div v-if="formMode === 'add'">
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-since">Member Since</label>
          <input id="m-since" v-model="form.memberSince" type="date" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-season">Season Year</label>
          <input id="m-season" v-model.number="form.seasonYear" type="number" min="2020" max="2099" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-purchase">Purchase Date</label>
          <input id="m-purchase" v-model="form.purchaseDate" type="date" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="m-expiry">Expiry Date</label>
          <input id="m-expiry" v-model="form.expiryDate" type="date" required class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div class="flex items-center gap-4 sm:col-span-2 lg:col-span-3">
          <label class="flex items-center gap-2 text-sm cursor-pointer">
            <input v-model="form.cartTrackage" type="checkbox" class="w-4 h-4 accent-primary">
            Cart Trackage
          </label>
          <label class="flex items-center gap-2 text-sm cursor-pointer">
            <input v-model="form.seasonalCartRental" type="checkbox" class="w-4 h-4 accent-primary">
            Seasonal Cart Rental
          </label>
        </div>
      </div>
      <div v-if="submitError" class="text-red-600 text-sm mb-3">{{ submitError }}</div>
      <button
        :disabled="submitting"
        class="px-5 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
        @click="submitForm"
      >
        {{ submitting ? 'Saving…' : (formMode === 'add' ? 'Add Member' : 'Save Changes') }}
      </button>
    </div>

    <!-- Members table -->
    <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
      <div v-if="loading" class="p-8 text-center text-text/40 text-sm">Loading…</div>
      <div v-else-if="loadError" class="p-4 text-red-600 text-sm">{{ loadError }}</div>
      <div v-else-if="members.length === 0" class="p-8 text-center text-text/40 text-sm">No members yet.</div>
      <table v-else class="w-full text-sm">
        <thead class="bg-gray-50 border-b border-gray-100">
          <tr>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Name</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide hidden md:table-cell">Contact</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Type</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide hidden lg:table-cell">Expiry</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide hidden lg:table-cell">Cart</th>
            <th class="text-right px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          <tr v-for="m in members" :key="m.id">
            <td class="px-4 py-3">
              <div class="font-medium text-text">{{ m.firstName }} {{ m.lastName }}</div>
              <div class="text-xs text-text/50">Since {{ m.memberSince }}</div>
            </td>
            <td class="px-4 py-3 text-text/60 hidden md:table-cell">
              <div>{{ m.email ?? '—' }}</div>
              <div class="text-xs">{{ m.phone ?? '' }}</div>
            </td>
            <td class="px-4 py-3">
              <span class="text-xs font-semibold bg-primary/10 text-primary px-2 py-0.5 rounded">{{ m.membershipType }}</span>
            </td>
            <td class="px-4 py-3 text-text/60 hidden lg:table-cell">{{ m.expiryDate }}</td>
            <td class="px-4 py-3 text-center hidden lg:table-cell">
              <span v-if="m.cartTrackage || m.seasonalCartRental" class="text-xs text-green-700">✓</span>
              <span v-else class="text-xs text-text/30">—</span>
            </td>
            <td class="px-4 py-3 text-right flex justify-end gap-3">
              <button
                class="text-xs text-primary hover:underline"
                @click="openEdit(m)"
              >
                Edit
              </button>
              <button
                :disabled="deletingId === m.id"
                class="text-xs text-red-600 hover:underline disabled:opacity-50"
                @click="deleteMember(m.id, `${m.firstName} ${m.lastName}`)"
              >
                Remove
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
