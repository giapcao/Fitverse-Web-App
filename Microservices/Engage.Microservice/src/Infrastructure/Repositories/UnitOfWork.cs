using Infrastructure.Common;
using Infrastructure.Context;
using SharedLibrary.Common;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FitverseEngageDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(FitverseEngageDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        if (_repositories.TryGetValue(typeof(T), out var repository))
        {
            return (IRepository<T>)repository;
        }

        repository = new Repository<T>(_context);
        _repositories[typeof(T)] = repository;
        return (IRepository<T>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

