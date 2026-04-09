// types/api.ts
export interface AnnouncementDto {
  id: string
  message: string
  type: 'CourseConditions' | 'General' | 'Closure'
  isActive: boolean
  createdAt: string
  expiresAt: string | null
}

export interface EventDto {
  id: string
  title: string
  description: string
  eventDate: string
  startTime: string | null
  isPublic: boolean
  category: 'Tournament' | 'SocialNight' | 'LadiesNight' | 'MensNight' | 'Other'
  imageUrl: string | null
}

export interface HoleDto {
  id: string
  holeNumber: number
  par: number
  handicap: number
  yardageBlue: number
  yardageWhite: number
  yardageRed: number
  description: string
  imageUrl: string | null
  diagramUrl: string | null
}

export interface SiteContentDto {
  key: string
  value: string
  lastUpdatedAt: string
}

export type RoundType = 'Eighteen' | 'FrontNine' | 'BackNine'

export interface TeeTimeSlotDto {
  id: string
  date: string
  startTime: string
  maxPlayers: number
  isBlocked: boolean
  blockReason: string | null
  bookingCount: number
  startingHole: number
}

export interface BookingDto {
  id: string
  teeTimeSlotId: string
  slotDate: string
  slotTime: string
  startingHole: number
  golferName: string
  golferEmail: string
  golferPhone: string
  numberOfPlayers: number
  numberOfCarts: number
  status: string
  roundType: string
  referralSource: string | null
  confirmationCode: string
  bookedAt: string
}

export interface BookingConfirmationDto {
  id: string
  confirmationCode: string
  slotDate: string
  slotTime: string
  startingHole: number
  numberOfPlayers: number
  numberOfCarts: number
  status: string
  roundType: string
}

export interface BookingSearchResultDto {
  id: string
  slotDate: string
  slotTime: string
  startingHole: number
  golferName: string
  golferEmail: string
  status: string
  roundType: string
  confirmationCode: string
}

export interface FlowThroughEntryDto {
  bookingId: string
  golferName: string
  estimatedArrival: string
  originSlotTime: string
  numberOfPlayers: number
}

export interface BookingSettingsDto {
  bookingWindowDays: number
  turnTimeOffsetMinutes: number
  teeTimeIntervalMinutes: number
  maxPlayersPerSlot: number
}

export interface CreateBookingPayload {
  teeTimeSlotId: string
  golferName: string
  golferEmail: string
  golferPhone: string
  numberOfPlayers: number
  numberOfCarts: number
  roundType: RoundType
  referralSource: string | null
}

export interface OperatingHoursDto {
  dayOfWeek: number
  dayName: string
  isOpen: boolean
  openTime: string    // "HH:mm"
  closeTime: string   // "HH:mm"
  intervalMinutes: number
  maxPlayers: number
}

export interface CourseHolidayDto {
  id: string
  date: string        // "YYYY-MM-DD"
  reason: string
}

export interface SpecialHoursDto {
  id: string
  date: string        // "YYYY-MM-DD"
  openTime: string    // "HH:mm"
  closeTime: string   // "HH:mm"
  reason: string
}

export interface MemberDto {
  id: string
  firstName: string
  lastName: string
  email: string | null
  phone: string | null
  membershipType: 'Adult' | 'Senior' | 'Junior' | 'Family' | 'YoungAdult' | 'SeniorCouple'
  memberSince: string        // YYYY-MM-DD (permanent — when they first joined the course)
  seasonYear: number
  purchaseDate: string       // YYYY-MM-DD
  expiryDate: string         // YYYY-MM-DD
  cartTrackage: boolean
  seasonalCartRental: boolean
}
