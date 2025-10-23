namespace MyApp;

public enum CaseMenu
{
    RegistrationProfile = 1,
    ManagementProfiles = 2,
    ManagementPark = 3,
    StartRent = 4,
    EndRent = 5,
    FindGeo = 6,
    Report = 7,
    Leave = 8
}

public enum CaseChangeProfile
{
    Wallet = 1,
    Name = 2,
    FamilyName = 3,
    Delete = 4
}

public enum CaseFleetManagement
{
    AddVehicle = 1,
    RemoveVehicle = 2,
    ChangeStatus = 3,
    ViewAll = 4,
    ManageStatuses = 5,
    AddParkingZone = 6
}

public enum CaseStatusManagement
{
    AddStatus = 1,
    ViewStatuses = 2,
    ViewParkingZones = 3,
    Back = 4,
}
