using System.IO.Compression;

namespace Bat.Shared.Cache;

internal interface ICompressor
{
	Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default);
	Task<byte[]> DecompressAsync(byte[] data, CancellationToken cancellationToken = default);
}

internal sealed class NoCompressionCompressor : ICompressor
{
	public Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default) => Task.FromResult(data);
	public Task<byte[]> DecompressAsync(byte[] data, CancellationToken cancellationToken = default) => Task.FromResult(data);
}

internal sealed class BrotliCompressor(CompressionLevel compressionLevel) : ICompressor
{
	public async Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default)
	{
		using var stream = new MemoryStream();
		using var compressor = new BrotliStream(stream, compressionLevel);
		await compressor.WriteAsync(data, cancellationToken);
		compressor.Close();
		return stream.ToArray();
	}

	public async Task<byte[]> DecompressAsync(byte[] data, CancellationToken cancellationToken = default)
	{
		using var stream = new MemoryStream();
		using var decompressor = new BrotliStream(new MemoryStream(data), CompressionMode.Decompress);
		await decompressor.CopyToAsync(stream, cancellationToken);
		return stream.ToArray();
	}
}

internal sealed class DeflateCompressor(CompressionLevel compressionLevel) : ICompressor
{
	public async Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default)
	{
		using var stream = new MemoryStream();
		using var compressor = new DeflateStream(stream, compressionLevel);
		await compressor.WriteAsync(data, cancellationToken);
		compressor.Close();
		return stream.ToArray();
	}

	public async Task<byte[]> DecompressAsync(byte[] data, CancellationToken cancellationToken = default)
	{
		using var stream = new MemoryStream();
		using var decompressor = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress);
		await decompressor.CopyToAsync(stream, cancellationToken);
		return stream.ToArray();
	}
}
