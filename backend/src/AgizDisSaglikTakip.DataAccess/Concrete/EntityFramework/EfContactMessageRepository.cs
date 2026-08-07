using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfContactMessageRepository : EfRepositoryBase<ContactMessage>, IContactMessageRepository
{
    public EfContactMessageRepository(AppDbContext context) : base(context)
    {
    }
}
