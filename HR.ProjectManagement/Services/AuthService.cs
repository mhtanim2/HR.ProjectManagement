using HR.ProjectManagement.Contracts.Identity;
using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Services.Interfaces;
using HR.ProjectManagement.Utils;
using HR.ProjectManagement.Validations.AuthValidations;
using Mapster;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using AppValidationException = HR.ProjectManagement.Exceptions.ValidationException;

namespace HR.ProjectManagement.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<LoginRequest> _loginRequestValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetRepository passwordResetRepository,
        IPasswordHasher passwordHasher,
        IConfiguration config,
        IUnitOfWork unitOfWork,
        IValidator<LoginRequest> loginRequestValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetRepository = passwordResetRepository;
        _passwordHasher = passwordHasher;
        _config = config;
        _unitOfWork = unitOfWork;
        _loginRequestValidator = loginRequestValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var validationResult = await _loginRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new AppValidationException("Login validation failed", validationResult);
        
        var user = await ValidateUserCredentialsAsync(request.Email, request.Password);
        var token = await GenerateTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);
        var userResponse = user.Adapt<UserResponse>();

        return new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            RefreshToken = refreshToken.Token,
            User = userResponse
        };
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        try
        {
            var claims = CreateClaimsForUser(user);
            var securityKey = CreateSecurityKey();
            var signingCredentials = CreateSigningCredentials(securityKey);
            var jwtToken = CreateJwtToken(claims, signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
        catch (Exception ex)
        {
            throw new AuthenticationException(AuthConstants.TokenGenerationFailedMessage, ex);
        }
    }

    public async Task<User> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null)
            throw new AuthenticationException(AuthConstants.InvalidCredentialsMessage);

        bool verifyPassword = _passwordHasher.Verify(password, user.PasswordHash);

        if (!verifyPassword)
            throw new AuthenticationException(AuthConstants.InvalidCredentialsMessage);

        return user;
    }

    
    private List<Claim> CreateClaimsForUser(User user)
    {
        return new List<Claim>
        {
            new Claim(AuthConstants.NameIdentifierClaimType, user.Id.ToString()),
            new Claim(AuthConstants.EmailClaimType, user.Email),
            new Claim(AuthConstants.RoleClaimType, user.Role.ToString())
        };
    }

    private SymmetricSecurityKey CreateSecurityKey()
    {
        var key = GetJwtKey();
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    private SigningCredentials CreateSigningCredentials(SymmetricSecurityKey securityKey)
    {
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials signingCredentials)
    {
        return new JwtSecurityToken(
            issuer: GetJwtIssuer(),
            audience: GetJwtAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            signingCredentials: signingCredentials
        );
    }

    private string GetJwtKey()
    {
        return _config["Jwt:Key"] ?? throw new AuthenticationException("JWT Key not configured");
    }

    private string GetJwtIssuer()
    {
        return _config["Jwt:Issuer"] ?? throw new AuthenticationException("JWT Issuer not configured");
    }

    private string GetJwtAudience()
    {
        return _config["Jwt:Audience"] ?? throw new AuthenticationException("JWT Audience not configured");
    }

    private int GetTokenExpirationMinutes()
    {
        var minutesString = _config["Jwt:AccessTokenMinutes"];
        return int.TryParse(minutesString, out var minutes) ? minutes : 60;
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Validate input using FluentValidation
        var validationResult = await _refreshTokenValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new AppValidationException("Validation failed", validationResult);
        }

        var existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (existingRefreshToken == null || !await _refreshTokenRepository.IsValidTokenAsync(request.RefreshToken))
        {
            throw new TokenExpiredException(AuthConstants.InvalidRefreshTokenMessage);
        }

        if (existingRefreshToken.IsUsed || existingRefreshToken.IsRevoked)
        {
            throw new UnauthorizedException(AuthConstants.RefreshTokenRevokedMessage);
        }

        var user = existingRefreshToken.User;
        if (user == null)
        {
            throw new AuthenticationException(AuthConstants.UserNotFoundMessage);
        }

        // Mark the old refresh token as used
        existingRefreshToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(existingRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        // Generate new tokens
        var newAccessToken = await GenerateTokenAsync(user);
        var newRefreshToken = await GenerateRefreshTokenAsync(user);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            RefreshToken = newRefreshToken.Token
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (existingRefreshToken != null)
        {
            existingRefreshToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(existingRefreshToken);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(User user)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateRandomToken(),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
            IsUsed = false,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();
        return refreshToken;
    }

    private string GenerateRandomToken()
    {
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private int GetRefreshTokenExpirationDays()
    {
        var daysString = _config["Jwt:RefreshTokenDays"];
        return int.TryParse(daysString, out var days) ? days : 7;
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        // Validate input using FluentValidation
        var validationResult = await _forgotPasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new AppValidationException("Validation failed", validationResult);
        }

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return new ForgotPasswordResponse
            {
                Success = true,
                Message = AuthConstants.PasswordResetTokenSent
            };
        }

        // Invalidate any existing password reset tokens for this email
        await _passwordResetRepository.InvalidateAllTokensForEmailAsync(request.Email);

        // Generate new password reset token
        var passwordReset = new PasswordReset
        {
            Token = GenerateRandomToken(),
            Email = request.Email,
            Expires = DateTime.UtcNow.AddHours(GetPasswordResetExpirationHours()),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _passwordResetRepository.CreateAsync(passwordReset);
        await _unitOfWork.SaveChangesAsync();

        // In a real application, you would send an email here
        // For development purposes, we'll return the token in the response
        return new ForgotPasswordResponse
        {
            Success = true,
            Message = AuthConstants.PasswordResetTokenSent,
            ResetToken = passwordReset.Token // Only for development/testing
        };
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Validate input using FluentValidation
        var validationResult = await _resetPasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new AppValidationException("Validation failed", validationResult);
        }

        var passwordReset = await _passwordResetRepository.GetByTokenAsync(request.Token);

        if (passwordReset == null || !await _passwordResetRepository.IsValidTokenAsync(request.Token))
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = AuthConstants.InvalidPasswordResetToken
            };
        }

        if (passwordReset.IsUsed || passwordReset.Expires < DateTime.UtcNow)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = AuthConstants.InvalidPasswordResetToken
            };
        }

        var user = await _userRepository.GetByEmailAsync(passwordReset.Email);
        if (user == null)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = AuthConstants.UserNotFoundMessage
            };
        }

        // Update user password
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        // Mark the password reset token as used
        passwordReset.IsUsed = true;
        await _passwordResetRepository.UpdateAsync(passwordReset);

        // Save all changes
        await _unitOfWork.SaveChangesAsync();

        // Invalidate all refresh tokens for this user for security
        await _refreshTokenRepository.RevokeTokensByUserIdAsync(user.Id);

        return new ResetPasswordResponse
        {
            Success = true,
            Message = AuthConstants.PasswordResetSuccess
        };
    }

    private int GetPasswordResetExpirationHours()
    {
        var hoursString = _config["PasswordReset:TokenExpirationHours"];
        return int.TryParse(hoursString, out var hours) ? hours : 1;
    }
}