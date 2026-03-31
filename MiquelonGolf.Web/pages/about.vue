<script setup lang="ts">
useSeoMeta({
  title: 'About — Miquelon Hills Golf Course',
  description: 'The story of Miquelon Hills Golf Course — an 18-hole public course in Camrose County, Alberta, bordering Miquelon Lake Provincial Park.',
})
</script>

<template>
  <div class="max-w-4xl mx-auto px-4 py-12">
    <h1 class="font-display text-4xl font-bold text-accent mb-2">About Miquelon Hills</h1>
    <p class="text-text/70 mb-10">An 18-hole public course with real Alberta character.</p>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-10 mb-12">
      <div>
        <h2 class="font-display text-2xl font-bold text-accent mb-3">The Course</h2>
        <p class="text-text leading-relaxed mb-4">
          Miquelon Hills Golf Course sits in Camrose County, just over an hour east of Edmonton on the
          edge of Miquelon Lake Provincial Park. The course was built to take advantage of the natural
          terrain — the front nine opens up across rolling hills accessible to all skill levels, while
          the back nine presses through narrow forest corridors that demand precision and punish the
          errant shot.
        </p>
        <p class="text-text leading-relaxed mb-4">
          It's a course that rewards second and third visits. You'll discover new challenges on the
          back nine and develop a strategy for the front that you'll refine all season.
        </p>
        <p class="text-text leading-relaxed">
          The provincial park backdrop means you're playing through genuine Alberta wilderness — white
          spruce and aspen, wildlife in the rough, and wide prairie skies above the open holes.
        </p>
      </div>

      <div>
        <h2 class="font-display text-2xl font-bold text-accent mb-3">The Owner</h2>
        <p class="text-text leading-relaxed mb-4">
          Randy Haselgruber has run Miquelon Hills for years with a straightforward philosophy:
          keep the course in great shape, keep the prices fair, and make every golfer feel welcome
          whether they're shooting 72 or 122.
        </p>
        <p class="text-text leading-relaxed">
          Randy and the pro shop team are on-site throughout the season. If you have a question
          about course conditions, tee times, or memberships, they're the best resource you'll find.
        </p>
      </div>
    </div>

    <div class="bg-surface rounded-lg p-6 shadow-sm">
      <h2 class="font-display text-xl font-bold text-accent mb-3">By the Numbers</h2>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
        <div>
          <p class="font-display text-3xl font-bold text-primary">18</p>
          <p class="text-sm text-text/60 mt-1">Holes</p>
        </div>
        <div>
          <p class="font-display text-3xl font-bold text-primary">1 hr</p>
          <p class="text-sm text-text/60 mt-1">From Edmonton</p>
        </div>
        <div>
          <p class="font-display text-3xl font-bold text-primary">May–Oct</p>
          <p class="text-sm text-text/60 mt-1">Season</p>
        </div>
        <div>
          <p class="font-display text-3xl font-bold text-primary">Public</p>
          <p class="text-sm text-text/60 mt-1">Always welcome</p>
        </div>
      </div>
    </div>
  </div>
</template>
