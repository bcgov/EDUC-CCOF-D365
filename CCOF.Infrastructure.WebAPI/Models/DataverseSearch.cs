namespace CCOF.Infrastructure.WebAPI.Models
{
    public class DataverseSearch
    {

        public string? search { get; set; }
        public bool usefuzzy { get; } = true;
        public string? filter { get; } = "account:ccof_accounttype eq 100000001"; // Restrict Search to Facility account type and excluding closed status
        public bool returntotalrecordcount { get; } = true;
        public List<string> entities { get; } = new List<string> { "account" }; // Restrict Search to account table only
 
    }
}