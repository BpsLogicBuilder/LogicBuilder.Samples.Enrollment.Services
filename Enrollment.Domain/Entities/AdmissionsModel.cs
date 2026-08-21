using LogicBuilder.Attributes;
using LogicBuilder.Domain;


namespace Enrollment.Domain.Entities
{
    public class AdmissionsModel : BaseModel
    {
		[VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Admissions_UserId")]
		public int UserId { get; set; }

        [AlsoKnownAs("Admissions_User")]
		public UserModel? User { get; set; }

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Admissions_EnteringStatus")]
		public string EnteringStatus { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Admissions_EnrollmentTerm")]
		public string EnrollmentTerm { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Admissions_EnrollmentYear")]
		public string EnrollmentYear { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Admissions_ProgramType")]
		public string ProgramType { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Admissions_Program")]
		public string Program { get; set; } = "";
    }
}