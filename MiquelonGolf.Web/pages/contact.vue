<script setup lang="ts">
useSeoMeta({
  title: 'Contact — Miquelon Hills Golf Course',
  description: 'Get in touch with Miquelon Hills Golf Course. Phone, email, and directions from Edmonton and Camrose.',
})

const form = reactive({ name: '', email: '', message: '' })
const submitted = ref(false)

async function handleSubmit() {
  const mailto = `mailto:info@miquelonhillsgolf.com?subject=Website Enquiry from ${encodeURIComponent(form.name)}&body=${encodeURIComponent(form.message)}%0A%0AReply to: ${encodeURIComponent(form.email)}`
  window.location.href = mailto
  submitted.value = true
}
</script>

<template>
  <div class="max-w-5xl mx-auto px-4 py-12 grid grid-cols-1 md:grid-cols-2 gap-12">
    <!-- Contact Info -->
    <div>
      <h1 class="font-display text-4xl font-bold text-accent mb-2">Contact Us</h1>
      <p class="text-text/70 mb-8">We'd love to hear from you — whether it's a tee time question, membership inquiry, or RV booking.</p>

      <div class="space-y-5 text-sm text-text">
        <div>
          <p class="font-semibold text-accent mb-1">Phone</p>
          <a href="tel:+17804732511" class="text-primary hover:underline text-base">(780) 473-2511</a>
        </div>
        <div>
          <p class="font-semibold text-accent mb-1">Email</p>
          <a href="mailto:info@miquelonhillsgolf.com" class="text-primary hover:underline">info@miquelonhillsgolf.com</a>
        </div>
        <div>
          <p class="font-semibold text-accent mb-1">Location</p>
          <p class="text-text/70 leading-relaxed">
            Camrose County, Alberta<br>
            Bordering Miquelon Lake Provincial Park<br>
            ~1 hour east of Edmonton · 45 min from Sherwood Park
          </p>
        </div>
        <div>
          <p class="font-semibold text-accent mb-1">Season</p>
          <p class="text-text/70">Open May through October, dawn to dusk.</p>
        </div>
        <div>
          <p class="font-semibold text-accent mb-2">Follow Us</p>
          <div class="flex gap-3">
            <a href="https://www.facebook.com/miquelonhillsgolf" target="_blank" rel="noopener" class="text-primary hover:underline">Facebook</a>
            <a href="https://www.instagram.com/miquelonhillsgolf" target="_blank" rel="noopener" class="text-primary hover:underline">Instagram</a>
          </div>
        </div>
      </div>
    </div>

    <!-- Contact Form -->
    <div>
      <h2 class="font-display text-2xl font-bold text-accent mb-6">Send a Message</h2>

      <div v-if="submitted" class="bg-green-50 border border-green-200 rounded p-4 text-green-700 text-sm">
        Your email client should have opened with a pre-filled message. If not, please email us directly at
        <a href="mailto:info@miquelonhillsgolf.com" class="underline">info@miquelonhillsgolf.com</a>.
      </div>

      <form v-else class="space-y-4" @submit.prevent="handleSubmit">
        <div>
          <label for="contact-name" class="block text-sm font-medium text-text mb-1">Name</label>
          <input
            id="contact-name"
            v-model="form.name"
            type="text"
            required
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
            placeholder="Your name"
          >
        </div>
        <div>
          <label for="contact-email" class="block text-sm font-medium text-text mb-1">Email</label>
          <input
            id="contact-email"
            v-model="form.email"
            type="email"
            required
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
            placeholder="your@email.com"
          >
        </div>
        <div>
          <label for="contact-message" class="block text-sm font-medium text-text mb-1">Message</label>
          <textarea
            id="contact-message"
            v-model="form.message"
            rows="5"
            required
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
            placeholder="How can we help?"
          />
        </div>
        <button
          type="submit"
          class="w-full px-4 py-2.5 bg-primary text-white font-semibold text-sm rounded hover:opacity-90 transition-opacity"
        >
          Send Message
        </button>
      </form>
    </div>
  </div>
</template>
