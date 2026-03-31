// MiquelonGolf.Web/tests/components/SlotGrid.test.ts
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import SlotGrid from '~/components/booking/SlotGrid.vue'
import type { TeeTimeSlotDto } from '~/types/api'

const makeSlot = (overrides: Partial<TeeTimeSlotDto> = {}): TeeTimeSlotDto => ({
  id: '1',
  date: '2026-05-01',
  startTime: '08:00',
  maxPlayers: 4,
  isBlocked: false,
  blockReason: null,
  bookingCount: 0,
  ...overrides,
})

describe('SlotGrid', () => {
  it('renders slot start time', async () => {
    const wrapper = await mountSuspended(SlotGrid, {
      props: { slots: [makeSlot({ startTime: '09:30' })], selectedSlotId: null },
    })
    expect(wrapper.text()).toContain('09:30')
  })

  it('marks slot as available when not blocked and has space', async () => {
    const wrapper = await mountSuspended(SlotGrid, {
      props: { slots: [makeSlot({ bookingCount: 2, maxPlayers: 4 })], selectedSlotId: null },
    })
    expect(wrapper.find('[data-testid="slot-available"]').exists()).toBe(true)
  })

  it('marks slot as full when bookingCount >= maxPlayers', async () => {
    const wrapper = await mountSuspended(SlotGrid, {
      props: { slots: [makeSlot({ bookingCount: 4, maxPlayers: 4 })], selectedSlotId: null },
    })
    const btn = wrapper.find('[data-testid="slot-full"]')
    expect(btn.exists()).toBe(true)
    expect(btn.attributes('disabled')).toBeDefined()
  })

  it('marks slot as blocked when isBlocked is true', async () => {
    const wrapper = await mountSuspended(SlotGrid, {
      props: { slots: [makeSlot({ isBlocked: true })], selectedSlotId: null },
    })
    const btn = wrapper.find('[data-testid="slot-blocked"]')
    expect(btn.exists()).toBe(true)
    expect(btn.attributes('disabled')).toBeDefined()
  })

  it('applies selected style when slot id matches selectedSlotId', async () => {
    const slot = makeSlot({ id: 'abc-123' })
    const wrapper = await mountSuspended(SlotGrid, {
      props: { slots: [slot], selectedSlotId: 'abc-123' },
    })
    expect(wrapper.find('[data-testid="slot-available"]').classes()).toContain('bg-primary')
  })

  it('does not apply selected style when id does not match', async () => {
    const slot = makeSlot({ id: 'abc-123' })
    const wrapper = await mountSuspended(SlotGrid, {
      props: { slots: [slot], selectedSlotId: 'other-id' },
    })
    expect(wrapper.find('[data-testid="slot-available"]').classes()).not.toContain('bg-primary')
  })

  it('emits select with slot when available slot is clicked', async () => {
    const slot = makeSlot({ id: 'abc-123' })
    const wrapper = await mountSuspended(SlotGrid, {
      props: { slots: [slot], selectedSlotId: null },
    })
    await wrapper.find('[data-testid="slot-available"]').trigger('click')
    expect(wrapper.emitted('select')).toBeTruthy()
    expect(wrapper.emitted('select')![0][0]).toEqual(slot)
  })

  it('does not emit select when full slot is clicked', async () => {
    const wrapper = await mountSuspended(SlotGrid, {
      props: { slots: [makeSlot({ bookingCount: 4, maxPlayers: 4 })], selectedSlotId: null },
    })
    await wrapper.find('[data-testid="slot-full"]').trigger('click')
    expect(wrapper.emitted('select')).toBeFalsy()
  })
})
