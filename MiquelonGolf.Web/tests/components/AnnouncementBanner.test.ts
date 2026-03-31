import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AnnouncementBanner from '~/components/AnnouncementBanner.vue'
import type { AnnouncementDto } from '~/types/api'

const base: AnnouncementDto = {
  id: '1',
  message: 'Course is open!',
  type: 'CourseConditions',
  isActive: true,
  createdAt: '2026-05-01T00:00:00Z',
  expiresAt: null,
}

describe('AnnouncementBanner', () => {
  it('renders active announcement message', async () => {
    const wrapper = await mountSuspended(AnnouncementBanner, {
      props: { announcements: [base] },
    })
    expect(wrapper.text()).toContain('Course is open!')
  })

  it('renders nothing when no announcements', async () => {
    const wrapper = await mountSuspended(AnnouncementBanner, {
      props: { announcements: [] },
    })
    expect(wrapper.find('[data-testid="banner"]').exists()).toBe(false)
  })

  it('applies green class for CourseConditions type', async () => {
    const wrapper = await mountSuspended(AnnouncementBanner, {
      props: { announcements: [base] },
    })
    const banner = wrapper.find('[data-testid="banner"]')
    expect(banner.classes()).toContain('bg-green-100')
  })

  it('applies red class for Closure type', async () => {
    const closure: AnnouncementDto = { ...base, type: 'Closure', message: 'Course closed today.' }
    const wrapper = await mountSuspended(AnnouncementBanner, {
      props: { announcements: [closure] },
    })
    const banner = wrapper.find('[data-testid="banner"]')
    expect(banner.classes()).toContain('bg-red-100')
  })

  it('renders multiple announcements', async () => {
    const second: AnnouncementDto = { ...base, id: '2', message: 'Cart paths only.' }
    const wrapper = await mountSuspended(AnnouncementBanner, {
      props: { announcements: [base, second] },
    })
    expect(wrapper.text()).toContain('Course is open!')
    expect(wrapper.text()).toContain('Cart paths only.')
  })
})
