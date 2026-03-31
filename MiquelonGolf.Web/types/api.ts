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
