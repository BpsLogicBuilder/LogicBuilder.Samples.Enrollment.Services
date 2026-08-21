using LogicBuilder.Data;

namespace Enrollment.Data.Entities
{
    public class StateLivedIn : BaseData
    {
        #region Properties
        public int StateLivedInId { get; set; }

        public string State { get; set; } = "";

        public int UserId { get; set; }

        public virtual Residency? Residency { get; set; }
        #endregion Properties
    }
}
