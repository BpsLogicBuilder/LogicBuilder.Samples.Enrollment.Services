using AutoMapper;
using Enrollment.Stores;
using LogicBuilder.EntityFrameworkCore.Repositories;

namespace Enrollment.Repositories
{
    public class EnrollmentRepository(IEnrollmentStore store, IMapper mapper) : ContextRepositoryBase(store, mapper), IEnrollmentRepository
    {
    }
}
