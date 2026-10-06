using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace CulinaryBlog.Infrastructure.Authentication;

public sealed class JwtService : IJwtService
{
	private readonly byte[]? _signingKey;
	private readonly string _issuer;
	private readonly string _audience;
	private readonly int _accessTokenMinutes;

	public JwtService(IConfiguration configuration)
	{
		var signingKey = configuration["Jwt:Secret"];
		_signingKey = !string.IsNullOrWhiteSpace(signingKey)
			&& Encoding.UTF8.GetByteCount(signingKey) >= 32
				? Encoding.UTF8.GetBytes(signingKey)
				: null;
		_issuer = configuration["Jwt:Issuer"] ?? "culinary-blog";
		_audience = configuration["Jwt:Audience"] ?? "culinary-blog-client";
		_accessTokenMinutes = int.TryParse(
			configuration["Jwt:AccessTokenMinutes"], out var tokenMinutes)
			&& tokenMinutes is > 0 and <= 60
				? tokenMinutes
				: 15;
	}

	public IssuedAccessToken CreateAccessToken(ApplicationUser user)
	{
		var signingKey = _signingKey
			?? throw new InvalidOperationException(
				"Jwt:Secret must be configured with at least 32 bytes.");
		var now = DateTime.UtcNow;
		var expiresAt = now.AddMinutes(_accessTokenMinutes);
		var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(
			new { alg = "HS256", typ = "JWT" }));
		var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
		{
			iss = _issuer,
			aud = _audience,
			sub = user.Id.ToString(),
			email = user.Email,
			name = user.DisplayName,
			role = user.Role,
			jti = Guid.NewGuid().ToString("N"),
			iat = new DateTimeOffset(now).ToUnixTimeSeconds(),
			exp = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
		}));
		var unsignedToken = $"{header}.{payload}";
		var signature = HMACSHA256.HashData(
			signingKey,
			Encoding.ASCII.GetBytes(unsignedToken));

		return new IssuedAccessToken(
			$"{unsignedToken}.{Base64UrlEncode(signature)}",
			expiresAt);
	}

	public ClaimsPrincipal? ValidateAccessToken(string token)
	{
		if (_signingKey is null)
		{
			return null;
		}

		var parts = token.Split('.');
		if (parts.Length != 3)
		{
			return null;
		}

		try
		{
			using var header = JsonDocument.Parse(Base64UrlDecode(parts[0]));
			if (header.RootElement.GetProperty("alg").GetString() != "HS256")
			{
				return null;
			}

			var signedData = Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}");
			var providedSignature = Base64UrlDecode(parts[2]);
			var expectedSignature = HMACSHA256.HashData(_signingKey, signedData);
			if (!CryptographicOperations.FixedTimeEquals(
					providedSignature,
					expectedSignature))
			{
				return null;
			}

			using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
			var claims = payload.RootElement;
			if (claims.GetProperty("iss").GetString() != _issuer
				|| claims.GetProperty("aud").GetString() != _audience
				|| claims.GetProperty("exp").GetInt64()
					<= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
			{
				return null;
			}

			var identityClaims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, claims.GetProperty("sub").GetString()!),
				new(ClaimTypes.Email, claims.GetProperty("email").GetString()!),
				new(ClaimTypes.Name, claims.GetProperty("name").GetString()!),
				new(ClaimTypes.Role, claims.GetProperty("role").GetString()!)
			};

			return new ClaimsPrincipal(
				new ClaimsIdentity(identityClaims, "Bearer"));
		}
		catch (Exception exception) when (
			exception is FormatException
				or JsonException
				or KeyNotFoundException
				or InvalidOperationException)
		{
			return null;
		}
	}

	private static string Base64UrlEncode(byte[] value) =>
		Convert.ToBase64String(value)
			.TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');

	private static byte[] Base64UrlDecode(string value)
	{
		var base64 = value.Replace('-', '+').Replace('_', '/');
		base64 += new string('=', (4 - base64.Length % 4) % 4);
		return Convert.FromBase64String(base64);
	}
}
