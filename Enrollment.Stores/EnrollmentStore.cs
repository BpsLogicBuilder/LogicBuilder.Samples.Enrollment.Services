using Enrollment.Contexts;
using LogicBuilder.EntityFrameworkCore.Crud.DataStores;

namespace Enrollment.Stores
{
    public class EnrollmentStore(EnrollmentContext context) : StoreBase(context), IEnrollmentStore
    {
    }
}
