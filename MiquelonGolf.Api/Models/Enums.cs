namespace MiquelonGolf.Api.Models;

public enum UserRole { Admin, Member, Public }

public enum MembershipType
{
    Adult, Senior, Junior, Family, YoungAdult, SeniorCouple
}

public enum BookingStatus { Confirmed, Cancelled }

public enum AnnouncementType { CourseConditions, General, Closure }

public enum EventCategory
{
    Tournament, SocialNight, LadiesNight, MensNight, Other
}
