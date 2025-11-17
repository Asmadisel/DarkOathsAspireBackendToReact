namespace DarkOathsAspireBackendToReact.ApiService.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Login {  get; set; }
        public required string PasswordHash { get; set; } // Всегда храните хеши, а не пароли в открытом виде!
    }

}
