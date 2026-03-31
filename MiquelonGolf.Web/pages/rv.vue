<script setup lang="ts">
useSeoMeta({
  title: 'Seasonal RV Sites — Stay & Play at Miquelon Hills',
  description: 'Stay the weekend at Miquelon Hills Golf Course. Seasonal RV sites steps from the first tee, bordering Miquelon Lake Provincial Park.',
})
</script>

<template>
  <div class="max-w-4xl mx-auto px-4 py-12">
    <h1 class="font-display text-4xl font-bold text-accent mb-2">Stay &amp; Play — Seasonal RV</h1>
    <p class="text-text/70 mb-10">Make a full weekend out of your golf trip. It's the one thing no other course near Edmonton offers.</p>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-10 mb-12">
      <div>
        <h2 class="font-display text-2xl font-bold text-accent mb-3">The Stay &amp; Play Experience</h2>
        <p class="text-text leading-relaxed mb-4">
          Imagine waking up, walking 50 feet, and teeing off on Hole 1. Miquelon Hills offers seasonal
          RV sites right on the property — the only "Stay &amp; Play" setup you'll find this close to Edmonton.
        </p>
        <p class="text-text leading-relaxed mb-4">
          After your round, unwind by Miquelon Lake Provincial Park, one of Alberta's most scenic
          provincial parks. The lake and trail system are minutes away from your RV site.
        </p>
        <p class="text-text leading-relaxed">
          Whether you're planning a family golf trip, a long weekend with friends, or a solo getaway,
          our seasonal sites give you a home base unlike anything else near Camrose.
        </p>
      </div>

      <div class="bg-background rounded-lg p-6">
        <h2 class="font-display text-xl font-bold text-accent mb-4">Site Details</h2>
        <ul class="space-y-3 text-sm text-text">
          <li class="flex gap-2">
            <span class="text-primary font-bold mt-0.5">✓</span>
            <span>Seasonal sites — available May through October</span>
          </li>
          <li class="flex gap-2">
            <span class="text-primary font-bold mt-0.5">✓</span>
            <span>Steps from the first tee — no shuttle needed</span>
          </li>
          <li class="flex gap-2">
            <span class="text-primary font-bold mt-0.5">✓</span>
            <span>Adjacent to Miquelon Lake Provincial Park</span>
          </li>
          <li class="flex gap-2">
            <span class="text-primary font-bold mt-0.5">✓</span>
            <span>One hour east of Edmonton, 45 min from Sherwood Park</span>
          </li>
        </ul>

        <div class="mt-6 p-4 bg-primary/5 border border-primary/20 rounded">
          <p class="text-sm text-text font-medium mb-1">To reserve an RV site:</p>
          <p class="text-sm text-text/70">Sites are booked by phone. Limited availability — call early in the season to secure your spot.</p>
          <a
            href="tel:+17804732511"
            class="inline-flex items-center mt-3 px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
          >
            Call (780) 473-2511
          </a>
        </div>
      </div>
    </div>

    <div class="bg-surface rounded-lg p-6 shadow-sm">
      <h2 class="font-display text-xl font-bold text-accent mb-2">About Miquelon Lake Provincial Park</h2>
      <p class="text-text text-sm leading-relaxed">
        Miquelon Lake Provincial Park borders the east side of our property. The park offers walking trails,
        birdwatching, swimming, and some of central Alberta's most beautiful lake scenery. It's a perfect
        complement to a day on the course.
      </p>
    </div>
  </div>
</template>
