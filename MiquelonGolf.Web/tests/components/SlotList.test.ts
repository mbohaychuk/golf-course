// MiquelonGolf.Web/tests/components/SlotList.test.ts
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import SlotList from '~/components/booking/SlotList.vue'
import type { TeeTimeSlotDto } from '~/types/api'

const makeSlot = (overrides: Partial<TeeTimeSlotDto> = {}): TeeTimeSlotDto => ({
  id: '1',
  date: '2026-05-01',
  startTime: '08:00',
  maxPlayers: 4,
  isBlocked: false,
  blockReason: null,
  bookingCount: 0,
  startingHole: 1,
  ...overrides,
})

describe('SlotList', () => {
  it('renders slot start time in 12-hour format', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [makeSlot({ startTime: '09:30' })], loading: false },
    })
    expect(wrapper.text()).toContain('9:30 AM')
  })

  it('shows Available label for open slots', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [makeSlot({ bookingCount: 0, maxPlayers: 4 })], loading: false },
    })
    expect(wrapper.text()).toContain('Available')
  })

  it('shows Booked label when slot is full', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [makeSlot({ bookingCount: 4, maxPlayers: 4 })], loading: false },
    })
    expect(wrapper.text()).toContain('Booked')
  })

  it('shows Booked label when slot is blocked', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [makeSlot({ isBlocked: true })], loading: false },
    })
    expect(wrapper.text()).toContain('Booked')
  })

  it('disables button for full slots', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [makeSlot({ bookingCount: 4, maxPlayers: 4 })], loading: false },
    })
    const btn = wrapper.find('button[aria-disabled="true"]')
    expect(btn.exists()).toBe(true)
  })

  it('renders loading skeleton when loading is true', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [], loading: true },
    })
    expect(wrapper.find('.animate-pulse').exists()).toBe(true)
  })

  it('renders filter tabs', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [makeSlot()], loading: false },
    })
    expect(wrapper.text()).toContain('All Times')
    expect(wrapper.text()).toContain('Morning')
    expect(wrapper.text()).toContain('Afternoon')
    expect(wrapper.text()).toContain('Twilight')
  })

  it('shows empty message when no slots match filter', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [], loading: false },
    })
    expect(wrapper.text()).toContain('No tee times available')
  })

  it('formats PM times correctly', async () => {
    const wrapper = await mountSuspended(SlotList, {
      props: { slots: [makeSlot({ startTime: '14:00' })], loading: false },
    })
    expect(wrapper.text()).toContain('2:00 PM')
  })
})
