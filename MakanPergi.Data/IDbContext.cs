using Microsoft.EntityFrameworkCore;

namespace MakanPergi.Data
{
    public interface IDbContext
    {
        DbContext Instance { get; }
    }
}
