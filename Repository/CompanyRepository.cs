﻿using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
{
  public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
  {
  }

  public async Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges) =>
    await FindAll(trackChanges)
      .OrderBy(c => c.Name)
      .ToListAsync();

  public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges) =>
    await FindByCondition(x => ids.Any(id => id.Equals(x.Id)),trackChanges).ToListAsync();
  

  public async Task<Company?> GetCompanyAsync(Guid companyId, bool trackChanges) =>
    await FindByCondition(x => Equals(x.Id, companyId), trackChanges).SingleOrDefaultAsync();
  

  public void CreateCompany(Company company) => Create(company);
  public void DeleteCompany(Company company) => Delete(company);
}