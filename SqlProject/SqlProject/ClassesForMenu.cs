using Dapper;
using Microsoft.Data.SqlClient;

namespace MyApp;

interface IUploadInServer
{
    public bool Upload(SqlConnection connection);
}
public class AddParkingZone : IUploadInServer
{
    public decimal Width { private get; set; }
    public decimal Length { private get; set; }
    public string ApproximateAddress { private get; set; }

    public AddParkingZone() { }

    public bool Upload(SqlConnection connection)
    {
        try
        {
            string sql = "INSERT INTO Parking_Zones (Width, Length, Approximate_address) VALUES (@Width, @Length, @ApproximateAddress)";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Width", Width);
                command.Parameters.AddWithValue("@Length", Length);
                command.Parameters.AddWithValue("@ApproximateAddress", ApproximateAddress);
            
                int affectedRows = command.ExecuteNonQuery();
                return affectedRows > 0;
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка добавления парковочной зоны: {ex.Message}", true, true, 50);
            return false;
        }
    }
}

public class UserRegistaryForm : IUploadInServer
{
    public string name { private get; set; }
    public string familyName { private get; set; }
    public string email { private get; set; }
    public Decimal numberCard { private get; set; }
    public int cvcCode { private get; set; }
    public DateTime Validity { private get; set; }
    
    public UserRegistaryForm(){}

    public override string ToString()
        => $"{name} {familyName} {email} {numberCard} {cvcCode} {Validity}";


    public bool Upload(SqlConnection connection)
    {
        try
        {
            string connectionString =
                "INSERT INTO Wallets (Number_card, THREE_code, Validity) VALUES (@numberCard, @cvcCode, @Validity)";

            using (SqlCommand command = new SqlCommand(connectionString, connection))
            {
                command.Parameters.AddWithValue("@cvcCode", cvcCode);
                command.Parameters.AddWithValue("@Validity", Validity);
                command.Parameters.AddWithValue("@numberCard", numberCard);

                command.ExecuteNonQuery();
            }

            int id = connection.Query("SELECT id FROM Wallets WHERE Number_card = @numberCards",
                new { @numberCards = numberCard }).First().id;

            connectionString =
                "INSERT INTO Users (Name, Family_Name, Email, Wallet) VALUES (@name, @familyName, @email, @Wallet)";

            using (SqlCommand command = new SqlCommand(connectionString, connection))
            {
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@familyName", familyName);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@Wallet", id);
                
                command.ExecuteNonQuery();
            }
        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }
}

public class ChangeWallet : IUploadInServer
{
    public string name;
    public string email;

    
    private int idWallet;
    
    public decimal numberCard { private get; set; }
    public int cvcCode {private get; set; }
    public DateTime Validity {private get; set;}

    public ChangeWallet() { }

    public bool SearchData(SqlConnection connection)
    {
        try
        {
            var result = connection.Query<string>("SELECT W.id FROM Wallets as W, Users as U" +
                                                  " WHERE U.Email = @email AND U.Wallet = W.id AND U.Name = @name", new {@email = email, @name = name}).First().ToString();

            idWallet = int.Parse(result);

        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }
    public bool Upload(SqlConnection connection)
    {
        try
        {
            string connectStr = "UPDATE Wallets SET Number_card = @numberCard, THREE_code = @three_code, Validity = @Validity WHERE Id = @id";
            using (SqlCommand command = new SqlCommand(connectStr, connection))
            {
                command.Parameters.AddWithValue("@numberCard", numberCard);
                command.Parameters.AddWithValue("@three_code", cvcCode);
                command.Parameters.AddWithValue("@Validity", Validity);
                command.Parameters.AddWithValue("@id", idWallet);

                command.ExecuteNonQuery();
            }
            
        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }
}

public class ChangeNameOrFamilyName : IUploadInServer
{
    public string name;
    public string email;
    public string familyName;

    private int idUser;
    private bool familyNameValid;
    public ChangeNameOrFamilyName() {}

    public bool SearchData(SqlConnection connection)
    {
        try
        {
            string result = connection
                .Query<string>("SELECT * FROM Users WHERE Family_Name = @FamilyName AND Email = @Email", new {@FamilyName = this.familyName, @Email = this.email}).First()
                .ToString();

            idUser = int.Parse(result);
        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }
    
    public bool SearchDataFamily(SqlConnection connection)
    {
        try
        {
            string result = connection
                .Query<string>("SELECT * FROM Users WHERE Name = @Name AND Email = @Email", new {@Name = this.name, @Email = this.email}).First()
                .ToString();

            idUser = int.Parse(result);
        }
        catch (Exception e)
        {
            return false;
        }
        
        familyNameValid = true;
        return true;
    }
    
    public bool Upload(SqlConnection connection)
    {
        if (familyNameValid)
        {
            try
            {
                string connectStr = "UPDATE Users SET Family_Name = @familyName WHERE Id = @id";
                using (SqlCommand command = new SqlCommand(connectStr, connection))
                {
                    command.Parameters.AddWithValue("@familyName", familyName);
                    command.Parameters.AddWithValue("@id", idUser);
                    command.ExecuteNonQuery();
                }

            }
            catch (Exception)
            {
                return false;
            }
        
            return true;
        }
        
        
        try
        {
            string connectStr = "UPDATE Users SET Name = @name WHERE Id = @id";
            using (SqlCommand command = new SqlCommand(connectStr, connection))
            {
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@id", idUser);
                
                command.ExecuteNonQuery();
            }

        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }
    
}

public class RemoveUser : IUploadInServer
{
    public string name;
    public string email;
    public string familyName;

    private int idUser;
    private int idWallet;

    public RemoveUser() { }

    public bool SearchData(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<(int UserId, int WalletId)>(
                @"SELECT U.id as UserId, W.id as WalletId 
                  FROM Users as U 
                  INNER JOIN Wallets as W ON U.Wallet = W.id 
                  WHERE U.Name = @Name AND U.Family_Name = @FamilyName AND U.Email = @Email",
                new { Name = name, FamilyName = familyName, Email = email });

            if (result.UserId != 0)
            {
                idUser = result.UserId;
                idWallet = result.WalletId;
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ошибка поиска: {ex.Message}");
        }
        return false;
    }

    public bool CheckRentals(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                "SELECT COUNT(*) FROM Rentals WHERE UserId = @UserId",
                new { UserId = idUser });

            return result.HasValue && result.Value > 0;
        }
        catch (Exception ex)
        {
            return true;
        }
    }

    public bool Upload(SqlConnection connection)
    {
        using var transaction = connection.BeginTransaction();
        try
        {
            string deleteRentalsSql = "DELETE FROM Rentals WHERE UserId = @UserId";
            connection.Execute(deleteRentalsSql, new { UserId = idUser }, transaction);

            string deleteUserSql = "DELETE FROM Users WHERE id = @UserId";
            int userDeleted = connection.Execute(deleteUserSql, new { UserId = idUser }, transaction);

            string deleteWalletSql = "DELETE FROM Wallets WHERE id = @WalletId";
            int walletDeleted = connection.Execute(deleteWalletSql, new { WalletId = idWallet }, transaction);

            transaction.Commit();
            return userDeleted > 0 && walletDeleted > 0;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.Clear();
            Console.WriteLine($"Ошибка удаления: {ex.Message}");
            return false;
        }
    }
}

public class AddVehicle : IUploadInServer
{
    public string QrCode { private get; set; }
    public string TypeTransport { private get; set; }
    public int StatusId { private get; set; }

    public AddVehicle() { }

    public bool Upload(SqlConnection connection)
    {
        try
        {
            string sql = "INSERT INTO Vehicles (QrCode, Type_Transport, Status) VALUES (@QrCode, @TypeTransport, @StatusId)";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@QrCode", QrCode);
                command.Parameters.AddWithValue("@TypeTransport", TypeTransport);
                command.Parameters.AddWithValue("@StatusId", StatusId);
                
                int affectedRows = command.ExecuteNonQuery();
                return affectedRows > 0;
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ошибка добавления: {ex.Message}");
            return false;
        }
    }
}

public class RemoveVehicle : IUploadInServer
{
    public string QrCode { private get; set; }
    private int vehicleId;

    public RemoveVehicle() { }

    public bool SearchData(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                "SELECT id FROM Vehicles WHERE QrCode = @QrCode",
                new { QrCode });

            if (result.HasValue)
            {
                vehicleId = result.Value;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ошибка поиска: {ex.Message}");
            return false;
        }
    }

    public bool CheckRentals(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                "SELECT COUNT(*) FROM Rentals WHERE Vehicle = @VehicleId",
                new { VehicleId = vehicleId });

            return result.HasValue && result.Value > 0;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ошибка проверки аренд: {ex.Message}");
            return true;
        }
    }

    public bool Upload(SqlConnection connection)
    {
        try
        {
            string sql = "DELETE FROM Vehicles WHERE id = @VehicleId";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@VehicleId", vehicleId);
                int affectedRows = command.ExecuteNonQuery();
                return affectedRows > 0;
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ошибка удаления: {ex.Message}");
            return false;
        }
    }
}

public class ChangeVehicleStatus : IUploadInServer
{
    public string QrCode { private get; set; }
    public int NewStatusId { private get; set; }
    private int vehicleId;

    public ChangeVehicleStatus() { }

    public bool SearchData(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                "SELECT id FROM Vehicles WHERE QrCode = @QrCode",
                new { QrCode });

            if (result.HasValue)
            {
                vehicleId = result.Value;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ошибка поиска: {ex.Message}");
            return false;
        } 
    }

    public bool Upload(SqlConnection connection)
    {
        try
        {
            string sql = "UPDATE Vehicles SET Status = @NewStatusId WHERE id = @VehicleId";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@NewStatusId", NewStatusId);
                command.Parameters.AddWithValue("@VehicleId", vehicleId);
                int affectedRows = command.ExecuteNonQuery();
                return affectedRows > 0;
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ошибка обновления: {ex.Message}");
            return false;
        }
    }
}

public class AddStatus : IUploadInServer
{
    public string TypeStatus { private get; set; }
    public string Description { private get; set; }

    public AddStatus() { }

    public bool Upload(SqlConnection connection)
    {
        try
        {
            string sql = "INSERT INTO Status (Type_status, Description) VALUES (@TypeStatus, @Description)";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TypeStatus", TypeStatus);
                command.Parameters.AddWithValue("@Description", Description);
            
                int affectedRows = command.ExecuteNonQuery();
                return affectedRows > 0;
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка добавления статуса: {ex.Message}", true, true, 50);
            return false;
        }
    }
}

public class EndRental : IUploadInServer
{
    public int RentalId { private get; set; }
    public int NewParkingZoneId { private get; set; }
    public DateTime ActualEndTime { private get; set; }

    public EndRental() { }

    public (bool Exists, int VehicleId, string Error) CheckRentalAndGetVehicle(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault(@"
                SELECT 
                    R.id as RentalId,
                    R.Vehicle as VehicleId,
                    V.QrCode,
                    V.Type_Transport,
                    S.Type_status as VehicleStatus
                FROM Rentals R
                INNER JOIN Vehicles V ON R.Vehicle = V.id
                INNER JOIN Status S ON V.Status = S.id
                WHERE R.id = @RentalId AND R.End_trip IS NULL",
                new { RentalId });
            
            if (result == null)
            {
                var rentalExists = connection.QueryFirstOrDefault<int?>(
                    "SELECT id FROM Rentals WHERE id = @RentalId",
                    new { RentalId });
                
                if (!rentalExists.HasValue)
                {
                    return (false, 0, $"Аренда с ID {RentalId} не существует");
                }
                else
                {
                    var endTime = connection.QueryFirstOrDefault<DateTime?>(
                        "SELECT End_trip FROM Rentals WHERE id = @RentalId",
                        new { RentalId });
                    
                    if (endTime.HasValue)
                    {
                        return (false, 0, $"Аренда с ID {RentalId} уже завершена {endTime.Value:dd.MM.yyyy HH:mm}");
                    }
                    else
                    {
                        return (false, 0, $"Аренда с ID {RentalId} существует, но транспорт не найден");
                    }
                }
            }
            
            return (true, result.VehicleId, null);
        }
        catch (Exception ex)
        {
            return (false, 0, $"Ошибка проверки аренды: {ex.Message}");
        }
    }

    public bool Upload(SqlConnection connection)
    {
        Console.Clear();
        Animation.AnimationText("Проверка данных аренды...", false, 0, 0, true);
        
        var (exists, vehicleId, error) = CheckRentalAndGetVehicle(connection);
        
        if (!exists)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка: {error}", true, true, 50);
            return false;
        }

        var parkingZoneExists = connection.QueryFirstOrDefault<int?>(
            "SELECT id FROM Parking_Zones WHERE id = @NewParkingZoneId",
            new { NewParkingZoneId });
        
        if (!parkingZoneExists.HasValue)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка: парковочная зона с ID {NewParkingZoneId} не найдена!", true, true, 50);
            return false;
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            string rentalSql = @"
                UPDATE Rentals 
                SET End_trip = @ActualEndTime, 
                    Parking_zone = @NewParkingZoneId 
                WHERE id = @RentalId";
            
            int affectedRows = connection.Execute(rentalSql, new 
            { 
                ActualEndTime, 
                NewParkingZoneId,
                RentalId 
            }, transaction);

            if (affectedRows == 0)
            {
                transaction.Rollback();
                Console.Clear();
                Animation.PrintRedText("Ошибка: не удалось обновить запись аренды!", true, true, 50);
                return false;
            }

            string vehicleSql = "UPDATE Vehicles SET Status = 3 WHERE id = @VehicleId";
            int vehicleUpdated = connection.Execute(vehicleSql, new { VehicleId = vehicleId }, transaction);

            if (vehicleUpdated == 0)
            {
                transaction.Rollback();
                Console.Clear();
                Animation.PrintRedText("Ошибка: не удалось обновить статус транспорта!", true, true, 50);
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.Clear();
            Animation.PrintRedText($"Ошибка завершения аренды: {ex.Message}", true, true, 50);
            return false;
        }
    }
}

public class StartRental : IUploadInServer
{
    public int UserId { private get; set; }
    public int VehicleId { private get; set; }
    public int ParkingZoneId { private get; set; }
    public DateTime StartTrip { private get; set; }

    public StartRental() { }

    public bool CheckUserExists(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                "SELECT id FROM Users WHERE id = @UserId",
                new { UserId });
            
            if (!result.HasValue)
            {
                Console.Clear();
                Animation.PrintRedText($"Ошибка: пользователь с ID {UserId} не найден!", true, true, 50);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка проверки пользователя: {ex.Message}", true, true, 50);
            return false;
        }
    }

    public bool CheckVehicleAvailable(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                @"SELECT V.id FROM Vehicles V, Status S
                  WHERE V.id = @VehicleId AND S.Type_status = N'Свободен' AND V.Status = S.id",
                new { VehicleId });
            
            if (!result.HasValue)
            {
                Console.Clear();
                Animation.PrintRedText($"Ошибка: транспорт с ID {VehicleId} не найден или недоступен!", true, true, 50);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка проверки транспорта: {ex.Message}", true, true, 50);
            return false;
        }
    }

    public bool CheckParkingZoneExists(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                "SELECT id FROM Parking_Zones WHERE id = @ParkingZoneId",
                new { ParkingZoneId });
            
            if (!result.HasValue)
            {
                Console.Clear();
                Animation.PrintRedText($"Ошибка: парковочная зона с ID {ParkingZoneId} не найдена!", true, true, 50);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка проверки парковочной зоны: {ex.Message}", true, true, 50);
            return false;
        }
    }

    public bool Upload(SqlConnection connection)
    {
        if (!CheckUserExists(connection))
            return false;
            
        if (!CheckVehicleAvailable(connection))
            return false;
            
        if (!CheckParkingZoneExists(connection))
            return false;

        using var transaction = connection.BeginTransaction();
        try
        {
            string rentalSql = @"
                INSERT INTO Rentals (UserId, Start_trip, Parking_zone, Vehicle) 
                VALUES (@UserId, @StartTrip, @ParkingZoneId, @VehicleId)";
            
            int affectedRows = connection.Execute(rentalSql, new 
            { 
                UserId, 
                StartTrip, 
                ParkingZoneId, 
                VehicleId 
            }, transaction);

            string vehicleSql = "UPDATE Vehicles SET Status = 2 WHERE id = @VehicleId";
            connection.Execute(vehicleSql, new { VehicleId }, transaction);

            transaction.Commit();
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.Clear();
            Animation.PrintRedText($"Ошибка оформления аренды: {ex.Message}", true, true, 50);
            
            
            return false;
        }
    }
}