using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static void SeedData(ApplicationDbContext dbContext)
        {
            dbContext.Database.Migrate();

            if (!dbContext.Users.Any()) // Nếu bảng Users rỗng
            {
                var user1 = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "John Doe",
                    Email = "johndoe@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                user1.SetPassword("password123"); // ✅ Thiết lập mật khẩu đúng cách

                var user2 = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Jane Smith",
                    Email = "janesmith@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                user2.SetPassword("password456"); // ✅

                var user3 = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Alice Johnson",
                    Email = "alicejohnson@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                user3.SetPassword("password789"); // ✅

                dbContext.Users.AddRange(user1, user2, user3);
                dbContext.SaveChanges();
            }
        }
    }
}
