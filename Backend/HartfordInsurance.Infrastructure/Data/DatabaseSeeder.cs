using System;
using System.Collections.Generic;
using System.Linq;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HartfordInsurance.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedData(ApplicationDbContext db)
        {
            if (!db.Users.Any())
            {
                using var transaction = db.Database.BeginTransaction();
                try
                {
                    var users = new List<User>
                    {
                        new User { Id = 1, FullName = "RadhaKrishna", Email = "RadhaKrishna@hartford.com", PasswordHash = "RadhaKrishna@12", Role = Role.Admin },
                        new User { Id = 2, FullName = "Poojitha Schrute", Email = "poojitha@hartford.com", PasswordHash = "Pooji@12", Role = Role.Agent, PhoneNumber = "9347758510" },
                        new User { Id = 3, FullName = "Jim Halpert", Email = "jim@hartford.com", PasswordHash = "Password123!", Role = Role.Agent, PhoneNumber = "7284672682" },
                        new User { Id = 4, FullName = "Angel Martin", Email = "angel@hartford.com", PasswordHash = "angel@12", Role = Role.ClaimsOfficer, PhoneNumber = "9813791191" },
                        new User { Id = 5, FullName = "Creed Bratton", Email = "creed@hartford.com", PasswordHash = "Password123!", Role = Role.Customer, PhoneNumber = "555-0104" },
                        new User { Id = 6, FullName = "Pam Beesly", Email = "pam@hartford.com", PasswordHash = "Password123!", Role = Role.Customer, PhoneNumber = "8329287429" },
                        new User { Id = 7, FullName = "Sweety", Email = "sweety@hartford.com", PasswordHash = "sweety@12", Role = Role.ClaimsOfficer, PhoneNumber = "8278681191" }
                    };

                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [Users] ON");
                    db.Users.AddRange(users);
                    db.SaveChanges();
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [Users] OFF");

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
