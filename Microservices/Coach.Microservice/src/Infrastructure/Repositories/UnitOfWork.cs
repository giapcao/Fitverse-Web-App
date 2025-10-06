using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Common;
using Domain.Persistence;
using SharedLibrary.Common;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FitverseCoachDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(FitverseCoachDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        if (!_repositories.TryGetValue(typeof(T), out var repository))
        {
            repository = new Repository<T>(_context);
            _repositories[typeof(T)] = repository;
        }

        return (IRepository<T>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

