using Microsoft.EntityFrameworkCore;
using Sigebi.Domain.Entities;
using Sigebi.Domain.Enums;

namespace Sigebi.Infrastructure.Persistence;

public static class SigebiDbSeeder
{
    public static async Task SeedAsync(SigebiDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var student = new User
        {
            FullName = "Ana Estudiante",
            Email = "ana@institucion.edu",
            UserType = UserType.Student,
            IsActive = true
        };
        var teacher = new User
        {
            FullName = "Luis Docente",
            Email = "luis@institucion.edu",
            UserType = UserType.Teacher,
            IsActive = true
        };
        var staff = new User
        {
            FullName = "María Biblioteca",
            Email = "maria@institucion.edu",
            UserType = UserType.Staff,
            IsActive = true
        };

        db.Users.AddRange(student, teacher, staff);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var bookA = new Book
        {
            Title = "Introducción a la arquitectura de software",
            Author = "Carlos Pérez",
            Isbn = "978-0000000001",
            Category = "Informática"
        };
        bookA.Copies.Add(new BookCopy { InventoryCode = "INF-0001-A", Status = CopyStatus.Available });
        bookA.Copies.Add(new BookCopy { InventoryCode = "INF-0001-B", Status = CopyStatus.Available });

        var bookB = new Book
        {
            Title = "Gestión de proyectos educativos",
            Author = "Rosa Gómez",
            Isbn = "978-0000000002",
            Category = "Educación"
        };
        bookB.Copies.Add(new BookCopy { InventoryCode = "EDU-0002-A", Status = CopyStatus.Available });

        db.Books.AddRange(bookA, bookB);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
