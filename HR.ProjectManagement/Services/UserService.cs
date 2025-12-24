using HR.ProjectManagement.Contracts.Identity;
using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Services.Interfaces;
using HR.ProjectManagement.Validations.UserValidations;
using Mapster;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace HR.ProjectManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IValidator<CreateUserRequest> _createUserValidator;
    private readonly IValidator<UpdateUserRequest> _updateUserValidator;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IHttpContextAccessor httpContextAccessor,
        IValidator<CreateUserRequest> createUserValidator,
        IValidator<UpdateUserRequest> updateUserValidator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _httpContextAccessor = httpContextAccessor;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        // Validate input
        var validationResult = await _createUserValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new BadRequestException("Validation failed", validationResult);
        

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Role = request.Role,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _userRepository.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user.Adapt<UserResponse>();
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request)
    {
        var validationResult = await _updateUserValidator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
            throw new BadRequestException("Validation failed", validationResult);
        

        var existingUser = await _userRepository.GetByIdAsync(id);
        
        if (existingUser == null)
            throw new NotFoundException("User", id);
        

        if (await _userRepository.IsAnyUserAvailableByEmailExcept(id, request.Email))
            throw new BadRequestException($"Email '{request.Email}' is already in use by another user");
        

        existingUser.FullName = request.FullName;
        existingUser.Email = request.Email;
        existingUser.Role = request.Role;

        if (!string.IsNullOrWhiteSpace(request.Password))
            existingUser.PasswordHash = _passwordHasher.Hash(request.Password);
        

        await _userRepository.UpdateAsync(existingUser);
        await _unitOfWork.SaveChangesAsync();

        return existingUser.Adapt<UserResponse>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        await _userRepository.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user?.Adapt<UserResponse>();
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAsync();
        return users.Adapt<IReadOnlyList<UserResponse>>();
    }

    public async Task<IReadOnlyList<UserResponse>> GetByRoleAsync(Role role)
    {
        var users = await _userRepository.GetByRoleAsync(role);
        return users.Adapt<IReadOnlyList<UserResponse>>();
    }

    public async Task<UserResponse?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user?.Adapt<UserResponse>();
    }

    
}
