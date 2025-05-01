using Northwind.EntityModels;
namespace Northwind.WebApi.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> CreateAsync(Customer c);
        Task<Customer[]> RetrieveAllAsync();
        Task<Customer?> RetrieveAsync(String id);
        Task<Customer?> UpdateAsync(Customer c);
        Task<bool> DeleteAsync(string c);
    }
}
