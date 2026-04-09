<script setup lang="ts">
const route = useRoute()
const siteUrl = 'https://miquelonhillsgolf.com'

useHead({
  link: [
    { rel: 'canonical', href: () => `${siteUrl}${route.path}` },
  ],
})

useSeoMeta({
  ogImage: `${siteUrl}/images/fairway-corridor.avif`,
  ogImageWidth: 1200,
  ogImageHeight: 630,
  ogType: 'website',
  ogSiteName: 'Miquelon Hills Golf Course',
  ogUrl: () => `${siteUrl}${route.path}`,
  twitterCard: 'summary_large_image',
})
</script>

<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>
