using Microsoft.EntityFrameworkCore;
using TransportTrack.Domain.Core;
using TransportTrack.Infrastructure.Context;

namespace TransportTrack.Infrastructure.Core;

public class BaseRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly TransportTrackContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public BaseRepository(TransportTrackContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await DbSet
            .Where(entity => !entity.IsDeleted)
            .ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await DbSet
            .FirstOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted);
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        entity.IsDeleted = true;
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
    }
}
