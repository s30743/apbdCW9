using System.Data;
using APBDcw9.Exceptions;
using APBDcw9.Modeks;
using Microsoft.Data.SqlClient;

namespace APBDcw9.Services;

public class wareHouseService : IwareHouseService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";
    
    
    public async Task<int> add(WarehouseRequest warehouse)
    {
        await using SqlConnection con = new SqlConnection(_connectionString);
        await using SqlCommand com = new SqlCommand();
        
        com.Connection = con;
        await con.OpenAsync();
        
        string checkProduct = "Select count(*) FROM Product WHERE IdProduct = @IdProduct";
        com.CommandText = checkProduct;
        com.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        int ProductExists = (int)await com.ExecuteScalarAsync();

        if (ProductExists == 0)
        {
            throw new NotFoundEx($"Produkt o podanym Id {warehouse.IdProduct} nie istnieje w bazie");
        }
        //
        string checkMagazyn = "Select count(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        com.CommandText = checkMagazyn;
        com.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
        int WarehouseExists = (int)await com.ExecuteScalarAsync();
        
        if (WarehouseExists == 0)
        {
            throw new NotFoundEx($"Magazyn o podanym Id {warehouse.IdWarehouse} nie istnieje w bazie");
        }

        if (warehouse.Amount <= 0)
        {
            throw new ConflictEx("za malo podanych danych");
        }
        
        //2
        com.Parameters.Clear();
        string checkOrder = @"Select Count(*) FROM [Order] where IdProduct = @IdProduct AND Amount =@Amount";
        com.CommandText = checkOrder;
        com.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        com.Parameters.AddWithValue("@Amount", warehouse.Amount);
        
        var OrderExists = (int)await com.ExecuteScalarAsync();
        
        
        if (OrderExists == 0)
        {
            throw new NotFoundEx("Nie istnieje zamowienie odpowiadajce parametrom ");
        }
        
        com.Parameters.Clear();
        string CheckDate = @"Select Count(*) FROM [Order] where IdProduct =@IdProduct AND Amount =@Amount AND CreatedAt < @CreatedAt";
        com.CommandText = CheckDate;
        com.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        com.Parameters.AddWithValue("@Amount", warehouse.Amount);
        com.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);
        
        int OrderDateExists = (int)await com.ExecuteScalarAsync();
        if (OrderDateExists == 0)
        {
            throw new  ConflictEx("Podana data nie moze byc wczesniejsza niz data wczesniej");
        }
        
        
        //3
        
        com.Parameters.Clear();
        string CheckOrderRealized = @"SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = (SELECT IdOrder FROM [Order] where [Order].IdProduct = @IdProduct AND [Order].Amount = @Amount)";
        com.CommandText = CheckOrderRealized;
        com.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        com.Parameters.AddWithValue("@Amount", warehouse.Amount);
        int OrderDateRealized = (int)await com.ExecuteScalarAsync();
        

        if (OrderDateRealized == 1)
        {
            throw new ConflictEx("zamowienie o takim produkcie do takeigo magazyunu zsotalo juz zrealizowane");
        }
        //4
        com.Parameters.Clear();
        
        string UpdateFulfilled = @"UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = (SELECT IdOrder FROM [Order] where [Order].IdProduct = @IdProduct)";
        com.CommandText = UpdateFulfilled;
        com.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        int rowsAffected = await com.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            throw new ConflictEx("NIe zaktaulizowano");
        }
        
        //5
        
        com.Parameters.Clear();
        string insertQuery = @"
        INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
        VALUES 
            (@IdWarehouse, @IdProduct, (SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount),
            @Amount, (SELECT Price FROM Product WHERE IdProduct = @IdProduct) * @Amount, @CreatedAt);
        
        SELECT SCOPE_IDENTITY();";
        
        com.CommandText = insertQuery;
        com.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
        com.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        com.Parameters.AddWithValue("@Amount", warehouse.Amount);
        com.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);
        
        var result = (int)await com.ExecuteNonQueryAsync();
        
        if (result == 0)
        {
            throw new Exception("NEI DZIALA");
        }
        
        return Convert.ToInt32(result);
    }

    public async Task ProcedureAsync(int IdProduct, int IdWarehouse, decimal Amount, DateTime CreatedAt)
    {
        await using SqlConnection con = new SqlConnection(_connectionString);
        await using SqlCommand com = new SqlCommand();
        
        com.Connection = con;
        con.OpenAsync();
        
        com.CommandText = "AddProductToWarehouse";
        com.CommandType = CommandType.StoredProcedure;
        com.Parameters.AddWithValue("@IdProduct", IdProduct);
        com.Parameters.AddWithValue("@Amount", Amount);
        com.Parameters.AddWithValue("@CreatedAt", CreatedAt);
        com.Parameters.AddWithValue("@IdWarehouse", IdWarehouse);
        
        await com.ExecuteNonQueryAsync();
    }
}