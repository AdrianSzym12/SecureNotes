
namespace SecureNotes.Application.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException()
            : base("User with this email already exists.")
        {
        }
    }
}
