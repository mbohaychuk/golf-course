import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import EventCard from '~/components/EventCard.vue'
import type { EventDto } from '~/types/api'

const base: EventDto = {
  id: '1',
  title: 'Club Championship',
  description: 'Annual club championship tournament.',
  eventDate: '2026-07-15',
  startTime: '08:00:00',
  isPublic: true,
  category: 'Tournament',
  imageUrl: null,
}

describe('EventCard', () => {
  it('renders the event title', async () => {
    const wrapper = await mountSuspended(EventCard, { props: { event: base } })
    expect(wrapper.text()).toContain('Club Championship')
  })

  it('renders the event date with the year', async () => {
    const wrapper = await mountSuspended(EventCard, { props: { event: base } })
    expect(wrapper.text()).toContain('2026')
  })

  it('renders category label', async () => {
    const wrapper = await mountSuspended(EventCard, { props: { event: base } })
    expect(wrapper.text()).toContain('Tournament')
  })

  it('shows members-only badge when isPublic is false', async () => {
    const membersEvent: EventDto = { ...base, isPublic: false }
    const wrapper = await mountSuspended(EventCard, { props: { event: membersEvent } })
    expect(wrapper.find('[data-testid="members-badge"]').exists()).toBe(true)
  })

  it('does not show members badge for public events', async () => {
    const wrapper = await mountSuspended(EventCard, { props: { event: base } })
    expect(wrapper.find('[data-testid="members-badge"]').exists()).toBe(false)
  })
})
