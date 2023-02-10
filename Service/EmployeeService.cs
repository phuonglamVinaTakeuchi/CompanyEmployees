﻿using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace Service
{
  internal sealed class EmployeeService : IEmployeeService
  {
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;

    public EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
    {
      _repository = repository;
      _logger = logger;
      _mapper = mapper;
    }

    private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
    {
      var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
      if (company is null)
        throw new CompanyNotFoundException(companyId); 
    }

    private async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists(Guid companyId, Guid id, bool trackChanges)
    {
      var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);
      if (employeeDb is null)
        throw new EmployeeNotFoundException(id);

      return employeeDb;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployees(Guid companyId, bool trackChanges)
    {
      await CheckIfCompanyExists(companyId,trackChanges);

      var employeesFromDb = _repository.Employee.GetEmployeesAsync(companyId, trackChanges);

      return _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
    }

    public async Task<EmployeeDto> GetEmployee(Guid companyId, Guid id, bool trackChanges)
    {
      await CheckIfCompanyExists(companyId,trackChanges);
      var employeeDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

      var employee = _mapper.Map<EmployeeDto>(employeeDb);
      return employee;
    }

    public async Task<EmployeeDto> CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
    {
      await CheckIfCompanyExists(companyId,trackChanges);

      var employeeEntity = _mapper.Map<Employee>(employeeForCreation);

      _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
      await _repository.SaveAsync();

      var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

      return employeeToReturn;
    }

    public async Task DeleteEmployeeForCompany(Guid companyId, Guid id, bool trackChanges)
    {
      await CheckIfCompanyExists(companyId,trackChanges);

      var employeeForCompany = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

      _repository.Employee.DeleteEmployee(employeeForCompany);
      await _repository.SaveAsync();
    }

    public async Task<EmployeeDto> UpdateEmployeeForCompany(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChanges)
    {
      await CheckIfCompanyExists(companyId, compTrackChanges);

      var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

      _mapper.Map(employeeForUpdate, employeeEntity);
      await _repository.SaveAsync();

      var employee = _mapper.Map<EmployeeDto>(employeeEntity);
      return employee;
    }

    public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatch(Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
    {
      await CheckIfCompanyExists(companyId,compTrackChanges);

      var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

      var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

      return (employeeToPatch, employeeEntity);
    }

    public async Task SaveChangesForPatch(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
    {
      _mapper.Map(employeeToPatch, employeeEntity);
      await _repository.SaveAsync();
    }
  }
}