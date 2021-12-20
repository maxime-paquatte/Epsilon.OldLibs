using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Commands
{
    public class Delete : ICommand
    {
        [Required]
        public int ResId { get; set; }
    }
}
