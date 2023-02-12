﻿using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using Shared.RequestFeatures;

namespace Repository;

public class EmployeeRepository : RepositoryBase<Employee>,IEmployeeRepository
{
  public EmployeeRepository(RepositoryContext repositoryContext) : base(repositoryContext)
  {
  }

  public async Task<PageList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
  {
    // I think paging and filtering and searching should be implementation in service, not in repository
    var employees = await
     FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
       .FilterEmployees(employeeParameters.MinAge,employeeParameters.MaxAge)
       .Search(employeeParameters.SearchTerm)
       .Sort(employeeParameters.OrderBy)
       .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize).Take(employeeParameters.PageSize)
       .ToListAsync();

    var count = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges).CountAsync();

    return new PageList<Employee>(employees, count, employeeParameters.PageNumber, employeeParameters.PageSize);
  }
  public async Task<Employee?> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
    => await FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id), trackChanges).SingleOrDefaultAsync();

  public void CreateEmployeeForCompany(Guid companyId, Employee employee)
  {
    employee.CompanyId = companyId;
    Create(employee);
  }

  public void DeleteEmployee(Employee employee) => Delete(employee);
}