﻿using Dapper;
using DapperDemo.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DapperDemo.Repository
{
    public class BonusRepository : IBonusRepository
    {
        private IDbConnection db;
        public BonusRepository(IConfiguration configuration)
        {
            this.db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        public Company GetCompanyWithAddresses(int id)
        {
            var p = new
            {
                CompanyId = id
            };

            var sql = "SELECT * FROM Companies WHERE CompanyId= @CompanyId;"
                + " SELECT * FROM Employees WHERE CompanyId = @CompanyId;";

            Company company;
            using (var lists= db.QueryMultiple(sql, p))
            {
                company = lists.Read<Company>().ToList().FirstOrDefault();
                company.Employees = lists.Read<Employee>().ToList();
            }

            return company;
        }

        public List<Employee> GetEmployeeWithCompany(int id)
        {
            var sql = "SELECT E.*, C.*\r\nFROM Employees as E\r\nINNER JOIN \r\nCompanies as C\r\nON E.CompanyId = C.CompanyId";

            if(id != 0)
            {
                sql += " WHERE E.CompanyId = @Id";
            }

            var employee = db.Query<Employee, Company, Employee>(sql, (e, c) =>
            {
                e.Company = c;
                return e;
            },new {id}, splitOn:"CompanyId");

            return employee.ToList();
        }
    }
}
