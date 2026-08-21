using LogicBuilder.Attributes;
using LogicBuilder.Domain;


namespace Enrollment.Domain.Entities
{
    public class CertificationModel : BaseModel
    {
		[VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Certification_UserId")]
		public int UserId { get; set; }

		[AlsoKnownAs("Certification_User")]
		public UserModel? User { get; set; }

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Certification_CertificateStatementChecked")]
		public bool CertificateStatementChecked { get; set; }

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Certification_DeclarationStatementChecked")]
		public bool DeclarationStatementChecked { get; set; }

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Certification_PolicyStatementsChecked")]
		public bool PolicyStatementsChecked { get; set; }
    }
}