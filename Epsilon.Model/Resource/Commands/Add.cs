using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Commands
{
    public class Add : ICommand
    {
        [Required]
        public string ResName { get; set; }
        public string Args { get; set; }
    }
}
