<script setup lang="ts">
useSeoMeta({
  title: 'Fees & Memberships — Miquelon Hills Golf Course',
  description: 'Green fees, cart rentals, and annual membership pricing for Miquelon Hills Golf Course near Camrose, Alberta.',
})

const api = useApi()

const allContent = await $fetch<{ key: string; value: string }[]>(api.url('/site-content')).catch(() => [])

const fee = (key: string): string => {
  const entry = allContent.find(r => r?.key === key)
  return entry?.value ?? '—'
}
</script>

<template>
  <div>
    <PageHero title="Fees & Memberships" subtitle="All prices are in Canadian dollars and subject to change. Contact us for the most current rates." />
    <div class="max-w-4xl mx-auto px-4 py-12">
      <!-- Green Fees -->
      <section class="mb-10">
        <h2 class="font-display text-2xl font-bold text-accent mb-4">Green Fees</h2>
        <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
          <table class="w-full text-sm">
            <thead class="bg-primary text-white">
              <tr>
                <th class="text-left px-4 py-3 font-semibold">Player Type</th>
                <th class="text-right px-4 py-3 font-semibold">Rate</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              <tr class="hover:bg-background transition-colors">
                <td class="px-4 py-3">Adult</td>
                <td class="px-4 py-3 text-right font-medium">{{ fee('fees.green.adult') }}</td>
              </tr>
              <tr class="hover:bg-background transition-colors">
                <td class="px-4 py-3">Senior</td>
                <td class="px-4 py-3 text-right font-medium">{{ fee('fees.green.senior') }}</td>
              </tr>
              <tr class="hover:bg-background transition-colors">
                <td class="px-4 py-3">Junior</td>
                <td class="px-4 py-3 text-right font-medium">{{ fee('fees.green.junior') }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Cart Rental -->
      <section class="mb-10">
        <h2 class="font-display text-2xl font-bold text-accent mb-4">Cart Rental</h2>
        <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
          <table class="w-full text-sm">
            <thead class="bg-primary text-white">
              <tr>
                <th class="text-left px-4 py-3 font-semibold">Holes</th>
                <th class="text-right px-4 py-3 font-semibold">Rate</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              <tr class="hover:bg-background transition-colors">
                <td class="px-4 py-3">9 Holes</td>
                <td class="px-4 py-3 text-right font-medium">{{ fee('fees.cart.9') }}</td>
              </tr>
              <tr class="hover:bg-background transition-colors">
                <td class="px-4 py-3">18 Holes</td>
                <td class="px-4 py-3 text-right font-medium">{{ fee('fees.cart.18') }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Memberships -->
      <section class="mb-10">
        <h2 class="font-display text-2xl font-bold text-accent mb-4">Annual Memberships</h2>
        <p class="text-sm text-text/70 mb-4">Memberships include unlimited green fees for the season. Cart trackage and seasonal cart rental available as add-ons.</p>
        <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
          <table class="w-full text-sm">
            <thead class="bg-primary text-white">
              <tr>
                <th class="text-left px-4 py-3 font-semibold">Membership Type</th>
                <th class="text-right px-4 py-3 font-semibold">Price</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              <tr v-for="[key, label] in [
                ['fees.membership.adult', 'Adult'],
                ['fees.membership.senior', 'Senior'],
                ['fees.membership.junior', 'Junior'],
                ['fees.membership.youngadult', 'Young Adult'],
                ['fees.membership.family', 'Family'],
                ['fees.membership.seniorcouple', 'Senior Couple'],
              ]" :key="key" class="hover:bg-background transition-colors">
                <td class="px-4 py-3">{{ label }}</td>
                <td class="px-4 py-3 text-right font-medium">{{ fee(key) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="mt-4 grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="bg-primary/8 rounded-lg border-l-4 border-primary shadow-sm p-4 text-sm">
            <span class="font-semibold text-accent">Cart Trackage add-on:</span>
            <span class="ml-2">{{ fee('fees.membership.cart') }}</span>
          </div>
          <div class="bg-primary/8 rounded-lg border-l-4 border-primary shadow-sm p-4 text-sm">
            <span class="font-semibold text-accent">Seasonal Cart Rental:</span>
            <span class="ml-2">{{ fee('fees.membership.seasonalcart') }}</span>
          </div>
        </div>
      </section>

      <section class="bg-primary rounded-lg p-8 text-center mt-10">
        <h2 class="font-display text-2xl font-bold text-white mb-3">Interested in a Membership?</h2>
        <p class="text-white/70 mb-2">Contact the pro shop to sign up or ask questions.</p>
        <a href="tel:+17804732511" class="inline-flex items-center px-6 py-3 bg-accent text-white font-semibold rounded hover:opacity-90 transition-opacity mt-3">
          Call (780) 473-2511
        </a>
      </section>

      <p class="text-sm text-text/70 mt-6">To purchase a membership, contact the pro shop or call us directly.</p>
    </div>
  </div>
</template>
