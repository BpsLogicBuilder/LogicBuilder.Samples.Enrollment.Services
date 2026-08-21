using LogicBuilder.Attributes;
using LogicBuilder.Domain;

namespace Enrollment.Domain.Entities
{
    public class PersonalModel : BaseModel
    {
		[VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_UserId")]
		public int UserId { get; set; }

		[AlsoKnownAs("Personal_User")]
		public UserModel? User { get; set; }

        public string UserIdString { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_FirstName")]
		public string FirstName { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_MiddleName")]
		public string MiddleName { get; set; } = "";

        public string FullName { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_LastName")]
		public string LastName { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_Suffix")]
		public string Suffix { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_Address1")]
		public string Address1 { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_Address2")]
		public string Address2 { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_City")]
		public string City { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_State")]
		public string State { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_County")]
		public string County { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_ZipCode")]
		public string ZipCode { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_CellPhone")]
		public string CellPhone { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_OtherPhone")]
		public string OtherPhone { get; set; } = "";

        [VariableEditorControl(VariableControlType.SingleLineTextBox)]
		[AlsoKnownAs("Personal_PrimaryEmail")]
		public string PrimaryEmail { get; set; } = "";
    }
}