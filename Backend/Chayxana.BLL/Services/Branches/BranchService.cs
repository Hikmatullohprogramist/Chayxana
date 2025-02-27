﻿using AutoMapper;
using Chayxana.BLL.Commons;
using Chayxana.BLL.DTOs.BranchDTOs;
using Chayxana.BLL.DTOs.UserDTOs;
using Chayxana.BLL.Interfaces.Branches;
using Chayxana.Domain.Entities.Branches;
using Chayxana.Domain.Enums;
using Chayxana.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chayxana.BLL.Services.Branches;

public class BranchService(
    IRepository<Branch> repository,
    IMapper mapper) : IBranchService
{
    public async Task<BranchDTO> AddBranchAsync(AddBranchDTO newBranch, CancellationToken cancellationToken = default)
    {
        try
        {
            var mapped = mapper.Map<Branch>(newBranch);
            await repository.AddAsync(mapped, cancellationToken);
            await repository.SaveAsync(cancellationToken);
            mapped.CreatedAt = DateTime.UtcNow;

            return mapper.Map<BranchDTO>(mapped);
        }
        catch (Exception ex)
        {
            throw new CustomException(404, "Branch not found.");
        }
    }

    public async Task<BranchDTO> ModifyBranchAsync(long id, ModifyBranchDTO branch, CancellationToken cancellationToken = default)
    {
        var existsBranch = await repository.ExistsAsync(id, cancellationToken);

        if (!existsBranch)
            throw new CustomException(404, "Branch not found.");

        var mapped = mapper.Map<Branch>(branch);
        mapped.Id = id;
        mapped.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(mapped, cancellationToken);
        await repository.SaveAsync(cancellationToken);

        return mapper.Map<BranchDTO>(mapped);
    }

    public async Task<bool> RemoveBranchAsync(long id, CancellationToken cancellationToken = default)
    {
        var branch = await repository.SelectAsync(x => x.Id == id, null, cancellationToken);

        if (branch is null)
            throw new CustomException(404, "Branch not found or null");

        await repository.DeleteAsync(id, cancellationToken);
        await repository.SaveAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<BranchDTO>> RetrieveAllBranchesAsync(CancellationToken cancellationToken = default)
    {
        var branchQuery = await repository.SelectAllAsync(x => x.Status == Status.Active, null, cancellationToken);

        var branches = await branchQuery
            .AsNoTracking()
            .ToListAsync();

        if (branches is null)
            throw new CustomException(404, "Branches not found.");

        return mapper.Map<IEnumerable<BranchDTO>>(branches);
    }

    public async Task<BranchDTO> RetrieveBranchAsync(long id, CancellationToken cancellationToken = default)
    {
        var branch = await repository.SelectAsync(x => x.Id == id, null, cancellationToken);

        if (branch is null)
            throw new CustomException(404, "Branch not found.");

        return mapper.Map<BranchDTO>(branch);
    }

    public async Task<IEnumerable<EmployeeDTO>> RetrieveEmployeesByBranchIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var branchQuery = await repository.SelectAllAsync(x => x.Id == id, null, cancellationToken);

        var employees = await branchQuery
            .Where(x => x.Status == Status.Active)
            .SelectMany(x => x.Employees)
            .ToListAsync(cancellationToken);

        if (employees is null)
            throw new CustomException(404, "Branch employees is null");

        return mapper.Map<IEnumerable<EmployeeDTO>>(employees);
    }
}
