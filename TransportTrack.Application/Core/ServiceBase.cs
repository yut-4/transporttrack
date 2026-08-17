using Microsoft.EntityFrameworkCore;
using TransportTrack.Domain.Core;
using TransportTrack.Infrastructure.Core;

namespace TransportTrack.Application.Core;

public abstract class ServiceBase<TEntity> where TEntity : BaseEntity
{
    protected readonly BaseRepository<TEntity> Repository;

    protected ServiceBase(BaseRepository<TEntity> repository)
    {
        Repository = repository;
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await Repository.GetAllAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await Repository.GetByIdAsync(id);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        await Repository.AddAsync(entity);
        return entity;
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        await Repository.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null)
            return;
        await Repository.DeleteAsync(entity);
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        await Repository.DeleteAsync(entity);
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await Repository.GetByIdAsync(id) != null;
    }
}
