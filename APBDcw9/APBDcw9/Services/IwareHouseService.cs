using APBDcw9.Modeks;

namespace APBDcw9.Services;

public interface IwareHouseService
{
    Task<int> add(WarehouseRequest warehouse);
    Task ProcedureAsync(int IdProduct, int IdWarehouse,decimal Amount, DateTime CreatedAt);
}