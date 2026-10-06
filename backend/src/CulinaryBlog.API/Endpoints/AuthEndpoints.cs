using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.API.Endpoints;

public static class AuthEndpoints
{
	public static IEndpointRouteBuilder MapAuthEndpoints(
		this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints
			.MapGroup("/api/v1/auth")
			.WithTags("Authentication");

		group.MapPost("/register", RegisterAsync)
			.AllowAnonymous()
			.WithName("Register")
			.Produces<RegisteredUserResponse>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status409Conflict);

		group.MapPost("/login", LoginAsync)
			.AllowAnonymous()
			.RequireRateLimiting("AuthLogin")
			.WithName("Login")
			.Produces<TokenResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status429TooManyRequests);

		group.MapPost("/google", GoogleLoginAsync)
			.AllowAnonymous()
			.RequireRateLimiting("AuthLogin")
			.WithName("GoogleLogin")
			.Produces<TokenResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status429TooManyRequests)
			.ProducesProblem(StatusCodes.Status503ServiceUnavailable);

		group.MapPost("/refresh", RefreshAsync)
			.AllowAnonymous()
			.WithName("RefreshToken")
			.Produces<TokenResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized);

		group.MapPost("/logout", LogoutAsync)
			.RequireAuthorization()
			.WithName("Logout")
			.Produces(StatusCodes.Status204NoContent)
			.ProducesProblem(StatusCodes.Status401Unauthorized);

		group.MapGet("/me", GetProfileAsync)
			.RequireAuthorization()
			.WithName("GetCurrentProfile")
			.Produces<UserProfileResponse>()
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status404NotFound);

		group.MapPatch("/me", UpdateProfileAsync)
			.RequireAuthorization()
			.WithName("UpdateCurrentProfile")
			.Produces<UserProfileResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status404NotFound);

		return endpoints;
	}

	private static async Task<IResult> RegisterAsync(
		RegisterRequest request,
		CulinaryBlogDbContext context,
		IPasswordHasher<ApplicationUser> passwordHasher,
		CancellationToken cancellationToken)
	{
		if (!IsValidEmail(request.Email)
			|| string.IsNullOrWhiteSpace(request.DisplayName)
			|| request.DisplayName.Trim().Length is < 2 or > 100
			|| string.IsNullOrWhiteSpace(request.Password)
			|| request.Password.Length is < 8 or > 128)
		{
			throw new ValidationException(
				"Provide a valid email, displayName (2-100 characters), and password (8-128 characters).");
		}

		var email = request.Email!.Trim().ToLowerInvariant();
		var alreadyExists = await context.ApplicationUsers
			.AnyAsync(user => user.Email.ToLower() == email, cancellationToken);
		if (alreadyExists)
		{
			throw new ConflictException("An account with this email already exists.");
		}

		var user = new ApplicationUser
		{
			Id = Guid.NewGuid(),
			Email = email,
			DisplayName = request.DisplayName.Trim(),
			Role = "Author",
			CreatedAt = DateTime.UtcNow
		};
		user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

		await context.ApplicationUsers.AddAsync(user, cancellationToken);
		await context.SaveChangesAsync(cancellationToken);

		return Results.Created(
			"/api/v1/auth/me",
			new RegisteredUserResponse(user.Id, user.Email, user.DisplayName));
	}

	private static async Task<IResult> LoginAsync(
		LoginRequest request,
		CulinaryBlogDbContext context,
		IPasswordHasher<ApplicationUser> passwordHasher,
		IJwtService jwtService,
		IConfiguration configuration,
		CancellationToken cancellationToken)
	{
		if (!IsValidEmail(request.Email) || string.IsNullOrEmpty(request.Password))
		{
			throw new ValidationException("A valid email and password are required.");
		}

		var email = request.Email!.Trim().ToLowerInvariant();
		var user = await context.ApplicationUsers
			.FirstOrDefaultAsync(
				candidate => candidate.Email.ToLower() == email,
				cancellationToken);

		if (user?.PasswordHash is null
			|| passwordHasher.VerifyHashedPassword(
				user,
				user.PasswordHash,
				request.Password) == PasswordVerificationResult.Failed)
		{
			throw new UnauthorizedException("Invalid email or password.");
		}

		return Results.Ok(await IssueTokensAsync(
			context,
			jwtService,
			user,
			configuration,
			cancellationToken));
	}

	private static async Task<IResult> GoogleLoginAsync(
		GoogleLoginRequest request,
		HttpClient httpClient,
		IConfiguration configuration,
		CulinaryBlogDbContext context,
		IJwtService jwtService,
		CancellationToken cancellationToken)
	{
		var clientId = configuration["Google:ClientId"];
		if (string.IsNullOrWhiteSpace(clientId))
		{
			return Results.Problem(
				statusCode: StatusCodes.Status503ServiceUnavailable,
				title: "Google sign-in is not configured.");
		}

		if (string.IsNullOrWhiteSpace(request.IdToken))
		{
			throw new ValidationException("idToken is required.");
		}

		using var response = await httpClient.GetAsync(
			"https://oauth2.googleapis.com/tokeninfo?id_token="
				+ Uri.EscapeDataString(request.IdToken),
			cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			throw new UnauthorizedException("The Google ID token is invalid.");
		}

		using var tokenInfo = await JsonDocument.ParseAsync(
			await response.Content.ReadAsStreamAsync(cancellationToken),
			cancellationToken: cancellationToken);
		var claims = tokenInfo.RootElement;
		var audience = claims.TryGetProperty("aud", out var aud) ? aud.GetString() : null;
		var subject = claims.TryGetProperty("sub", out var sub) ? sub.GetString() : null;
		var email = claims.TryGetProperty("email", out var mail) ? mail.GetString() : null;
		var verified = claims.TryGetProperty("email_verified", out var emailVerified)
			&& (emailVerified.ValueKind == JsonValueKind.True
				|| emailVerified.ValueKind == JsonValueKind.String
					&& bool.TryParse(emailVerified.GetString(), out var isVerified)
					&& isVerified);

		if (audience != clientId || string.IsNullOrWhiteSpace(subject)
			|| !verified || !IsValidEmail(email))
		{
			throw new UnauthorizedException("The Google ID token is invalid.");
		}

		var normalizedEmail = email!.Trim().ToLowerInvariant();
		var user = await context.ApplicationUsers.FirstOrDefaultAsync(
			candidate => candidate.GoogleSubject == subject
				|| candidate.Email.ToLower() == normalizedEmail,
			cancellationToken);

		if (user is null)
		{
			var displayName = claims.TryGetProperty("name", out var name)
				? name.GetString()
				: null;
			var avatarUrl = claims.TryGetProperty("picture", out var picture)
				? picture.GetString()
				: null;
			user = new ApplicationUser
			{
				Id = Guid.NewGuid(),
				Email = normalizedEmail,
				DisplayName = string.IsNullOrWhiteSpace(displayName)
					? normalizedEmail
					: displayName,
				Role = "Author",
				EmailConfirmed = true,
				GoogleSubject = subject,
				AvatarUrl = avatarUrl,
				CreatedAt = DateTime.UtcNow
			};
			context.ApplicationUsers.Add(user);
		}
		else
		{
			user.GoogleSubject = subject;
			user.EmailConfirmed = true;
			if (string.IsNullOrWhiteSpace(user.AvatarUrl)
				&& claims.TryGetProperty("picture", out var picture))
			{
				user.AvatarUrl = picture.GetString();
			}
		}

		return Results.Ok(await IssueTokensAsync(
			context,
			jwtService,
			user,
			configuration,
			cancellationToken));
	}

	private static async Task<IResult> RefreshAsync(
		RefreshRequest request,
		CulinaryBlogDbContext context,
		IJwtService jwtService,
		IConfiguration configuration,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.RefreshToken))
		{
			throw new ValidationException("refreshToken is required.");
		}

		var tokenHash = HashToken(request.RefreshToken);
		var storedToken = await context.RefreshTokens
			.Include(token => token.User)
			.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
		var now = DateTime.UtcNow;

		if (storedToken is null || storedToken.ExpiresAt <= now)
		{
			throw new UnauthorizedException("The refresh token is invalid or expired.");
		}

		if (storedToken.RevokedAt is not null)
		{
			var activeTokens = await context.RefreshTokens
				.Where(token => token.UserId == storedToken.UserId
					&& token.RevokedAt == null)
				.ToListAsync(cancellationToken);
			foreach (var activeToken in activeTokens)
			{
				activeToken.RevokedAt = now;
			}

			await context.SaveChangesAsync(cancellationToken);
			throw new UnauthorizedException("Refresh token reuse detected.");
		}

		storedToken.RevokedAt = now;
		var rawRefreshToken = CreateRefreshToken();
		context.RefreshTokens.Add(new RefreshToken
		{
			Id = Guid.NewGuid(),
			UserId = storedToken.UserId,
			TokenHash = HashToken(rawRefreshToken),
			ExpiresAt = now.AddDays(GetRefreshTokenDays(configuration)),
			CreatedAt = now
		});
		var accessToken = jwtService.CreateAccessToken(storedToken.User);
		await context.SaveChangesAsync(cancellationToken);

		return Results.Ok(new TokenResponse(
			accessToken.Value,
			rawRefreshToken,
			(int)(accessToken.ExpiresAt - now).TotalSeconds));
	}

	private static async Task<IResult> LogoutAsync(
		RefreshRequest request,
		HttpContext httpContext,
		CulinaryBlogDbContext context,
		CancellationToken cancellationToken)
	{
		var userId = GetUserId(httpContext.User);
		if (!string.IsNullOrWhiteSpace(request.RefreshToken))
		{
			var tokenHash = HashToken(request.RefreshToken);
			var token = await context.RefreshTokens.FirstOrDefaultAsync(
				candidate => candidate.UserId == userId
					&& candidate.TokenHash == tokenHash
					&& candidate.RevokedAt == null,
				cancellationToken);
			if (token is not null)
			{
				token.RevokedAt = DateTime.UtcNow;
				await context.SaveChangesAsync(cancellationToken);
			}
		}

		return Results.NoContent();
	}

	private static async Task<IResult> GetProfileAsync(
		HttpContext httpContext,
		CulinaryBlogDbContext context,
		CancellationToken cancellationToken)
	{
		var userId = GetUserId(httpContext.User);
		var user = await context.ApplicationUsers
			.AsNoTracking()
			.FirstOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
			?? throw new NotFoundException("User profile was not found.");

		return Results.Ok(ToProfile(user));
	}

	private static async Task<IResult> UpdateProfileAsync(
		UpdateProfileRequest request,
		HttpContext httpContext,
		CulinaryBlogDbContext context,
		CancellationToken cancellationToken)
	{
		var userId = GetUserId(httpContext.User);
		var user = await context.ApplicationUsers
			.FirstOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
			?? throw new NotFoundException("User profile was not found.");

		if (request.DisplayName is not null)
		{
			var displayName = request.DisplayName.Trim();
			if (displayName.Length is < 2 or > 100)
			{
				throw new ValidationException("displayName must be 2-100 characters.");
			}

			user.DisplayName = displayName;
		}

		if (request.AvatarUrl is not null)
		{
			if (!string.IsNullOrWhiteSpace(request.AvatarUrl)
				&& (!Uri.TryCreate(request.AvatarUrl, UriKind.Absolute, out var avatarUri)
					|| avatarUri.Scheme is not ("http" or "https")))
			{
				throw new ValidationException("avatarUrl must be a valid HTTP or HTTPS URL.");
			}

			user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
				? null
				: request.AvatarUrl.Trim();
		}

		if (request.Bio is not null)
		{
			if (request.Bio.Length > 1000)
			{
				throw new ValidationException("bio cannot exceed 1000 characters.");
			}

			user.Bio = request.Bio.Trim();
		}

		if (request.DisplayName is null
			&& request.AvatarUrl is null
			&& request.Bio is null)
		{
			throw new ValidationException("At least one profile field must be provided.");
		}

		user.UpdatedAt = DateTime.UtcNow;
		await context.SaveChangesAsync(cancellationToken);
		return Results.Ok(ToProfile(user));
	}

	private static async Task<TokenResponse> IssueTokensAsync(
		CulinaryBlogDbContext context,
		IJwtService jwtService,
		ApplicationUser user,
		IConfiguration configuration,
		CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;
		var accessToken = jwtService.CreateAccessToken(user);
		var refreshToken = CreateRefreshToken();
		context.RefreshTokens.Add(new RefreshToken
		{
			Id = Guid.NewGuid(),
			UserId = user.Id,
			TokenHash = HashToken(refreshToken),
			ExpiresAt = now.AddDays(GetRefreshTokenDays(configuration)),
			CreatedAt = now
		});
		await context.SaveChangesAsync(cancellationToken);

		return new TokenResponse(
			accessToken.Value,
			refreshToken,
			(int)(accessToken.ExpiresAt - now).TotalSeconds);
	}

	private static UserProfileResponse ToProfile(ApplicationUser user) =>
		new(user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.Bio,
			[user.Role], user.EmailConfirmed);

	private static Guid GetUserId(ClaimsPrincipal principal)
	{
		var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
		return Guid.TryParse(value, out var userId)
			? userId
			: throw new UnauthorizedException("The access token has no valid user identifier.");
	}

	private static bool IsValidEmail(string? email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			return false;
		}

		try
		{
			return new MailAddress(email).Address == email;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	private static string CreateRefreshToken() =>
		Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
			.TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');

	private static string HashToken(string token) =>
		Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

	private static int GetRefreshTokenDays(IConfiguration configuration) =>
		int.TryParse(configuration["Jwt:RefreshTokenDays"], out var days)
			&& days is > 0 and <= 90
				? days
				: 7;

	public sealed record RegisterRequest(string? Email, string? DisplayName, string? Password);

	public sealed record LoginRequest(string? Email, string? Password);

	public sealed record GoogleLoginRequest(string? IdToken);

	public sealed record RefreshRequest(string? RefreshToken);

	public sealed record UpdateProfileRequest(
		string? DisplayName,
		string? AvatarUrl,
		string? Bio);

	public sealed record RegisteredUserResponse(
		Guid UserId,
		string Email,
		string DisplayName);

	public sealed record TokenResponse(
		string AccessToken,
		string RefreshToken,
		int ExpiresIn);

	public sealed record UserProfileResponse(
		Guid Id,
		string Email,
		string DisplayName,
		string? AvatarUrl,
		string? Bio,
		IReadOnlyList<string> Roles,
		bool EmailConfirmed);
}
