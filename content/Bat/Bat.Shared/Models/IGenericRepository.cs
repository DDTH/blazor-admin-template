namespace Bat.Shared.Models;

/// <summary>
/// Generic interface for repositories.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IGenericRepository<TEntity, TKey> where TEntity : class, new() where TKey : IEquatable<TKey>
{
	/// <summary>
	/// Creates a new entity.
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	TEntity Create(TEntity t);

	/// <summary>
	/// Creates a new entity asynchronously.
	/// </summary>
	/// <param name="t"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<TEntity> CreateAsync(TEntity t, CancellationToken cancellationToken = default);

	/// <summary>
	/// Fetches an entity by its ID.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	TEntity? GetByID(TKey id);

	/// <summary>
	/// Fetches an entity by its ID asynchronously.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<TEntity?> GetByIDAsync(TKey id, CancellationToken cancellationToken = default);

	/// <summary>
	/// Fetches all entities.
	/// </summary>
	/// <returns></returns>
	IEnumerable<TEntity> GetAll();

	/// <summary>
	/// Fetches all entities asynchronously.
	/// </summary>
	/// <returns></returns>
	IAsyncEnumerable<TEntity> GetAllAsync();

	/// <summary>
	/// Updates an existing entity.
	/// </summary>
	/// <param name="t"></param>
	/// <returns>the entity if the update was successful, null otherwise (e.g. the entity does not exist)</returns>
	TEntity? Update(TEntity t);

	/// <summary>
	/// Updates an existing entity asynchronously.
	/// </summary>
	/// <param name="t"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>the entity if the update was successful, null otherwise (e.g. the entity does not exist)</returns>
	ValueTask<TEntity?> UpdateAsync(TEntity t, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes an existing entity.
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	bool Delete(TEntity t);

	/// <summary>
	/// Deletes an existing entity asynchronously.
	/// </summary>
	/// <param name="t"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	ValueTask<bool> DeleteAsync(TEntity t, CancellationToken cancellationToken = default);
}
