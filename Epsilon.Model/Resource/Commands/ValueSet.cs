using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Commands
{
    public class ValueSet : ICommand
    {
        [Required]
        public int ResId { get; set; }
        [Required]
        public int CultureId { get; set; }

        public string ResValue { get; set; }
    }
}
