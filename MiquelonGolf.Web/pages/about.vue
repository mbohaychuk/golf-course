<script setup lang="ts">
useSeoMeta({
  title: 'About — Miquelon Hills Golf Course',
  description: 'The story of Miquelon Hills Golf Course — an 18-hole public course in Camrose County, Alberta, bordering Miquelon Lake Provincial Park.',
})
</script>

<template>
  <div>
    <PageHero title="About Miquelon Hills" subtitle="An 18-hole public course with real Alberta character." />
    <div class="max-w-4xl mx-auto px-4 py-12">
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

      <div class="relative h-56 md:h-72 my-12 -mx-4 overflow-hidden">
        <img src="/images/fairway-open.avif" alt="Open fairway at Miquelon Hills Golf Course" class="w-full h-full object-cover" />
        <div class="absolute inset-0 bg-gradient-to-t from-forest/60 to-transparent" />
      </div>

      <div class="bg-surface rounded-lg p-6 shadow-sm mb-10">
        <h2 class="font-display text-xl font-bold text-accent mb-3">By the Numbers</h2>
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
          <div class="border-l-2 border-primary pl-4">
            <p class="font-display text-4xl font-bold text-accent">18</p>
            <p class="text-sm text-text/60 mt-1">Holes</p>
          </div>
          <div class="border-l-2 border-primary pl-4">
            <p class="font-display text-4xl font-bold text-accent">1 hr</p>
            <p class="text-sm text-text/60 mt-1">From Edmonton</p>
          </div>
          <div class="border-l-2 border-primary pl-4">
            <p class="font-display text-4xl font-bold text-accent">May–Oct</p>
            <p class="text-sm text-text/60 mt-1">Season</p>
          </div>
          <div class="border-l-2 border-primary pl-4">
            <p class="font-display text-4xl font-bold text-accent">Public</p>
            <p class="text-sm text-text/60 mt-1">Always welcome</p>
          </div>
        </div>
      </div>

      <section class="bg-primary rounded-lg p-8 text-center mt-10">
        <h2 class="font-display text-2xl font-bold text-white mb-3">Ready to Play?</h2>
        <p class="text-white/70 mb-5">Book your tee time online or call the pro shop.</p>
        <NuxtLink to="/book" class="inline-flex items-center px-6 py-3 bg-accent text-white font-semibold rounded hover:opacity-90 transition-opacity">
          Book a Tee Time
        </NuxtLink>
      </section>
    </div>
  </div>
</template>
