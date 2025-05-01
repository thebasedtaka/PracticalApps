using Microsoft.EntityFrameworkCore.ChangeTracking;
using Northwind.EntityModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace Northwind.WebApi.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private NorthwindContext _db;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
        public CustomerRepository(NorthwindContext db, IMemoryCache memoryCache)
        {
            _db = db;
            _memoryCache = memoryCache;
        }

        async Task<Customer?> ICustomerRepository.CreateAsync(Customer c)
        {
            c.CustomerId = c.CustomerId.ToUpper();
            EntityEntry<Customer> added = await _db.Customers.AddAsync(c);
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                _memoryCache.Set(c.CustomerId, c, _cacheEntryOptions);
                return c;
            }
            return null;
        }

        async Task<bool> ICustomerRepository.DeleteAsync(string id)
        {
            id = id.ToUpper();
            
            Customer? c = await _db.Customers.FindAsync(id);
            if (c is null) return false;
            _db.Customers.Remove(c);

            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                _memoryCache.Remove(c.CustomerId);
                return true;
            }
            return false;
        }

        Task<Customer[]> ICustomerRepository.RetrieveAllAsync()
        {
            return _db.Customers.ToArrayAsync();
        }

        Task<Customer?> ICustomerRepository.RetrieveAsync(string id)
        {
            id = id.ToUpper();
            if (_memoryCache.TryGetValue(id, out Customer? fromCache))
                return Task.FromResult(fromCache);

            Customer? fromDb = _db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (fromDb == null) return Task.FromResult(fromDb);
            _memoryCache.Set(fromDb.CustomerId, fromDb, _cacheEntryOptions);
            
            return Task.FromResult(fromDb)!;
        }

        async Task<Customer?> ICustomerRepository.UpdateAsync(Customer c)
        {
            c.CustomerId = c.CustomerId.ToUpper();
            _db.Customers.Update(c);

            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                _memoryCache.Set(c.CustomerId, c, _cacheEntryOptions);
                return c;
            }
            return null;
        }
    }

}
