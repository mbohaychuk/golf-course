// MiquelonGolf.Web/middleware/admin.ts
export default defineNuxtRouteMiddleware((to) => {
  const { isAdmin } = useAuth()
  if (to.path === '/admin/login') {
    if (isAdmin.value) return navigateTo('/admin')
    return
  }
  if (!isAdmin.value) return navigateTo('/admin/login')
})
