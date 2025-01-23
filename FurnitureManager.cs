```c# 
using System;
using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;


namespace mebeluveikals
{
    public class FurnitureManager
    {
        private readonly string connectionString;

        public FurnitureManager(string connectionString)
        {
            this.connectionString = connectionString;

            CreateFurnitureTable();
        }


        public void DeleteFurnitureByName(string name)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var deleteCmd = connection.CreateCommand();
                deleteCmd.CommandText = @"DELETE FROM Furniture WHERE Name = @name";
                deleteCmd.Parameters.AddWithValue("name", name);

                deleteCmd.ExecuteNonQuery();
            }
        }


        public Furniture ReadFurnitureByName(string name)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = @"SELECT * FROM Furniture WHERE Name = @name";
                selectCmd.Parameters.AddWithValue("name", name);

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return new Furniture(
                                reader["Name"].ToString(),
                                reader["Description"].ToString(),
                                Convert.ToDouble(reader["Price"]),
                                Convert.ToInt32(reader["Height"]),
                                Convert.ToInt32(reader["Width"]),
                                Convert.ToInt32(reader["Length"])
                            );
                    }
                }
            }

            throw new Exception("Furniture with such name not found");
        }

        public List<Furniture> ReadFurniture()
        {
            var furnitureList = new List<Furniture>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Furniture";

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var newFurniture = new Furniture(
                                reader["Name"].ToString(),
                                reader["Description"].ToString(),
                                Convert.ToDouble(reader["Price"]),
                                Convert.ToInt32(reader["Height"]),
                                Convert.ToInt32(reader["Width"]),
                                Convert.ToInt32(reader["Length"])
                            );
                        furnitureList.Add(newFurniture);
                    }
                }
            }

            return furnitureList;
        }

        public void AddFurniture(string name, string description, double price,
            int height, int width, int length)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var createRecordCommand = connection.CreateCommand();
                createRecordCommand.CommandText = @"INSERT INTO Furniture(Name, Description, Price, Height, Width, Length)
                VALUES (@name, @description, @price, @height, @width, @length)";

                createRecordCommand.Parameters.AddWithValue("name", name);
                createRecordCommand.Parameters.AddWithValue("description", description);
                createRecordCommand.Parameters.AddWithValue("price", price);
                createRecordCommand.Parameters.AddWithValue("height", height);
                createRecordCommand.Parameters.AddWithValue("width", width);
                createRecordCommand.Parameters.AddWithValue("length", length);

                createRecordCommand.ExecuteNonQuery();
            }
        }

        public void ExportFurnitureToCsv(string filePath)
        {
            try
            {
                var furnitureList = ReadFurniture();

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Name,Description,Price,Height,Width,Length");
                    foreach (var furniture in furnitureList)
                    {
                        writer.WriteLine($"{furniture.Name},{furniture.Description},{furniture.Price}," +
                                         $"{furniture.Height},{furniture.Width},{furniture.Length}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Radās kļūda eksportējot mēbeles uz CSV failu: " + ex.Message);
            }
        }
        public void ImportFurnitureFromCsv(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string line;
                    int lineNumber = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (lineNumber == 1) continue;
                        var columns = line.Split(',');

                        if (columns.Length != 6)
                        {
                            throw new Exception($"Kļūda CSV rindā {lineNumber}: Nepareizs kolonnu skaits.");
                        }

                        string name = columns[0];
                        string description = columns[1];
                        double price = Convert.ToDouble(columns[2]);
                        int height = Convert.ToInt32(columns[3]);
                        int width = Convert.ToInt32(columns[4]);
                        int length = Convert.ToInt32(columns[5]);
                        try
                        {
                            var existingFurniture = ReadFurnitureByName(name);
                            DeleteFurnitureByName(name);
                        }
                        catch (Exception)
                        {

                        }
                        AddFurniture(name, description, price, height, width, length);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Radās kļūda importējot mēbeles no CSV faila: " + ex.Message);
            }
        }

        private void CreateFurnitureTable()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var createTableCommand = connection.CreateCommand();
                createTableCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Furniture (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE,
                        Description TEXT NOT NULL,
                        Price REAL NOT NULL,
                        Height INTEGER NOT NULL,
                        Width INTEGER NOT NULL,
                        Length INTEGER NOT NULL   
                    ); 
                ";
                createTableCommand.ExecuteNonQuery();
            }
        }
    }
}
```
