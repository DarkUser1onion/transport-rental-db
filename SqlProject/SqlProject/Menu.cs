using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlTypes;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MyApp;

public class Menu
{
    private bool exit = false;
    
    public SqlConnection conn {private get; set;}
    interface IUploadInServer
    {
        public bool Upload(SqlConnection connection);
    }

    public Menu(SqlConnection con) 
        => conn = con;
    
    
    private string ReadLineWithCancel()
    {
        var input = new StringBuilder();
        while (true)
        {
        
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
            
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }
                else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    exit = true;
                    return string.Empty;
                }
            }
            else
            {
                Thread.Sleep(50);
            }
        }
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
    
    public static void StartMenu()
    {
        Console.Clear();
        Animation.PrintRedText("Управление", true, false, 70);
        
        Animation.PrintSetCursor("1) Регистрация профиля", 22, true, 9);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Управление профилями", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("3) Управление автопарком", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("4) Оформление аренды", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("5) Завершение аренды", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("6) Узнать геолокацию транспортов", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("7) Сформировать отчет", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("8) Выход", 22);
        Console.WriteLine("\n\n");
        Animation.PrintSetCursor("Выбор: ", 22);
    }

    public UserRegistaryForm RegistrationProfile()
    {
        exit = false;
        UserRegistaryForm user = new UserRegistaryForm();
        
        Console.Clear();
        
        Animation.PrintRedText("Регистрация профиля", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите ваше имя: ", 27, true, 6);
        user.name = ReadLineWithCancel();
        if (exit)
            return null;
        Animation.PrintSetCursor("Введите вашу фамилию: ", 27);
        user.familyName = ReadLineWithCancel();
        if (exit)
            return null;
        Animation.PrintSetCursor("Введите ваш email: ", 27);
        user.email = ReadLineWithCancel();
        if (exit)
            return null;
        Animation.PrintSetCursor("Введите ваш номер карты (напр. 1234567891234567): ", 27);

        Decimal numberCard;

        int cursorPosition = Console.GetCursorPosition().Left;

        while (true)
        {
            string temp = ReadLineWithCancel();;
            if (!Decimal.TryParse(temp, out numberCard))
            {
                if (exit)
                    return null;
                
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }

            if (numberCard.ToString().Length == 16)
                break;

            Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
        }

        user.numberCard = numberCard;

        Animation.PrintSetCursor("Введите ваш CVC код (напр. 123): ", 27);

        cursorPosition = Console.GetCursorPosition().Left;

        int number;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (!int.TryParse(temp, out number))
            {
                if (exit)
                    return null;
                
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }

            if (number.ToString().Length == 3)
                break;

            Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
        }

        user.cvcCode = number;

        Animation.PrintSetCursor("Введите срок действия карты (напр. 2030-12): ", 27);

        cursorPosition = Console.GetCursorPosition().Left;

        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit)
                return null;
            
            try
            {
                user.Validity = DateTime.Parse(temp);
                break;
            }
            catch (Exception)
            {
                Animation.AnimationText("Ошибка даты!", true, cursorPosition, temp.Length);
            }

        }

        Console.Clear();
        
        Animation.AnimationText("Загрузка данных на сервер", false, 0, 0, true);
        
        return user;
    }
    

    public void ManagementProfileMenu(SqlConnection connection)
    {
        exit = false;
        
        Console.Clear();
        
        Animation.PrintRedText("Управление профилем", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Что нужно сделать?", 27, true, 6);
        Console.WriteLine();
        Animation.PrintSetCursor("1) Сменить кошелек", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Сменить имя пользователя", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("3) Сменить фамилию пользователя", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("4) Удалить пользователя", 27);
        Console.WriteLine("\n\n");
        Animation.PrintSetCursor("Выбор: ", 27);

        int cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if(exit)
                return;
            if (!int.TryParse(temp, out choice))
            {
                if (exit)
                    return;
                
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }

            if (int.Parse(temp) >= 1 && int.Parse(temp) <= 4)
                break;
            

            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }


        switch ((CaseChangeProfile)choice)
        {
            case CaseChangeProfile.Wallet:
                ManagementChangeWallet();
                break;
            case CaseChangeProfile.Name:
                ManagementChangeName();
                break;
            case CaseChangeProfile.FamilyName:
                ManagementChangeFamilyName();
                break;
            case CaseChangeProfile.Delete:
                ManagementRemoveUser();
                break;
            default:
                throw CheckoutException.Canceled;
        }
    }

    public bool ManagementRemoveUser()
    {
        exit = false;
        RemoveUser remover = new RemoveUser();
        
        Console.Clear();
        Animation.PrintRedText("Удаление пользователя", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите данные пользователя для удаления", 35, true, 4);
        Console.WriteLine();
        Animation.PrintSetCursor("Имя: ", 35);
        remover.name = ReadLineWithCancel();
        if (exit)
            return false;
        Animation.PrintSetCursor("Фамилия: ", 35);
        remover.familyName = ReadLineWithCancel();
        if (exit)
            return false;
        Animation.PrintSetCursor("Email: ", 35);
        remover.email = ReadLineWithCancel();
        if (exit)
            return false;
        
        Console.Clear();
        Animation.AnimationText("Поиск пользователя...", false, 0, 0, true);
        
        if (!remover.SearchData(conn))
        {
            Animation.PrintRedText("Пользователь не найден!", true, true, 50);
            return false;
        }
        
        if (remover.CheckRentals(conn))
        {
            Animation.PrintRedText("У пользователя есть активные аренды! Удаление невозможно.", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Вы уверены, что хотите удалить пользователя?", true, false, 50);
        Console.WriteLine();
        Animation.PrintSetCursor("1) Да", 18, true, 3);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Нет", 18);
        Console.WriteLine();
        Animation.PrintSetCursor("Выбор: ", 18);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit)
                return false;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (choice == 1 || choice == 2)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        if (choice == 2)
        {
            Console.Clear();
            Animation.PrintRedText("Удаление отменено!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.AnimationText("Удаление пользователя...", false, 0, 0, true);
        
        if (!remover.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка удаления пользователя!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Пользователь успешно удален!", true, true, 50);
            return true;
        }
    }
    
    public bool ManagementChangeFamilyName()
    {
        exit = false;
        ChangeNameOrFamilyName changer = new ChangeNameOrFamilyName();
        
        Console.Clear();
        Animation.PrintRedText("Смена фамилии", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите данные", 18, true, 4);
        Console.WriteLine();
        Animation.PrintSetCursor("Имя пользователя: ", 18);
        changer.name = ReadLineWithCancel();
        if(exit)
            return false;
        Animation.PrintSetCursor("Почта пользователя: ", 18);
        changer.email = ReadLineWithCancel();
        if(exit)
            return false;
        
        Console.Clear();
        
        Animation.AnimationText("Запрос данных...", false, 0, 0, true);
        
        if (changer.SearchDataFamily(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ввод новых данных", true, false, 50);
            Console.WriteLine();
            Animation.PrintRedText("Выход на Esc", true, false, 50);
            
            Animation.PrintSetCursor("Введите ваше новую фамилию: ", 27, true, 1);
            changer.familyName = ReadLineWithCancel();
            
            
            Console.Clear();
            Animation.AnimationText("Загрузка данных на сервер...", false, 0, 0, true);
            
            if (!changer.Upload(conn))
            {
                Console.Clear();
                Animation.PrintRedText("Ошибка загрузки данных!", true, true, 50);
                
                return false;
            }
            else
            {
                Console.Clear();
                Animation.PrintRedText("Успешно!", true, true, 50);
                
                return true;
            }
        }
        else
        {
            Animation.PrintRedText("Такого пользователя нету!", true, true, 50);
            return false;
        }
    }
    
    public bool ManagementChangeName()
    {
        exit = false;
        ChangeNameOrFamilyName changer = new ChangeNameOrFamilyName();
        
        Console.Clear();
        Animation.PrintRedText("Смена имени", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите данные", 18, true, 4);
        Console.WriteLine();
        Animation.PrintSetCursor("Фамилия пользователя: ", 18);
        changer.familyName = ReadLineWithCancel();
        if(exit)
            return false;
        Animation.PrintSetCursor("Почта пользователя: ", 18);
        changer.email = ReadLineWithCancel();
        if(exit)
            return false;
        
        Console.Clear();
        
        Animation.AnimationText("Запрос данных...", false, 0, 0, true);
        
        
        
        Console.Clear();
        if (changer.SearchData(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ввод новых данных", true, false, 50);
            Console.WriteLine();
            Animation.PrintRedText("Выход на Esc", true, false, 50);
            
            Animation.PrintSetCursor("Введите ваше новое имя: ", 27, true, 1);
            changer.name = ReadLineWithCancel();
            
            
            Console.Clear();
            Animation.AnimationText("Загрузка данных на сервер...", false, 0, 0, true);
            

            if (!changer.Upload(conn))
            {
                Console.Clear();
                Animation.PrintRedText("Ошибка загрузки данных!", true, true, 50);
                
                return false;
            }
            else
            {
                Console.Clear();
                Animation.PrintRedText("Успешно!", true, true, 50);
                
                return true;
            }
        }
        else
        {
            Animation.PrintRedText("Такого пользователя нету!", true, true, 50);
            return false;
        }
    }
    public bool ManagementChangeWallet()
    {
        exit = false;
        ChangeWallet wallet = new ChangeWallet();
        Console.Clear();
        
        Animation.PrintRedText("Смена кошелька", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        
        Animation.PrintSetCursor("Введите данные", 18, true, 4);
        Console.WriteLine();
        Animation.PrintSetCursor("Имя пользователя: ", 18);
        wallet.name = ReadLineWithCancel();
        if(exit)
            return false;
        Animation.PrintSetCursor("Почта пользователя: ", 18);
        wallet.email = ReadLineWithCancel();
        if(exit)
            return false;
        
        Console.Clear();
        
        Animation.AnimationText("Запрос данных...", false, 0, 0, true);

        if (wallet.SearchData(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ввод данных кошелька", true, false, 50);
            Console.WriteLine();
            Animation.PrintRedText("Выход на Esc", true, false, 50);
            
            Animation.PrintSetCursor("Введите ваш номер карты (напр. 1234567891234567): ", 40, true, 3);

            Decimal numberCard;

            int cursorPosition = Console.GetCursorPosition().Left;

            while (true)
            {
                string temp = ReadLineWithCancel();;
                if (!Decimal.TryParse(temp, out numberCard))
                {
                    if (exit)
                        return false;
                    
                    Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                    continue;
                }

                if (numberCard.ToString().Length == 16)
                    break;

                Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
            }

            wallet.numberCard = numberCard;

            Animation.PrintSetCursor("Введите ваш CVC код (напр. 123): ", 40);

            cursorPosition = Console.GetCursorPosition().Left;

            int number;
            while (true)
            {
                string temp = ReadLineWithCancel();
                if (!int.TryParse(temp, out number))
                {
                    if (exit)
                        return false;
                    
                    Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                    continue;
                }

                if (number.ToString().Length == 3)
                    break;

                Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
            }

            wallet.cvcCode = number;

            Animation.PrintSetCursor("Введите срок действия карты (напр. 2030-12): ", 40);

            cursorPosition = Console.GetCursorPosition().Left;

            while (true)
            {
                string temp = ReadLineWithCancel();
                if (exit)
                    return false;
                
                try
                {
                    wallet.Validity = DateTime.Parse(temp);
                    break;
                }
                catch (Exception)
                {
                    Animation.AnimationText("Ошибка даты!", true, cursorPosition, temp.Length);
                }

            }
            
            Console.Clear();
            Animation.AnimationText("Загрузка данных на сервер...", false, 0, 0, true);

            
            if (!wallet.Upload(conn))
            {
                Console.Clear();
                Animation.PrintRedText("Ошибка загрузки данных!", true, true, 50);
                
                return false;
            }
            else
            {
                Console.Clear();
                Animation.PrintRedText("Успешно!", true, true, 50);
            }
            return true;
        }
        else
        {
            Animation.PrintRedText("Такого кошелька нету!", true, true, 50);
            return false;
        }
    }
    
    public void ManagementFleetMenu(SqlConnection connection)
    {
        exit = false;
        
        Console.Clear();
        Animation.PrintRedText("Управление автопарком", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Что нужно сделать?", 27, true, 6);
        Console.WriteLine();
        Animation.PrintSetCursor("1) Добавить транспорт", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Удалить транспорт", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("3) Изменить статус транспорта", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("4) Просмотреть весь транспорт", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("5) Управление статусами", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("6) Добавить парковочную зону", 27);
        Console.WriteLine("\n\n");
        Animation.PrintSetCursor("Выбор: ", 27);

        int cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if(exit)
                return;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }

            if (choice >= 1 && choice <= 6)
                break;

            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }

        switch ((CaseFleetManagement)choice)
        {
            case CaseFleetManagement.AddVehicle:
                ManagementAddVehicle();
                break;
            case CaseFleetManagement.RemoveVehicle:
                ManagementRemoveVehicle();
                break;
            case CaseFleetManagement.ChangeStatus:
                ManagementChangeVehicleStatus();
                break;
            case CaseFleetManagement.ViewAll:
                ManagementViewAllVehicles();
                break;
            case CaseFleetManagement.ManageStatuses:
                ManagementStatusMenu(connection);
                break;
            case CaseFleetManagement.AddParkingZone:
                ManagementAddParkingZone();
                break;
            default:
                throw CheckoutException.Canceled;
        }
    }

    public bool ManagementAddVehicle()
    {
        exit = false;
        AddVehicle vehicle = new AddVehicle();
        
        Console.Clear();
        Animation.PrintRedText("Добавление транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите QR-код транспорта (19 символов): ", 27, true, 1);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        string qrCode;
        while (true)
        {
            qrCode = ReadLineWithCancel();
            if (exit) return false;
            
            if (qrCode.Length == 19)
                break;
                
            Animation.AnimationText("QR-код должен быть 19 символов!", true, cursorPosition, qrCode.Length);
        }
        vehicle.QrCode = qrCode;
        
        Animation.PrintSetCursor("Введите тип транспорта: ", 27);
        vehicle.TypeTransport = ReadLineWithCancel();
        if (exit) return false;
        
        Console.Clear();
        Animation.AnimationText("Загрузка списка статусов...", false, 0, 0, true);
        
        var statuses = conn.Query("SELECT * FROM Status").ToList();
        
        Console.Clear();
        Animation.PrintRedText("Выберите статус транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        Console.WriteLine();
        Console.WriteLine();
        
        for (int i = 0; i < statuses.Count; i++)
        {
            Animation.PrintSetCursor($"{i + 1}) {statuses[i].Type_status} - {statuses[i].Description}", 27);
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 5, true, 1);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (choice >= 1 && choice <= statuses.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        vehicle.StatusId = statuses[choice - 1].id;
        
        Console.Clear();
        Animation.AnimationText("Добавление транспорта...", false, 0, 0, true);
        
        if (!vehicle.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка добавления транспорта!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Транспорт успешно добавлен!", true, true, 50);
            return true;
        }
    }

    public bool ManagementRemoveVehicle()
    {
        exit = false;
        RemoveVehicle remover = new RemoveVehicle();
        
        Console.Clear();
        Animation.PrintRedText("Удаление транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите QR-код транспорта для удаления (19 символов): ", 27, true, 6);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        string qrCode;
        while (true)
        {
            qrCode = ReadLineWithCancel();
            if (exit) return false;
            
            if (qrCode.Length == 19)
                break;
                
            Animation.AnimationText("QR-код должен быть 19 символов!", true, cursorPosition, qrCode.Length);
        }
        remover.QrCode = qrCode;
        
        Console.Clear();
        Animation.AnimationText("Поиск транспорта...", false, 0, 0, true);
        
        if (!remover.SearchData(conn))
        {
            Animation.PrintRedText("Транспорт не найден!", true, true, 50);
            return false;
        }
        
        if (remover.CheckRentals(conn))
        {
            Animation.PrintRedText("У транспорта есть активные аренды! Удаление невозможно.", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Вы уверены, что хотите удалить транспорт?", true, false, 50);
        Console.WriteLine();
        Animation.PrintSetCursor("1) Да", 18, true, 3);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Нет", 18);
        Console.WriteLine();
        Animation.PrintSetCursor("Выбор: ", 18);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (choice == 1 || choice == 2)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        if (choice == 2)
        {
            Console.Clear();
            Animation.PrintRedText("Удаление отменено!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.AnimationText("Удаление транспорта...", false, 0, 0, true);
        
        if (!remover.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка удаления транспорта!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Транспорт успешно удален!", true, true, 50);
            return true;
        }
    }

    public bool ManagementChangeVehicleStatus()
    {
        exit = false;
        ChangeVehicleStatus changer = new ChangeVehicleStatus();
        
        Console.Clear();
        Animation.PrintRedText("Изменение статуса транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите QR-код транспорта (19 символов): ", 27, true, 1);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        string qrCode;
        while (true)
        {
            qrCode = ReadLineWithCancel();
            if (exit) return false;
            
            if (qrCode.Length == 19)
                break;
                
            Animation.AnimationText("QR-код должен быть 19 символов!", true, cursorPosition, qrCode.Length);
        }
        changer.QrCode = qrCode;
        
        Console.Clear();
        Animation.AnimationText("Поиск транспорта...", false, 0, 0, true);
        
        if (!changer.SearchData(conn))
        {
            Animation.PrintRedText("Транспорт не найден!", true, true, 50);
            return false;
        }
        
        var statuses = conn.Query("SELECT * FROM Status").ToList();
        
        Console.Clear();
        Animation.PrintRedText("Выберите новый статус транспорта", true, false, 50);
        Console.WriteLine();
        
        for (int i = 0; i < statuses.Count; i++)
        {
            Animation.PrintSetCursor($"{i + 1}) {statuses[i].Type_status} - {statuses[i].Description}", 27);
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 27);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (choice >= 1 && choice <= statuses.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        changer.NewStatusId = statuses[choice - 1].id;
        
        Console.Clear();
        Animation.AnimationText("Изменение статуса...", false, 0, 0, true);
        
        if (!changer.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка изменения статуса!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Статус успешно изменен!", true, true, 50);
            return true;
        }
    }

    public void ManagementViewAllVehicles()
    {
        Console.Clear();
        Animation.AnimationText("Загрузка данных...", false, 0, 0, true);
        
        try
        {
            var vehicles = conn.Query(@"
                SELECT V.id, V.QrCode, V.Type_Transport, S.Type_status, S.Description
                FROM Vehicles V 
                INNER JOIN Status S ON V.Status = S.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("Список всего транспорта", true, false, 50);
            Console.WriteLine();
            
            if (vehicles.Count == 0)
            {
                Animation.PrintSetCursor("Транспорт не найден", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor("ID".PadRight(5) + "QR-код".PadRight(21) + "Тип".PadRight(20) + "Статус".PadRight(15) + "Описание", 80, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 80), 80);
                Console.WriteLine();
                
                foreach (var vehicle in vehicles)
                {
                    Animation.PrintSetCursor(
                        vehicle.id.ToString().PadRight(5) + 
                        vehicle.QrCode.PadRight(21) + 
                        vehicle.Type_Transport.PadRight(20) + 
                        vehicle.Type_status.PadRight(15) + 
                        vehicle.Description, 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 80);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка загрузки данных: {ex.Message}", true, true, 50);
        }
    }
    
    public void ManagementStatusMenu(SqlConnection connection)
    {
        exit = false;
    
        while (!exit)
        {
            Console.Clear();
            Animation.PrintRedText("Управление статусами", true, false, 50);
            Console.WriteLine();
            Animation.PrintRedText("Выход на Esc", true, false, 50);
        
            Animation.PrintSetCursor("Что нужно сделать?", 27, true, 6);
            Console.WriteLine();
            Animation.PrintSetCursor("1) Добавить новый статус", 27);
            Console.WriteLine();
            Animation.PrintSetCursor("2) Просмотреть все статусы", 27);
            Console.WriteLine();
            Animation.PrintSetCursor("3) Просмотреть парковочные зоны", 27);
            Console.WriteLine();
            Animation.PrintSetCursor("4) Назад в меню автопарка", 27);
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Выбор: ", 27);

            int cursorPosition = Console.GetCursorPosition().Left;
            int choice;
            while (true)
            {
                string temp = ReadLineWithCancel();
                if(exit)
                    return;
                if (!int.TryParse(temp, out choice))
                {
                    Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                    continue;
                }

                if (choice >= 1 && choice <= 4)
                    break;

                Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
            }

            switch ((CaseStatusManagement)choice)
            {
                case CaseStatusManagement.AddStatus:
                    ManagementAddStatus();
                    break;
                case CaseStatusManagement.ViewStatuses:
                    ManagementViewAllStatuses();
                    break;
                case CaseStatusManagement.ViewParkingZones:
                    ManagementViewAllParkingZones();
                    break;
                case CaseStatusManagement.Back:
                    return;
                default:
                    throw new Exception("Неизвестный выбор");
            }
        }
    }
    
    public bool ManagementAddStatus()
    {
        exit = false;
        AddStatus status = new AddStatus();
    
        Console.Clear();
        Animation.PrintRedText("Добавление нового статуса", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
    
        Animation.PrintSetCursor("Введите название статуса: ", 27, true, 6);
        status.TypeStatus = ReadLineWithCancel();
        if (exit)
            return false;
    
        Animation.PrintSetCursor("Введите описание статуса: ", 27);
        status.Description = ReadLineWithCancel();
        if (exit)
            return false;
    
        Console.Clear();
        Animation.AnimationText("Добавление статуса...", false, 0, 0, true);
    
        if (!status.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка добавления статуса!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Статус успешно добавлен!", true, true, 50);
            return true;
        }
    }
    
    public void ManagementViewAllStatuses()
    {
        Console.Clear();
        Animation.AnimationText("Загрузка данных...", false, 0, 0, true);
    
        try
        {
            var statuses = conn.Query("SELECT * FROM Status").ToList();
        
            Console.Clear();
            Animation.PrintRedText("Список всех статусов", true, false, 50);
            Console.WriteLine();
        
            if (statuses.Count == 0)
            {
                Animation.PrintSetCursor("Статусы не найдены", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor("ID".PadRight(5) + "Название".PadRight(20) + "Описание", 20, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 60), 20);
                Console.WriteLine();
            
                foreach (var status in statuses)
                {
                    Animation.PrintSetCursor(
                        status.id.ToString().PadRight(5) + 
                        status.Type_status.PadRight(20) + 
                        status.Description, 20);
                    Console.WriteLine();
                }
            }
        
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка загрузки данных: {ex.Message}", true, true, 50);
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
                    @"SELECT V.id FROM Vehicles V 
                      WHERE V.id = @VehicleId AND V.Status = 1",
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
                
                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    Animation.PrintRedText("Проблема с ссылочной целостностью данных.", true, true, 50);
                    Animation.PrintRedText("Проверьте существование UserId, VehicleId и ParkingZoneId.", true, true, 50);
                }
                
                return false;
            }
        }
    }

    public bool StartRent()
    {
        exit = false;
        StartRental rental = new StartRental();
        
        Console.Clear();
        Animation.PrintRedText("Оформление аренды", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Console.Clear();
        Animation.AnimationText("Проверка данных...", false, 0, 0, true);
        
        try
        {
            var userCount = conn.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Users");
            var vehicleCount = conn.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Vehicles WHERE Status = 1");
            var zoneCount = conn.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Parking_Zones");
            
            Console.Clear();
            Animation.PrintRedText("Статус системы:", true, false, 50);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Animation.PrintSetCursor($"Пользователей: {userCount}", 20);
            Console.WriteLine();
            Animation.PrintSetCursor($"Доступного транспорта: {vehicleCount}", 20);
            Console.WriteLine();
            Animation.PrintSetCursor($"Парковочных зон: {zoneCount}", 20);
            Console.WriteLine("\n\n");
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
            
            if (userCount == 0)
            {
                Animation.PrintRedText("В системе нет зарегистрированных пользователей!", true, true, 50);
                Animation.PrintRedText("Сначала зарегистрируйте пользователя.", true, true, 50);
                return false;
            }
            
            if (vehicleCount == 0)
            {
                Animation.PrintRedText("Нет доступного транспорта для аренды!", true, true, 50);
                return false;
            }
            
            if (zoneCount == 0)
            {
                Animation.PrintRedText("В системе нет парковочных зон!", true, true, 50);
                Animation.PrintRedText("Сначала добавьте парковочные зоны.", true, true, 50);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка проверки данных: {ex.Message}", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.AnimationText("Загрузка списка пользователей...", false, 0, 0, true);
        
        var users = conn.Query("SELECT id, Name, Family_Name, Email FROM Users").ToList();
        
        Console.Clear();
        Animation.PrintRedText("Выберите пользователя", true, false, 50);
        Console.WriteLine();
        
        for (int i = 0; i < users.Count; i++)
        {
            string displayText = $"{i + 1}) ID:{users[i].id} {users[i].Name} {users[i].Family_Name} ({users[i].Email})";
            if (displayText.Length > Console.WindowWidth - 10)
            {
                Console.WriteLine(displayText);
            }
            else
            {
                Animation.PrintSetCursor(displayText, Math.Min(displayText.Length + 10, Console.WindowWidth - 5));
                Console.WriteLine();
            }
        }
        
        Animation.PrintSetCursor("Выбор: ", 50);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        int userChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out userChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (userChoice >= 1 && userChoice <= users.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        rental.UserId = users[userChoice - 1].id;
        
        Console.Clear();
        Animation.AnimationText("Загрузка свободного транспорта...", false, 0, 0, true);
        
        var vehicles = conn.Query(@"SELECT V.id, V.QrCode, V.Type_Transport 
                                   FROM Vehicles V 
                                   WHERE V.Status = 1").ToList();
        
        if (vehicles.Count == 0)
        {
            Animation.PrintRedText("Нет свободного транспорта!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Выберите транспорт", true, false, 50);
        Console.WriteLine();
        
        for (int i = 0; i < vehicles.Count; i++)
        {
            Animation.PrintSetCursor($"{i + 1}) {vehicles[i].Type_Transport} (QR: {vehicles[i].QrCode})", 27);
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 50);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int vehicleChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out vehicleChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (vehicleChoice >= 1 && vehicleChoice <= vehicles.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        rental.VehicleId = vehicles[vehicleChoice - 1].id;
        
        Console.Clear();
        Animation.AnimationText("Загрузка парковочных зон...", false, 0, 0, true);
        
        var parkingZones = conn.Query("SELECT id, Approximate_address FROM Parking_Zones").ToList();
        
        Console.Clear();
        Animation.PrintRedText("Выберите парковочную зону", true, false, 50);
        Console.WriteLine();
        
        for (int i = 0; i < parkingZones.Count; i++)
        {
            string displayText = $"{i + 1}) {parkingZones[i].Approximate_address}";
            if (displayText.Length > Console.WindowWidth - 10)
            {
                Console.WriteLine(displayText);
            }
            else
            {
                Animation.PrintSetCursor(displayText, Math.Min(displayText.Length + 10, Console.WindowWidth - 5));
                Console.WriteLine();
            }
        }
        
        Console.WriteLine();
        Animation.PrintSetCursor("Выбор: ", 20);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int zoneChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out zoneChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (zoneChoice >= 1 && zoneChoice <= parkingZones.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        rental.ParkingZoneId = parkingZones[zoneChoice - 1].id;
        
        rental.StartTrip = DateTime.Now;
        
        Console.Clear();
        Animation.AnimationText("Оформление аренды...", false, 0, 0, true);
        
        if (!rental.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка оформления аренды!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Аренда успешно оформлена!", true, true, 50);
            return true;
        }
    }
    
    public class EndRental : IUploadInServer
    {
    public int RentalId { private get; set; }
    public DateTime ActualEndTime { private get; set; }

    public EndRental() { }

    public bool CheckRentalExists(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                "SELECT id FROM Rentals WHERE id = @RentalId AND End_trip > GETDATE()",
                new { RentalId });
            return result.HasValue;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public int GetVehicleId(SqlConnection connection)
    {
        try
        {
            var result = connection.QueryFirstOrDefault<int?>(
                "SELECT Vehicle FROM Rentals WHERE id = @RentalId",
                new { RentalId });
            return result ?? 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public bool Upload(SqlConnection connection)
    {
        using var transaction = connection.BeginTransaction();
        try
        {
            string rentalSql = "UPDATE Rentals SET End_trip = @ActualEndTime WHERE id = @RentalId";
            int affectedRows = connection.Execute(rentalSql, new 
            { 
                ActualEndTime, 
                RentalId 
            }, transaction);

            int vehicleId = GetVehicleId(connection);
            
            string vehicleSql = "UPDATE Vehicles SET Status = 1 WHERE id = @VehicleId";
            connection.Execute(vehicleSql, new { VehicleId = vehicleId }, transaction);

            transaction.Commit();
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.Clear();
            Animation.PrintRedText($"Ошибка завершения аренды!", true, true, 50);
            return false;
        }
    }
}

    public bool EndRent()
    {
        exit = false;
        EndRental endRental = new EndRental();
        
        Console.Clear();
        Animation.PrintRedText("Завершение аренды", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Console.Clear();
        Animation.AnimationText("Загрузка активных аренд...", false, 0, 0, true);
        
        var activeRentals = conn.Query(@"
            SELECT R.id, U.Name, U.Family_Name, V.Type_Transport, R.Start_trip
            FROM Rentals R
            INNER JOIN Users U ON R.UserId = U.id
            INNER JOIN Vehicles V ON R.Vehicle = V.id
            WHERE R.End_trip IS NULL").ToList();
        
        if (activeRentals.Count == 0)
        {
            Animation.PrintRedText("Нет активных аренд!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Выберите аренду для завершения", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        Console.WriteLine("\n\n\n");
        
        for (int i = 0; i < activeRentals.Count; i++)
        {
            Animation.PrintSetCursor($"{i + 1}) {activeRentals[i].Name} {activeRentals[i].Family_Name} - " +
                                   $"{activeRentals[i].Type_Transport} (начало: {activeRentals[i].Start_trip:g})", 27);
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 50);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        int rentalChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out rentalChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (rentalChoice >= 1 && rentalChoice <= activeRentals.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        endRental.RentalId = activeRentals[rentalChoice - 1].id;
        endRental.ActualEndTime = DateTime.Now;
        
        Console.Clear();
        Animation.AnimationText("Завершение аренды...", false, 0, 0, true);
        
        if (!endRental.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка завершения аренды!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Аренда успешно завершена!", true, true, 50);
            return true;
        }
    }
    
    public void FindGeo()
    {
        Console.Clear();
        Animation.AnimationText("Загрузка данных о местоположении...", false, 0, 0, true);
        
        try
        {
            var vehicleLocations = conn.Query(@"
                    SELECT 
                        V.id,
                        V.QrCode,
                        V.Type_Transport,
                        S.Type_status as Status,
                        PZ.Approximate_address as Location,
                        CASE 
                            WHEN R.id IS NOT NULL AND R.End_trip IS NULL THEN 'В аренде'
                            ELSE 'Свободен'
                        END as RentalStatus,
                        U.Name + ' ' + U.Family_Name as RentedBy
                    FROM Vehicles V
                    INNER JOIN Status S ON V.Status = S.id
                    LEFT JOIN Rentals R ON R.Vehicle = V.id AND R.End_trip IS NULL
                    LEFT JOIN Parking_Zones PZ ON R.Parking_zone = PZ.id
                    LEFT JOIN Users U ON R.UserId = U.id
                    ORDER BY V.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("Геолокация транспортов", true, false, 50);
            Console.WriteLine("\n\n\n");
            
            if (vehicleLocations.Count == 0)
            {
                Animation.PrintSetCursor("Транспорт не найден", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor("ID".PadRight(5) + "QR-код".PadRight(21) + "Тип".PadRight(15) + 
                                       "Статус".PadRight(12) + "Аренда".PadRight(12) + "Местоположение", 50, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 90), 50);
                Console.WriteLine();
                
                foreach (var vehicle in vehicleLocations)
                {
                    string location = vehicle.Location ?? "Не указано";
                    string rentedBy = vehicle.RentedBy ?? "-";
                    
                    Animation.PrintSetCursor(
                        vehicle.id.ToString().PadRight(5) + 
                        vehicle.QrCode.PadRight(21) + 
                        vehicle.Type_Transport.PadRight(15) + 
                        vehicle.Status.PadRight(12) + 
                        vehicle.RentalStatus.PadRight(12) + 
                        location, 50);
                    Console.WriteLine();
                    
                    if (vehicle.RentalStatus == "В аренде")
                    {
                        Animation.PrintSetCursor("     Арендован: " + rentedBy, 50);
                        Console.WriteLine();
                    }
                }
            }
            
            Console.WriteLine("\n\n");
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 50);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка загрузки данных: {ex.Message}", true, true, 50);
        }
    }
    
    public bool ManagementAddParkingZone()
    {
        exit = false;
        AddParkingZone parkingZone = new AddParkingZone();
        
        Console.Clear();
        Animation.PrintRedText("Добавление парковочной зоны", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите ширину зоны (например: 12.345678): ", 27, true, 6);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        decimal width;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!decimal.TryParse(temp, out width))
            {
                Animation.AnimationText("Ошибка ввода! Введите число.", true, cursorPosition, temp.Length);
                continue;
            }
            break;
        }
        parkingZone.Width = width;
        
        Animation.PrintSetCursor("Введите длину зоны (например: 12.345678): ", 27);
        
        cursorPosition = Console.GetCursorPosition().Left;
        decimal length;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!decimal.TryParse(temp, out length))
            {
                Animation.AnimationText("Ошибка ввода! Введите число.", true, cursorPosition, temp.Length);
                continue;
            }
            break;
        }
        parkingZone.Length = length;
        
        Animation.PrintSetCursor("Введите приблизительный адрес: ", 27);
        parkingZone.ApproximateAddress = ReadLineWithCancel();
        if (exit) return false;
        
        Console.Clear();
        Animation.AnimationText("Добавление парковочной зоны...", false, 0, 0, true);
        
        if (!parkingZone.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка добавления парковочной зоны!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Парковочная зона успешно добавлена!", true, true, 50);
            return true;
        }
    }
    
    public void ManagementViewAllParkingZones()
    {
        Console.Clear();
        Animation.AnimationText("Загрузка данных...", false, 0, 0, true);
    
        try
        {
            var parkingZones = conn.Query("SELECT * FROM Parking_Zones").ToList();
        
            Console.Clear();
            Animation.PrintRedText("Список всех парковочных зон", true, false, 50);
            Console.WriteLine();
        
            if (parkingZones.Count == 0)
            {
                Animation.PrintSetCursor("Парковочные зоны не найдены", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor("ID".PadRight(5) + "Ширина".PadRight(12) + "Длина".PadRight(12) + "Адрес", 20, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 80), 20);
                Console.WriteLine();
            
                foreach (var zone in parkingZones)
                {
                    Animation.PrintSetCursor(
                        zone.id.ToString().PadRight(5) + 
                        zone.Width.ToString().PadRight(12) + 
                        zone.Length.ToString().PadRight(12) + 
                        zone.Approximate_address, 20);
                    Console.WriteLine();
                }
            }
        
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка загрузки данных: {ex.Message}", true, true, 50);
        }
    }
    
    public void GenerateReport()
    {
        Console.Clear();
        Animation.AnimationText("Формирование полного отчета...", false, 0, 0, true);
        
        try
        {
            var generalStats = conn.Query(@"
                SELECT 
                    (SELECT COUNT(*) FROM Users) as TotalUsers,
                    (SELECT COUNT(*) FROM Vehicles) as TotalVehicles,
                    (SELECT COUNT(*) FROM Parking_Zones) as TotalParkingZones,
                    (SELECT COUNT(*) FROM Rentals) as TotalRentals,
                    (SELECT COUNT(*) FROM Rentals WHERE End_trip IS NULL) as ActiveRentals,
                    (SELECT COUNT(*) FROM Status) as TotalStatuses,
                    (SELECT COUNT(*) FROM Wallets) as TotalWallets
            ").First();
            
            Console.Clear();
            Animation.PrintRedText("ПОЛНЫЙ ОТЧЕТ СИСТЕМЫ АРЕНДЫ", true, false, 50);
            Console.WriteLine();
            
            Animation.PrintSetCursor("=== ОБЩАЯ СТАТИСТИКА ===", 20, true, 2);
            Console.WriteLine();
            Animation.PrintSetCursor($"Пользователей: {generalStats.TotalUsers}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Транспортных средств: {generalStats.TotalVehicles}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Парковочных зон: {generalStats.TotalParkingZones}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Всего аренд: {generalStats.TotalRentals}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Активных аренд: {generalStats.ActiveRentals}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Статусов: {generalStats.TotalStatuses}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Кошельков: {generalStats.TotalWallets}", 40);
            Console.WriteLine("\n\n");
            
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
            
            var users = conn.Query(@"
                SELECT U.id, U.Name, U.Family_Name, U.Email, W.Number_card, W.Validity,
                       (SELECT COUNT(*) FROM Rentals R WHERE R.UserId = U.id) as RentalCount
                FROM Users U 
                INNER JOIN Wallets W ON U.Wallet = W.id
                ORDER BY U.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("ОТЧЕТ: ПОЛЬЗОВАТЕЛИ", true, false, 50);
            Console.WriteLine();
            
            if (users.Count == 0)
            {
                Animation.PrintSetCursor("Пользователи не найдены", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor($"Всего пользователей: {users.Count}", 20, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor("ID".PadRight(5) + "Имя".PadRight(15) + "Фамилия".PadRight(15) + 
                                       "Email".PadRight(25) + "Аренд".PadRight(8) + "Номер карты".PadRight(20) + "Срок действия", 80);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 110), 80);
                Console.WriteLine();
                
                foreach (var user in users)
                {
                    Animation.PrintSetCursor(
                        user.id.ToString().PadRight(5) + 
                        user.Name.PadRight(15) + 
                        user.Family_Name.PadRight(15) + 
                        user.Email.PadRight(25) + 
                        user.RentalCount.ToString().PadRight(8) + 
                        user.Number_card.ToString().PadRight(20) + 
                        user.Validity.ToString("MM/yyyy"), 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
            
            var vehicles = conn.Query(@"
                SELECT V.id, V.QrCode, V.Type_Transport, S.Type_status as Status, S.Description,
                       (SELECT COUNT(*) FROM Rentals R WHERE R.Vehicle = V.id) as RentalCount
                FROM Vehicles V 
                INNER JOIN Status S ON V.Status = S.id
                ORDER BY V.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("ОТЧЕТ: ТРАНСПОРТ", true, false, 80);
            Console.WriteLine();
            
            if (vehicles.Count == 0)
            {
                Animation.PrintSetCursor("Транспорт не найден", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor($"Всего транспортных средств: {vehicles.Count}", 20, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor("ID".PadRight(5) + "QR-код".PadRight(21) + "Тип".PadRight(15) + 
                                       "Статус".PadRight(12) + "Аренд".PadRight(8) + "Описание", 80);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 90), 80);
                Console.WriteLine();
                
                foreach (var vehicle in vehicles)
                {
                    Animation.PrintSetCursor(
                        vehicle.id.ToString().PadRight(5) + 
                        vehicle.QrCode.PadRight(21) + 
                        vehicle.Type_Transport.PadRight(15) + 
                        vehicle.Status.PadRight(12) + 
                        vehicle.RentalCount.ToString().PadRight(8) + 
                        vehicle.Description, 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
            
            var parkingZones = conn.Query(@"
                SELECT PZ.id, PZ.Width, PZ.Length, PZ.Approximate_address,
                       (SELECT COUNT(*) FROM Rentals R WHERE R.Parking_zone = PZ.id) as RentalCount
                FROM Parking_Zones PZ
                ORDER BY PZ.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("ОТЧЕТ: ПАРКОВОЧНЫЕ ЗОНЫ", true, false, 50);
            Console.WriteLine();
            
            if (parkingZones.Count == 0)
            {
                Animation.PrintSetCursor("Парковочные зоны не найдены", 80, true, 2);
            }
            else
            {
                Animation.PrintSetCursor($"Всего парковочных зон: {parkingZones.Count}", 80, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor("ID".PadRight(5) + "Ширина".PadRight(12) + "Длина".PadRight(12) + 
                                       "Аренд".PadRight(8) + "Адрес", 80);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 80), 80);
                Console.WriteLine();
                
                foreach (var zone in parkingZones)
                {
                    Animation.PrintSetCursor(
                        zone.id.ToString().PadRight(5) + 
                        zone.Width.ToString().PadRight(12) + 
                        zone.Length.ToString().PadRight(12) + 
                        zone.RentalCount.ToString().PadRight(8) + 
                        zone.Approximate_address, 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
            
            var rentals = conn.Query(@"
                SELECT 
                    R.id,
                    U.Name + ' ' + U.Family_Name as UserName,
                    V.Type_Transport as VehicleType,
                    V.QrCode,
                    PZ.Approximate_address as ParkingLocation,
                    R.Start_trip,
                    R.End_trip,
                    CASE 
                        WHEN R.End_trip IS NULL THEN 'Активна'
                        ELSE 'Завершена'
                    END as Status,
                    DATEDIFF(MINUTE, R.Start_trip, ISNULL(R.End_trip, GETDATE())) as DurationMinutes
                FROM Rentals R
                INNER JOIN Users U ON R.UserId = U.id
                INNER JOIN Vehicles V ON R.Vehicle = V.id
                INNER JOIN Parking_Zones PZ ON R.Parking_zone = PZ.id
                ORDER BY R.Start_trip DESC").ToList();
            
            Console.Clear();
            Animation.PrintRedText("ОТЧЕТ: ИСТОРИЯ АРЕНД", true, false, 50);
            Console.WriteLine();
            
            if (rentals.Count == 0)
            {
                Animation.PrintSetCursor("Аренды не найдены", 80, true, 2);
            }
            else
            {
                Animation.PrintSetCursor($"Всего аренд в истории: {rentals.Count}", 20, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor("ID".PadRight(5) + "Пользователь".PadRight(25) + "Транспорт".PadRight(15) + 
                                       "Начало".PadRight(20) + "Окончание".PadRight(20) + "Статус".PadRight(10) + "Длительность", 80);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 120), 80);
                Console.WriteLine();
                
                foreach (var rental in rentals)
                {
                    string endTime = rental.End_trip?.ToString("dd.MM.yyyy HH:mm") ?? "---";
                    string duration = rental.DurationMinutes >= 60 
                        ? $"{rental.DurationMinutes / 60}ч {rental.DurationMinutes % 60}м"
                        : $"{rental.DurationMinutes}м";
                    
                    Animation.PrintSetCursor(
                        rental.id.ToString().PadRight(5) + 
                        rental.UserName.PadRight(25) + 
                        rental.VehicleType.PadRight(15) + 
                        rental.Start_trip.ToString("dd.MM.yyyy HH:mm").PadRight(20) + 
                        endTime.PadRight(20) + 
                        rental.Status.PadRight(10) + 
                        duration, 80);
                    Console.WriteLine();
                }
            }
            
            var statuses = conn.Query("SELECT * FROM Status ORDER BY id").ToList();
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("=== СТАТУСЫ ТРАНСПОРТА ===", 20);
            Console.WriteLine();
            
            if (statuses.Count == 0)
            {
                Animation.PrintSetCursor("Статусы не найдены", 20);
            }
            else
            {
                foreach (var status in statuses)
                {
                    Animation.PrintSetCursor($"ID: {status.id} - {status.Type_status}: {status.Description}", 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("=== ОТЧЕТ СФОРМИРОВАН ===", 20);
            Console.WriteLine();
            Animation.PrintSetCursor("Нажмите любую клавишу для возврата в меню...", 40);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка формирования отчета: {ex.Message}", true, true, 50);
            Animation.PrintRedText("Нажмите любую клавишу для продолжения...", true, true, 50);
            Console.ReadKey();
        }
    }
    
}
