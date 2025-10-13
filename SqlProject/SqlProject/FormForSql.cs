namespace MyApp;

public static class FormForSql
{
    public class Vehicles
    {
        private string QrCode;
        private string TypeTransport;
        private int StatusId;
    }
    
    public class User
    {
        private int UserID;
        private string Name;
        private string FamilyName;
        private string Email;
        private int WalletId;
    }
    
    public class Rentals
    {
        private int UserId;
        private DateTime StartTrip;
        private DateTime EndTrip;
        private int ParkingZoneId;
        private int VehicleId;
    }
    
    public class Status
    {
        private string TypeStatus;
        private string Description;
    }
    
    public class Wallets
    {
        private int NumberCard;
        private int CvcCode;
        private DateTime Validity;
    }
    
    public class ParkingZones
    {
        private int Width;
        private int Length;
        private string ApproximateAddress;
    }
}